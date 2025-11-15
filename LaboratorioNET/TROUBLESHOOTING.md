# 🔧 Guía Completa de Troubleshooting

## 🚨 Problemas Comunes y Soluciones

### 1. Error: "Bucket not found"

**Síntomas**:
```
Error: gs://laboratorionet-sensor-data not found
Exception: Google.Cloud.Storage.V1.GoogleApiException
```

**Causas Posibles**:
- El bucket no existe en Google Cloud
- Credenciales sin permisos de acceso
- Nombre de bucket incorrecto

**Soluciones**:

**Opción A: Crear el bucket**
```bash
# Listar buckets existentes
gsutil ls

# Crear nuevo bucket
gsutil mb gs://laboratorionet-sensor-data

# Verificar creación
gsutil ls -b gs://laboratorionet-sensor-data
```

**Opción B: Verificar credenciales**
```powershell
# Windows PowerShell
$env:GOOGLE_APPLICATION_CREDENTIALS = "$(pwd)/firebase-credentials.json"

# Verificar acceso
$env:GOOGLE_APPLICATION_CREDENTIALS

# Si está vacío, establecer manualmente
$creds = Get-Content firebase-credentials.json
$env:GOOGLE_APPLICATION_CREDENTIALS = $creds
```

**Opción C: Verificar nombre en código**
```csharp
// En BucketService.cs
_bucketName = Environment.GetEnvironmentVariable("GCS_BUCKET_NAME") 
    ?? "laboratorionet-sensor-data";  // ← Verificar aquí
```

---

### 2. Error: "Credential not found" o "Permission denied"

**Síntomas**:
```
Exception: Google.Apis.Auth.OAuth2.InvalidOperationException
The Application Default Credentials are not available
```

**Causas Posibles**:
- Archivo de credenciales no encontrado
- Variable de entorno no establecida
- Permisos insuficientes en credenciales

**Soluciones**:

**Paso 1: Verificar archivo**
```bash
# Windows
dir firebase-credentials.json
dir .\firebase-credentials.json

# Linux/Mac
ls -la firebase-credentials.json
```

**Paso 2: Si el archivo existe, establecer variable**
```powershell
# PowerShell - Método 1 (Sesión actual)
$env:GOOGLE_APPLICATION_CREDENTIALS = "$(pwd)/firebase-credentials.json"

# PowerShell - Método 2 (Variables de entorno permanentes)
[Environment]::SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", "$(pwd)/firebase-credentials.json", "User")

# PowerShell - Método 3 (Si tienes el JSON como string)
$jsonContent = Get-Content firebase-credentials.json -Raw
$env:GOOGLE_APPLICATION_CREDENTIALS_JSON = $jsonContent
```

**Paso 3: Verificar permisos en la credencial**
```bash
# Revisar roles en Google Cloud Console
# La credencial debe tener:
# - Viewer
# - Storage Object Admin
# - Storage Object Creator
```

**Paso 4: Reiniciar la aplicación**
```bash
dotnet run --launch-profile https
```

---

### 3. Error: "Carrera no encontrada"

**Síntomas**:
```json
{
  "error": "La carrera no existe",
  "detalle": "El carreraId especificado no se encontró en Firestore"
}
```

**Causa**: La carrera no existe o el ID es incorrecto

**Solución**:

```powershell
# 1. Verificar en Firestore Console
# Ir a: https://console.firebase.google.com/
# Colección: "carrera"
# Verificar que existe un documento con el ID

# 2. Obtener ID correcto
# En la consola de Firebase, copiar el Document ID

# 3. Verificar con Query si usas script
curl -X GET "https://localhost:7174/api/sensor/estado-carrera/carrera-correcta-id" `
  --insecure
```

---

### 4. Error: "Corredor no inscrito en carrera"

**Síntomas**:
```json
{
  "error": "El corredor no está inscrito en esta carrera",
  "detalle": "No se encontró registro para el corredor"
}
```

**Causa**: El corredor no tiene un registro (inscripción) en esa carrera

**Solución**:

```javascript
// En Firestore Console, crear el registro manualmente
db.collection("registro").add({
  "IDCarrera": "maraton-2024",
  "IdentifiCorredor": "12345678",
  "NumDorsal": 1,
  "Tiempos": []
})

// O verificar que el corredor está inscrito
// Ir a colección "registro"
// Filtrar por IDCarrera
// Buscar el documento con IdentifiCorredor
```

---

### 5. Error: "HTTPS certificate error"

**Síntomas**:
```
Exception: System.Net.Http.HttpRequestException
The SSL connection could not be established
```

**Causa**: Certificado auto-firmado en desarrollo

**Soluciones**:

**Opción A: Usar flag SkipCertificateCheck en PowerShell**
```powershell
# Cuando uses Invoke-WebRequest o llamadas HTTP
$response = Invoke-WebRequest `
  -Uri "https://localhost:7174/api/sensor" `
  -SkipCertificateCheck

# O en curl
curl --insecure https://localhost:7174/api/sensor
```

**Opción B: Usar REST Client (VS Code)**
```
Extensión: REST Client
La extensión maneja certificados automáticamente
```

**Opción C: Desabilitar HTTPS en desarrollo (NO RECOMENDADO)**
```bash
# Ejecutar con HTTP
dotnet run --launch-profile http
```

---

### 6. Error: "Tiempo en el futuro"

**Síntomas**:
```json
{
  "error": "El tiempo no puede estar en el futuro",
  "validacionBasica": {
    "valido": false,
    "error": "El tiempo no puede estar en el futuro"
  }
}
```

**Causa**: El timestamp enviado es posterior a ahora

**Solución**:

```powershell
# Verificar reloj del sistema
Get-Date

# Si está muy adelantado, sincronizar
# Windows: Configuración → Hora y zona horaria → Sincronizar ahora

# En el JSON, usar timestamp actual
$ahora = [DateTime]::UtcNow.ToString("o")
Write-Host "Usar este tiempo: $ahora"

# En el sensor/script, usar
"tiempo": "2024-11-12T15:30:45Z"  # Tiempo actual
```

---

### 7. Error: "Datos duplicados detectados"

**Síntomas**:
```json
{
  "validacionBasica": { "valido": true },
  "deteccionAnomalias": {
    "sospechoso": true,
    "advertencia": "Este dato es muy similar a uno reciente (posible duplicado)"
  }
}
```

**Causa**: Se envió un dato muy similar recientemente

**Solución**:

```powershell
# Ignorar advertencia si es intencional
# Los duplicados se descartan automáticamente

# Para evitar duplicados en el futuro:
# 1. Aumentar delay entre envíos
# 2. Verificar que los sensores no envían dos veces
# 3. Agregar validación en el sensor
```

---

### 8. Error: "Timeout al conectar a Firestore"

**Síntomas**:
```
Exception: RpcException: Status(StatusCode=Unavailable)
```

**Causa**: 
- Firestore no está disponible
- Conexión de internet lenta
- Credenciales expiradas

**Soluciones**:

```powershell
# 1. Verificar conectividad
Test-NetConnection -ComputerName firestore.googleapis.com -Port 443

# 2. Verificar credenciales
# En Google Cloud Console → Credenciales
# Verificar que la credencial es válida

# 3. Aumentar timeout (en código)
// FirebaseService.cs - agregar opciones de timeout

# 4. Reintentar
# La mayoría de métodos ya tienen retry logic
```

---

### 9. Error: "Sin espacio en bucket"

**Síntomas**:
```
Exception: Google.Cloud.Storage.V1.GoogleApiException
Quota exceeded
```

**Solución**:

```powershell
# 1. Obtener estadísticas
curl -X GET "https://localhost:7174/api/sensor/estadisticas/maraton-2024" `
  --insecure

# 2. Limpiar datos antiguos
curl -X POST "https://localhost:7174/api/sensor/limpiar/maraton-2024?dias=7" `
  --insecure

# 3. Si es crítico, aumentar cuota en Google Cloud Console
```

---

### 10. Error: "Aplicación no inicia"

**Síntomas**:
```
Exception en Program.cs
```

**Checklist de Diagnóstico**:

```powershell
# 1. Verificar dependencias
dotnet list package

# 2. Restaurar paquetes
dotnet restore

# 3. Limpiar y reconstruir
dotnet clean
dotnet build

# 4. Ejecutar en modo verbose
dotnet run --no-build --verbose

# 5. Revisar appsettings.json
Get-Content appsettings.json

# 6. Verificar firebase-credentials.json
Test-Path firebase-credentials.json
```

---

## 🩺 Comando de Diagnóstico Completo

```powershell
# Script para diagnosticar todos los problemas
$diagnostico = @{
    "Archivo de credenciales" = Test-Path firebase-credentials.json
    "Contenido JSON válido" = {
        try { 
            $json = Get-Content firebase-credentials.json | ConvertFrom-Json
            "Válido"
        } catch {
            "Inválido"
        }
    }
    "Conexión a Internet" = (Test-NetConnection -ComputerName google.com -Port 443).TcpTestSucceeded
    "Firestore accesible" = (Test-NetConnection -ComputerName firestore.googleapis.com -Port 443).TcpTestSucceeded
    ".NET version" = dotnet --version
    "Paquetes instalados" = (dotnet list package).Split("`n").Count
}

$diagnostico | Format-Table -AutoSize
```

---

## 📊 Tabla de Error Codes

| Code | Mensaje | Causa | Acción |
|------|---------|-------|--------|
| 400 | Bad Request | Datos inválidos | Revisar formato JSON |
| 404 | Not Found | Recurso no existe | Verificar IDs en Firestore |
| 409 | Conflict | Dato duplicado | Esperar e intentar después |
| 500 | Server Error | Error interno | Revisar logs del servidor |
| 503 | Service Unavailable | Firestore offline | Reintentar en 30 segundos |

---

## 🔍 Debug Logging

**Activar logs detallados en Program.cs**:
```csharp
builder.Services.AddLogging(config =>
{
    config.SetMinimumLevel(LogLevel.Debug);
    config.AddConsole();
});
```

**Revisar logs en consola**:
```
✅ = Éxito
❌ = Error
⚠️ = Advertencia
📡 = Evento de sensor
✓ = Completado
```

---

## 📞 Escalación

Si después de todas estas soluciones aún hay problemas:

1. Recolectar logs completos
2. Captura de pantalla del error
3. Versión de .NET y Windows
4. Configuración de firewall
5. Contactar al equipo de desarrollo

