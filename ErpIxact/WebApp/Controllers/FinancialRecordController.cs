using FinancialRecord.Application.Commands.CreateFinancialRecord;
using FinancialRecord.Application.Commands.DeactivateFinancialRecord;
using FinancialRecord.Application.Commands.UpdateOverdueStatus;
using FinancialRecord.Application.Queries.GetById;
using FinancialRecord.Application.Queries.GetByMonthYear;
using FinancialRecord.Application.Queries.GetByStatus;
using FinancialRecord.Application.Queries.GetCurrentMonth;
using FinancialRecord.Application.Queries.GetOverdue;
using FinancialRecord.Application.Queries.GetUpcomingDue;
using FinancialRecord.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Shared.Kernel;

namespace WebApp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FinancialRecordController : ControllerBase
{
    private readonly IMediator _mediator;

    public FinancialRecordController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("financial-record/create")]
    public async Task<IActionResult> Create(
        [FromBody] CreateFinancialRecordCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);

        if (!result.IsSuccess)
        {
            return ToErrorResponse(result);
        }

        return Created(string.Empty, result.Value);
    }

    [HttpDelete("financial-record/deactivate/{id:guid}")]
    public async Task<IActionResult> Deactivate(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new DeactivateFinancialRecordCommand(id), cancellationToken);

        if (!result.IsSuccess)
        {
            return ToErrorResponse(result);
        }

        return Ok(new { message = result.Value });
    }

    [HttpPatch("financial-record/update-overdue-status")]
    public async Task<IActionResult> UpdateOverdueStatus(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new UpdateOverdueStatusCommand(), cancellationToken);

        if (!result.IsSuccess)
        {
            return ToErrorResponse(result);
        }

        return Ok(new { message = result.Value });
    }

    [HttpGet("financial-record/get-by-id/{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetFinancialRecordByIdQuery(id), cancellationToken);

        if (!result.IsSuccess)
        {
            return ToErrorResponse(result);
        }

        return Ok(result.Value);
    }

    [HttpGet("financial-record/get-current-month")]
    public async Task<IActionResult> GetCurrentMonth(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(new GetCurrentMonthQuery(page, pageSize), cancellationToken);

        if (!result.IsSuccess)
        {
            return ToErrorResponse(result);
        }

        return Ok(result.Value);
    }

    [HttpGet("financial-record/get-by-month-year/{month}/{year}")]
    public async Task<IActionResult> GetByMonthYear(
        int month,
        int year,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(new GetByMonthYearQuery(month, year, page, pageSize), cancellationToken);

        if (!result.IsSuccess)
        {
            return ToErrorResponse(result);
        }

        return Ok(result.Value);
    }

    [HttpGet("financial-record/get-upcoming-due/{days}")]
    public async Task<IActionResult> GetUpcomingDue(
        int days,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(new GetUpcomingDueQuery(days, page, pageSize), cancellationToken);

        if (!result.IsSuccess)
        {
            return ToErrorResponse(result);
        }

        return Ok(result.Value);
    }

    [HttpGet("financial-record/get-overdue")]
    public async Task<IActionResult> GetOverdue(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(new GetOverdueQuery(page, pageSize), cancellationToken);

        if (!result.IsSuccess)
        {
            return ToErrorResponse(result);
        }

        return Ok(result.Value);
    }

    [HttpGet("financial-record/get-by-status/{status}")]
    public async Task<IActionResult> GetByStatus(
        FinancialRecordStatus status,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(new GetByStatusQuery(status, page, pageSize), cancellationToken);

        if (!result.IsSuccess)
        {
            return ToErrorResponse(result);
        }

        return Ok(result.Value);
    }

    private IActionResult ToErrorResponse(Result result) => result.ErrorType switch
    {
        ErrorType.NotFound => NotFound(new { error = result.Error }),
        ErrorType.Conflict => Conflict(new { error = result.Error }),
        _ => BadRequest(new { error = result.Error }),
    };
}
