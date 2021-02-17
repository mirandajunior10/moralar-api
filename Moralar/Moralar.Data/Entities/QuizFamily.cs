using MongoDB.Bson.Serialization.Attributes;
using Moralar.Data.Enum;
using System;
using System.Collections.Generic;
using System.Text;
using UtilityFramework.Infra.Core.MongoDb.Data.Modelos;

namespace Moralar.Data.Entities
{
    [BsonIgnoreExtraElements]
    public class QuizFamily : ModelBase
    {
        public string FamilyId { get; set; }
        public string HolderName { get; set; }
        public string HolderCpf { get; set; }
        public string HolderNumber { get; set; }
        public string QuizId { get; set; }
        public TypeStatusQuiz TypeStatusQuiz { get; set; }
        public override string CollectionName => nameof(QuizFamily);
    }
}
