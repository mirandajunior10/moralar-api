using MongoDB.Bson.Serialization.Attributes;
using Moralar.Data.Enum;
using System;
using System.Collections.Generic;
using System.Text;
using UtilityFramework.Infra.Core.MongoDb.Data.Modelos;

namespace Moralar.Data.Entities
{

    [BsonIgnoreExtraElements]
    public class Informative : ModelBase
    {
        public string Image { get; set; }
        public string Description { get; set; }
        public long Date { get; set; }
        public TypeStatusActiveInactive Status { get; set; }
        public List<string> FamilyId { get; set; }
        public override string CollectionName => nameof(Informative);
    }
}
