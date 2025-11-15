# 🎯 GUÍA VISUAL RÁPIDA

## 🚀 Comenzar en 5 Minutos

### Paso 1: Configurar
```powershell
# Terminal - Windows PowerShell
$env:GOOGLE_APPLICATION_CREDENTIALS = "firebase-credentials.json"
$env:GCS_BUCKET_NAME = "laboratorionet-sensor-data"
```

### Paso 2: Compilar
```bash
dotnet build
```

### Paso 3: Ejecutar
```bash
dotnet run --launch-profile https
```

### Paso 4: Probar
```powershell
.\sensor-simulator.ps1 `
  -CarreraId "maraton-2024" `
  -CorredoresIds @("12345678", "87654321") `
  -CantSecciones 3
```

### Paso 5: Ver Resultados
```
Navegar a: https://localhost:7174/info_carreras
```

---

## 📋 Ejemplo de Flujo Completo

```
┌─────────────────────────────────────────┐
│ 1. SENSOR ENVÍA DATOS                   │
│ POST /api/sensor                        │
│ {                                       │
│   "corredorId": "12345678",            │
│   "carreraId": "maraton-2024",         │
│   "tiempo": "2024-11-12T14:30:00Z"     │
│ }                                       │
└────────────────┬────────────────────────┘
                 │
                 ▼
┌─────────────────────────────────────────┐
│ 2. VALIDACIÓN                           │
│ ✓ CorredorId existe                    │
│ ✓ CarreraId existe                     │
│ ✓ Timestamp válido                     │
└────────────────┬────────────────────────┘
                 │
                 ▼
┌─────────────────────────────────────────┐
│ 3. GUARDAR EN BUCKET                    │
│ gs://laboratorionet-sensor-data/        │
│ sensor_data/maraton-2024/               │
│ 20241112_143000_12345678.json           │
└────────────────┬────────────────────────┘
                 │
                 ▼
┌─────────────────────────────────────────┐
│ 4. ACTUALIZAR FIRESTORE                 │
│ collection: "registro"                  │
│ Tiempos: [timestamp1, timestamp2, ...]  │
└────────────────┬────────────────────────┘
                 │
                 ▼
┌─────────────────────────────────────────┐
│ 5. VERIFICAR ESTADO                     │
│ ¿Todos completaron?                     │
│ ├─ SÍ → Marcar TERMINADA               │
│ └─ NO → Continuar EN PROGRESO          │
└────────────────┬────────────────────────┘
                 │
                 ▼
┌─────────────────────────────────────────┐
│ 6. RESPUESTA API                        │
│ {                                       │
│   "mensaje": "Procesado",              │
│   "guardadoEnBucket": true,            │
│   "carreraTerminada": false            │
│ }                                       │
└────────────────┬────────────────────────┘
                 │
                 ▼
┌─────────────────────────────────────────┐
│ 7. UI ACTUALIZADA                       │
│ ⏱️ EN PROGRESO  [50% ████░░░░]         │
│ o                                       │
│ ✓ TERMINADA                            │
└─────────────────────────────────────────┘
```

---

## 🔧 Archivos Clave

```
📁 BucketService
   └─ Guarda datos en Google Cloud Storage
   └─ Respaldo automático
   └─ Recuperable siempre

📁 SensorValidationService
   └─ Valida todos los datos
   └─ Detecta anomalías
   └─ Genera reportes

📁 SensorController
   └─ Expone 7 endpoints
   └─ Procesa sensores
   └─ Retorna JSON

📁 Info_Carrera.razor
   └─ Muestra estado visual
   └─ Barra de progreso
   └─ Colores por estado
```

---

## 📊 Estados Visuales

### En Progreso
```
Carrera: Maratón 2024
Estado: ⏱️ EN PROGRESO

Corredor 1: [██████░░░░] 50% (2/4)
Corredor 2: [████████░░] 80% (3/4)
Corredor 3: [░░░░░░░░░░] 0%  (0/4)
```

### Terminada
```
Carrera: Maratón 2024
Estado: ✓ TERMINADA

Corredor 1: [██████████] 100% (4/4) ✓
Corredor 2: [██████████] 100% (4/4) ✓
Corredor 3: [██████████] 100% (4/4) ✓
```

---

## 🎮 Comando Simulador

### Sintaxis Completa
```powershell
.\sensor-simulator.ps1 `
  -Url "https://localhost:7174" `           # URL servidor
  -CarreraId "maraton-2024" `               # ID carrera (requerido)
  -CorredoresIds @("id1","id2","id3") `    # IDs corredores (requerido)
  -CantSecciones 5 `                        # Número de checkpoints
  -DelayMs 1500                             # Milisegundos entre eventos
```

### Ejemplo Rápido
```powershell
.\sensor-simulator.ps1 -CarreraId "carrera1" -CorredoresIds @("123","456")
```

---

## 🌐 Endpoints API Rápida

### 1. Procesar Sensor (Principal)
```
POST /api/sensor
Content-Type: application/json

{"corredorId":"123", "carreraId":"car1", "tiempo":"2024-11-12T14:30:00Z"}
```

### 2. Ver Estado
```
GET /api/sensor/estado-carrera/car1
```

### 3. Ver Datos
```
GET /api/sensor/carrera/car1
```

### 4. Estadísticas
```
GET /api/sensor/estadisticas/car1
```

### 5. Validar
```
POST /api/sensor/validar
Content-Type: application/json

{mismos datos que POST}
```

### 6. Reporte Completo
```
GET /api/sensor/reporte/car1
```

### 7. Limpiar
```
POST /api/sensor/limpiar/car1?dias=7
```

---

## 🎓 Estructura de Carpetas

```
Services/
├── BucketService.cs            ← Almacenamiento
├── FirebaseService.cs           ← Base de datos
└── SensorValidationService.cs   ← Validación

Controllers/
└── SensorController.cs          ← API REST

Models/
└── SensorCheckpointData.cs      ← Modelo de datos

Entities/
└── Carrera.cs                   ← + campo terminada
```

---

## ✅ Testing Rápido

### 1. HTTP Requests (VS Code)
```
Instalar: REST Client extension
Abrir: api-requests.http
Click: Send Request
```

### 2. PowerShell Script
```powershell
Set-ExecutionPolicy RemoteSigned -Scope CurrentUser
.\sensor-simulator.ps1 -CarreraId "test" -CorredoresIds @("123")
```

### 3. cURL
```bash
curl -X POST "https://localhost:7174/api/sensor" \
  -H "Content-Type: application/json" \
  -d '{"corredorId":"123","carreraId":"car1","tiempo":"2024-11-12T14:30:00Z"}' \
  --insecure
```

---

## 🐛 Troubleshooting Rápido

| Problema | Solución |
|----------|----------|
| "Bucket not found" | `gsutil mb gs://laboratorionet-sensor-data` |
| "Credential not found" | `$env:GOOGLE_APPLICATION_CREDENTIALS = "firebase-credentials.json"` |
| "Carrera no encontrada" | Verificar ID en Firestore Console |
| "HTTPS error" | Agregar `-SkipCertificateCheck` o `--insecure` |
| "Connection timeout" | Aumentar delay en simulador |

---

## 📈 Flujo de Desarrollo

```
1. Clone proyecto
        ↓
2. Configure credenciales
        ↓
3. dotnet restore
        ↓
4. dotnet build
        ↓
5. dotnet run --launch-profile https
        ↓
6. .\sensor-simulator.ps1 [params]
        ↓
7. Verificar en https://localhost:7174/info_carreras
        ↓
8. ✅ ¡Listo!
```

---

## 🎯 Checklist de Verificación

```
[ ] Credenciales configuradas
[ ] Bucket creado
[ ] Carrera en Firestore
[ ] Corredores inscritos
[ ] Registros creados
[ ] Aplicación ejecutando
[ ] Simulador probado
[ ] Datos en Bucket
[ ] UI actualizada
[ ] Carrera marcada como terminada
```

---

## 💡 Tips & Tricks

### PowerShell
```powershell
# Limpiar variable
Remove-Item env:GOOGLE_APPLICATION_CREDENTIALS

# Ver variable
$env:GOOGLE_APPLICATION_CREDENTIALS

# Set permanente
[Environment]::SetEnvironmentVariable("KEY","VALUE","User")
```

### .NET CLI
```bash
# Build release
dotnet build -c Release

# Run production
dotnet run -c Release

# Clean
dotnet clean
```

### Bucket
```bash
# Listar bucket
gsutil ls

# Ver tamaño
gsutil du -s gs://laboratorionet-sensor-data

# Sincronizar localmente
gsutil -m cp -r gs://laboratorionet-sensor-data ./local-backup
```

---

## 📞 Links Útiles

- **Firestore Console**: https://console.firebase.google.com/
- **Google Cloud Console**: https://console.cloud.google.com/
- **Local App**: https://localhost:7174/
- **Info Carreras**: https://localhost:7174/info_carreras

---

## 🚀 Próximas Iteraciones

Para mejorar aún más, considerar:

1. **Autenticación API**: Agregar API keys
2. **Rate Limiting**: Proteger contra spam
3. **WebSocket**: Actualizaciones en tiempo real
4. **Dashboard**: Panel de monitoreo
5. **Exportar**: PDF/Excel de resultados
6. **Notificaciones**: Email/SMS cuando termina

---

**¡Listo para comenzar! 🎉**

Cualquier pregunta, revisar documentación en la carpeta raíz.

