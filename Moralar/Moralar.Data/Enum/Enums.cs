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

    public enum TypeProfile
    {
        [EnumMember(Value = "Usuário")]
        [Description("Usuário")]
        Profile = 0,
        [EnumMember(Value = "Usuário administrador")]
        [Description("Usuário administrador")]
        UserAdministrator = 1


    }
}