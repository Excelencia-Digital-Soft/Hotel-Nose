<template>
  <div 
    class="glass-card p-6 cursor-pointer hover:bg-white/15 transition-all duration-300 group transform hover:scale-[1.02] hover:shadow-2xl"
    @click="$emit('click', cierre)"
  >
    <!-- Header with Status -->
    <div class="flex items-center justify-between mb-4">
      <div class="flex items-center">
        <div class="bg-gradient-to-r from-primary-400 to-secondary-500 p-3 rounded-full mr-3 group-hover:scale-110 transition-transform">
          <i class="pi pi-calculator text-white text-lg"></i>
        </div>
        <div>
          <h4 class="text-lg font-bold text-white mb-1">
            🧮 Cierre #{{ cierre.cajaId }}
          </h4>
          <p class="text-gray-300 text-sm">
            {{ formatDate(cierre.fechaHoraCierre) }}
          </p>
        </div>
      </div>
      
      <!-- Status Badge -->
      <div class="bg-green-500/20 px-3 py-1 rounded-full border border-green-500/30">
        <span class="text-green-400 text-xs font-semibold flex items-center">
          <i class="pi pi-check-circle mr-1"></i>
          Cerrado
        </span>
      </div>
    </div>

    <!-- Financial Summary -->
    <div class="grid grid-cols-2 gap-4 mb-4">
      <!-- Initial Amount -->
      <div class="bg-white/5 rounded-lg p-3 border border-white/10">
        <div class="flex items-center mb-1">
          <i class="pi pi-wallet text-blue-400 text-sm mr-1"></i>
          <span class="text-gray-300 text-xs">Inicial</span>
        </div>
        <p class="text-white font-semibold text-sm">
          ${{ formatCurrency(cierre.montoInicial) }}
        </p>
      </div>

      <!-- Final Amount -->
      <div class="bg-white/5 rounded-lg p-3 border border-white/10">
        <div class="flex items-center mb-1">
          <i class="pi pi-money-bill text-green-400 text-sm mr-1"></i>
          <span class="text-gray-300 text-xs">Final</span>
        </div>
        <p class="text-white font-semibold text-sm">
          ${{ formatCurrency(cierre.montoFinal) }}
        </p>
      </div>
    </div>

    <!-- Payment Methods -->
    <div class="grid grid-cols-3 gap-2 mb-4">
      <!-- Cash -->
      <div class="bg-gradient-to-br from-emerald-500/10 to-emerald-600/5 rounded-lg p-2 border border-emerald-500/20">
        <div class="flex items-center mb-1">
          <i class="pi pi-money-bill text-emerald-400 text-xs mr-1"></i>
          <span class="text-gray-300 text-xs">Efectivo</span>
        </div>
        <p class="text-emerald-400 font-semibold text-xs">
          ${{ formatCurrency(cierre.totalEfectivo) }}
        </p>
      </div>

      <!-- Card -->
      <div class="bg-gradient-to-br from-blue-500/10 to-blue-600/5 rounded-lg p-2 border border-blue-500/20">
        <div class="flex items-center mb-1">
          <i class="pi pi-credit-card text-blue-400 text-xs mr-1"></i>
          <span class="text-gray-300 text-xs">Tarjeta</span>
        </div>
        <p class="text-blue-400 font-semibold text-xs">
          ${{ formatCurrency(cierre.totalTarjeta) }}
        </p>
      </div>

      <!-- Discounts -->
      <div class="bg-gradient-to-br from-orange-500/10 to-orange-600/5 rounded-lg p-2 border border-orange-500/20">
        <div class="flex items-center mb-1">
          <i class="pi pi-percentage text-orange-400 text-xs mr-1"></i>
          <span class="text-gray-300 text-xs">Desc.</span>
        </div>
        <p class="text-orange-400 font-semibold text-xs">
          ${{ formatCurrency(cierre.totalDescuentos) }}
        </p>
      </div>
    </div>

    <!-- Closure Time and Observation -->
    <div class="border-t border-white/10 pt-3">
      <div class="flex items-center justify-between">
        <div class="flex items-center text-gray-300 text-xs">
          <i class="pi pi-clock mr-1"></i>
          {{ formatTime(cierre.fechaHoraCierre) }}
        </div>
        
        <!-- Action Arrow -->
        <div class="flex items-center space-x-2">
          <span class="text-gray-400 text-xs">Ver detalles</span>
          <i class="pi pi-chevron-right text-gray-400 group-hover:text-white transition-colors"></i>
        </div>
      </div>
      
      <!-- Observation (if exists) -->
      <div v-if="cierre.observacion" class="mt-2 p-2 bg-white/5 rounded-md border border-white/10">
        <p class="text-gray-300 text-xs italic">
          "{{ cierre.observacion }}"
        </p>
      </div>
    </div>

    <!-- Hover Effect Overlay -->
    <div class="absolute inset-0 bg-gradient-to-r from-primary-400/5 to-secondary-400/5 rounded-lg opacity-0 group-hover:opacity-100 transition-opacity duration-300 pointer-events-none"></div>
  </div>
</template>

<script setup>
import { defineProps, defineEmits } from 'vue';

const props = defineProps({
  cierre: {
    type: Object,
    required: true
  }
});

const emit = defineEmits(['click']);

// Format currency helper
const formatCurrency = (amount) => {
  if (amount === null || amount === undefined) return '0.00';
  return Number(amount).toLocaleString('es-AR', {
    minimumFractionDigits: 2,
    maximumFractionDigits: 2
  });
};

// Date formatting utilities
const formatDate = (fechaHora) => {
  if (!fechaHora) return 'Sin fecha';
  const date = new Date(fechaHora);
  return date.toLocaleDateString('es-ES', {
    year: 'numeric',
    month: 'short',
    day: 'numeric',
  });
};

const formatTime = (fechaHora) => {
  if (!fechaHora) return 'Sin hora';
  const date = new Date(fechaHora);
  return date.toLocaleTimeString('es-ES', {
    hour: '2-digit',
    minute: '2-digit',
  });
};
</script>

<style scoped>
.glass-card {
  background-color: rgba(255, 255, 255, 0.1);
  backdrop-filter: blur(16px);
  border: 1px solid rgba(255, 255, 255, 0.2);
  border-radius: 12px;
  position: relative;
  overflow: hidden;
}

/* Hover shadow effect */
.glass-card:hover {
  box-shadow: 
    0 20px 25px -5px rgba(0, 0, 0, 0.3), 
    0 10px 10px -5px rgba(0, 0, 0, 0.2),
    0 0 0 1px rgba(255, 255, 255, 0.1);
}

/* Smooth transitions */
.transition-all {
  transition: all 0.3s cubic-bezier(0.4, 0, 0.2, 1);
}

.transition-transform {
  transition: transform 0.3s cubic-bezier(0.4, 0, 0.2, 1);
}

.transition-colors {
  transition: color 0.3s cubic-bezier(0.4, 0, 0.2, 1);
}

.transition-opacity {
  transition: opacity 0.3s cubic-bezier(0.4, 0, 0.2, 1);
}

/* Custom hover animations */
.group:hover .group-hover\:scale-110 {
  transform: scale(1.1);
}

.group:hover .group-hover\:text-white {
  color: rgb(255 255 255);
}
</style>