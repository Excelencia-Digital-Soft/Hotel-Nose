<template>
  <div class="space-y-6">
    <!-- Loading State -->
    <div v-if="loading" class="text-center py-8">
      <div class="glass-card p-6">
        <i class="pi pi-spinner pi-spin text-primary-400 text-3xl mb-3"></i>
        <p class="text-white">Cargando gráficos...</p>
      </div>
    </div>

    <!-- Charts Content -->
    <div v-else class="space-y-6">
      <!-- Row 1: Consumption by Service & Date Trend -->
      <div class="grid grid-cols-1 xl:grid-cols-2 gap-6">
        <!-- Consumption by Service Type -->
        <div class="glass-card p-6">
          <div class="flex items-center mb-6">
            <i class="pi pi-chart-pie text-secondary-400 text-xl mr-2"></i>
            <h3 class="text-xl font-bold text-white">🎯 Consumo por Tipo de Servicio</h3>
          </div>

          <div v-if="consumptionByService && consumptionByService.length > 0">
            <!-- Pie Chart Simulation -->
            <div class="relative h-64 mb-6">
              <div class="absolute inset-0 flex items-center justify-center">
                <div class="relative w-48 h-48 rounded-full border-8 border-white/10">
                  <div
                    v-for="(segment, index) in serviceChartSegments"
                    :key="index"
                    class="absolute inset-0 rounded-full"
                    :style="segment.style"
                  ></div>
                  <div class="absolute inset-0 flex items-center justify-center">
                    <div class="text-center">
                      <p class="text-white font-bold text-lg">{{ formatCurrency(totalServiceAmount) }}</p>
                      <p class="text-gray-300 text-sm">Total</p>
                    </div>
                  </div>
                </div>
              </div>
            </div>

            <!-- Legend -->
            <div class="space-y-3">
              <div
                v-for="service in consumptionByService"
                :key="service.label"
                class="flex items-center justify-between bg-white/5 p-3 rounded-lg"
              >
                <div class="flex items-center">
                  <div class="w-4 h-4 rounded-full mr-3"
                       :style="{ backgroundColor: getServiceColor(service.label) }"></div>
                  <div>
                    <p class="text-white font-medium">{{ service.label }}</p>
                    <p class="text-gray-400 text-sm">{{ service.items }} items</p>
                  </div>
                </div>
                <div class="text-right">
                  <p class="text-green-400 font-bold">{{ formatCurrency(service.value) }}</p>
                  <p class="text-gray-400 text-sm">{{ service.percentage.toFixed(1) }}%</p>
                </div>
              </div>
            </div>
          </div>

          <div v-else class="text-center py-8">
            <i class="pi pi-chart-pie text-gray-400 text-3xl mb-3"></i>
            <p class="text-gray-300">Sin datos por servicio</p>
          </div>
        </div>

        <!-- Consumption Trend by Date -->
        <div class="glass-card p-6">
          <div class="flex items-center mb-6">
            <i class="pi pi-chart-line text-primary-400 text-xl mr-2"></i>
            <h3 class="text-xl font-bold text-white">📈 Tendencia por Fecha</h3>
          </div>

          <div v-if="consumptionByDate && consumptionByDate.length > 0">
            <!-- Simple Line Chart Simulation -->
            <div class="h-64 relative mb-4">
              <div class="absolute inset-0 flex items-end justify-between px-2">
                <div
                  v-for="(dataPoint, index) in consumptionByDate.slice(-7)"
                  :key="index"
                  class="flex flex-col items-center flex-1 mx-1"
                >
                  <!-- Bar -->
                  <div class="w-full bg-gradient-to-t from-primary-400 to-primary-600 rounded-t-md mb-2 relative"
                       :style="{ height: getBarHeight(dataPoint.total) + 'px' }">
                    <!-- Tooltip -->
                    <div class="absolute -top-8 left-1/2 transform -translate-x-1/2 bg-black/80 text-white text-xs px-2 py-1 rounded opacity-0 hover:opacity-100 transition-opacity whitespace-nowrap">
                      {{ formatCurrency(dataPoint.total) }}
                    </div>
                  </div>
                  
                  <!-- Date Label -->
                  <p class="text-gray-400 text-xs transform -rotate-45 origin-bottom">
                    {{ formatShortDate(dataPoint.date) }}
                  </p>
                </div>
              </div>
            </div>

            <!-- Statistics -->
            <div class="grid grid-cols-3 gap-4">
              <div class="text-center">
                <p class="text-primary-400 font-bold text-lg">{{ consumptionByDate.length }}</p>
                <p class="text-gray-300 text-xs">Días con consumo</p>
              </div>
              <div class="text-center">
                <p class="text-green-400 font-bold text-lg">{{ formatCurrency(averageDailyConsumption) }}</p>
                <p class="text-gray-300 text-xs">Promedio diario</p>
              </div>
              <div class="text-center">
                <p class="text-yellow-400 font-bold text-lg">{{ formatCurrency(maxDailyConsumption) }}</p>
                <p class="text-gray-300 text-xs">Máximo diario</p>
              </div>
            </div>
          </div>

          <div v-else class="text-center py-8">
            <i class="pi pi-chart-line text-gray-400 text-3xl mb-3"></i>
            <p class="text-gray-300">Sin datos de tendencia</p>
          </div>
        </div>
      </div>

      <!-- Row 2: Top Items -->
      <div class="glass-card p-6">
        <div class="flex items-center mb-6">
          <i class="pi pi-star text-accent-400 text-xl mr-2"></i>
          <h3 class="text-xl font-bold text-white">🏆 Top Artículos Consumidos</h3>
        </div>

        <div v-if="topItems && topItems.length > 0">
          <div class="space-y-4">
            <div
              v-for="(item, index) in topItems"
              :key="item.articuloId"
              class="bg-white/5 p-4 rounded-lg border border-white/10 hover:bg-white/10 transition-colors"
            >
              <div class="flex items-center justify-between">
                <!-- Ranking & Item Info -->
                <div class="flex items-center flex-1">
                  <!-- Ranking Badge -->
                  <div class="mr-4">
                    <div class="w-10 h-10 rounded-full flex items-center justify-center font-bold"
                         :class="getRankingClass(index)">
                      {{ index + 1 }}
                    </div>
                  </div>

                  <!-- Item Details -->
                  <div class="flex-1">
                    <p class="text-white font-semibold text-lg">{{ item.articuloNombre }}</p>
                    <div class="flex items-center space-x-4 mt-1">
                      <span class="text-blue-400 text-sm">
                        📦 {{ item.totalQuantity }} unidades
                      </span>
                      <span class="text-purple-400 text-sm">
                        💰 {{ formatCurrency(item.totalAmount / item.totalQuantity) }} c/u
                      </span>
                    </div>
                  </div>
                </div>

                <!-- Progress Bar & Amount -->
                <div class="text-right min-w-[120px]">
                  <p class="text-green-400 font-bold text-xl mb-2">{{ formatCurrency(item.totalAmount) }}</p>
                  
                  <!-- Progress Bar -->
                  <div class="w-24 h-2 bg-white/20 rounded-full overflow-hidden">
                    <div class="h-full bg-gradient-to-r from-green-400 to-green-500 rounded-full transition-all duration-300"
                         :style="{ width: getItemPercentage(item.totalAmount) + '%' }"></div>
                  </div>
                  <p class="text-gray-400 text-xs mt-1">{{ getItemPercentage(item.totalAmount) }}%</p>
                </div>
              </div>
            </div>
          </div>
        </div>

        <div v-else class="text-center py-8">
          <i class="pi pi-star text-gray-400 text-3xl mb-3"></i>
          <p class="text-gray-300">Sin datos de artículos más consumidos</p>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { computed } from 'vue'

const props = defineProps({
  consumptionByService: {
    type: Array,
    default: () => []
  },
  consumptionByDate: {
    type: Array,
    default: () => []
  },
  topItems: {
    type: Array,
    default: () => []
  },
  loading: {
    type: Boolean,
    default: false
  }
})

// Computed properties
const totalServiceAmount = computed(() => {
  return props.consumptionByService.reduce((total, service) => total + service.value, 0)
})

const maxDailyConsumption = computed(() => {
  if (!props.consumptionByDate || props.consumptionByDate.length === 0) return 0
  return Math.max(...props.consumptionByDate.map(d => d.total))
})

const averageDailyConsumption = computed(() => {
  if (!props.consumptionByDate || props.consumptionByDate.length === 0) return 0
  const total = props.consumptionByDate.reduce((sum, d) => sum + d.total, 0)
  return total / props.consumptionByDate.length
})

const serviceChartSegments = computed(() => {
  if (!props.consumptionByService || props.consumptionByService.length === 0) return []
  
  let currentAngle = 0
  return props.consumptionByService.map((service, index) => {
    const angle = (service.percentage / 100) * 360
    const style = {
      background: `conic-gradient(from ${currentAngle}deg, ${getServiceColor(service.label)} 0deg, ${getServiceColor(service.label)} ${angle}deg, transparent ${angle}deg)`,
      mask: 'radial-gradient(circle at center, transparent 60px, black 60px)'
    }
    currentAngle += angle
    return { style, service }
  })
})

const totalTopItemsAmount = computed(() => {
  return props.topItems.reduce((total, item) => total + item.totalAmount, 0)
})

// Methods
const formatCurrency = (amount) => {
  if (amount == null || amount == undefined) return 'S/ 0.00'
  return new Intl.NumberFormat('es-PE', {
    style: 'currency',
    currency: 'PEN'
  }).format(amount)
}

const formatShortDate = (date) => {
  if (!date) return '-'
  return new Date(date).toLocaleDateString('es-PE', {
    day: '2-digit',
    month: '2-digit'
  })
}

const getServiceColor = (serviceType) => {
  const colors = {
    'Habitacion': '#8b5cf6', // Purple
    'Servicio': '#3b82f6',   // Blue
    'General': '#6b7280'     // Gray
  }
  return colors[serviceType] || colors['General']
}

const getRankingClass = (index) => {
  switch (index) {
    case 0:
      return 'bg-gradient-to-r from-yellow-400 to-yellow-500 text-white'
    case 1:
      return 'bg-gradient-to-r from-gray-400 to-gray-500 text-white'
    case 2:
      return 'bg-gradient-to-r from-amber-600 to-amber-700 text-white'
    default:
      return 'bg-gradient-to-r from-blue-400 to-blue-500 text-white'
  }
}

const getBarHeight = (value) => {
  if (!value || maxDailyConsumption.value === 0) return 0
  const maxHeight = 200 // px
  return Math.max((value / maxDailyConsumption.value) * maxHeight, 4)
}

const getItemPercentage = (amount) => {
  if (!amount || !totalTopItemsAmount.value) return 0
  return Math.round((amount / totalTopItemsAmount.value) * 100)
}
</script>

<style scoped>
.glass-card {
  @apply bg-white/10 backdrop-blur-md border border-white/20 rounded-xl;
}
</style>