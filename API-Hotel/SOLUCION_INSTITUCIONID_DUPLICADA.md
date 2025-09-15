# Solución: Error "El nombre de columna 'InstitucionID' se ha especificado más de una vez"

## 🎯 **Problema Identificado**

El error ocurre porque Entity Framework estaba intentando mapear `InstitucionID` dos veces:

1. **Modelo `Institucion`** tiene `InstitucionId` (con 'd' minúscula)
2. **Tablas de DB** esperan `InstitucionID` (con 'D' mayúscula)
3. **Modelos relacionados** tienen `InstitucionID` (con 'D' mayúscula)

Esto causaba que EF intentara crear una shadow property `InstitucionId` para las relaciones de navegación, duplicando el mapeo con la propiedad existente `InstitucionID`.

## ✅ **Solución Aplicada**

### **1. Configuración de InstitucionConfiguration**

Se agregó el mapeo explícito de la columna:

```csharp
public class InstitucionConfiguration : IEntityTypeConfiguration<Institucion>
{
    public void Configure(EntityTypeBuilder<Institucion> builder)
    {
        builder.HasKey(e => e.InstitucionId);
        builder.ToTable("Instituciones");
        
        // Mapear la propiedad InstitucionId a la columna InstitucionID
        builder.Property(e => e.InstitucionId)
            .HasColumnName("InstitucionID");
    }
}
```

### **2. Corrección de Nombres de Constraints**

Se actualizaron los nombres de las foreign key constraints para que coincidan con los de la base de datos:

- **VisitasConfiguration**: `FK_Visitas_Instituciones` → `FKVisitas_Institucion`
- **ReservasConfiguration**: `FK_Reservas_Instituciones` → `FKReservas_Institucion`

### **3. Configuraciones Completas**

Las configuraciones ahora mapean correctamente:

#### **VisitasConfiguration:**
- `VisitaId` → `VisitaID`
- `InstitucionID` → `InstitucionID`
- `HabitacionId` → `HabitacionId` (nueva columna)

#### **ReservasConfiguration:**
- `ReservaId` → `ReservaID`
- `HabitacionId` → `HabitacionID`
- `VisitaId` → `VisitaID`
- `InstitucionID` → `InstitucionID`
- `PromocionId` → `PromocionID`
- `MovimientoId` → `MovimientoID`

#### **MovimientosConfiguration:**
- `MovimientosId` → `MovimientosID`
- `HabitacionId` → `HabitacionID`
- `VisitaId` → `VisitaID`
- `PagoId` → `PagoID`
- `InstitucionID` → `InstitucionID`

## 🔍 **Causa Raíz**

El problema ocurría porque:

1. **Convención de nombres inconsistente**: El modelo `Institucion` usa `InstitucionId` pero las tablas usan `InstitucionID`
2. **Shadow Properties**: EF creaba propiedades ocultas para las relaciones de navegación
3. **Mapeo duplicado**: Se intentaba mapear la misma columna dos veces con diferentes configuraciones

## ✨ **Resultado**

- ✅ No más errores de columna duplicada
- ✅ Las relaciones de navegación funcionan correctamente
- ✅ Los INSERTs y UPDATEs se ejecutan sin problemas
- ✅ La aplicación compila y ejecuta correctamente

## 📝 **Recomendación**

Para evitar futuros problemas similares:

1. **Estandarizar nombres**: Usar siempre el mismo casing (`Id` o `ID`) en todo el proyecto
2. **Configuración explícita**: Siempre especificar nombres de columnas cuando difieren del modelo
3. **Evitar shadow properties**: Ser explícito con las foreign keys en las relaciones