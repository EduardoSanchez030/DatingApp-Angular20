using System.Security.Claims;
using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

public class MembersController(IMemberRepository memberRepository) : BaseApiController
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<Member>>> GetMembers()
    {
        return Ok(await memberRepository.GetMembersAync());
    }

    [Authorize]
    [HttpGet("{id}")]
    public async Task<ActionResult<Member>> GetMember(string id)
    {
        var member = await memberRepository.GetMemberByIdAsync(id);

        if (member == null)
        {
            return NotFound();
        }

        return member;
    }

    [Authorize]
    [HttpGet("{memberId}/Photos")]
    public async Task<ActionResult<IReadOnlyList<Photo>>> GetMemberPhotos(string memberId)
    {
        return Ok(await memberRepository.GetPhotosForMemberAsync(memberId));
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

        var member = await memberRepository.GetMemberForUpdateAsync(memberId);

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
        if(await memberRepository.SaveAllAsync()) return NoContent();

        return BadRequest("Failed to update member");
    }
}