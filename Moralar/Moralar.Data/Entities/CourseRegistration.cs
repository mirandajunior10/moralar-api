using MongoDB.Bson.Serialization.Attributes;
using Moralar.Data.Enum;
using System;
using System.Collections.Generic;
using System.Text;
using UtilityFramework.Infra.Core.MongoDb.Data.Modelos;

namespace Moralar.Data.Entities
{

    [BsonIgnoreExtraElements]
    public class CourseRegistration : ModelBase
    {
        public  string FamilyId { get; set; }
        public string CourseId { get; set; }
        public string FamilyHolder { get; set; }
        public string FamilyCpf { get; set; }
        public TypeStatusCourse TypeStatusCourse { get; set; }
        public override string CollectionName => nameof(CourseRegistration);
    }
}
