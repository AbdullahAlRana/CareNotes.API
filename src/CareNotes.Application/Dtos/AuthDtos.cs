using System.ComponentModel.DataAnnotations;

namespace CareNotes.Application.Dtos;

public record RegisterRequest(
    [Required, StringLength(100)] string Name,
    [Required, EmailAddress] string Email,
    [Required, MinLength(8)] string Password,
    List<string>? Interests);

public record LoginRequest([Required, EmailAddress] string Email, [Required] string Password);

public record AuthResponse(string Token, DateTime ExpiresAt, UserDto User);
