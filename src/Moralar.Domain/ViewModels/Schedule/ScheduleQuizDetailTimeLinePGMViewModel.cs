using System;
using System.Collections.Generic;
using System.Text;

namespace Moralar.Domain.ViewModels.Schedule
{
    public class ScheduleQuizDetailTimeLinePGMViewModel
    {
        public long? Created { get; set; } = DateTimeOffset.Now.ToUnixTimeSeconds();
        public string Title { get; set; }
        public string Date { get; set; }
        public string HasAnswered { get; set; }
        public string QuizId { get; set; }
        public string QuizFamilyId { get; set; }
    }
}
