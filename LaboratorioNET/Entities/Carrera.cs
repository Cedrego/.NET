using Google.Cloud.Firestore;

namespace LaboratorioNET.Entities
{
    [FirestoreData]
    public class Carrera
    {
        [FirestoreDocumentId]
        public string? Id { get; set; }
        
        [FirestoreProperty("IdCarrera")]
        public int IdCarrera { get; set; }

        [FirestoreProperty("Nombre")]
        public string Nombre { get; set; } = string.Empty;
        
        [FirestoreProperty("FechaInicio")]
        public DateTime FechaInicio { get; set; }

        [FirestoreProperty("LugarSalida")]
        public string LugarSalida { get; set; } = string.Empty;
 
        [FirestoreProperty("FechaInicioInsc")]
        public DateTime FechaInicioInsc { get; set; }

        [FirestoreProperty("LimiteParticipantes")]
        public int LimiteParticipantes { get; set; }

        [FirestoreProperty("CantSecciones")]
        public double CantSecciones { get; set; }

    }
}