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
        public string FullName { get; set; }
        [Required(ErrorMessage = DefaultMessages.FieldRequired)]
        public string Login { get; set; }

        [Required(ErrorMessage = DefaultMessages.FieldRequired)]
        [EmailAddress(ErrorMessage = DefaultMessages.EmailInvalid)]
        [JsonConverter(typeof(ToLowerCase))]
        public string Email { get; set; }
        [Required(ErrorMessage = DefaultMessages.FieldRequired)]
        [Display(Name = "Foto")]
        public string Photo { get; set; }
        [Required(ErrorMessage = DefaultMessages.FieldRequired)]
        [IsValidCpf(ErrorMessage = DefaultMessages.CpfInvalid)]
        [JsonConverter(typeof(OnlyNumber))]
        public string Cpf { get; set; }
        [Display(Name = "Telefone")]
        public string Phone { get; set; }
        public bool Blocked { get; set; }
    }
}