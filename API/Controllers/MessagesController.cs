using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

public class MessagesController(
    IMessageRepository messageRepository,
    IMemberRepository memberRepository) : BaseApiController
{
    [HttpPost()]
    public async Task<ActionResult<MessageDto>> CreateMessage(CreateMessageDto messageDto)
    {
        var sender = await memberRepository.GetMemberByIdAsync(User.GetMemberId());
        if (sender == null) return BadRequest("Sender does not exist");
        
        var recipient = await memberRepository.GetMemberByIdAsync(messageDto.RecipientId);
        if (recipient == null) return BadRequest("Recipient does not exist");

        if (sender.Id == messageDto.RecipientId) return BadRequest("You cannot message your self");
        
        var message = new Message()
        {
            Content = messageDto.Content,
            RecipientId = recipient.Id,
            SenderId = sender.Id
        };

        messageRepository.AddMessage(message);

        if (await messageRepository.SaveAllChanges()) return message.ToDto();

        return BadRequest("Failed to save message");
    }

    [HttpGet]
    public async Task<ActionResult<PaginatedResult<MessageDto>>> GetMessagesByContainer(
        [FromQuery]MessageParams messageParams)
    {
        messageParams.MemberId = User.GetMemberId();

        return await messageRepository.GetMesagesForMember(messageParams);
    }

    [HttpGet("thread/{recipientId}")]
    public async Task<ActionResult<IReadOnlyList<MessageDto>>> GetMessagesThread(string recipientId)
    {
        var memberId = User.GetMemberId();
        
        return Ok(await messageRepository.GetMessageThread(memberId, recipientId));
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteMessage(string id)
    {
        var memberId = User.GetMemberId();
        var message = await messageRepository.GetMessage(id);
        if( message == null) return BadRequest("Cannot delete this message");

        if(message.SenderId != memberId && message.RecipientId != memberId) return BadRequest("Cannot delete this message");
        
        if(message.SenderId != memberId) message.SenderDeleted = true;
        if(message.RecipientId != memberId) message.SenderDeleted = true;
        
        if (message is {SenderDeleted: true, RecipientDeleted: true})
        {
            messageRepository.DeleteMessage(message);
        }

        if(await memberRepository.SaveAllAsync()) return Ok();

        return BadRequest("Cannot delete this message");
    }
}
