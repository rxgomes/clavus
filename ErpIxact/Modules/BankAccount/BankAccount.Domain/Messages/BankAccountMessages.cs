namespace BankAccount.Domain.Messages;

public static class BankAccountMessages
{
    public static class Errors
    {
        public const string NameBankRequired = "Nome do banco é obrigatório.";
        public const string NameBankTooLong = "Nome do banco deve ter no máximo 25 caracteres.";
        public const string NumberAccountRequired = "Número da conta é obrigatório.";
        public const string NumberAccountTooLong = "Número da conta deve ter no máximo 15 caracteres.";
        public const string DigitAccountRequired = "Dígito da conta é obrigatório.";
        public const string DigitAccountTooLong = "Dígito da conta deve ter no máximo 5 caracteres.";
        public const string AlreadyExistsActive = "Conta já existe e status ativa";
        public const string AlreadyExistsInactive = "Conta já existe e o status é inativo";
        public const string NotFound = "Conta bancária não encontrada.";
    }

    public static class Success
    {
        public const string Created = "Conta bancária criada com sucesso.";
        public const string Deactivated = "Conta bancária desativada com sucesso.";
    }
}
