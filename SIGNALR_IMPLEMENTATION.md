# 🔔 SignalR Notification System - Hotel Management

Documentación completa del sistema de notificaciones en tiempo real usando SignalR para el sistema de gestión hotelera.

## 📋 Tabla de Contenidos

- [🎯 Resumen Ejecutivo](#-resumen-ejecutivo)
- [🏗️ Arquitectura](#️-arquitectura)
- [⚡ Conexión Automática](#-conexión-automática)
- [🔔 Notificaciones Globales](#-notificaciones-globales)
- [🎨 Integración UI](#-integración-ui)
- [📦 Migración desde Sistema Anterior](#-migración-desde-sistema-anterior)
- [🚀 Guía de Implementación](#-guía-de-implementación)

---

## 🎯 Resumen Ejecutivo

### ✅ Estado Actual
- **Sistema anterior eliminado**: `src/websocket.ts` (obsoleto) ❌
- **Store websocket**: `src/store/websocket.ts` ⚠️ (en uso, pendiente migración)
- **Nueva implementación**: SignalR TypeScript completa ✅
- **Auth store**: Actualizado para usar nueva implementación ✅

### 🚀 Características Principales
- ✅ **Conexión automática** al hacer login
- ✅ **Notificaciones globales** en toda la aplicación
- ✅ **Toasts glassmorphism** automáticos
- ✅ **TypeScript completo** con interfaces tipadas
- ✅ **Reconexión automática** con exponential backoff
- ✅ **Gestión de memoria** optimizada
- ✅ **Filtros avanzados** por categoría/severidad

---

## 🏗️ Arquitectura

### 📁 Estructura de Archivos
```
src/
├── services/
│   └── NotificationService.ts      # ✅ Servicio principal SignalR
├── composables/
│   ├── useNotifications.ts         # ✅ Composable principal
│   └── useSignalRAutoConnect.ts    # ✅ Auto-conexión
├── types/
│   └── signalr.ts                  # ✅ Interfaces TypeScript
└── store/
    ├── auth.js                     # ✅ Integrado con SignalR
    └── websocket.ts                # ⚠️ Pendiente migración
```

### 🔄 Flujo de Conexión
```
Usuario Login → Auth Store → NotificationService → SignalR Hub
                    ↓
            Auto-suscripción por institución
                    ↓
            Notificaciones en tiempo real
                    ↓
            Toasts + Event Listeners
```

---

## ⚡ Conexión Automática

### 🔌 ¿Cómo se conecta al hacer login?

#### 1. **En el Auth Store** (`store/auth.js`)
```javascript
async login(credentials) {
  // ... proceso de login ...
  
  if (response.data.isSuccess) {
    this.token = token
    this.user = user
    this.isAuthenticated = true
    
    if (user.institucionId) {
      this.institucionID = user.institucionId
      // 🔌 AQUÍ SE CONECTA AUTOMÁTICAMENTE
      this.connectWebSocket() // ← Usa la nueva implementación SignalR
    }
  }
}
```

#### 2. **Método connectWebSocket actualizado**
```javascript
async connectWebSocket() {
  try {
    // ✨ Usa el nuevo NotificationService
    const { NotificationService } = await import('../services/NotificationService')
    const notificationService = NotificationService.getInstance()
    
    if (this.token && this.institucionID) {
      await notificationService.initialize(this.token)
      console.log('SignalR connected for institution:', this.institucionID)
    }
  } catch (error) {
    console.error('Failed to connect SignalR:', error)
  }
}
```

#### 3. **Desconexión automática en logout**
```javascript
async logout() {
  // ... logout logic ...
  
  // Disconnect SignalR
  const { NotificationService } = await import('../services/NotificationService')
  const notificationService = NotificationService.getInstance()
  await notificationService.stop()
}
```

---

## 🔔 Notificaciones Globales

### ✨ **¡SÍ! Las notificaciones avisan automáticamente en cualquier parte de la aplicación**

#### 🎨 Toasts Glassmorphism Automáticos
```vue
<!-- En cualquier componente o App.vue -->
<script setup>
import { useNotificationToasts } from '@/composables/useNotifications'

// 🔔 Esto hace que aparezcan toasts automáticamente para TODAS las notificaciones
useNotificationToasts()
</script>
```

#### 🌍 Event Listeners Globales
```vue
<script setup>
import { useNotifications } from '@/composables/useNotifications'

const { onNotificationReceived } = useNotifications()

// 🌍 Escucha TODAS las notificaciones que llegan
onNotificationReceived((notification) => {
  console.log('Nueva notificación recibida:', notification)
  
  // Acciones específicas según el tipo
  switch (notification.category) {
    case 'room_status':
      // Actualizar vista de habitaciones
      break
    case 'payment':
      // Mostrar alerta de pago
      break
    case 'alert':
      // Mostrar alerta crítica + sonido
      playAlertSound()
      break
  }
})
</script>
```

#### 🏨 Notificaciones Específicas por Vista
```vue
<!-- En la vista de habitaciones -->
<script setup>
import { useRoomNotifications } from '@/composables/useNotifications'

// 🏨 Solo notificaciones de habitaciones
const { notifications, onNotificationReceived } = useRoomNotifications()

onNotificationReceived((notification) => {
  // Solo llegan notificaciones de habitaciones aquí
  console.log('Habitación actualizada:', notification.data.roomNumber)
})
</script>
```

### 📊 Tipos de Notificaciones

#### Por Severidad:
- 🔴 **Error**: Bordes rojos, sonido opcional
- 🟡 **Warning**: Bordes amarillos  
- 🟢 **Success**: Bordes verdes
- 🔵 **Info**: Bordes azules

#### Por Categoría:
- 🏨 **room_status**: Estado de habitaciones
- 💰 **payment**: Confirmaciones de pago
- 📦 **inventory**: Alertas de stock
- 🔧 **maintenance**: Mantenimiento
- 🚨 **alert**: Emergencias (con sonido)
- 📋 **reservation**: Reservas
- 🛒 **consumption**: Consumos
- 💳 **checkout**: Check-outs

---

## 🎨 Integración UI

### 🔥 Toast Glassmorphism Automático
Cuando llega una notificación, aparece un toast así:

```vue
<!-- Toast que aparece automáticamente -->
<div class="fixed top-4 right-4 z-50 transform transition-all duration-500">
  <div class="glass-card p-4 border-l-4 border-green-400 bg-green-500/10">
    <div class="flex items-start space-x-3">
      <div class="flex-shrink-0">
        <i class="fas fa-check-circle text-green-400"></i>
      </div>
      <div class="flex-1">
        <h4 class="text-white font-medium">Pago Confirmado</h4>
        <p class="text-gray-300 text-sm">
          El pago de la habitación 205 ha sido procesado exitosamente.
        </p>
        <span class="text-xs text-gray-400">Hace 2 segundos</span>
      </div>
      <button class="text-gray-400 hover:text-white">
        <i class="fas fa-times"></i>
      </button>
    </div>
  </div>
</div>
```

### 🏠 Configuración Recomendada en App.vue

**❌ ACTUALMENTE NO ESTÁ IMPLEMENTADO** - Necesitas agregar esto:

```vue
<!-- App.vue -->
<template>
  <div id="app">
    <!-- Menu Coordination Provider con overlay opcional -->
    <MenuCoordinationProvider :show-overlay="true" overlay-opacity="0.1">
      <!-- Main content area with router view -->
      <router-view />
    </MenuCoordinationProvider>
    
    <!-- 🆕 Agregar: Toast Container -->
    <NotificationToastContainer />
    
    <!-- 🆕 Agregar: Global Notification Badge -->
    <div v-if="unreadCount > 0" class="fixed top-4 left-4 z-50">
      <div class="glass-card px-3 py-2 bg-red-500/20 border-red-400">
        <span class="text-white font-bold">{{ unreadCount }}</span>
        <span class="text-gray-300 text-sm ml-1">notificaciones</span>
      </div>
    </div>
  </div>
</template>

<script setup>
import { onMounted, computed } from 'vue'
import { useAuthStore } from './store/auth.js'
import MenuCoordinationProvider from './components/NavBar/MenuCoordinationProvider.vue'
// 🆕 Agregar imports
import { useNotifications, useNotificationToasts } from '@/composables/useNotifications'
import { useSignalRAutoConnect } from '@/composables/useSignalRAutoConnect'
import NotificationToastContainer from '@/components/NotificationToastContainer.vue'

const authStore = useAuthStore()

// 🆕 Agregar: Auto-conecta SignalR
useSignalRAutoConnect()

// 🆕 Agregar: Habilita toasts automáticos globales
useNotificationToasts()

// 🆕 Agregar: Estado global de notificaciones
const { unreadNotifications, onNotificationReceived } = useNotifications()
const unreadCount = computed(() => unreadNotifications.value.length)

// 🆕 Agregar: Sonidos para notificaciones críticas
onNotificationReceived((notification) => {
  if (notification.severity === 'error' || notification.category === 'alert') {
    // Reproducir sonido de alerta
    const audio = new Audio('/sounds/alert.mp3')
    audio.play().catch(console.error)
  }
})

onMounted(() => {
  // Initialize auth store if needed
  if (localStorage.getItem('token')) {
    authStore.checkAuth()
  }
})
</script>
```

---

## 📦 Migración desde Sistema Anterior

### 🔄 Componentes a Migrar

#### 1. **NotificacionPedidoModal.vue**
**❌ Antes (websocket store):**
```vue
<script setup>
import { useWebSocketStore } from "../store/websocket.js"

const websocketStore = useWebSocketStore()
const notifications = computed(() => websocketStore.notifications)
</script>
```

**✅ Después (nueva implementación):**
```vue
<script setup>
import { useNotifications } from '@/composables/useNotifications'

const { 
  notifications, 
  unreadNotifications,
  markAsRead,
  dismissNotification 
} = useNotifications()

// Con toasts automáticos glassmorphism
useNotificationToasts()
</script>
```

#### 2. **views/Rooms.vue**
**❌ Antes:**
```vue
<script setup>
import { useWebSocketStore } from '../store/websocket.js'

const websocketStore = useWebSocketStore()
</script>
```

**✅ Después:**
```vue
<script setup>
import { useRoomNotifications } from '@/composables/useNotifications'

// Solo notificaciones relacionadas con habitaciones
const { 
  notifications: roomNotifications,
  onNotificationReceived 
} = useRoomNotifications()

// Manejar notificaciones específicas de habitaciones
onNotificationReceived((notification) => {
  if (notification.category === 'room_status') {
    // Actualizar estado de habitación
    console.log('Room status updated:', notification.data)
  }
})
</script>
```

#### 3. **composables/rooms/useRoomWebSocket.ts**
**❌ Antes:**
```typescript
import { useWebSocketStore } from '../../store/websocket'

export function useRoomWebSocket() {
  const websocketStore = useWebSocketStore()
  // ...
}
```

**✅ Después:**
```typescript
import { useNotificationsByCategory } from '@/composables/useNotifications'

export function useRoomWebSocket() {
  const { 
    notifications,
    onNotificationReceived,
    filterNotifications 
  } = useNotificationsByCategory('room_status')

  // Filtrar por habitación específica
  const getRoomNotifications = (roomId: number) => {
    return filterNotifications({
      customFilter: (notification) => 
        notification.data?.roomId === roomId
    })
  }

  return {
    notifications,
    getRoomNotifications,
    onNotificationReceived
  }
}
```

### 📋 Checklist de Migración

#### Archivos que necesitan migración:
- [ ] `views/Rooms.vue`
- [ ] `components/NotificacionPedidoModal.vue`
- [ ] `composables/rooms/useRoomWebSocket.ts`
- [ ] **App.vue** (agregar configuración global)

#### Después de la migración:
- [ ] Eliminar `src/store/websocket.ts`
- [ ] Verificar que no hay imports del store websocket anterior
- [ ] Probar notificaciones en todas las vistas

---

## 🚀 Guía de Implementación

### 📦 1. Instalación
```bash
npm install @microsoft/signalr
```

### 🔧 2. Configuración Básica (App.vue)

**PENDIENTE**: Agregar al `App.vue` actual:

```vue
<script setup>
// ... imports existentes ...

// 🆕 Agregar estos imports
import { useNotifications, useNotificationToasts } from '@/composables/useNotifications'
import { useSignalRAutoConnect } from '@/composables/useSignalRAutoConnect'

// 🆕 Agregar estas líneas
useSignalRAutoConnect()      // Auto-conecta al login
useNotificationToasts()     // Toasts automáticos globales
</script>
```

### 🎯 3. Uso en Componentes

#### Notificaciones globales:
```vue
<script setup>
import { useNotifications } from '@/composables/useNotifications'

const { notifications, onNotificationReceived } = useNotifications()
</script>
```

#### Notificaciones específicas:
```vue
<script setup>
import { useRoomNotifications } from '@/composables/useNotifications'

const { notifications } = useRoomNotifications(roomId)
</script>
```

#### Solo toasts (sin gestión):
```vue
<script setup>
import { useNotificationToasts } from '@/composables/useNotifications'

useNotificationToasts() // Solo muestra toasts automáticos
</script>
```

### 🔍 4. Debugging

#### Ver estado de conexión:
```vue
<script setup>
import { useNotifications } from '@/composables/useNotifications'

const { connectionState, isConnected } = useNotifications()

console.log('Connected:', isConnected.value)
console.log('Connection state:', connectionState.value)
</script>
```

#### Ver todas las notificaciones:
```vue
<script setup>
const { notifications, stats } = useNotifications()

console.log('All notifications:', notifications.value)
console.log('Stats:', stats.value)
</script>
```

---

## 🎉 Beneficios de la Nueva Implementación

### 🚀 **Performance**
- ✅ Singleton pattern para una sola conexión
- ✅ Gestión de memoria optimizada
- ✅ Reconexión inteligente con exponential backoff

### 🎨 **UI/UX**
- ✅ Toasts glassmorphism automáticos
- ✅ Notificaciones tipadas por categoría
- ✅ Estados de lectura/no leído
- ✅ Filtros avanzados

### 🛠️ **Desarrollo**
- ✅ TypeScript completo
- ✅ Composables reutilizables
- ✅ Mejor manejo de errores
- ✅ Integración perfecta con auth

### 🔐 **Seguridad**
- ✅ Autenticación automática con token
- ✅ Auto-suscripción por institución
- ✅ Validación de conexión

---

## 📞 Soporte

### 🐛 Problemas Comunes

#### Conexión no se establece:
1. Verificar que el token esté presente
2. Verificar que `institucionID` esté configurado
3. Revisar la consola para errores de SignalR

#### Notificaciones no aparecen:
1. Verificar que `useNotificationToasts()` esté en App.vue
2. Verificar la conexión SignalR
3. Revivar el endpoint `/api/v1/notifications`

#### Toasts no tienen estilo glassmorphism:
1. Verificar que las clases CSS estén disponibles
2. Verificar que TailwindCSS esté configurado
3. Agregar los estilos de glassmorphism al proyecto

---

**✨ ¡El sistema SignalR está listo para usar! Solo falta agregar la configuración global en App.vue para tener notificaciones automáticas en toda la aplicación.** 🎉