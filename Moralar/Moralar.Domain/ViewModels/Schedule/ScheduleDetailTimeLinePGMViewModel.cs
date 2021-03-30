using Moralar.Data.Enum;
using Moralar.Domain.ViewModels.Course;
using Moralar.Domain.ViewModels.Family;
using System;
using System.Collections.Generic;
using System.Text;
using UtilityFramework.Application.Core.ViewModels;

namespace Moralar.Domain.ViewModels.Schedule
{
    public class ScheduleDetailTimeLinePGMViewModel: BaseViewModel
    {
        public ScheduleDetailTimeLinePGMViewModel()
        {
            DetailQuiz = new List<ScheduleQuizDetailTimeLinePGMViewModel>();
            DetailEnquete = new List<ScheduleQuizDetailTimeLinePGMViewModel>();
            Courses = new List<ScheduleCourseViewModel>();
        }
        public long Date { get; set; }
        public string Place { get; set; }
        public string Description { get; set; }
        public string FamilyId { get; set; }
        public string HolderNumber { get; set; }
        public string HolderName { get; set; }
        public string HolderCpf { get; set; }

        public TypeSubject TypeSubject { get; set; }

        public TypeScheduleStatus TypeScheduleStatus { get; set; }

        public List<ScheduleQuizDetailTimeLinePGMViewModel> DetailQuiz { get; set; }
        public List<ScheduleQuizDetailTimeLinePGMViewModel> DetailEnquete { get; set; }
        public List<ScheduleCourseViewModel> Courses { get; set; }
    }
}
