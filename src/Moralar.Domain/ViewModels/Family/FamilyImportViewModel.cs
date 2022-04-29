

using Moralar.Data.Entities;
using Moralar.Data.Enum;
using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;
using UtilityFramework.Application.Core;
using static Moralar.Domain.Util;

namespace Moralar.Domain.ViewModels.Family
{
    public class FamilyImportViewModel
    {
        
        [Display(Name = "Número do cadastro*")]
        [Required(ErrorMessage = DefaultMessages.FieldRequired)]
        [Column(1)]
        public string Numero_do_cadastro { get; set; }
        
        [Display(Name = "Nome do titular*")]
        [Required(ErrorMessage = DefaultMessages.FieldRequired)]
        [Column(2)]
        public string Nome_do_titular { get; set; }
        
        [Display(Name = "Cpf do titular* Ex: ###.###.###-##")]
        [Required(ErrorMessage = DefaultMessages.FieldRequired)]
        [IsValidCpf(ErrorMessage = DefaultMessages.CpfInvalid)]
        [JsonConverter(typeof(OnlyNumber))]
        [Column(3)]
        public string Cpf_do_titular { get; set; }
       
        [Display(Name = "Data de nascimento*  Ex: (dd/mm/aaaa)")]
        [Required(ErrorMessage = DefaultMessages.FieldRequired)]
        [RegularExpression(@"^([0]?[0-9]|[12][0-9]|[3][01])[./-]([0]?[1-9]|[1][0-2])[./-]([0-9]{4}|[0-9]{2})( 00\:00\:00)?$", ErrorMessage = "Verifique o formato de dados enviado no campo \"{0}\", o mesmo deve ser formatado como texto")]
        [Column(4)]
        public string Data_de_Nascimento { get; set; }
        [Column(5)]
        [DropDownExcel(Options = typeof(TypeGenre), AllowBlank = true)]
        public string Genero { get; set; }
       
        [Display(Name = "E-mail*")]        
        [EmailAddress(ErrorMessage = DefaultMessages.EmailInvalid)]
        [JsonConverter(typeof(ToLowerCase))]
        [Column(6)]
        public string E_mail { get; set; }
        [Display(Name = "Telefone  Ex: 55119981-3547")]
        [Column(7)]
        public string Telefone { get; set; }       
        [Column(8)]
        [DropDownExcel(Options = typeof(TypeScholarity), AllowBlank = true)]
        public string Escolaridade { get; set; }
        
        [Display(Name = "CEP* Ex: #####-###")]
        [Required(ErrorMessage = DefaultMessages.FieldRequired)]
        [Column(9)]
        public string CEP { get; set; }
        
        [Display(Name = "Endereço*")]
        [Required(ErrorMessage = DefaultMessages.FieldRequired)]
        [Column(10)]
        public string StreetAddress { get; set; }
        
        [Display(Name = "Número*")]
        [Required(ErrorMessage = DefaultMessages.FieldRequired)]
        [Column(11)]

        public string Number { get; set; }
        
        [Display(Name = "Nome da cidade*")]
        [Required(ErrorMessage = DefaultMessages.FieldRequired)]
        [Column(12)]
        public string CityName { get; set; }
        
        [Display(Name = "Nome do estado*")]
        [Required(ErrorMessage = DefaultMessages.FieldRequired)]
        [Column(13)]
        public string StateName { get; set; }
       
        [Display(Name = "UF*")]
        [Required(ErrorMessage = DefaultMessages.FieldRequired)]
        [Column(14)]
        public string StateUf { get; set; }
        
        [Display(Name = "Bairro*")]
        [Required(ErrorMessage = DefaultMessages.FieldRequired)]
        [Column(15)]
        public string Neighborhood { get; set; }
        
        [Display(Name = "Complemento")]
        [Column(16)]
        public string Complement { get; set; }

        [Column(17)]
        public string Nome_do_Conjuge { get; set; }
        [Column(18)]
        public string Data_de_Nascimento_do_Conjuge { get; set; }
        [Column(19)]
        [DropDownExcel(Options = typeof(TypeGenre), AllowBlank = true)]
        public string Genero_Conjuge { get; set; }
        [Column(20)]
        [DropDownExcel(Options = typeof(TypeScholarity), AllowBlank = true)]
        public string Escolaridade_Conjuge { get; set; }
       
        [Display(Name = "Renda familiar* Ex: R$ #,##")]
        [Required(ErrorMessage = DefaultMessages.FieldRequired)]
        [Column(21)]
        public string Renda_Familiar { get; set; }
       
        [Display(Name = "Valor imóvel demolido* Ex: R$ #,##")]
        [Required(ErrorMessage = DefaultMessages.FieldRequired)]
        [Column(22)]
        public string Valor_imovel_demolido { get; set; }
       
        [Display(Name = "Valor para a compra do imóvel* Ex: R$ #,##")]
        [Required(ErrorMessage = DefaultMessages.FieldRequired)]
        [Column(23)]
        public string Valor_para_compra_de_imovel { get; set; }
        [Column(24)]
        public string Valor_Incremento { get; set; }
        [Column(25)]
        public string Frente_de_Obras { get; set; }
        [Column(26)]
        public string Deficiencia_que_demande_imovel_acessivel { get; set; }
        [Column(27)]
        public string Idoso_acima_de_80_anos { get; set; }
        [Column(28)]
        public string Idoso_60_ate_79_anos { get; set; }
        [Column(29)]
        public string Mulher_atendida_por_medida_protetiva { get; set; }
        [Column(30)]
        public string Mulher_chefe_de_familia { get; set; }
        [Column(31)]
        public string Monoparental { get; set; }
        [Column(32)]
        public string Familia_com_mais_5_pessoas { get; set; }
        [Column(33)]
        public string Filhos_menores_de_18_anos { get; set; }
        [Column(34)]
        public string Chefe_de_familia_sem_renda { get; set; }
        [Column(35)]
        public string Beneficio_de_prestacao_continuada { get; set; }
        [Column(36)]
        public string Bolsa_Familia { get; set; }
        [Column(37)]
        public string Coabitacao_involuntaria { get; set; }
        [Column(38)]
        public string Renda_familiar_de_ate_dois_salarios_minimos { get; set; }

       
    }    
}

