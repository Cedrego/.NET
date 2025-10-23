using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace LaboratorioNET.Entities
{
    public class Admin
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("Contrasena")]
        public string Contrasena { get; set; } = string.Empty;

        [BsonElement("IDAdmin")]
        public string IDAdmin { get; set; } = string.Empty;
    }
}