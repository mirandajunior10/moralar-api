using System;
using System.Collections.Generic;
using System.Text;

namespace Moralar.Data.Entities.Auxiliar
{
    public class FamilyPriorization
    {
        public bool WorkFront { get; set; }
        public bool PermanentDisabled { get; set; }
        public bool ElderlyOverEighty { get; set; }
        public bool WomanServedByProtectiveMeasure { get; set; }
        public bool FemaleBreadwinner { get; set; }
        public bool SingleParent { get; set; }
        public bool FamilyWithMoreThanFivePeople { get; set; }
        public bool ChildUnderEighteen { get; set; }
        public bool HeadOfHouseholdWithoutIncome { get; set; }
        public bool BenefitOfContinuedProvision { get; set; }
        public bool FamilyPurse { get; set; }
        public bool InvoluntaryCohabitation { get; set; }
        public bool FamilyIncomeOfUpTwoMinimumWages { get; set; }
    }
}
