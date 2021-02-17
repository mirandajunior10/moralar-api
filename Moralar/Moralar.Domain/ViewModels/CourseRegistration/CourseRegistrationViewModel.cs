using Moralar.Data.Enum;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using UtilityFramework.Application.Core.ViewModels;

namespace Moralar.Domain.ViewModels.Course
{
    public class CourseRegistrationViewModel : BaseViewModel
    {
        [Required(ErrorMessage = DefaultMessages.FieldRequired)]
        [Display(Name = "Id da família")]
        public string FamilyId { get; set; }


        [Required(ErrorMessage = DefaultMessages.FieldRequired)]
        [Display(Name = "Id do Curso")]
        public string CourseId { get; set; }

    }
}
