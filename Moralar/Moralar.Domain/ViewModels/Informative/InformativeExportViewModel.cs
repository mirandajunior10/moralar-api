using System.ComponentModel.DataAnnotations;

namespace Moralar.Domain.ViewModels.Informative
{
    public class InformativeExportViewModel
    {
        /// <summary>
        /// Data de publicação
        /// </summary> 
        [Display(Name = "Data de publicação")]
        public string DatePublishDate { get; set; }
        /// <summary>
        /// Imagem
        /// </summary>
        [Display(Name = "Imagem")]
        public string Image { get; set; }
        /// <summary>
        /// Descrição do informativo
        /// </summary>
        [Display(Name = "Descrição do informativo")]
        public string Description { get; set; }
        /// <summary>
        /// Data
        /// </summary>
        [Display(Name = "Data")]
        public string Date { get; set; }

    }
}
