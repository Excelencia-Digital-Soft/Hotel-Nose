<template>
  <div
    class="min-h-screen bg-gradient-to-br from-neutral-900 via-neutral-800 to-neutral-900 p-4 lg:p-6"
  >
    <!-- Header Section -->
    <div class="glass-container mb-6 p-6 transform hover:scale-[1.02] transition-all duration-300">
      <div
        class="flex flex-col lg:flex-row lg:items-center lg:justify-between space-y-4 lg:space-y-0"
      >
        <div class="text-center lg:text-left">
          <div class="flex items-center justify-center lg:justify-start mb-2">
            <div class="bg-gradient-to-r from-primary-400 to-accent-400 p-3 rounded-full mr-3">
              <i class="pi pi-calculator text-white text-2xl"></i>
            </div>
            <h1 class="text-3xl lg:text-4xl font-bold text-white">🧮 Gestión de Cierres</h1>
          </div>
          <p class="text-gray-300 text-lg">
            Controla y revisa todos los cierres de caja de manera fácil 💰
          </p>
        </div>

        <!-- Quick Stats -->
        <div class="glass-card p-4">
          <div class="grid grid-cols-2 gap-4 text-center">
            <div class="transform hover:scale-110 transition-transform duration-200">
              <div
                class="bg-gradient-to-r from-primary-400 to-primary-500 text-white p-3 rounded-full mx-auto w-12 h-12 flex items-center justify-center mb-2"
              >
                <span class="font-bold text-lg">{{ totalCierres }}</span>
              </div>
              <p class="text-xs text-gray-300">Cierres</p>
            </div>
            <div class="transform hover:scale-110 transition-transform duration-200">
              <div
                class="bg-gradient-to-r from-secondary-400 to-secondary-500 text-white p-3 rounded-full mx-auto w-12 h-12 flex items-center justify-center mb-2"
              >
                <i class="pi pi-clock text-white text-lg"></i>
              </div>
              <p class="text-xs text-gray-300">Histórico</p>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- Current Session Section -->
    <div v-if="hasCurrentSession" class="glass-container mb-6 p-6">
      <div class="flex items-center mb-4">
        <i class="pi pi-clock text-green-400 text-xl mr-2"></i>
        <h3 class="text-xl font-bold text-white">🟢 Sesión Actual</h3>
      </div>

      <div
        @click="openCurrentSessionModal"
        class="glass-card p-6 cursor-pointer hover:bg-white/15 transition-all duration-300 group transform hover:scale-[1.02]"
      >
        <div class="flex items-center justify-between">
          <div class="flex items-center">
            <div
              class="bg-gradient-to-r from-green-400 to-emerald-500 p-4 rounded-full mr-4 group-hover:scale-110 transition-transform"
            >
              <i class="pi pi-play text-white text-2xl"></i>
            </div>
            <div>
              <h4 class="text-xl font-bold text-white mb-1">💸 Cierre Actual</h4>
              <p class="text-gray-300">Sesión activa - Haz clic para ver detalles</p>
            </div>
          </div>
          <div class="flex items-center space-x-2">
            <div class="bg-green-500/20 px-3 py-1 rounded-full border border-green-500/30">
              <span class="text-green-400 text-sm font-semibold flex items-center">
                <i class="pi pi-circle-fill mr-1 animate-pulse"></i>
                Activo
              </span>
            </div>
            <i
              class="pi pi-chevron-right text-gray-400 group-hover:text-white transition-colors"
            ></i>
          </div>
        </div>
      </div>
    </div>

    <!-- Historical Closures Section -->
    <div v-if="canViewHistorical" class="glass-container p-6">
      <div class="flex flex-col lg:flex-row lg:items-center lg:justify-between space-y-4 lg:space-y-0 mb-6">
        <div class="flex items-center">
          <i class="pi pi-history text-primary-400 text-xl mr-2"></i>
          <h3 class="text-xl font-bold text-white">
            {{
              hasHistoricalClosures
                ? `📚 ${totalCierres} cierres`
                : '📚 Gestión de Cierres'
            }}
          </h3>
        </div>

        <!-- View Mode Selector -->
        <div class="flex items-center space-x-2">
          <button
            @click="switchToCurrentView"
            :class="[
              'px-4 py-2 transition-all text-sm font-medium rounded-lg',
              viewMode === 'current'
                ? 'bg-gradient-to-r from-primary-400 to-secondary-500 text-white'
                : 'glass-button text-gray-300 hover:bg-white/20'
            ]"
          >
            <i class="pi pi-clock mr-2"></i>
            Actual
          </button>
          <button
            @click="switchToHistoricalView"
            :class="[
              'px-4 py-2 transition-all text-sm font-medium rounded-lg',
              viewMode === 'historical'
                ? 'bg-gradient-to-r from-primary-400 to-secondary-500 text-white'
                : 'glass-button text-gray-300 hover:bg-white/20'
            ]"
          >
            <i class="pi pi-history mr-2"></i>
            Histórico
          </button>
        </div>

        <button
          @click="refreshCierres"
          :disabled="isLoading"
          class="glass-button px-4 py-2 text-white hover:bg-white/20 transform hover:scale-105 transition-all disabled:opacity-50"
        >
          <i :class="isLoading ? 'pi pi-spinner pi-spin' : 'pi pi-refresh'" class="mr-2"></i>
          Actualizar
        </button>
      </div>

      <!-- Loading State -->
      <LoadingState v-if="isLoading" />

      <!-- Closures Grid -->
      <div v-else-if="hasHistoricalClosures" class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
        <CierreCard
          v-for="cierre in cierres"
          :key="cierre.cierreId"
          :cierre="cierre"
          @click="openCierreModal(cierre)"
        />
      </div>

      <!-- Empty State -->
      <EmptyState v-else @refresh="refreshCierres" />

      <!-- Pagination Controls -->
      <div v-if="hasHistoricalClosures && !isLoading" class="mt-6">
        <PaginationControls
          :current-page="currentPage"
          :total-pages="totalPages"
          :total-records="totalRecords"
          :page-size="pageSize"
          :can-go-next="canGoNext"
          :can-go-previous="canGoPrevious"
          :is-loading="isLoading"
          @go-to-page="(page) => goToPage(page, viewMode)"
          @next="() => nextPage(viewMode)"
          @previous="() => previousPage(viewMode)"
          @set-page-size="(size) => setPageSize(size, viewMode)"
        />
      </div>
    </div>

    <!-- Access Denied for Regular Users -->
    <AccessDenied v-else />

    <!-- Modals -->
    <ModalCierre
      v-if="showPagosModal"
      :selectedPagos="selectedPagos"
      :idcierre="selectedIdCierre"
      :selectedEgresos="selectedEgresos"
      :esAbierto="false"
      @imprimir-modal="handlePrint"
      @close-modal="closePagosModal"
    />

    <ModalCierre
      v-if="showCurrentSessionModal"
      :selectedPagos="transaccionesPendientes"
      :selectedEgresos="egresosPendientes"
      :esAbierto="true"
      @imprimir-modal="handlePrint"
      @close-modal="closeCurrentSessionModal"
    />

    <!-- Toast for notifications -->
    <Toast />
    <ConfirmDialog />
  </div>
</template>

<script setup>
import { ref, onBeforeMount } from 'vue';
import { useCierres } from '../composables/useCierres';
import ModalCierre from '../components/ModalCierre.vue';
import Toast from 'primevue/toast';
import ConfirmDialog from 'primevue/confirmdialog';

// Import child components
import LoadingState from '../components/cierres/LoadingState.vue';
import CierreCard from '../components/cierres/CierreCard.vue';
import EmptyState from '../components/cierres/EmptyState.vue';
import AccessDenied from '../components/cierres/AccessDenied.vue';
import PaginationControls from '../components/cierres/PaginationControls.vue';

// Composable
const {
  // State
  cierres,
  transaccionesPendientes,
  egresosPendientes,
  isLoading,
  selectedCierre,

  // Pagination State
  currentPage,
  pageSize,
  totalRecords,
  totalPages,

  // Computed
  hasHistoricalClosures,
  hasCurrentSession,
  totalCierres,
  canViewHistorical,
  canGoNext,
  canGoPrevious,

  // Methods
  fetchCierres,
  fetchCierresHistoricos,
  fetchDetalleCierre,
  selectCierre,
  openCurrentSession,
  handlePrint,
  initialize,

  // Pagination Methods
  goToPage,
  nextPage,
  previousPage,
  setPageSize,

  // Utilities
  formatFechaHora,
  formatFecha,
  formatHora,
} = useCierres();

// Local state for modals
const showPagosModal = ref(false);
const showCurrentSessionModal = ref(false);
const selectedPagos = ref([]);
const selectedEgresos = ref([]);
const selectedIdCierre = ref(null);

// Local state for view mode
const viewMode = ref('current'); // 'current' or 'historical'

// Methods
const refreshCierres = async () => {
  if (viewMode.value === 'current') {
    await fetchCierres(currentPage.value);
  } else {
    await fetchCierresHistoricos(currentPage.value);
  }
};

const switchToCurrentView = async () => {
  viewMode.value = 'current';
  await fetchCierres(1);
};

const switchToHistoricalView = async () => {
  viewMode.value = 'historical';
  await fetchCierresHistoricos(1);
};

const openCierreModal = async (cierre) => {
  selectCierre(cierre);
  selectedIdCierre.value = cierre.cierreId;
  
  // Fetch detailed data for the specific cierre
  const detalle = await fetchDetalleCierre(cierre.cierreId);
  if (detalle) {
    selectedPagos.value = [...(detalle.pagos || []), ...(detalle.anulaciones || [])];
    selectedEgresos.value = detalle.egresos || [];
    showPagosModal.value = true;
  }
};

const openCurrentSessionModal = () => {
  openCurrentSession();
  showCurrentSessionModal.value = true;
};

const closePagosModal = () => {
  showPagosModal.value = false;
  selectedPagos.value = [];
  selectedEgresos.value = [];
  selectedIdCierre.value = null;
};

const closeCurrentSessionModal = () => {
  showCurrentSessionModal.value = false;
};

// Lifecycle
onBeforeMount(async () => {
  await initialize();
});
</script>

<style scoped>
/* Glass effect styles */
.glass-container {
  background-color: rgba(255, 255, 255, 0.05);
  backdrop-filter: blur(32px);
  border: 1px solid rgba(255, 255, 255, 0.2);
  border-radius: 24px;
}

.glass-card {
  background-color: rgba(255, 255, 255, 0.1);
  backdrop-filter: blur(16px);
  border: 1px solid rgba(255, 255, 255, 0.2);
  border-radius: 12px;
}

.glass-button {
  background-color: rgba(255, 255, 255, 0.1);
  backdrop-filter: blur(8px);
  border: 1px solid rgba(255, 255, 255, 0.3);
  border-radius: 8px;
  transition: all 0.3s ease;
}

.glass-button:hover {
  background-color: rgba(255, 255, 255, 0.2);
}

.glass-input {
  background-color: rgba(255, 255, 255, 0.1);
  backdrop-filter: blur(8px);
  border: 1px solid rgba(255, 255, 255, 0.3);
  border-radius: 8px;
  color: white;
}

.glass-input::placeholder {
  color: rgba(255, 255, 255, 0.6);
}

.glass-input:focus {
  outline: none;
  border-color: rgba(139, 92, 246, 0.5);
  box-shadow: 0 0 0 2px rgba(139, 92, 246, 0.2);
}

/* Custom animations */
@keyframes shake {
  0%,
  100% {
    transform: translateX(0);
  }
  25% {
    transform: translateX(-5px);
  }
  75% {
    transform: translateX(5px);
  }
}

.shake {
  animation: shake 0.5s ease-in-out;
}

/* Hover animations */
.animate-pulse {
  animation: pulse 2s cubic-bezier(0.4, 0, 0.6, 1) infinite;
}

.animate-bounce {
  animation: bounce 1s infinite;
}

/* Custom scrollbar */
::-webkit-scrollbar {
  width: 8px;
}

::-webkit-scrollbar-track {
  background: rgba(255, 255, 255, 0.1);
  border-radius: 4px;
}

::-webkit-scrollbar-thumb {
  background: rgba(255, 255, 255, 0.3);
  border-radius: 4px;
}

::-webkit-scrollbar-thumb:hover {
  background: rgba(255, 255, 255, 0.5);
}
</style>