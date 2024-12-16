<template>
  <!-- Modal container -->
  <div class="fixed inset-0 bg-gray-800 bg-opacity-50 flex justify-center items-center" @click.self="closeModal">
    <!-- Modal content -->
    <div class="bg-white w-3/4 h-5/6 rounded-lg overflow-y-auto p-6 relative">
      <!-- Title of the modal -->
      <h2 class="text-2xl text-black font-semibold mb-4">Lista de Pagos</h2>

      <!-- "Cerrar" Button -->
      <button @click="closeModal" class="absolute top-4 right-4 bg-gray-300 hover:bg-gray-400 text-black px-4 py-2 rounded-full">
        Cerrar
      </button>

      <!-- Grid Layout for Pagos -->
      <div class="grid grid-cols-9 gap-0 border-t border-l border-gray-300">
        <!-- Headers -->
        <div class="font-bold text-black p-2 border-b border-r border-gray-300">Pago</div>
        <div class="font-bold text-black p-2 border-b border-r border-gray-300">Hora Ingreso</div>
        <div class="font-bold text-black p-2 border-b border-r border-gray-300">Hora Salida</div>
        <div class="font-bold text-black p-2 border-b border-r border-gray-300">Tipo Habitacion</div>
        <div class="font-bold text-black p-2 border-b border-r border-gray-300">Efectivo</div>
        <div class="font-bold text-black p-2 border-b border-r border-gray-300">Tarjeta</div>
        <div class="font-bold text-black p-2 border-b border-r border-gray-300">Billetera Virtual</div>
        <div class="font-bold text-black p-2 border-b border-r border-gray-300">Descuento</div>
        <div class="font-bold text-black p-2 border-b border-r border-gray-300">Observacion</div>


        <!-- Iterate over Pagos -->
        <div v-for="(pago, index) in selectedPagos" :key="pago.pagoId" class="contents">
          <!-- Pago Info -->
          <div @click="openInfoModal(pago)" class="cursor-pointer text-blue-600 hover:underline p-2 border-b border-r border-gray-300">
            Pago {{ pago.pagoId }}
            <div class="text-gray-500 text-sm">{{ formatFechaHora(pago.fecha) }}</div>
          </div>
          <div class="text-black font-semibold p-2 border-b border-r border-gray-300">{{ formatFechaHora(pago.horaIngreso) }}</div>
          <div class="text-black font-semibold p-2 border-b border-r border-gray-300">{{ formatFechaHora(pago.horaSalida) }}</div>
          <div class="text-black font-semibold p-2 border-b border-r border-gray-300">{{ pago.tipoHabitacion}}</div>

          <!-- Efectivo -->
          <div class="text-green-600 font-semibold p-2 border-b border-r border-gray-300">${{ pago.montoEfectivo }}</div>
          <!-- Tarjeta -->
          <div class="text-green-600 font-semibold p-2 border-b border-r border-gray-300">${{ pago.montoTarjeta }}</div>
          <!-- Billetera Virtual -->
          <div class="text-green-600 font-semibold p-2 border-b border-r border-gray-300">${{ pago.montoBillVirt }}</div>
          <!-- Descuento -->
          <div class="text-green-600 font-semibold p-2 border-b border-r border-gray-300">${{ pago.montoDescuento }}</div>
          <div class="text-black font-semibold p-2 border-b border-r border-gray-300">{{ pago.observacion }}</div>

        </div>
      </div>

      <!-- Total Section -->
      <div class="mt-4 font-semibold">
        <label class="text-black">Total:</label>
        <span class="text-green-600">${{ calculateTotal() }}</span>
      </div>

      <!-- "Cerrar Caja" Button (only shown if esAbierto is true) -->
  <!-- "Cerrar Caja" Button -->
  <div v-if="esAbierto" class="mt-4">
        <button @click="showCerrarCajaModal = true" class="w-full bg-red-600 text-black p-3 rounded-xl font-semibold hover:bg-red-700">
          Cerrar Caja
        </button>
      </div>
    </div>

    <!-- InfoPago Modal -->
    <InfoPago v-if="showInfoModal" :pago="selectedPago" @close="closeInfoModal" />

    <!-- CerrarCaja Modal -->
    <CerrarCajaModal v-if="showCerrarCajaModal" @close="closeCerrarCajaModal" @refresh="refreshPage" />

  </div>
</template>


<script setup>
import { ref, defineProps, defineEmits } from 'vue';
import InfoPago from './InfoPago.vue';
import CerrarCajaModal from './CerrarCajaModal.vue';

const props = defineProps({
  selectedPagos: Array,
  esAbierto: Boolean,
});

const emit = defineEmits(['close-modal']);

const closeModal = () => {
  emit('close-modal');
};

// State for the InfoPago modal
const showInfoModal = ref(false);
const selectedPago = ref(null);

// Open InfoPago modal with detailed information
const openInfoModal = (pago) => {
  selectedPago.value = pago;
  showInfoModal.value = true;
};

// Close InfoPago modal
const closeInfoModal = () => {
  showInfoModal.value = false;
  selectedPago.value = null;
};

const showCerrarCajaModal = ref(false);

const closeCerrarCajaModal = () => {
  showCerrarCajaModal.value = false;
};

// Refresh the page
const refreshPage = () => {
  window.location.reload();
};

// Format fechaHora (assuming it's in ISO format)
const formatFechaHora = (fechaHora) => {
  const date = new Date(fechaHora);
  return date.toLocaleString();
};

// Calculate total amount
const calculateTotal = () => {
  return props.selectedPagos.reduce((total, pago) => {
    return total + pago.montoEfectivo + pago.montoTarjeta + pago.montoBillVirt;
  }, 0);
};

const cerrarCaja = () => {
  // Logic to close the cash register
  console.log('Caja cerrada');
};
</script>

  
  <style scoped>
  /* Modal content styles */
  .bg-white {
    background-color: white;
  }
  
  .w- {
    width: 75%;
  }
  
  .h- {
    height: 87vh;
  }
  
  .overflow-y-auto {
    overflow-y: auto;
  }
 
  
  .text-gray-700 {
    color: #4a4a4a;
  }
  
  .border-b {
    border-bottom-width: 1px;
  }
  
  .border-gray-300 {
    border-color: #e0e0e0;
  }
  
  .grid-cols-5 {
  grid-template-columns: repeat(5, 1fr);
}

.cursor-pointer {
  cursor: pointer;
}
  .gap-4 {
    gap: 1rem;
  }
  
  .mt-4 {
    margin-top: 1rem;
  }
  
  .font-semibold {
    font-weight: 600;
  }
  
  
  .font-medium {
    font-weight: 500;
  }
  
  .p-3 {
    padding: 0.75rem;
  }
  
  .rounded-xl {
    border-radius: 1rem;
  }
  
  .bg-red-600 {
    background-color: #f56565;
  }
  
  
  .hover\:bg-red-700:hover {
    background-color: #c53030;
  }
  </style>
  