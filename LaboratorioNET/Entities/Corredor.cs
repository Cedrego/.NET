using Google.Cloud.Firestore;

namespace LaboratorioNET.Entities
{
    [FirestoreData]
    public class Corredor
    {
        [FirestoreProperty("documentoIdentidad")]
        public string DocumentoIdentidad { get; set; } = string.Empty;

        [FirestoreProperty("nacionalidad")]
        public string Nacionalidad { get; set; } = string.Empty;

        [FirestoreProperty("fechaNacimiento")]
        public string FechaNacimiento { get; set; } = string.Empty;

        [FirestoreProperty("nombre")]
        public string Nombre { get; set; } = string.Empty;

        [FirestoreProperty("contraseña")]
        public string Contraseña { get; set; } = string.Empty;

        [FirestoreProperty("telefono")]
        public string Telefono { get; set; } = string.Empty;

        [FirestoreProperty("correo")]
        public string Correo { get; set; } = string.Empty;
        [FirestoreProperty("rol")]
        public string Rol { get; set; } = "Corredor";


        [FirestoreProperty("registros")]
        public List<string> Registros { get; set; } = new List<string>();
    }
}