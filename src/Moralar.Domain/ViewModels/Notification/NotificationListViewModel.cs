using Newtonsoft.Json;
using UtilityFramework.Application.Core;
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
        [JsonConverter(typeof(PathImage))]
        public string Image { get; set; }
        public bool Arquived { get; set; }
    }
}
