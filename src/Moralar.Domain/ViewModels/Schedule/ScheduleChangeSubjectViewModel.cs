﻿using Moralar.Data.Enum;
using System;
using System.Collections.Generic;
using System.Text;
using UtilityFramework.Application.Core.ViewModels;

namespace Moralar.Domain.ViewModels.Schedule
{
    public class ScheduleChangeSubjectViewModel : BaseViewModel
    {
        public string Place { get; set; }
        public string Description { get; set; }
        public long Date { get; set; }
        public string FamilyId { get; set; }
        public TypeSubject TypeSubject { get; set; }
        //public TypeScheduleStatus TypeScheduleStatus { get; set; }
    }
}
