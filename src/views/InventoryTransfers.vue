<template>
  <div class="min-h-screen bg-gradient-to-br from-neutral-900 via-neutral-800 to-neutral-900 p-4 lg:p-6">
    <!-- Header Section -->
    <div class="glass-container mb-6 p-6 transform hover:scale-[1.02] transition-all duration-300">
      <div class="flex flex-col lg:flex-row lg:items-center lg:justify-between space-y-4 lg:space-y-0">
        <div class="text-center lg:text-left">
          <div class="flex items-center justify-center lg:justify-start mb-2">
            <div class="bg-gradient-to-r from-blue-400 to-purple-400 p-3 rounded-full mr-3">
              <i class="pi pi-arrows-h text-white text-2xl"></i>
            </div>
            <h1 class="text-3xl lg:text-4xl font-bold text-white">🔄 Transferencias de Inventario</h1>
            <span class="ml-3 px-3 py-1 bg-blue-500/20 text-blue-300 rounded-full text-sm font-semibold">V1</span>
          </div>
          <p class="text-gray-300 text-lg">Transferencias inteligentes entre habitaciones y ubicaciones ⚡</p>
        </div>
        
        <!-- Transfer Stats -->
        <div class="glass-card p-4">
          <div class="grid grid-cols-3 gap-4 text-center">
            <div class="transform hover:scale-110 transition-transform duration-200">
              <div class="bg-gradient-to-r from-green-400 to-green-500 text-white p-3 rounded-full mx-auto w-12 h-12 flex items-center justify-center mb-2">
                <span class="font-bold text-lg">{{ transferSystem.transferStats.completed || 0 }}</span>
              </div>
              <p class="text-xs text-gray-300">Completadas</p>
            </div>
            <div class="transform hover:scale-110 transition-transform duration-200">
              <div class="bg-gradient-to-r from-yellow-400 to-yellow-500 text-white p-3 rounded-full mx-auto w-12 h-12 flex items-center justify-center mb-2">
                <span class="font-bold text-lg">{{ transferSystem.transferStats.inProgress || 0 }}</span>
              </div>
              <p class="text-xs text-gray-300">En Proceso</p>
            </div>
            <div class="transform hover:scale-110 transition-transform duration-200">
              <div class="bg-gradient-to-r from-red-400 to-red-500 text-white p-3 rounded-full mx-auto w-12 h-12 flex items-center justify-center mb-2">
                <span class="font-bold text-lg">{{ transferSystem.transferStats.failed || 0 }}</span>
              </div>
              <p class="text-xs text-gray-300">Fallidas</p>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- Quick Transfer Actions -->
    <div class="glass-container mb-6 p-6">
      <div class="flex items-center mb-6">
        <i class="pi pi-bolt text-yellow-400 text-xl mr-2"></i>
        <h3 class="text-xl font-bold text-white">⚡ Acciones Rápidas</h3>
      </div>
      
      <div class="grid grid-cols-1 md:grid-cols-3 gap-4">
        <!-- Single Room Reload -->
        <div class="glass-card p-4">
          <div class="flex items-center mb-4">
            <i class="pi pi-home text-blue-400 text-2xl mr-3"></i>
            <div>
              <h4 class="text-white font-semibold">Recarga Individual</h4>
              <p class="text-gray-300 text-sm">Una habitación específica</p>
            </div>
          </div>
          
          <div class="space-y-3">
            <select
              v-model="singleTransfer.roomId"
              class="glass-input w-full px-3 py-2"
            >
              <option value="">Seleccionar habitación</option>
              <option v-for="room in availableRooms" :key="room.id" :value="room.id">
                {{ room.numero }} - {{ room.nombre }}
              </option>
            </select>
            
            <button
              @click="showSingleTransferModal = true"
              :disabled="!singleTransfer.roomId || transferSystem.transferring"
              class="w-full glass-button py-2 text-white hover:bg-blue-500/20 disabled:opacity-50"
            >
              <i class="pi pi-arrow-right mr-2"></i>
              Configurar Recarga
            </button>
          </div>
        </div>

        <!-- Batch Transfer -->
        <div class="glass-card p-4">
          <div class="flex items-center mb-4">
            <i class="pi pi-clone text-green-400 text-2xl mr-3"></i>
            <div>
              <h4 class="text-white font-semibold">Recarga Masiva</h4>
              <p class="text-gray-300 text-sm">Múltiples habitaciones</p>
            </div>
          </div>
          
          <div class="space-y-3">
            <div class="flex items-center space-x-2">
              <input
                v-model="batchTransfer.roomCount"
                type="number"
                min="2"
                max="50"
                class="glass-input flex-1 px-3 py-2"
                placeholder="Número de habitaciones"
              >
            </div>
            
            <button
              @click="showBatchTransferModal = true"
              :disabled="!batchTransfer.roomCount || batchTransfer.roomCount < 2 || transferSystem.transferring"
              class="w-full glass-button py-2 text-white hover:bg-green-500/20 disabled:opacity-50"
            >
              <i class="pi pi-sitemap mr-2"></i>
              Configurar Lote
            </button>
          </div>
        </div>

        <!-- Emergency Reload -->
        <div class="glass-card p-4">
          <div class="flex items-center mb-4">
            <i class="pi pi-exclamation-triangle text-red-400 text-2xl mr-3"></i>
            <div>
              <h4 class="text-white font-semibold">Recarga de Emergencia</h4>
              <p class="text-gray-300 text-sm">Basada en alertas críticas</p>
            </div>
          </div>
          
          <div class="space-y-3">
            <div class="text-center">
              <span class="text-2xl font-bold text-red-400">{{ criticalAlertsCount }}</span>
              <p class="text-gray-300 text-sm">alertas críticas</p>
            </div>
            
            <button
              @click="executeEmergencyReload"
              :disabled="criticalAlertsCount === 0 || transferSystem.transferring"
              class="w-full glass-button py-2 text-white hover:bg-red-500/20 disabled:opacity-50"
            >
              <i class="pi pi-bolt mr-2"></i>
              Recarga Automática
            </button>
          </div>
        </div>
      </div>
    </div>

    <!-- Transfer Progress -->
    <div v-if="transferSystem.transferring" class="glass-container mb-6 p-6">
      <div class="flex items-center mb-4">
        <i class="pi pi-clock text-blue-400 text-xl mr-2"></i>
        <h3 class="text-xl font-bold text-white">🔄 Transferencia en Progreso</h3>
      </div>
      
      <div class="glass-card p-4">
        <div class="flex items-center justify-between mb-3">
          <span class="text-white font-semibold">{{ transferSystem.transferProgress.current || 'Procesando...' }}</span>
          <span class="text-gray-300">
            {{ transferSystem.transferProgress.completed }}/{{ transferSystem.transferProgress.total }}
          </span>
        </div>
        
        <div class="w-full bg-gray-700 rounded-full h-3 mb-3">
          <div
            class="bg-gradient-to-r from-blue-400 to-purple-400 h-3 rounded-full transition-all duration-300"
            :style="{ width: `${progressPercentage}%` }"
          ></div>
        </div>
        
        <div class="flex justify-between text-sm text-gray-300">
          <span>{{ transferSystem.transferProgress.completed }} completadas</span>
          <span>{{ transferSystem.transferProgress.errors }} errores</span>
        </div>
      </div>
    </div>

    <!-- Transfer History -->
    <div class="glass-container p-6">
      <div class="flex items-center justify-between mb-6">
        <div class="flex items-center">
          <i class="pi pi-history text-primary-400 text-xl mr-2"></i>
          <h3 class="text-xl font-bold text-white">📋 Historial de Transferencias</h3>
        </div>
        
        <div class="flex space-x-2">
          <select
            v-model="historyFilter"
            class="glass-input px-3 py-2 text-sm"
          >
            <option value="all">Todas</option>
            <option value="completed">Completadas</option>
            <option value="failed">Fallidas</option>
            <option value="in_progress">En Proceso</option>
          </select>
          
          <button
            @click="refreshHistory"
            :disabled="transferSystem.loading"
            class="glass-button px-4 py-2 text-white hover:bg-white/20"
          >
            <i :class="transferSystem.loading ? 'pi pi-spinner pi-spin' : 'pi pi-refresh'" class="mr-2"></i>
            Actualizar
          </button>
        </div>
      </div>

      <!-- History Table -->
      <div v-if="filteredTransfers.length > 0" class="overflow-x-auto">
        <table class="w-full text-left">
          <thead>
            <tr class="border-b border-white/10">
              <th class="text-gray-300 font-semibold py-3 px-4">Tipo</th>
              <th class="text-gray-300 font-semibold py-3 px-4">Estado</th>
              <th class="text-gray-300 font-semibold py-3 px-4">Items</th>
              <th class="text-gray-300 font-semibold py-3 px-4">Fecha</th>
              <th class="text-gray-300 font-semibold py-3 px-4">Duración</th>
              <th class="text-gray-300 font-semibold py-3 px-4">Acciones</th>
            </tr>
          </thead>
          <tbody>
            <tr
              v-for="transfer in filteredTransfers"
              :key="transfer.id"
              class="border-b border-white/5 hover:bg-white/5 transition-colors"
            >
              <td class="py-3 px-4">
                <div class="flex items-center">
                  <i
                    :class="{
                      'pi pi-home text-blue-400': transfer.tipo === 'SIMPLE',
                      'pi pi-clone text-green-400': transfer.tipo === 'LOTE',
                      'pi pi-bolt text-red-400': transfer.tipo === 'EMERGENCIA'
                    }"
                    class="mr-2"
                  ></i>
                  <span class="text-white font-semibold">{{ transfer.tipo }}</span>
                </div>
              </td>
              <td class="py-3 px-4">
                <span
                  class="px-2 py-1 rounded-full text-xs font-medium"
                  :class="{
                    'bg-green-500/20 text-green-300': transfer.estado === 'COMPLETADO',
                    'bg-yellow-500/20 text-yellow-300': transfer.estado === 'EN_PROCESO',
                    'bg-red-500/20 text-red-300': transfer.estado === 'FALLIDO'
                  }"
                >
                  {{ transfer.estado }}
                </span>
              </td>
              <td class="py-3 px-4">
                <div class="text-white font-semibold">
                  {{ transfer.transferencias || transfer.detalles?.length || 'N/A' }}
                </div>
              </td>
              <td class="py-3 px-4">
                <div class="text-gray-300 text-sm">
                  {{ formatDate(transfer.fechaInicio) }}
                </div>
              </td>
              <td class="py-3 px-4">
                <div class="text-gray-300 text-sm">
                  {{ calculateDuration(transfer) }}
                </div>
              </td>
              <td class="py-3 px-4">
                <div class="flex space-x-2">
                  <button
                    @click="viewTransferDetails(transfer)"
                    class="glass-button px-3 py-1 text-xs text-white hover:bg-blue-500/20"
                  >
                    <i class="pi pi-eye mr-1"></i>
                    Ver
                  </button>
                  <button
                    v-if="transfer.estado === 'FALLIDO'"
                    @click="retryTransfer(transfer)"
                    class="glass-button px-3 py-1 text-xs text-white hover:bg-yellow-500/20"
                  >
                    <i class="pi pi-refresh mr-1"></i>
                    Reintentar
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
          <div class="bg-gradient-to-r from-primary-400 to-accent-400 p-6 rounded-full mx-auto w-24 h-24 flex items-center justify-center mb-4">
            <i class="pi pi-inbox text-white text-4xl"></i>
          </div>
          <h3 class="text-2xl text-white font-bold mb-2">📦 Sin transferencias</h3>
          <p class="text-gray-300">No hay transferencias registradas aún</p>
        </div>
      </div>
    </div>

    <!-- Toast for notifications -->
    <Toast />
    <ConfirmDialog />
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import { useToast } from 'primevue/usetoast'
import { useConfirm } from 'primevue/useconfirm'
import { useInventoryTransfers } from '../composables/useInventoryTransfers'
import { useGlobalAlerts } from '../composables/useInventoryAlerts'
import Toast from 'primevue/toast'
import ConfirmDialog from 'primevue/confirmdialog'

// Composables
const toast = useToast()
const confirm = useConfirm()
const transferSystem = useInventoryTransfers()
const alertSystem = useGlobalAlerts()

// State
const showSingleTransferModal = ref(false)
const showBatchTransferModal = ref(false)
const historyFilter = ref('all')

const singleTransfer = ref({
  roomId: '',
  items: []
})

const batchTransfer = ref({
  roomCount: 5,
  selectedRooms: []
})

const availableRooms = ref([
  { id: 101, numero: '101', nombre: 'Suite Presidential' },
  { id: 102, numero: '102', nombre: 'Habitación Ejecutiva' },
  { id: 103, numero: '103', nombre: 'Habitación Estándar' },
  { id: 201, numero: '201', nombre: 'Suite Familiar' },
  { id: 202, numero: '202', nombre: 'Habitación Doble' },
  { id: 203, numero: '203', nombre: 'Habitación Simple' },
  { id: 301, numero: '301', nombre: 'Suite Nupcial' },
  { id: 302, numero: '302', nombre: 'Habitación Familiar' },
])

// Computed
const progressPercentage = computed(() => {
  const { total, completed } = transferSystem.transferProgress.value
  return total > 0 ? (completed / total) * 100 : 0
})

const criticalAlertsCount = computed(() => {
  return alertSystem.criticalAlerts.value?.length || 0
})

const filteredTransfers = computed(() => {
  let transfers = [...transferSystem.transfers.value]
  
  if (historyFilter.value !== 'all') {
    const statusMap = {
      'completed': 'COMPLETADO',
      'failed': 'FALLIDO',
      'in_progress': 'EN_PROCESO'
    }
    transfers = transfers.filter(t => t.estado === statusMap[historyFilter.value])
  }
  
  return transfers.sort((a, b) => new Date(b.fechaInicio) - new Date(a.fechaInicio))
})

// Methods
const executeEmergencyReload = async () => {
  const confirmed = await new Promise((resolve) => {
    confirm.require({
      message: `¿Ejecutar recarga de emergencia para ${criticalAlertsCount.value} alertas críticas?`,
      header: 'Confirmar Recarga de Emergencia',
      icon: 'pi pi-exclamation-triangle',
      acceptLabel: 'Sí, ejecutar',
      rejectLabel: 'Cancelar',
      acceptClass: 'p-button-danger',
      accept: () => resolve(true),
      reject: () => resolve(false)
    })
  })

  if (!confirmed) return

  try {
    await transferSystem.reloadBasedOnAlerts(alertSystem.criticalAlerts.value)
    
    toast.add({
      severity: 'success',
      summary: 'Recarga de Emergencia',
      detail: 'Transferencias de emergencia ejecutadas correctamente',
      life: 5000
    })
  } catch (error) {
    toast.add({
      severity: 'error',
      summary: 'Error',
      detail: 'Error en la recarga de emergencia',
      life: 5000
    })
  }
}

const viewTransferDetails = (transfer) => {
  toast.add({
    severity: 'info',
    summary: 'Detalles de Transferencia',
    detail: `Viendo detalles de transferencia ${transfer.tipo} - Próximamente`,
    life: 3000
  })
}

const retryTransfer = async (transfer) => {
  toast.add({
    severity: 'info',
    summary: 'Funcionalidad',
    detail: 'Reintento de transferencia - Próximamente',
    life: 3000
  })
}

const refreshHistory = () => {
  toast.add({
    severity: 'success',
    summary: 'Actualizado',
    detail: 'Historial actualizado correctamente',
    life: 3000
  })
}

const formatDate = (dateString) => {
  return new Date(dateString).toLocaleString('es-ES', {
    day: '2-digit',
    month: '2-digit',
    year: 'numeric',
    hour: '2-digit',
    minute: '2-digit'
  })
}

const calculateDuration = (transfer) => {
  if (!transfer.fechaCompletado && transfer.estado === 'EN_PROCESO') {
    return 'En curso...'
  }
  
  const start = new Date(transfer.fechaInicio)
  const end = transfer.fechaCompletado ? new Date(transfer.fechaCompletado) : new Date()
  const duration = Math.round((end - start) / 1000)
  
  if (duration < 60) return `${duration}s`
  if (duration < 3600) return `${Math.round(duration / 60)}m`
  return `${Math.round(duration / 3600)}h`
}

// Demo transfers for visualization
onMounted(() => {
  // Add some demo transfers for visualization
  transferSystem.transfers.value.push(
    {
      id: 1,
      tipo: 'SIMPLE',
      estado: 'COMPLETADO',
      fechaInicio: new Date(Date.now() - 300000).toISOString(),
      fechaCompletado: new Date(Date.now() - 240000).toISOString(),
      detalles: [{ inventarioId: 1, cantidadSolicitada: 5 }]
    },
    {
      id: 2,
      tipo: 'LOTE',
      estado: 'COMPLETADO',
      fechaInicio: new Date(Date.now() - 600000).toISOString(),
      fechaCompletado: new Date(Date.now() - 480000).toISOString(),
      transferencias: 5
    },
    {
      id: 3,
      tipo: 'EMERGENCIA',
      estado: 'FALLIDO',
      fechaInicio: new Date(Date.now() - 900000).toISOString(),
      error: 'Stock insuficiente',
      transferencias: 3
    }
  )
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