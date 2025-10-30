using Google.Cloud.Firestore;

namespace LaboratorioNET.Entities
{
    [FirestoreData]
    public class Registro
    {
        [FirestoreDocumentId]
        public string? Id { get; set; }

        [FirestoreProperty("IDCarrera")]
        public string IDCarrera { get; set; } = string.Empty;

        [FirestoreProperty("IdentifiCorredor")]
        public string IdentifiCorredor { get; set; } = string.Empty;

        [FirestoreProperty("NumDorsal")]
        public int NumDorsal { get; set; }

        [FirestoreProperty("Tiempos")]
        public List<Timestamp> Tiempos { get; set; } = new List<Timestamp>();
    }
}
