using MediatR;
using Microsoft.AspNetCore.Mvc;
using Patners.Application.Commands.CreatePartner;
using Patners.Application.Commands.UpdatePartner;
using Patners.Application.Queries.GetPartnerById;
using Patners.Application.Queries.GetPartnerByDocNumber;
using Patners.Application.Queries.GetPartnerByName;
using Patners.Application.Queries.GetPartners;
using Shared.Kernel;

namespace WebApp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PartnersController : ControllerBase
{
    private readonly IMediator _mediator;

    public PartnersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("partner/get-all")]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetPartnersQuery(), cancellationToken);

        if (!result.IsSuccess)
        {
            return ToErrorResponse(result);
        }

        return Ok(result.Value);
    }

    [HttpGet("partner/get-by-id/{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetPartnerByIdQuery(id), cancellationToken);

        if (!result.IsSuccess)
        {
            return ToErrorResponse(result);
        }

        return Ok(result.Value);
    }

    [HttpGet("partner/get-by-doc/{docNumber}")]
    public async Task<IActionResult> GetByDocNumber(string docNumber, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetPartnerByDocNumberQuery(docNumber), cancellationToken);

        if (!result.IsSuccess)
        {
            return ToErrorResponse(result);
        }

        return Ok(result.Value);
    }

    [HttpGet("partner/get-by-name/{name}")]
    public async Task<IActionResult> GetByName(string name, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetPartnerByNameQuery(name), cancellationToken);
        
        if (!result.IsSuccess)
        {
            return ToErrorResponse(result);
        }

        return Ok(result.Value);
    }

    [HttpPut("partner/update/{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdatePartnerCommand command, CancellationToken cancellationToken)
    {
        if (id != command.Id)
        {
            return BadRequest(new { error = "O id da rota não corresponde ao id do corpo da requisição." });
        }

        var result = await _mediator.Send(command, cancellationToken);

        if (!result.IsSuccess)
        {
            return ToErrorResponse(result);
        }

        return Ok(result.Value);
    }

    [HttpPost("partner/create")]
    public async Task<IActionResult> Create(
        [FromBody] CreatePartnerCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);

        if (!result.IsSuccess)
        {
            return ToErrorResponse(result);
        }

        return CreatedAtAction(nameof(GetById), new { id = result.Value!.Id }, result.Value);
    }

    private IActionResult ToErrorResponse(Result result) => result.ErrorType switch
    {
        ErrorType.NotFound => NotFound(new { error = result.Error }),
        ErrorType.Conflict => Conflict(new { error = result.Error }),
        _ => BadRequest(new { error = result.Error }),
    };
}
