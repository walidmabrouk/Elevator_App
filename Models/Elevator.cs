using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace crudmongo.Models
{
    public class Elevator
    { 
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }
        public string State { get; set; }
        public string Floor { get; set; } 
        public string Direction { get; set; }
    }
}
