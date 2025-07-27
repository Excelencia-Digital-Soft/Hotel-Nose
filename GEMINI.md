
# Gemini Project Documentation: Hotel-Nose API

This document provides a guide for developers working on the Hotel-Nose API project. It outlines the project structure, conventions, and key architectural concepts.

## 1. Project Overview

Hotel-Nose is a comprehensive API for hotel management. It handles core functionalities like reservations, guest management (visitas), consumption tracking (minibar, restaurant), promotions, and inventory. The API is built with ASP.NET Core and follows RESTful principles for its V1 endpoints.

## 2. Tech Stack

-   **Backend:** C# with ASP.NET Core
-   **Database:** Microsoft SQL Server
-   **ORM:** Entity Framework Core
-   **Authentication:** JWT Bearer Tokens

## 3. Project Structure

The solution is organized into two main projects:

-   `API-Hotel/`: The main ASP.NET Core Web API project.
    -   `Controllers/`: Contains API controllers, with V1 endpoints in a dedicated `V1/` subfolder.
    -   `Data/`: Holds the `HotelDbContext` and Entity Framework configurations.
    -   `Models/`: Defines the database entities.
    -   `DTOs/`: Data Transfer Objects used for API requests and responses.
    -   `Services/`: Contains the business logic for different domains (Reservas, Consumos, etc.).
    -   `Interfaces/`: Defines the contracts for the services.
    -   `Extensions/`: Extension methods for service registration, authentication, etc.
    -   `appsettings.json`: Configuration file for the application.
-   `Scripts/`: Contains SQL scripts for database migrations and maintenance.

## 4. API Versioning & Migration

The project is currently migrating from legacy endpoints to a versioned V1 API.

-   **Legacy Endpoints:** Mixed conventions (e.g., `/GetConsumosVisita`).
-   **V1 Endpoints:** RESTful conventions (e.g., `GET /api/v1/consumos/visita/{visitaId}`).

All new development should use the V1 architecture. Refer to `MIGRATION_GUIDE.md` for detailed instructions on migrating frontend calls to the new endpoints.

## 5. Database

-   **ORM:** Entity Framework Core is used for data access.
-   **Schema:** The database schema uses `PascalCase` for table and column names. Foreign keys are typically named with an `ID` suffix (e.g., `VisitaID`).
-   **Configuration:** EF Core entity configurations are defined in `Data/Configurations/`.
-   **Migrations:** Manual SQL scripts are used for schema changes and are located in the `Scripts/` directory.

## 6. Key Architectural Concepts

The business logic is divided into several service domains.

### a. Reservas (Reservations)

-   **State Management:** A reservation is considered **active** if `FechaFin` and `FechaAnula` are `NULL`.
-   **Finalization:** Sets `FechaFin` to the current timestamp and marks the room as available.
-   **Cancellation:** Sets `FechaAnula` to the current timestamp.
-   **Promotions:** Promotions are validated against the room's category (`Habitacion.CategoriaId`).
-   **See:** `RESERVAS_ARCHITECTURE_GUIDE.md`

### b. Consumos (Consumptions)

-   **Dual Inventory:** The system manages two types of inventory:
    1.  **General Inventory:** For items from central stock (e.g., restaurant).
    2.  **Room Inventory:** For items in a specific room's minibar.
-   **Transactions:** Consumptions are recorded in the `Consumo` table, linked to a `Movimiento` (invoice).
-   **Traceability:** The `MovimientosStock` table tracks every change in inventory.
-   **See:** `CONSUMOS_ARCHITECTURE_GUIDE.md`

### c. Promociones (Promotions)

-   **Categorization:** Promotions are linked to `CategoriasHabitaciones` (Room Categories).
-   **Validation:** The system validates that a promotion is applicable to the category of the room in a reservation.
-   **Pricing:** The promotional price (`Promociones.Tarifa`) is applied instead of the category's standard price (`CategoriasHabitaciones.PrecioNormal`).
-   **See:** `PROMOCIONES_ARCHITECTURE_GUIDE.md`

## 7. Development Workflow

### Running the Application

1.  Ensure the database connection string in `appsettings.Development.json` is correctly configured.
2.  Run the application from the `API-Hotel` directory:
    ```bash
    dotnet run
    ```

### Database Migrations

1.  Create a new SQL script in the `Scripts/` directory.
2.  Write the necessary `ALTER TABLE`, `CREATE TABLE`, etc. statements.
3.  Execute the script against the target database.

## 8. Code Style & Conventions

-   **C#:** Follow standard .NET coding conventions (PascalCase for classes, methods, properties; camelCase for local variables).
-   **API Design:**
    -   Use plural nouns for resource names (e.g., `/api/v1/reservas`).
    -   Use HTTP verbs correctly (`GET`, `POST`, `PUT`, `DELETE`).
    -   Use kebab-case for URL segments.
-   **DTOs:** Use Data Transfer Objects to decouple the API layer from the database models.
-   **Responses:** All V1 API responses are wrapped in a standard `ApiResponse<T>` object:
    ```json
    {
      "isSuccess": true,
      "data": { ... },
      "errors": [],
      "message": "Operation completed successfully"
    }
    ```
-   **Authentication:** All V1 endpoints require a `Bearer` token in the `Authorization` header.
