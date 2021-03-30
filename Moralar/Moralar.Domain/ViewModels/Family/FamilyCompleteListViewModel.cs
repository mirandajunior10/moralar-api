using AutoMapper;
using System;
using System.Collections.Generic;
using System.Text;
using UtilityFramework.Application.Core.ViewModels;

namespace Moralar.Domain.ViewModels.Family
{
    public class FamilyCompleteListViewModel:BaseViewModel
    {
        public FamilyCompleteListViewModel()
        {
            Priorization = new List<PriorityRateViewModel>();
        }
        /// <summary>
        /// Dados do Titular
        /// </summary>
        public FamilyHolderViewModel Holder { get; set; }

        /// <summary>
        /// Dados do conjuge
        /// </summary>
        public FamilySpouseViewModel Spouse { get; set; }


        /// <summary>
        /// Dados do membro da Família
        /// </summary>
        public List<FamilyMemberViewModel> Members { get; set; }


        /// <summary>
        /// Dados Financeiros
        /// </summary>
        public FamilyFinancialViewModel Financial { get; set; }

        /// <summary>
        /// Dados de Priorização
        /// </summary>
        [IgnoreMapAttribute]
        public List<PriorityRateViewModel> Priorization { get; set; }
        public string MotherName { get; set; }
        public string MotherCityBorned { get; set; }
    }
}
