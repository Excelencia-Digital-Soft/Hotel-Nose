<<<<<<< HEAD
# InRoom API

Este documento proporciona una guía para los desarrolladores que trabajan en el proyecto de la API de InRoom. Describe la estructura del proyecto, las convenciones y los conceptos arquitectónicos clave.

## 1. Descripción general del proyecto

InRoom es una API completa para la gestión hotelera. Maneja funcionalidades básicas como reservas, gestión de huéspedes (visitas), seguimiento de consumos (minibar, restaurante), promociones e inventario. La API está construida con ASP.NET Core y sigue los principios RESTful para sus puntos finales V1.

## 2. Tecnologías

-   **Backend:** C# con Asp.Net Core 9
-   **Base de datos:** Microsoft SQL Server 2024
-   **ORM:** Entity Framework Core
-   **Autenticación:** JWT Bearer Tokens

## 3. Estructura del proyecto

La solución se organiza en dos proyectos principales:

-   `API-Hotel/`: El proyecto principal de la API web de ASP.NET Core.
    -   `Controllers/`: Contiene los controladores de la API, con los puntos finales V1 en una subcarpeta dedicada `V1/`.
    -   `Data/`: Contiene el `HotelDbContext` y las configuraciones de Entity Framework.
    -   `Models/`: Define las entidades de la base de datos.
    -   `DTOs/`: Objetos de transferencia de datos utilizados para las solicitudes y respuestas de la API.
    -   `Services/`: Contiene la lógica de negocio para diferentes dominios (Reservas, Consumos, etc.).
    -   `Interfaces/`: Define los contratos para los servicios.
    -   `Extensions/`: Métodos de extensión para el registro de servicios, autenticación, etc.
    -   `appsettings.json`: Archivo de configuración para la aplicación.
-   `Scripts/`: Contiene scripts SQL para migraciones y mantenimiento de la base de datos.

## 4. Versionado y migración de la API

El proyecto se encuentra actualmente en proceso de migración de los puntos finales heredados a una API V1 versionada.

-   **Puntos finales heredados:** Convenciones mixtas (p. ej., `/GetConsumosVisita`).
-   **Puntos finales V1:** Convenciones RESTful (p. ej., `GET /api/v1/consumos/visita/{visitaId}`).

Todo el nuevo desarrollo debe utilizar la arquitectura V1. Consulte `MIGRATION_GUIDE.md` para obtener instrucciones detalladas sobre cómo migrar las llamadas de frontend a los nuevos puntos finales.

## 5. Base de datos

-   **ORM:** Se utiliza Entity Framework Core para el acceso a los datos.
-   **Esquema:** El esquema de la base de datos utiliza `PascalCase` para los nombres de las tablas y columnas. Las claves foráneas suelen nombrarse con un sufijo `ID` (p. ej., `VisitaID`).
-   **Configuración:** Las configuraciones de las entidades de EF Core se definen en `Data/Configurations/`.
-   **Migraciones:** Se utilizan scripts SQL manuales para los cambios de esquema y se encuentran en el directorio `Scripts/`.

## 6. Conceptos arquitectónicos clave

La lógica de negocio se divide en varios dominios de servicio.

### a. Reservas

-   **Gestión de estados:** Una reserva se considera **activa** si `FechaFin` y `FechaAnula` son `NULL`.
-   **Finalización:** Establece `FechaFin` a la marca de tiempo actual y marca la habitación como disponible.
-   **Cancelación:** Establece `FechaAnula` a la marca de tiempo actual.
-   **Promociones:** Las promociones se validan contra la categoría de la habitación (`Habitacion.CategoriaId`).
-   **Ver:** `RESERVAS_ARCHITECTURE_GUIDE.md`

### b. Consumos

-   **Inventario dual:** El sistema gestiona dos tipos de inventario:
    1.  **Inventario general:** Para artículos del stock central (p. ej., restaurante).
    2.  **Inventario de la habitación:** Para artículos en el minibar de una habitación específica.
-   **Transacciones:** Los consumos se registran en la tabla `Consumo`, vinculados a un `Movimiento` (factura).
-   **Trazabilidad:** La tabla `MovimientosStock` rastrea cada cambio en el inventario.
-   **Ver:** `CONSUMOS_ARCHITECTURE_GUIDE.md`

### c. Promociones

-   **Categorización:** Las promociones están vinculadas a `CategoriasHabitaciones` (categorías de habitaciones).
-   **Validación:** El sistema valida que una promoción sea aplicable a la categoría de la habitación en una reserva.
-   **Precios:** El precio promocional (`Promociones.Tarifa`) se aplica en lugar del precio estándar de la categoría (`CategoriasHabitaciones.PrecioNormal`).
-   **Ver:** `PROMOCIONES_ARCHITECTURE_GUIDE.md`

## 7. Flujo de trabajo de desarrollo

### Ejecución de la aplicación

1.  Asegúrese de que la cadena de conexión de la base de datos en `appsettings.Development.json` esté configurada correctamente.
2.  Ejecute la aplicación desde el directorio `API-Hotel`:
    ```bash
    dotnet run
    ```

### Migraciones de la base de datos

1.  Cree un nuevo script SQL en el directorio `Scripts/`.
2.  Escriba las sentencias `ALTER TABLE`, `CREATE TABLE`, etc. necesarias.
3.  Ejecute el script en la base de datos de destino.

## 8. Estilo de código y convenciones

-   **C#:** Siga las convenciones de codificación estándar de .NET (PascalCase para clases, métodos, propiedades; camelCase para variables locales).
-   **Diseño de la API:**
    -   Use sustantivos en plural para los nombres de los recursos (p. ej., `/api/v1/reservas`).
    -   Use los verbos HTTP correctamente (`GET`, `POST`, `PUT`, `DELETE`).
    -   Use kebab-case para los segmentos de la URL.
-   **DTOs:** Utilice objetos de transferencia de datos para desacoplar la capa de la API de los modelos de la base de datos.
-   **Respuestas:** Todas las respuestas de la API V1 se envuelven en un objeto `ApiResponse<T>` estándar:
    ```json
    {
      "isSuccess": true,
      "data": { ... },
      "errors": [],
      "message": "Operación completada con éxito"
    }
    ```
-   **Autenticación:** Todos los puntos finales V1 requieren un token `Bearer` en el encabezado `Authorization`.
=======
# 🏨 FrontHotel - Sistema de Gestión Hotelera

Un moderno sistema de gestión hotelera frontend construido con Vue 3, con actualizaciones en tiempo real, diseño glassmorphism y capacidades completas de gestión de habitaciones.

## ✨ Características

- **Gestión de Habitaciones en Tiempo Real**: Actualizaciones en vivo del estado de habitaciones vía SignalR WebSocket
- **Sistema de Reservas**: Flujo completo de reservas y check-in/check-out
- **Seguimiento de Consumos**: Gestión de consumos y facturación de huéspedes
- **UI Moderna**: Diseño glassmorphism con animaciones fluidas
- **Soporte Multi-idioma**: Preparado para internacionalización
- **Diseño Responsivo**: Enfoque mobile-first con TailwindCSS
- **Seguridad de Tipos**: Migración progresiva a TypeScript para mejor calidad de código

## 🚀 Inicio Rápido

### Requisitos Previos

- Node.js 18+
- npm o yarn
- Servidor API backend en ejecución

### Instalación

```bash
# Clonar el repositorio
git clone https://github.com/Excelencia-Digital-Soft/FrontHotel.git

# Navegar al directorio del proyecto
cd FrontHotel

# Instalar dependencias
npm install

# Copiar variables de entorno
cp .env.example .env.development

# Iniciar servidor de desarrollo
npm run dev
```

La aplicación estará disponible en `http://localhost:3001/hotel/`

## 📁 Estructura del Proyecto

```
src/
├── components/         # Componentes Vue
│   ├── popovers/      # Componentes popover
│   ├── modals/        # Diálogos modales
│   └── cards/         # Componentes de tarjetas
├── composables/       # Funciones de composición reutilizables
├── services/          # Capa de servicios API
├── store/             # Gestión de estado con Pinia
├── types/             # Definiciones de tipos TypeScript
├── views/             # Componentes de páginas
├── router/            # Configuración de Vue Router
└── assets/            # Recursos estáticos
```

## 🛠️ Scripts Disponibles

```bash
# Desarrollo
npm run dev              # Iniciar servidor de desarrollo (puerto 3001)
npm run build:dev        # Construir para desarrollo
npm run preview:dev      # Previsualizar build de desarrollo

# Producción
npm run build           # Construir para producción con verificación de tipos
npm run build:prod      # Construir para entorno de producción
npm run preview         # Previsualizar build de producción

# Calidad de Código
npm run typecheck       # Ejecutar verificación de tipos TypeScript
npm run lint            # Ejecutar linting
```

## 🔧 Configuración

### Variables de Entorno

Crear un archivo `.env.development` con:

```env
# Configuración API
VITE_API_BASE_URL=http://localhost:5000

# SignalR WebSocket
VITE_SIGNALR_HUB_URL=http://localhost:5000/rooms

# Entorno
VITE_APP_ENV=development
```

### Configuración de Build

El proyecto usa Vite con chunking optimizado:
- División automática de código para mejor rendimiento
- Chunks manuales para librerías vendor
- Optimización y minificación de recursos
- División de código CSS

## 🎨 Sistema de Diseño

La aplicación usa un patrón de diseño **glassmorphism** con:

- Fondos translúcidos con desenfoque de fondo
- Bordes y sombras sutiles
- Acentos con gradientes
- Optimizado para tema oscuro

### Paleta de Colores

- **Primario**: Índigo (`#6366f1`)
- **Secundario**: Púrpura (`#8b5cf6`)
- **Acento**: Rosa (`#ec4899`)
- **Neutro**: Grises oscuros para fondos

## 🔌 Integración API

### Migración a API V1

El proyecto está migrando a APIs V1 con estructura mejorada:

- ✅ **API de Consumos**: `/api/v1/consumos/*`
- ✅ **API de Reservas**: `/api/v1/reservas/*`
- ✅ **API de Promociones**: `/api/v1/promociones/*`

Las feature flags controlan el uso de versiones API:
```javascript
const USE_V1_API = true; // Habilitar APIs V1
```

### Actualizaciones en Tiempo Real

La integración con SignalR proporciona actualizaciones en tiempo real para:
- Cambios de estado de habitaciones
- Nuevas reservas
- Eventos de check-in/check-out
- Actualizaciones de mantenimiento
- Notificaciones de progreso

## 🏗️ Arquitectura

### Arquitectura de Componentes

```
Componentes (UI) → Composables (Lógica) → Servicios (API) → Store (Estado)
```

- **Componentes**: Presentación UI pura
- **Composables**: Lógica de negocio y estado local
- **Servicios**: Comunicación con API
- **Store**: Gestión de estado global con Pinia

### Gestión de Estado

- **Estado Local**: Refs de componente para estado UI
- **Estado de Funcionalidad**: Composables para estado específico de funcionalidad
- **Estado Global**: Stores de Pinia para estado de toda la aplicación

## 🚀 Despliegue

### Build de Producción

```bash
# Construir para producción
npm run build:prod

# La salida estará en el directorio dist/
# Desplegar la carpeta dist en tu servidor web
```

### Configuración del Servidor

La aplicación se sirve desde el subdirectorio `/hotel/`. Configura tu servidor web apropiadamente:

**Ejemplo con Nginx:**
```nginx
location /hotel/ {
    root /ruta/a/dist;
    try_files $uri $uri/ /hotel/index.html;
}
```

## 🤝 Contribuyendo

1. Hacer fork del repositorio
2. Crear tu rama de funcionalidad (`git checkout -b feature/FuncionalidadIncreible`)
3. Hacer commit de tus cambios (`git commit -m 'Agregar FuncionalidadIncreible'`)
4. Hacer push a la rama (`git push origin feature/FuncionalidadIncreible`)
5. Abrir un Pull Request

### Estándares de Código

- Seguir las mejores prácticas de Vue 3 Composition API
- Usar TypeScript para código nuevo
- Aplicar diseño glassmorphism consistentemente
- Separar responsabilidades: UI → Lógica → API
- Escribir código autodocumentado

## 📝 Licencia

Este proyecto es software propietario. Todos los derechos reservados.

## 🆘 Soporte

Para problemas y preguntas:
- Crear un issue en el repositorio de GitHub
- Contactar al equipo de desarrollo

## 🏆 Equipo

Desarrollado por **Excelencia Digital Soft**

---

**Nota**: Este es un proyecto activo en desarrollo continuo. Las funcionalidades y APIs pueden cambiar.
>>>>>>> 042a1f9163b31d928325cd23a5b4a01b4b223977
