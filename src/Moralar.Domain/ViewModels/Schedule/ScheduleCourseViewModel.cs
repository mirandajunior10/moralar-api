using System;
using System.Collections.Generic;
using System.Text;
using Moralar.Data.Enum;

namespace Moralar.Domain.ViewModels.Schedule
{
    public class ScheduleCourseViewModel
    {
        public long? Created { get; set; } = DateTimeOffset.Now.ToUnixTimeSeconds();
        public string Title { get; set; }
        public long StartDate { get; set; }
        public long EndDate { get; set; }
        public TypeStatusCourse TypeStatusCourse { get; set; }
    }
}
