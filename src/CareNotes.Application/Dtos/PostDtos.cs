using CareNotes.Application.Common;

using System.ComponentModel.DataAnnotations;

namespace CareNotes.Application.Dtos;

public record CreatePostRequest([Required, StringLength(200)] string Title, [Required, StringLength(20000)] string Content);

public record PostDto(string Id, string AuthorId, string? AuthorName, string Title, string Content, DateTime CreatedAt);

public record UserPostsDto(string UserId, string Name, PagedResult<PostDto> Posts);
