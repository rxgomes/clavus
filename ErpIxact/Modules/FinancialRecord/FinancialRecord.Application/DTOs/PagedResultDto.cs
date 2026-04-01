namespace FinancialRecord.Application.DTOs;

public record PagedResultDto<T>(IReadOnlyList<T> Items, int TotalCount, int Page, int PageSize);
