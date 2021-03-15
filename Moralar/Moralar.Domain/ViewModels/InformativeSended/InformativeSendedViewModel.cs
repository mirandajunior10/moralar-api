using System;
using System.Collections.Generic;
using System.Text;
using UtilityFramework.Application.Core.ViewModels;

namespace Moralar.Domain.ViewModels.InformativeSended
{
    public class InformativeSendedViewModel : BaseViewModel
    {
        public string InformativeId { get; set; }
        public List<string> FamilyId { get; set; }
    }
}
