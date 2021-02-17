using Moralar.Data.Enum;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using UtilityFramework.Application.Core.ViewModels;

namespace Moralar.Domain.ViewModels.Course
{
    public class CourseViewModel : BaseViewModel
    {
        [Required(ErrorMessage = DefaultMessages.FieldRequired)]
        [Display(Name = "Título")]
        public string Title { get; set; }
        
        
        
        [Required(ErrorMessage = DefaultMessages.FieldRequired)]
        [Display(Name = "Imagem")]
        public string Img { get; set; }
        
        
        
        [Required(ErrorMessage = DefaultMessages.FieldRequired)]
        [Display(Name = "Data de Início")]
        public long StartDate { get; set; }


        [Required(ErrorMessage = DefaultMessages.FieldRequired)]
        [Display(Name = "Data final")]
        public long EndDate { get; set; }



        [Required(ErrorMessage = DefaultMessages.FieldRequired)]
        [Display(Name = "Hora de início")]
        public string Schedule { get; set; }




        [Required(ErrorMessage = DefaultMessages.FieldRequired)]
        [Display(Name = "Local do curso")]
        public string Place { get; set; }



        [Required(ErrorMessage = DefaultMessages.FieldRequired)]
        [Display(Name = "Carga horária")]
        public string WorkLoad { get; set; }



        [Required(ErrorMessage = DefaultMessages.FieldRequired)]
        [Display(Name = "Descrição do curso ")]
        public string Description { get; set; }



        [Required(ErrorMessage = DefaultMessages.FieldRequired)]
        [Display(Name = "Idade do público alvo")]
        public string TargetAudienceAge { get; set; }



        [Required(ErrorMessage = DefaultMessages.FieldRequired)]
        [Display(Name = "Gênero")]
        public TypeGenre TypeGenre { get; set; }


        [Required(ErrorMessage = DefaultMessages.FieldRequired)]
        [Display(Name = "Quantidade de vagas")]
        public int NumberOfVacancies { get; set; }
    }
}
