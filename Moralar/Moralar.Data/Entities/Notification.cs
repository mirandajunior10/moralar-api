using MongoDB.Bson.Serialization.Attributes;
using Moralar.Data.Enum;
using System;
using System.Collections.Generic;
using System.Text;
using UtilityFramework.Infra.Core.MongoDb.Data.Modelos;

namespace Moralar.Data.Entities
{
    [BsonIgnoreExtraElements]
    public class Notification : ModelBase
    {
        public string Title { get; set; }
        public string Description { get; set; }

        public override string CollectionName => nameof(Notification);
    }

}
