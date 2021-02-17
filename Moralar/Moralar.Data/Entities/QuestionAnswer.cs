using Moralar.Data.Enum;
using System;
using System.Collections.Generic;
using System.Text;
using UtilityFramework.Infra.Core.MongoDb.Data.Modelos;

namespace Moralar.Data.Entities
{
    public class QuestionAnswer : ModelBase
    {
        public List<string> QuestionDescriptionId { get; set; }

        public string QuestionId { get; set; }


        public string AnswerDescription { get; set; }


        public TypeResponse TypeResponse { get; set; }


        public string FamilyId { get; set; }
        public string FamilyNumber { get; set; }
        public string FamilyHolderName { get; set; }
        public string FamilyHolderCpf { get; set; }



        public string ResponsibleForResponses { get; set; }
        public string ResponsibleForResponsesName { get; set; }
        public string ResponsibleForResponsesCpf { get; set; }

        public override string CollectionName => nameof(QuestionAnswer);
    }
}
