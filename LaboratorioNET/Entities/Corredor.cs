using Google.Cloud.Firestore;

namespace LaboratorioNET.Entities
{
    [FirestoreData]
    public class Corredor
    {
        [FirestoreDocumentId]
        public string? Id { get; set; }

        [FirestoreProperty("DocumentoIdentidad")]
        public string DocumentoIdentidad { get; set; } = string.Empty;

        [FirestoreProperty("Nombre")]
        public string Nombre { get; set; } = string.Empty;

        [FirestoreProperty("Origen")]
        public string Origen { get; set; } = string.Empty;

        [FirestoreProperty("Telefono")]
        public string Telefono { get; set; } = string.Empty;
    }
}
