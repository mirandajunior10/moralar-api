using System.ComponentModel;
using System.Runtime.Serialization;

namespace Moralar.Data.Enum
{
    public enum RouteNotification
    {
        System,

    }

    public enum TypeProvider
    {
        [EnumMember(Value = "Senha")]
        [Description("Senha")]
        Password = 0,
        [EnumMember(Value = "Facebook")]
        [Description("Facebook")]
        Facebook = 1,
        [EnumMember(Value = "AppleId")]
        [Description("AppleId")]
        Apple = 2,
        [EnumMember(Value = "GoogleId")]
        [Description("GoogleId")]
        Google = 3
    }

    public enum TypeAccount
    {
        [EnumMember(Value = "Corrente")]
        [Description("Corrente")]
        CC = 0,
        [EnumMember(Value = "Poupança")]
        [Description("Poupança")]
        CP = 1

    }

    public enum TypePersonBank
    {
        [EnumMember(Value = "Pessoa Jurídica")]
        [Description("Pessoa Jurídica")]

        LegalPerson = 0,
        [EnumMember(Value = "Pessoa Física")]
        [Description("Pessoa Física")]
        PhysicalPerson = 1
    }

    public enum Language
    {
        [EnumMember(Value = "Ingles")]
        [Description("Ingles")]
        En = 0,
        [EnumMember(Value = "Espanhol")]
        [Description("Espanhol")]
        Es = 1,
        [EnumMember(Value = "Português")]
        [Description("Português")]
        Pt = 2

    }
    public enum TypeProperty
    {
        [EnumMember(Value = "Casa")]
        [Description("Casa")]
        Casa = 0,
        [EnumMember(Value = "Apartamento")]
        [Description("Apartamento")]
        Apartamento = 1
    }
    public enum TypePropertyRegularization
    {
        [EnumMember(Value = "Regular")]
        [Description("Regular")]
        Regular = 0,
        [EnumMember(Value = "Regularizável")]
        [Description("Regularizável")]
        Regularizável = 1
    }

    public enum TypePropertyGasInstallation
    {
        [EnumMember(Value = "Gás encanado")]
        [Description("Gás encanado")]
        GasEncanado = 0,
        [EnumMember(Value = "Botijão")]
        [Description("Botijão")]
        Botijao = 1
    }
    public enum TypeProfile
    {
        [EnumMember(Value = "Usuário")]
        [Description("Usuário")]
        Profile = 0,
        [EnumMember(Value = "Usuário administrador")]
        [Description("Usuário administrador")]
        UserAdministrator = 1
    }
    public enum TypeScholarity
    {
        [EnumMember(Value = "Não possui")]
        [Description("Não possui")]
        NaoPossui = 0,
        [EnumMember(Value = "Fundamental (incompleto)")]
        [Description("Fundamental (incompleto)")]
        FundamentalIncompleto = 1,
        [EnumMember(Value = "Fundamental (completo)")]
        [Description("Fundamental (completo)")]
        FundamentalCompleto = 2,
        [EnumMember(Value = "Médio (incompleto)")]
        [Description("Médio (incompleto)")]
        MedioIncompleto = 3,
        [EnumMember(Value = "Médio (completo)")]
        [Description("Médio (completo)")]
        MedioCompleto = 4,
        [EnumMember(Value = "Superior (incompleto)")]
        [Description("Superior (incompleto)")]
        SuperiorIncompleto = 5,
        [EnumMember(Value = "Superior (completo)")]
        [Description("Superior (completo)")]
        SuperiorCompleto = 6,
        [EnumMember(Value = " Pós-graduação (incompleto)")]
        [Description(" Pós-graduação (incompleto)")]
        PosGraduaçãoIncompleto = 7,
        [EnumMember(Value = "Pós-graduação (completo")]
        [Description("Pós-graduação (completo")]
        PosGraduaçãoCompleto = 8
    }
    public enum TypeKingShip
    {
        [EnumMember(Value = "Filha")]
        [Description("Filha")]
        Filha = 0,
        [EnumMember(Value = "Filho")]
        [Description("Filho")]
        Filho = 1,
        [EnumMember(Value = "Mãe")]
        [Description("Mãe")]
        Mae = 2,
        [EnumMember(Value = "Pai")]
        [Description("Pai")]
        Pai = 3,
        [EnumMember(Value = "Avó")]
        [Description("Avó")]
        AvoMulher = 4,
        [EnumMember(Value = "Avô")]
        [Description("Avô")]
        AvoHomem = 5,
        [EnumMember(Value = "Enteada")]
        [Description("Enteada")]
        Enteada = 6,
        [EnumMember(Value = " Enteado")]
        [Description("Enteado")]
        Enteado = 7,
        [EnumMember(Value = "Tia")]
        [Description("Tia")]
        Tia = 8,
        [EnumMember(Value = "Tio")]
        [Description("Tio")]
        Tio = 9,
        [EnumMember(Value = "Outro")]
        [Description("Outro")]
        Outro = 10
    }
    public enum TypeAction
    {
        [EnumMember(Value = "Cadastro")]
        [Description("Cadastro")]
        Register,
        [EnumMember(Value = "Alteração")]
        [Description("Alteração")]
        Change,
        [EnumMember(Value = "Remoção")]
        [Description("Remoção")]
        Delete,
        [EnumMember(Value = "Upload")]
        [Description("Upload")]
        Upload,
        [EnumMember(Value = "Download")]
        [Description("Download")]
        Download,
        [EnumMember(Value = "UnBlock")]
        [Description("Block")]
        Block,
        [EnumMember(Value = "UnBlock")]
        [Description("UnBlock")]
        UnBlock,
    }
    public enum TypeResposible
    {
        [EnumMember(Value = "Gestor")]
        [Description("Gestor")]
        Gestor = 0,
        [EnumMember(Value = "Usuário administrador")]
        [Description("Usuário administrador")]
        UserAdminstrator = 1,
        [EnumMember(Value = "Usuário administrador/gestor")]
        [Description("Usuário administrador/gestor")]
        UserAdminstratorGestor = 2
    }
    public enum TypeGenre
    {
        [EnumMember(Value = "Feminino")]
        [Description("Feminino")]
        Feminino = 0,
        [EnumMember(Value = "Masculino")]
        [Description("Masculino")]
        Masculino = 1,
        [EnumMember(Value = "Outro")]
        [Description("Outro")]
        Outro = 2
    }
    public enum LocalAction
    {
        [EnumMember(Value = "Família")]
        [Description("Família")]
        Familia = 0,
        [EnumMember(Value = "Propriedade Residencial")]
        [Description("Propriedade Residencial")]
        ResidencialProperty,
    }


}