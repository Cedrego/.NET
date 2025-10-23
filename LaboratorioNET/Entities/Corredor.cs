using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace LaboratorioNET.Entities
{
    public class Corredor
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("DocumentoIdentidad")]
        public string DocumentoIdentidad { get; set; } = string.Empty;

        [BsonElement("Nombre")]
        public string Nombre { get; set; } = string.Empty;

        [BsonElement("Origen")]
        public string Origen { get; set; } = string.Empty;

        [BsonElement("Telefono")]
        public string Telefono { get; set; } = string.Empty;

        [BsonElement("Registros")]
        public List<string> Registros { get; set; } = new List<string>();
    }
}