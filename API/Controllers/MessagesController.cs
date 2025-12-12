using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

public class MessagesController(
    IUnitOfWork uow) : BaseApiController
{
    [HttpPost()]
    public async Task<ActionResult<MessageDto>> CreateMessage(CreateMessageDto messageDto)
    {
        var sender = await uow.MemberRepository.GetMemberByIdAsync(User.GetMemberId());
        if (sender == null) return BadRequest("Sender does not exist");
        
        var recipient = await uow.MemberRepository.GetMemberByIdAsync(messageDto.RecipientId);
        if (recipient == null) return BadRequest("Recipient does not exist");

        if (sender.Id == messageDto.RecipientId) return BadRequest("You cannot message your self");
        
        var message = new Message()
        {
            Content = messageDto.Content,
            RecipientId = recipient.Id,
            SenderId = sender.Id
        };

        uow.MessageRepository.AddMessage(message);

        if (await uow.Complete()) return message.ToDto();

        return BadRequest("Failed to save message");
    }

    [HttpGet]
    public async Task<ActionResult<PaginatedResult<MessageDto>>> GetMessagesByContainer(
        [FromQuery]MessageParams messageParams)
    {
        messageParams.MemberId = User.GetMemberId();

        return await uow.MessageRepository.GetMesagesForMember(messageParams);
    }

    [HttpGet("thread/{recipientId}")]
    public async Task<ActionResult<IReadOnlyList<MessageDto>>> GetMessagesThread(string recipientId)
    {
        var memberId = User.GetMemberId();
        
        return Ok(await uow.MessageRepository.GetMessageThread(memberId, recipientId));
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteMessage(string id)
    {
        var memberId = User.GetMemberId();
        var message = await uow.MessageRepository.GetMessage(id);
        if( message == null) return BadRequest("Cannot delete this message");

        if(message.SenderId != memberId && message.RecipientId != memberId) return BadRequest("Cannot delete this message");
        
        if(message.SenderId != memberId) message.SenderDeleted = true;
        if(message.RecipientId != memberId) message.SenderDeleted = true;
        
        if (message is {SenderDeleted: true, RecipientDeleted: true})
        {
            uow.MessageRepository.DeleteMessage(message);
        }

        if(await uow.Complete()) return Ok();

        return BadRequest("Cannot delete this message");
    }
}
