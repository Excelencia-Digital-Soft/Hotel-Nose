# 🤖 Claude Development Guidelines - Hotel Management System

## 📋 **Table of Contents**
- [Overview](#overview)
- [API Architecture](#api-architecture)
- [Development Standards](#development-standards)
- [Database Guidelines](#database-guidelines)
- [Testing Guidelines](#testing-guidelines)
- [Migration Strategy](#migration-strategy)
- [Common Commands](#common-commands)

---

## 🎯 **Overview**

This is a **Hotel Management System** built with **ASP.NET Core** and **Entity Framework Core**, featuring:
- **Multi-tenant architecture** (multiple hotels/institutions)
- **RESTful API** with versioning (V1)
- **JWT Authentication** with role-based authorization
- **Clean Architecture** with separation of concerns
- **Real-time features** with SignalR
- **Comprehensive logging** and monitoring

---

## 🏗️ **API Architecture**

### **🔄 API Versioning Strategy**

#### **V1 Controllers (Modern - Preferred)**
- **Location**: `/Controllers/V1/`
- **Route Pattern**: `api/v1/{resource}`
- **Response**: Always use `ApiResponse<T>`
- **Authentication**: Bearer Token required
- **Standards**: Follow RESTful conventions

#### **Legacy Controllers (Deprecated)**
- **Location**: `/Controllers/`
- **Route Pattern**: Various patterns (inconsistent)
- **Response**: Custom `Respuesta` objects
- **Authentication**: `[AllowAnonymous]` (security issue)
- **Status**: Mark as `[Obsolete]` when replacing

### **📊 RESTful API Standards**

| HTTP Method | Purpose | Example Endpoint | Response |
|-------------|---------|------------------|----------|
| `GET` | Retrieve resource(s) | `GET /api/v1/habitaciones` | `ApiResponse<IEnumerable<T>>` |
| `GET` | Retrieve single resource | `GET /api/v1/habitaciones/{id}` | `ApiResponse<T>` |
| `POST` | Create new resource | `POST /api/v1/habitaciones` | `ApiResponse<T>` (201 Created) |
| `PUT` | Update entire resource | `PUT /api/v1/habitaciones/{id}` | `ApiResponse<T>` |
| `PATCH` | Partial update | `PATCH /api/v1/habitaciones/{id}/status` | `ApiResponse<T>` |
| `DELETE` | Delete resource | `DELETE /api/v1/habitaciones/{id}` | `ApiResponse` |

### **📦 ApiResponse Structure**

**ALL V1 endpoints MUST use ApiResponse:**

```csharp
// For data responses
public class ApiResponse<T>
{
    public bool IsSuccess { get; set; }
    public T? Data { get; set; }
    public List<string> Errors { get; set; } = new();
    public string? Message { get; set; }
}

// For non-data responses
public class ApiResponse : ApiResponse<object>
{
    // Inherits all properties from generic version
}

// Usage examples:
return Ok(ApiResponse<HabitacionDto>.Success(habitacion));
return BadRequest(ApiResponse.Failure("Validation failed"));
return NotFound(ApiResponse.Failure("Resource not found"));
```

---

## 🛠️ **Development Standards**

### **🏛️ Clean Architecture Structure**

```
API-Hotel/
├── Controllers/V1/          # Modern REST controllers
├── Controllers/             # Legacy controllers (mark obsolete)
├── Services/                # Business logic implementation
├── Interfaces/              # Service contracts
├── DTOs/                    # Data Transfer Objects
│   ├── Common/             # Shared DTOs (ApiResponse, etc.)
│   ├── Habitaciones/       # Room-related DTOs
│   ├── Reservas/           # Reservation DTOs
│   ├── Consumos/           # Consumption DTOs
│   └── Promociones/        # Promotion DTOs
├── Models/                 # Entity Framework models
├── Data/                   # DbContext and configurations
└── Extensions/             # Service registration extensions
```

### **🔧 Service Layer Pattern**

**When creating new features, ALWAYS follow this pattern:**

#### **1. Create Interface**
```csharp
// File: /Interfaces/IExampleService.cs
public interface IExampleService
{
    Task<ApiResponse<IEnumerable<ExampleDto>>> GetAllAsync(
        int institucionId, 
        CancellationToken cancellationToken = default);
    
    Task<ApiResponse<ExampleDto>> GetByIdAsync(
        int id, 
        int institucionId, 
        CancellationToken cancellationToken = default);
    
    Task<ApiResponse<ExampleDto>> CreateAsync(
        ExampleCreateDto createDto, 
        int institucionId, 
        CancellationToken cancellationToken = default);
    
    Task<ApiResponse<ExampleDto>> UpdateAsync(
        int id, 
        ExampleUpdateDto updateDto, 
        int institucionId, 
        CancellationToken cancellationToken = default);
    
    Task<ApiResponse> DeleteAsync(
        int id, 
        int institucionId, 
        CancellationToken cancellationToken = default);
}
```

#### **2. Create DTOs**
```csharp
// File: /DTOs/Example/ExampleDto.cs
public class ExampleDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    // ... other properties
}

public class ExampleCreateDto
{
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;
    // ... validation attributes
}

public class ExampleUpdateDto
{
    [StringLength(100)]
    public string? Name { get; set; }
    // ... optional fields for partial updates
}
```

#### **3. Implement Service**
```csharp
// File: /Services/ExampleService.cs
public class ExampleService : IExampleService
{
    private readonly HotelDbContext _context;
    private readonly ILogger<ExampleService> _logger;

    public ExampleService(HotelDbContext context, ILogger<ExampleService> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<ApiResponse<IEnumerable<ExampleDto>>> GetAllAsync(
        int institucionId, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            var items = await _context.Examples
                .AsNoTracking() // ALWAYS use for read-only queries
                .Where(e => e.InstitucionID == institucionId)
                .Select(e => new ExampleDto
                {
                    Id = e.Id,
                    Name = e.Name
                })
                .ToListAsync(cancellationToken);

            _logger.LogInformation("Retrieved {Count} examples for institution {InstitucionId}", 
                items.Count, institucionId);

            return ApiResponse<IEnumerable<ExampleDto>>.Success(items);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving examples for institution {InstitucionId}", institucionId);
            return ApiResponse<IEnumerable<ExampleDto>>.Failure(
                "Error retrieving examples", 
                "An error occurred while retrieving the examples");
        }
    }

    // ... implement other methods
}
```

#### **4. Create V1 Controller**
```csharp
// File: /Controllers/V1/ExamplesController.cs
[ApiController]
[Route("api/v1/examples")]
[Produces("application/json")]
[Authorize(AuthenticationSchemes = "Bearer")]
public class ExamplesController : ControllerBase
{
    private readonly IExampleService _exampleService;
    private readonly ILogger<ExamplesController> _logger;

    public ExamplesController(IExampleService exampleService, ILogger<ExamplesController> logger)
    {
        _exampleService = exampleService ?? throw new ArgumentNullException(nameof(exampleService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<ExampleDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<IEnumerable<ExampleDto>>>> GetExamples(
        CancellationToken cancellationToken = default)
    {
        var institucionId = GetCurrentInstitucionId();
        if (!institucionId.HasValue)
        {
            return BadRequest(ApiResponse.Failure("Institution ID is required"));
        }

        var result = await _exampleService.GetAllAsync(institucionId.Value, cancellationToken);
        return result.IsSuccess ? Ok(result) : StatusCode(500, result);
    }

    // ... other endpoints

    #region Private Methods
    private int? GetCurrentInstitucionId()
    {
        var institucionIdClaim = User.FindFirstValue("InstitucionId");
        if (!string.IsNullOrEmpty(institucionIdClaim) && int.TryParse(institucionIdClaim, out int institucionId))
        {
            return institucionId;
        }
        
        _logger.LogWarning("Institution ID not found in user claims for user {UserId}", 
            User.FindFirstValue(ClaimTypes.NameIdentifier));
        
        return null;
    }
    #endregion
}
```

#### **5. Register Service**
```csharp
// File: /Extensions/ServiceCollectionExtensions.cs
public static IServiceCollection AddApplicationServices(this IServiceCollection services)
{
    // ... existing services
    services.AddScoped<IExampleService, ExampleService>();
    return services;
}
```

#### **6. Mark Legacy Endpoint as Obsolete**
```csharp
// File: /Controllers/LegacyController.cs
[HttpGet]
[Route("OldEndpoint")]
[AllowAnonymous]
[Obsolete("This endpoint is deprecated. Use /api/v1/examples instead.")]
public async Task<Respuesta> OldMethod()
{
    // ... legacy implementation
}
```

---

## 🗄️ **Database Guidelines**

### **🔍 Entity Relationship Patterns**

#### **Core Entities and Relationships:**
```
Instituciones (Multi-tenant root)
├── CategoriasHabitaciones (Room categories)
│   ├── Habitaciones (Rooms)
│   └── Promociones (Promotions)
├── Visitas (Guest visits)
│   └── Reservas (Reservations)
├── Movimientos (Billing movements)
│   └── Consumo (Consumption details)
│       └── Articulos (Articles/Products)
└── Inventarios (Room inventory) / InventarioGeneral (Institution inventory)
```

#### **Field Naming Conventions:**
- **Primary Keys**: `EntityNameId` (e.g., `HabitacionId`, `ReservaId`)
- **Foreign Keys**: `EntityNameId` or `EntityNameID` (check existing schema)
- **Institution Field**: `InstitucionID` (consistent across all entities)
- **Soft Delete**: Use `Anulado` field (bool?) - `true` = deleted, `null/false` = active
- **Audit Fields**: `FechaRegistro`, `UsuarioId` (where available)

#### **Multi-Tenancy Pattern:**
```csharp
// ALL entities MUST respect institution isolation
var items = await _context.Examples
    .Where(e => e.InstitucionID == institucionId) // ALWAYS filter by institution
    .ToListAsync();
```

### **⚡ Performance Guidelines**

#### **Query Optimization:**
```csharp
// ✅ ALWAYS use AsNoTracking for read-only queries
var items = await _context.Examples
    .AsNoTracking()
    .Where(e => e.InstitucionID == institucionId)
    .ToListAsync();

// ✅ Use Include for needed relationships only
var items = await _context.Examples
    .AsNoTracking()
    .Include(e => e.Category)
    .Where(e => e.InstitucionID == institucionId)
    .ToListAsync();

// ✅ Project to DTOs to reduce data transfer
var items = await _context.Examples
    .AsNoTracking()
    .Where(e => e.InstitucionID == institucionId)
    .Select(e => new ExampleDto
    {
        Id = e.Id,
        Name = e.Name
        // Only select needed fields
    })
    .ToListAsync();
```

#### **Transaction Guidelines:**
```csharp
// ✅ Use transactions for multi-table operations
using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
try
{
    // Multiple operations
    await _context.SaveChangesAsync(cancellationToken);
    await transaction.CommitAsync(cancellationToken);
}
catch (Exception)
{
    await transaction.RollbackAsync(cancellationToken);
    throw;
}
```

---

## 🧪 **Testing Guidelines**

### **🔍 Before Making Changes**

#### **Test Commands (when available):**
```bash
# Run unit tests
dotnet test

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"

# Run integration tests
dotnet test --filter Category=Integration

# Build and check for warnings
dotnet build --verbosity normal
```

#### **Manual Testing Checklist:**
1. **Authentication**: Verify Bearer token is required for V1 endpoints
2. **Multi-tenancy**: Test with different `InstitucionId` values
3. **Validation**: Test with invalid data to verify error responses
4. **Performance**: Check for N+1 query issues with multiple records
5. **Transactions**: Verify data consistency in multi-step operations

### **🏥 Health Checks**
```csharp
// Every V1 controller should have a health endpoint
[HttpGet("health")]
[AllowAnonymous]
[ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
public IActionResult Health()
{
    return Ok(new 
    { 
        service = "ExampleService V1", 
        status = "healthy", 
        timestamp = DateTime.UtcNow,
        version = "1.0.0"
    });
}
```

---

## 🔄 **Migration Strategy**

### **📋 Legacy to V1 Migration Process**

#### **Step 1: Identify Legacy Endpoint**
```csharp
// Legacy endpoint example
[HttpPost]
[Route("CreateExample")]
[AllowAnonymous]
public async Task<Respuesta> CreateExample([FromBody] ExampleModel model)
{
    // Legacy implementation
}
```

#### **Step 2: Create V1 Equivalent**
1. Create interface (`IExampleService`)
2. Create DTOs (`ExampleDto`, `ExampleCreateDto`, etc.)
3. Implement service (`ExampleService`)
4. Create V1 controller (`ExamplesController`)
5. Register service in DI container

#### **Step 3: Mark Legacy as Obsolete**
```csharp
[HttpPost]
[Route("CreateExample")]
[AllowAnonymous]
[Obsolete("This endpoint is deprecated. Use POST /api/v1/examples instead.")]
public async Task<Respuesta> CreateExample([FromBody] ExampleModel model)
{
    // Legacy implementation remains for compatibility
}
```

#### **Step 4: Update Documentation**
- Add new endpoint to API documentation
- Update client applications gradually
- Monitor usage of legacy endpoints

### **🔄 Response Migration**
```csharp
// ❌ Legacy Response
public class Respuesta
{
    public bool Exito { get; set; }
    public string Mensaje { get; set; }
    public object Datos { get; set; }
}

// ✅ V1 Response
public class ApiResponse<T>
{
    public bool IsSuccess { get; set; }
    public T? Data { get; set; }
    public List<string> Errors { get; set; } = new();
    public string? Message { get; set; }
}
```

---

## 💻 **Common Commands**

### **🏗️ Development Commands**
```bash
# Restore packages
dotnet restore

# Build solution
dotnet build

# Run application
dotnet run --project API-Hotel

# Watch for changes (development)
dotnet watch run --project API-Hotel

# Format code
dotnet format
```

### **📊 Database Commands**
```bash
# Add migration
dotnet ef migrations add MigrationName --project API-Hotel

# Update database
dotnet ef database update --project API-Hotel

# View pending migrations
dotnet ef migrations list --project API-Hotel

# Generate SQL script
dotnet ef migrations script --project API-Hotel
```

### **📦 Package Management**
```bash
# Add package
dotnet add package PackageName

# Remove package
dotnet remove package PackageName

# List packages
dotnet list package

# Update packages
dotnet add package PackageName
```

---

## 🎯 **Quick Reference**

### **✅ Do's**
- ✅ Always use `ApiResponse<T>` for V1 endpoints
- ✅ Use `AsNoTracking()` for read-only queries
- ✅ Implement proper logging with structured data
- ✅ Validate DTOs with data annotations
- ✅ Use transactions for multi-table operations
- ✅ Filter by `InstitucionID` for multi-tenancy
- ✅ Follow RESTful conventions
- ✅ Mark legacy endpoints as `[Obsolete]`

### **❌ Don'ts**
- ❌ Don't create endpoints without authentication in V1
- ❌ Don't use `Respuesta` class in V1 endpoints
- ❌ Don't forget to register services in DI container
- ❌ Don't ignore institution-level filtering
- ❌ Don't create hard deletes (use soft delete with `Anulado`)
- ❌ Don't return entities directly (use DTOs)
- ❌ Don't forget error handling and logging

---

## 📞 **Support**

For questions about this codebase:
1. Check existing V1 implementations for patterns
2. Review the architecture guides in the repository
3. Ensure all new code follows these guidelines
4. Test thoroughly before deployment

**Remember**: This is a production hotel management system. Always prioritize data consistency, security, and performance. 🏨✨