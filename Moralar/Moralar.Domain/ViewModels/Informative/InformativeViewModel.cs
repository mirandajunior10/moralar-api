using System;
using System.Collections.Generic;
using System.Text;
using UtilityFramework.Application.Core.ViewModels;

namespace Moralar.Domain.ViewModels.Informative
{
    public class InformativeViewModel : BaseViewModel
    {
        public string Image { get; set; }
        public string Description { get; set; }
        public long Date { get; set; }
        public List<string> FamilyId { get; set; }
    }
}
