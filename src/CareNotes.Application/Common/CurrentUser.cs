using CareNotes.Domain.Entities;

namespace CareNotes.Application.Common;

public record CurrentUser(string Id, string Role)
{
    public bool IsAdmin => Role == Roles.Admin;
}
