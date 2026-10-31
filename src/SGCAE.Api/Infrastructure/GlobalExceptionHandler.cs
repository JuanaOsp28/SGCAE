using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace SGCAE.Api.Infrastructure;

/// <summary>
/// Convierte las excepciones no controladas en respuestas ProblemDetails. Una violación
/// de índice único (p. ej. dos registros simultáneos con el mismo código) se informa como 409.
/// </summary>
internal sealed class GlobalExceptionHandler(IProblemDetailsService problemDetails, ILogger<GlobalExceptionHandler> logger)
    : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var (status, title, detail) = exception switch
        {
            DbUpdateException => (StatusCodes.Status409Conflict, "Conflicto de datos",
                "La operación entra en conflicto con datos existentes (registro duplicado o modificado)."),
            BadHttpRequestException bad => (bad.StatusCode, "Solicitud inválida", bad.Message),
            _ => (StatusCodes.Status500InternalServerError, "Error interno", "Ocurrió un error inesperado.")
        };

        if (status >= 500)
            logger.LogError(exception, "Error no controlado");
        else
            logger.LogWarning(exception, "Solicitud rechazada: {Title}", title);

        httpContext.Response.StatusCode = status;
        return await problemDetails.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = exception,
            ProblemDetails = new ProblemDetails { Status = status, Title = title, Detail = detail }
        });
    }
}
