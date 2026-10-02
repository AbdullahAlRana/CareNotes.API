using CareNotes.Api.Infrastructure;
using CareNotes.Application.Common;
using CareNotes.Application.Dtos;
using CareNotes.Application.Services;
using CareNotes.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CareNotes.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/users")]
public class UsersController(UserService users) : ControllerBase
{
    [HttpGet("me")]
    public async Task<ActionResult<UserDto>> Me(CancellationToken ct) =>
        await users.GetAsync(User.ToCurrentUser().Id, ct);

    [HttpPut("me")]
    public async Task<ActionResult<UserDto>> UpdateMe(UpdateProfileRequest request, CancellationToken ct) =>
        await users.UpdateProfileAsync(User.ToCurrentUser().Id, request, ct);

    [HttpGet]
    [Authorize(Policy = Roles.Admin)]
    public async Task<ActionResult<PagedResult<UserDto>>> List([FromQuery] PageQuery paging, CancellationToken ct) =>
        await users.ListAsync(paging, ct);

    [HttpGet("by-interest")]
    [Authorize(Policy = Roles.Admin)]
    public async Task<ActionResult<PagedResult<InterestGroupDto>>> ByInterest(
        [FromQuery] string[]? interests, [FromQuery] PageQuery paging, CancellationToken ct) =>
        await users.GroupByInterestsAsync(interests, paging, ct);

    [HttpGet("{id}")]
    [Authorize(Policy = Roles.Admin)]
    public async Task<ActionResult<UserDto>> Get(string id, CancellationToken ct) =>
        await users.GetAsync(id, ct);

    [HttpPost]
    [Authorize(Policy = Roles.Admin)]
    public async Task<ActionResult<UserDto>> Create(CreateUserRequest request, CancellationToken ct)
    {
        var user = await users.CreateAsync(request, ct);
        return CreatedAtAction(nameof(Get), new { id = user.Id }, user);
    }

    [HttpPut("{id}")]
    [Authorize(Policy = Roles.Admin)]
    public async Task<ActionResult<UserDto>> Update(string id, UpdateUserRequest request, CancellationToken ct) =>
        await users.UpdateAsync(id, request, ct);

    [HttpDelete("{id}")]
    [Authorize(Policy = Roles.Admin)]
    public async Task<IActionResult> Delete(string id, CancellationToken ct)
    {
        await users.DeleteAsync(id, User.ToCurrentUser().Id, ct);
        return NoContent();
    }
}
