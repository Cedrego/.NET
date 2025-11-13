# 🚀 Guía Rápida de Inicio

## 1️⃣ Configuración Inicial

### Paso 1: Verificar dependencias en `.csproj`
```bash
dotnet list package
```

Asegúrate de tener:
- `Google.Cloud.Firestore`
- `Google.Cloud.Storage.V1`
- `FirebaseAdmin`

### Paso 2: Configurar Google Cloud Bucket

**Opción A: Usando archivo local** (Desarrollo)
```bash
# En appsettings.json
{
  "FirebaseSettings": {
    "ProjectId": "tu-proyecto",
    "ServiceAccountKeyPath": "firebase-credentials.json"
  }
}
```

**Opción B: Usando variable de entorno** (Producción)
```bash
# Windows PowerShell
$env:GOOGLE_APPLICATION_CREDENTIALS_JSON = $(Get-Content firebase-credentials.json | ConvertTo-Json)
$env:GCS_BUCKET_NAME = "laboratorionet-sensor-data"

# Windows CMD
set GOOGLE_APPLICATION_CREDENTIALS_JSON=<tu-json>
set GCS_BUCKET_NAME=laboratorionet-sensor-data

# Linux/Mac
export GOOGLE_APPLICATION_CREDENTIALS_JSON='<tu-json>'
export GCS_BUCKET_NAME='laboratorionet-sensor-data'
```

### Paso 3: Crear el Bucket en Google Cloud (si no existe)
```bash
gsutil mb gs://laboratorionet-sensor-data
```

---

## 2️⃣ Compilar y Ejecutar

```bash
# Limpiar solución
dotnet clean

# Restaurar paquetes
dotnet restore

# Compilar
dotnet build

# Ejecutar en desarrollo
dotnet run --launch-profile https
```

**URLs locales**:
- 🌐 HTTPS: `https://localhost:7174`
- 🌐 HTTP: `http://localhost:5174`

---

## 3️⃣ Configurar Datos de Prueba

### Crear una Carrera
```bash
# En Firestore Console o mediante Admin
db.collection("carrera").add({
  "idCarrera": "maraton-2024",
  "nombre": "Maratón Ciudad 2024",
  "cantSecciones": 3,
  "fechaInicio": Timestamp.now(),
  "lugarSalida": "Plaza Central",
  "limiteParticipantes": 50,
  "tipo": "Maratón",
  "terminada": false
})
```

### Crear Corredores
```javascript
db.collection("corredores").add({
  "documentoIdentidad": "12345678",
  "nombre": "Juan Pérez",
  "correo": "juan@example.com",
  "rol": "Corredor"
})

db.collection("corredores").add({
  "documentoIdentidad": "87654321",
  "nombre": "María García",
  "correo": "maria@example.com",
  "rol": "Corredor"
})
```

### Crear Registros (Inscriciones)
```javascript
db.collection("registro").add({
  "IDCarrera": "maraton-2024",
  "IdentifiCorredor": "12345678",
  "NumDorsal": 1,
  "Tiempos": []
})

db.collection("registro").add({
  "IDCarrera": "maraton-2024",
  "IdentifiCorredor": "87654321",
  "NumDorsal": 2,
  "Tiempos": []
})
```

---

## 4️⃣ Probar el Sistema

### Opción A: Usar Simulador PowerShell (⭐ RECOMENDADO)
```powershell
# Con permiso de ejecución
Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope CurrentUser

# Ejecutar simulador
.\sensor-simulator.ps1 `
  -CarreraId "maraton-2024" `
  -CorredoresIds @("12345678", "87654321") `
  -CantSecciones 3 `
  -DelayMs 1500
```

### Opción B: Usar REST Client (VS Code)
```
Instalar extensión: "REST Client" de Huachao Mao
Abrir: api-requests.http
Enviar requests con "Send Request"
```

### Opción C: Usar cURL
```bash
# Enviar dato de sensor
curl -X POST "https://localhost:7174/api/sensor" `
  -H "Content-Type: application/json" `
  -d @"{
    \"corredorId\": \"12345678\",
    \"carreraId\": \"maraton-2024\",
    \"tiempo\": \"2024-11-12T14:30:00Z\",
    \"numeroCheckpoint\": 1
  }" `
  --insecure

# Obtener estado
curl -X GET "https://localhost:7174/api/sensor/estado-carrera/maraton-2024" `
  --insecure
```

---

## 5️⃣ Verificar Resultados

### En la Aplicación Web
1. Navegar a `/info_carreras`
2. Seleccionar "Maratón Ciudad 2024"
3. Verificar:
   - ✅ Estado: "⏱️ EN PROGRESO" o "✓ TERMINADA"
   - ✅ Progreso: Barra de progreso en cada corredor
   - ✅ Tiempos: Listado de tiempos por sección

### En Google Cloud Console
1. Navegar a Cloud Storage
2. Abrir bucket `laboratorionet-sensor-data`
3. Verificar carpeta `sensor_data/{carreraId}/`
4. Revisar archivos JSON con los datos

### En Firestore Console
1. Colección `registro`
2. Buscar documento de corredor
3. Verificar campo `Tiempos` con timestamps

---

## 6️⃣ Endpoints Disponibles

```
POST   /api/sensor
  └─ Recibe datos de sensores

GET    /api/sensor/carrera/{carreraId}
  └─ Obtiene datos guardados en bucket

GET    /api/sensor/estado-carrera/{carreraId}
  └─ Obtiene estado actual de carrera
```

---

## 7️⃣ Troubleshooting

### ❌ "Bucket not found"
```bash
# Solución 1: Crear bucket
gsutil mb gs://laboratorionet-sensor-data

# Solución 2: Verificar nombre exacto
gsutil ls
```

### ❌ "Credential not found"
```bash
# Verificar archivo
ls firebase-credentials.json

# O establecer variable de entorno
$env:GOOGLE_APPLICATION_CREDENTIALS = "$(pwd)/firebase-credentials.json"
```

### ❌ "Carrera no encontrada"
```bash
# Verificar que existe en Firestore
db.collection("carrera").where("IdCarrera", "==", "maraton-2024").get()
```

### ❌ "HTTPS certificate error"
```powershell
# Usar -SkipCertificateCheck en PowerShell
Invoke-WebRequest -SkipCertificateCheck -Uri "https://localhost:7174/api/sensor"

# O desabilitar validación en curl
curl --insecure https://localhost:7174/api/sensor
```

---

## 📋 Checklist de Verificación

- [ ] Dependencias instaladas
- [ ] Credenciales configuradas
- [ ] Bucket creado en Google Cloud
- [ ] Carrera creada en Firestore
- [ ] Corredores creados
- [ ] Registros creados
- [ ] Aplicación ejecutándose
- [ ] Simulador probado
- [ ] Datos en Info_Carrera actualizados
- [ ] Archivos en bucket visibles

---

## 📚 Archivos Clave

| Archivo | Propósito |
|---------|----------|
| `Services/BucketService.cs` | Manejo de Google Cloud Storage |
| `Services/FirebaseService.cs` | Métodos nuevos para actualizar registros |
| `Controllers/SensorController.cs` | Endpoints de API |
| `Models/SensorCheckpointData.cs` | Modelo de datos de sensores |
| `Components/Pages/Info_Carrera.razor` | UI mejorada |
| `Program.cs` | Configuración de servicios |
| `sensor-simulator.ps1` | Script de prueba |
| `SENSOR_SYSTEM_DOCUMENTATION.md` | Documentación completa |
| `api-requests.http` | Ejemplos de requests |

---

## 🎓 Conceptos Clave

### ¿Cómo funciona el Bucket?
1. Cada dato de sensor se guarda en bucket **antes** de actualizar Firestore
2. Si Firestore falla, los datos están seguros en el bucket
3. Se pueden recuperar después si es necesario

### ¿Cuándo se marca como "TERMINADA"?
```
La carrera está TERMINADA cuando:
TODOS los corredores = tienen TODOS los tiempos
```

### ¿Qué pasa si un corredor no llega a un checkpoint?
```
El registro sigue mostrando solo los tiempos que registró
La carrera permanece EN PROGRESO
Se puede actualizar manualmente o esperar al siguiente checkpoint
```

---

## 💡 Próximas Mejoras Sugeridas

1. **Autenticación** en API endpoints
2. **Rate limiting** para prevenir spam
3. **Histórico** de cambios de estado
4. **Notificaciones** en tiempo real con WebSocket
5. **Panel** de monitoreo en tiempo real
6. **Exportar** resultados a Excel/PDF

