using System.ComponentModel.DataAnnotations;

using UtilityFramework.Application.Core;

namespace Moralar.Domain.ViewModels.Family
{
    public class FamilyMemberImportViewModel
    {
        [Display(Name = "Cpf Titular")]       
        [Column(1)]
        public string HolderCpf { get; set; }
        [Display(Name = "Nome do membro")]        
        [Column(2)]
        public string Name { get; set; }

        [Display(Name = "Data de nascimento (DD/MM/AAAA)")]
        [Column(3)]
        public string Birthday { get; set; }

        
        [Display(Name = " Gênero")]
        [Column(4)]
        public string Genre { get; set; }
       
        [Display(Name = "Grau de parentesco")]             
        [Column(5)]
        public string KinShip { get; set; }
        
        [Display(Name = "Escolaridade")]
        [Column(6)]
        public string Scholarity { get; set; }
        
    }
}