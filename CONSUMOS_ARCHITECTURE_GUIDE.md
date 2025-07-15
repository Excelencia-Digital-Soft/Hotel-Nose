# ✅ ConsumosService - Arquitectura Corregida

## 🏗️ **Estructura de Base de Datos Correcta**

### **📊 Relaciones de Tablas:**

```
Movimientos (Facturación)
    ↓ (1:N)
Consumo (Detalle de consumos)
    ↓ (N:1)
Articulos (Catálogo de productos)
    ↓ (1:N)
Inventarios (Por habitación) / InventarioGeneral (Institucional)
```

### **🔗 Entidades Principales:**

#### **1. Movimientos** - Registro de facturación de la reserva
```csharp
public class Movimientos
{
    public int MovimientosId { get; set; }
    public int? VisitaId { get; set; }              // Vincula con la visita
    public int? HabitacionId { get; set; }          // Habitación de la reserva
    public decimal? TotalFacturado { get; set; }    // Total a facturar
    public DateTime? FechaInicio { get; set; }
    public DateTime? FechaRegistro { get; set; }
    public bool? Anulado { get; set; }
    public int InstitucionID { get; set; }
    
    // Navegación
    public virtual ICollection<Consumo> Consumo { get; set; }
}
```

#### **2. Consumo** - Detalle de artículos consumidos
```csharp
public class Consumo
{
    public int ConsumoId { get; set; }
    public int? MovimientosId { get; set; }         // FK a Movimientos
    public int? ArticuloId { get; set; }            // FK a Articulos
    public int? Cantidad { get; set; }
    public decimal? PrecioUnitario { get; set; }
    public bool? EsHabitacion { get; set; }         // true = inventario habitación, false = general
    public bool? Anulado { get; set; }
    
    // Navegación
    public virtual Movimientos? Movimientos { get; set; }
    public virtual Articulos? Articulo { get; set; }
}
```

#### **3. Articulos** - Catálogo de productos
```csharp
public class Articulos
{
    public int ArticuloId { get; set; }
    public string? NombreArticulo { get; set; }
    public decimal Precio { get; set; }
    public int InstitucionID { get; set; }
    
    // Navegación
    public virtual ICollection<Consumo> Consumo { get; set; }
    public virtual ICollection<Inventarios> Inventarios { get; set; }        // Inventario por habitación
    public virtual ICollection<InventarioGeneral> InventarioGeneral { get; set; } // Inventario general
}
```

#### **4. Inventarios** - Inventario por habitación
```csharp
public class Inventarios
{
    public int InventarioId { get; set; }
    public int? HabitacionId { get; set; }          // Específico de habitación
    public int? ArticuloId { get; set; }
    public int? Cantidad { get; set; }
    public int InstitucionID { get; set; }
}
```

#### **5. InventarioGeneral** - Inventario institucional
```csharp
public class InventarioGeneral
{
    public int InventarioId { get; set; }
    public int? ArticuloId { get; set; }
    public int? Cantidad { get; set; }               // Sin HabitacionId = general
    public int InstitucionID { get; set; }
}
```

## 🔄 **Flujo de Operaciones Corregido**

### **1. Agregar Consumos Generales:**
```csharp
1. Obtener o crear Movimiento para la visita
2. Crear registro en Consumo (EsHabitacion = false)
3. Actualizar InventarioGeneral (reducir stock)
4. Crear MovimientosStock (registro de movimiento)
```

### **2. Agregar Consumos de Habitación:**
```csharp
1. Obtener o crear Movimiento para la visita
2. Crear registro en Consumo (EsHabitacion = true)
3. Actualizar Inventarios de la habitación específica (reducir stock)
4. Crear MovimientosStock (registro de movimiento)
```

### **3. Anular Consumo:**
```csharp
1. Marcar Consumo.Anulado = true
2. Restaurar inventario (general o de habitación según corresponda)
3. Crear MovimientosStock de reversión
```

### **4. Actualizar Cantidad:**
```csharp
1. Calcular diferencia de cantidad
2. Actualizar Consumo.Cantidad
3. Ajustar inventario con la diferencia
4. Crear MovimientosStock del ajuste
```

## 🚀 **Mejoras Implementadas**

### **✅ Rendimiento:**
- **AsNoTracking()** en consultas de solo lectura
- Transacciones solo cuando es necesario
- Consultas optimizadas con Include

### **✅ Gestión de Inventario:**
- **Inventario dual**: Por habitación y general
- **Control de stock**: Previene cantidades negativas
- **Trazabilidad**: MovimientosStock registra todos los movimientos
- **Reversión**: Restaura inventario al anular consumos

### **✅ Integridad de Datos:**
- **Transacciones**: Operaciones atómicas
- **Validaciones**: Verificación de existencia de entidades
- **Soft Delete**: Usa Anulado en lugar de eliminar registros
- **Audit Trail**: FechaRegistro y UsuarioId en cambios

### **✅ Logging y Monitoreo:**
- **Logging estructurado** con niveles apropiados
- **Métricas de rendimiento** implícitas
- **Manejo de errores** comprehensivo

## 📝 **Diferencias con la Implementación Anterior**

| Aspecto | ❌ Anterior (Incorrecto) | ✅ Corregido |
|---------|-------------------------|-------------|
| **Tabla Principal** | ConsumoGeneral | Movimientos → Consumo |
| **Estructura** | Tabla única plana | Relación jerárquica |
| **Inventario** | No diferenciaba tipos | Dual: Habitación/General |
| **Trazabilidad** | Limitada | MovimientosStock completa |
| **Reversión** | Campo Activo | Anulado + restaurar inventario |
| **Rendimiento** | Sin optimización | AsNoTracking en lecturas |

## 🎯 **Endpoints Actualizados**

Los endpoints del controlador **no cambian**, pero ahora funcionan con la arquitectura correcta:

- `GET /api/v1/consumos/visita/{visitaId}` - Usa Movimientos → Consumo → Articulos
- `POST /api/v1/consumos/general` - Crea Movimiento y actualiza InventarioGeneral
- `POST /api/v1/consumos/room` - Crea Movimiento y actualiza Inventarios específicos
- `DELETE /api/v1/consumos/{consumoId}` - Anula y restaura inventario correspondiente
- `PUT /api/v1/consumos/{consumoId}` - Actualiza cantidad y ajusta inventario

## 🔍 **Casos de Uso Soportados**

### **Escenario 1: Consumo de Minibar (Habitación)**
```
Cliente consume cerveza del minibar habitación 101
→ Crea Consumo vinculado a Movimiento de la visita
→ Reduce Inventarios.Cantidad para ArticuloId=cerveza, HabitacionId=101
→ Registra MovimientosStock de egreso
```

### **Escenario 2: Consumo de Restaurant (General)**
```
Cliente pide comida del restaurant
→ Crea Consumo vinculado a Movimiento de la visita
→ Reduce InventarioGeneral.Cantidad para ArticuloId=comida
→ Registra MovimientosStock de egreso del inventario general
```

### **Escenario 3: Anulación de Consumo**
```
Cliente devuelve item no consumido
→ Marca Consumo.Anulado = true
→ Restaura inventario (habitación o general según EsHabitacion)
→ Registra MovimientosStock de ingreso de reversión
```

Esta arquitectura corregida proporciona un sistema robusto, auditable y escalable para la gestión de consumos en el hotel. ✨