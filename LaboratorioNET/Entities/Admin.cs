using Google.Cloud.Firestore;

namespace LaboratorioNET.Entities
{  
    [FirestoreData]
    public class Admin
    {
        [FirestoreDocumentId]
        public string? Id { get; set; }

        [FirestoreProperty("contrasena")]
        public string Contrasena { get; set; } = string.Empty;

        [FirestoreProperty("idAdmin")]
        public string IDAdmin { get; set; } = string.Empty;
    }
}