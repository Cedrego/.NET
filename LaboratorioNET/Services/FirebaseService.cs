using Google.Cloud.Firestore;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using LaboratorioNET.Models;
using LaboratorioNET.Entities;
using Microsoft.Extensions.Options;

namespace LaboratorioNET.Services
{
    public class FirebaseService
    {
        private readonly FirestoreDb _firestoreDb;

        public FirebaseService(IOptions<FirebaseSettings> settings)
        {
            try
            {
                GoogleCredential credential;
                
                // PRIORIDAD 1: Variable de entorno (para CI/CD y producción)
                string? credentialsJson = Environment.GetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS_JSON");
                
                if (!string.IsNullOrEmpty(credentialsJson))
                {
                    Console.WriteLine("✅ Usando credenciales desde variable de entorno");
                    try
                    {
                        credential = GoogleCredential.FromJson(credentialsJson)
                            .CreateScoped(
                                "https://www.googleapis.com/auth/cloud-platform",
                                "https://www.googleapis.com/auth/datastore",
                                "https://www.googleapis.com/auth/firebase"
                            );
                        Console.WriteLine("✅ Credenciales cargadas y configuradas correctamente desde variable de entorno");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"❌ Error al cargar credenciales desde variable de entorno: {ex.Message}");
                        throw;
                    }
                }
                // PRIORIDAD 2: Archivo local (para desarrollo)
                else if (!string.IsNullOrEmpty(settings.Value.ServiceAccountKeyPath))
                {
                    string finalCredentialsPath = settings.Value.ServiceAccountKeyPath; // Cambiamos el nombre
                    
                    // Si la ruta no es absoluta, buscar en diferentes ubicaciones
                    if (!Path.IsPathRooted(finalCredentialsPath))
                    {
                        // 1. Intentar en el directorio de ejecución
                        string execPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, finalCredentialsPath);
                        
                        // 2. Intentar en el directorio actual del proyecto
                        string currentPath = Path.Combine(Directory.GetCurrentDirectory(), finalCredentialsPath);
                        
                        // 3. Intentar subiendo niveles desde bin/Debug/net9.0
                        string projectPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", finalCredentialsPath);
                        string normalizedProjectPath = Path.GetFullPath(projectPath);
                        
                        if (File.Exists(execPath))
                        {
                            finalCredentialsPath = execPath;
                            Console.WriteLine($"✅ Credenciales encontradas en: {execPath}");
                        }
                        else if (File.Exists(currentPath))
                        {
                            finalCredentialsPath = currentPath;
                            Console.WriteLine($"✅ Credenciales encontradas en: {currentPath}");
                        }
                        else if (File.Exists(normalizedProjectPath))
                        {
                            finalCredentialsPath = normalizedProjectPath;
                            Console.WriteLine($"✅ Credenciales encontradas en: {normalizedProjectPath}");
                        }
                        else
                        {
                            string errorMsg = $"ERROR: No se encontró firebase-credentials.json\n" +
                                $"Ubicaciones buscadas:\n" +
                                $"  1. {execPath}\n" +
                                $"  2. {currentPath}\n" +
                                $"  3. {normalizedProjectPath}\n\n" +
                                $"SOLUCIÓN: Configure variable de entorno GOOGLE_APPLICATION_CREDENTIALS_JSON o coloque el archivo.";
                            
                            Console.WriteLine(errorMsg);
                            throw new FileNotFoundException(errorMsg, finalCredentialsPath);
                        }
                    }
                    else if (!File.Exists(finalCredentialsPath))
                    {
                        throw new FileNotFoundException($"No se encontró el archivo de credenciales: {finalCredentialsPath}");
                    }

                    try 
                    {
                        Console.WriteLine($"✅ Usando archivo de credenciales: {finalCredentialsPath}");
                        Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", finalCredentialsPath);
                        
                        // Usar autenticación por defecto
                        credential = GoogleCredential.GetApplicationDefault()
                            .CreateScoped(
                                "https://www.googleapis.com/auth/cloud-platform",
                                "https://www.googleapis.com/auth/datastore",
                                "https://www.googleapis.com/auth/firebase"
                            );
                        Console.WriteLine("✅ Credenciales configuradas correctamente");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"❌ Error al configurar credenciales: {ex.Message}");
                        throw;
                    }
                }
                else
                {
                    throw new InvalidOperationException(
                        "No se encontraron credenciales de Firebase. Configure:\n" +
                        "1. Variable de entorno GOOGLE_APPLICATION_CREDENTIALS_JSON, o\n" +
                        "2. Archivo firebase-credentials.json en el proyecto");
                }
                
                // Inicializar Firebase Admin
                if (FirebaseApp.DefaultInstance == null)
                {
                    FirebaseApp.Create(new AppOptions
                    {
                        Credential = credential,
                        ProjectId = settings.Value.ProjectId
                    });
                }

                // Crear instancia de Firestore usando las credenciales
                var firestoreClientBuilder = new Google.Cloud.Firestore.V1.FirestoreClientBuilder
                {
                    Credential = credential
                };
                var firestoreClient = firestoreClientBuilder.Build();
                _firestoreDb = FirestoreDb.Create(settings.Value.ProjectId, firestoreClient);
                
                Console.WriteLine("✅ Firebase inicializado correctamente");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error al inicializar Firebase: {ex.Message}");
                throw;
            }
        }

        // Colecciones de Firestore
        public CollectionReference Corredores => _firestoreDb.Collection("corredores");
        public CollectionReference Carreras => _firestoreDb.Collection("carrera");
        public CollectionReference Admins => _firestoreDb.Collection("admin");
        public CollectionReference Registros => _firestoreDb.Collection("registro");

        // Acceso directo a la base de datos
        public FirestoreDb Database => _firestoreDb;

        // ===== MÉTODOS PARA CORREDORES =====
        
        public async Task<List<Corredor>> ObtenerTodosCorredoresAsync()
        {
            try
            {
                var snapshot = await Corredores.GetSnapshotAsync();
                return snapshot.Documents.Select(doc => doc.ConvertTo<Corredor>()).ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al obtener corredores: {ex.Message}");
                return new List<Corredor>();
            }
        }

        public async Task<Corredor?> ObtenerCorredorPorDocumentoAsync(string documentoIdentidad)
        {
            try
            {
                var snapshot = await Corredores
                    .WhereEqualTo("DocumentoIdentidad", documentoIdentidad)
                    .Limit(1)
                    .GetSnapshotAsync();

                return snapshot.Documents.FirstOrDefault()?.ConvertTo<Corredor>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al obtener corredor: {ex.Message}");
                return null;
            }
        }
        public async Task<Corredor?> ObtenerCorredorPorNombreAsync(string NombreIdentidad)
                {
                    try
                    {
                        Console.WriteLine($"Buscando corredor con nombre: '{NombreIdentidad}'");
                        var snapshot = await Corredores
                            .WhereEqualTo("nombre", NombreIdentidad)
                            .Limit(1)
                            .GetSnapshotAsync();

                        var encontrados = snapshot.Documents.Count;
                        Console.WriteLine($"Documentos encontrados: {encontrados}");
                        
                        if (encontrados == 0)
                        {
                            Console.WriteLine("No se encontraron corredores con ese nombre");
                            return null;
                        }

                        var corredor = snapshot.Documents.First().ConvertTo<Corredor>();
                        Console.WriteLine($"Corredor encontrado: {corredor.Nombre}");
                        return corredor;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error al obtener corredor: {ex.Message}");
                        Console.WriteLine($"Stack trace: {ex.StackTrace}");
                        return null;
                    }
                }
        public async Task<List<Corredor>> ObtenerCorredoresFiltradosAsync(string? carreraId = null, string? origen = null)
        {
            try
            {
                Query query = Corredores;

                // Filtrar por origen si se especifica
                if (!string.IsNullOrEmpty(origen))
                {
                    query = query.WhereEqualTo("Origen", origen);
                }

                var snapshot = await query.GetSnapshotAsync();
                var corredores = snapshot.Documents.Select(doc => doc.ConvertTo<Corredor>()).ToList();

                // Filtrar por carrera si se especifica
                if (!string.IsNullOrEmpty(carreraId))
                {
                    var registrosSnapshot = await Registros.WhereEqualTo("IDCarrera", carreraId).GetSnapshotAsync();
                    var documentosEnCarrera = registrosSnapshot.Documents
                        .Select(doc => doc.ConvertTo<Registro>().IdentifiCorredor)
                        .ToHashSet();

                    corredores = corredores.Where(c => documentosEnCarrera.Contains(c.DocumentoIdentidad)).ToList();
                }

                return corredores;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al obtener corredores filtrados: {ex.Message}");
                return new List<Corredor>();
            }
        }

        public async Task<List<string>> ObtenerOrigenesUnicosAsync()
        {
            try
            {
                var snapshot = await Corredores.GetSnapshotAsync();
                var origenes = snapshot.Documents
                    .Select(doc => doc.GetValue<string>("Origen"))
                    .Where(o => !string.IsNullOrWhiteSpace(o))
                    .Distinct()
                    .OrderBy(o => o)
                    .ToList();
                return origenes;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al obtener orígenes: {ex.Message}");
                return new List<string>();
            }
        }

        public async Task<bool> AgregarCorredorAsync(Corredor corredor)
        {
            try
            {
                // Verificar si ya existe un corredor con ese documento
                var existente = await ObtenerCorredorPorDocumentoAsync(corredor.DocumentoIdentidad);
                if (existente != null)
                {
                    Console.WriteLine("Ya existe un corredor con ese documento de identidad");
                    return false;
                }

                await Corredores.AddAsync(corredor);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al agregar corredor: {ex.Message}");
                return false;
            }
        }

        // ===== MÉTODOS PARA CARRERAS =====
        
        public async Task<List<Carrera>> ObtenerTodasCarrerasAsync()
        {
            try
            {
                var snapshot = await Carreras.GetSnapshotAsync();
                return snapshot.Documents.Select(doc => doc.ConvertTo<Carrera>()).ToList();
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
                var docSnapshot = await Carreras.Document(id).GetSnapshotAsync();
                return docSnapshot.Exists ? docSnapshot.ConvertTo<Carrera>() : null;
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
                var snapshot = await Carreras
                    .WhereEqualTo("IdCarrera", idCarrera)
                    .Limit(1)
                    .GetSnapshotAsync();

                return snapshot.Documents.FirstOrDefault()?.ConvertTo<Carrera>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al obtener carrera: {ex.Message}");
                return null;
            }
        }

        // ===== MÉTODOS PARA REGISTROS =====
        
        public async Task<List<Registro>> ObtenerRegistrosPorCarreraAsync(string idCarrera)
        {
            try
            {
                var snapshot = await Registros
                    .WhereEqualTo("IDCarrera", idCarrera)
                    .GetSnapshotAsync();

                return snapshot.Documents.Select(doc => doc.ConvertTo<Registro>()).ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al obtener registros: {ex.Message}");
                return new List<Registro>();
            }
        }

        public async Task<Registro?> ObtenerRegistroCorredorEnCarreraAsync(string idCarrera, string documentoCorredor)
        {
            try
            {
                var snapshot = await Registros
                    .WhereEqualTo("IDCarrera", idCarrera)
                    .WhereEqualTo("IdentifiCorredor", documentoCorredor)
                    .Limit(1)
                    .GetSnapshotAsync();

                return snapshot.Documents.FirstOrDefault()?.ConvertTo<Registro>();
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
                var snapshot = await Registros
                    .WhereEqualTo("IdentifiCorredor", documentoCorredor)
                    .GetSnapshotAsync();

                return snapshot.Documents.Select(doc => doc.ConvertTo<Registro>()).ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al obtener registros del corredor: {ex.Message}");
                return new List<Registro>();
            }
        }

        // ===== MÉTODOS DE UTILIDAD =====
        
        public async Task<int> ContarCorredoresEnCarreraAsync(string idCarrera)
        {
            try
            {
                var snapshot = await Registros
                    .WhereEqualTo("IDCarrera", idCarrera)
                    .GetSnapshotAsync();

                return snapshot.Documents.Count;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al contar corredores: {ex.Message}");
                return 0;
            }
        }
    }
}
