using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace LaboratorioNET.Entities
{
    public class Carrera
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("idCarrera")]
        public int IdCarrera { get; set; }

        [BsonElement("nombre")]
        public string Nombre { get; set; } = string.Empty;

        [BsonElement("fecha")]
        public DateTime Fecha { get; set; }

        [BsonElement("lugarSalida")]
        public string LugarSalida { get; set; } = string.Empty;

        [BsonElement("fechaInicioInsc")]
        public DateTime FechaInicioInsc { get; set; }

        [BsonElement("cupoParticipantes")]
        public int CupoParticipantes { get; set; }

        [BsonElement("distanciaOCheckpoint")]
        public double DistanciaOCheckpoint { get; set; }

        [BsonElement("seccTorres")]
        public int SeccTorres { get; set; }
    }
}