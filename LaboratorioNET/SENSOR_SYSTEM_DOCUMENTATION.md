# 📋 Sistema de Actualizaciones de Tiempos con Bucket

## Descripción General

Este sistema permite registrar los tiempos de los corredores en una carrera de forma confiable sin perder datos cuando los sensores de checkpoint envían información. Incluye:

1. **Servicio Bucket** - Almacena datos en Google Cloud Storage como respaldo
2. **API REST** - Endpoint para recibir datos de sensores
3. **Actualización de Registros** - Agrega tiempos a los registros de corredores
4. **Estado de Carrera** - Marca automáticamente cuando una carrera está terminada
5. **Simulador** - Script PowerShell para probar el sistema

---

## 🏗️ Arquitectura de Cambios

### Entidades Modificadas

#### **Carrera.cs**
```csharp
[FirestoreProperty("terminada")]
public bool Terminada { get; set; } = false;
```
- Campo booleano que indica si la carrera está terminada
- Se actualiza automáticamente cuando todos los corredores completan todos los checkpoints

### Nuevos Servicios

#### **BucketService.cs**
```csharp
- GuardarDatosSensorAsync()      // Guarda datos en bucket como respaldo
- ObtenerDatosSensorPorCarreraAsync()  // Obtiene todos los datos de una carrera
- EliminarDatosSensorAsync()     // Elimina datos después de procesar
```

#### **FirebaseService.cs** (Métodos Nuevos)
```csharp
- AgregarTiempoAlRegistroAsync()       // Agrega tiempo al registro
- ActualizarEstadoCarreraAsync()       // Actualiza campo "terminada"
- VerificarCarreraTerminadaAsync()     // Verifica si carrera debe terminarse
```

### Nuevos Controllers

#### **SensorController.cs**
- `POST /api/sensor` - Recibe datos de sensores
- `GET /api/sensor/carrera/{carreraId}` - Obtiene datos guardados
- `GET /api/sensor/estado-carrera/{carreraId}` - Verifica estado actual

---

## 📡 API Endpoints

### 1. Procesar Datos de Sensor
```
POST /api/sensor
Content-Type: application/json

{
  "corredorId": "12345678",
  "carreraId": "carrera-id-123",
  "tiempo": "2024-11-12T14:30:45Z",
  "numeroCheckpoint": 1
}
```

**Respuesta Exitosa (200):**
```json
{
  "mensaje": "Datos procesados correctamente",
  "guardadoEnBucket": true,
  "registroActualizado": true,
  "carreraTerminada": false,
  "timestamp": "2024-11-12T14:30:45Z",
  "corredorId": "12345678",
  "carreraId": "carrera-id-123"
}
```

### 2. Obtener Datos Almacenados
```
GET /api/sensor/carrera/carrera-id-123
```

**Respuesta:**
```json
{
  "carreraId": "carrera-id-123",
  "cantidadDatos": 5,
  "datos": [
    { "corredorId": "...", "tiempo": "...", "registradoEn": "..." }
  ]
}
```

### 3. Obtener Estado de Carrera
```
GET /api/sensor/estado-carrera/carrera-id-123
```

**Respuesta:**
```json
{
  "carreraId": "carrera-id-123",
  "nombreCarrera": "Maratón 2024",
  "terminada": false,
  "cantidadSecciones": 3,
  "corredoresRegistrados": 2,
  "detalleCorredores": [
    {
      "corredorId": "12345678",
      "dorsal": 1,
      "tiemposRegistrados": 2,
      "completado": false,
      "tiempos": ["2024-11-12T14:30:45Z", "2024-11-12T14:35:20Z"]
    }
  ]
}
```

---

## 🧪 Usar el Simulador de Sensores

### Requisitos
- PowerShell 5.1 o superior
- Acceso HTTPS al servidor

### Sintaxis
```powershell
.\sensor-simulator.ps1 -CarreraId "carrera-123" -CorredoresIds @("12345678", "87654321") -CantSecciones 3 -DelayMs 2000
```

### Parámetros
- **CarreraId**: ID de la carrera (requerido)
- **CorredoresIds**: Array de IDs de corredores (requerido)
- **CantSecciones**: Número de checkpoints (default: 3)
- **DelayMs**: Delay entre envíos en milisegundos (default: 2000)
- **Url**: URL del servidor (default: https://localhost:7174)

### Ejemplo Completo
```powershell
$corredores = @("12345678", "87654321", "55555555")
.\sensor-simulator.ps1 `
  -Url "https://localhost:7174" `
  -CarreraId "maraton-2024" `
  -CorredoresIds $corredores `
  -CantSecciones 5 `
  -DelayMs 1500
```

### Salida Esperada
```
🏃 SIMULADOR DE CARRERA
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
Carrera ID: maraton-2024
Corredores: 3
Checkpoints: 5
Delay entre checkpoint: 1500ms
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

🚩 CHECKPOINT 1 de 5
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
📤 Enviando dato de sensor: Corredor=12345678, Checkpoint=1
✅ Dato procesado exitosamente
   Carrera Terminada: False
```

---

## 🗂️ Almacenamiento en Bucket

### Estructura de Carpetas
```
gs://laboratorionet-sensor-data/
└── sensor_data/
    └── {carreraId}/
        └── {timestamp}_{corredorId}.json
```

### Ejemplo de Archivo
```
gs://laboratorionet-sensor-data/sensor_data/maraton-2024/20241112_143045_123_12345678.json

Contenido:
{
  "carreraId": "maraton-2024",
  "corredorId": "12345678",
  "tiempo": "2024-11-12T14:30:45Z",
  "registradoEn": "2024-11-12T14:30:46.123Z"
}
```

---

## 🔄 Flujo de Procesamiento

```
1. SENSOR ENVÍA DATOS
   ↓
2. API RECIBE JSON
   ↓
3. GUARDAR EN BUCKET (respaldo)
   ↓
4. CONVERTIR A TIMESTAMP DE FIRESTORE
   ↓
5. ACTUALIZAR REGISTRO DEL CORREDOR
   ↓
6. VERIFICAR SI CARRERA ESTÁ COMPLETA
   ├─ Si: Marcar como "terminada = true"
   └─ Si no: Continuar
   ↓
7. DEVOLVER RESPUESTA AL SENSOR
```

---

## 📊 Lógica de Finalización de Carrera

Una carrera se marca como **TERMINADA** cuando:
- ✅ Todos los corredores registrados han pasado por TODOS los checkpoints
- ✅ Cada corredor tiene `Tiempos.Count == CantSecciones`

Visualización en Info_Carrera:
- **En Progreso** ⏱️: Aún hay corredores sin completar
- **Terminada** ✓: Todos completaron

---

## ⚙️ Configuración

### Archivo appsettings.json
```json
{
  "FirebaseSettings": {
    "ProjectId": "tu-proyecto",
    "ServiceAccountKeyPath": "firebase-credentials.json"
  }
}
```

### Variables de Entorno
```bash
# Google Cloud Storage Bucket
GCS_BUCKET_NAME=laboratorionet-sensor-data

# Firebase
GOOGLE_APPLICATION_CREDENTIALS_JSON={tu-json-credenciales}
GOOGLE_APPLICATION_CREDENTIALS=/ruta/a/credenciales.json
```

---

## 🔐 Notas de Seguridad

1. **Validación de Datos**
   - Se validan los campos requeridos (CorredorId, CarreraId)
   - Se verifica que el corredor y carrera existan

2. **Respaldo en Bucket**
   - Los datos se guardan automáticamente antes de actualizar
   - Permite recuperación si falla Firestore

3. **Logging**
   - Se registran todos los eventos
   - Útil para debugging y auditoría

---

## 📝 Cambios en UI (Info_Carrera.razor)

### Nuevas Características
1. **Indicador de Estado**: Muestra si la carrera está "En Progreso" o "Terminada"
2. **Barra de Progreso**: Visualiza % de checkpoints completados por corredor
3. **Color de Fila**: Filas verdes para corredores que terminaron

```html
<!-- Indicador de Estado -->
<span style="color: green; font-weight: bold;">✓ TERMINADA</span>

<!-- Barra de Progreso -->
<div style="width: @progreso%; background-color: #4caf50; height: 20px;">
  @progreso%
</div>
```

---

## ✅ Testing

### Requisitos Previos
1. Carrera creada en Firestore con `CantSecciones` definidas
2. Corredores inscritos en la carrera
3. Registros creados para los corredores
4. Servidor ejecutándose

### Pasos de Prueba

1. **Verificar Estado Inicial**
```powershell
curl -X GET "https://localhost:7174/api/sensor/estado-carrera/maraton-2024" -SkipCertificateCheck
```

2. **Ejecutar Simulador**
```powershell
.\sensor-simulator.ps1 -CarreraId "maraton-2024" -CorredoresIds @("12345678", "87654321") -CantSecciones 3
```

3. **Verificar Estado Final**
```powershell
curl -X GET "https://localhost:7174/api/sensor/estado-carrera/maraton-2024" -SkipCertificateCheck
```

4. **Revisar en Info_Carrera**
   - Navegar a `/info_carreras`
   - Seleccionar la carrera
   - Verificar que muestre "TERMINADA" y progreso al 100%

---

## 🐛 Troubleshooting

| Error | Causa | Solución |
|-------|-------|----------|
| "Carrera no encontrada" | ID de carrera incorrecto | Verificar ID en Firestore |
| "No se encontró registro" | Corredor no inscrito | Crear registro manualmente |
| "Error al conectar Bucket" | Google Cloud no configurado | Verificar credenciales |
| "HTTPS certificate error" | Certificado auto-firmado | Usar `-SkipCertificateCheck` en PowerShell |

---

## 📚 Referencias

- [Google Cloud Storage .NET Client](https://cloud.google.com/dotnet/docs/reference/Google.Cloud.Storage.V1/latest)
- [Firestore .NET SDK](https://firebase.google.com/docs/firestore)
- [ASP.NET Core APIs](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/apis)

