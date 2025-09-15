<!-- File: src/components/CerrarCajaModal.vue -->
<template>
  <!-- Modal container with glassmorphism -->
  <div class="fixed inset-0 z-50 flex items-center justify-center p-4">
    <!-- Backdrop with blur -->
    <div class="absolute inset-0 bg-black/60 backdrop-blur-xl" @click.self="closeModal"></div>
    
    <!-- Modal content with animations -->
    <div 
      class="relative w-full max-w-md glass-container p-0 transform transition-all duration-500 scale-100 opacity-100"
      :class="{ 'scale-95 opacity-0': isClosing }"
    >
      <!-- Header -->
      <div class="glass-header p-6 border-b border-white/20">
        <div class="flex items-center space-x-4">
          <div class="bg-gradient-to-r from-red-400 to-red-600 p-3 rounded-full">
            <i class="pi pi-lock text-white text-2xl"></i>
          </div>
          <div>
            <h3 class="text-2xl font-bold text-white">🔒 Cerrar Caja</h3>
            <p class="text-gray-300 text-sm mt-1">Confirma el cierre de caja actual</p>
          </div>
        </div>
      </div>

      <!-- Body -->
      <div class="p-6">
        <!-- Loading overlay -->
        <div v-if="isLoading" class="absolute inset-0 bg-black/50 backdrop-blur-sm rounded-3xl flex items-center justify-center z-10">
          <div class="glass-card p-6 flex flex-col items-center">
            <ProgressSpinner 
              style="width: 75px; height: 75px" 
              strokeWidth="6" 
              fill="transparent"
              animationDuration=".5s" 
              aria-label="Loading"
              class="mb-4"
            />
            <p class="text-white font-semibold">Procesando cierre...</p>
          </div>
        </div>

        <!-- Warning message -->
        <div class="glass-card p-4 mb-6 bg-yellow-500/10 border-yellow-500/30">
          <div class="flex items-center space-x-3">
            <i class="pi pi-exclamation-triangle text-yellow-400 text-2xl"></i>
            <div>
              <p class="text-white font-semibold">⚠️ Atención</p>
              <p class="text-gray-300 text-sm">Esta acción no se puede deshacer. Se cerrará la caja actual y se creará una nueva.</p>
            </div>
          </div>
        </div>

        <!-- Initial amount input -->
        <div class="mb-6">
          <label for="montoInicial" class="block text-white font-semibold mb-3">
            💰 Monto inicial de la nueva caja:
          </label>
          <div class="relative">
            <span class="absolute left-4 top-1/2 transform -translate-y-1/2 text-gray-400 text-lg">$</span>
            <input
              type="number"
              v-model="montoInicial"
              id="montoInicial"
              class="w-full glass-input pl-10 pr-4 py-3 text-white text-lg font-semibold"
              placeholder="0.00"
              step="0.01"
              min="0"
              @keypress.enter="confirmCerrarCaja"
            />
          </div>
          <p class="text-gray-400 text-sm mt-2">
            Este será el monto inicial para la próxima caja
          </p>
        </div>

        <!-- Summary card -->
        <div class="glass-card p-4 mb-6">
          <h4 class="text-white font-semibold mb-2">📊 Resumen del cierre:</h4>
          <div class="space-y-2 text-sm">
            <div class="flex justify-between">
              <span class="text-gray-300">Fecha y hora:</span>
              <span class="text-white font-semibold">{{ getCurrentDateTime() }}</span>
            </div>
            <div class="flex justify-between">
              <span class="text-gray-300">Usuario:</span>
              <span class="text-white font-semibold">{{ authStore.user?.userName || 'Usuario' }}</span>
            </div>
            <div class="flex justify-between">
              <span class="text-gray-300">Institución:</span>
              <span class="text-white font-semibold">{{ authStore.user?.institucionName || 'Hotel' }}</span>
            </div>
          </div>
        </div>
      </div>

      <!-- Footer with actions -->
      <div class="glass-header p-6 border-t border-white/20">
        <div class="flex justify-end space-x-3">
          <button 
            @click="closeModal" 
            class="glass-button px-6 py-3 text-white font-semibold hover:bg-white/20 
                   transform hover:scale-105 transition-all"
          >
            <i class="pi pi-times mr-2"></i>
            Cancelar
          </button>
          <button 
            @click.prevent="confirmCerrarCaja"  
            :disabled="isLoading" 
            class="bg-gradient-to-r from-red-500 to-red-600 hover:from-red-600 hover:to-red-700 
                   text-white font-bold py-3 px-6 rounded-lg transform transition-all
                   shadow-lg hover:shadow-red-500/50 disabled:opacity-50 disabled:cursor-not-allowed
                   hover:scale-105 flex items-center"
            :class="{ 'animate-pulse': isLoading }"
          >
            <i class="pi pi-check mr-2"></i>
            {{ isLoading ? 'Cerrando...' : 'Confirmar Cierre' }}
          </button>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue';
import axiosClient from '../axiosClient';
import ProgressSpinner from 'primevue/progressspinner';
import { useAuthStore } from '../store/auth';
import { useToast } from 'primevue/usetoast';

// Composables
const authStore = useAuthStore();
const toast = useToast();

// Emit events
const emit = defineEmits(['close', 'refresh']);

// State
const montoInicial = ref(0);
const isLoading = ref(false);
const isClosing = ref(false);
const InstitucionID = ref(null);

// Lifecycle
onMounted(() => {
  getDatosLogin();
});

// Methods
const getDatosLogin = () => {
  InstitucionID.value = authStore.institucionID;
};

const getCurrentDateTime = () => {
  return new Date().toLocaleString('es-ES', {
    year: 'numeric',
    month: 'long',
    day: 'numeric',
    hour: '2-digit',
    minute: '2-digit'
  });
};

const closeModal = () => {
  isClosing.value = true;
  setTimeout(() => {
    emit('close');
  }, 300);
};

const showSuccess = (message) => {
  toast.add({
    severity: 'success',
    summary: 'Éxito',
    detail: message,
    life: 5000
  });
};

const showError = (message) => {
  toast.add({
    severity: 'error',
    summary: 'Error',
    detail: message,
    life: 5000
  });
};

const confirmCerrarCaja = async () => {
  if (isLoading.value) return;
  
  // Validate initial amount
  if (montoInicial.value < 0) {
    showError('El monto inicial no puede ser negativo');
    return;
  }

  isLoading.value = true;
  
  try {
    const response = await axiosClient.post(
      `/api/Caja/CierreCaja?montoInicial=${montoInicial.value}&InstitucionID=${InstitucionID.value}&observacion=Cierre de caja`
    );
    
    if (response.data.ok) {
      showSuccess('✅ Caja cerrada exitosamente');
      setTimeout(() => {
        emit('refresh');
      }, 1500);
    } else {
      showError(`❌ Error al cerrar caja: ${response.data.message}`);
    }
  } catch (error) {
    console.error('Error al cerrar caja:', error);
    showError('❌ Ocurrió un error al cerrar caja. Por favor, intente nuevamente.');
  } finally {
    isLoading.value = false;
  }
};
</script>

<style scoped>
/* Glassmorphism components */
.glass-container {
  @apply bg-white/10 backdrop-blur-2xl border border-white/20 rounded-3xl shadow-2xl;
}

.glass-header {
  @apply bg-white/5 backdrop-blur-md;
}

.glass-card {
  @apply bg-white/10 backdrop-blur-md border border-white/20 rounded-xl p-4;
}

.glass-button {
  @apply bg-white/10 backdrop-blur-sm border border-white/30 rounded-lg transition-all;
}

.glass-input {
  @apply bg-white/10 backdrop-blur-sm border border-white/30 rounded-lg placeholder-gray-400;
  @apply focus:ring-2 focus:ring-white/50 focus:border-white/50 focus:outline-none;
}

/* Remove spinner arrows from number input */
input[type="number"]::-webkit-inner-spin-button,
input[type="number"]::-webkit-outer-spin-button {
  -webkit-appearance: none;
  margin: 0;
}

input[type="number"] {
  -moz-appearance: textfield;
}

/* Animations */
@keyframes pulse {
  0%, 100% {
    opacity: 1;
  }
  50% {
    opacity: 0.5;
  }
}

.animate-pulse {
  animation: pulse 2s cubic-bezier(0.4, 0, 0.6, 1) infinite;
}
</style>