namespace LaboratorioNET.Models
{
    /// <summary>
    /// Modelo para recibir datos de sensores en los checkpoints
    /// Ejemplo JSON: { "corredorId": "12345678", "carreraId": "carrera1", "tiempo": "2024-11-12T14:30:45Z" }
    /// </summary>
    public class SensorCheckpointData
    {
        /// <summary>
        /// Identificación del corredor (documento de identidad)
        /// </summary>
        public string CorredorId { get; set; } = string.Empty;

        /// <summary>
        /// Identificación de la carrera
        /// </summary>
        public string CarreraId { get; set; } = string.Empty;

        /// <summary>
        /// Tiempo en que el corredor pasó por el checkpoint
        /// </summary>
        public DateTime Tiempo { get; set; }

        /// <summary>
        /// Número del checkpoint/sección (opcional, puede inferirse de la cantidad de tiempos)
        /// </summary>
        public int? NumeroCheckpoint { get; set; }
    }
}
