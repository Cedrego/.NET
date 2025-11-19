using LaboratorioNET.Components;
using LaboratorioNET.Models;
using LaboratorioNET.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.Configure<MongoDbSettings>(
    builder.Configuration.GetSection("MongoDB"));
builder.Services.AddSingleton<MongoDbService>();
builder.Services.AddSingleton<BucketService>();

// Agregar SignalR para actualizaciones en tiempo real
builder.Services.AddSignalR();

// Agregar controladores para API
builder.Services.AddControllers();

// Registrar servicio de sesión (Scoped para que sea por usuario)
builder.Services.AddSingleton<SesionService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// Mapear controladores de API
app.MapControllers();

// Mapear SignalR Hub
app.MapHub<RaceSimulationHub>("/raceSimulationHub");

app.Run();