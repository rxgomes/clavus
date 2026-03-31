namespace CreditCard.Domain.Messages;

public static class CreditCardMessages
{
    public static class Errors
    {
        public const string NameRequired = "Nome do cartão é obrigatório.";
        public const string NameTooLong = "Nome do cartão deve ter no máximo 15 caracteres.";
        public const string FlagRequired = "Bandeira do cartão é obrigatória.";
        public const string FlagTooLong = "Bandeira do cartão deve ter no máximo 15 caracteres.";
        public const string CloseDayInvalid = "Dia de fechamento deve estar entre 1 e 31.";
        public const string DueDayInvalid = "Dia de vencimento deve estar entre 1 e 31.";
        public const string AlreadyExistsActive = "Cartão já existe com status ativa";
        public const string AlreadyExistsInactive = "Cartão já existe com status é inativo";
        public const string NotFound = "Cartão de crédito não encontrado.";
    }

    public static class Success
    {
        public const string Created = "Cartão de crédito criado com sucesso.";
    }
}
