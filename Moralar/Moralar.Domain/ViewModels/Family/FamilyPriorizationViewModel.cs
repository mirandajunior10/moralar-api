using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Moralar.Domain.ViewModels.Family
{
    public class FamilyPriorizationViewModel
    {
        [Required(ErrorMessage = DefaultMessages.FieldRequired)]
        [Display(Name = "Frente de obras")]
        public bool WorkFront { get; set; }


        [Required(ErrorMessage = DefaultMessages.FieldRequired)]
        [Display(Name = "Deficiência permanente que demande imóvel acessível")]
        public bool PermanentDisabled { get; set; }


        [Required(ErrorMessage = DefaultMessages.FieldRequired)]
        [Display(Name = "Idoso mais de 80 anos")]
        public bool ElderlyOverEighty { get; set; }


        [Required(ErrorMessage = DefaultMessages.FieldRequired)]
        [Display(Name = "Idoso 60 - 79 anos")]
        public bool YearsInSextyAndSeventyNine { get; set; }


        [Required(ErrorMessage = DefaultMessages.FieldRequired)]
        [Display(Name = "Mulher atendida por medida protetiva")]
        public bool WomanServedByProtectiveMeasure { get; set; }


        [Required(ErrorMessage = DefaultMessages.FieldRequired)]
        [Display(Name = "Mulher chefe de família")]
        public bool FemaleBreadWinner { get; set; }


        [Required(ErrorMessage = DefaultMessages.FieldRequired)]
        [Display(Name = "Monoparental (pai e mãe)")]
        public bool SingleParent { get; set; }


        [Required(ErrorMessage = DefaultMessages.FieldRequired)]
        [Display(Name = "Família com mais de 5 pessoas")]
        public bool FamilyWithMoreThanFivePeople { get; set; }


        [Required(ErrorMessage = DefaultMessages.FieldRequired)]
        [Display(Name = "Filho(s) menor(es) de 18 anos na composição familiar")]
        public bool ChildUnderEighteen { get; set; }


        [Required(ErrorMessage = DefaultMessages.FieldRequired)]
        [Display(Name = "Chefe de família sem renda *")]
        public bool HeadOfHouseholdWithoutIncome { get; set; }


        [Required(ErrorMessage = DefaultMessages.FieldRequired)]
        [Display(Name = "Benefício de prestação continuada - BPC")]
        public bool BenefitOfContinuedProvision { get; set; }


        [Required(ErrorMessage = DefaultMessages.FieldRequired)]
        [Display(Name = "Bolsa família")]
        public bool FamilyPurse { get; set; }


        [Required(ErrorMessage = DefaultMessages.FieldRequired)]
        [Display(Name = "Coabitação involuntária")]
        public bool InvoluntaryCohabitation { get; set; }


        [Required(ErrorMessage = DefaultMessages.FieldRequired)]
        [Display(Name = "Renda familiar de até 2 salários mínimos")]
        public bool FamilyIncomeOfUpTwoMinimumWages { get; set; }

    }
}
