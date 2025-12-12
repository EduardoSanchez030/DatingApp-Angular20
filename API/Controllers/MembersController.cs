using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

public class MembersController(
    IUnitOfWork uow,
    IPhotoService photoService) : BaseApiController
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<Member>>> GetMembers([FromQuery]MemberParams memberParams)
    {
        var memberId = User.GetMemberId();
        if (memberId == null)
        {
            return BadRequest("No Id found in token");
        }

        memberParams.CurrentMemberid = memberId;

        return Ok(await uow.MemberRepository.GetMembersAync(memberParams));
    }

    [Authorize]
    [HttpGet("{id}")]
    public async Task<ActionResult<Member>> GetMember(string id)
    {
        var member = await uow.MemberRepository.GetMemberByIdAsync(id);

        if (member == null)
        {
            return NotFound();
        }

        return member;
    }

    [Authorize]
    [HttpGet("{id}/Photos")]
    public async Task<ActionResult<IReadOnlyList<Photo>>> GetMemberPhotos(string id)
    {
        var isCurrentUser = User.GetMemberId() == id;
        return Ok(await uow.MemberRepository.GetPhotosForMemberAsync(id, isCurrentUser));
    }

    [Authorize]
    [HttpPut]
    public async Task<ActionResult> UpdateMember(MemberUpdateDto memberUpdateDto)
    {
        var memberId = User.GetMemberId();
        if (memberId == null)
        {
            return BadRequest("No Id found in token");
        }

        var member = await uow.MemberRepository.GetMemberForUpdateAsync(memberId);

        if (member == null)
        {
            return BadRequest("Could not get member");
        }
        
        member.DisplayName = memberUpdateDto.DisplayName ?? member.DisplayName;
        member.Description = memberUpdateDto.Description ??  member.Description;
        member.City = memberUpdateDto.City ?? member.City;
        member.Country = memberUpdateDto.Country ?? member.Country;

        member.User.DisplayName =  memberUpdateDto.DisplayName ?? member.User.DisplayName;

        //memberRepository.Update(member);
        if(await uow.Complete()) return NoContent();

        return BadRequest("Failed to update member");
    }

    [Authorize]
    [HttpPost("add-photo")]
    public async Task<ActionResult<Photo>> AddPhoto(IFormFile file)
    {
        var memberId = User.GetMemberId();
        if (memberId == null)
        {
            return BadRequest("No Id found in token");
        }

        var member = await uow.MemberRepository.GetMemberForUpdateAsync(memberId);

        if (member == null)
        {
            return BadRequest("Could not update member");
        }

        var result = await photoService.UploadPhotoAsync(file);

        if (result.Error != null) return BadRequest(result.Error.Message);
        
        var photo = new Photo()
        {
            PublicId = result.PublicId,
            Url = result.SecureUrl.AbsoluteUri,
            MemberId = memberId
        };
       
        member.Photos.Add(photo);

        //memberRepository.Update(member);
        if(await uow.Complete()) return photo;

        return BadRequest("Problem adding photo");
    }

    [Authorize]
    [HttpPut("set-main-photo/{photoId}")]
    public async Task<ActionResult> SetMainPhoto(int photoId)
    {
        var memberId = User.GetMemberId();
        if (memberId == null)
        {
            return BadRequest("No Id found in token");
        }

        var member = await uow.MemberRepository.GetMemberForUpdateAsync(memberId);

        if (member == null)
        {
            return BadRequest("Cannot get member from token");
        }

        var photo = member.Photos.SingleOrDefault(x => x.Id == photoId);

        if (member.ImageUrl == photo?.Url || photo == null)
        {
             return BadRequest("Cannot set as main image");
        }
       
        member.ImageUrl = photo.Url;
        member.User.ImageUrl = photo.Url;

        if(await uow.Complete()) return NoContent();

        return BadRequest("Problem setting main photo");
    }

    [Authorize]
    [HttpDelete("delete-photo/{photoId}")]
    public async Task<ActionResult<Photo>> DeletePhoto(int photoId)
    {
        var memberId = User.GetMemberId();
        if (memberId == null)
        {
            return BadRequest("No Id found in token");
        }

        var member = await uow.MemberRepository.GetMemberForUpdateAsync(memberId);

        if (member == null)
        {
            return BadRequest("Could not update member");
        }

        var photo = member.Photos.SingleOrDefault(x => x.Id == photoId);

        if (member.ImageUrl == photo?.Url || photo == null)
        {
             return BadRequest("Cannot delete main photo");
        }
        
        if (photo.PublicId != null)
        {
            var result = await photoService.DeletePhotoAsync(photo.PublicId);
            if (result.Error != null)
            {
                return BadRequest(result.Error.Message);
            }

            member.Photos.Remove(photo);        
        }

        if (await uow.Complete()) return Ok();

        return BadRequest("Problem removing photo");
    }
}