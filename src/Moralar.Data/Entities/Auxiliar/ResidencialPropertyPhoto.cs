using MongoDB.Bson.Serialization.Attributes;


namespace Moralar.Data.Entities.Auxiliar
{
    [BsonIgnoreExtraElements]
    public class ResidencialPropertyPhoto
    {
        public string ImageUrl { get; set; }
        public string Description { get; set; }
    }
}
