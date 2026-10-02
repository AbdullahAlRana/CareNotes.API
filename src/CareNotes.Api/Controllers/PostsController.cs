using CareNotes.Api.Infrastructure;
using CareNotes.Application.Common;
using CareNotes.Application.Dtos;
using CareNotes.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CareNotes.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/posts")]
public class PostsController(PostService posts) : ControllerBase
{
    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<PagedResult<PostDto>>> Feed([FromQuery] PageQuery paging, CancellationToken ct) =>
        await posts.FeedAsync(paging, ct);

    [HttpGet("~/api/users/{userId}/posts")]
    [AllowAnonymous]
    public async Task<ActionResult<UserPostsDto>> ByUser(string userId, [FromQuery] PageQuery paging, CancellationToken ct) =>
        await posts.GetUserPostsAsync(userId, paging, ct);

    [HttpPost]
    public async Task<ActionResult<PostDto>> Create(CreatePostRequest request, CancellationToken ct) =>
        await posts.CreateAsync(User.ToCurrentUser(), request, ct);

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id, CancellationToken ct)
    {
        await posts.DeleteAsync(User.ToCurrentUser(), id, ct);
        return NoContent();
    }
}
