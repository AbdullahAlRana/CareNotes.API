using CareNotes.Application.Common;
using CareNotes.Application.Dtos;
using CareNotes.Domain.Entities;

namespace CareNotes.Application.Abstractions;

public interface IPostRepository
{
    Task<Post?> GetByIdAsync(string id, CancellationToken ct = default);
    Task CreateAsync(Post post, CancellationToken ct = default);
    Task<bool> DeleteAsync(string id, CancellationToken ct = default);
    Task DeleteByAuthorAsync(string authorId, CancellationToken ct = default);
    Task<PagedResult<PostDto>> FeedAsync(PageQuery page, CancellationToken ct = default);
    Task<UserPostsDto?> GetUserPostsAsync(string userId, PageQuery page, CancellationToken ct = default);
}
