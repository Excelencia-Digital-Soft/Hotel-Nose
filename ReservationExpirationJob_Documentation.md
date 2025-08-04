# 🕐 ReservationExpirationJob - Sistema de Monitoreo de Reservas

## 🎯 Propósito

El `ReservationExpirationJob` es un trabajo programado (Cron Job) que monitorea las reservas activas y envía notificaciones automáticas cuando están próximas a vencer o han expirado.

---

## ⚙️ Configuración

### **Frecuencia de Ejecución**
```cron
*/5 * * * *  // Cada 5 minutos
```

### **Cron Expression Explicada**
- `*/5` = Cada 5 minutos
- `*` = Cualquier hora
- `*` = Cualquier día del mes  
- `*` = Cualquier mes
- `*` = Cualquier día de la semana

---

## 🚨 Tipos de Notificaciones

| Tiempo Restante | Tipo | Icono | Urgencia | Descripción |
|-----------------|------|-------|----------|-------------|
| **≤ 0 min** | `warning` | ⏰ | `critical` | Reserva expirada |
| **≤ 5 min** | `warning` | 🚨 | `critical` | Alerta crítica |
| **≤ 15 min** | `warning` | ⚠️ | `high` | Advertencia importante |
| **≤ 30 min** | `warning` | ℹ️ | `medium` | Información temprana |

---

## 📊 Lógica de Funcionamiento

### **1. Detección de Reservas Activas**
```sql
-- Criterios para reservas activas
WHERE 
    r.FechaFin == null AND        -- Sin fecha de finalización
    r.FechaAnula == null AND      -- No cancelada
    r.FechaReserva != null AND    -- Tiene fecha de inicio
    r.TotalHoras != null          -- Tiene duración definida
```

### **2. Cálculo de Tiempo de Expiración**
```csharp
// Tiempo base de la reserva
var totalMinutes = (reserva.TotalHoras ?? 0) * 60 + (reserva.TotalMinutos ?? 0);

// Agregar tiempo pausado si existe
var pausedMinutes = (reserva.PausaHoras ?? 0) * 60 + (reserva.PausaMinutos ?? 0);
totalMinutes += pausedMinutes;

// Calcular tiempo final
var endTime = startTime.AddMinutes(totalMinutes);
var timeRemaining = endTime - DateTime.Now;
```

### **3. Envío de Notificaciones**
- Usa `IReservationNotificationService.NotifyReservationWarningAsync()`
- Incluye datos contextuales (tiempo restante, urgencia, etc.)
- Se envía únicamente a la institución correspondiente

---

## 📋 Datos Incluidos en Notificaciones

### **Notificación de Reserva Expirada**
```json
{
  "type": "reservation_warning",
  "reservaId": 123,
  "habitacionNombre": "Habitación 101",
  "warningMessage": "⏰ Tiempo agotado - La reserva ha expirado",
  "additionalData": {
    "overdueMinutes": 15.5,
    "expectedEndTime": "2024-01-15T14:30:00",
    "actualTime": "2024-01-15T14:45:30"
  },
  "timestamp": "2024-01-15T14:45:30Z"
}
```

### **Notificación de Advertencia (5 min)**
```json
{
  "type": "reservation_warning",
  "reservaId": 123,
  "habitacionNombre": "Habitación 101", 
  "warningMessage": "🚨 Quedan 3 minutos",
  "additionalData": {
    "remainingMinutes": 3.2,
    "endTime": "2024-01-15T14:30:00",
    "urgency": "critical"
  },
  "timestamp": "2024-01-15T14:26:48Z"
}
```

---

## 🔍 Logging y Monitoreo

### **Logs de Información**
- Inicio de cada ejecución con timestamp
- Número de reservas activas encontradas
- Número de notificaciones enviadas por ejecución
- Detalles de cada notificación (ReservaId, habitación, tiempo restante)

### **Logs de Error**
- Errores por reserva individual (no interrumpe el procesamiento)
- Errores generales del job
- Excepciones de base de datos o servicios

### **Ejemplo de Logs**
```
[INFO] Starting reservation expiration check at 2024-01-15T14:25:00Z
[INFO] Found 12 active reservations to check
[INFO] Sent critical warning for ReservaId=123, Room=Habitación 101, Remaining=3min
[INFO] Sent warning for ReservaId=456, Room=Habitación 205, Remaining=12min
[INFO] Reservation expiration check completed. Processed 12 reservations, sent 5 notifications
```

---

## 🏗️ Arquitectura

### **Dependencias**
- `IServiceScopeFactory` - Para crear scope por ejecución
- `HotelDbContext` - Acceso a base de datos
- `IReservationNotificationService` - Envío de notificaciones
- `ILogger<ReservationExpirationJob>` - Logging estructurado

### **Patrón de Ejecución**
1. **Scope Creation** - Nuevo scope por cada ejecución
2. **Data Retrieval** - Consulta optimizada con `AsNoTracking()`
3. **Processing** - Iteración segura con manejo de errores por item
4. **Notification** - Uso del servicio especializado
5. **Cleanup** - Dispose automático del scope

---

## ⚡ Optimizaciones

### **Performance**
- Consulta `AsNoTracking()` para lecturas optimizadas
- Include específicos para evitar N+1 queries
- Procesamiento en lotes sin cargar todo en memoria

### **Reliability**
- Manejo de errores por reserva individual
- Logging detallado para debugging
- Uso de `CancellationToken` para shutdown graceful

### **Escalabilidad**
- Configuración por TimeZone
- Fácil modificación de intervalos de notificación
- Separation of concerns con servicios especializados

---

## 🔧 Configuración Personalizable

### **Intervalos de Notificación** (Modificables en código)
```csharp
// Personalizar los umbrales de tiempo
if (timeRemaining.TotalMinutes <= 0)      // Expirada
if (timeRemaining.TotalMinutes <= 5)      // Crítica
if (timeRemaining.TotalMinutes <= 15)     // Alta
if (timeRemaining.TotalMinutes <= 30)     // Media
```

### **Frecuencia de Ejecución** (Modificable en configuración)
```csharp
// En ServiceCollectionExtensions.cs
options.CronExpression = "*/5 * * * *";  // Cada 5 minutos
options.CronExpression = "*/2 * * * *";  // Cada 2 minutos
options.CronExpression = "*/10 * * * *"; // Cada 10 minutos
```

---

## 🎯 Beneficios

### **Para el Usuario**
- ✅ Notificaciones automáticas sin intervención manual
- ✅ Diferentes niveles de urgencia para mejor gestión
- ✅ Información contextual completa
- ✅ Notificaciones en tiempo real vía SignalR

### **Para el Sistema**
- ✅ Ejecución eficiente cada 5 minutos (no continua)
- ✅ Procesamiento resiliente con manejo de errores
- ✅ Logging completo para auditoría y debugging
- ✅ Arquitectura escalable y mantenible

---

## 🚀 Próximas Mejoras Posibles

1. **Configuración Dinámica** - Intervalos personalizables por institución
2. **Templates de Mensajes** - Mensajes configurables desde base de datos
3. **Snooze Functionality** - Posponer notificaciones por X minutos
4. **Escalation Rules** - Notificaciones a supervisores si no hay respuesta
5. **Analytics** - Métricas de tiempo promedio de respuesta

Este sistema proporciona una base sólida y escalable para el monitoreo automático de reservas! 🎉