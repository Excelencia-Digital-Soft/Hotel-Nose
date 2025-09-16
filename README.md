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