using System;
using System.Collections.Generic;
using System.Text;
using UtilityFramework.Application.Core.ViewModels;
using UtilityFramework.Infra.Core.MongoDb.Data.Modelos;

namespace Moralar.Domain.ViewModels.NotificationSended
{
    public class NotificationSendedViewModel : BaseViewModel
    {

        public string FamilyId { get; set; }
        public long DateViewed { get; set; }
    }
}