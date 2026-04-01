using BankAccount.Application.Commands.CreateBankAccount;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Shared.Kernel;

namespace WebApp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BankAccountController : ControllerBase
{
    private readonly IMediator _mediator;

    public BankAccountController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("bank-account/create")]
    public async Task<IActionResult> Create(
        [FromBody] CreateBankAccountCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);

        if (!result.IsSuccess)
        {
            return ToErrorResponse(result);
        }

        return Created(string.Empty, result.Value);
    }

    private IActionResult ToErrorResponse(Result result) => result.ErrorType switch
    {
        ErrorType.NotFound => NotFound(new { error = result.Error }),
        ErrorType.Conflict => Conflict(new { error = result.Error }),
        _ => BadRequest(new { error = result.Error }),
    };
}
