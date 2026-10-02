namespace CareNotes.Application.Common;

public abstract class AppException(string message, int statusCode) : Exception(message)
{
    public int StatusCode { get; } = statusCode;
}

public class NotFoundException(string message) : AppException(message, 404);

public class ForbiddenException(string message = "You do not have access to this resource.") : AppException(message, 403);

public class ConflictException(string message) : AppException(message, 409);

public class UnauthorizedException(string message = "Invalid email or password.") : AppException(message, 401);
