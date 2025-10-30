using Google.Cloud.Firestore;

namespace LaboratorioNET.Entities
{
    [FirestoreData]
    public class Carrera
    {
        [FirestoreProperty("id")]
        public string? Id { get; set; }

        [FirestoreProperty("idCarrera")]
        public string IdCarrera { get; set; } = string.Empty;

        [FirestoreProperty("nombre")]
        public string Nombre { get; set; } = string.Empty;

        [FirestoreProperty("fechaInicio")]
        public Timestamp FechaInicio { get; set; }

        [FirestoreProperty("lugarSalida")]
        public string LugarSalida { get; set; } = string.Empty;

        [FirestoreProperty("fechaInicioInsc")]
        public Timestamp FechaInicioInsc { get; set; }

        [FirestoreProperty("limiteParticipantes")]
        public int LimiteParticipantes { get; set; }

        [FirestoreProperty("cantSecciones")]
        public int CantSecciones { get; set; }

        [FirestoreProperty("tipo")]
        public string Tipo { get; set; } = string.Empty;
    }
}