using Google.Cloud.Firestore;

namespace LaboratorioNET.Entities
{
    [FirestoreData]
    public class Admin
    {
        [FirestoreDocumentId]
        public string? Id { get; set; }

        // 👇 usa exactamente el nombre del campo que aparece en Firestore
        [FirestoreProperty("Contraseña")]
        public string Contrasena { get; set; } = string.Empty;

        [FirestoreProperty("IDAdmin")]
        public string IDAdmin { get; set; } = string.Empty;
    }
}
