using LaboratorioNET.Components;
using LaboratorioNET.Models;
using LaboratorioNET.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.Configure<FirebaseSettings>(
    builder.Configuration.GetSection("FirebaseSettings"));
builder.Services.AddScoped<FirebaseService>();

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


app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
