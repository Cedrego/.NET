using Google.Cloud.Firestore;

namespace LaboratorioNET.Entities
{
    [FirestoreData]
    public class Carrera
    {
        [FirestoreDocumentId]
        public string? Id { get; set; }
        
        [FirestoreProperty("IdCarrera")]
        public string IdCarrera { get; set; } = string.Empty;

        [FirestoreProperty("Nombre")]
        public string Nombre { get; set; } = string.Empty;
        
        [FirestoreProperty("FechaInicio")]
        public Timestamp FechaInicio { get; set; }

        [FirestoreProperty("LugarSalida")]
        public string LugarSalida { get; set; } = string.Empty;

        [FirestoreProperty("LimiteParticipantes")]
        public int LimiteParticipantes { get; set; }

        [FirestoreProperty("CantSecciones")]
        public int CantSecciones { get; set; }

        [FirestoreProperty("Tipo")]
        public string Tipo { get; set; } = string.Empty;
    }
}
