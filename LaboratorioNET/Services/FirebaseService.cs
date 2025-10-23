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
                // Verificar que el archivo existe
                if (!File.Exists(settings.Value.ServiceAccountKeyPath))
                {
                    throw new FileNotFoundException($"No se encontró el archivo de credenciales: {settings.Value.ServiceAccountKeyPath}");
                }

                // Configurar credenciales desde el archivo JSON
                var credential = GoogleCredential.FromFile(settings.Value.ServiceAccountKeyPath);
                
                // Inicializar Firebase Admin solo una vez
                if (FirebaseApp.DefaultInstance == null)
                {
                    FirebaseApp.Create(new AppOptions
                    {
                        Credential = credential,
                        ProjectId = settings.Value.ProjectId
                    });
                }

                // Crear instancia de Firestore
                _firestoreDb = FirestoreDb.Create(settings.Value.ProjectId);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al inicializar Firebase: {ex.Message}");
                throw;
            }
        }

        // Colecciones de Firestore
        public CollectionReference Corredores => _firestoreDb.Collection("corredores");
        public CollectionReference Carreras => _firestoreDb.Collection("carreras");
        public CollectionReference Admins => _firestoreDb.Collection("admins");
        public CollectionReference Registros => _firestoreDb.Collection("registros");

        // Acceso directo a la base de datos
        public FirestoreDb Database => _firestoreDb;
    }
}