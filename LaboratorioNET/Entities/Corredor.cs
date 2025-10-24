using Google.Cloud.Firestore;

namespace LaboratorioNET.Entities
{
    [FirestoreData]
    public class Corredor
    {
        [FirestoreDocumentId]
        public string? Id { get; set; }

        [FirestoreProperty("documentoIdentidad")]
        public string DocumentoIdentidad { get; set; } = string.Empty;

        [FirestoreProperty("nombre")]
        public string Nombre { get; set; } = string.Empty;

        [FirestoreProperty("origen")]
        public string Origen { get; set; } = string.Empty;

        [FirestoreProperty("telefono")]
        public string Telefono { get; set; } = string.Empty;
    }
}