# 📊 Sistema de Estadísticas Hoteleras

Sistema completo de análisis y visualización de estadísticas para la gestión hotelera, desarrollado con Vue 3, Chart.js y el diseño glassmorphism.

## 🎯 Características Principales

### 📈 Análisis Disponibles
1. **Ranking de Habitaciones** - Por número total de reservas
2. **Ingresos por Habitación** - Desglose detallado de reservas vs consumos  
3. **Ocupación por Categoría** - Tasas de ocupación y horas utilizadas
4. **Consumo por Habitación** - Análisis detallado de productos consumidos

### 🎨 Diseño y UX
- **Glassmorphism Design**: Interfaz moderna con efectos de vidrio y blur
- **Responsive**: Adaptable a todos los tamaños de pantalla
- **Micro-interacciones**: Animaciones suaves y hover effects
- **Toast Notifications**: Feedback inmediato con emojis
- **Estados de carga**: Indicadores elegantes y informativos

### 📅 Gestión de Períodos
- **Períodos predefinidos**: Hoy, ayer, última semana, último mes, etc.
- **Selección personalizada**: Rangos de fechas flexibles
- **Validación avanzada**: Control de rangos máximos y fechas futuras
- **Cache inteligente**: Optimización de rendimiento

## 🏗️ Arquitectura del Sistema

### 📁 Estructura de Archivos

```
src/
├── services/
│   └── StatisticsService.js          # API calls y validaciones
├── store/
│   └── statistics.js                 # Estado global con Pinia
├── composables/
│   └── useStatistics.js             # Lógica reactiva y helpers
├── components/
│   ├── charts/
│   │   ├── RoomRankingChart.vue      # Gráfico de ranking
│   │   ├── RoomRevenueChart.vue      # Gráfico de ingresos
│   │   ├── CategoryOccupancyChart.vue # Gráfico de ocupación
│   │   └── RoomConsumptionChart.vue  # Gráfico de consumos
│   └── statistics/
│       └── DateRangeSelector.vue     # Selector de períodos
└── views/
    └── StatisticsManager.vue         # Vista principal
```

### 🔧 Servicios (Services Layer)

**StatisticsService.js** - Manejo de API calls
```javascript
// Métodos principales
- getRoomRanking(dateRange)      // Ranking de habitaciones
- getRoomRevenue(dateRange)      // Ingresos por habitación  
- getCategoryOccupancy(dateRange) // Ocupación por categoría
- getRoomConsumption(dateRange)   // Consumo por habitación

// Utilidades
- validateDateRange(dateRange)    // Validación de fechas
- formatDateRangeDisplay(range)   // Formateo para mostrar
- getDefaultDateRange(id)         // Rango por defecto
- getPredefinedRanges(id)         // Rangos predefinidos
```

### 🗃️ Store (Estado Global)

**statistics.js** - Store con Pinia
```javascript
// Estado
- roomRanking[]           // Datos de ranking
- roomRevenue[]           // Datos de ingresos
- categoryOccupancy[]     // Datos de ocupación
- roomConsumption[]       // Datos de consumo
- isLoading...            // Estados de carga
- errors{}                // Manejo de errores
- lastFetchTimes{}        // Cache timestamps

// Getters
- totalRooms              // Total de habitaciones
- totalRevenue            // Ingresos totales
- averageOccupancy        // Ocupación promedio
- topRoomsByReservations  // Top habitaciones
- dashboardSummary        // Resumen ejecutivo

// Actions
- fetchRoomRanking()      // Cargar ranking
- fetchAllStatistics()    // Cargar todo
- refreshAllData()        // Refrescar datos
```

### 🎣 Composables (Lógica Reactiva)

**useStatistics.js** - Composable principal
```javascript
// Estado reactivo
- selectedDateRange       // Rango seleccionado
- predefinedRanges       // Rangos predefinidos
- currentDateRangeDisplay // Texto del rango actual

// Métodos
- setPredefinedRange()    // Seleccionar rango predefinido
- setCustomDateRange()    // Seleccionar rango personalizado
- fetchAllStatistics()    // Cargar estadísticas
- refreshAllData()        // Refrescar datos
- formatCurrency()        // Formatear moneda
- formatPercentage()      // Formatear porcentajes
```

## 📊 Componentes de Charts

### 🏆 RoomRankingChart.vue
**Funcionalidades:**
- Gráfico de barras/circular intercambiable
- Top 10 habitaciones por reservas
- Estadísticas resumidas (total habitaciones, reservas, promedio)
- Colores por categoría de habitación

### 💰 RoomRevenueChart.vue  
**Funcionalidades:**
- 3 modos de vista: Total, Desglose, Comparación
- Gráfico stacked para mostrar reservas vs consumos
- Top performers por ingresos totales y consumos
- Cálculo de porcentajes de distribución

### 📈 CategoryOccupancyChart.vue
**Funcionalidades:**
- Vista de tasa de ocupación/horas ocupadas/combinada
- Tabla con barras de progreso por categoría
- Estados de rendimiento (Excelente, Buena, Regular, Baja)
- Insights de mejores performers y oportunidades

### 🛒 RoomConsumptionChart.vue
**Funcionalidades:**
- Vista total por habitación/desglose/productos populares
- Detalles expandibles por habitación
- Top productos más consumidos
- Tabla de ranking con métricas detalladas

## 🛠️ Instalación y Configuración

### 1. Instalar Dependencias
```bash
npm install chart.js
```

### 2. Configurar Router
```javascript
// En /src/router/routes/admin.js
{
  path: "/StatisticsManager",
  name: "StatisticsManager", 
  component: StatisticsManager,
  meta: {
    requireAuth: true,
    roles: ROLE_GROUPS.ADMIN_ACCESS,
    description: "View detailed statistics and analytics",
    category: "Analytics"
  }
}
```

### 3. Configurar Store
```javascript
// En tu store principal, asegúrate de que Pinia esté configurado
import { useStatisticsStore } from './store/statistics'
```

### 4. Configurar API Endpoints
El sistema espera los siguientes endpoints V1:
```
POST /api/v1/statistics/room-ranking
POST /api/v1/statistics/room-revenue  
POST /api/v1/statistics/category-occupancy
POST /api/v1/statistics/room-consumption
```

## 📋 DTOs Esperados

### DateRangeDto (Request)
```javascript
{
  fechaInicio: "2024-01-01",    // ISO date string
  fechaFin: "2024-01-31",       // ISO date string  
  institucionID: 1              // Institution ID
}
```

### RoomRankingDto (Response)
```javascript
{
  habitacionID: 1,
  nombreHabitacion: "Suite 101",
  nombreCategoria: "Suite Premium", 
  totalReservas: 25
}
```

### RoomRevenueDto (Response)
```javascript
{
  habitacionID: 1,
  nombreHabitacion: "Suite 101",
  nombreCategoria: "Suite Premium",
  totalIngresos: 1500000,
  ingresosReservas: 1200000,
  ingresosConsumos: 300000
}
```

### CategoryOccupancyDto (Response)
```javascript
{
  categoriaID: 1,
  nombreCategoria: "Suite Premium",
  tasaOcupacion: 75.5,          // Percentage
  totalHorasOcupadas: 450
}
```

### RoomConsumptionDto (Response)
```javascript
{
  habitacionID: 1,
  nombreHabitacion: "Suite 101", 
  nombreCategoria: "Suite Premium",
  totalConsumos: 300000,
  detalles: [
    {
      articuloID: 1,
      nombreArticulo: "Cerveza",
      cantidad: 10,
      precioTotal: 50000
    }
  ]
}
```

## 🎯 Uso del Sistema

### 1. Navegación
```
/StatisticsManager - Vista principal de estadísticas
```

### 2. Selección de Período
- Usar botones predefinidos para períodos comunes
- Seleccionar fechas personalizadas para análisis específicos
- El sistema valida rangos y muestra errores claros

### 3. Visualización de Datos
- Los gráficos se actualizan automáticamente al cambiar períodos
- Usar botones de refresh individual para cada gráfico
- Alternar entre tipos de vista según el gráfico

### 4. Interpretación de Resultados
- **Dashboard Summary**: Vista general de KPIs principales
- **Gráficos individuales**: Análisis detallado por área
- **Estados y colores**: Indicadores visuales de rendimiento

## 🔧 Personalización

### Colores y Temas
```javascript
// En useStatistics.js
const getChartColors = () => {
  return {
    primary: ['#818cf8', '#6366f1', '#4f46e5'],
    secondary: ['#a78bfa', '#8b5cf6', '#7c3aed'], 
    accent: ['#f472b6', '#ec4899', '#db2777'],
    // ... más colores
  }
}
```

### Períodos Predefinidos
```javascript
// En StatisticsService.js - getPredefinedRanges()
// Personalizar los rangos disponibles según necesidades
```

### Validaciones
```javascript
// En StatisticsService.js - validateDateRange()
// Ajustar límites máximos de período según requerimientos
```

## 🚀 Características Avanzadas

### Cache Inteligente
- Los datos se almacenan en caché por 5 minutos
- Evita llamadas innecesarias a la API
- Indicadores visuales de datos frescos

### Manejo de Errores
- Toast notifications con mensajes específicos
- Estados de error por gráfico individual
- Opciones de reintento automático

### Optimización de Rendimiento
- Lazy loading de gráficos
- Destrucción automática de charts
- Debounce en cambios de fecha

### Responsive Design
- Grid layouts adaptativos
- Gráficos escalables
- Navegación optimizada para móvil

## 🎨 Guía de Estilo

### Glassmorphism Classes
```css
.glass-container    /* Contenedores principales */
.glass-card        /* Tarjetas y paneles */
.glass-button      /* Botones interactivos */
.glass-input       /* Campos de entrada */
```

### Iconografía
- PrimeIcons para iconos técnicos
- Emojis para elementos amigables
- Gradientes para elementos destacados

### Animaciones
- Hover effects en todos los elementos interactivos
- Transiciones suaves (300ms)
- Loading spinners elegantes
- Scale effects en botones

## 📱 Compatibilidad

- **Navegadores**: Chrome 90+, Firefox 88+, Safari 14+, Edge 90+
- **Dispositivos**: Desktop, Tablet, Mobile
- **Resoluciones**: 320px - 4K+
- **Frameworks**: Vue 3.3+, Chart.js 4+

## 🔒 Seguridad

### Control de Acceso
- Rutas protegidas por roles de administrador
- Validación de institución en todos los endpoints
- Sanitización de parámetros de fecha

### Validación de Datos
- Rangos de fecha limitados (máximo 1 año)
- Validación de institución ID
- Manejo seguro de errores de API

## 🐛 Troubleshooting

### Problemas Comunes

**Error: "Property 'statistics' was accessed during render but is not defined"**
```javascript
// Solución: Usar optional chaining
{{ statistics?.total || 0 }}
```

**Gráficos no se muestran**
```javascript
// Verificar que Chart.js esté importado correctamente
import { Chart, registerables } from 'chart.js'
Chart.register(...registerables)
```

**Fechas no válidas**
```javascript
// Verificar formato ISO de fechas
fechaInicio: "2024-01-01"  // ✅ Correcto
fechaInicio: "01/01/2024"  // ❌ Incorrecto
```

### Logs de Debug
```javascript
// Activar logs detallados en desarrollo
console.log('Statistics data:', statisticsStore.roomRanking)
console.log('Date range:', selectedDateRange.value)
console.log('Validation errors:', dateRangeErrors.value)
```

---

## 👥 Créditos

Desarrollado siguiendo los patrones de diseño glassmorphism y arquitectura Vue 3 moderna establecidos en el proyecto. Sistema completamente integrado con el backend V1 API y diseñado para escalabilidad y mantenibilidad.

**¡Sistema listo para análisis profesional de estadísticas hoteleras! 📊✨**