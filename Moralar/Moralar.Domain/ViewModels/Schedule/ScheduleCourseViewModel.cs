using Moralar.Data.Enum;
using System;
using System.Collections.Generic;
using System.Text;

namespace Moralar.Domain.ViewModels.Schedule
{
    public class ScheduleCourseViewModel
    {
        public string Title { get; set; }
        public long StartDate { get; set; }
        public long EndDate { get; set; }
        public TypeStatusCourse TypeStatusCourse { get; set; }
    }
}
