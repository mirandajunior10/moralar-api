using Moralar.Data.Enum;
using Moralar.Domain.ViewModels.Question;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using UtilityFramework.Application.Core;
using UtilityFramework.Application.Core.ViewModels;

namespace Moralar.Domain.ViewModels.Quiz
{
    public class QuizViewModel: BaseViewModel
    {
        [Required(ErrorMessage = DefaultMessages.FieldRequired)]
        [Display(Name = "Título")]
        public string Title { get; set; }       
        public TypeQuiz TypeQuiz { get; set; }
        public QuestionRegisterViewModel QuestionRegister { get; set; }
       
        //[Display(Name = "Data")]
        //[IsReadOnly]
        //public long? Created { get; set; }

        [Display(Name = "Status")]
        [IsReadOnly]
        public TypeStatus TypeStatus { get; set; }
    }
}
