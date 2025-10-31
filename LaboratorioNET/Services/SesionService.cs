namespace LaboratorioNET.Services
{
    public class SesionService
    {
        // Evento que se dispara cuando cambia el estado de sesión
        public event Action? OnCambioSesion;

        // Información del usuario actual
        public string? DocumentoIdentidad { get; private set; }
        public string? NombreUsuario { get; private set; }
        public bool EstaAutenticado => !string.IsNullOrEmpty(DocumentoIdentidad);

        // Iniciar sesión
        public void IniciarSesion(string documentoIdentidad, string nombreUsuario)
        {
            DocumentoIdentidad = documentoIdentidad;
            NombreUsuario = nombreUsuario;
            NotificarCambioSesion();
            Console.WriteLine($"✅ Sesión iniciada: {nombreUsuario}");
        }

        // Cerrar sesión
        public void CerrarSesion()
        {
            Console.WriteLine($"🚪 Cerrando sesión de: {NombreUsuario}");
            DocumentoIdentidad = null;
            NombreUsuario = null;
            NotificarCambioSesion();
        }

        // Obtener información del usuario
        public (string? Documento, string? Nombre) ObtenerUsuarioActual()
        {
            return (DocumentoIdentidad, NombreUsuario);
        }

        // Notificar cambios
        private void NotificarCambioSesion()
        {
            OnCambioSesion?.Invoke();
        }
    }
}