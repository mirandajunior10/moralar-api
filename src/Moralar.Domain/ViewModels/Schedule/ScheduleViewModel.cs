using Moralar.Data.Enum;

using UtilityFramework.Application.Core.ViewModels;

namespace Moralar.Domain.ViewModels.Schedule
{
    public class ScheduleViewModel : BaseViewModel
    {
        public long Date { get; set; }
        public string Place { get; set; }
        public string Description { get; set; }
        public string FamilyId { get; set; }
        public string HolderNumber { get; set; }
        public string HolderName { get; set; }
        public string HolderCpf { get; set; }

        public TypeSubject TypeSubject { get; set; }

        public TypeScheduleStatus TypeScheduleStatus { get; set; }
    }
}