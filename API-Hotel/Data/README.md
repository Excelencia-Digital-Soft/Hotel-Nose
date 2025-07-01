# HotelDbContext - Estructura y Organización

## 📁 Estructura de Archivos

```
Data/
├── HotelDbContext.Core.cs              # Clase principal del contexto
├── HotelDbContext.DbSets.cs            # Definición de DbSets organizados por categoría
├── HotelDbContext.LegacyConfigurations.cs # Configuraciones legacy inline
├── Configurations/                      # Configuraciones separadas por entidad
│   ├── ApplicationUserConfiguration.cs
│   ├── HabitacionesConfiguration.cs
│   ├── PagosConfiguration.cs
│   ├── ReservasConfiguration.cs
│   └── VisitasConfiguration.cs
└── README.md                           # Este archivo
```

## 🎯 Principios de Diseño

### 1. **Responsabilidad Única (SRP)**
- Cada archivo tiene una responsabilidad específica
- Las configuraciones de entidades están separadas en archivos individuales
- El contexto principal está dividido en partial classes

### 2. **Organización por Categorías**
Los DbSets están organizados en las siguientes categorías:

- **Identity**: Gestión de usuarios y roles (ASP.NET Core Identity)
- **Core Business**: Habitaciones, Visitas, Reservas
- **Financial**: Pagos, Movimientos, Cierres de caja
- **Inventory**: Gestión de inventario y stock
- **Administration**: Personal, Instituciones, Servicios
- **User Management (Legacy)**: Sistema de usuarios anterior

### 3. **Documentación XML**
- Todos los DbSets tienen comentarios XML
- Las clases de configuración están documentadas
- Los métodos principales incluyen descripciones

## 🔧 Cómo Agregar una Nueva Entidad

1. **Crear el modelo** en `Models/`
```csharp
public class NuevaEntidad
{
    public int Id { get; set; }
    // ... propiedades
}
```

2. **Agregar el DbSet** en `HotelDbContext.DbSets.cs` en la categoría apropiada:
```csharp
/// <summary>
/// Descripción de la entidad
/// </summary>
public virtual DbSet<NuevaEntidad> NuevasEntidades { get; set; }
```

3. **Crear la configuración** en `Configurations/NuevaEntidadConfiguration.cs`:
```csharp
public class NuevaEntidadConfiguration : IEntityTypeConfiguration<NuevaEntidad>
{
    public void Configure(EntityTypeBuilder<NuevaEntidad> builder)
    {
        // Configuración aquí
    }
}
```

## 🚀 Ventajas de esta Estructura

1. **Mantenibilidad**: Fácil localizar y modificar configuraciones
2. **Escalabilidad**: Agregar nuevas entidades sin modificar archivos grandes
3. **Claridad**: Separación clara de responsabilidades
4. **Testabilidad**: Configuraciones aisladas son más fáciles de probar
5. **Colaboración**: Menos conflictos en control de versiones

## 📝 Notas Importantes

- Las configuraciones se aplican automáticamente usando `ApplyConfigurationsFromAssembly`
- El contexto hereda de `IdentityDbContext` para integración con ASP.NET Core Identity
- Las tablas legacy (Usuarios, Roles) se mantienen para compatibilidad durante la migración

## 🔄 Migración desde el Contexto Anterior

El contexto anterior tenía todo en un solo archivo con más de 600 líneas. La nueva estructura:
- Divide las responsabilidades en archivos más pequeños
- Facilita el mantenimiento y la comprensión del código
- Permite trabajo en paralelo sin conflictos
- Mejora la documentación y organización del código