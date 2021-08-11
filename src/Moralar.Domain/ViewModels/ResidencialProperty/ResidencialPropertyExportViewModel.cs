using System.ComponentModel.DataAnnotations;

namespace Moralar.Domain.ViewModels.ResidencialProperty
{
    public class ResidencialPropertyExportViewModel
    {

        /// <summary>
        /// "Código do imóvel
        /// </summary> 
        [Display(Name = "Código do imóvel")]
        public string Code { get; set; }
        /// <summary>
        /// Tipo do imóvel
        /// </summary>
        [Display(Name = "Tipo do imóvel")]
        public string TypeProperty { get; set; }
        /// <summary>
        /// Valor do imóvel
        /// </summary>
        [Display(Name = "Valor do imóvel")]
        public decimal PropertyValue { get; set; }
        /// <summary>
        /// Bairro de localização
        /// </summary>

        [Display(Name = "Bairro de localização")]
        public string Neighborhood { get; set; }
        /// <summary>
        /// Status
        /// </summary>
        [Display(Name = "Status")]
        public string TypeStatusResidencialProperty { get; set; }
        /// <summary>
        /// Situação
        /// </summary>
        [Display(Name = "Situação")]
        public string Blocked { get; set; }

    }
}
