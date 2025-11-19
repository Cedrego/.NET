using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace LaboratorioNET.Entities
{
   public class Registro
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("IDCarrera")]
        public string IDCarrera { get; set; } = string.Empty;

        [BsonElement("IdentifiCorredor")]
        public string IdentifiCorredor { get; set; } = string.Empty;

        [BsonElement("NumDorsal")]
        public int NumDorsal { get; set; }

        [BsonElement("Tiempos")]
        public List<DateTime> Tiempos { get; set; } = new List<DateTime>();
    }
}
