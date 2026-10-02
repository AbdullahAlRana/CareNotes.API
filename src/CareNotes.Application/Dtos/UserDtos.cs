using System.ComponentModel.DataAnnotations;
using CareNotes.Domain.Entities;

namespace CareNotes.Application.Dtos;

public record UserDto(string Id, string Name, string Email, string Role, List<string> Interests, DateTime CreatedAt)
{
    public static UserDto From(User u) => new(u.Id, u.Name, u.Email, u.Role, u.Interests, u.CreatedAt);
}

public record CreateUserRequest(
    [Required, StringLength(100)] string Name,
    [Required, EmailAddress] string Email,
    [Required, MinLength(8)] string Password,
    [Required, RegularExpression("^(User|Admin)$")] string Role,
    List<string>? Interests);

public record UpdateUserRequest(
    [Required, StringLength(100)] string Name,
    [Required, EmailAddress] string Email,
    [Required, RegularExpression("^(User|Admin)$")] string Role,
    List<string>? Interests,
    [MinLength(8)] string? Password);

public record UpdateProfileRequest([Required, StringLength(100)] string Name, List<string>? Interests);

public record InterestMemberDto(string Id, string Name, string Email);

public record InterestGroupDto(string Interest, int Count, List<InterestMemberDto> Users);
