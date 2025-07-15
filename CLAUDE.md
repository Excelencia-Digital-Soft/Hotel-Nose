# CLAUDE.md - Project Guidelines

## 🎯 Project Overview
This is a hotel management system frontend built with Vue 3, PrimeVue, and TailwindCSS. The application manages room reservations, guest check-ins/check-outs, and various hotel operations.

## 🏗️ Architecture Principles

### Separation of Concerns
Each code unit should have a **single, unique responsibility**:

#### Components (`/src/components/`)
- **Purpose**: UI presentation and user interaction
- **Responsibilities**:
  - Render UI elements
  - Handle user events
  - Delegate business logic to composables
  - Manage local UI state only
- **Example**:
  ```vue
  <!-- Good: Component only handles UI -->
  <template>
    <button @click="handleClick">{{ label }}</button>
  </template>
  <script setup>
  const emit = defineEmits(['action'])
  const handleClick = () => emit('action')
  </script>
  ```

#### Composables (`/src/composables/`)
- **Purpose**: Reusable business logic and state management
- **Responsibilities**:
  - Encapsulate reactive state
  - Implement business rules
  - Coordinate between services
  - Provide computed properties
- **Example**:
  ```js
  // Good: Composable manages state and logic
  export function useTimer() {
    const time = ref(0)
    const isRunning = ref(false)
    
    const start = () => { isRunning.value = true }
    const stop = () => { isRunning.value = false }
    
    return { time, isRunning, start, stop }
  }
  ```

#### Services (`/src/services/`)
- **Purpose**: External communication and data operations
- **Responsibilities**:
  - API calls
  - Data transformation
  - Error handling
  - No reactive state
- **Example**:
  ```js
  // Good: Service only handles API communication
  export class RoomService {
    static async getRooms() {
      const response = await axiosClient.get('/api/v1/rooms')
      return response.data
    }
  }
  ```

### Directory Structure
```
src/
├── components/
│   ├── popovers/      # Popover components
│   ├── modals/        # Modal components
│   └── cards/         # Card components
├── composables/
│   ├── useRoom.js     # Room-related logic
│   ├── useAuth.js     # Authentication logic
│   └── useTimer.js    # Timer functionality
└── services/
    ├── roomService.js    # Room API calls
    ├── authService.js    # Auth API calls
    └── consumoService.js # Consumption API calls
```

## 🎨 Design System: Glassmorphism

### Core Design Principles
All UI elements follow a **glassmorphism** design pattern with:
- Translucent backgrounds
- Backdrop blur effects
- Subtle borders
- Depth through layering

### Color Palette
```css
/* Primary Colors */
--primary-400: #818cf8;  /* Indigo */
--primary-500: #6366f1;
--secondary-400: #a78bfa; /* Purple */
--secondary-500: #8b5cf6;
--accent-400: #f472b6;   /* Pink */
--accent-500: #ec4899;

/* Neutral Colors */
--neutral-800: #262626;
--neutral-900: #171717;
```

### Glassmorphism Classes
```css
/* Glass Container */
.glass-container {
  @apply bg-white/5 backdrop-blur-2xl border border-white/20 rounded-3xl;
}

/* Glass Card */
.glass-card {
  @apply bg-white/10 backdrop-blur-md border border-white/20 rounded-xl;
}

/* Glass Button */
.glass-button {
  @apply bg-white/10 hover:bg-white/20 backdrop-blur-sm border border-white/30 rounded-lg transition-all;
}

/* Glass Input */
.glass-input {
  @apply bg-white/10 backdrop-blur-sm border border-white/30 rounded-lg text-white placeholder-gray-300;
}
```

### Component Styling Examples

#### Modal with Glassmorphism
```vue
<div class="fixed inset-0 bg-black/60 backdrop-blur-xl z-50">
  <div class="bg-white/5 backdrop-blur-2xl border border-white/20 rounded-3xl p-6">
    <!-- Content -->
  </div>
</div>
```

#### Button with Gradient
```vue
<button class="bg-gradient-to-r from-primary-400 via-secondary-400 to-accent-400 
               hover:from-primary-500 hover:via-secondary-500 hover:to-accent-500 
               backdrop-blur-sm border border-white/30 rounded-lg">
  Click me
</button>
```

#### Card with Glow Effect
```vue
<div class="relative overflow-hidden rounded-xl bg-white/5 backdrop-blur-md border border-white/20">
  <div class="absolute w-full h-full bg-white opacity-5 blur-[50px] -left-1/2 -top-1/2"></div>
  <div class="relative z-10 p-4">
    <!-- Content -->
  </div>
</div>
```

## 🛠️ Development Guidelines

### Component Creation Checklist
- [ ] Single responsibility principle
- [ ] Props validation with TypeScript/PropTypes
- [ ] Emit events for parent communication
- [ ] Use composables for business logic
- [ ] Apply glassmorphism styling
- [ ] Add loading and error states
- [ ] Include accessibility attributes

### Composable Best Practices
1. **Prefix with 'use'**: `useRoom`, `useAuth`, `useTimer`
2. **Return reactive values**: Use `ref()` and `computed()`
3. **Avoid side effects in setup**: Use lifecycle hooks
4. **Document parameters and return values**

### Service Guidelines
1. **Static methods for stateless operations**
2. **Consistent error handling**
3. **Transform API responses to frontend models**
4. **Use TypeScript interfaces for API contracts**

### State Management
- **Local state**: Component refs for UI state
- **Shared state**: Composables for feature state
- **Global state**: Pinia stores for app-wide state

## 🚀 Common Patterns

### Modal/Popover Pattern
```vue
<!-- Parent Component -->
<OverlayPanel ref="panel">
  <ChildComponent @action="handleAction" />
</OverlayPanel>

<!-- Script -->
const panel = ref(null)
const showPanel = (event) => panel.value.toggle(event)
```

### API Integration Pattern
```js
// Service
export class EntityService {
  static async getAll() { /* ... */ }
  static async getById(id) { /* ... */ }
  static async create(data) { /* ... */ }
  static async update(id, data) { /* ... */ }
  static async delete(id) { /* ... */ }
}

// Composable
export function useEntity() {
  const entities = ref([])
  const loading = ref(false)
  
  const fetchEntities = async () => {
    loading.value = true
    try {
      entities.value = await EntityService.getAll()
    } finally {
      loading.value = false
    }
  }
  
  return { entities, loading, fetchEntities }
}
```

### Form Handling Pattern
```vue
<template>
  <form @submit.prevent="handleSubmit" class="glass-card p-4">
    <input v-model="form.name" class="glass-input" />
    <button type="submit" class="glass-button">Submit</button>
  </form>
</template>

<script setup>
const form = reactive({
  name: ''
})

const handleSubmit = async () => {
  await EntityService.create(form)
  // Reset or redirect
}
</script>
```

## 📝 Code Style

### Vue SFC Order
1. `<template>`
2. `<script setup>`
3. `<style scoped>`

### Import Order
1. Vue core imports
2. Third-party libraries
3. Composables
4. Components
5. Services
6. Types/Interfaces
7. Utils/Helpers

### Naming Conventions
- **Components**: PascalCase (`RoomCard.vue`)
- **Composables**: camelCase with 'use' prefix (`useRoom.js`)
- **Services**: PascalCase with 'Service' suffix (`RoomService.js`)
- **Props/Events**: camelCase (`roomData`, `onRoomSelect`)

## 🔧 Tools & Commands

### Development
```bash
npm run dev          # Start dev server
npm run build        # Build for production
npm run lint         # Run ESLint
npm run typecheck    # Run TypeScript checks
```

### Testing
```bash
npm run test         # Run unit tests
npm run test:e2e     # Run E2E tests
```

## 🚨 Important Notes

1. **Always separate concerns**: UI → Composables → Services
2. **Use glassmorphism consistently**: All modals, cards, and overlays
3. **Prefer composition over inheritance**
4. **Handle loading and error states in every async operation**
5. **Use PrimeVue components when available, customize with glassmorphism**
6. **Maintain backwards compatibility when updating APIs**

## 🎯 Performance Guidelines

1. **Lazy load components**: Use dynamic imports for modals
2. **Debounce search inputs**: 300ms delay minimum
3. **Virtualize long lists**: Use virtual scrolling for 50+ items
4. **Optimize images**: Use WebP format with fallbacks
5. **Cache API responses**: Use composables to cache data

## 🔐 Security Guidelines

1. **Never store sensitive data in localStorage**
2. **Validate all user inputs**
3. **Sanitize HTML content**
4. **Use HTTPS for all API calls**
5. **Implement proper CORS policies**

## 🚀 API Migration V1

### Current Status
✅ **ReserveRoom component is now using V1 APIs** (`USE_V1_API = true`)

### V1 Endpoints Implementation

#### 📦 **ConsumosController V1**
```js
// New V1 endpoints
GET /api/v1/consumos/visita/{visitaId}         // Get consumos by visita
GET /api/v1/consumos/visita/{visitaId}/summary // Get consumos summary  
POST /api/v1/consumos/general                  // Add general consumos
POST /api/v1/consumos/room                     // Add room consumos
PUT /api/v1/consumos/{consumoId}               // Update quantity
DELETE /api/v1/consumos/{consumoId}            // Cancel consumo

// Legacy endpoints (marked as Obsolete)
POST /ConsumoGeneral → /api/v1/consumos/general
POST /ConsumoHabitacion → /api/v1/consumos/room
GET /GetConsumosVisita → /api/v1/consumos/visita/{id}
PUT /UpdateConsumo → /api/v1/consumos/{id}
DELETE /AnularConsumo → /api/v1/consumos/{id}
```

#### 🏨 **ReservasController V1**
```js
// New V1 endpoints
GET /api/v1/reservas/{reservaId}               // Get reserva
GET /api/v1/reservas/active                    // Get active reservas
POST /api/v1/reservas/finalize                 // Finalize reserva
POST /api/v1/reservas/{visitaId}/pause         // Pause ocupacion
POST /api/v1/reservas/{visitaId}/resume        // Resume ocupacion
PUT /api/v1/reservas/{reservaId}/promotion     // Update promotion
PUT /api/v1/reservas/{reservaId}/extend        // Extend time
DELETE /api/v1/reservas/{reservaId}            // Cancel reserva

// Legacy endpoints (marked as Obsolete)
PUT /FinalizarReserva → /api/v1/reservas/finalize
PUT /PausarOcupacion → /api/v1/reservas/{id}/pause
PUT /ActualizarReservaPromocion → /api/v1/reservas/{id}/promotion
```

#### 🎫 **PromocionesController V1**
```js
// New V1 endpoints
GET /api/v1/promociones/categoria/{categoriaId} // Get by category
GET /api/v1/promociones/active                  // Get active promotions
GET /api/v1/promociones/{promocionId}           // Get promotion
POST /api/v1/promociones                        // Create promotion
PUT /api/v1/promociones/{promocionId}           // Update promotion
DELETE /api/v1/promociones/{promocionId}        // Delete promotion
POST /api/v1/promociones/{promocionId}/validate // Validate promotion

// Legacy endpoints (marked as Obsolete)
GET /api/Promociones/GetPromocionesCategoria → /api/v1/promociones/categoria/{id}
```

### Service Layer Implementation
```js
// Example: ConsumosService.js
export class ConsumosService {
  // V1 Methods
  static async addGeneralConsumos(visitaId, habitacionId, consumos) {
    const payload = { visitaId, habitacionId, consumos }
    return await axiosClient.post('/api/v1/consumos/general', payload)
  }
  
  // Legacy Methods (for backward compatibility)
  static async addLegacyGeneralConsumos(habitacionId, visitaId, selectedItems) {
    return await axiosClient.post(`/ConsumoGeneral?habitacionId=${habitacionId}&visitaId=${visitaId}`, selectedItems)
  }
}
```

### Composable Integration
```js
// Example: useConsumos.js
export function useConsumos(selectedRoom, useV1Api = false) {
  const agregarConsumos = async (selectedItems) => {
    if (useV1Api) {
      await ConsumosService.addGeneralConsumos(
        selectedRoom.value.VisitaID,
        selectedRoom.value.HabitacionID, 
        selectedItems
      )
    } else {
      await ConsumosService.addLegacyGeneralConsumos(
        selectedRoom.value.HabitacionID,
        selectedRoom.value.VisitaID,
        selectedItems
      )
    }
  }
}
```

### Migration Checklist
- [x] **ConsumosController**: All endpoints implemented with proper DTOs
- [x] **ReservasController**: All endpoints implemented with proper DTOs  
- [x] **PromocionesController**: All endpoints implemented with proper DTOs
- [x] **Services**: V1 and Legacy methods available
- [x] **Composables**: Feature flag support (`useV1Api`)
- [x] **ReserveRoom**: Feature flag activated (`USE_V1_API = true`)
- [x] **Backward Compatibility**: Legacy methods maintained
- [x] **Error Handling**: Consistent across all services
- [x] **TypeScript Support**: Ready for DTO interfaces

### Benefits of V1 APIs
1. **Structured DTOs**: Consistent request/response formats
2. **Better Error Handling**: Standardized error responses
3. **Validation**: Built-in request validation
4. **Documentation**: Auto-generated API documentation
5. **Versioning**: Clear API versioning strategy
6. **Performance**: Optimized endpoints
7. **Type Safety**: Ready for TypeScript integration

---

**Remember**: Clean code is more important than clever code. When in doubt, choose readability and maintainability.