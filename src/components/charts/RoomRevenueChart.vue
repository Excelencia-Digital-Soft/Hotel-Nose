<template>
  <div class="glass-card p-6">
    <!-- Chart Header -->
    <div class="flex items-center justify-between mb-6">
      <div class="flex items-center">
        <div class="bg-gradient-to-r from-secondary-400 to-accent-400 p-3 rounded-full mr-3">
          <i class="pi pi-dollar text-white text-xl"></i>
        </div>
        <div>
          <h3 class="text-xl font-bold text-white">💰 Ingresos por Habitación</h3>
          <p class="text-gray-300 text-sm">Desglose de reservas vs consumos</p>
        </div>
      </div>
      
      <div class="flex items-center space-x-2">
        <select
          v-model="viewMode"
          class="glass-input px-3 py-2 text-sm"
        >
          <option value="total">Ingresos Totales</option>
          <option value="breakdown">Desglose Detallado</option>
          <option value="comparison">Comparación</option>
        </select>
        
        <button
          @click="refreshData"
          :disabled="isLoading"
          class="glass-button px-3 py-2 text-white hover:bg-white/20 transition-all disabled:opacity-50"
        >
          <i :class="isLoading ? 'pi pi-spinner pi-spin' : 'pi pi-refresh'"></i>
        </button>
      </div>
    </div>

    <!-- Loading State -->
    <div v-if="isLoading" class="flex items-center justify-center py-16">
      <div class="text-center">
        <div class="bg-gradient-to-r from-secondary-400 to-accent-400 p-4 rounded-full mx-auto w-16 h-16 flex items-center justify-center mb-4">
          <i class="pi pi-spinner pi-spin text-white text-2xl"></i>
        </div>
        <p class="text-white font-semibold">Cargando ingresos...</p>
        <p class="text-gray-300 text-sm">Calculando datos financieros</p>
      </div>
    </div>

    <!-- Error State -->
    <div v-else-if="error" class="text-center py-16">
      <div class="glass-card max-w-md mx-auto p-6">
        <div class="bg-red-500/20 p-4 rounded-full mx-auto w-16 h-16 flex items-center justify-center mb-4">
          <i class="pi pi-exclamation-triangle text-red-400 text-2xl"></i>
        </div>
        <h4 class="text-white font-bold mb-2">❌ Error al cargar datos</h4>
        <p class="text-gray-300 text-sm mb-4">{{ error }}</p>
        <button
          @click="refreshData"
          class="glass-button px-4 py-2 text-white hover:bg-white/20"
        >
          <i class="pi pi-refresh mr-2"></i>
          Reintentar
        </button>
      </div>
    </div>

    <!-- Chart Container -->
    <div v-else-if="hasData" class="relative">
      <!-- Chart Canvas -->
      <div class="glass-card p-4 mb-4">
        <canvas ref="chartCanvas" class="max-w-full"></canvas>
      </div>

      <!-- Revenue Summary -->
      <div class="grid grid-cols-1 md:grid-cols-4 gap-4 mb-4">
        <div class="glass-card p-4 text-center">
          <div class="bg-gradient-to-r from-green-400 to-green-500 text-white p-3 rounded-full mx-auto w-12 h-12 flex items-center justify-center mb-2">
            <i class="pi pi-dollar text-lg"></i>
          </div>
          <p class="text-green-400 font-bold text-lg">{{ formatCurrency(totalRevenue) }}</p>
          <p class="text-gray-300 text-sm">Ingresos Totales</p>
        </div>
        
        <div class="glass-card p-4 text-center">
          <div class="bg-gradient-to-r from-blue-400 to-blue-500 text-white p-3 rounded-full mx-auto w-12 h-12 flex items-center justify-center mb-2">
            <i class="pi pi-home text-lg"></i>
          </div>
          <p class="text-blue-400 font-bold text-lg">{{ formatCurrency(totalReservationRevenue) }}</p>
          <p class="text-gray-300 text-sm">Reservas</p>
        </div>
        
        <div class="glass-card p-4 text-center">
          <div class="bg-gradient-to-r from-purple-400 to-purple-500 text-white p-3 rounded-full mx-auto w-12 h-12 flex items-center justify-center mb-2">
            <i class="pi pi-shopping-cart text-lg"></i>
          </div>
          <p class="text-purple-400 font-bold text-lg">{{ formatCurrency(totalConsumptionRevenue) }}</p>
          <p class="text-gray-300 text-sm">Consumos</p>
        </div>
        
        <div class="glass-card p-4 text-center">
          <div class="bg-gradient-to-r from-yellow-400 to-yellow-500 text-white p-3 rounded-full mx-auto w-12 h-12 flex items-center justify-center mb-2">
            <i class="pi pi-percentage text-lg"></i>
          </div>
          <p class="text-yellow-400 font-bold text-lg">{{ consumptionPercentage }}%</p>
          <p class="text-gray-300 text-sm">% Consumos</p>
        </div>
      </div>

      <!-- Top Performers -->
      <div class="grid grid-cols-1 lg:grid-cols-2 gap-6">
        <!-- Top by Total Revenue -->
        <div class="glass-card p-4">
          <h4 class="text-white font-bold mb-3 flex items-center">
            <i class="pi pi-trophy text-yellow-400 mr-2"></i>
            🏆 Top Ingresos Totales
          </h4>
          <div class="space-y-2">
            <div
              v-for="(room, index) in topByRevenue"
              :key="room.habitacionID"
              class="flex items-center justify-between p-3 glass-button rounded-lg"
            >
              <div class="flex items-center">
                <span class="bg-gradient-to-r from-yellow-400 to-yellow-500 text-white w-6 h-6 rounded-full flex items-center justify-center text-xs font-bold mr-3">
                  {{ index + 1 }}
                </span>
                <div>
                  <p class="text-white font-semibold">{{ room.nombreHabitacion }}</p>
                  <p class="text-gray-300 text-xs">{{ room.nombreCategoria }}</p>
                </div>
              </div>
              <p class="text-green-400 font-bold">{{ formatCurrency(room.totalIngresos) }}</p>
            </div>
          </div>
        </div>

        <!-- Top by Consumption Revenue -->
        <div class="glass-card p-4">
          <h4 class="text-white font-bold mb-3 flex items-center">
            <i class="pi pi-shopping-cart text-purple-400 mr-2"></i>
            🛒 Top Consumos
          </h4>
          <div class="space-y-2">
            <div
              v-for="(room, index) in topByConsumption"
              :key="room.habitacionID"
              class="flex items-center justify-between p-3 glass-button rounded-lg"
            >
              <div class="flex items-center">
                <span class="bg-gradient-to-r from-purple-400 to-purple-500 text-white w-6 h-6 rounded-full flex items-center justify-center text-xs font-bold mr-3">
                  {{ index + 1 }}
                </span>
                <div>
                  <p class="text-white font-semibold">{{ room.nombreHabitacion }}</p>
                  <p class="text-gray-300 text-xs">{{ room.nombreCategoria }}</p>
                </div>
              </div>
              <p class="text-purple-400 font-bold">{{ formatCurrency(room.ingresosConsumos) }}</p>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- Empty State -->
    <div v-else class="text-center py-16">
      <div class="glass-card max-w-md mx-auto p-8">
        <div class="bg-gradient-to-r from-secondary-400 to-accent-400 p-6 rounded-full mx-auto w-24 h-24 flex items-center justify-center mb-4">
          <i class="pi pi-dollar text-white text-4xl"></i>
        </div>
        <h3 class="text-2xl text-white font-bold mb-2">💰 Sin datos de ingresos</h3>
        <p class="text-gray-300 mb-6">No hay datos de ingresos para el período seleccionado</p>
        <button
          @click="refreshData"
          class="bg-gradient-to-r from-secondary-400 to-accent-400 hover:from-secondary-500 hover:to-accent-500 
                 text-white font-bold py-3 px-6 rounded-lg transform hover:scale-105 transition-all duration-300"
        >
          <i class="pi pi-refresh mr-2"></i>
          🔄 Actualizar datos
        </button>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted, onUnmounted, watch, nextTick } from 'vue'
import { Chart, registerables } from 'chart.js'

// Register Chart.js components
Chart.register(...registerables)

// Props
const props = defineProps({
  data: {
    type: Array,
    default: () => []
  },
  isLoading: {
    type: Boolean,
    default: false
  },
  error: {
    type: String,
    default: null
  }
})

// Emits
const emit = defineEmits(['refresh'])

// Reactive state
const chartCanvas = ref(null)
const chart = ref(null)
const viewMode = ref('total')

// Computed properties
const hasData = computed(() => props.data && props.data.length > 0)

const totalRevenue = computed(() => {
  return props.data.reduce((total, room) => total + room.totalIngresos, 0)
})

const totalReservationRevenue = computed(() => {
  return props.data.reduce((total, room) => total + room.ingresosReservas, 0)
})

const totalConsumptionRevenue = computed(() => {
  return props.data.reduce((total, room) => total + room.ingresosConsumos, 0)
})

const consumptionPercentage = computed(() => {
  if (totalRevenue.value === 0) return 0
  return Math.round((totalConsumptionRevenue.value / totalRevenue.value) * 100)
})

const topByRevenue = computed(() => {
  return [...props.data]
    .sort((a, b) => b.totalIngresos - a.totalIngresos)
    .slice(0, 5)
})

const topByConsumption = computed(() => {
  return [...props.data]
    .sort((a, b) => b.ingresosConsumos - a.ingresosConsumos)
    .slice(0, 5)
})

const chartData = computed(() => {
  if (!hasData.value) return null

  // Sort data by total revenue (descending) and take top 10
  const sortedData = [...props.data]
    .sort((a, b) => b.totalIngresos - a.totalIngresos)
    .slice(0, 10)

  const labels = sortedData.map(room => room.nombreHabitacion)

  if (viewMode.value === 'breakdown') {
    return {
      labels,
      datasets: [
        {
          label: 'Ingresos por Reservas',
          data: sortedData.map(room => room.ingresosReservas),
          backgroundColor: '#60a5fa',
          borderColor: '#3b82f6',
          borderWidth: 2,
          borderRadius: 8,
          borderSkipped: false
        },
        {
          label: 'Ingresos por Consumos',
          data: sortedData.map(room => room.ingresosConsumos),
          backgroundColor: '#a78bfa',
          borderColor: '#8b5cf6',
          borderWidth: 2,
          borderRadius: 8,
          borderSkipped: false
        }
      ]
    }
  } else if (viewMode.value === 'comparison') {
    const reservationData = sortedData.map(room => room.ingresosReservas)
    const consumptionData = sortedData.map(room => room.ingresosConsumos)
    
    return {
      labels: ['Reservas', 'Consumos'],
      datasets: [{
        label: 'Distribución de Ingresos',
        data: [
          reservationData.reduce((a, b) => a + b, 0),
          consumptionData.reduce((a, b) => a + b, 0)
        ],
        backgroundColor: ['#60a5fa', '#a78bfa'],
        borderColor: ['#3b82f6', '#8b5cf6'],
        borderWidth: 2
      }]
    }
  } else {
    return {
      labels,
      datasets: [{
        label: 'Ingresos Totales',
        data: sortedData.map(room => room.totalIngresos),
        backgroundColor: 'rgba(52, 211, 153, 0.8)',
        borderColor: '#10b981',
        borderWidth: 2,
        borderRadius: 8,
        borderSkipped: false
      }]
    }
  }
})

const chartOptions = computed(() => {
  const baseOptions = {
    responsive: true,
    maintainAspectRatio: false,
    plugins: {
      legend: {
        display: viewMode.value !== 'total',
        position: 'bottom',
        labels: {
          color: '#e5e7eb',
          padding: 20,
          usePointStyle: true,
          font: {
            size: 12
          }
        }
      },
      tooltip: {
        backgroundColor: 'rgba(0, 0, 0, 0.8)',
        titleColor: '#ffffff',
        bodyColor: '#e5e7eb',
        borderColor: 'rgba(255, 255, 255, 0.2)',
        borderWidth: 1,
        cornerRadius: 8,
        displayColors: true,
        callbacks: {
          label: function(context) {
            const value = formatCurrency(context.raw)
            if (viewMode.value === 'comparison') {
              const total = context.dataset.data.reduce((a, b) => a + b, 0)
              const percentage = ((context.raw / total) * 100).toFixed(1)
              return `${context.dataset.label}: ${value} (${percentage}%)`
            }
            return `${context.dataset.label}: ${value}`
          }
        }
      }
    },
    scales: viewMode.value !== 'comparison' ? {
      x: {
        stacked: viewMode.value === 'breakdown',
        ticks: {
          color: '#9ca3af',
          maxRotation: 45,
          font: {
            size: 11
          }
        },
        grid: {
          color: 'rgba(255, 255, 255, 0.1)',
          drawBorder: false
        }
      },
      y: {
        stacked: viewMode.value === 'breakdown',
        beginAtZero: true,
        ticks: {
          color: '#9ca3af',
          font: {
            size: 11
          },
          callback: function(value) {
            return formatCurrencyShort(value)
          }
        },
        grid: {
          color: 'rgba(255, 255, 255, 0.1)',
          drawBorder: false
        }
      }
    } : {},
    animation: {
      duration: 1000,
      easing: 'easeInOutQuart'
    }
  }

  return baseOptions
})

// Methods
const formatCurrency = (amount) => {
  return new Intl.NumberFormat('es-CO', {
    style: 'currency',
    currency: 'COP',
    minimumFractionDigits: 0,
    maximumFractionDigits: 0
  }).format(amount || 0)
}

const formatCurrencyShort = (amount) => {
  if (amount >= 1000000) {
    return `$${(amount / 1000000).toFixed(1)}M`
  } else if (amount >= 1000) {
    return `$${(amount / 1000).toFixed(1)}K`
  }
  return `$${amount}`
}

const createChart = () => {
  if (!chartCanvas.value || !chartData.value) return

  // Destroy existing chart
  if (chart.value) {
    chart.value.destroy()
  }

  const ctx = chartCanvas.value.getContext('2d')
  
  chart.value = new Chart(ctx, {
    type: viewMode.value === 'comparison' ? 'doughnut' : 'bar',
    data: chartData.value,
    options: chartOptions.value
  })
}

const updateChart = () => {
  if (!chart.value || !chartData.value) return

  chart.value.data = chartData.value
  chart.value.options = chartOptions.value
  chart.value.update('active')
}

const refreshData = () => {
  emit('refresh')
}

// Watchers
watch(chartData, async () => {
  if (hasData.value) {
    await nextTick()
    if (chart.value) {
      updateChart()
    } else {
      createChart()
    }
  }
}, { deep: true })

watch(viewMode, async () => {
  await nextTick()
  createChart()
})

// Lifecycle
onMounted(async () => {
  if (hasData.value) {
    await nextTick()
    createChart()
  }
})

onUnmounted(() => {
  if (chart.value) {
    chart.value.destroy()
  }
})
</script>

<style scoped>
canvas {
  height: 400px !important;
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
</style>