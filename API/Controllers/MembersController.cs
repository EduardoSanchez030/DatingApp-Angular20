using API.Entities;
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
}