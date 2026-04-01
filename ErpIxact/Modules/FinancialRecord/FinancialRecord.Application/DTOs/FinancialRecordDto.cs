using FinancialRecord.Domain.Enums;

namespace FinancialRecord.Application.DTOs;

public record FinancialRecordDto(
    Guid Id,
    string Description,
    decimal Value,
    DateOnly DueDate,
    int TotalInstallment,
    int Installment,
    FinancialRecordStatus Status,
    bool Active);
