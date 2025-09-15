<template>
  <div class="space-y-6">
    <!-- Loading State -->
    <div v-if="loading" class="text-center py-8">
      <div class="glass-card p-6">
        <i class="pi pi-spinner pi-spin text-primary-400 text-3xl mb-3"></i>
        <p class="text-white">Cargando resumen...</p>
      </div>
    </div>

    <!-- Summary Content -->
    <div v-else-if="summary">
      <!-- Main Summary Stats -->
      <div class="glass-card p-6">
        <div class="flex items-center mb-6">
          <i class="pi pi-chart-pie text-accent-400 text-xl mr-2"></i>
          <h2 class="text-xl font-bold text-white">📊 Resumen General</h2>
        </div>

        <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6">
          <!-- Total Items -->
          <div class="text-center">
            <div class="bg-gradient-to-r from-blue-400 to-blue-500 text-white p-4 rounded-full mx-auto w-16 h-16 flex items-center justify-center mb-3">
              <i class="pi pi-shopping-cart text-2xl"></i>
            </div>
            <p class="text-blue-400 font-bold text-3xl">{{ summary.totalItems || 0 }}</p>
            <p class="text-gray-300 text-sm">Total Items</p>
          </div>

          <!-- Total Quantity -->
          <div class="text-center">
            <div class="bg-gradient-to-r from-purple-400 to-purple-500 text-white p-4 rounded-full mx-auto w-16 h-16 flex items-center justify-center mb-3">
              <i class="pi pi-box text-2xl"></i>
            </div>
            <p class="text-purple-400 font-bold text-3xl">{{ summary.totalQuantity || 0 }}</p>
            <p class="text-gray-300 text-sm">Cantidad Total</p>
          </div>

          <!-- Total Amount -->
          <div class="text-center">
            <div class="bg-gradient-to-r from-green-400 to-green-500 text-white p-4 rounded-full mx-auto w-16 h-16 flex items-center justify-center mb-3">
              <i class="pi pi-dollar text-2xl"></i>
            </div>
            <p class="text-green-400 font-bold text-2xl">{{ formatCurrency(summary.totalAmount) }}</p>
            <p class="text-gray-300 text-sm">Monto Total</p>
          </div>

          <!-- Period -->
          <div class="text-center">
            <div class="bg-gradient-to-r from-yellow-400 to-yellow-500 text-white p-4 rounded-full mx-auto w-16 h-16 flex items-center justify-center mb-3">
              <i class="pi pi-calendar text-2xl"></i>
            </div>
            <p class="text-yellow-400 font-bold text-sm">{{ formatPeriod() }}</p>
            <p class="text-gray-300 text-sm">Período</p>
          </div>
        </div>
      </div>

      <!-- Amount by Type -->
      <div v-if="summary.amountByType && Object.keys(summary.amountByType).length > 0" class="glass-card p-6">
        <div class="flex items-center mb-6">
          <i class="pi pi-tags text-secondary-400 text-xl mr-2"></i>
          <h3 class="text-xl font-bold text-white">🏷️ Distribución por Tipo</h3>
        </div>

        <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
          <div
            v-for="(amount, type) in summary.amountByType"
            :key="type"
            class="bg-white/5 p-4 rounded-lg border border-white/10 hover:bg-white/10 transition-colors"
          >
            <div class="flex justify-between items-center">
              <div>
                <p class="text-white font-semibold">{{ type }}</p>
                <p class="text-gray-400 text-sm">{{ getTypeDescription(type) }}</p>
              </div>
              <div class="text-right">
                <p class="text-green-400 font-bold text-lg">{{ formatCurrency(amount) }}</p>
                <p class="text-gray-400 text-sm">{{ getPercentage(amount) }}%</p>
              </div>
            </div>
          </div>
        </div>
      </div>

      <!-- Top Consumed Items -->
      <div v-if="summary.topConsumedItems && summary.topConsumedItems.length > 0" class="glass-card p-6">
        <div class="flex items-center mb-6">
          <i class="pi pi-star text-accent-400 text-xl mr-2"></i>
          <h3 class="text-xl font-bold text-white">⭐ Artículos Más Consumidos</h3>
        </div>

        <div class="space-y-3">
          <div
            v-for="(item, index) in summary.topConsumedItems"
            :key="item.articuloId"
            class="bg-white/5 p-4 rounded-lg border border-white/10 hover:bg-white/10 transition-colors"
          >
            <div class="flex items-center justify-between">
              <div class="flex items-center">
                <!-- Ranking Badge -->
                <div class="mr-4">
                  <div class="w-8 h-8 rounded-full flex items-center justify-center font-bold text-sm"
                       :class="getRankingClass(index)">
                    {{ index + 1 }}
                  </div>
                </div>

                <!-- Item Info -->
                <div>
                  <p class="text-white font-semibold">{{ item.articuloNombre }}</p>
                  <p class="text-gray-400 text-sm">
                    Cantidad: {{ item.totalQuantity }} | Promedio: {{ formatCurrency(item.totalAmount / item.totalQuantity) }}
                  </p>
                </div>
              </div>

              <!-- Amount -->
              <div class="text-right">
                <p class="text-green-400 font-bold text-lg">{{ formatCurrency(item.totalAmount) }}</p>
                <p class="text-gray-400 text-sm">{{ getItemPercentage(item.totalAmount) }}%</p>
              </div>
            </div>
          </div>
        </div>
      </div>

      <!-- By Service Breakdown -->
      <div v-if="byService && byService.length > 0" class="glass-card p-6">
        <div class="flex items-center mb-6">
          <i class="pi pi-list text-primary-400 text-xl mr-2"></i>
          <h3 class="text-xl font-bold text-white">🛎️ Desglose por Servicio</h3>
        </div>

        <div class="space-y-4">
          <div
            v-for="service in byService"
            :key="service.serviceType"
            class="bg-white/5 p-4 rounded-lg border border-white/10"
          >
            <!-- Service Header -->
            <div class="flex justify-between items-center mb-4">
              <div>
                <h4 class="text-white font-bold text-lg">{{ service.serviceType }}</h4>
                <p class="text-gray-400 text-sm">{{ service.totalItems }} items</p>
              </div>
              <div class="text-right">
                <p class="text-green-400 font-bold text-xl">{{ formatCurrency(service.totalAmount) }}</p>
                <p class="text-gray-400 text-sm">{{ service.percentage.toFixed(1) }}% del total</p>
              </div>
            </div>

            <!-- Service Items -->
            <div v-if="service.items && service.items.length > 0" class="space-y-2">
              <div
                v-for="item in service.items.slice(0, 5)"
                :key="item.articuloId"
                class="flex justify-between items-center bg-white/5 p-3 rounded"
              >
                <div>
                  <p class="text-white font-medium">{{ item.articuloNombre }}</p>
                  <p class="text-gray-400 text-xs">
                    Último: {{ formatDate(item.lastConsumedDate) }}
                  </p>
                </div>
                <div class="text-right">
                  <p class="text-green-400 font-semibold">{{ formatCurrency(item.amount) }}</p>
                  <p class="text-gray-400 text-xs">Cant: {{ item.quantity }}</p>
                </div>
              </div>
              
              <div v-if="service.items.length > 5" class="text-center pt-2">
                <p class="text-gray-400 text-sm">y {{ service.items.length - 5 }} artículos más...</p>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- Empty State -->
    <div v-else class="text-center py-12">
      <div class="glass-card p-8">
        <div class="mb-4">
          <i class="pi pi-chart-pie text-gray-400 text-4xl"></i>
        </div>
        <h3 class="text-white font-bold text-lg mb-2">Sin Datos de Resumen</h3>
        <p class="text-gray-300">No hay datos suficientes para mostrar el resumen</p>
      </div>
    </div>
  </div>
</template>

<script setup>
import { computed } from 'vue'

const props = defineProps({
  summary: {
    type: Object,
    default: null
  },
  byService: {
    type: Array,
    default: () => []
  },
  loading: {
    type: Boolean,
    default: false
  }
})

// Computed properties
const totalAmount = computed(() => props.summary?.totalAmount || 0)

// Methods
const formatCurrency = (amount) => {
  if (amount == null || amount == undefined) return 'S/ 0.00'
  return new Intl.NumberFormat('es-PE', {
    style: 'currency',
    currency: 'PEN'
  }).format(amount)
}

const formatDate = (date) => {
  if (!date) return '-'
  return new Date(date).toLocaleDateString('es-PE', {
    day: '2-digit',
    month: '2-digit',
    year: 'numeric'
  })
}

const formatPeriod = () => {
  if (!props.summary?.periodStart || !props.summary?.periodEnd) return 'Sin período'
  
  const start = new Date(props.summary.periodStart).toLocaleDateString('es-PE')
  const end = new Date(props.summary.periodEnd).toLocaleDateString('es-PE')
  
  if (start === end) return start
  return `${start} - ${end}`
}

const getTypeDescription = (type) => {
  switch (type) {
    case 'Habitacion':
      return 'Consumos en habitación'
    case 'Servicio':
      return 'Servicios generales'
    default:
      return 'Otros consumos'
  }
}

const getPercentage = (amount) => {
  if (!amount || !totalAmount.value) return 0
  return Math.round((amount / totalAmount.value) * 100)
}

const getItemPercentage = (amount) => {
  if (!amount || !totalAmount.value) return 0
  return ((amount / totalAmount.value) * 100).toFixed(1)
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
</script>

<style scoped>
.glass-card {
  @apply bg-white/10 backdrop-blur-md border border-white/20 rounded-xl;
}
</style>