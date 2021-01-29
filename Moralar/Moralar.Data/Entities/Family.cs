using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Moralar.Data.Entities.Auxiliar;
using Moralar.Data.Enum;
using System;
using System.Collections.Generic;
using System.Text;
using UtilityFramework.Infra.Core.MongoDb.Data.Modelos;

namespace Moralar.Data.Entities
{
    [BsonIgnoreExtraElements]
    public class Family : ModelBase
    {
        public Family()
        {
            Members = new List<FamilyMember>();
        }
        /// <summary>
        /// Dados do Titular
        /// </summary>
        public FamilyHolder Holder { get; set; }

        /// <summary>
        /// Dados do conjuge
        /// </summary>
        public FamilySpouse Spouse { get; set; }

        /// <summary>
        /// Dados do membro da Família
        /// </summary>
        public List<FamilyMember> Members { get; set; }

        /// <summary>
        /// Dados Financeiros
        /// </summary>
        public FamilyFinancial Financial { get; set; }

        /// <summary>
        /// Dados de Priorização
        /// </summary>
        public FamilyPriorization Priorization { get; set; }
        public string Password { get; set; }
        public bool IsFirstAcess { get; set; }
        public string ProviderId { get; set; }
        public string Reason { get; set; }


        public override string CollectionName => nameof(Family);

    }
}
