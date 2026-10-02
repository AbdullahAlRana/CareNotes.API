using CareNotes.Application.Common;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace CareNotes.Api.Infrastructure;

public class AppExceptionHandler(IProblemDetailsService problemDetails) : IExceptionHandler
{
    /// Maps application exceptions to ProblemDetails responses with their status code.
    public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception, CancellationToken ct)
    {
        if (exception is not AppException app)
            return false;

        context.Response.StatusCode = app.StatusCode;
        return await problemDetails.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = context,
            Exception = exception,
            ProblemDetails = new ProblemDetails { Status = app.StatusCode, Title = app.Message }
        });
    }
}
