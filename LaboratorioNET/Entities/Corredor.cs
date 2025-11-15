using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
namespace LaboratorioNET.Entities
{
    public class Corredor
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("documentoIdentidad")]
        public string DocumentoIdentidad { get; set; } = string.Empty;

        [BsonElement("nacionalidad")]
        public string Nacionalidad { get; set; } = string.Empty;

        [BsonElement("fechaNacimiento")]
        public string FechaNacimiento { get; set; } = string.Empty;

        [BsonElement("nombre")]
        public string Nombre { get; set; } = string.Empty;

        [BsonElement("contraseña")]
        public string Contraseña { get; set; } = string.Empty;

        [BsonElement("telefono")]
        public string Telefono { get; set; } = string.Empty;

        [BsonElement("correo")]
        public string Correo { get; set; } = string.Empty;

        [BsonElement("rol")]
        public string Rol { get; set; } = "Corredor";

        [BsonElement("registros")]
        public List<string> Registros { get; set; } = new List<string>();
    }
}