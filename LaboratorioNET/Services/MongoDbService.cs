using MongoDB.Driver;
using MongoDB.Bson;
using LaboratorioNET.Models;
using LaboratorioNET.Entities;
using Microsoft.Extensions.Options;
using BCrypt.Net;

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
                return corredor != null && BCrypt.Net.BCrypt.Verify(contraseña, corredor.Contraseña);
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

                // Hashear la contraseña antes de guardar
                corredor.Contraseña = BCrypt.Net.BCrypt.HashPassword(corredor.Contraseña);

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

                // Obtener la carrera para verificar el máximo de secciones
                var carrera = await ObtenerCarreraPorIdCarreraAsync(idCarrera);
                if (carrera == null)
                {
                    Console.WriteLine($"No se encontró carrera {idCarrera}");
                    return false;
                }

                // Validar que no haya alcanzado el máximo de tiempos
                if (registro.Tiempos.Count >= carrera.CantSecciones)
                {
                    Console.WriteLine($"⚠️ Corredor {documentoCorredor} ya completó todos los {carrera.CantSecciones} checkpoints");
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

        public async Task<bool> ResetearRegistrosCarreraAsync(string idCarrera)
        {
            try
            {
                var registros = await ObtenerRegistrosPorCarreraAsync(idCarrera);

                if (!registros.Any())
                {
                    Console.WriteLine($"No hay registros para resetear en carrera: {idCarrera}");
                    return true;
                }

                foreach (var registro in registros)
                {
                    var update = Builders<Registro>.Update
                        .Set(r => r.Tiempos, new List<DateTime>());

                    await _registros.UpdateOneAsync(
                        r => r.Id == registro.Id,
                        update
                    );
                }

                // Resetear también el flag Terminada de la carrera
                await ActualizarEstadoCarreraAsync(idCarrera, false);

                Console.WriteLine($"✅ Todos los registros de la carrera {idCarrera} han sido reseteados");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error al resetear registros: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> ActualizarCarreraAIniciada(string idCarrera)
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

                // Si algún corredor tiene tiempos, la carrera está iniciada
                bool alguienTienetiempos = registros.Any(r => r.Tiempos.Any());

                if (alguienTienetiempos)
                {
                    var update = Builders<Carrera>.Update
                        .Set(c => c.FechaInicio, DateTime.UtcNow);

                    var result = await _carreras.UpdateOneAsync(
                        c => c.IdCarrera == idCarrera,
                        update
                    );

                    if (result.ModifiedCount > 0)
                    {
                        Console.WriteLine($"✅ Carrera {idCarrera} ha sido marcada como iniciada (FechaInicio actualizada)");
                    }

                    return result.ModifiedCount > 0;
                }

                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error al actualizar carrera a iniciada: {ex.Message}");
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
                return admin != null && BCrypt.Net.BCrypt.Verify(contraseña, admin.Contrasena);
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
                // Hashear la contraseña antes de guardar
                admin.Contrasena = BCrypt.Net.BCrypt.HashPassword(admin.Contrasena);
                
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

        // Método para agregar corredores de prueba
        // Devuelve la lista de documentosIdentidad de los corredores (inser­tados o ya existentes)
        public async Task<List<string>> AgregarCorredoresPruebaAsync()
        {
            try
            {
                var nuevosCorredores = new List<Corredor>
                {
                    new Corredor
                    {
                        DocumentoIdentidad = "98765432",
                        Nacionalidad = "Colombia",
                        FechaNacimiento = "1995-05-10",
                        Nombre = "Carlos Rodríguez",
                        Contraseña = "pass123",
                        Telefono = "3105551234",
                        Correo = "carlos@example.com",
                        Rol = "Corredor",
                        Registros = new List<string>()
                    },
                    new Corredor
                    {
                        DocumentoIdentidad = "87654321",
                        Nacionalidad = "Colombia",
                        FechaNacimiento = "1998-08-22",
                        Nombre = "Sofía Martínez",
                        Contraseña = "pass123",
                        Telefono = "3105555678",
                        Correo = "sofia@example.com",
                        Rol = "Corredor",
                        Registros = new List<string>()
                    },
                    new Corredor
                    {
                        DocumentoIdentidad = "76543210",
                        Nacionalidad = "Colombia",
                        FechaNacimiento = "1992-03-15",
                        Nombre = "Andrés López",
                        Contraseña = "pass123",
                        Telefono = "3105559012",
                        Correo = "andres@example.com",
                        Rol = "Corredor",
                        Registros = new List<string>()
                    },
                    new Corredor
                    {
                        DocumentoIdentidad = "65432109",
                        Nacionalidad = "Colombia",
                        FechaNacimiento = "1996-11-30",
                        Nombre = "Laura Gómez",
                        Contraseña = "pass123",
                        Telefono = "3105553456",
                        Correo = "laura@example.com",
                        Rol = "Corredor",
                        Registros = new List<string>()
                    }
                };

                var documentos = new List<string>();

                // Agregar cada corredor (si no existe) y recopilar su documento
                foreach (var corredor in nuevosCorredores)
                {
                    var existe = await _corredores.Find(c => c.DocumentoIdentidad == corredor.DocumentoIdentidad).FirstOrDefaultAsync();
                    if (existe == null)
                    {
                        await AgregarCorredorAsync(corredor);
                        Console.WriteLine($"✅ Corredor de prueba agregado: {corredor.Nombre}");
                    }
                    else
                    {
                        Console.WriteLine($"⚠️ Corredor ya existe: {corredor.Nombre}");
                    }

                    documentos.Add(corredor.DocumentoIdentidad);
                }

                return documentos;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al agregar corredores de prueba: {ex.Message}");
                return new List<string>();
            }
        }

        // Método para registrar corredores específicos en una carrera
        public async Task<bool> RegistrarCorredoresEnCarreraAsync(string carreraId, IEnumerable<string> documentosCorredores, int dorsalInicial = 1)
        {
            try
            {
                var carrera = await ObtenerCarreraPorIdAsync(carreraId);
                if (carrera == null)
                {
                    Console.WriteLine($"❌ Carrera no encontrada: {carreraId}");
                    return false;
                }

                int dorsal = dorsalInicial;

                foreach (var documento in documentosCorredores)
                {
                    var corredor = await ObtenerCorredorPorDocumentoAsync(documento);
                    if (corredor == null)
                    {
                        Console.WriteLine($"⚠️ Corredor con documento {documento} no encontrado, se omite.");
                        continue;
                    }

                    // Verificar si ya existe registro para este corredor en esta carrera
                    var registroExistente = await _registros.Find(r => 
                        r.IDCarrera == carreraId && r.IdentifiCorredor == corredor.DocumentoIdentidad
                    ).FirstOrDefaultAsync();

                    if (registroExistente == null)
                    {
                        var registro = new Registro
                        {
                            IDCarrera = carreraId,
                            IdentifiCorredor = corredor.DocumentoIdentidad,
                            NumDorsal = dorsal,
                            Tiempos = new List<DateTime>()
                        };

                        await _registros.InsertOneAsync(registro);
                        Console.WriteLine($"✅ Corredor {corredor.Nombre} registrado en carrera - Dorsal: {dorsal}");
                        dorsal++;
                    }
                    else
                    {
                        Console.WriteLine($"⚠️ Registro ya existe para corredor {corredor.Nombre} en la carrera");
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al registrar corredores en carrera: {ex.Message}");
                return false;
            }
        }
    }
}