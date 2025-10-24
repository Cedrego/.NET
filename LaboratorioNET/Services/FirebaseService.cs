// FirebaseService.cs

using Google.Cloud.Firestore;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using LaboratorioNET.Models;
using Microsoft.Extensions.Options;
using System.IO; // Necesario para Path.Combine y File.Exists

namespace LaboratorioNET.Services
{
    public class FirebaseService
    {
        private readonly FirestoreDb _firestoreDb;

        public FirebaseService(IOptions<FirebaseSettings> settings)
        {
            try
            {
                // CRÍTICO: Construir la ruta absoluta (garantiza que el archivo se encuentre)
                string fullPath = Path.Combine(AppContext.BaseDirectory, settings.Value.ServiceAccountKeyPath);

                // 1. Verificación de archivo existente (Ayuda a diagnosticar)
                if (!File.Exists(fullPath))
                {
                    throw new FileNotFoundException($"No se encontró el archivo de credenciales en la ruta: {fullPath}. Asegúrese de que el archivo '{settings.Value.ServiceAccountKeyPath}' esté en la raíz y configurado con CopyToOutputDirectory en el .csproj.");
                }

                // 2. Configurar credenciales desde el archivo JSON
                var credential = GoogleCredential.FromFile(fullPath);
                
                // Inicializar Firebase Admin (necesario para la autenticación general)
                if (FirebaseApp.DefaultInstance == null)
                {
                    FirebaseApp.Create(new AppOptions
                    {
                        Credential = credential,
                        ProjectId = settings.Value.ProjectId
                    });
                }

                // 3. CORRECCIÓN CLAVE: Usar FirestoreDbBuilder para asignar la credencial
                _firestoreDb = new FirestoreDbBuilder
                {
                    ProjectId = settings.Value.ProjectId,
                    Credential = credential
                }.Build();
            }
            catch (Exception ex)
            {
                // Si esto falla, el error aparecerá en la terminal (consola del servidor)
                Console.WriteLine($"[ERROR FATAL FIREBASE] Error al inicializar Firebase: {ex.Message}");
                throw; 
            }
        }

        // Colecciones de Firestore
        public CollectionReference Corredores => _firestoreDb.Collection("corredores");
        public CollectionReference Carreras => _firestoreDb.Collection("carreras");
        public CollectionReference Admins => _firestoreDb.Collection("admins");
        public CollectionReference Registros => _firestoreDb.Collection("registros");
        public FirestoreDb Database => _firestoreDb;
    }
}