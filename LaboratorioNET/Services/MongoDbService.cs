using MongoDB.Driver;
using MongoDB.Bson;
using LaboratorioNET.Models;
using LaboratorioNET.Entities;
using Microsoft.Extensions.Options;

namespace LaboratorioNET.Services
{
    public class MongoDbService
    {
        private readonly IMongoDatabase _database;
        private readonly IMongoCollection<Corredor> _corredores;
        private readonly IMongoCollection<Carrera> _carreras;
        private readonly IMongoCollection<Admin> _admins;
        private readonly IMongoCollection<Registro> _registros;

        public MongoDbService(IOptions<MongoDbSettings> settings)
        {
            try
            {
                var mongoSettings = settings.Value;
                
                Console.WriteLine($"🔄 Conectando a MongoDB: {mongoSettings.ConnectionString}");
                
                var client = new MongoClient(mongoSettings.ConnectionString);
                _database = client.GetDatabase(mongoSettings.DatabaseName);

                // Inicializar colecciones
                _corredores = _database.GetCollection<Corredor>(mongoSettings.Collections.Corredores);
                _carreras = _database.GetCollection<Carrera>(mongoSettings.Collections.Carreras);
                _admins = _database.GetCollection<Admin>(mongoSettings.Collections.Admins);
                _registros = _database.GetCollection<Registro>(mongoSettings.Collections.Registros);

                Console.WriteLine($"✅ MongoDB inicializado correctamente");
                Console.WriteLine($"   Base de datos: {mongoSettings.DatabaseName}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error al inicializar MongoDB: {ex.Message}");
                throw;
            }
        }

        // Acceso directo a colecciones
        public IMongoCollection<Corredor> Corredores => _corredores;
        public IMongoCollection<Carrera> Carreras => _carreras;
        public IMongoCollection<Admin> Admins => _admins;
        public IMongoCollection<Registro> Registros => _registros;
        public IMongoDatabase Database => _database;

        // ===== MÉTODOS PARA CORREDORES =====

        public async Task<List<Corredor>> ObtenerTodosCorredoresAsync()
        {
            try
            {
                return await _corredores.Find(_ => true).ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al obtener corredores: {ex.Message}");
                return new List<Corredor>();
            }
        }

        public async Task<Corredor?> ObtenerCorredorPorIdAsync(string id)
        {
            try
            {
                return await _corredores.Find(c => c.Id == id).FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al obtener corredor por ID: {ex.Message}");
                return null;
            }
        }

        public async Task<Corredor?> ObtenerCorredorPorDocumentoAsync(string documentoIdentidad)
        {
            try
            {
                return await _corredores
                    .Find(c => c.DocumentoIdentidad == documentoIdentidad)
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al obtener corredor: {ex.Message}");
                return null;
            }
        }

        public async Task<Corredor?> ObtenerCorredorPorNombreAsync(string nombre)
        {
            try
            {
                Console.WriteLine($"Buscando corredor con nombre: '{nombre}'");
                var corredor = await _corredores
                    .Find(c => c.Nombre == nombre)
                    .FirstOrDefaultAsync();

                if (corredor != null)
                {
                    Console.WriteLine($"Corredor encontrado: {corredor.Nombre}");
                }
                else
                {
                    Console.WriteLine("No se encontraron corredores con ese nombre");
                }

                return corredor;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al obtener corredor: {ex.Message}");
                return null;
            }
        }

        public async Task<Corredor?> ObtenerCorredorPorCorreoAsync(string correo)
        {
            try
            {
                return await _corredores
                    .Find(c => c.Correo == correo)
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al obtener corredor por correo: {ex.Message}");
                return null;
            }
        }

        public async Task<bool> ValidarCredencialesCorredorAsync(string correo, string contraseña)
        {
            try
            {
                var corredor = await ObtenerCorredorPorCorreoAsync(correo);
                return corredor != null && corredor.Contraseña == contraseña;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al validar credenciales: {ex.Message}");
                return false;
            }
        }

        public async Task<List<Corredor>> ObtenerCorredoresFiltradosAsync(string? carreraId = null, string? nacionalidad = null)
        {
            try
            {
                var filterBuilder = Builders<Corredor>.Filter;
                var filter = filterBuilder.Empty;

                // Filtrar por nacionalidad si se especifica
                if (!string.IsNullOrEmpty(nacionalidad))
                {
                    filter = filterBuilder.And(filter, filterBuilder.Eq(c => c.Nacionalidad, nacionalidad));
                }

                var corredores = await _corredores.Find(filter).ToListAsync();

                // Filtrar por carrera si se especifica
                if (!string.IsNullOrEmpty(carreraId))
                {
                    var registros = await _registros
                        .Find(r => r.IDCarrera == carreraId)
                        .ToListAsync();

                    var documentosEnCarrera = registros
                        .Select(r => r.IdentifiCorredor)
                        .ToHashSet();

                    corredores = corredores
                        .Where(c => documentosEnCarrera.Contains(c.DocumentoIdentidad))
                        .ToList();
                }

                return corredores;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al obtener corredores filtrados: {ex.Message}");
                return new List<Corredor>();
            }
        }

        public async Task<List<string>> ObtenerNacionalidadesUnicasAsync()
        {
            try
            {
                var nacionalidades = await _corredores
                    .Distinct<string>("nacionalidad", Builders<Corredor>.Filter.Empty)
                    .ToListAsync();

                return nacionalidades
                    .Where(n => !string.IsNullOrWhiteSpace(n))
                    .OrderBy(n => n)
                    .ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al obtener nacionalidades: {ex.Message}");
                return new List<string>();
            }
        }

        public async Task<bool> AgregarCorredorAsync(Corredor corredor)
        {
            try
            {
                // Verificar si ya existe
                var existente = await ObtenerCorredorPorDocumentoAsync(corredor.DocumentoIdentidad);
                if (existente != null)
                {
                    Console.WriteLine("Ya existe un corredor con ese documento de identidad");
                    return false;
                }

                await _corredores.InsertOneAsync(corredor);
                Console.WriteLine($"✅ Corredor agregado: {corredor.Nombre}");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al agregar corredor: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> ActualizarCorredorAsync(string id, Corredor corredor)
        {
            try
            {
                corredor.Id = id;
                var result = await _corredores.ReplaceOneAsync(c => c.Id == id, corredor);
                return result.ModifiedCount > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al actualizar corredor: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> EliminarCorredorAsync(string id)
        {
            try
            {
                var result = await _corredores.DeleteOneAsync(c => c.Id == id);
                return result.DeletedCount > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al eliminar corredor: {ex.Message}");
                return false;
            }
        }

        // ===== MÉTODOS PARA CARRERAS =====

        public async Task<List<Carrera>> ObtenerTodasCarrerasAsync()
        {
            try
            {
                return await _carreras.Find(_ => true).ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al obtener carreras: {ex.Message}");
                return new List<Carrera>();
            }
        }

        public async Task<Carrera?> ObtenerCarreraPorIdAsync(string id)
        {
            try
            {
                return await _carreras.Find(c => c.Id == id).FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al obtener carrera: {ex.Message}");
                return null;
            }
        }

        public async Task<Carrera?> ObtenerCarreraPorIdCarreraAsync(string idCarrera)
        {
            try
            {
                return await _carreras
                    .Find(c => c.IdCarrera == idCarrera)
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al obtener carrera: {ex.Message}");
                return null;
            }
        }

        public async Task<List<Carrera>> ObtenerCarrerasActivasAsync()
        {
            try
            {
                return await _carreras
                    .Find(c => !c.Terminada)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al obtener carreras activas: {ex.Message}");
                return new List<Carrera>();
            }
        }

        public async Task<List<Carrera>> ObtenerCarrerasPorTipoAsync(string tipo)
        {
            try
            {
                return await _carreras
                    .Find(c => c.Tipo == tipo)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al obtener carreras por tipo: {ex.Message}");
                return new List<Carrera>();
            }
        }

        public async Task<bool> AgregarCarreraAsync(Carrera carrera)
        {
            try
            {
                await _carreras.InsertOneAsync(carrera);
                Console.WriteLine($"✅ Carrera agregada: {carrera.Nombre}");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al agregar carrera: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> ActualizarCarreraAsync(string id, Carrera carrera)
        {
            try
            {
                carrera.Id = id;
                var result = await _carreras.ReplaceOneAsync(c => c.Id == id, carrera);
                return result.ModifiedCount > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al actualizar carrera: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> EliminarCarreraAsync(string id)
        {
            try
            {
                var result = await _carreras.DeleteOneAsync(c => c.Id == id);
                return result.DeletedCount > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al eliminar carrera: {ex.Message}");
                return false;
            }
        }

        // ===== MÉTODOS PARA REGISTROS =====

        public async Task<List<Registro>> ObtenerRegistrosPorCarreraAsync(string idCarrera)
        {
            try
            {
                return await _registros
                    .Find(r => r.IDCarrera == idCarrera)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al obtener registros: {ex.Message}");
                return new List<Registro>();
            }
        }

        public async Task<Registro?> ObtenerRegistroPorIdAsync(string id)
        {
            try
            {
                return await _registros.Find(r => r.Id == id).FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al obtener registro: {ex.Message}");
                return null;
            }
        }

        public async Task<Registro?> ObtenerRegistroCorredorEnCarreraAsync(string idCarrera, string documentoCorredor)
        {
            try
            {
                return await _registros
                    .Find(r => r.IDCarrera == idCarrera && r.IdentifiCorredor == documentoCorredor)
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al obtener registro: {ex.Message}");
                return null;
            }
        }

        public async Task<List<Registro>> ObtenerRegistrosPorCorredorAsync(string documentoCorredor)
        {
            try
            {
                return await _registros
                    .Find(r => r.IdentifiCorredor == documentoCorredor)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al obtener registros del corredor: {ex.Message}");
                return new List<Registro>();
            }
        }

        public async Task<bool> AgregarRegistroAsync(Registro registro)
        {
            try
            {
                await _registros.InsertOneAsync(registro);
                Console.WriteLine($"✅ Registro agregado");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al agregar registro: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> ActualizarRegistroAsync(string id, Registro registro)
        {
            try
            {
                registro.Id = id;
                var result = await _registros.ReplaceOneAsync(r => r.Id == id, registro);
                return result.ModifiedCount > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al actualizar registro: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> EliminarRegistroAsync(string id)
        {
            try
            {
                var result = await _registros.DeleteOneAsync(r => r.Id == id);
                return result.DeletedCount > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al eliminar registro: {ex.Message}");
                return false;
            }
        }

        // ===== MÉTODOS DE UTILIDAD =====

        public async Task<int> ContarCorredoresEnCarreraAsync(string idCarrera)
        {
            try
            {
                var count = await _registros.CountDocumentsAsync(r => r.IDCarrera == idCarrera);
                return (int)count;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al contar corredores: {ex.Message}");
                return 0;
            }
        }

        public async Task<bool> AgregarTiempoAlRegistroAsync(string idCarrera, string documentoCorredor, DateTime tiempo)
        {
            try
            {
                var registro = await ObtenerRegistroCorredorEnCarreraAsync(idCarrera, documentoCorredor);

                if (registro == null)
                {
                    Console.WriteLine($"No se encontró registro para corredor {documentoCorredor} en carrera {idCarrera}");
                    return false;
                }

                registro.Tiempos.Add(tiempo);

                var update = Builders<Registro>.Update
                    .Set(r => r.Tiempos, registro.Tiempos);

                var result = await _registros.UpdateOneAsync(
                    r => r.Id == registro.Id,
                    update
                );

                Console.WriteLine($"✅ Tiempo agregado al registro");
                return result.ModifiedCount > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error al agregar tiempo: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> ActualizarEstadoCarreraAsync(string idCarrera, bool terminada)
        {
            try
            {
                var update = Builders<Carrera>.Update
                    .Set(c => c.Terminada, terminada);

                var result = await _carreras.UpdateOneAsync(
                    c => c.IdCarrera == idCarrera,
                    update
                );

                Console.WriteLine($"✅ Estado de carrera actualizado - Terminada: {terminada}");
                return result.ModifiedCount > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error al actualizar carrera: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> VerificarCarreraTerminadaAsync(string idCarrera)
        {
            try
            {
                var carrera = await ObtenerCarreraPorIdCarreraAsync(idCarrera);
                if (carrera == null)
                {
                    Console.WriteLine($"Carrera no encontrada: {idCarrera}");
                    return false;
                }

                var registros = await ObtenerRegistrosPorCarreraAsync(idCarrera);

                if (!registros.Any())
                {
                    Console.WriteLine("No hay registros para esta carrera");
                    return false;
                }

                bool todosCompletos = registros.All(r => r.Tiempos.Count == carrera.CantSecciones);

                if (todosCompletos && !carrera.Terminada)
                {
                    await ActualizarEstadoCarreraAsync(idCarrera, true);
                }

                return todosCompletos;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error al verificar estado de carrera: {ex.Message}");
                return false;
            }
        }

        // ===== MÉTODOS PARA ADMINS =====

        public async Task<Admin?> ObtenerAdminPorIdAsync(string idAdmin)
        {
            try
            {
                return await _admins
                    .Find(a => a.IDAdmin == idAdmin)
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al obtener admin: {ex.Message}");
                return null;
            }
        }

        public async Task<bool> ValidarCredencialesAdminAsync(string idAdmin, string contraseña)
        {
            try
            {
                var admin = await ObtenerAdminPorIdAsync(idAdmin);
                return admin != null && admin.Contrasena == contraseña;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al validar credenciales admin: {ex.Message}");
                return false;
            }
        }

        public async Task<List<Admin>> ObtenerTodosAdminsAsync()
        {
            try
            {
                return await _admins.Find(_ => true).ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al obtener admins: {ex.Message}");
                return new List<Admin>();
            }
        }

        public async Task<bool> AgregarAdminAsync(Admin admin)
        {
            try
            {
                await _admins.InsertOneAsync(admin);
                Console.WriteLine($"✅ Admin agregado: {admin.IDAdmin}");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al agregar admin: {ex.Message}");
                return false;
            }
        }
    }
}