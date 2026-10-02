namespace CareNotes.Domain.Entities;

public class User
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Role { get; set; } = Roles.User;
    public List<string> Interests { get; set; } = [];
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
