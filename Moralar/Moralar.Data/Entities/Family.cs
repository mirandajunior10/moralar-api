using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
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
        /// <summary>
        /// Dados do Titular
        /// </summary>
        public string HolderNumber { get; set; }
        public string HolderName { get; set; }
        public string HolderCpf { get; set; }
        public long HolderBirthday { get; set; }
        public string HolderGenre { get; set; }
        public string HolderEmail { get; set; }
        public string HolderPhone { get; set; }
        [BsonRepresentation(BsonType.Int32)]
        public TypeScholarity HolderScholarity { get; set; }


        /// <summary>
        /// Dados do conjuge
        /// </summary>
        public string SpouseNumber { get; set; }
        public string SpouseName { get; set; }
        public string SpouseCpf { get; set; }
        public long SpouseBirthday { get; set; }
        public string SpouseGenre { get; set; }
        public string SpouseEmail { get; set; }
        public string SpousePhone { get; set; }
        public string SpouseRelationship { get; set; }
        [BsonRepresentation(BsonType.Int32)]
        public TypeScholarity SpouseScholarity { get; set; }


        /// <summary>
        /// Dados do membro da Família
        /// </summary>
        public string FamilyMemberNumber { get; set; }
        public string FamilyMemberName { get; set; }
        public string FamilyMemberCpf { get; set; }
        public long FamilyMemberBirthday { get; set; }
        public string FamilyMemberGenre { get; set; }
        public string FamilyMemberEmail { get; set; }
        public string FamilyMemberPhone { get; set; }
        public string FamilyMemberRelationship { get; set; }
        [BsonRepresentation(BsonType.Int32)]
        public TypeScholarity FamilyMemberScholarity { get; set; }
        [BsonRepresentation(BsonType.Int32)]
        public TypeKingShip FamilyKinShip { get; set; }
        public string Password { get; set; }
        public bool IsFirstAcess { get; set; }

        public override string CollectionName => nameof(Family);

    }
}
