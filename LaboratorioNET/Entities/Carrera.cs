using Google.Cloud.Firestore;

namespace LaboratorioNET.Entities
{
    [FirestoreData]
    public class Carrera
    {
        [FirestoreDocumentId]
        public string? Id { get; set; }
        
        [FirestoreProperty("idCarrera")]
        public int IdCarrera { get; set; }

        [FirestoreProperty("nombre")]
        public string Nombre { get; set; } = string.Empty;
        
        [FirestoreProperty("fecha")]
        public DateTime Fecha { get; set; }

        [FirestoreProperty("lugarSalida")]
        public string LugarSalida { get; set; } = string.Empty;

        [FirestoreProperty("fechaInicioInsc")]
        public DateTime FechaInicioInsc { get; set; }

        [FirestoreProperty("cupoParticipantes")]
        public int CupoParticipantes { get; set; }

        [FirestoreProperty("distanciaOCheckpoint")]
        public double DistanciaOCheckpoint { get; set; }

        [FirestoreProperty("seccTorres")]
        public int SeccTorres { get; set; }
    }
}