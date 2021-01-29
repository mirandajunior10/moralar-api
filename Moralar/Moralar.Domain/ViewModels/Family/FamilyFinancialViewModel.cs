using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Moralar.Domain.ViewModels.Family
{
    public class FamilyFinancialViewModel
    {
        [Required(ErrorMessage = DefaultMessages.FieldRequired)]
        [Display(Name = "Renda familiar")]
        public decimal FamilyIncome { get; set; }

        [Required(ErrorMessage = DefaultMessages.FieldRequired)]
        [Display(Name = "Valor da avaliação do imóvel a ser demolido")]
        public decimal PropertyValueForDemolished { get; set; }

        [Required(ErrorMessage = DefaultMessages.FieldRequired)]
        [Display(Name = "Valor máximo para compra do imóvel")]
        public decimal MaximumPurchase { get; set; }

        [Display(Name = "Valor de incremento")]
        public decimal IncrementValue { get; set; }
    }
}
