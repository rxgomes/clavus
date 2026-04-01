namespace FinancialRecord.Domain.Messages;

public static class FinancialRecordMessages
{
    public static class Errors
    {
        public const string DescriptionRequired          = "A descrição é obrigatória.";
        public const string DescriptionTooLong           = "A descrição deve ter no máximo 100 caracteres.";
        public const string ValueRequired                = "O valor deve ser maior que zero.";
        public const string DueDateInvalid               = "A data de vencimento não pode ser menor que a data atual.";
        public const string TotalInstallmentInvalid      = "O total de parcelas deve ser maior que zero.";
        public const string PaidValueInvalid             = "O valor pago deve ser maior que zero.";
        public const string PaidValuePartialNotAllowed   = "Pagamento parcial não é permitido. O valor pago deve ser igual ou maior ao valor do registro.";
        public const string DigitableLineTooLong         = "A linha digitável deve ter no máximo 50 caracteres.";
        public const string DigitableLineOnlyDigits      = "A linha digitável deve conter apenas dígitos numéricos.";
        public const string StatusInvalid                = "O status informado é inválido.";
        public const string NotFound                     = "Registro financeiro não encontrado.";
    }

    public static class Success
    {
        public const string Created     = "Registro financeiro criado com sucesso.";
        public const string Updated     = "Registro financeiro atualizado com sucesso.";
        public const string Deactivated = "Registro financeiro desativado com sucesso.";
        public const string OverdueUpdated = "Status de registros vencidos atualizado com sucesso.";
    }
}
