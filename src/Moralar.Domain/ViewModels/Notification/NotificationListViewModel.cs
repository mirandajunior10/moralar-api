using System;
using System.Collections.Generic;
using System.Text;
using UtilityFramework.Application.Core.ViewModels;

namespace Moralar.Domain.ViewModels.Notification
{
    public class NotificationListViewModel : BaseViewModel
    {
        public bool Status { get; set; }
        public long? DateViewed { get; set; }
        public long Created { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }        
        public string Image { get; set; }
    }
}
