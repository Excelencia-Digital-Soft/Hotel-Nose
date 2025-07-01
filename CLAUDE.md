# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Build and Development Commands

- **Build**: `dotnet build` - Builds the solution with multiple warnings but successful compilation
- **Run**: `dotnet run --project API-Hotel` - Starts the web API server
- **Restore packages**: `dotnet restore` - Restores NuGet packages
- **Target Framework**: .NET 9.0

## Architecture Overview

This is a **Hotel Management System** ASP.NET Core Web API built with Entity Framework and SQL Server. The system manages hotel rooms, reservations, payments, cash register operations, and inventory.

### Core Business Logic

**Room Management Flow**:
1. Rooms have availability status (`Disponible` field in `Habitaciones` table)
2. When reserved: `habitacion.Disponible = false` and `VisitaID` is set
3. **Critical Issue**: Payment processing (`PagosController.PagarVisita`) does NOT automatically update room status
4. Manual room release requires calling `ReservasController.FinalizarReserva`

**Payment & Cash Register Flow**:
- `PagosController` handles individual payments
- `CajaController.CierreCaja` processes cash register closing
- **Bug**: Rooms remain occupied after payment until manually released

### Key Controllers & Responsibilities

- **ReservasController**: Room reservations, availability management
- **PagosController**: Payment processing (missing room status updates)
- **CajaController**: Cash register operations, daily closing
- **HabitacionesController**: Room CRUD operations
- **VisitasController**: Customer visit management
- **StatisticsController**: Revenue and occupancy analytics
- **InventarioController**: Stock management for room supplies

### Database Context

- **HotelDbContext**: Main EF context with comprehensive entity mappings
- **Connection**: SQL Server via Entity Framework 7.0.0
- **Key Entities**: Habitaciones, Reservas, Visitas, Pagos, Movimientos, Cierre

### Authentication & Security

- JWT Bearer authentication configured
- API key middleware (`ApiKeyMiddleware`)
- CORS enabled for cross-origin requests
- Swagger/OpenAPI documentation with JWT support
- **Dual Login Support**: 
  - Email login: Input contains `@` symbol
  - Username login: Input without `@` tries username first, then `{username}@hotel.fake` format
  - Legacy user migration creates fake emails for username-only accounts

### Background Services

- **ReservationMonitorService**: Monitors reservation states
- **MySchedulerJob**: Cron job running daily at 8:20 AM
- **NotificationsHub**: SignalR for real-time notifications

### Common Patterns

- Controllers use dependency injection for DbContext
- AutoMapper for DTO mappings (`MappingProfile`)
- Consistent error handling with `Respuesta` model
- File uploads stored in `wwwroot/uploads/`

### Known Issues

1. **Room Status Bug**: Payment processing doesn't update room availability
2. **Nullable Reference Warnings**: Multiple CS8618 warnings throughout codebase
3. **Unused Variables**: Several CS0168 and CS0219 warnings
4. **Null Reference Checks**: Many CS8602 warnings for potential null dereferencing

### Development Notes

- Solution file: `API-Hotel.sln`
- Main project: `API-Hotel/hotel.csproj`
- Uses nullable reference types (enabled in project)
- XML documentation generation enabled
- Entity relationships extensively configured in `OnModelCreating`