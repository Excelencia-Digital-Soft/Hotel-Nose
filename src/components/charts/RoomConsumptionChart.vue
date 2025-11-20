<template>
  <div class="glass-card p-6">
    <!-- Chart Header -->
    <div class="flex items-center justify-between mb-6">
      <div class="flex items-center">
        <div class="bg-gradient-to-r from-purple-400 to-pink-500 p-3 rounded-full mr-3">
          <i class="pi pi-shopping-cart text-white text-xl"></i>
        </div>
        <div>
          <h3 class="text-xl font-bold text-white">🛒 Consumo por Habitación</h3>
          <p class="text-gray-300 text-sm">Análisis detallado de productos consumidos</p>
        </div>
      </div>
      
      <div class="flex items-center space-x-2">
        <select
          v-model="viewMode"
          class="glass-input px-3 py-2 text-sm"
        >
          <option value="total">Total por Habitación</option>
          <option value="details">Desglose de Productos</option>
          <option value="top-products">Productos Populares</option>
        </select>
        
        <button
          @click="toggleChartType"
          v-if="viewMode === 'total'"
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
        <div class="bg-gradient-to-r from-purple-400 to-pink-500 p-4 rounded-full mx-auto w-16 h-16 flex items-center justify-center mb-4">
          <i class="pi pi-spinner pi-spin text-white text-2xl"></i>
        </div>
        <p class="text-white font-semibold">Cargando consumos...</p>
        <p class="text-gray-300 text-sm">Analizando datos de productos</p>
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

      <!-- Consumption Summary -->
      <div class="grid grid-cols-1 md:grid-cols-4 gap-4 mb-4">
        <div class="glass-card p-4 text-center">
          <div class="bg-gradient-to-r from-green-400 to-green-500 text-white p-3 rounded-full mx-auto w-12 h-12 flex items-center justify-center mb-2">
            <i class="pi pi-dollar text-lg"></i>
          </div>
          <p class="text-green-400 font-bold text-lg">{{ formatCurrency(totalConsumption) }}</p>
          <p class="text-gray-300 text-sm">Total Consumos</p>
        </div>
        
        <div class="glass-card p-4 text-center">
          <div class="bg-gradient-to-r from-blue-400 to-blue-500 text-white p-3 rounded-full mx-auto w-12 h-12 flex items-center justify-center mb-2">
            <i class="pi pi-shopping-cart text-lg"></i>
          </div>
          <p class="text-blue-400 font-bold text-lg">{{ formatNumber(totalProducts) }}</p>
          <p class="text-gray-300 text-sm">Productos Únicos</p>
        </div>
        
        <div class="glass-card p-4 text-center">
          <div class="bg-gradient-to-r from-purple-400 to-purple-500 text-white p-3 rounded-full mx-auto w-12 h-12 flex items-center justify-center mb-2">
            <i class="pi pi-home text-lg"></i>
          </div>
          <p class="text-purple-400 font-bold text-lg">{{ roomsWithConsumption }}</p>
          <p class="text-gray-300 text-sm">Habitaciones Activas</p>
        </div>
        
        <div class="glass-card p-4 text-center">
          <div class="bg-gradient-to-r from-yellow-400 to-yellow-500 text-white p-3 rounded-full mx-auto w-12 h-12 flex items-center justify-center mb-2">
            <i class="pi pi-chart-line text-lg"></i>
          </div>
          <p class="text-yellow-400 font-bold text-lg">{{ formatCurrency(averageConsumption) }}</p>
          <p class="text-gray-300 text-sm">Promedio por Habitación</p>
        </div>
      </div>

      <!-- Detailed Analysis -->
      <div v-if="viewMode === 'details'" class="grid grid-cols-1 lg:grid-cols-2 gap-6 mb-6">
        <!-- Room Consumption Details -->
        <div class="glass-card p-4">
          <h4 class="text-white font-bold mb-4 flex items-center">
            <i class="pi pi-list text-primary-400 mr-2"></i>
            📋 Detalles por Habitación
          </h4>
          
          <div class="space-y-3 max-h-96 overflow-y-auto">
            <div
              v-for="room in topRoomsByConsumption"
              :key="room.habitacionID"
              class="glass-button p-3 rounded-lg cursor-pointer"
              @click="toggleRoomDetails(room.habitacionID)"
            >
              <div class="flex items-center justify-between">
                <div class="flex items-center">
                  <div class="bg-gradient-to-r from-purple-400 to-purple-500 text-white w-8 h-8 rounded-full flex items-center justify-center text-xs font-bold mr-3">
                    {{ room.detalles.length }}
                  </div>
                  <div>
                    <p class="text-white font-semibold">{{ room.nombreHabitacion }}</p>
                    <p class="text-gray-300 text-sm">{{ room.nombreCategoria }}</p>
                  </div>
                </div>
                <div class="text-right">
                  <p class="text-purple-400 font-bold">{{ formatCurrency(room.totalConsumos) }}</p>
                  <i class="pi pi-chevron-down text-gray-400 transition-transform" 
                     :class="{ 'rotate-180': expandedRooms.has(room.habitacionID) }"></i>
                </div>
              </div>
              
              <!-- Expanded Details -->
              <div v-if="expandedRooms.has(room.habitacionID)" class="mt-3 pt-3 border-t border-white/20">
                <div class="space-y-2">
                  <div
                    v-for="detail in room.detalles.slice(0, 5)"
                    :key="detail.articuloID"
                    class="flex items-center justify-between text-sm"
                  >
                    <span class="text-gray-300">{{ detail.nombreArticulo }}</span>
                    <div class="text-right">
                      <span class="text-white">x{{ detail.cantidad }}</span>
                      <span class="text-purple-400 ml-2">{{ formatCurrency(detail.precioTotal) }}</span>
                    </div>
                  </div>
                  <div v-if="room.detalles.length > 5" class="text-center text-gray-400 text-xs">
                    ... y {{ room.detalles.length - 5 }} productos más
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>

        <!-- Top Products -->
        <div class="glass-card p-4">
          <h4 class="text-white font-bold mb-4 flex items-center">
            <i class="pi pi-star text-yellow-400 mr-2"></i>
            ⭐ Productos Más Consumidos
          </h4>
          
          <div class="space-y-3">
            <div
              v-for="(product, index) in topProducts"
              :key="product.articuloID"
              class="flex items-center justify-between p-3 glass-button rounded-lg"
            >
              <div class="flex items-center">
                <span class="bg-gradient-to-r from-yellow-400 to-yellow-500 text-white w-6 h-6 rounded-full flex items-center justify-center text-xs font-bold mr-3">
                  {{ index + 1 }}
                </span>
                <div>
                  <p class="text-white font-semibold">{{ product.nombreArticulo }}</p>
                  <p class="text-gray-300 text-xs">{{ product.cantidad }} unidades</p>
                </div>
              </div>
              <p class="text-yellow-400 font-bold">{{ formatCurrency(product.precioTotal) }}</p>
            </div>
          </div>
        </div>
      </div>

      <!-- Top Rooms Table -->
      <div v-if="viewMode === 'total'" class="glass-card p-4">
        <h4 class="text-white font-bold mb-4 flex items-center">
          <i class="pi pi-trophy text-yellow-400 mr-2"></i>
          🏆 Ranking de Consumo por Habitación
        </h4>
        
        <div class="overflow-x-auto">
          <table class="w-full">
            <thead>
              <tr class="border-b border-white/20">
                <th class="text-left py-3 px-4 text-gray-300 font-semibold">#</th>
                <th class="text-left py-3 px-4 text-gray-300 font-semibold">Habitación</th>
                <th class="text-left py-3 px-4 text-gray-300 font-semibold">Categoría</th>
                <th class="text-center py-3 px-4 text-gray-300 font-semibold">Productos</th>
                <th class="text-right py-3 px-4 text-gray-300 font-semibold">Total Consumo</th>
              </tr>
            </thead>
            <tbody>
              <tr
                v-for="(room, index) in topRoomsByConsumption.slice(0, 10)"
                :key="room.habitacionID"
                class="border-b border-white/10 hover:bg-white/5 transition-colors"
              >
                <td class="py-3 px-4">
                  <span class="bg-gradient-to-r from-purple-400 to-purple-500 text-white w-6 h-6 rounded-full flex items-center justify-center text-xs font-bold">
                    {{ index + 1 }}
                  </span>
                </td>
                <td class="py-3 px-4">
                  <span class="text-white font-medium">{{ room.nombreHabitacion }}</span>
                </td>
                <td class="py-3 px-4">
                  <span class="text-gray-300">{{ room.nombreCategoria }}</span>
                </td>
                <td class="py-3 px-4 text-center">
                  <span class="text-blue-400 font-semibold">{{ room.detalles.length }}</span>
                </td>
                <td class="py-3 px-4 text-right">
                  <span class="text-purple-400 font-bold">{{ formatCurrency(room.totalConsumos) }}</span>
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>
    </div>

    <!-- Empty State -->
    <div v-else class="text-center py-16">
      <div class="glass-card max-w-md mx-auto p-8">
        <div class="bg-gradient-to-r from-purple-400 to-pink-500 p-6 rounded-full mx-auto w-24 h-24 flex items-center justify-center mb-4">
          <i class="pi pi-shopping-cart text-white text-4xl"></i>
        </div>
        <h3 class="text-2xl text-white font-bold mb-2">🛒 Sin datos de consumo</h3>
        <p class="text-gray-300 mb-6">No hay datos de consumo para el período seleccionado</p>
        <button
          @click="refreshData"
          class="bg-gradient-to-r from-purple-400 to-pink-500 hover:from-purple-500 hover:to-pink-600 
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
const chartType = ref('bar')
const expandedRooms = ref(new Set())

// Computed properties
const hasData = computed(() => props.data && props.data.length > 0)

const totalConsumption = computed(() => {
  return props.data.reduce((total, room) => total + room.totalConsumos, 0)
})

const totalProducts = computed(() => {
  const uniqueProducts = new Set()
  props.data.forEach(room => {
    room.detalles.forEach(detail => {
      uniqueProducts.add(detail.articuloID)
    })
  })
  return uniqueProducts.size
})

const roomsWithConsumption = computed(() => {
  return props.data.filter(room => room.totalConsumos > 0).length
})

const averageConsumption = computed(() => {
  if (roomsWithConsumption.value === 0) return 0
  return totalConsumption.value / roomsWithConsumption.value
})

const topRoomsByConsumption = computed(() => {
  return [...props.data]
    .filter(room => room.totalConsumos > 0)
    .sort((a, b) => b.totalConsumos - a.totalConsumos)
})

const topProducts = computed(() => {
  const productMap = new Map()
  
  props.data.forEach(room => {
    room.detalles.forEach(detail => {
      const key = detail.articuloID
      if (productMap.has(key)) {
        const existing = productMap.get(key)
        existing.cantidad += detail.cantidad
        existing.precioTotal += detail.precioTotal
      } else {
        productMap.set(key, { ...detail })
      }
    })
  })
  
  return Array.from(productMap.values())
    .sort((a, b) => b.precioTotal - a.precioTotal)
    .slice(0, 10)
})

const chartData = computed(() => {
  if (!hasData.value) return null

  if (viewMode.value === 'top-products') {
    const products = topProducts.value.slice(0, 10)
    return {
      labels: products.map(product => product.nombreArticulo),
      datasets: [{
        label: 'Valor Total Consumido',
        data: products.map(product => product.precioTotal),
        backgroundColor: [
          '#818cf8', '#a78bfa', '#f472b6', '#34d399', '#fbbf24',
          '#f87171', '#60a5fa', '#c084fc', '#fb7185', '#4ade80'
        ],
        borderColor: [
          '#6366f1', '#8b5cf6', '#ec4899', '#10b981', '#f59e0b',
          '#ef4444', '#3b82f6', '#a855f7', '#e11d48', '#059669'
        ],
        borderWidth: 2,
        borderRadius: chartType.value === 'bar' ? 8 : 0,
        borderSkipped: false
      }]
    }
  } else {
    // Top rooms by consumption
    const rooms = topRoomsByConsumption.value.slice(0, 10)
    const labels = rooms.map(room => room.nombreHabitacion)
    const colors = rooms.map((_, index) => {
      const colorPalette = [
        '#818cf8', '#a78bfa', '#f472b6', '#34d399', '#fbbf24',
        '#f87171', '#60a5fa', '#c084fc', '#fb7185', '#4ade80'
      ]
      return colorPalette[index % colorPalette.length]
    })

    return {
      labels,
      datasets: [{
        label: 'Total Consumos',
        data: rooms.map(room => room.totalConsumos),
        backgroundColor: colors,
        borderColor: colors.map(color => color + '80'),
        borderWidth: 2,
        borderRadius: chartType.value === 'bar' ? 8 : 0,
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
          label: function(context) {
            const value = formatCurrency(context.raw)
            if (chartType.value === 'doughnut') {
              const total = context.dataset.data.reduce((a, b) => a + b, 0)
              const percentage = ((context.raw / total) * 100).toFixed(1)
              return `${value} (${percentage}%)`
            }
            return `${context.dataset.label}: ${value}`
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
          color: 'rgba(255, 255, 255, 0.1)',
          drawBorder: false
        }
      },
      y: {
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

const formatNumber = (value) => {
  return new Intl.NumberFormat('es-ES').format(value || 0)
}

const toggleRoomDetails = (roomId) => {
  if (expandedRooms.value.has(roomId)) {
    expandedRooms.value.delete(roomId)
  } else {
    expandedRooms.value.add(roomId)
  }
}

const toggleChartType = () => {
  chartType.value = chartType.value === 'bar' ? 'doughnut' : 'bar'
}

const createChart = () => {
  if (!chartCanvas.value || !chartData.value) return

  // Destroy existing chart
  if (chart.value) {
    chart.value.destroy()
  }

  const ctx = chartCanvas.value.getContext('2d')
  
  chart.value = new Chart(ctx, {
    type: chartType.value,
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

watch([viewMode, chartType], async () => {
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

table {
  border-collapse: separate;
  border-spacing: 0;
}

.rotate-180 {
  transform: rotate(180deg);
}
</style>