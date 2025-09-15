<template>
  <div class="space-y-4">
    <!-- Header -->
    <div class="flex items-center justify-between">
      <div class="flex items-center">
        <i class="pi pi-list text-primary-400 text-xl mr-2"></i>
        <h2 class="text-xl font-bold text-white">{{ title }}</h2>
      </div>
      
      <div class="flex items-center space-x-2">
        <!-- Export Button -->
        <button
          @click="handleExport"
          :disabled="!data || data.length === 0"
          class="glass-button py-2 px-4 text-white hover:bg-white/20 transform hover:scale-105 transition-all disabled:opacity-50"
        >
          <i class="pi pi-download mr-2"></i>
          📤 Exportar
        </button>
        
        <!-- Total Items Count -->
        <div class="bg-primary-500/20 px-3 py-1 rounded-full border border-primary-500/30">
          <span class="text-primary-300 text-sm font-semibold">
            {{ data?.length || 0 }} items
          </span>
        </div>
      </div>
    </div>

    <!-- Loading State -->
    <div v-if="loading" class="text-center py-8">
      <div class="glass-card p-6">
        <i class="pi pi-spinner pi-spin text-primary-400 text-3xl mb-3"></i>
        <p class="text-white">Cargando consumos...</p>
      </div>
    </div>

    <!-- Data Table -->
    <div v-else-if="data && data.length > 0" class="glass-card overflow-hidden">
      <!-- Table Header -->
      <div class="bg-white/5 px-6 py-4 border-b border-white/10">
        <div class="grid grid-cols-12 gap-4 text-sm font-semibold text-gray-300">
          <div class="col-span-2">Fecha</div>
          <div class="col-span-3">Artículo</div>
          <div class="col-span-1 text-center">Cant.</div>
          <div class="col-span-2 text-right">P. Unit.</div>
          <div class="col-span-2 text-right">Total</div>
          <div class="col-span-2">Tipo/Habitación</div>
        </div>
      </div>

      <!-- Table Body -->
      <div class="max-h-96 overflow-y-auto">
        <div
          v-for="(item, index) in data"
          :key="item.id || index"
          class="px-6 py-4 border-b border-white/5 hover:bg-white/5 transition-colors"
          :class="{ 'bg-red-500/10': item.anulado }"
        >
          <div class="grid grid-cols-12 gap-4 items-center">
            <!-- Date -->
            <div class="col-span-2">
              <div class="text-white text-sm">
                {{ formatDate(item.fechaConsumo) }}
              </div>
              <div class="text-gray-400 text-xs">
                {{ formatTime(item.fechaConsumo) }}
              </div>
            </div>

            <!-- Article -->
            <div class="col-span-3">
              <div class="text-white font-medium">{{ item.articuloNombre }}</div>
              <div v-if="item.articuloCodigo" class="text-gray-400 text-xs">
                Código: {{ item.articuloCodigo }}
              </div>
              <div v-if="item.observaciones" class="text-gray-400 text-xs">
                {{ item.observaciones }}
              </div>
            </div>

            <!-- Quantity -->
            <div class="col-span-1 text-center">
              <span class="bg-blue-500/20 px-2 py-1 rounded text-blue-300 text-sm font-semibold">
                {{ item.cantidad }}
              </span>
            </div>

            <!-- Unit Price -->
            <div class="col-span-2 text-right">
              <span class="text-white font-medium">
                {{ item.precioUnitarioFormatted || formatCurrency(item.precioUnitario) }}
              </span>
            </div>

            <!-- Total -->
            <div class="col-span-2 text-right">
              <span class="text-green-400 font-bold text-lg">
                {{ item.totalFormatted || formatCurrency(item.total) }}
              </span>
            </div>

            <!-- Type/Room -->
            <div class="col-span-2">
              <div class="flex flex-col space-y-1">
                <span class="px-2 py-1 rounded text-xs font-semibold"
                      :class="getTypeClass(item.tipoConsumo)">
                  {{ item.tipoConsumo || 'General' }}
                </span>
                <div v-if="item.habitacionNumero" class="text-gray-400 text-xs">
                  Hab. {{ item.habitacionNumero }}
                </div>
                <div v-if="item.anulado" class="text-red-400 text-xs font-semibold">
                  ❌ ANULADO
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>

      <!-- Table Footer -->
      <div class="bg-white/5 px-6 py-4 border-t border-white/10">
        <div class="flex justify-between items-center">
          <div class="text-gray-300 text-sm">
            Total de {{ data.length }} registros
          </div>
          <div class="flex items-center space-x-4">
            <div class="text-gray-300 text-sm">
              Total General:
            </div>
            <div class="text-green-400 font-bold text-xl">
              {{ formatCurrency(totalAmount) }}
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- Empty State -->
    <div v-else class="text-center py-12">
      <div class="glass-card p-8">
        <div class="mb-4">
          <i class="pi pi-inbox text-gray-400 text-4xl"></i>
        </div>
        <h3 class="text-white font-bold text-lg mb-2">Sin Consumos</h3>
        <p class="text-gray-300">No se encontraron consumos para el período seleccionado</p>
      </div>
    </div>
  </div>
</template>

<script setup>
import { computed } from 'vue'

const props = defineProps({
  data: {
    type: Array,
    default: () => []
  },
  loading: {
    type: Boolean,
    default: false
  },
  title: {
    type: String,
    default: 'Lista de Consumos'
  }
})

const emit = defineEmits(['export'])

// Computed properties
const totalAmount = computed(() => {
  if (!props.data || props.data.length === 0) return 0
  return props.data
    .filter(item => !item.anulado)
    .reduce((total, item) => total + (item.total || 0), 0)
})

// Methods
const formatDate = (date) => {
  if (!date) return '-'
  return new Date(date).toLocaleDateString('es-PE', {
    day: '2-digit',
    month: '2-digit',
    year: 'numeric'
  })
}

const formatTime = (date) => {
  if (!date) return '-'
  return new Date(date).toLocaleTimeString('es-PE', {
    hour: '2-digit',
    minute: '2-digit'
  })
}

const formatCurrency = (amount) => {
  if (amount == null || amount == undefined) return 'S/ 0.00'
  return new Intl.NumberFormat('es-PE', {
    style: 'currency',
    currency: 'PEN'
  }).format(amount)
}

const getTypeClass = (type) => {
  switch (type) {
    case 'Habitacion':
      return 'bg-purple-500/20 text-purple-300'
    case 'Servicio':
      return 'bg-blue-500/20 text-blue-300'
    default:
      return 'bg-gray-500/20 text-gray-300'
  }
}

const handleExport = () => {
  emit('export')
}
</script>

<style scoped>
.glass-card {
  @apply bg-white/10 backdrop-blur-md border border-white/20 rounded-xl;
}

.glass-button {
  @apply bg-white/10 hover:bg-white/20 backdrop-blur-sm border border-white/30 rounded-lg transition-all;
}

/* Custom scrollbar */
::-webkit-scrollbar {
  width: 6px;
}

::-webkit-scrollbar-track {
  background: rgba(255, 255, 255, 0.1);
  border-radius: 3px;
}

::-webkit-scrollbar-thumb {
  background: rgba(255, 255, 255, 0.3);
  border-radius: 3px;
}

::-webkit-scrollbar-thumb:hover {
  background: rgba(255, 255, 255, 0.5);
}
</style>