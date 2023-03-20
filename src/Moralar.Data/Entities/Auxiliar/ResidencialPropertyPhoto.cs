using System;
using System.Collections.Generic;
using System.Text;
using MongoDB.Bson.Serialization.Attributes;
using Moralar.Data.Enum;
using UtilityFramework.Infra.Core.MongoDb.Data.Modelos;

namespace Moralar.Data.Entities.Auxiliar
{
    [BsonIgnoreExtraElements]
    public class ResidencialPropertyPhoto
    {
        public string ImageUrl { get; set; }
        public string Description { get; set; }
    }
}
