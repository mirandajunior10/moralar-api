using Moralar.Data.Enum;
using Moralar.Domain.ViewModels.Question;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using UtilityFramework.Application.Core.ViewModels;

namespace Moralar.Domain.ViewModels.Quiz
{
    public class QuizFamilyListViewModel
    {
        public string Id{get;set;}
        public string QuizId { get; set; }
        public string Title { get; set; }
        public string Created { get; set; }
        public string HolderName { get; set; }
        public string HolderCpf { get; set; }
        public string HolderNumber { get; set; }
        public string Status { get; set; }
        public TypeQuiz TypeQuiz { get; set; }
    }
}
