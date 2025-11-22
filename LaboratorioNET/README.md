# 📚 ÍNDICE COMPLETO DE DOCUMENTACIÓN

## 🎯 Sistema de Gestión de Carreras - LaboratorioNET v2.0

### 📖 Documentación (9 Archivos)

#### 1. **RESUMEN_IMPLEMENTACION.md** ⭐ COMIENZA AQUÍ
- 📋 Descripción general
- ✅ Objetivos cumplidos
- 🎯 Características implementadas
- 📊 Estadísticas del proyecto
# LaboratorioNET — Sistema de gestión y simulación de carreras

Este repositorio contiene una aplicación web desarrollada con ASP.NET Core + Blazor que permite gestionar carreras, corredores y puntos de control basados en datos de sensores. Incluye servicios para persistencia (MongoDB), almacenamiento de ficheros (BucketService), validación de sensores y una capa en tiempo real (SignalR) para la simulación de carreras.

Este `README` actúa como la documentación principal: tecnologías usadas, guía de uso, arquitectura, ejecución y notas de despliegue.

**Estado:** Mantenimiento activo
**Lenguaje:** C# / Blazor
**Solución:** `LaboratorioNET.sln`

----

**Índice rápido**
- **Visión general**: descripción del proyecto.
- **Tecnologías**: lista de tecnologías y librerías clave.
- **Requisitos**: dependencias y servicios externos (MongoDB, etc.).
- **Instalación y ejecución**: pasos para ejecutar localmente.
- **Arquitectura**: capas y responsabilidades.
- **Estructura del repositorio**: rutas principales de código.
- **Desarrollo**: cómo contribuir y ejecutar en modo desarrollo.
- **Despliegue**: recomendaciones para producción.

----

**Visión general**

LaboratorioNET es una aplicación diseñada para administrar competiciones atléticas y simular carreras usando datos de sensores. Funciona como una SPA con Blazor (componentes UI) y ofrece una API REST para ingestión y consulta de datos de sensores. La aplicación también incluye una parte en tiempo real mediante SignalR (`RaceSimulationHub`) que facilita la simulación y notificaciones en vivo.

**Casos de uso principales**
- Crear y listar carreras.
- Registrar corredores y preinscripciones.
- Ingestar eventos de checkpoints desde sensores.
- Simular una carrera en tiempo real y visualizar resultados y ranking.

----

**Tecnologías y dependencias clave**
- `ASP.NET Core` — backend y servidor web.
- `Blazor` — UI de cliente (componentes en `Components/Pages`).
- `MongoDB` — base de datos NoSQL para entidades (`MongoDbService`, `MongoDbSettings`).
- `SignalR` — comunicación en tiempo real (`RaceSimulationHub`).
- Servicios propios: `SensorValidationService`, `BucketService`, `SesionService`.
- `C#` 10+ (según SDK instalado) y `dotnet` CLI para compilación/ejecución.

----

**Requisitos**
- .NET SDK instalado (versión compatible con el proyecto).
- MongoDB (local o Atlas). Se usa desde `Services/MongoDbService.cs`.
- Opcional: servicio de almacenamiento compatible con `BucketService` (configuración en `appsettings.json`).

----

**Configuración**

1. Copia el fichero de configuración y actualiza los valores necesarios:

    - Edita `appsettings.json` y ajusta `MongoDbSettings` con tu conexión (ej. MongoDB Atlas URI o `mongodb://localhost:27017`).

2. Variables importantes en `appsettings.json`:

    - `MongoDbSettings:ConnectionString` — cadena de conexión a MongoDB.
    - `MongoDbSettings:DatabaseName` — nombre de la base de datos.
    - Ajustes para `BucketService` si usas almacenamiento externo.

----

**Instalación y ejecución local (PowerShell)**

1. Abrir PowerShell en la carpeta raíz del repositorio (donde está `LaboratorioNET.sln`).

2. Restaurar dependencias y compilar:

```powershell
dotnet restore; 
dotnet build
```

3. Ejecutar la aplicación (modo desarrollo):

```powershell
dotnet run --project .\LaboratorioNET\LaboratorioNET.csproj
```

4. Abrir el navegador en `https://localhost:5001` o la URL indicada por la salida de `dotnet run`.

----

**Estructura del proyecto (resumen)**

- `Program.cs` — arranque de la aplicación y registro de servicios.
- `Controllers/` — controladores Web API (por ejemplo `SensorController.cs`).
- `Services/` — lógica de negocio y acceso a datos (`MongoDbService.cs`, `SensorValidationService.cs`, `BucketService.cs`, `SesionService.cs`).
- `Entities/` — modelos del dominio (`Admin.cs`, `Carrera.cs`, `Corredor.cs`, `Registro.cs`).
- `Components/Pages/` — páginas Blazor para la interfaz.
- `Models/` — modelos auxiliares y DTOs (`MongoDbSettings.cs`, `SensorCheckpointData.cs`).
- `wwwroot/` — activos estáticos (CSS, imágenes, libs).

----

**Arquitectura (alto nivel)**

La aplicación sigue una arquitectura en capas sencilla:

- Capa de Presentación: Blazor (componentes en `Components/Pages`).
- Capa de Controladores/API: recibe peticiones REST (`Controllers/`).
- Capa de Servicios: lógica del dominio y abstracción de datos (`Services/`).
- Persistencia: MongoDB a través de `MongoDbService`.
- Integración en tiempo real: SignalR (`RaceSimulationHub`) para push de eventos y simulación.

Los servicios se inyectan con DI (registrados en `Program.cs`). La separación facilita pruebas unitarias y evolución del sistema.

----

**Endpoints y puntos de entrada**

- Las APIs REST principales están en `Controllers/SensorController.cs` (consultar ese archivo para rutas y payloads concretos).
- La UI Blazor expone páginas en `Components/Pages/` para administración, inscripción y simulación.

Si necesitas ejemplos de requests, revisa el archivo `api-requests.http` incluido en el repositorio.


----
Comandos útiles:

```powershell
dotnet restore
dotnet build
dotnet run --project .\LaboratorioNET\LaboratorioNET.csproj
```

----

**Problemas comunes y debugging**

- Si la app no conecta a MongoDB: revisar `MongoDbSettings:ConnectionString` y comprobar que MongoDB acepta conexiones desde la IP del host.
- Errores en la UI: abrir la consola del navegador para ver errores JS/SignalR.
- Revisar logs de la aplicación (nivel de logging ajustable en `Program.cs` / `appsettings.json`).
