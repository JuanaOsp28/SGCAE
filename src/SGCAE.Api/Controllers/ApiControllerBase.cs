using Microsoft.AspNetCore.Mvc;
using SGCAE.Application.Abstractions;
using SGCAE.Application.Common;

namespace SGCAE.Api.Controllers;

[ApiController]
[Produces("application/json")]
public abstract class ApiControllerBase(IDispatcher dispatcher) : ControllerBase
{
    protected IDispatcher Dispatcher { get; } = dispatcher;

    protected async Task<IActionResult> Execute<T>(IQuery<T> query, CancellationToken cancellationToken)
    {
        var result = await Dispatcher.Query(query, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : Problem(result.Error!);
    }

    protected async Task<IActionResult> Execute<T>(ICommand<T> command, CancellationToken cancellationToken)
    {
        var result = await Dispatcher.Send(command, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : Problem(result.Error!);
    }

    protected async Task<IActionResult> ExecuteCreated<T>(
        ICommand<T> command, string getActionName, Func<T, object> routeValues, CancellationToken cancellationToken)
    {
        var result = await Dispatcher.Send(command, cancellationToken);
        return result.IsSuccess
            ? CreatedAtAction(getActionName, routeValues(result.Value), result.Value)
            : Problem(result.Error!);
    }

    private ObjectResult Problem(Error error)
    {
        var status = error.Type switch
        {
            ErrorType.NotFound => StatusCodes.Status404NotFound,
            ErrorType.Conflict => StatusCodes.Status409Conflict,
            _ => StatusCodes.Status400BadRequest
        };
        var problem = ProblemDetailsFactory.CreateProblemDetails(HttpContext, status, title: error.Code, detail: error.Message);
        problem.Extensions["code"] = error.Code;
        return new ObjectResult(problem) { StatusCode = status };
    }
}
