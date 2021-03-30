﻿using MongoDB.Bson.Serialization.Attributes;
using Moralar.Data.Enum;
using System;
using System.Collections.Generic;
using System.Text;
using UtilityFramework.Infra.Core.MongoDb.Data.Modelos;

namespace Moralar.Data.Entities
{

    [BsonIgnoreExtraElements]
    public class InformativeSended : ModelBase
    {
        public string InformativeId { get; set; }
        public string FamilyId { get; set; }
        public long DateViewed { get; set; }
        public override string CollectionName => nameof(InformativeSended);
    }
}
