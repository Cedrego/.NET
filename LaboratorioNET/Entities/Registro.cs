using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace LaboratorioNET.Entities
{
    public class Registro
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("CICorredor")]
        public string CICorredor { get; set; } = string.Empty;

        [BsonElement("IDCarrera")]
        public string IDCarrera { get; set; } = string.Empty;

        [BsonElement("FechaRegistro")]
        public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;
    }
}