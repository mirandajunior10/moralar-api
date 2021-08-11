using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Moralar.Data.Enum;
using System;
using System.Collections.Generic;
using System.Text;

namespace Moralar.Data.Entities.Auxiliar
{
    [BsonIgnoreExtraElements]
    public class FamilyMember
    {
        public string Name { get; set; }
        public long   Birthday { get; set; }
        public TypeGenre Genre { get; set; }
        [BsonRepresentation(BsonType.Int32)]
        public TypeKingShip KinShip { get; set; }
        public TypeScholarity Scholarity { get; set; }
    }
}
