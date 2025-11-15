using Microsoft.AspNetCore.Mvc;
using LaboratorioNET.Services;
using LaboratorioNET.Models;
using Google.Cloud.Firestore;
using LaboratorioNET.Entities;

namespace LaboratorioNET.Controllers
{
    /// <summary>
    /// API Controller para recibir datos de sensores en los checkpoints
    /// Endpoint: POST /api/sensor
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class SensorController : ControllerBase
    {
        private readonly FirebaseService _firebaseService;
        private readonly BucketService _bucketService;
        private readonly SensorValidationService _validationService;
        private readonly ILogger<SensorController> _logger;

        public SensorController(
            FirebaseService firebaseService, 
            BucketService bucketService,
            SensorValidationService validationService,
            ILogger<SensorController> logger)
        {
            _firebaseService = firebaseService;
            _bucketService = bucketService;
            _validationService = validationService;
            _logger = logger;
        }

        /// <summary>
        /// Recibe datos de un sensor en un checkpoint
        /// POST: /api/sensor
        /// 
        /// Body JSON:
        /// {
        ///   "corredorId": "12345678",
        ///   "carreraId": "carrera-id",
        ///   "tiempo": "2024-11-12T14:30:45Z"
        /// }
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> ProcesarDatosSensor([FromBody] SensorCheckpointData datos)
        {
            try
            {
                // 1. Validar datos básicos
                var (isValid, errorMessage) = _validationService.ValidarDatosSensor(datos);
                if (!isValid)
                {
                    _logger.LogWarning($"❌ Validación fallida: {errorMessage}");
                    return BadRequest(new 
                    { 
                        error = errorMessage,
                        timestamp = DateTime.UtcNow
                    });
                }

                _logger.LogInformation($"📡 Datos de sensor validados - Corredor: {datos.CorredorId}, Carrera: {datos.CarreraId}");

                // 2. Validar que la carrera exista
                var (carreraExists, carreraError) = await _validationService.ValidarCarreraAsync(datos.CarreraId, _firebaseService);
                if (!carreraExists)
                {
                    return NotFound(new 
                    { 
                        error = carreraError,
                        timestamp = DateTime.UtcNow
                    });
                }

                // 3. Validar que el corredor esté inscrito en la carrera
                var (corredorExists, corredorError) = await _validationService.ValidarCorredorEnCarreraAsync(
                    datos.CarreraId,
                    datos.CorredorId,
                    _firebaseService
                );
                if (!corredorExists)
                {
                    return BadRequest(new 
                    { 
                        error = corredorError,
                        timestamp = DateTime.UtcNow
                    });
                }

                // 4. Guardar en bucket como respaldo
                bool guardadoEnBucket = await _bucketService.GuardarDatosSensorAsync(
                    datos.CarreraId,
                    datos.CorredorId,
                    datos.Tiempo
                );

                if (!guardadoEnBucket)
                {
                    _logger.LogWarning($"⚠️ No se pudo guardar en bucket, pero continuando...");
                }

                // 5. Convertir DateTime a Timestamp de Firestore
                var timestamp = Timestamp.FromDateTime(datos.Tiempo.ToUniversalTime());

                // 6. Agregar el tiempo al registro del corredor
                bool actualizado = await _firebaseService.AgregarTiempoAlRegistroAsync(
                    datos.CarreraId,
                    datos.CorredorId,
                    timestamp
                );

                if (!actualizado)
                {
                    _logger.LogError($"❌ No se pudo actualizar el registro del corredor");
                    return StatusCode(500, new 
                    { 
                        error = "No se pudo actualizar el registro del corredor",
                        timestamp = DateTime.UtcNow
                    });
                }

                _logger.LogInformation($"✅ Registro actualizado correctamente para corredor {datos.CorredorId}");

                // 7. Verificar si la carrera debe marcarse como terminada
                bool carreraTerminada = await _firebaseService.VerificarCarreraTerminadaAsync(datos.CarreraId);

                return Ok(new
                {
                    mensaje = "Datos procesados correctamente",
                    guardadoEnBucket = guardadoEnBucket,
                    registroActualizado = actualizado,
                    carreraTerminada = carreraTerminada,
                    timestamp = timestamp,
                    corredorId = datos.CorredorId,
                    carreraId = datos.CarreraId,
                    procesadoEn = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error al procesar datos de sensor: {ex.Message}\n{ex.StackTrace}");
                return StatusCode(500, new 
                { 
                    error = "Error al procesar datos", 
                    detalle = ex.Message,
                    timestamp = DateTime.UtcNow
                });
            }
        }

        /// <summary>
        /// Obtiene todos los datos guardados en bucket para una carrera
        /// GET: /api/sensor/carrera/{carreraId}
        /// </summary>
        [HttpGet("carrera/{carreraId}")]
        public async Task<IActionResult> ObtenerDatosCarrera(string carreraId)
        {
            try
            {
                if (string.IsNullOrEmpty(carreraId))
                {
                    return BadRequest(new { error = "carreraId es requerido" });
                }

                var datos = await _bucketService.ObtenerDatosSensorPorCarreraAsync(carreraId);

                return Ok(new
                {
                    carreraId = carreraId,
                    cantidadDatos = datos.Count,
                    datos = datos
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error al obtener datos: {ex.Message}");
                return StatusCode(500, new { error = "Error al obtener datos", detalle = ex.Message });
            }
        }

        /// <summary>
        /// Verifica el estado actual de una carrera
        /// GET: /api/sensor/estado-carrera/{carreraId}
        /// </summary>
        [HttpGet("estado-carrera/{carreraId}")]
        public async Task<IActionResult> ObtenerEstadoCarrera(string carreraId)
        {
            try
            {
                if (string.IsNullOrEmpty(carreraId))
                {
                    return BadRequest(new { error = "carreraId es requerido" });
                }

                var carrera = await _firebaseService.ObtenerCarreraPorIdAsync(carreraId);
                if (carrera == null)
                {
                    return NotFound(new { error = "Carrera no encontrada" });
                }

                var registros = await _firebaseService.ObtenerRegistrosPorCarreraAsync(carreraId);

                var info = new
                {
                    carreraId = carreraId,
                    nombreCarrera = carrera.Nombre,
                    terminada = carrera.Terminada,
                    cantidadSecciones = carrera.CantSecciones,
                    corredoresRegistrados = registros.Count,
                    detalleCorredores = registros.Select(r => new
                    {
                        corredorId = r.IdentifiCorredor,
                        dorsal = r.NumDorsal,
                        tiemposRegistrados = r.Tiempos.Count,
                        completado = r.Tiempos.Count == carrera.CantSecciones,
                        tiempos = r.Tiempos.Select(t => t.ToDateTime().ToString("o"))
                    }).ToList()
                };

                return Ok(info);
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error al obtener estado: {ex.Message}");
                return StatusCode(500, new { error = "Error al obtener estado", detalle = ex.Message });
            }
        }

        /// <summary>
        /// Obtiene estadísticas de almacenamiento para una carrera
        /// GET: /api/sensor/estadisticas/{carreraId}
        /// </summary>
        [HttpGet("estadisticas/{carreraId}")]
        public async Task<IActionResult> ObtenerEstadisticas(string carreraId)
        {
            try
            {
                if (string.IsNullOrEmpty(carreraId))
                {
                    return BadRequest(new { error = "carreraId es requerido" });
                }

                var stats = await _bucketService.ObtenerEstadisticasCarreraAsync(carreraId);

                return Ok(new
                {
                    estadisticas = stats,
                    timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error al obtener estadísticas: {ex.Message}");
                return StatusCode(500, new { error = "Error al obtener estadísticas", detalle = ex.Message });
            }
        }

        /// <summary>
        /// Limpia datos antiguos de una carrera
        /// POST: /api/sensor/limpiar/{carreraId}?dias=30
        /// </summary>
        [HttpPost("limpiar/{carreraId}")]
        public async Task<IActionResult> LimpiarDatosAntiguos(string carreraId, [FromQuery] int dias = 30)
        {
            try
            {
                if (string.IsNullOrEmpty(carreraId))
                {
                    return BadRequest(new { error = "carreraId es requerido" });
                }

                if (dias < 1)
                {
                    return BadRequest(new { error = "dias debe ser mayor a 0" });
                }

                int eliminados = await _bucketService.LimpiarDatosAntiguosAsync(carreraId, dias);

                return Ok(new
                {
                    carreraId = carreraId,
                    archivosEliminados = eliminados,
                    diasRetension = dias,
                    mensaje = $"Se eliminaron {eliminados} archivos más antiguos de {dias} días"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error al limpiar datos: {ex.Message}");
                return StatusCode(500, new { error = "Error al limpiar datos", detalle = ex.Message });
            }
        }

        /// <summary>
        /// Obtiene reporte de validación completo para un dato de sensor
        /// POST: /api/sensor/validar
        /// </summary>
        [HttpPost("validar")]
        public async Task<IActionResult> ValidarDatos([FromBody] SensorCheckpointData datos)
        {
            try
            {
                var reporte = await _validationService.GenerarReporteValidacionAsync(datos, _firebaseService);

                return Ok(new
                {
                    reporte = reporte,
                    timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error al generar reporte de validación: {ex.Message}");
                return StatusCode(500, new { error = "Error al validar", detalle = ex.Message });
            }
        }

        /// <summary>
        /// Obtiene reporte de una carrera completamente
        /// GET: /api/sensor/reporte/{carreraId}
        /// </summary>
        [HttpGet("reporte/{carreraId}")]
        public async Task<IActionResult> ObtenerReporteCarrera(string carreraId)
        {
            try
            {
                if (string.IsNullOrEmpty(carreraId))
                {
                    return BadRequest(new { error = "carreraId es requerido" });
                }

                // Obtener info de carrera
                var carrera = await _firebaseService.ObtenerCarreraPorIdAsync(carreraId);
                if (carrera == null)
                {
                    return NotFound(new { error = "Carrera no encontrada" });
                }

                // Obtener registros
                var registros = await _firebaseService.ObtenerRegistrosPorCarreraAsync(carreraId);

                // Obtener estadísticas del bucket
                var estadisticas = await _bucketService.ObtenerEstadisticasCarreraAsync(carreraId);

                // Calcular estadísticas
                var totalTiempos = registros.Sum(r => r.Tiempos.Count);
                var tiemposEsperados = registros.Count * carrera.CantSecciones;
                var porcentajeCompletitud = tiemposEsperados > 0 
                    ? Math.Round((totalTiempos * 100.0) / tiemposEsperados, 2) 
                    : 0;

                var corredoresCompletados = registros.Count(r => r.Tiempos.Count == carrera.CantSecciones);
                var corredoresEnProgreso = registros.Count(r => r.Tiempos.Count > 0 && r.Tiempos.Count < carrera.CantSecciones);
                var corredoresSinIniciar = registros.Count(r => r.Tiempos.Count == 0);

                var reporte = new
                {
                    carreraId = carreraId,
                    nombreCarrera = carrera.Nombre,
                    terminada = carrera.Terminada,
                    cantidadSecciones = carrera.CantSecciones,
                    estadisticasGenerales = new
                    {
                        corredoresRegistrados = registros.Count,
                        corredoresCompletados = corredoresCompletados,
                        corredoresEnProgreso = corredoresEnProgreso,
                        corredoresSinIniciar = corredoresSinIniciar,
                        totalTiemposRegistrados = totalTiempos,
                        tiemposEsperados = tiemposEsperados,
                        porcentajeCompletitud = porcentajeCompletitud
                    },
                    estadisticasBucket = estadisticas,
                    detalleCorredores = registros.Select(r => new
                    {
                        corredorId = r.IdentifiCorredor,
                        dorsal = r.NumDorsal,
                        tiemposRegistrados = r.Tiempos.Count,
                        completado = r.Tiempos.Count == carrera.CantSecciones,
                        ultimoTiempo = r.Tiempos.Any() ? r.Tiempos.Last().ToDateTime() : (DateTime?)null
                    }).ToList(),
                    generadoEn = DateTime.UtcNow
                };

                return Ok(reporte);
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error al obtener reporte: {ex.Message}");
                return StatusCode(500, new { error = "Error al obtener reporte", detalle = ex.Message });
            }
        }
    }
}
