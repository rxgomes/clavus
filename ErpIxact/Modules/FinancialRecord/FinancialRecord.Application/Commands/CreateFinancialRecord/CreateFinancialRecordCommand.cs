using FinancialRecord.Application.DTOs;
using FinancialRecord.Domain.Enums;
using MediatR;
using Shared.Kernel;

namespace FinancialRecord.Application.Commands.CreateFinancialRecord;

public record CreateFinancialRecordCommand(
    string Description,
    decimal Value,
    DateOnly DueDate,
    int TotalInstallment,
    FinancialRecordStatus Status,
    decimal? PaidValue = null,
    DateOnly? PaymentDate = null,
    string? DigitableLine = null)
    : IRequest<Result<List<FinancialRecordDto>>>;
