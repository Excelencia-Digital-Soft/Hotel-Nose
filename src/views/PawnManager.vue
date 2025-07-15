<template>
  <div class="min-h-screen bg-gradient-to-br from-slate-900 via-purple-900 to-slate-900 p-6">
    <!-- Glass Container -->
    <div class="max-w-7xl mx-auto">
      <!-- Header -->
      <div class="glass-card mb-8">
        <div class="flex items-center justify-between">
          <div>
            <h1 class="text-3xl font-bold text-white lexend-exa mb-2">
              Gestión de Empeños
            </h1>
            <p class="text-white/70">Administrar empeños y procesar pagos</p>
          </div>
          <div class="flex items-center space-x-4">
            <div class="text-right">
              <div class="text-2xl font-bold text-white">{{ statistics.total }}</div>
              <div class="text-white/70 text-sm">Empeños Activos</div>
            </div>
            <div class="text-right">
              <div class="text-2xl font-bold text-white">{{ statistics.totalAmount }}</div>
              <div class="text-white/70 text-sm">Valor Total</div>
            </div>
          </div>
        </div>
      </div>

      <!-- Filters and Search -->
      <div class="glass-card mb-6">
        <div class="grid lg:grid-cols-4 gap-4">
          <!-- Search -->
          <div class="space-y-2">
            <label class="block text-white font-medium text-sm">Buscar</label>
            <div class="relative">
              <i class="pi pi-search absolute left-3 top-1/2 transform -translate-y-1/2 text-white/70"></i>
              <input 
                v-model="searchTerm"
                type="text" 
                placeholder="ID, detalle, monto..."
                class="glass-input w-full pl-10"
              />
            </div>
          </div>
          
          <!-- Status Filter -->
          <div class="space-y-2">
            <label class="block text-white font-medium text-sm">Estado</label>
            <select v-model="statusFilter" class="glass-input w-full">
              <option value="all">Todos los Estados</option>
              <option value="active">Activo</option>
              <option value="warning">En Alerta</option>
              <option value="overdue">Vencido</option>
            </select>
          </div>
          
          <!-- Sort By -->
          <div class="space-y-2">
            <label class="block text-white font-medium text-sm">Ordenar por</label>
            <select v-model="sortBy" class="glass-input w-full">
              <option value="date">Fecha</option>
              <option value="amount">Monto</option>
              <option value="status">Estado</option>
            </select>
          </div>
          
          <!-- Refresh Button -->
          <div class="space-y-2">
            <label class="block text-white font-medium text-sm opacity-0">Acciones</label>
            <button 
              @click="fetchPawns"
              :disabled="isLoading"
              class="glass-button w-full py-3 px-4 rounded-xl font-medium transition-all duration-300 hover:scale-105"
            >
              <i v-if="isLoading" class="pi pi-spin pi-spinner mr-2"></i>
              <i v-else class="pi pi-refresh mr-2"></i>
              Actualizar
            </button>
          </div>
        </div>
      </div>

      <!-- Statistics Cards -->
      <div class="grid lg:grid-cols-4 gap-6 mb-8">
        <!-- Total Pawns -->
        <div class="glass-stats-card">
          <div class="flex items-center justify-between">
            <div>
              <p class="text-white/70 text-sm">Total Empeños</p>
              <p class="text-2xl font-bold text-white">{{ statistics.total }}</p>
            </div>
            <div class="w-12 h-12 bg-blue-500/20 rounded-xl flex items-center justify-center">
              <i class="pi pi-list text-blue-400 text-xl"></i>
            </div>
          </div>
        </div>
        
        <!-- Total Amount -->
        <div class="glass-stats-card">
          <div class="flex items-center justify-between">
            <div>
              <p class="text-white/70 text-sm">Valor Total</p>
              <p class="text-2xl font-bold text-white">{{ statistics.totalAmount }}</p>
            </div>
            <div class="w-12 h-12 bg-green-500/20 rounded-xl flex items-center justify-center">
              <i class="pi pi-dollar text-green-400 text-xl"></i>
            </div>
          </div>
        </div>
        
        <!-- Overdue -->
        <div class="glass-stats-card">
          <div class="flex items-center justify-between">
            <div>
              <p class="text-white/70 text-sm">Vencidos</p>
              <p class="text-2xl font-bold text-red-400">{{ statistics.overdue }}</p>
            </div>
            <div class="w-12 h-12 bg-red-500/20 rounded-xl flex items-center justify-center">
              <i class="pi pi-exclamation-triangle text-red-400 text-xl"></i>
            </div>
          </div>
        </div>
        
        <!-- Average Amount -->
        <div class="glass-stats-card">
          <div class="flex items-center justify-between">
            <div>
              <p class="text-white/70 text-sm">Promedio</p>
              <p class="text-2xl font-bold text-white">{{ statistics.averageAmount }}</p>
            </div>
            <div class="w-12 h-12 bg-purple-500/20 rounded-xl flex items-center justify-center">
              <i class="pi pi-chart-bar text-purple-400 text-xl"></i>
            </div>
          </div>
        </div>
      </div>

      <!-- Pawns List -->
      <div class="glass-card">
        <div class="flex items-center justify-between mb-6">
          <h2 class="text-xl text-white lexend-exa font-bold">
            Lista de Empeños ({{ filteredPawns.length }})
          </h2>
        </div>
        
        <div v-if="isLoading" class="text-center py-12">
          <i class="pi pi-spin pi-spinner text-4xl text-white/50 mb-4 block"></i>
          <p class="text-white/70 text-lg">Cargando empeños...</p>
        </div>
        
        <div v-else-if="filteredPawns.length === 0" class="text-center py-12">
          <i class="pi pi-inbox text-4xl text-white/50 mb-4 block"></i>
          <p class="text-white/70 text-lg">No se encontraron empeños</p>
          <p class="text-white/50 text-sm">Ajusta los filtros o verifica la conexión</p>
        </div>
        
        <div v-else class="space-y-4">
          <div v-for="pawn in filteredPawns" :key="pawn.empeñoID"
            class="glass-pawn-card group">
            <div class="p-6">
              <div class="flex items-start justify-between">
                <!-- Pawn Info -->
                <div class="flex-1 min-w-0">
                  <div class="flex items-center space-x-3 mb-3">
                    <span class="glass-badge text-sm">
                      #{{ pawn.empeñoID }}
                    </span>
                    <span :class="['text-sm font-medium', getStatusColor(pawn.status)]">
                      <i :class="['pi mr-1', getStatusIcon(pawn.status)]"></i>
                      {{ getStatusText(pawn.status) }}
                    </span>
                    <span class="text-white/60 text-sm">
                      {{ pawn.daysOverdue }} días
                    </span>
                  </div>
                  
                  <h3 class="text-lg font-semibold text-white mb-2">
                    {{ pawn.detalle || 'Sin descripción' }}
                  </h3>
                  
                  <div class="grid grid-cols-2 gap-4 text-sm">
                    <div>
                      <span class="text-white/70">Monto:</span>
                      <span class="text-white font-semibold ml-2">{{ pawn.formattedAmount }}</span>
                    </div>
                    <div>
                      <span class="text-white/70">Fecha:</span>
                      <span class="text-white font-semibold ml-2">{{ pawn.formattedDate }}</span>
                    </div>
                    <div>
                      <span class="text-white/70">Visita:</span>
                      <span class="text-white font-semibold ml-2">#{{ pawn.visitaID }}</span>
                    </div>
                    <div>
                      <span class="text-white/70">Estado:</span>
                      <span :class="['font-semibold ml-2', getStatusColor(pawn.status)]">
                        {{ getStatusText(pawn.status) }}
                      </span>
                    </div>
                  </div>
                </div>
                
                <!-- Payment Button -->
                <div class="flex-shrink-0 ml-6">
                  <button 
                    @click="openPaymentModal(pawn)"
                    class="glass-button-primary py-3 px-6 rounded-xl font-medium transition-all duration-300 hover:scale-105"
                  >
                    <i class="pi pi-credit-card mr-2"></i>
                    Pagar
                  </button>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- Payment Modal -->
    <PawnPaymentModal
      v-if="showPaymentModal"
      :pawn="selectedPawn"
      :payment-cards="paymentCards"
      @close="closePaymentModal"
      @payment-completed="handlePaymentCompleted"
    />
  </div>
</template>

<script setup>
import { ref, onMounted, computed } from 'vue'
import { usePawnManager } from '../composables/usePawnManager.js'
import { PawnService } from '../services/pawnService.js'
import { useAuthStore } from '../store/auth.js'
import PawnPaymentModal from '../components/PawnPaymentModal.vue'

// Feature flag for V1 API (set to false to use legacy endpoints)
const USE_V1_API = false

// Auth store
const authStore = useAuthStore()

// Composable
const {
  pawns,
  paymentCards,
  isLoading,
  showPaymentModal,
  selectedPawn,
  filteredPawns,
  statistics,
  searchTerm,
  statusFilter,
  sortBy,
  openPaymentModal,
  closePaymentModal,
  getStatusColor,
  getStatusIcon,
  showSuccess,
  showError
} = usePawnManager()

// Computed
const getStatusText = (status) => {
  switch (status) {
    case 'overdue': return 'Vencido'
    case 'warning': return 'En Alerta'
    case 'active': return 'Activo'
    default: return 'Desconocido'
  }
}

// Lifecycle
onMounted(() => {
  Promise.all([
    fetchPawns(),
    fetchPaymentCards()
  ])
})

// Methods
const fetchPawns = async () => {
  try {
    isLoading.value = true
    const institucionId = authStore.institucionID
    
    if (!institucionId) {
      showError('No se pudo obtener el ID de la institución. Por favor inicia sesión nuevamente.')
      return
    }

    let response
    if (USE_V1_API) {
      response = await PawnService.getPawns(institucionId)
    } else {
      response = await PawnService.legacyGetPawns(institucionId)
    }

    if (response && response.data) {
      pawns.value = response.data
    } else {
      showError('Error al cargar los empeños')
    }
  } catch (error) {
    console.error('Error fetching pawns:', error)
    showError('Error al cargar los empeños')
  } finally {
    isLoading.value = false
  }
}

const fetchPaymentCards = async () => {
  try {
    const institucionId = authStore.institucionID
    
    if (!institucionId) return

    let response
    if (USE_V1_API) {
      response = await PawnService.getPaymentCards(institucionId)
    } else {
      response = await PawnService.legacyGetPaymentCards(institucionId)
    }

    if (response && response.data) {
      paymentCards.value = response.data
    }
  } catch (error) {
    console.error('Error fetching payment cards:', error)
    showError('Error al cargar las tarjetas de pago')
  }
}

const handlePaymentCompleted = async () => {
  showSuccess('Pago procesado exitosamente')
  closePaymentModal()
  await fetchPawns()
}
</script>

<style scoped>
.glass-stats-card {
  @apply bg-neutral-900/90 backdrop-blur-xl rounded-xl border border-white/20 p-6 shadow-lg hover:shadow-xl transition-all duration-300;
  background: linear-gradient(135deg, rgba(255, 255, 255, 0.1), rgba(255, 255, 255, 0.05));
  backdrop-filter: blur(20px);
}

.glass-pawn-card {
  @apply bg-neutral-900/90 backdrop-blur-xl rounded-xl border border-white/20 shadow-lg hover:shadow-xl transition-all duration-300 hover:scale-[1.02];
  background: linear-gradient(135deg, rgba(255, 255, 255, 0.1), rgba(255, 255, 255, 0.05));
  backdrop-filter: blur(20px);
}

.glass-pawn-card:hover {
  border-color: rgba(244, 63, 184, 0.3);
  box-shadow: 0 8px 32px rgba(244, 63, 184, 0.15);
}
</style>