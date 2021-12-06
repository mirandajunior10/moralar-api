using Moralar.Data.Enum;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using UtilityFramework.Application.Core.ViewModels;

namespace Moralar.Domain.ViewModels.Property
{
    public class ResidencialPropertyViewModel : BaseViewModel
    {
        [Required(ErrorMessage = DefaultMessages.FieldRequired)]
        [Display(Name = "Código do imóvel")]
        public string Code { get; set; }


        [Required(ErrorMessage = DefaultMessages.FieldRequired)]
        [Display(Name = "Foto")]
        //[Range(1, 15, ErrorMessage = "A foto deve ter no mínimo {1} e no máximo {2} imagens")]
        public List<string> Photo { get; set; }//mínimo 1 e máximo 15


        [Required(ErrorMessage = DefaultMessages.FieldRequired)]
        [Display(Name = "Planta")]
        public string Project { get; set; }
        public bool Blocked { get; set; }
        public TypeStatusResidencial TypeStatusResidencialProperty { get; set; }
        public ResidencialPropertyAdressViewModel ResidencialPropertyAdress { get; set; }
        public ResidencialPropertyFeatureViewModel ResidencialPropertyFeatures { get; set; }
        /// <summary>
        /// Total de famílias interessadas
        /// </summary>
        [Display(Name = "Famílias interessadas")]
        public long InterestedFamilies { get; set; }
    }
}
