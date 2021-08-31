using System;
using System.Collections.Generic;
using System.Text;
using UtilityFramework.Application.Core.ViewModels;

namespace Moralar.Domain.ViewModels.NotificationSended
{
    public class NotificationSendedListViewModel: BaseViewModel
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public long Created { get; set; }
        public string NotificationId { get; set; }
        public string FamilyId { get; set; }
        public long? DateViewed { get; set; }
        public bool Arquived { get; set; }
    }
}
