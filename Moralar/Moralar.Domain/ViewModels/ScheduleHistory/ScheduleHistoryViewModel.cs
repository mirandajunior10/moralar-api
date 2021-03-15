using Moralar.Data.Enum;
using System;
using System.Collections.Generic;
using System.Text;
using UtilityFramework.Application.Core.ViewModels;

namespace Moralar.Domain.ViewModels.ScheduleHistory
{
    public class ScheduleHistoryViewModel : BaseViewModel
    {
        public string ScheduleId { get; set; }
        public TypeSubject TypeSubject { get; set; }
        public TypeScheduleStatus TypeScheduleStatus { get; set; }
    }
}
