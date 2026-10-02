using CareNotes.Application.Abstractions;
using CareNotes.Application.Common;
using CareNotes.Application.Dtos;
using CareNotes.Domain.Entities;

namespace CareNotes.Application.Services;

public class PostService(IPostRepository posts)
{
    /// Lists all posts, newest first.
    public Task<PagedResult<PostDto>> FeedAsync(PageQuery page, CancellationToken ct = default) =>
        posts.FeedAsync(page, ct);

    /// Returns a user together with a page of their posts.
    public async Task<UserPostsDto> GetUserPostsAsync(string userId, PageQuery page, CancellationToken ct = default) =>
        await posts.GetUserPostsAsync(userId, page, ct) ?? throw new NotFoundException("User not found.");

    /// Publishes a post authored by the caller.
    public async Task<PostDto> CreateAsync(CurrentUser user, CreatePostRequest request, CancellationToken ct = default)
    {
        var post = new Post { AuthorId = user.Id, Title = request.Title.Trim(), Content = request.Content };
        await posts.CreateAsync(post, ct);
        return new PostDto(post.Id, post.AuthorId, null, post.Title, post.Content, post.CreatedAt);
    }

    /// Deletes a post; allowed for its author or an admin.
    public async Task DeleteAsync(CurrentUser user, string id, CancellationToken ct = default)
    {
        var post = await posts.GetByIdAsync(id, ct) ?? throw new NotFoundException("Post not found.");
        if (post.AuthorId != user.Id && !user.IsAdmin)
            throw new ForbiddenException();
        await posts.DeleteAsync(id, ct);
    }
}
