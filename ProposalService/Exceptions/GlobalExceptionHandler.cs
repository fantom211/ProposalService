using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using static WorkService.Exceptions.MyCustomExceptions;

namespace WorkService.Exceptions
{
    public class GlobalExceptionHandler : IExceptionHandler
    {
        public async ValueTask<bool> TryHandleAsync(
        HttpContext context,
        Exception exception,
        CancellationToken cancellationToken)
        {
            var statusCode = exception switch
            {
                NotFoundException => StatusCodes.Status404NotFound,
                ForbiddenException => StatusCodes.Status403Forbidden,
                BadRequestException => StatusCodes.Status400BadRequest,
                ConflictException => StatusCodes.Status409Conflict,
                _ => StatusCodes.Status500InternalServerError
            };

            var problem = new ProblemDetails
            {
                Title = exception.Message,
                Status = statusCode,
                Type = $"https://httpstatuses.com/{statusCode}",
                Instance = context.Request.Path
            };

            context.Response.StatusCode = statusCode;

            await context.Response.WriteAsJsonAsync(problem, cancellationToken);
            return true;
        }
    }
}
