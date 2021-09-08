using Moralar.Data.Enum;
using System.ComponentModel.DataAnnotations;

namespace Moralar.Domain.ViewModels
{
    public class SearchViewModel
    {
        public SearchViewModel()
        {
            Page = 1;
            Limit = 30;
        }
        /// <summary>
        /// Termo de busca
        /// </summary>
        /// <example>Nome</example>
        [Required(ErrorMessage = nameof(DefaultMessages.FieldRequired))]
        [Display(Name = nameof(DefaultMessages.FieldSearchTerm))]
        public string SearchTerm { get; set; }
        public TypeSubject? TypeSubject { get; set; }
        public int Page { get; set; }
        public int Limit { get; set; }
    }
}