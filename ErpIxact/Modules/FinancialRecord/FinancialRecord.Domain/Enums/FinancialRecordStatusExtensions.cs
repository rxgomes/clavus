namespace FinancialRecord.Domain.Enums;

public static class FinancialRecordStatusExtensions
{
    public static string ToLabel(this FinancialRecordStatus status) => status switch
    {
        FinancialRecordStatus.Pending => "Pendente",
        FinancialRecordStatus.Overdue  => "Vencido",
        FinancialRecordStatus.Paid     => "Pago",
        _ => status.ToString()
    };
}
