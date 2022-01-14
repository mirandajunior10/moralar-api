using System.ComponentModel.DataAnnotations;

namespace Moralar.Domain.ViewModels.Course
{
    public class CourseExportViewModel
    {
        /// <summary>
        /// Título
        /// </summary>
        [Display(Name = "Título")]
        public string Title { get; set; }
        /// <summary>
        /// Data de Início
        /// </summary>
        [Display(Name = "Data de Início")]
        public string StartDate { get; set; }
        /// <summary>
        /// Data final
        /// </summary>
        [Display(Name = "Data final")]
        public string EndDate { get; set; }
        /// <summary>
        /// Quantidade de vagas
        /// </summary>
        [Display(Name = "Quantidade de vagas")]
        public int NumberOfVacancies { get; set; }
        /// <summary>
        /// Total de inscritos
        /// </summary>
        [Display(Name = "Total de inscritos")]
        public int TotalInscriptions { get; set; }
        /// <summary>
        /// Total aguardando lista de espera
        /// </summary>
        [Display(Name = "Total aguardando lista de espera")]
        public int TotalWaitingList { get; set; }
        /// <summary>
        /// Situação
        /// </summary>
        [Display(Name = "Situação")]
        public string Blocked { get; set; }
    }
}
