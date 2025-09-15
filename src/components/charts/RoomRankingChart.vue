<template>
  <div class="glass-card p-6">
    <!-- Chart Header -->
    <div class="flex items-center justify-between mb-6">
      <div class="flex items-center">
        <div class="bg-gradient-to-r from-primary-400 to-accent-400 p-3 rounded-full mr-3">
          <i class="pi pi-chart-bar text-white text-xl"></i>
        </div>
        <div>
          <h3 class="text-xl font-bold text-white">🏆 Ranking de Habitaciones</h3>
          <p class="text-gray-50 text-sm">Por número total de reservas</p>
        </div>
      </div>
      
      <div class="flex items-center space-x-2">
        <button
          @click="toggleChartType"
          class="glass-button px-3 py-2 text-white hover:bg-white/20 transition-all"
          :title="chartType === 'bar' ? 'Cambiar a gráfico circular' : 'Cambiar a gráfico de barras'"
        >
          <i :class="chartType === 'bar' ? 'pi pi-chart-pie' : 'pi pi-chart-bar'"></i>
        </button>
        
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
        <div class="bg-gradient-to-r from-primary-400 to-accent-400 p-4 rounded-full mx-auto w-16 h-16 flex items-center justify-center mb-4">
          <i class="pi pi-spinner pi-spin text-white text-2xl"></i>
        </div>
        <p class="text-white font-semibold">Cargando ranking...</p>
        <p class="text-gray-300 text-sm">Analizando datos de reservas</p>
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

      <!-- Statistics Summary -->
      <div class="grid grid-cols-1 md:grid-cols-3 gap-4">
        <div class="glass-card p-4 text-center">
          <div class="bg-gradient-to-r from-primary-400 to-primary-500 text-white p-3 rounded-full mx-auto w-12 h-12 flex items-center justify-center mb-2">
            <span class="font-bold text-lg">{{ totalRooms }}</span>
          </div>
          <p class="text-gray-300 text-sm">Habitaciones Activas</p>
        </div>
        
        <div class="glass-card p-4 text-center">
          <div class="bg-gradient-to-r from-secondary-400 to-secondary-500 text-white p-3 rounded-full mx-auto w-12 h-12 flex items-center justify-center mb-2">
            <span class="font-bold text-lg">{{ totalReservations }}</span>
          </div>
          <p class="text-gray-300 text-sm">Total Reservas</p>
        </div>
        
        <div class="glass-card p-4 text-center">
          <div class="bg-gradient-to-r from-accent-400 to-accent-500 text-white p-3 rounded-full mx-auto w-12 h-12 flex items-center justify-center mb-2">
            <span class="font-bold text-lg">{{ averageReservations }}</span>
          </div>
          <p class="text-gray-300 text-sm">Promedio por Habitación</p>
        </div>
      </div>
    </div>

    <!-- Empty State -->
    <div v-else class="text-center py-16">
      <div class="glass-card max-w-md mx-auto p-8">
        <div class="bg-gradient-to-r from-primary-400 to-accent-400 p-6 rounded-full mx-auto w-24 h-24 flex items-center justify-center mb-4">
          <i class="pi pi-chart-bar text-white text-4xl"></i>
        </div>
        <h3 class="text-2xl text-white font-bold mb-2">📊 Sin datos disponibles</h3>
        <p class="text-gray-300 mb-6">No hay datos de reservas para el período seleccionado</p>
        <button
          @click="refreshData"
          class="bg-gradient-to-r from-primary-400 to-accent-400 hover:from-primary-500 hover:to-accent-500 
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
const chartType = ref('bar')

// Computed properties
const hasData = computed(() => {
  console.log('🏆 [RoomRankingChart] hasData check:', {
    data: props.data,
    length: props.data?.length,
    hasData: props.data && props.data.length > 0
  })
  return props.data && props.data.length > 0
})

const totalRooms = computed(() => props.data.length)

const totalReservations = computed(() => {
  return props.data.reduce((total, room) => total + room.totalReservas, 0)
})

const averageReservations = computed(() => {
  if (totalRooms.value === 0) return 0
  return Math.round(totalReservations.value / totalRooms.value)
})

const chartData = computed(() => {
  if (!hasData.value) return null

  // Sort data by total reservations (descending) and take top 10
  const sortedData = [...props.data]
    .sort((a, b) => b.totalReservas - a.totalReservas)
    .slice(0, 10)

  const labels = sortedData.map(room => room.nombreHabitacion)
  const values = sortedData.map(room => room.totalReservas)
  const categories = sortedData.map(room => room.nombreCategoria)

  // Generate colors based on categories
  const colorMap = new Map()
  const colors = [
    '#818cf8', '#a78bfa', '#f472b6', '#34d399', '#fbbf24',
    '#f87171', '#60a5fa', '#c084fc', '#fb7185', '#4ade80'
  ]
  
  let colorIndex = 0
  const backgroundColors = values.map((_, index) => {
    const category = categories[index]
    if (!colorMap.has(category)) {
      colorMap.set(category, colors[colorIndex % colors.length])
      colorIndex++
    }
    return colorMap.get(category)
  })

  return {
    labels,
    datasets: [{
      label: 'Total de Reservas',
      data: values,
      backgroundColor: backgroundColors,
      borderColor: backgroundColors.map(color => color + '80'),
      borderWidth: 2,
      borderRadius: chartType.value === 'bar' ? 8 : 0,
      borderSkipped: false
    }]
  }
})

const chartOptions = computed(() => {
  const baseOptions = {
    responsive: true,
    maintainAspectRatio: false,
    plugins: {
      legend: {
        display: chartType.value === 'doughnut',
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
          title: function(context) {
            return context[0].label
          },
          label: function(context) {
            if (chartType.value === 'doughnut') {
              const total = context.dataset.data.reduce((a, b) => a + b, 0)
              const percentage = ((context.raw / total) * 100).toFixed(1)
              return `${context.raw} reservas (${percentage}%)`
            }
            return `${context.raw} reservas`
          }
        }
      }
    },
    scales: chartType.value === 'bar' ? {
      x: {
        ticks: {
          color: '#9ca3af',
          maxRotation: 45,
          font: {
            size: 11
          }
        },
        grid: {
          color: 'rgba(255, 255, 255, 0.5)',
          drawBorder: false
        }
      },
      y: {
        beginAtZero: true,
        ticks: {
          color: '#9ca3af',
          font: {
            size: 11
          }
        },
        grid: {
          color: 'rgba(255, 255, 255, 0.5)',
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
const createChart = () => {
  console.log('🏆 [RoomRankingChart] createChart called:', {
    chartCanvas: !!chartCanvas.value,
    chartData: !!chartData.value,
    hasData: hasData.value,
    dataLength: props.data?.length
  })
  
  if (!chartCanvas.value || !chartData.value) {
    console.log('🏆 [RoomRankingChart] Cannot create chart - missing canvas or data')
    return
  }

  // Destroy existing chart
  if (chart.value) {
    chart.value.destroy()
  }

  const ctx = chartCanvas.value.getContext('2d')
  
  try {
    chart.value = new Chart(ctx, {
      type: chartType.value,
      data: chartData.value,
      options: chartOptions.value
    })
    console.log('🏆 [RoomRankingChart] Chart created successfully')
  } catch (error) {
    console.error('🏆 [RoomRankingChart] Error creating chart:', error)
  }
}

const updateChart = () => {
  if (!chart.value || !chartData.value) return

  chart.value.data = chartData.value
  chart.value.options = chartOptions.value
  chart.value.update('active')
}

const toggleChartType = () => {
  chartType.value = chartType.value === 'bar' ? 'doughnut' : 'bar'
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

watch(chartType, async () => {
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
</style>
