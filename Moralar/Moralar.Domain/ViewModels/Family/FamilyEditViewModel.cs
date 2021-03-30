using MongoDB.Bson;
using Moralar.Data.Enum;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using UtilityFramework.Application.Core.ViewModels;

namespace Moralar.Domain.ViewModels.Family
{
    public class FamilyEditViewModel: BaseViewModel
    {

        /// <summary>
        /// Dados do Endereço
        /// </summary>
        public FamilyAddressViewModel Address { get; set; }
        /// <summary>
        /// Dados do Titular da Família
        /// </summary>
        public FamilyHolderMinViewModel Holder { get; set; }


        /// <summary>
        /// Dados do Cônjuge
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
        public FamilyPriorizationViewModel Priorization { get; set; }
    }
}
