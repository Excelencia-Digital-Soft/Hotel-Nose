<template>
  <!-- Modal container -->
  <div class="fixed inset-0 bg-gray-800 bg-opacity-50 flex justify-center items-center" @click.self="closeModal">
    <!-- Modal content -->
    <div class="bg-white w-11/12 h-5/6 rounded-lg overflow-y-auto p-6 relative">
      <!-- Title of the modal -->
      <h2 class="text-2xl text-black font-semibold mb-4">Lista de Pagos</h2>

      <!-- "Cerrar" Button -->
      <button @click="closeModal" class="absolute top-4 right-4 bg-gray-300 hover:bg-gray-400 text-black px-4 py-2 rounded-full">
        Cerrar
      </button>

      <!-- Grid Layout for Pagos -->
      <div class="grid grid-cols-[repeat(14,minmax(0,1fr))] gap-0 border-t border-l border-gray-300">
        <!-- Headers -->
        <div class="font-bold col-span-2 text-black p-2 border-b border-r border-gray-300">Tipo Habitacion</div>
        <div class="font-bold text-black p-2 border-b border-r border-gray-300">Pago</div>
        <div class="font-bold text-black p-2 border-b border-r border-gray-300 text-right">Periodo</div>
        <div class="font-bold text-black p-2 border-b border-r border-gray-300 text-right">Adicional</div>
        <div class="font-bold text-black p-2 border-b border-r border-gray-300 text-right">Consumo</div>

        <div class="font-bold text-black p-2 border-b border-r border-gray-300">Hora Ingreso</div>
        <div class="font-bold text-black p-2 border-b border-r border-gray-300">Hora Salida</div>

        <div class="font-bold text-black p-2 border-b border-r border-gray-300 text-right">Efectivo</div>
        <div class="font-bold text-black p-2 border-b border-r border-gray-300 text-right">Tarjeta</div>
        <div class="font-bold text-black p-2 border-b border-r border-gray-300 text-right">Billetera Virtual</div>
        <div class="font-bold text-black p-2 border-b border-r border-gray-300 text-right">Descuento</div>
        <div class="font-bold text-black p-2 border-b border-r border-gray-300 text-right">Total</div>
        <div class="font-bold text-black p-2 border-b border-r border-gray-300">Observacion</div>

        <!-- Iterate over Pagos -->
        <div v-for="(pago, index) in selectedPagos" :key="pago.pagoId" class="contents">
          <!-- Pago Info -->
          <div class="text-black col-span-2 font-semibold p-2 border-b border-r border-gray-300">{{ pago.tipoHabitacion}}</div>
          <div @click="openInfoModal(pago)" class="cursor-pointer text-blue-600 hover:underline p-2 border-b border-r border-gray-300">
            Pago {{ pago.pagoId }}
            <div class="text-gray-500 text-sm">{{ formatFechaHora(pago.fecha) }}</div>
          </div>
          <div class="text-green-600 font-semibold p-2 border-b border-r border-gray-300 text-right">{{ pago.montoEfectivo + pago.montoBillVirt + pago.montoTarjeta - pago.montoAdicional - pago.totalConsumo || '' }}</div>
          <div class="text-green-600 font-semibold p-2 border-b border-r border-gray-300 text-right">{{ pago.montoAdicional || '' }}</div>
          <div class="text-green-600 font-semibold p-2 border-b border-r border-gray-300 text-right">{{ pago.totalConsumo|| '' }}</div>
          <div class="text-black font-semibold p-2 border-b border-r border-gray-300">{{ formatFechaHora(pago.horaIngreso) }}</div>
          <div class="text-black font-semibold p-2 border-b border-r border-gray-300">{{ formatFechaHora(pago.horaSalida) }}</div>


          <!-- Money values with right alignment and hiding zeros -->
          <div class="text-green-600 font-semibold p-2 border-b border-r border-gray-300 text-right">{{ pago.montoEfectivo || '' }}</div>
          <div class="text-green-600 font-semibold p-2 border-b border-r border-gray-300 text-right">{{ pago.montoTarjeta || '' }}</div>
          <div class="text-green-600 font-semibold p-2 border-b border-r border-gray-300 text-right">{{ pago.montoBillVirt || '' }}</div>
          <div class="text-red-600 font-semibold p-2 border-b border-r border-gray-300 text-right">{{ pago.montoDescuento || '' }}</div>
          <div class="text-green-600 font-semibold p-2 border-b border-r border-gray-300 text-right">{{ (pago.montoEfectivo + pago.montoBillVirt + pago.montoTarjeta) || '' }}</div>

          <div class="text-black font-semibold p-2 border-b border-r border-gray-300">{{ pago.observacion }}</div>
        </div>

        <!-- Total values per column -->
        <div class="font-bold col-span-2 text-black p-2 border-t border-r border-gray-300">
  <div class="font-bold text-black p-2 text-left">Habitaciones</div>
  <div v-for="(item, index) in formatCategorias()" :key="index">
    {{ item.categoria }}: {{ item.count }}
  </div>
</div>
        <div class="font-bold text-black p-2 border-t border-r border-gray-300"></div>
        <div class="font-bold text-green-600 p-2 border-t border-r border-gray-300 text-left"><div class="font-bold text-black p-2 text-left">Periodo Total</div>{{ calculatePeriodo() }}</div>
        <div class="font-bold text-green-600 p-2 border-t border-r border-gray-300 text-right"><div class="font-bold text-black p-2 text-center">Adicional Total</div>{{ calculateAdicional() }}</div>
        <div class="font-bold text-green-600 p-2 border-t border-r border-gray-300 text-right"><div class="font-bold text-black p-2 text-center">Consumo Total</div>{{ calculateConsumo() }}</div>
        <div class="font-bold text-black p-2 border-t border-r border-gray-300"></div>
        <div class="font-bold text-black p-2 border-t border-r border-gray-300"></div>

        <div class="font-bold text-green-600 p-2 border-t border-r border-gray-300 text-right"><div class="font-bold text-black p-2 text-center">Efectivo Total</div>{{ calculateEfectivo() }}</div>
        <div class="font-bold text-green-600 p-2 border-t border-r border-gray-300 text-right"><div class="font-bold text-black p-2 text-center">Tarjeta Total</div>{{ calculateTarjeta() }}</div>
        <div class="font-bold text-green-600 p-2 border-t border-r border-gray-300 text-right"><div class="font-bold text-black p-2 text-center">MP Total</div>{{ calculateMP() }}</div>
        <div class="font-bold text-red-600 p-2 border-t border-r border-gray-300 text-right"><div class="font-bold text-black p-2 text-center">Desc.</div>{{ calculateDescuento() }}</div>
        <div class="font-bold text-green-600 p-2 border-t border-r border-gray-300 text-right"><div class="font-bold text-black p-2 text-center">Total</div>{{ calculateTotal() }}</div>
        <div class="font-bold text-black p-2 border-t border-r border-gray-300"></div>
      </div>

      <!-- "Cerrar Caja" Button (only shown if esAbierto is true) -->
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
  if (!fechaHora) return ''; // Return blank if null or undefined
  const date = new Date(fechaHora);
  return date.toLocaleString();
};

// Calculate total amount
const calculateTotal = () => {
  return props.selectedPagos.reduce((total, pago) => {
    return total + pago.montoEfectivo + pago.montoTarjeta + pago.montoBillVirt;
  }, 0);
};
const calculateEfectivo = () => {
  return props.selectedPagos.reduce((total, pago) => {
    return total + pago.montoEfectivo;
  }, 0);
};
const calculateDescuento = () => {
  return props.selectedPagos.reduce((total, pago) => {
    return total + pago.montoDescuento;
  }, 0);
};
const calculateTarjeta = () => {
  return props.selectedPagos.reduce((total, pago) => {
    return total + pago.montoTarjeta;
  }, 0);
};
const calculateConsumo = () => {
  return props.selectedPagos.reduce((total, pago) => {
    return total + (pago.totalConsumo || 0);
  }, 0);
};
const calculatePeriodo = () => {
  return props.selectedPagos.reduce((total, pago) => {
    return total + ( pago.montoEfectivo + pago.montoBillVirt + pago.montoTarjeta - pago.montoAdicional - pago.totalConsumo );
  }, 0);
};
const countCategorias = () => {
  return props.selectedPagos.reduce((counts, pago) => {
    if (pago.categoriaNombre) {
      counts[pago.categoriaNombre] = (counts[pago.categoriaNombre] || 0) + 1;
    }
    return counts;
  }, {});
};

const formatCategorias = () => {
  const categorias = countCategorias();
  return Object.entries(categorias).map(([categoria, count]) => ({ categoria, count }));
};

const calculateAdicional = () => {
  return props.selectedPagos.reduce((total, pago) => {
    return total + (pago.montoAdicional || 0);
  }, 0);
};
const calculateMP = () => {
  return props.selectedPagos.reduce((total, pago) => {
    return total + pago.montoBillVirt;
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
  