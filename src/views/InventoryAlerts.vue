<template>
  <div class="min-h-screen bg-gradient-to-br from-neutral-900 via-neutral-800 to-neutral-900 p-4 lg:p-6">
    <!-- Header Section -->
    <div class="glass-container mb-6 p-6 transform hover:scale-[1.02] transition-all duration-300">
      <div class="flex flex-col lg:flex-row lg:items-center lg:justify-between space-y-4 lg:space-y-0">
        <div class="text-center lg:text-left">
          <div class="flex items-center justify-center lg:justify-start mb-2">
            <div class="bg-gradient-to-r from-red-400 to-orange-400 p-3 rounded-full mr-3">
              <i class="pi pi-exclamation-triangle text-white text-2xl"></i>
            </div>
            <h1 class="text-3xl lg:text-4xl font-bold text-white">🚨 Alertas de Inventario</h1>
            <span class="ml-3 px-3 py-1 bg-red-500/20 text-red-300 rounded-full text-sm font-semibold">Nuevo</span>
          </div>
          <p class="text-gray-300 text-lg">Monitoreo en tiempo real de stock bajo y crítico 📊</p>
        </div>
        
        <!-- Alert Summary -->
        <div class="glass-card p-4">
          <div class="grid grid-cols-4 gap-4 text-center">
            <div class="transform hover:scale-110 transition-transform duration-200">
              <div class="bg-gradient-to-r from-red-400 to-red-500 text-white p-3 rounded-full mx-auto w-12 h-12 flex items-center justify-center mb-2">
                <span class="font-bold text-lg">{{ alertSystem.alertCounts.total || 0 }}</span>
              </div>
              <p class="text-xs text-gray-300">Total</p>
            </div>
            <div class="transform hover:scale-110 transition-transform duration-200">
              <div class="bg-gradient-to-r from-red-500 to-red-600 text-white p-3 rounded-full mx-auto w-12 h-12 flex items-center justify-center mb-2">
                <span class="font-bold text-lg">{{ alertSystem.alertCounts.critical || 0 }}</span>
              </div>
              <p class="text-xs text-gray-300">Críticas</p>
            </div>
            <div class="transform hover:scale-110 transition-transform duration-200">
              <div class="bg-gradient-to-r from-yellow-400 to-yellow-500 text-white p-3 rounded-full mx-auto w-12 h-12 flex items-center justify-center mb-2">
                <span class="font-bold text-lg">{{ alertSystem.alertCounts.lowStock || 0 }}</span>
              </div>
              <p class="text-xs text-gray-300">Stock Bajo</p>
            </div>
            <div class="transform hover:scale-110 transition-transform duration-200">
              <div class="bg-gradient-to-r from-purple-400 to-purple-500 text-white p-3 rounded-full mx-auto w-12 h-12 flex items-center justify-center mb-2">
                <span class="font-bold text-lg">{{ alertSystem.alertCounts.roomsAffected || 0 }}</span>
              </div>
              <p class="text-xs text-gray-300">Habitaciones</p>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- Quick Actions -->
    <div class="glass-container mb-6 p-6">
      <div class="flex items-center justify-between mb-4">
        <div class="flex items-center">
          <i class="pi pi-bolt text-yellow-400 text-xl mr-2"></i>
          <h3 class="text-xl font-bold text-white">⚡ Acciones Rápidas</h3>
        </div>
        <div class="flex items-center space-x-2">
          <span class="text-gray-300 text-sm">Auto-refresh:</span>
          <div class="glass-card p-1">
            <div class="w-3 h-3 bg-green-400 rounded-full animate-pulse"></div>
          </div>
        </div>
      </div>
      
      <div class="grid grid-cols-1 md:grid-cols-3 gap-4">
        <button
          @click="acknowledgeAllAlerts"
          :disabled="alertSystem.saving || alertSystem.activeAlerts.length === 0"
          class="glass-button p-4 text-white hover:bg-white/20 transition-all disabled:opacity-50"
        >
          <i class="pi pi-check-circle text-green-400 text-2xl mb-2"></i>
          <h4 class="font-semibold">Reconocer Todas</h4>
          <p class="text-sm text-gray-300">{{ alertSystem.activeAlerts.length }} alertas pendientes</p>
        </button>
        
        <button
          @click="showCriticalOnly = !showCriticalOnly"
          class="glass-button p-4 text-white hover:bg-white/20 transition-all"
          :class="{ 'bg-red-500/30': showCriticalOnly }"
        >
          <i class="pi pi-exclamation-triangle text-red-400 text-2xl mb-2"></i>
          <h4 class="font-semibold">Solo Críticas</h4>
          <p class="text-sm text-gray-300">{{ alertSystem.criticalAlerts.length }} alertas críticas</p>
        </button>
        
        <button
          @click="refreshAlerts"
          :disabled="alertSystem.loading"
          class="glass-button p-4 text-white hover:bg-white/20 transition-all disabled:opacity-50"
        >
          <i :class="alertSystem.loading ? 'pi pi-spinner pi-spin' : 'pi pi-refresh'" class="text-blue-400 text-2xl mb-2"></i>
          <h4 class="font-semibold">Actualizar</h4>
          <p class="text-sm text-gray-300">Última actualización hace {{ timeSinceUpdate }}s</p>
        </button>
      </div>
    </div>

    <!-- Alerts by Room -->
    <div v-if="alertSystem.alertsByRoom.length > 0" class="glass-container mb-6 p-6">
      <div class="flex items-center mb-6">
        <i class="pi pi-home text-primary-400 text-xl mr-2"></i>
        <h3 class="text-xl font-bold text-white">🏨 Alertas por Habitación</h3>
      </div>
      
      <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
        <div
          v-for="room in filteredRoomAlerts"
          :key="room.roomId"
          class="glass-card p-4 hover:bg-white/15 transition-all duration-300"
        >
          <div class="flex items-center justify-between mb-3">
            <div>
              <h4 class="text-white font-semibold text-lg">{{ room.roomName }}</h4>
              <p class="text-gray-300 text-sm">{{ room.alerts.length }} alertas activas</p>
            </div>
            <div class="flex items-center space-x-1">
              <div
                v-for="alert in room.alerts.slice(0, 3)"
                :key="alert.alertaId"
                class="w-3 h-3 rounded-full"
                :class="{
                  'bg-red-500': alert.severidad === 'Critica',
                  'bg-orange-500': alert.severidad === 'Alta',
                  'bg-yellow-500': alert.severidad === 'Media',
                  'bg-blue-500': alert.severidad === 'Baja'
                }"
              ></div>
              <span v-if="room.alerts.length > 3" class="text-gray-400 text-xs">+{{ room.alerts.length - 3 }}</span>
            </div>
          </div>
          
          <div class="space-y-2 mb-4">
            <div
              v-for="alert in room.alerts.slice(0, 2)"
              :key="alert.alertaId"
              class="text-sm"
            >
              <div class="flex items-center justify-between">
                <span class="text-gray-300">{{ alert.articuloNombre }}</span>
                <span class="text-white font-semibold">{{ alert.cantidadActual }}</span>
              </div>
              <div class="flex items-center space-x-2">
                <span
                  class="px-2 py-1 rounded-full text-xs"
                  :class="{
                    'bg-red-500/20 text-red-300': alert.tipoAlerta === 'StockAgotado',
                    'bg-orange-500/20 text-orange-300': alert.tipoAlerta === 'StockCritico',
                    'bg-yellow-500/20 text-yellow-300': alert.tipoAlerta === 'StockBajo'
                  }"
                >
                  {{ alert.tipoAlerta.replace('Stock', '') }}
                </span>
                <span class="text-gray-400 text-xs">{{ alert.severidad }}</span>
              </div>
            </div>
          </div>
          
          <div class="flex space-x-2">
            <button
              @click="acknowledgeRoomAlerts(room)"
              class="flex-1 glass-button py-2 text-white hover:bg-green-500/20 text-sm"
            >
              <i class="pi pi-check mr-1"></i>
              Reconocer
            </button>
            <button
              @click="viewRoomDetails(room)"
              class="flex-1 glass-button py-2 text-white hover:bg-blue-500/20 text-sm"
            >
              <i class="pi pi-eye mr-1"></i>
              Ver Detalles
            </button>
          </div>
        </div>
      </div>
    </div>

    <!-- Detailed Alerts List -->
    <div class="glass-container p-6">
      <div class="flex items-center justify-between mb-6">
        <div class="flex items-center">
          <i class="pi pi-list text-primary-400 text-xl mr-2"></i>
          <h3 class="text-xl font-bold text-white">📋 Lista Detallada de Alertas</h3>
        </div>
        
        <!-- Filter Controls -->
        <div class="flex space-x-2">
          <select
            v-model="severityFilter"
            class="glass-input px-3 py-2 text-sm"
          >
            <option value="">Todas las severidades</option>
            <option value="Critica">Crítica</option>
            <option value="Alta">Alta</option>
            <option value="Media">Media</option>
            <option value="Baja">Baja</option>
          </select>
          
          <select
            v-model="typeFilter"
            class="glass-input px-3 py-2 text-sm"
          >
            <option value="">Todos los tipos</option>
            <option value="StockAgotado">Sin Stock</option>
            <option value="StockCritico">Stock Crítico</option>
            <option value="StockBajo">Stock Bajo</option>
          </select>
        </div>
      </div>

      <!-- Loading State -->
      <div v-if="alertSystem.loading" class="text-center py-16">
        <div class="glass-card max-w-md mx-auto p-8">
          <div class="bg-gradient-to-r from-primary-400 to-accent-400 p-6 rounded-full mx-auto w-24 h-24 flex items-center justify-center mb-4">
            <i class="pi pi-spinner pi-spin text-white text-4xl"></i>
          </div>
          <h3 class="text-xl text-white font-bold mb-2">🔄 Cargando alertas...</h3>
          <p class="text-gray-300">Obteniendo información actualizada</p>
        </div>
      </div>

      <!-- Alerts Table -->
      <div v-else-if="filteredAlerts.length > 0" class="overflow-x-auto">
        <table class="w-full text-left">
          <thead>
            <tr class="border-b border-white/10">
              <th class="text-gray-300 font-semibold py-3 px-4">Artículo</th>
              <th class="text-gray-300 font-semibold py-3 px-4">Habitación</th>
              <th class="text-gray-300 font-semibold py-3 px-4">Tipo</th>
              <th class="text-gray-300 font-semibold py-3 px-4">Severidad</th>
              <th class="text-gray-300 font-semibold py-3 px-4">Stock Actual</th>
              <th class="text-gray-300 font-semibold py-3 px-4">Fecha</th>
              <th class="text-gray-300 font-semibold py-3 px-4">Acciones</th>
            </tr>
          </thead>
          <tbody>
            <tr
              v-for="alert in filteredAlerts"
              :key="alert.alertaId"
              class="border-b border-white/5 hover:bg-white/5 transition-colors"
            >
              <td class="py-3 px-4">
                <div class="text-white font-semibold">{{ alert.articuloNombre }}</div>
              </td>
              <td class="py-3 px-4">
                <div class="text-gray-300">{{ alert.ubicacionNombre || `Habitación ${alert.ubicacionId}` }}</div>
              </td>
              <td class="py-3 px-4">
                <span
                  class="px-2 py-1 rounded-full text-xs font-medium"
                  :class="{
                    'bg-red-500/20 text-red-300': alert.tipoAlerta === 'StockAgotado',
                    'bg-orange-500/20 text-orange-300': alert.tipoAlerta === 'StockCritico',
                    'bg-yellow-500/20 text-yellow-300': alert.tipoAlerta === 'StockBajo'
                  }"
                >
                  {{ alert.tipoAlerta.replace('Stock', '') }}
                </span>
              </td>
              <td class="py-3 px-4">
                <span
                  class="px-2 py-1 rounded-full text-xs font-medium"
                  :class="{
                    'bg-red-500/20 text-red-300': alert.severidad === 'Critica',
                    'bg-orange-500/20 text-orange-300': alert.severidad === 'Alta',
                    'bg-yellow-500/20 text-yellow-300': alert.severidad === 'Media',
                    'bg-blue-500/20 text-blue-300': alert.severidad === 'Baja'
                  }"
                >
                  {{ alert.severidad }}
                </span>
              </td>
              <td class="py-3 px-4">
                <div class="text-white font-bold">{{ alert.cantidadActual }}</div>
                <div class="text-gray-400 text-xs">
                  Min: {{ alert.umbralStockBajo || 'N/A' }}
                </div>
              </td>
              <td class="py-3 px-4">
                <div class="text-gray-300 text-sm">
                  {{ formatDate(alert.fechaCreacion) }}
                </div>
              </td>
              <td class="py-3 px-4">
                <div class="flex space-x-2">
                  <button
                    @click="acknowledgeAlert(alert)"
                    :disabled="alert.reconocida || alertSystem.saving"
                    class="glass-button px-3 py-1 text-xs text-white hover:bg-green-500/20 disabled:opacity-50"
                  >
                    <i class="pi pi-check mr-1"></i>
                    {{ alert.reconocida ? 'Reconocida' : 'Reconocer' }}
                  </button>
                </div>
              </td>
            </tr>
          </tbody>
        </table>
      </div>

      <!-- Empty State -->
      <div v-else class="text-center py-16">
        <div class="glass-card max-w-md mx-auto p-8">
          <div class="bg-gradient-to-r from-green-400 to-green-500 p-6 rounded-full mx-auto w-24 h-24 flex items-center justify-center mb-4">
            <i class="pi pi-check-circle text-white text-4xl"></i>
          </div>
          <h3 class="text-2xl text-white font-bold mb-2">✅ ¡Todo en orden!</h3>
          <p class="text-gray-300">No hay alertas activas en este momento</p>
        </div>
      </div>
    </div>

    <!-- Toast for notifications -->
    <Toast />
    <ConfirmDialog />
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, onUnmounted, type Ref, type ComputedRef } from 'vue'
import { useToast, type ToastServiceMethods } from 'primevue/usetoast'
import { useConfirm, type ConfirmationService } from 'primevue/useconfirm'
import { useGlobalAlerts } from '../composables/useInventoryAlerts'
import Toast from 'primevue/toast'
import ConfirmDialog from 'primevue/confirmdialog'
import type { InventoryAlertDto } from '../types'

// Types for component state
interface RoomAlertGroup {
  roomId: number
  roomName: string
  alerts: InventoryAlertDto[]
}

type AlertSeverity = 'Critica' | 'Alta' | 'Media' | 'Baja'
type AlertType = 'StockBajo' | 'StockCritico' | 'StockAgotado'

// Composables
const toast: ToastServiceMethods = useToast()
const confirm: ConfirmationService = useConfirm()
const alertSystem = useGlobalAlerts()

// State
const showCriticalOnly: Ref<boolean> = ref(false)
const severityFilter: Ref<string> = ref('')
const typeFilter: Ref<string> = ref('')
const timeSinceUpdate: Ref<number> = ref(0)
const updateTimer: Ref<ReturnType<typeof setInterval> | null> = ref(null)

// Computed
const filteredRoomAlerts: ComputedRef<RoomAlertGroup[]> = computed(() => {
  if (showCriticalOnly.value) {
    return alertSystem.alertsByRoom.value.filter(room => 
      room.alerts.some(alert => alert.severidad === 'Critica')
    )
  }
  return alertSystem.alertsByRoom.value
})

const filteredAlerts: ComputedRef<InventoryAlertDto[]> = computed(() => {
  let alerts = [...alertSystem.activeAlerts.value]
  
  if (showCriticalOnly.value) {
    alerts = alerts.filter(alert => alert.severidad === 'Critica')
  }
  
  if (severityFilter.value) {
    alerts = alerts.filter(alert => alert.severidad === severityFilter.value)
  }
  
  if (typeFilter.value) {
    alerts = alerts.filter(alert => alert.tipoAlerta === typeFilter.value)
  }
  
  return alerts.sort((a, b) => {
    // Sort by severity priority
    const severityPriority: Record<AlertSeverity, number> = { 
      Critica: 4, Alta: 3, Media: 2, Baja: 1 
    }
    return severityPriority[b.severidad as AlertSeverity] - severityPriority[a.severidad as AlertSeverity]
  })
})

// Methods
const acknowledgeAlert = async (alert: InventoryAlertDto): Promise<void> => {
  try {
    await alertSystem.acknowledgeAlert(alert.alertaId, 'Reconocida desde panel de alertas')
    toast.add({
      severity: 'success',
      summary: 'Alerta Reconocida',
      detail: `Alerta de ${alert.articuloNombre} reconocida correctamente`,
      life: 3000
    })
  } catch (error) {
    toast.add({
      severity: 'error',
      summary: 'Error',
      detail: 'Error al reconocer la alerta',
      life: 5000
    })
  }
}

const acknowledgeAllAlerts = async (): Promise<void> => {
  const confirmed = await new Promise<boolean>((resolve) => {
    confirm.require({
      message: `¿Estás seguro de reconocer todas las ${alertSystem.activeAlerts.value.length} alertas activas?`,
      header: 'Confirmar Reconocimiento Masivo',
      icon: 'pi pi-exclamation-triangle',
      acceptLabel: 'Sí, reconocer todas',
      rejectLabel: 'Cancelar',
      acceptClass: 'p-button-warning',
      accept: () => resolve(true),
      reject: () => resolve(false)
    })
  })

  if (!confirmed) return

  try {
    const alertIds = alertSystem.activeAlerts.value.map(a => a.alertaId)
    await alertSystem.acknowledgeMultipleAlerts(alertIds, 'Reconocimiento masivo desde panel')
    
    toast.add({
      severity: 'success',
      summary: 'Alertas Reconocidas',
      detail: `${alertIds.length} alertas reconocidas correctamente`,
      life: 3000
    })
  } catch (error) {
    toast.add({
      severity: 'error',
      summary: 'Error',
      detail: 'Error al reconocer las alertas',
      life: 5000
    })
  }
}

const acknowledgeRoomAlerts = async (room: RoomAlertGroup): Promise<void> => {
  try {
    const alertIds = room.alerts.map(a => a.alertaId)
    await alertSystem.acknowledgeMultipleAlerts(alertIds, `Reconocidas para ${room.roomName}`)
    
    toast.add({
      severity: 'success',
      summary: 'Alertas de Habitación Reconocidas',
      detail: `${alertIds.length} alertas de ${room.roomName} reconocidas`,
      life: 3000
    })
  } catch (error) {
    toast.add({
      severity: 'error',
      summary: 'Error',
      detail: `Error al reconocer alertas de ${room.roomName}`,
      life: 5000
    })
  }
}

const viewRoomDetails = (room: RoomAlertGroup): void => {
  toast.add({
    severity: 'info',
    summary: 'Funcionalidad',
    detail: `Detalles de ${room.roomName} - Próximamente`,
    life: 3000
  })
}

const refreshAlerts = async (): Promise<void> => {
  try {
    await alertSystem.refresh()
    timeSinceUpdate.value = 0
    toast.add({
      severity: 'success',
      summary: 'Actualizado',
      detail: 'Alertas actualizadas correctamente',
      life: 3000
    })
  } catch (error) {
    toast.add({
      severity: 'error',
      summary: 'Error',
      detail: 'Error al actualizar alertas',
      life: 5000
    })
  }
}

const formatDate = (dateString: string): string => {
  return new Date(dateString).toLocaleString('es-ES', {
    day: '2-digit',
    month: '2-digit',
    year: 'numeric',
    hour: '2-digit',
    minute: '2-digit'
  })
}

// Lifecycle
onMounted(() => {
  // Update timer
  updateTimer.value = setInterval(() => {
    timeSinceUpdate.value++
  }, 1000)
})

onUnmounted(() => {
  if (updateTimer.value) {
    clearInterval(updateTimer.value)
  }
})
</script>

<style scoped>
/* Glass effect styles */
.glass-container {
  @apply bg-white/5 backdrop-blur-2xl border border-white/20 rounded-3xl;
}

.glass-card {
  @apply bg-white/10 backdrop-blur-md border border-white/20 rounded-xl;
}

.glass-button {
  @apply bg-white/10 hover:bg-white/20 backdrop-blur-sm border border-white/30 rounded-lg transition-all;
}

.glass-input {
  @apply bg-white/10 backdrop-blur-sm border border-white/30 rounded-lg text-white placeholder-gray-300;
}

.glass-input:focus {
  @apply ring-2 ring-primary-400 border-primary-400 outline-none;
}
</style>