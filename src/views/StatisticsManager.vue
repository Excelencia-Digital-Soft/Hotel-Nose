<template>
  <div class="min-h-screen bg-gradient-to-br from-neutral-900 via-neutral-800 to-neutral-900 p-4 lg:p-6">
    <!-- Header Section -->
    <div class="glass-container mb-6 p-6 transform hover:scale-[1.02] transition-all duration-300">
      <div class="flex flex-col lg:flex-row lg:items-center lg:justify-between space-y-4 lg:space-y-0">
        <div class="text-center lg:text-left">
          <div class="flex items-center justify-center lg:justify-start mb-2">
            <div class="bg-gradient-to-r from-primary-400 to-accent-400 p-3 rounded-full mr-3">
              <i class="pi pi-chart-line text-white text-2xl"></i>
            </div>
            <h1 class="text-3xl lg:text-4xl font-bold text-white">📊 Análisis de Estadísticas</h1>
          </div>
          <p class="text-gray-300 text-lg">Dashboard completo de rendimiento hotelero 📈</p>
        </div>
        
        <!-- Date Range Summary -->
        <div class="glass-card p-4">
          <div class="text-center">
            <div class="bg-gradient-to-r from-secondary-400 to-secondary-500 text-white p-3 rounded-full mx-auto w-12 h-12 flex items-center justify-center mb-2">
              <i class="pi pi-calendar text-lg"></i>
            </div>
            <p class="text-white font-bold text-sm">{{ currentDateRangeDisplay || 'Seleccionar período' }}</p>
            <p class="text-gray-300 text-xs">Período de análisis</p>
          </div>
        </div>
      </div>
    </div>

    <!-- Date Range Selector -->
    <div class="glass-container mb-6 p-6">
      <div class="flex items-center mb-4">
        <i class="pi pi-filter text-accent-400 text-xl mr-2"></i>
        <h3 class="text-xl font-bold text-white">📅 Selección de Período</h3>
      </div>
      
      <div class="grid grid-cols-1 lg:grid-cols-3 gap-6">
        <!-- Predefined Ranges -->
        <div class="lg:col-span-2">
          <label class="block text-white font-semibold mb-3">
            <i class="pi pi-clock text-primary-400 mr-2"></i>
            Períodos predefinidos
          </label>
          <div class="grid grid-cols-2 md:grid-cols-4 gap-3">
            <button
              v-for="(range, key) in predefinedRanges"
              :key="key"
              @click="setPredefinedRange(key)"
              class="glass-button py-3 px-4 text-white hover:bg-white/20 transform hover:scale-105 transition-all"
              :class="{ 'bg-primary-500/50 border-primary-400': selectedPredefinedRange === key }"
            >
              <div class="text-center">
                <p class="font-semibold text-sm">{{ range.label }}</p>
              </div>
            </button>
          </div>
        </div>

        <!-- Custom Date Range -->
        <div>
          <label class="block text-white font-semibold mb-3">
            <i class="pi pi-calendar text-accent-400 mr-2"></i>
            Período personalizado
          </label>
          <div class="space-y-3">
            <div>
              <label class="block text-gray-300 text-sm mb-1">Fecha inicio</label>
              <input
                v-model="customStartDate"
                type="date"
                class="glass-input w-full px-3 py-2 text-sm"
                @change="updateCustomRange"
              />
            </div>
            <div>
              <label class="block text-gray-300 text-sm mb-1">Fecha fin</label>
              <input
                v-model="customEndDate"
                type="date"
                class="glass-input w-full px-3 py-2 text-sm"
                @change="updateCustomRange"
              />
            </div>
          </div>
        </div>
      </div>

      <!-- Validation Messages -->
      <div v-if="dateRangeErrors.length > 0" class="mt-4 p-3 bg-red-500/20 rounded-lg border border-red-500/30">
        <div class="flex items-start">
          <i class="pi pi-exclamation-triangle text-red-400 mr-2 mt-0.5"></i>
          <div>
            <p class="text-red-300 font-semibold text-sm">Errores en el rango de fechas:</p>
            <ul class="text-red-300 text-sm mt-1 space-y-1">
              <li v-for="error in dateRangeErrors" :key="error">• {{ error }}</li>
            </ul>
          </div>
        </div>
      </div>

      <!-- Action Buttons -->
      <div class="flex items-center justify-between mt-6">
        <div class="flex items-center space-x-3">
          <button
            @click="refreshAllData"
            :disabled="isRefreshing || !isValidDateRange"
            class="bg-gradient-to-r from-primary-400 via-secondary-400 to-accent-400 
                   hover:from-primary-500 hover:via-secondary-500 hover:to-accent-500 
                   disabled:opacity-50 disabled:cursor-not-allowed
                   text-white font-bold py-3 px-6 rounded-lg 
                   transition-all duration-300 transform hover:scale-105"
          >
            <i :class="isRefreshing ? 'pi pi-spinner pi-spin' : 'pi pi-refresh'" class="mr-2"></i>
            {{ isRefreshing ? '🔄 Actualizando...' : '🚀 Cargar Estadísticas' }}
          </button>
          
          <button
            @click="clearData"
            class="glass-button py-3 px-4 text-white hover:text-red-300 transform hover:scale-105 transition-all"
          >
            <i class="pi pi-trash mr-2"></i>
            🗑️ Limpiar
          </button>
        </div>

        <!-- Loading Indicator -->
        <div v-if="isLoadingAny" class="flex items-center space-x-2">
          <div class="bg-blue-500/20 px-3 py-1 rounded-full border border-blue-500/30">
            <span class="text-blue-300 text-sm font-semibold flex items-center">
              <i class="pi pi-spinner pi-spin mr-2"></i>
              Cargando datos...
            </span>
          </div>
        </div>
      </div>
    </div>

    <!-- Dashboard Summary -->
    <div v-if="dashboardSummary" class="glass-container mb-6 p-6">
      <div class="flex items-center mb-4">
        <i class="pi pi-chart-bar text-green-400 text-xl mr-2"></i>
        <h3 class="text-xl font-bold text-white">📈 Resumen Ejecutivo</h3>
      </div>
      
      <div class="grid grid-cols-2 md:grid-cols-5 gap-4">
        <div class="glass-card p-4 text-center transform hover:scale-105 transition-all duration-200">
          <div class="bg-gradient-to-r from-blue-400 to-blue-500 text-white p-3 rounded-full mx-auto w-12 h-12 flex items-center justify-center mb-2">
            <i class="pi pi-home text-lg"></i>
          </div>
          <p class="text-blue-400 font-bold text-lg">{{ dashboardSummary.totalRooms || 0 }}</p>
          <p class="text-gray-300 text-xs">Habitaciones</p>
        </div>
        
        <div class="glass-card p-4 text-center transform hover:scale-105 transition-all duration-200">
          <div class="bg-gradient-to-r from-green-400 to-green-500 text-white p-3 rounded-full mx-auto w-12 h-12 flex items-center justify-center mb-2">
            <i class="pi pi-dollar text-lg"></i>
          </div>
          <p class="text-green-400 font-bold text-lg">{{ formatCurrency(dashboardSummary.totalRevenue || 0) }}</p>
          <p class="text-gray-300 text-xs">Ingresos Totales</p>
        </div>
        
        <div class="glass-card p-4 text-center transform hover:scale-105 transition-all duration-200">
          <div class="bg-gradient-to-r from-purple-200 to-purple-500 text-white p-3 rounded-full mx-auto w-12 h-12 flex items-center justify-center mb-2">
            <i class="pi pi-calendar text-lg"></i>
          </div>
          <p class="text-purple-200 font-bold text-lg">{{ dashboardSummary.totalReservations || 0 }}</p>
          <p class="text-gray-300 text-xs">Reservas</p>
        </div>
        
        <div class="glass-card p-4 text-center transform hover:scale-105 transition-all duration-200">
          <div class="bg-gradient-to-r from-yellow-400 to-yellow-500 text-white p-3 rounded-full mx-auto w-12 h-12 flex items-center justify-center mb-2">
            <i class="pi pi-percentage text-lg"></i>
          </div>
          <p class="text-yellow-400 font-bold text-lg">{{ formatPercentage(dashboardSummary.averageOccupancy || 0) }}</p>
          <p class="text-gray-300 text-xs">Ocupación Prom.</p>
        </div>
        
        <div class="glass-card p-4 text-center transform hover:scale-105 transition-all duration-200">
          <div class="bg-gradient-to-r from-pink-400 to-pink-500 text-white p-3 rounded-full mx-auto w-12 h-12 flex items-center justify-center mb-2">
            <i class="pi pi-shopping-cart text-lg"></i>
          </div>
          <p class="text-pink-400 font-bold text-lg">{{ formatCurrency(dashboardSummary.totalConsumption || 0) }}</p>
          <p class="text-gray-300 text-xs">Consumos</p>
        </div>
      </div>
    </div>

    <!-- Charts Grid -->
    <div class="space-y-6">
      <!-- Row 1: Room Ranking & Revenue -->
      <div class="grid grid-cols-1 xl:grid-cols-2 gap-6">
        <RoomRankingChart
          :data="roomRanking"
          :isLoading="isLoadingRoomRanking"
          :error="errors.roomRanking"
          @refresh="fetchRoomRanking(true)"
        />
        
        <RoomRevenueChart
          :data="roomRevenue"
          :isLoading="isLoadingRoomRevenue"
          :error="errors.roomRevenue"
          @refresh="fetchRoomRevenue(true)"
        />
      </div>

      <!-- Row 2: Category Occupancy & Room Consumption -->
      <div class="grid grid-cols-1 xl:grid-cols-2 gap-6">
        <CategoryOccupancyChart
          :data="categoryOccupancy"
          :isLoading="isLoadingCategoryOccupancy"
          :error="errors.categoryOccupancy"
          @refresh="fetchCategoryOccupancy(true)"
        />
        
        <RoomConsumptionChart
          :data="roomConsumption"
          :isLoading="isLoadingRoomConsumption"
          :error="errors.roomConsumption"
          @refresh="fetchRoomConsumption(true)"
        />
      </div>
    </div>

    <!-- Empty State -->
    <div v-if="!hasAnyData && !isLoadingAny" class="text-center py-16">
      <div class="glass-card max-w-md mx-auto p-8">
        <div class="mb-6">
          <div class="bg-gradient-to-r from-primary-400 to-accent-400 p-6 rounded-full mx-auto w-24 h-24 flex items-center justify-center mb-4">
            <i class="pi pi-chart-line text-white text-4xl"></i>
          </div>
          <h3 class="text-2xl text-white font-bold mb-2">📊 ¡Comienza tu análisis!</h3>
          <p class="text-gray-300 mb-6">Selecciona un período y carga las estadísticas para ver insights detallados</p>
        </div>
        
        <button
          @click="fetchAllStatistics()"
          :disabled="!isValidDateRange"
          class="bg-gradient-to-r from-primary-400 to-accent-400 hover:from-primary-500 hover:to-accent-500 
                 disabled:opacity-50 disabled:cursor-not-allowed
                 text-white font-bold py-3 px-6 rounded-lg transform hover:scale-105 transition-all duration-300"
        >
          <i class="pi pi-chart-line mr-2"></i>
          🚀 ¡Cargar Estadísticas!
        </button>
      </div>
    </div>

    <!-- Toast for notifications -->
    <Toast />
    <ConfirmDialog />
  </div>
</template>

<script setup>
import { ref, computed, onMounted, watch } from 'vue'
import { useStatistics } from '../composables/useStatistics'
import RoomRankingChart from '../components/charts/RoomRankingChart.vue'
import RoomRevenueChart from '../components/charts/RoomRevenueChart.vue'
import CategoryOccupancyChart from '../components/charts/CategoryOccupancyChart.vue'
import RoomConsumptionChart from '../components/charts/RoomConsumptionChart.vue'
import Toast from 'primevue/toast'
import ConfirmDialog from 'primevue/confirmdialog'

// Composables
const {
  // Reactive state
  selectedDateRange,
  selectedPredefinedRange,
  isRefreshing,

  // Computed properties
  predefinedRanges,
  currentDateRangeDisplay,
  isValidDateRange,
  dateRangeErrors,

  // Store data
  roomRanking,
  roomRevenue,
  categoryOccupancy,
  roomConsumption,

  // Loading states
  isLoadingAny,
  isLoadingRoomRanking,
  isLoadingRoomRevenue,
  isLoadingCategoryOccupancy,
  isLoadingRoomConsumption,

  // Computed statistics
  dashboardSummary,

  // Errors
  errors,

  // Methods
  setPredefinedRange,
  setCustomDateRange,
  fetchRoomRanking,
  fetchRoomRevenue,
  fetchCategoryOccupancy,
  fetchRoomConsumption,
  fetchAllStatistics,
  refreshAllData,
  clearData,

  // Formatting helpers
  formatCurrency,
  formatPercentage,
  formatNumber
} = useStatistics()

// Local reactive state
const customStartDate = ref('')
const customEndDate = ref('')

// Computed properties
const hasAnyData = computed(() => {
  return (roomRanking.value?.length || 0) > 0 ||
         (roomRevenue.value?.length || 0) > 0 ||
         (categoryOccupancy.value?.length || 0) > 0 ||
         (roomConsumption.value?.length || 0) > 0
})

// Methods
const updateCustomRange = () => {
  if (customStartDate.value && customEndDate.value) {
    setCustomDateRange(customStartDate.value, customEndDate.value)
  }
}

// Initialize custom date inputs when date range changes
watch(selectedDateRange, (newRange) => {
  if (newRange && selectedPredefinedRange.value === 'custom') {
    customStartDate.value = newRange.fechaInicio
    customEndDate.value = newRange.fechaFin
  }
}, { deep: true })

// Debug watcher to see data changes
watch(roomRanking, (newData) => {
  console.log('📊 [StatisticsManager] roomRanking changed:', {
    data: newData,
    length: newData?.length,
    sample: newData?.slice(0, 3)
  })
}, { deep: true })

watch(roomRevenue, (newData) => {
  console.log('💰 [StatisticsManager] roomRevenue changed:', {
    data: newData,
    length: newData?.length,
    sample: newData?.slice(0, 3)
  })
}, { deep: true })

// Initialize component
onMounted(() => {
  // Ensure the store has default values
  if (!selectedDateRange.value) {
    setPredefinedRange('lastMonth')
  }
  
  console.log('📊 [StatisticsManager] Component mounted, initial data:', {
    roomRanking: roomRanking.value?.length || 0,
    roomRevenue: roomRevenue.value?.length || 0,
    categoryOccupancy: categoryOccupancy.value?.length || 0,
    roomConsumption: roomConsumption.value?.length || 0
  })
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

/* Custom animations */
@keyframes shake {
  0%, 100% { transform: translateX(0); }
  25% { transform: translateX(-5px); }
  75% { transform: translateX(5px); }
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
