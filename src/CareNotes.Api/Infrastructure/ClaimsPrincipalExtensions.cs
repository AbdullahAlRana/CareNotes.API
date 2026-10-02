using System.Security.Claims;
using CareNotes.Application.Common;

namespace CareNotes.Api.Infrastructure;

public static class ClaimsPrincipalExtensions
{
    public static CurrentUser ToCurrentUser(this ClaimsPrincipal principal) =>
        new(principal.FindFirstValue("sub")!, principal.FindFirstValue("role")!);
}
