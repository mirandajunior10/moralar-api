using Moralar.Data.Enum;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using UtilityFramework.Application.Core;
using UtilityFramework.Application.Core.ViewModels;

namespace Moralar.Domain.ViewModels
{
    public class ProfileViewModel : BaseViewModel
    {
        [Required(ErrorMessage = DefaultMessages.FieldRequired)]
        [Display(Name = "Nome completo")]
        public string Name { get; set; }


        [Required(ErrorMessage = DefaultMessages.FieldRequired)]
        [Display(Name = "Cargo")]
        public string JobPost { get; set; }

        [Required(ErrorMessage = DefaultMessages.FieldRequired)]
        [IsValidCpf(ErrorMessage = DefaultMessages.CpfInvalid)]
        [JsonConverter(typeof(OnlyNumber))]
        public string Cpf { get; set; }


        [Required(ErrorMessage = DefaultMessages.FieldRequired)]
        [EmailAddress(ErrorMessage = DefaultMessages.EmailInvalid)]
        [JsonConverter(typeof(ToLowerCase))]
        public string Email { get; set; }

        [Required(ErrorMessage = DefaultMessages.FieldRequired)]
        [Display(Name = "Telefone")]
        public string Phone { get; set; }

        [Required(ErrorMessage = DefaultMessages.FieldRequired)]
        [Display(Name = "Tipo do Perfil do Usuário")]
        public TypeUserProfile TypeProfile { get; set; }
        public bool Blocked { get; set; }
    }
}