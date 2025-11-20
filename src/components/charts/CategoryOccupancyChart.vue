<template>
  <div class="glass-card p-6">
    <!-- Chart Header -->
    <div class="flex items-center justify-between mb-6">
      <div class="flex items-center">
        <div class="bg-gradient-to-r from-blue-400 to-purple-500 p-3 rounded-full mr-3">
          <i class="pi pi-chart-pie text-white text-xl"></i>
        </div>
        <div>
          <h3 class="text-xl font-bold text-white">📊 Ocupación por Categoría</h3>
          <p class="text-gray-300 text-sm">Tasas de ocupación y horas utilizadas</p>
        </div>
      </div>
      
      <div class="flex items-center space-x-2">
        <select
          v-model="viewMode"
          class="glass-input px-3 py-2 text-sm"
        >
          <option value="rate">Tasa de Ocupación</option>
          <option value="hours">Horas Ocupadas</option>
          <option value="combined">Vista Combinada</option>
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
        <div class="bg-gradient-to-r from-blue-400 to-purple-500 p-4 rounded-full mx-auto w-16 h-16 flex items-center justify-center mb-4">
          <i class="pi pi-spinner pi-spin text-white text-2xl"></i>
        </div>
        <p class="text-white font-semibold">Cargando ocupación...</p>
        <p class="text-gray-300 text-sm">Analizando datos de uso</p>
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

      <!-- Occupancy Summary -->
      <div class="grid grid-cols-1 md:grid-cols-4 gap-4 mb-4">
        <div class="glass-card p-4 text-center">
          <div class="bg-gradient-to-r from-green-400 to-green-500 text-white p-3 rounded-full mx-auto w-12 h-12 flex items-center justify-center mb-2">
            <i class="pi pi-percentage text-lg"></i>
          </div>
          <p class="text-green-400 font-bold text-lg">{{ formatPercentage(averageOccupancy) }}</p>
          <p class="text-gray-300 text-sm">Ocupación Promedio</p>
        </div>
        
        <div class="glass-card p-4 text-center">
          <div class="bg-gradient-to-r from-blue-400 to-blue-500 text-white p-3 rounded-full mx-auto w-12 h-12 flex items-center justify-center mb-2">
            <i class="pi pi-clock text-lg"></i>
          </div>
          <p class="text-blue-400 font-bold text-lg">{{ formatNumber(totalHours) }}</p>
          <p class="text-gray-300 text-sm">Total Horas</p>
        </div>
        
        <div class="glass-card p-4 text-center">
          <div class="bg-gradient-to-r from-purple-400 to-purple-500 text-white p-3 rounded-full mx-auto w-12 h-12 flex items-center justify-center mb-2">
            <i class="pi pi-home text-lg"></i>
          </div>
          <p class="text-purple-400 font-bold text-lg">{{ props.data.length }}</p>
          <p class="text-gray-300 text-sm">Categorías</p>
        </div>
        
        <div class="glass-card p-4 text-center">
          <div class="bg-gradient-to-r from-yellow-400 to-yellow-500 text-white p-3 rounded-full mx-auto w-12 h-12 flex items-center justify-center mb-2">
            <i class="pi pi-trophy text-lg"></i>
          </div>
          <p class="text-yellow-400 font-bold text-lg">{{ topCategory?.nombreCategoria || 'N/A' }}</p>
          <p class="text-gray-300 text-sm">Más Ocupada</p>
        </div>
      </div>

      <!-- Category Performance Table -->
      <div class="glass-card p-4">
        <h4 class="text-white font-bold mb-4 flex items-center">
          <i class="pi pi-list text-primary-400 mr-2"></i>
          📋 Rendimiento por Categoría
        </h4>
        
        <div class="overflow-x-auto">
          <table class="w-full">
            <thead>
              <tr class="border-b border-white/20">
                <th class="text-left py-3 px-4 text-gray-300 font-semibold">Categoría</th>
                <th class="text-center py-3 px-4 text-gray-300 font-semibold">Ocupación</th>
                <th class="text-center py-3 px-4 text-gray-300 font-semibold">Horas Ocupadas</th>
                <th class="text-center py-3 px-4 text-gray-300 font-semibold">Estado</th>
              </tr>
            </thead>
            <tbody>
              <tr
                v-for="category in sortedCategories"
                :key="category.categoriaID"
                class="border-b border-white/10 hover:bg-white/5 transition-colors"
              >
                <td class="py-3 px-4">
                  <div class="flex items-center">
                    <div 
                      class="w-3 h-3 rounded-full mr-3"
                      :style="{ backgroundColor: getCategoryColor(category.categoriaID) }"
                    ></div>
                    <span class="text-white font-medium">{{ category.nombreCategoria }}</span>
                  </div>
                </td>
                <td class="py-3 px-4 text-center">
                  <div class="flex items-center justify-center">
                    <div class="relative w-16 h-2 bg-white/20 rounded-full mr-3">
                      <div
                        class="absolute top-0 left-0 h-full bg-gradient-to-r from-blue-400 to-purple-500 rounded-full transition-all duration-300"
                        :style="{ width: `${Math.min(category.tasaOcupacion, 100)}%` }"
                      ></div>
                    </div>
                    <span class="text-white font-bold">{{ formatPercentage(category.tasaOcupacion) }}</span>
                  </div>
                </td>
                <td class="py-3 px-4 text-center">
                  <span class="text-blue-400 font-semibold">{{ formatNumber(category.totalHorasOcupadas) }}h</span>
                </td>
                <td class="py-3 px-4 text-center">
                  <span 
                    class="px-3 py-1 rounded-full text-xs font-semibold"
                    :class="getStatusClass(category.tasaOcupacion)"
                  >
                    {{ getStatusText(category.tasaOcupacion) }}
                  </span>
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>

      <!-- Insights Panel -->
      <div class="grid grid-cols-1 lg:grid-cols-2 gap-6 mt-6">
        <!-- Best Performers -->
        <div class="glass-card p-4">
          <h4 class="text-white font-bold mb-3 flex items-center">
            <i class="pi pi-trophy text-yellow-400 mr-2"></i>
            🏆 Mejores Resultados
          </h4>
          <div class="space-y-3">
            <div
              v-for="(category, index) in topPerformers"
              :key="category.categoriaID"
              class="flex items-center justify-between p-3 glass-button rounded-lg"
            >
              <div class="flex items-center">
                <span class="bg-gradient-to-r from-yellow-400 to-yellow-500 text-white w-6 h-6 rounded-full flex items-center justify-center text-xs font-bold mr-3">
                  {{ index + 1 }}
                </span>
                <span class="text-white font-medium">{{ category.nombreCategoria }}</span>
              </div>
              <div class="text-right">
                <p class="text-green-400 font-bold">{{ formatPercentage(category.tasaOcupacion) }}</p>
                <p class="text-gray-300 text-xs">{{ formatNumber(category.totalHorasOcupadas) }}h</p>
              </div>
            </div>
          </div>
        </div>

        <!-- Improvement Opportunities -->
        <div class="glass-card p-4">
          <h4 class="text-white font-bold mb-3 flex items-center">
            <i class="pi pi-chart-line text-blue-400 mr-2"></i>
            📈 Oportunidades de Mejora
          </h4>
          <div class="space-y-3">
            <div
              v-for="category in improvementOpportunities"
              :key="category.categoriaID"
              class="flex items-center justify-between p-3 glass-button rounded-lg"
            >
              <div class="flex items-center">
                <i class="pi pi-exclamation-circle text-yellow-400 mr-3"></i>
                <span class="text-white font-medium">{{ category.nombreCategoria }}</span>
              </div>
              <div class="text-right">
                <p class="text-yellow-400 font-bold">{{ formatPercentage(category.tasaOcupacion) }}</p>
                <p class="text-gray-300 text-xs">{{ formatNumber(category.totalHorasOcupadas) }}h</p>
              </div>
            </div>
            <div v-if="improvementOpportunities.length === 0" class="text-center py-4">
              <p class="text-gray-300 text-sm">🎉 ¡Todas las categorías tienen buen rendimiento!</p>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- Empty State -->
    <div v-else class="text-center py-16">
      <div class="glass-card max-w-md mx-auto p-8">
        <div class="bg-gradient-to-r from-blue-400 to-purple-500 p-6 rounded-full mx-auto w-24 h-24 flex items-center justify-center mb-4">
          <i class="pi pi-chart-pie text-white text-4xl"></i>
        </div>
        <h3 class="text-2xl text-white font-bold mb-2">📊 Sin datos de ocupación</h3>
        <p class="text-gray-300 mb-6">No hay datos de ocupación para el período seleccionado</p>
        <button
          @click="refreshData"
          class="bg-gradient-to-r from-blue-400 to-purple-500 hover:from-blue-500 hover:to-purple-600 
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
const viewMode = ref('rate')

// Computed properties
const hasData = computed(() => props.data && props.data.length > 0)

const averageOccupancy = computed(() => {
  if (!hasData.value) return 0
  const total = props.data.reduce((sum, category) => sum + category.tasaOcupacion, 0)
  return total / props.data.length
})

const totalHours = computed(() => {
  return props.data.reduce((total, category) => total + category.totalHorasOcupadas, 0)
})

const topCategory = computed(() => {
  if (!hasData.value) return null
  return [...props.data].sort((a, b) => b.tasaOcupacion - a.tasaOcupacion)[0]
})

const sortedCategories = computed(() => {
  return [...props.data].sort((a, b) => b.tasaOcupacion - a.tasaOcupacion)
})

const topPerformers = computed(() => {
  return sortedCategories.value.slice(0, 3)
})

const improvementOpportunities = computed(() => {
  return props.data.filter(category => category.tasaOcupacion < 50)
})

const chartData = computed(() => {
  if (!hasData.value) return null

  const labels = props.data.map(category => category.nombreCategoria)
  const colors = props.data.map((_, index) => getCategoryColor(index))

  if (viewMode.value === 'hours') {
    return {
      labels,
      datasets: [{
        label: 'Horas Ocupadas',
        data: props.data.map(category => category.totalHorasOcupadas),
        backgroundColor: colors,
        borderColor: colors.map(color => color + '80'),
        borderWidth: 2,
        borderRadius: 8,
        borderSkipped: false
      }]
    }
  } else if (viewMode.value === 'combined') {
    return {
      labels,
      datasets: [
        {
          label: 'Tasa de Ocupación (%)',
          data: props.data.map(category => category.tasaOcupacion),
          backgroundColor: '#60a5fa80',
          borderColor: '#3b82f6',
          borderWidth: 2,
          yAxisID: 'y',
          type: 'bar',
          borderRadius: 8,
          borderSkipped: false
        },
        {
          label: 'Horas Ocupadas',
          data: props.data.map(category => category.totalHorasOcupadas),
          backgroundColor: '#a78bfa',
          borderColor: '#8b5cf6',
          borderWidth: 2,
          yAxisID: 'y1',
          type: 'line',
          fill: false,
          tension: 0.4
        }
      ]
    }
  } else {
    return {
      labels,
      datasets: [{
        label: 'Tasa de Ocupación',
        data: props.data.map(category => category.tasaOcupacion),
        backgroundColor: colors,
        borderColor: colors.map(color => color + '80'),
        borderWidth: 2
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
        display: viewMode.value === 'combined',
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
            if (viewMode.value === 'hours') {
              return `${context.dataset.label}: ${context.raw} horas`
            } else if (context.dataset.label === 'Tasa de Ocupación (%)' || viewMode.value === 'rate') {
              return `${context.dataset.label}: ${context.raw.toFixed(1)}%`
            }
            return `${context.dataset.label}: ${context.raw}`
          }
        }
      }
    },
    scales: viewMode.value === 'rate' ? {} : viewMode.value === 'combined' ? {
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
        type: 'linear',
        display: true,
        position: 'left',
        beginAtZero: true,
        max: 100,
        ticks: {
          color: '#9ca3af',
          font: {
            size: 11
          },
          callback: function(value) {
            return value + '%'
          }
        },
        grid: {
          color: 'rgba(255, 255, 255, 0.1)',
          drawBorder: false
        }
      },
      y1: {
        type: 'linear',
        display: true,
        position: 'right',
        beginAtZero: true,
        ticks: {
          color: '#9ca3af',
          font: {
            size: 11
          }
        },
        grid: {
          drawOnChartArea: false
        }
      }
    } : {
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
          }
        },
        grid: {
          color: 'rgba(255, 255, 255, 0.1)',
          drawBorder: false
        }
      }
    },
    animation: {
      duration: 1000,
      easing: 'easeInOutQuart'
    }
  }

  return baseOptions
})

// Methods
const formatPercentage = (value) => {
  return `${(value || 0).toFixed(1)}%`
}

const formatNumber = (value) => {
  return new Intl.NumberFormat('es-ES').format(value || 0)
}

const getCategoryColor = (index) => {
  const colors = [
    '#818cf8', '#a78bfa', '#f472b6', '#34d399', '#fbbf24',
    '#f87171', '#60a5fa', '#c084fc', '#fb7185', '#4ade80'
  ]
  return colors[index % colors.length]
}

const getStatusClass = (occupancy) => {
  if (occupancy >= 80) return 'bg-green-500/20 text-green-400 border border-green-500/30'
  if (occupancy >= 60) return 'bg-yellow-500/20 text-yellow-400 border border-yellow-500/30'
  if (occupancy >= 30) return 'bg-orange-500/20 text-orange-400 border border-orange-500/30'
  return 'bg-red-500/20 text-red-400 border border-red-500/30'
}

const getStatusText = (occupancy) => {
  if (occupancy >= 80) return '🟢 Excelente'
  if (occupancy >= 60) return '🟡 Buena'
  if (occupancy >= 30) return '🟠 Regular'
  return '🔴 Baja'
}

const createChart = () => {
  if (!chartCanvas.value || !chartData.value) return

  // Destroy existing chart
  if (chart.value) {
    chart.value.destroy()
  }

  const ctx = chartCanvas.value.getContext('2d')
  
  chart.value = new Chart(ctx, {
    type: viewMode.value === 'rate' ? 'doughnut' : viewMode.value === 'combined' ? 'bar' : 'bar',
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

table {
  border-collapse: separate;
  border-spacing: 0;
}
</style>