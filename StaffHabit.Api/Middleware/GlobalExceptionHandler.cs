using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace StaffHabit.Api.Middleware;

public class GlobalExceptionHandler(IProblemDetailsService problemDetailsService) : IExceptionHandler
{
    public ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        return problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = exception,
            ProblemDetails = new ProblemDetails
            {
                Title = "An unexpected error occurred.",
                Status = StatusCodes.Status500InternalServerError,
                Detail = exception.Message,
                Instance = httpContext.Request.Path,
                Extensions = { ["requestId"] = httpContext.TraceIdentifier }
            }
        });
    }
}
