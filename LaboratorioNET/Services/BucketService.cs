using Google.Cloud.Storage.V1;
using System.Text.Json;

namespace LaboratorioNET.Services
{

    public class BucketService
    {
        private readonly StorageClient _storageClient;
        private readonly string _bucketName;
        private readonly ILogger<BucketService> _logger;

        public BucketService(ILogger<BucketService> logger)
        {
            _logger = logger;
            
            // Obtener el nombre del bucket de variables de entorno o configuración
            _bucketName = Environment.GetEnvironmentVariable("GCS_BUCKET_NAME") 
                ?? "laboratorionet-sensor-data";
            
            try
            {
                // Usa las credenciales de Firebase configuradas
                _storageClient = StorageClient.Create();
                _logger.LogInformation($"✅ Bucket Service inicializado. Bucket: {_bucketName}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error al inicializar Bucket Service: {ex.Message}");
                throw;
            }
        }

        public async Task<bool> GuardarDatosSensorAsync(string carreraId, string corredorId, DateTime tiempo)
        {
            try
            {
                var data = new
                {
                    carreraId,
                    corredorId,
                    tiempo = tiempo.ToUniversalTime(),
                    registradoEn = DateTime.UtcNow
                };

                var jsonContent = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
                
                // Crear ruta: sensor_data/{carreraId}/{timestamp}_{corredorId}.json
                string timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss_fff");
                string objectName = $"sensor_data/{carreraId}/{timestamp}_{corredorId}.json";

                await _storageClient.UploadObjectAsync(
                    _bucketName,
                    objectName,
                    "application/json",
                    new MemoryStream(System.Text.Encoding.UTF8.GetBytes(jsonContent))
                );

                _logger.LogInformation($"✅ Datos de sensor guardados en bucket: {objectName}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error al guardar datos en bucket: {ex.Message}");
                return false;
            }
        }

        public async Task<List<string>> ObtenerDatosSensorPorCarreraAsync(string carreraId)
        {
            try
            {
                var datos = new List<string>();
                var objects = _storageClient.ListObjectsAsync(_bucketName, $"sensor_data/{carreraId}/");

                await foreach (var obj in objects)
                {
                    using (var stream = new MemoryStream())
                    {
                        await _storageClient.DownloadObjectAsync(_bucketName, obj.Name, stream);
                        stream.Position = 0;
                        using (var reader = new StreamReader(stream))
                        {
                            datos.Add(await reader.ReadToEndAsync());
                        }
                    }
                }

                return datos;
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error al obtener datos del bucket: {ex.Message}");
                return new List<string>();
            }
        }

        public async Task<bool> EliminarDatosSensorAsync(string carreraId, string timestamp, string corredorId)
        {
            try
            {
                string objectName = $"sensor_data/{carreraId}/{timestamp}_{corredorId}.json";
                await _storageClient.DeleteObjectAsync(_bucketName, objectName);
                
                _logger.LogInformation($"✅ Datos de sensor eliminados del bucket: {objectName}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error al eliminar datos del bucket: {ex.Message}");
                return false;
            }
        }

        public async Task<Dictionary<string, object>> ObtenerEstadisticasCarreraAsync(string carreraId)
        {
            try
            {
                var stats = new Dictionary<string, object>();
                var objects = _storageClient.ListObjectsAsync(_bucketName, $"sensor_data/{carreraId}/");
                
                long totalBytes = 0;
                int totalArchivos = 0;
                var corredoresUnicos = new HashSet<string>();
                DateTime? archivoMasAntiguo = null;
                DateTime? archivoMasReciente = null;

                await foreach (var obj in objects)
                {

                    totalBytes += (long)(obj.Size ?? 0UL);
                    totalArchivos++;
                    
                    // Extraer ID del corredor del nombre del archivo
                    var partes = obj.Name.Split('_');
                    if (partes.Length > 0)
                    {
                        corredoresUnicos.Add(partes.Last().Replace(".json", ""));
                    }

                    // Rastrear fechas
                    if (obj.Updated.HasValue)
                    {
                        if (archivoMasAntiguo == null || obj.Updated < archivoMasAntiguo)
                            archivoMasAntiguo = obj.Updated;
                        if (archivoMasReciente == null || obj.Updated > archivoMasReciente)
                            archivoMasReciente = obj.Updated;
                    }
                }

                stats["carreraId"] = carreraId;
                stats["totalArchivos"] = totalArchivos;
                stats["totalBytes"] = totalBytes;
                stats["totalMB"] = Math.Round(totalBytes / (1024.0 * 1024.0), 2);
                stats["corredoresUnicos"] = corredoresUnicos.Count;
                stats["archivoMasAntiguo"] = (object?)archivoMasAntiguo;
                stats["archivoMasReciente"] = (object?)archivoMasReciente;
                stats["tiempoPromedioPorArchivo"] = totalArchivos > 0 ? totalBytes / totalArchivos : 0;

                _logger.LogInformation($"📊 Estadísticas de {carreraId}: {totalArchivos} archivos, {stats["totalMB"]} MB");
                return stats;
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error al obtener estadísticas: {ex.Message}");
                return new Dictionary<string, object> { { "error", ex.Message } };
            }
        }

        /// <summary>
        /// Limpia datos antiguos de una carrera (más viejos que X días)
        /// </summary>
        public async Task<int> LimpiarDatosAntiguosAsync(string carreraId, int diasRetension = 30)
        {
            try
            {
                var objects = _storageClient.ListObjectsAsync(_bucketName, $"sensor_data/{carreraId}/");
                int eliminados = 0;
                var fechaLimite = DateTime.UtcNow.AddDays(-diasRetension);

                await foreach (var obj in objects)
                {
                    if (obj.Updated.HasValue && obj.Updated < fechaLimite)
                    {
                        await _storageClient.DeleteObjectAsync(_bucketName, obj.Name);
                        eliminados++;
                    }
                }

                _logger.LogInformation($"🧹 Limpieza completada: {eliminados} archivos eliminados de {carreraId}");
                return eliminados;
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error al limpiar datos antiguos: {ex.Message}");
                return 0;
            }
        }
    }
}
