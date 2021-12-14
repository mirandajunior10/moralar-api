using System.ComponentModel.DataAnnotations;

using Moralar.Data.Enum;

namespace Moralar.Domain.ViewModels.Family
{
    public class FamilyHolderExportViewModel
    {
        public string Id { get; set; }

        [Display(Name = "Número do cadastro")]
        public string Number { get; set; }
       
        [Display(Name = "Nome do titular")]
        public string Name { get; set; }
       
        [Display(Name = "CPF do titular")]
        public string Cpf { get; set; }

        [Display(Name = "Status da linha do tempo")]
        public TypeSubject TypeSubject { get; set; }
    }
}
