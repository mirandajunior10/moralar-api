using Moralar.Data.Entities.Auxiliar;
using System;
using System.Collections.Generic;
using System.Text;
using UtilityFramework.Application.Core.ViewModels;

namespace Moralar.Domain.ViewModels.PropertiesInterest
{
    public class PropertiesInterestViewModel : BaseViewModel
    {
        public PropertiesInterestViewModel() {
            PriorityRates = new List<PriorityRate>();
        }

        public string Id { get; set; }
        public string FamilyId { get; set; }
        public string ResidencialPropertyId { get; set; }
        public string HolderName { get; set; }
        public string HolderEmail { get; set; }
        public string HolderCpf { get; set; }
        public string HolderNumber { get; set; }
        public string ResidencialCode { get; set; }
        public int Interest { get; set; }
        public ResidencialPropertyAdress ResidencialPropertyAdress { get; set; }
        public List<PriorityRate> PriorityRates { get; set; }

       
    }
}
