namespace LaboratorioNET.Models
{
    public class MongoDbSettings
    {
        public string ConnectionString { get; set; } = string.Empty;
        public string DatabaseName { get; set; } = string.Empty;
        public CollectionNames Collections { get; set; } = new();
    }

    public class CollectionNames
    {
        public string Corredores { get; set; } = "corredores";
        public string Carreras { get; set; } = "carreras";
        public string Admins { get; set; } = "admins";
        public string Registros { get; set; } = "registros";
    }
}