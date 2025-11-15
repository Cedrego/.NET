using LaboratorioNET.Models;
using LaboratorioNET.Entities;

namespace LaboratorioNET.Services
{
    /// <summary>
    /// Servicio para validación de datos de sensores
    /// Previene datos inválidos o inconsistentes
    /// </summary>
    public class SensorValidationService
    {
        private readonly ILogger<SensorValidationService> _logger;

        public SensorValidationService(ILogger<SensorValidationService> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Valida los datos recibidos del sensor
        /// </summary>
        public (bool isValid, string? errorMessage) ValidarDatosSensor(SensorCheckpointData datos)
        {
            // Validar nulos
            if (datos == null)
            {
                _logger.LogWarning("❌ Datos de sensor nulos");
                return (false, "Los datos del sensor no pueden ser nulos");
            }

            // Validar CorredorId
            if (string.IsNullOrWhiteSpace(datos.CorredorId))
            {
                _logger.LogWarning("❌ CorredorId vacío");
                return (false, "CorredorId es requerido y no puede estar vacío");
            }

            if (datos.CorredorId.Length > 50)
            {
                _logger.LogWarning("❌ CorredorId muy largo");
                return (false, "CorredorId no puede exceder 50 caracteres");
            }

            // Validar CarreraId
            if (string.IsNullOrWhiteSpace(datos.CarreraId))
            {
                _logger.LogWarning("❌ CarreraId vacío");
                return (false, "CarreraId es requerido y no puede estar vacío");
            }

            if (datos.CarreraId.Length > 50)
            {
                _logger.LogWarning("❌ CarreraId muy largo");
                return (false, "CarreraId no puede exceder 50 caracteres");
            }

            // Validar Tiempo
            if (datos.Tiempo == default(DateTime))
            {
                _logger.LogWarning("❌ Tiempo por defecto");
                return (false, "El tiempo no puede estar vacío");
            }

            // Validar que el tiempo no sea en el futuro (con tolerancia de 1 minuto)
            if (datos.Tiempo > DateTime.UtcNow.AddMinutes(1))
            {
                _logger.LogWarning($"❌ Tiempo en el futuro: {datos.Tiempo}");
                return (false, "El tiempo no puede estar en el futuro");
            }

            // Validar que el tiempo no sea muy antiguo (más de 1 año)
            if (datos.Tiempo < DateTime.UtcNow.AddDays(-365))
            {
                _logger.LogWarning($"❌ Tiempo muy antiguo: {datos.Tiempo}");
                return (false, "El tiempo no puede ser más antiguo de 1 año");
            }

            // Validar NumeroCheckpoint si se proporciona
            if (datos.NumeroCheckpoint.HasValue && datos.NumeroCheckpoint.Value < 1)
            {
                _logger.LogWarning("❌ NumeroCheckpoint inválido");
                return (false, "NumeroCheckpoint debe ser mayor a 0");
            }

            return (true, null);
        }

        /// <summary>
        /// Valida la existencia del corredor en la carrera
        /// </summary>
        public async Task<(bool exists, string? errorMessage)> ValidarCorredorEnCarreraAsync(
            string carreraId, 
            string documentoCorredor,
            MongoDbService mongoDbService)
        {
            try
            {
                var registro = await mongoDbService.ObtenerRegistroCorredorEnCarreraAsync(carreraId, documentoCorredor);
                
                if (registro == null)
                {
                    _logger.LogWarning($"❌ Corredor {documentoCorredor} no inscrito en carrera {carreraId}");
                    return (false, "El corredor no está inscrito en esta carrera");
                }

                return (true, null);
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error validando corredor: {ex.Message}");
                return (false, $"Error al validar corredor: {ex.Message}");
            }
        }

        /// <summary>
        /// Valida la existencia de la carrera
        /// </summary>
        public async Task<(bool exists, string? errorMessage)> ValidarCarreraAsync(
            string carreraId,
            MongoDbService mongoDbService)
        {
            try
            {
                var carrera = await mongoDbService.ObtenerCarreraPorIdAsync(carreraId);
                
                if (carrera == null)
                {
                    _logger.LogWarning($"❌ Carrera no encontrada: {carreraId}");
                    return (false, "La carrera no existe");
                }

                return (true, null);
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error validando carrera: {ex.Message}");
                return (false, $"Error al validar carrera: {ex.Message}");
            }
        }

        /// <summary>
        /// Detecta datos duplicados o sospechosos
        /// </summary>
        public (bool isSuspicious, string? warning) DetectarDatosSospechosos(
            SensorCheckpointData datos,
            List<SensorCheckpointData>? datosRecientes = null)
        {
            // Si tenemos datos recientes, buscar duplicados
            if (datosRecientes != null && datosRecientes.Any())
            {
                var duplicado = datosRecientes.FirstOrDefault(d =>
                    d.CorredorId == datos.CorredorId &&
                    d.CarreraId == datos.CarreraId &&
                    Math.Abs((d.Tiempo - datos.Tiempo).TotalSeconds) < 5
                );

                if (duplicado != null)
                {
                    _logger.LogWarning($"⚠️ Posible dato duplicado: {datos.CorredorId} en {datos.CarreraId}");
                    return (true, "Este dato es muy similar a uno reciente (posible duplicado)");
                }
            }

            // Advertencia si el checkpoint es muy rápido o muy lento
            if (datos.NumeroCheckpoint.HasValue && datos.NumeroCheckpoint > 1)
            {
                // Esto es una verificación simplista, en producción deberías
                // comparar con tiempos históricos de la carrera
                _logger.LogInformation($"ℹ️ Checkpoint {datos.NumeroCheckpoint} registrado para {datos.CorredorId}");
            }

            return (false, null);
        }

        /// <summary>
        /// Genera un reporte de validación completo
        /// </summary>
        public async Task<Dictionary<string, object>> GenerarReporteValidacionAsync(
            SensorCheckpointData datos,
            MongoDbService mongoDbService)
        {
            var reporte = new Dictionary<string, object>();

            // Validaciones básicas
            var (basicValid, basicError) = ValidarDatosSensor(datos);
            reporte["validacionBasica"] = new
            {
                valido = basicValid,
                error = basicError
            };

            if (!basicValid)
            {
                reporte["estado"] = "FALLIDO - Validación básica";
                return reporte;
            }

            // Validar carrera
            var (carreraValid, carreraError) = await ValidarCarreraAsync(datos.CarreraId, mongoDbService);
            reporte["validacionCarrera"] = new
            {
                valido = carreraValid,
                error = carreraError
            };

            if (!carreraValid)
            {
                reporte["estado"] = "FALLIDO - Carrera no existe";
                return reporte;
            }

            // Validar corredor en carrera
            var (corredorValid, corredorError) = await ValidarCorredorEnCarreraAsync(
                datos.CarreraId,
                datos.CorredorId,
                mongoDbService
            );
            reporte["validacionCorredor"] = new
            {
                valido = corredorValid,
                error = corredorError
            };

            if (!corredorValid)
            {
                reporte["estado"] = "FALLIDO - Corredor no inscrito";
                return reporte;
            }

            // Detectar sospechosos
            var (suspicious, warning) = DetectarDatosSospechosos(datos);
            reporte["deteccionAnomalias"] = new
            {
                sospechoso = suspicious,
                advertencia = warning
            };

            reporte["estado"] = "EXITOSO";
            reporte["timestamp"] = DateTime.UtcNow;

            return reporte;
        }
    }
}
