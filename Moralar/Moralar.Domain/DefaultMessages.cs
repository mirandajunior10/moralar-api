using OfficeOpenXml.FormulaParsing.Excel.Functions.Text;

namespace Moralar.Domain
{
    public class DefaultMessages
    {

        /*REQUIRED*/
        public const string FieldRequired = "Informe o campo {0}";
        public const string Minlength = "Informe no mínimo {1} caracteres no campo {0}.";
        public const string Maxlength = "Informe no máximo {1} caracteres no campo {0}.";
        public const string Range = "Informe no mínimo {1} e no maxímo {2} caracteres no campo {0}.";
        public const string RequiredPassword = "Informe o campo senha.";

        /*CUSTOM MESSAGES*/
        public const string ProfileBlocked = "Usuário bloqueado. entre em contato com suporte";
        public const string UserAdministratorBlocked = "Acesso bloqueado. entre em contato com suporte";
        public const string ProfileUnRegistred = "Informe um e-mail registrado.";
        public const string AwaitApproval = "Seu perfil está sendo analizado, logo entraremos em contato";
        public const string PasswordNoMatch = "Senha atual não confere com a informada";
        public const string ConfirmPasswordNoMatch = "Confirmação de senha não confere com a nova senha informada";
        public const string InvalidRegisterAddress = "Não é possível registrar esse tipo de endereço.";
        public const string AmountPhoto = "A foto deve ter no mínimo 1 e no máximo 15 imagens";
        public const string AmountPhotoInclude = "A inclusão das fotos está acima do máximo de 15 imagens";

        /*IN USE*/
        public const string CpfInUse = "Cpf em uso.";
        public const string CarPlateInUse = "Placa em uso.";
        public const string CnpjInUse = "Cnpj em uso.";
        public const string PhoneInUse = "Telefone em uso.";
        public const string LoginInUse = "Login em uso.";
        public const string EmailInUse = "Email em uso.";
        public const string FacebookInUse = "Ja existe um usuário com essa conta do facebook.";
        public const string GoogleIdInUse = "Ja existe um usuário com essa conta do google plus.";
        public const string MemberInUse = "Membro da família já cadastrado";

        /*INVALID*/
        public const string InvalidCredencials = "Credênciais inválidas.";
        public const string EmailInvalid = "Informe um email válido.";
        public const string CpfInvalid = "Informe um cpf válido.";
        public const string CnpjInvalid = "Informe um cnpj válido.";
        public const string PhoneInvalid = "Informe um telefone válido.";
        public const string InvalidIdentifier = "Formato de id inválido.";
        public const string InvalidEntityMap = "Mapeamento inválido.";
        public const string InvalidLogin = "Login e/ou senha inválidos.";
        public const string InvalidCredentials = "Credênciais inválidas.";

        /*NOT FOUND*/
        public const string UserAdministratorNotFound = "Usuário de acessso não encontrado";
        public const string ProfileNotFound = "Usuário não encontrado";
        public const string ResidencialPropertyNotFound = "Propriedade não encontrado";
        public const string QuizNotFound = "Quiz não encontrado";
        public const string DescriptionNotFound = "Descrição não encontrado";
        public const string QuestionNotFound = "Quiz não encontrado";
        public const string FamilyNotFound = "Família não encontrado";
        public const string MemberNotFound = "Membro da família não encontrado";
        public const string PhotoNotFound = "Membro da família não encontrado";
        public const string CreditCardNotFound = "Cartão não encontrado";
        public const string CreditCardNotFoundIugu = "Forma de pagamento não encontrada";
        public const string ZipCodeNotFound = "Cep não encontrado";
        public const string Updated = "Alterações salvas com sucesso";
        public const string Registred = "Cadastrado com sucesso";
        public const string Deleted = "Removido com sucesso";
        public const string EmptyProviderId = "Informe o providerId";
        public const string AppleIdInUse = "AppleId em uso, tente fazer login.";
        public const string AccessBlocked = "Acesso bloqueado. entre em contato com suporte";
        public const string OnlyAdministrator = "Você precisa ser administrador para realizar essa ação";
        public const string MessageException = "Ocorreu um erro, verifique e tente novamente";
        public const string LogActionNotFound = "Log de ação não encontrado.";

    }
}