using Microsoft.AspNetCore.SignalR;
using System.Text.Json;

namespace LaboratorioNET.Services
{
    /// <summary>
    /// SignalR Hub para actualizaciones en tiempo real de carreras y registros
    /// </summary>
    public class RaceSimulationHub : Hub
    {
        /// <summary>
        /// Notifica a todos los clientes que se agregó un nuevo tiempo a un registro
        /// Acepta dynamic para compatibilidad con llamadas desde componentes Blazor
        /// </summary>
        public async Task NotifyTimeAdded(dynamic data)
        {
            await Clients.All.SendAsync("TimeAdded", (object)data);
        }

        /// <summary>
        /// Notifica cambio de estado de la carrera (Programada, En Progreso, Terminada)
        /// </summary>
        public async Task NotifyCarreraStatusChanged(dynamic data)
        {
            await Clients.All.SendAsync("CarreraStatusChanged", (object)data);
        }

        /// <summary>
        /// Notifica que la simulación inició
        /// </summary>
        public async Task NotifySimulationStarted(dynamic data)
        {
            await Clients.All.SendAsync("SimulationStarted", (object)data);
        }

        /// <summary>
        /// Notifica que la simulación se detuvo
        /// </summary>
        public async Task NotifySimulationStopped(dynamic data)
        {
            await Clients.All.SendAsync("SimulationStopped", (object)data);
        }

        public override async Task OnConnectedAsync()
        {
            await base.OnConnectedAsync();
            Console.WriteLine($"[SignalR] Cliente conectado: {Context.ConnectionId}");
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            await base.OnDisconnectedAsync(exception);
            Console.WriteLine($"[SignalR] Cliente desconectado: {Context.ConnectionId}");
        }
    }
}
