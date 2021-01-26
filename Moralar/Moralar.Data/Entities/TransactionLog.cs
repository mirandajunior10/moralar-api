using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Moralar.Data.Enum;
using System;
using System.Collections.Generic;
using System.Text;
using UtilityFramework.Infra.Core.MongoDb.Data.Modelos;

namespace Moralar.Data.Entities
{
    /// <summary>
    /// TABELA DE TRANSAÇÕES
    /// DynamicDataBefore -- DADOS ANTES DA TRANSÇÃO
    /// DynamicDataAfter --  DADOS DEPOIS DA TRANSÇÃO
    /// </summary>
    [BsonIgnoreExtraElements]
    public class TransactionLog
    {

        public string UserId { get; set; }
        public long Date { get; set; }
        public string Route { get; set; }
        public string DynamicDataBefore { get; set; }
        public string DynamicDataAfter { get; set; }
        
        
        [BsonRepresentation(BsonType.Int32)]
        public string Type { get; set; }
        [BsonRepresentation(BsonType.Int32)]
        public string HttpVerb { get; set; }
        private string CollectionName => nameof(TransactionLog);
    }
}
