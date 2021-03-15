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
    public class ResidencialProperty : ModelBase
    {
        public string Code { get; set; }
        public List<string> Photo { get; set; }
        public string Project { get; set; }
        public string Reason { get; set; }
        public ResidencialPropertyAdress ResidencialPropertyAdress { get; set; }
       
        public ResidencialPropertyFeatures ResidencialPropertyFeatures { get; set; }
        public TypeStatusResidencial TypeStatusResidencialProperty { get; set; }
        public string FamiliIdResidencialChosen { get; set; }
        public override string CollectionName => nameof(ResidencialProperty);
    }
}
