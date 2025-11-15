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
        public string IdCarrera { get; set; } = string.Empty;

        [BsonElement("nombre")]
        public string Nombre { get; set; } = string.Empty;

        [BsonElement("fechaInicio")]
        public DateTime FechaInicio { get; set; }

        [BsonElement("lugarSalida")]
        public string LugarSalida { get; set; } = string.Empty;

        [BsonElement("fechaInicioInsc")]
        public DateTime FechaInicioInsc { get; set; }

        [BsonElement("limiteParticipantes")]
        public int LimiteParticipantes { get; set; }

        [BsonElement("cantSecciones")]
        public int CantSecciones { get; set; }

        [BsonElement("tipo")]
        public string Tipo { get; set; } = string.Empty;

        [BsonElement("terminada")]
        public bool Terminada { get; set; } = false;
    }
}