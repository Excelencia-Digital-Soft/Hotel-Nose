<template>
  <div class="min-h-screen bg-gradient-to-br from-slate-900 via-purple-900 to-slate-900 p-6">
    <!-- Glass Container -->
    <div class="max-w-7xl mx-auto">
      <!-- Header -->
      <div class="glass-card mb-8">
        <div class="flex items-center justify-between">
          <div>
            <h1 class="text-3xl font-bold text-white lexend-exa mb-2">Gestión de Empeños</h1>
            <p class="text-white/70">Administrar empeños y procesar pagos</p>
          </div>
          <div class="flex items-center space-x-4">
            <div class="text-right">
              <div class="text-2xl font-bold text-white">{{ statistics.total }}</div>
              <div class="text-white/70 text-sm">
                {{ viewMode === 'active' ? 'Empeños Activos' : 'Total Empeños' }}
              </div>
            </div>
            <div class="text-right">
              <div class="text-2xl font-bold text-white">{{ statistics.totalAmount }}</div>
              <div class="text-white/70 text-sm">Valor Total</div>
            </div>
          </div>
        </div>
      </div>

      <!-- View Mode Toggle -->
      <div class="glass-card mb-6">
        <div class="flex items-center justify-between">
          <div class="flex items-center space-x-4">
            <span class="text-white font-medium">Vista:</span>
            <div class="flex bg-white/5 rounded-xl p-1">
              <button
                @click="changeFilter('active')"
                :class="[
                  'px-4 py-2 rounded-lg transition-all duration-300 text-sm font-medium',
                  viewMode === 'active'
                    ? 'bg-primary-500 text-white shadow-md'
                    : 'text-white/70 hover:text-white hover:bg-white/10',
                ]"
              >
                <i class="pi pi-clock mr-2"></i>
                Activos
              </button>
              <button
                @click="changeFilter('all')"
                :class="[
                  'px-4 py-2 rounded-lg transition-all duration-300 text-sm font-medium',
                  viewMode === 'all'
                    ? 'bg-secondary-500 text-white shadow-md'
                    : 'text-white/70 hover:text-white hover:bg-white/10',
                ]"
              >
                <i class="pi pi-history mr-2"></i>
                Historial
              </button>
            </div>
          </div>
          <div class="text-right">
            <div class="text-sm text-white/70">
              {{
                viewMode === 'active'
                  ? 'Empeños pendientes de pago'
                  : 'Todos los empeños (incluye pagados)'
              }}
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
              <i
                class="pi pi-search absolute left-3 top-1/2 transform -translate-y-1/2 text-white/70"
              ></i>
              <input
                v-model="searchTerm"
                type="text"
                placeholder="ID, detalle, monto..."
                class="glass-input w-full pl-10"
              />
            </div>
          </div>

          <!-- Status Filter -->
          <div v-if="viewMode === 'all'" class="space-y-2">
            <label class="block text-white font-medium text-sm">Estado</label>
            <select v-model="statusFilter" class="glass-input w-full">
              <option value="all">Todos los Estados</option>
              <option value="paid">Pagados</option>
              <option value="unpaid">Sin Pagar</option>
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

        <!-- Overdue / Paid Stats -->
        <div class="glass-stats-card">
          <div class="flex items-center justify-between">
            <div>
              <p class="text-white/70 text-sm">
                {{ viewMode === 'active' ? 'Vencidos' : 'Pagados' }}
              </p>
              <p
                :class="[
                  'text-2xl font-bold',
                  viewMode === 'active' ? 'text-red-400' : 'text-green-400',
                ]"
              >
                {{ statistics.overdue }}
              </p>
            </div>
            <div
              :class="[
                'w-12 h-12 rounded-xl flex items-center justify-center',
                viewMode === 'active' ? 'bg-red-500/20' : 'bg-green-500/20',
              ]"
            >
              <i
                :class="[
                  'text-xl',
                  viewMode === 'active'
                    ? 'pi pi-exclamation-triangle text-red-400'
                    : 'pi pi-check-circle text-green-400',
                ]"
              ></i>
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
            {{ viewMode === 'active' ? 'Empeños Activos' : 'Historial de Empeños' }} ({{
              filteredPawns.length
            }})
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
          <div v-for="pawn in filteredPawns" :key="pawn.empeñoId" class="glass-pawn-card group">
            <div class="p-6">
              <div class="flex items-start justify-between">
                <!-- Pawn Info -->
                <div class="flex-1 min-w-0">
                  <div class="flex items-center space-x-3 mb-3">
                    <span class="glass-badge text-sm"> #{{ pawn.empeñoId }} </span>

                    <!-- Payment Status Badge (only in 'all' view) -->
                    <span
                      v-if="viewMode === 'all'"
                      :class="[
                        'px-2 py-1 rounded-lg text-xs font-medium border',
                        pawn.estado === 'pagado'
                          ? 'bg-green-500/20 text-green-400 border-green-400/30'
                          : 'bg-red-500/20 text-red-400 border-red-400/30',
                      ]"
                    >
                      <i
                        :class="
                          pawn.estadoPago === 'Pagado'
                            ? 'pi pi-check-circle mr-1'
                            : 'pi pi-clock mr-1'
                        "
                      ></i>
                      {{ pawn.estadoPago }}
                    </span>

                    <!-- Time Status Badge (always shown) -->
                    <span :class="['text-sm font-medium', getStatusColor(pawn.status)]">
                      <i :class="['pi mr-1', getStatusIcon(pawn.status)]"></i>
                      {{ getStatusText(pawn.status) }}
                    </span>

                    <span class="text-white/60 text-sm"> {{ pawn.daysOverdue }} días </span>
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
                      <span class="text-white font-semibold ml-2">#{{ pawn.visitaId }}</span>
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
                    v-if="pawn.estadoPago === 'Pendiente'"
                    @click="openPaymentModal(pawn)"
                    class="glass-button-primary py-3 px-6 rounded-xl font-medium transition-all duration-300 hover:scale-105"
                  >
                    <i class="pi pi-credit-card mr-2"></i>
                    Pagar
                  </button>
                  <div
                    v-else
                    class="px-6 py-3 rounded-xl bg-green-500/20 border border-green-400/30 text-green-400 font-medium"
                  >
                    <i class="pi pi-check-circle mr-2"></i>
                    Pagado
                  </div>
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

    <!-- Confirmation Dialog -->
    <ConfirmDialog />

    <!-- Toast Messages -->
    <Toast />
  </div>
</template>

<script setup lang="ts">
  import { ref, onMounted } from 'vue'
  import { usePawnManager } from '../composables/usePawnManager'
  import { PawnService } from '../services/pawnService'
  import { useAuthStore } from '../store/auth.js'
  import PawnPaymentModal from '../components/PawnPaymentModal.vue'
  import ConfirmDialog from 'primevue/confirmdialog'
  import Toast from 'primevue/toast'
  import type { PawnDto, PawnStatus } from '../types'

  // Auth store
  const authStore = useAuthStore()

  // Composable
  const {
    pawns,
    paymentCards,
    isLoading,
    showPaymentModal,
    selectedPawn,
    viewMode,
    filteredPawns,
    statistics,
    searchTerm,
    statusFilter,
    sortBy,
    openPaymentModal,
    closePaymentModal,
    fetchPaymentCards,
    fetchPawns,
    toggleViewMode,
    getStatusColor,
    getStatusIcon,
    showSuccess,
    showError,
  } = usePawnManager()

  // Utility functions
  const getStatusText = (status: PawnStatus): string => {
    switch (status) {
      case 'overdue':
        return 'Vencido'
      case 'warning':
        return 'En Alerta'
      case 'active':
        return 'Activo'
      default:
        return 'Desconocido'
    }
  }

  // Lifecycle
  onMounted(() => {
    Promise.all([fetchPawns('active'), fetchPaymentCards()])
  })

  const changeFilter = async (typeFilter: 'active' | 'all') => {
    viewMode.value = typeFilter
    await fetchPawns(typeFilter)
  }

  const handlePaymentCompleted = async (): Promise<void> => {
    showSuccess('Pago procesado exitosamente')
    closePaymentModal()
    await fetchPawns(viewMode.value)
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
