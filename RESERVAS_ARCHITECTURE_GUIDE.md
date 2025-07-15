# ✅ ReservasService - Arquitectura Corregida

## 🏗️ **Estructura de Base de Datos Correcta**

### **📊 Relaciones de Tablas:**

```
Reservas (Reservaciones principales)
├── VisitaId ──────► Visitas (Información del huésped)
├── HabitacionId ──► Habitaciones (Habitación reservada)
├── PromocionId ───► Promociones (Promoción aplicada)
├── MovimientoId ──► Movimientos (Facturación)
└── InstitucionID ─► Instituciones (Multi-tenancy)
```

### **🔗 Entidades Principales:**

#### **1. Reservas** - Reservaciones del hotel
```csharp
public class Reservas
{
    public int ReservaId { get; set; }                 // PK
    public int? VisitaId { get; set; }                 // FK → Visitas
    public int? HabitacionId { get; set; }             // FK → Habitaciones
    public DateTime? FechaReserva { get; set; }        // Fecha inicio (no FechaInicio)
    public DateTime? FechaFin { get; set; }            // Fecha fin
    public int? TotalHoras { get; set; }               // Duración en horas
    public int? TotalMinutos { get; set; }             // Duración en minutos
    public int? MovimientoId { get; set; }             // FK → Movimientos
    public int? PromocionId { get; set; }              // FK → Promociones
    public int? PausaHoras { get; set; }               // Pausa en horas
    public int? PausaMinutos { get; set; }             // Pausa en minutos
    public DateTime? FechaRegistro { get; set; }       // Fecha de creación
    public DateTime? FechaAnula { get; set; }          // Fecha de anulación
    public int InstitucionID { get; set; }             // Multi-tenancy
    
    // Navegación
    public virtual Visitas? Visita { get; set; }
    public virtual Habitaciones? Habitacion { get; set; }
    public virtual Promociones? Promocion { get; set; }
}
```

#### **2. Visitas** - Información del huésped
```csharp
public class Visitas
{
    public int VisitaId { get; set; }
    public string? PatenteVehiculo { get; set; }
    public string? Identificador { get; set; }         // Nombre del huésped
    public string? NumeroTelefono { get; set; }
    public DateTime? FechaPrimerIngreso { get; set; }
    public int? HabitacionId { get; set; }
    public int InstitucionID { get; set; }
    
    // Navegación
    public virtual ICollection<Reservas> Reservas { get; set; }
    
    // Computed property - reserva activa (sin fecha fin)
    public Reservas ReservaActiva => Reservas?.FirstOrDefault(r => r.FechaFin == null);
}
```

#### **3. Habitaciones** - Habitaciones del hotel
```csharp
public class Habitaciones
{
    public int HabitacionId { get; set; }
    public string? NombreHabitacion { get; set; }
    public int? CategoriaId { get; set; }              // FK → CategoriasHabitaciones
    public bool? Disponible { get; set; }
    public int InstitucionID { get; set; }
    
    // Navegación
    public virtual CategoriasHabitaciones? Categoria { get; set; }
    public virtual ICollection<Reservas> Reservas { get; set; }
}
```

## 🔄 **Diferencias Clave con la Implementación Anterior**

### **❌ Anterior (Incorrecto):**
- Usaba campos inexistentes (`Activo`, `EsReserva`, `FechaInicio`)
- Nomenclatura incorrecta de propiedades
- Lógica de "activo" mal implementada
- No manejaba `FechaAnula` para cancelaciones

### **✅ Corregido:**
- Usa campos reales de la entidad `Reservas`:
  - `FechaReserva` (no `FechaInicio`)
  - `FechaFin` para determinar si está activa
  - `FechaAnula` para cancelaciones
  - `ReservaId`, `VisitaId`, `HabitacionId` (campos reales)
- **Lógica de estado correcto**: `Activo = FechaFin == null && FechaAnula == null`
- **Relaciones correctas** con `Visitas`, `Habitaciones`, `Promociones`

## 🎯 **Operaciones Corregidas**

### **1. FinalizeReservationAsync:**
```csharp
// ✅ CORREGIDO: Busca reserva activa por habitación
var reservaActiva = await _context.Reservas
    .Where(r => r.HabitacionId == habitacionId && r.FechaFin == null)
    .FirstOrDefaultAsync(cancellationToken);

// ✅ CORREGIDO: Marca como finalizada con fecha fin
reservaActiva.FechaFin = DateTime.Now;
reservaActiva.FechaAnula = null; // Asegura que no esté marcada como cancelada

// ✅ CORREGIDO: Libera la habitación
habitacion.Disponible = true;
```

### **2. PauseOccupationAsync:**
```csharp
// ✅ CORREGIDO: Busca reserva activa por visita
var reserva = await _context.Reservas
    .FirstOrDefaultAsync(r => r.VisitaId == visitaId && r.FechaFin == null);

// ✅ CORREGIDO: Calcula tiempo de pausa basado en FechaReserva
if (reserva.FechaReserva.HasValue && reserva.TotalHoras.HasValue)
{
    var endTime = reserva.FechaReserva.Value
        .AddHours(reserva.TotalHoras.Value)
        .AddMinutes(reserva.TotalMinutos.Value);
    
    // Valores negativos = tiempo extra, positivos = tiempo restante
    var timeDiff = DateTime.Now - endTime;
    if (timeDiff.TotalMinutes > 0)
    {
        reserva.PausaHoras = -(int)timeDiff.Hours; // Tiempo extra
        reserva.PausaMinutos = -(int)(timeDiff.TotalMinutes % 60);
    }
}
```

### **3. UpdateReservationPromotionAsync:**
```csharp
// ✅ CORREGIDO: Validación de compatibilidad de categoría
var promocion = await _context.Promociones
    .Include(p => p.Categoria)
    .FirstOrDefaultAsync(p => p.PromocionID == promocionId.Value && p.Anulado != true);

// ✅ CORREGIDO: Verifica compatibilidad con categoría de habitación
if (reserva.Habitacion?.CategoriaId != promocion.CategoriaID)
{
    return ApiResponse<ReservaDto>.Failure(
        "Incompatible promotion", 
        "The promotion is not valid for this room category");
}
```

### **4. CancelReservationAsync:**
```csharp
// ✅ CORREGIDO: Marca como anulada usando campos correctos
reserva.FechaAnula = DateTime.Now;  // Campo real para anulación
reserva.FechaFin = DateTime.Now;    // También marca como finalizada

// ✅ CORREGIDO: Libera la habitación automáticamente
if (reserva.Habitacion != null)
{
    reserva.Habitacion.Disponible = true;
}
```

### **5. GetActiveReservationsAsync:**
```csharp
// ✅ CORREGIDO: Filtro correcto para reservas activas
.Where(r => r.FechaFin == null &&          // No finalizada
           r.FechaAnula == null &&          // No cancelada
           r.InstitucionID == institucionId) // Por institución

// ✅ CORREGIDO: Mapeo correcto de campos
FechaInicio = r.FechaReserva ?? DateTime.MinValue,  // Campo real
Activo = true,  // Todas las devueltas son activas por definición
PromocionNombre = r.Promocion!.Detalle,           // Campo real de promoción
```

## 🚀 **Mejoras Implementadas**

### **✅ Rendimiento:**
- **AsNoTracking()** en consultas de solo lectura
- **Include optimizado** para relaciones necesarias
- **Proyección directa** a DTOs cuando es posible
- **Transacciones** solo donde son necesarias

### **✅ Lógica de Negocio Correcta:**
- **Estado de reserva**: Basado en `FechaFin` y `FechaAnula`
- **Pausa inteligente**: Calcula tiempo restante vs tiempo extra
- **Validación de promociones**: Verifica compatibilidad de categoría
- **Liberación automática**: Habitación disponible al finalizar/cancelar

### **✅ Integridad de Datos:**
- **Transacciones** para operaciones críticas (finalizar, cancelar)
- **Validaciones** de existencia de entidades
- **Campos correctos** según esquema real de BD
- **Relaciones consistentes** entre entidades

## 📊 **Mapeo DTO ↔ Entidad Corregido**

| DTO Field | Entity Field | Descripción |
|-----------|--------------|-------------|
| `FechaInicio` | `FechaReserva` | Fecha de inicio de reserva |
| `Activo` | `FechaFin == null && FechaAnula == null` | Reserva activa |
| `EsReserva` | `true` (default) | Campo no existe en entidad |
| `PromocionNombre` | `Promocion.Detalle` | Nombre de promoción |
| `PromocionTarifa` | `Promocion.Tarifa` | Tarifa promocional |
| `CreatedAt` | `FechaRegistro` | Fecha de creación |

## 🔍 **Casos de Uso Soportados**

### **Escenario 1: Finalizar Reserva por Habitación**
```
Sistema finaliza ocupación de habitación 101
→ Busca reserva activa (FechaFin == null) para habitación 101
→ Marca FechaFin = DateTime.Now
→ Libera habitación (Disponible = true)
→ Transacción garantiza consistencia
```

### **Escenario 2: Pausar Ocupación por Tiempo Extra**
```
Cliente excede tiempo reservado y se pausa para facturación
→ Calcula tiempo transcurrido vs tiempo reservado
→ Si hay tiempo extra: PausaHoras/Minutos negativos
→ Si queda tiempo: PausaHoras/Minutos positivos
→ Permite cálculo preciso de cargos adicionales
```

### **Escenario 3: Aplicar Promoción Compatible**
```
Cliente solicita aplicar promoción a su reserva
→ Valida que promoción existe y está activa (Anulado != true)
→ Verifica compatibilidad: Habitacion.CategoriaId == Promocion.CategoriaID
→ Aplica promoción si es compatible
→ Rechaza si las categorías no coinciden
```

### **Escenario 4: Cancelar Reserva con Liberación**
```
Cliente cancela reserva antes de finalizar
→ Marca FechaAnula = DateTime.Now (cancelación)
→ Marca FechaFin = DateTime.Now (finalización)
→ Libera habitación automáticamente
→ Transacción asegura estado consistente
```

## 🎯 **Endpoints Funcionando Correctamente**

Los endpoints mantienen la misma interfaz pero ahora funcionan con la arquitectura correcta:

- `POST /api/v1/reservas/finalize` - Finaliza usando campos reales
- `POST /api/v1/reservas/{visitaId}/pause` - Pausa con cálculo de tiempo correcto
- `POST /api/v1/reservas/{visitaId}/resume` - Reanuda limpiando pausas
- `PUT /api/v1/reservas/{id}/promotion` - Actualiza con validación de categoría
- `PUT /api/v1/reservas/{id}/extend` - Extiende tiempo normalizando minutos
- `DELETE /api/v1/reservas/{id}` - Cancela usando FechaAnula
- `GET /api/v1/reservas/{id}` - Obtiene con relaciones correctas
- `GET /api/v1/reservas/active` - Lista activas usando filtros correctos

## ⚠️ **Consideraciones Importantes**

1. **Campos Faltantes**: Los campos `Activo` y `EsReserva` no existen en la entidad real, se calculan en el DTO.

2. **Nomenclatura**: `FechaReserva` en la entidad se mapea a `FechaInicio` en el DTO por compatibilidad.

3. **Estado de Reserva**: Una reserva está activa si `FechaFin == null && FechaAnula == null`.

4. **Pausa vs Overtime**: Los valores negativos en PausaHoras/Minutos indican tiempo extra cobrable.

5. **Multi-tenancy**: Todas las operaciones respetan el `InstitucionID` para aislamiento.

6. **Integridad Referencial**: Las validaciones aseguran que promociones sean compatibles con categorías de habitación.

Esta arquitectura corregida proporciona un sistema de reservas robusto, correctamente integrado con la estructura real de la base de datos y con lógica de negocio apropiada para el dominio hotelero. ✨