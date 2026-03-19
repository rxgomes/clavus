namespace Patners.Domain.Messages;

public static class PartnersMessages
{
    public static class Errors
    {
        public const string DocNumberRequired = "Número documento é obrigatório.";
        public const string DocNumberInvalid = "Número documento inválido.";
        public const string NameRequired = "Nome do parceiro é obrigatório.";
        public const string NotFound = "Parceiro não encontrado.";
        public const string AlreadyExists = "Parceiro já cadastrado com este documento.";
    }

    public static class Alerts
    {
        public const string DocNumberChanged = "O documento do parceiro foi alterado.";
    }

    public static class Success
    {
        public const string Created = "Parceiro criado com sucesso.";
        public const string Updated = "Parceiro atualizado com sucesso.";
        public const string Deleted = "Parceiro removido com sucesso.";
    }
}
