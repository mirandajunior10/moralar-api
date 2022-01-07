using MongoDB.Bson.Serialization.Attributes;

using Moralar.Data.Enum;

namespace Moralar.Data.Entities.Auxiliar
{
    [BsonIgnoreExtraElements]
    public class FamilyMember
    {
        public string Name { get; set; }
        public long Birthday { get; set; }
        public TypeGenre Genre { get; set; }
        public TypeKingShip KinShip { get; set; }
        public TypeScholarity Scholarity { get; set; }
    }
}
