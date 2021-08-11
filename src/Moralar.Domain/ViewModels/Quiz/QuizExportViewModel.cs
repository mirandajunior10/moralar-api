using System.ComponentModel.DataAnnotations;

namespace Moralar.Domain.ViewModels.Quiz
{
    public class QuizExportViewModel
    {
        [Display(Name = "Título")]
        public string Title { get; set; }
       
        [Display(Name = "Data do Cadastro")]
        public string Created { get; set; }
    }
}
