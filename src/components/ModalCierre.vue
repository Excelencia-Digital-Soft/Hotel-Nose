<template>
  <!-- Modal container -->
  <div class="fixed inset-0 bg-gray-800 bg-opacity-50 flex justify-center items-center" @click.self="closeModal">
    <!-- Modal content -->
    <div class="bg-white w-11/12 h-5/6 rounded-lg overflow-y-auto p-6 relative" ref="prueba" id="testCierre">
      <!-- Title of the modal -->
      <h2 class="text-2xl text-black font-semibold mb-4">Lista de Pagos</h2>

      <!-- "Cerrar" Button -->
      <button @click="closeModal"
        class="fixed top-12 right-12 btn-danger px-4 py-2 rounded-full">
        X
      </button>

      <!-- Grid Layout for Pagos -->
      <div ref="cierreCajaRef" id="cierre-caja-content"
        class="grid grid-cols-[repeat(14,minmax(0,1fr))] gap-0 border-t border-l border-gray-300">
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
        <div class="font-bold text-black p-2 border-b border-r border-gray-300 text-right">Tarjeta Usada</div>
        <div class="font-bold text-black p-2 border-b border-r border-gray-300 text-right">Descuento</div>
        <div class="font-bold text-black p-2 border-b border-r border-gray-300 text-right">Total</div>
        <div class="font-bold text-black p-2 border-b border-r border-gray-300">Observacion</div>

        <!-- Iterate over Pagos -->
        <template v-for="(pago, index) in listaPagos" :key="pago.pagoId">
          <div v-if="pago.tipoHabitacion !== null" class="contents" :class="{ 'bg-red-400': pago.pagoId === 0 }">
            <!-- Pago Info -->
            <div class="text-black col-span-2 font-semibold p-2 border-b border-r border-gray-300"
              :class="{ 'bg-red-400': pago.pagoId === 0 }">
              {{ pago.pagoId === 0 ? pago.tipoHabitacion + ' ANULADA' : pago.tipoHabitacion }}</div>
            <div @click="openInfoModal(pago)"
              class="cursor-pointer text-blue-600 hover:underline p-2 border-b border-r border-gray-300">
              Pago {{ pago.pagoId }}
              <div class="text-gray-500 text-sm ">{{ formatFechaHora(pago.fecha) }}</div>
            </div>
            <div class="text-green-600 font-semibold p-2 border-b border-r border-gray-300 text-right">{{ pago.periodo }}
            </div>
            <div class="text-green-600 font-semibold p-2 border-b border-r border-gray-300 text-right">{{
              pago.montoAdicional || '' }}</div>
            <div class="text-green-600 font-semibold p-2 border-b border-r border-gray-300 text-right">{{
              pago.totalConsumo || '' }}</div>
            <div class="text-black font-semibold p-2 border-b border-r border-gray-300">{{
              formatFechaHora(pago.horaIngreso) }}</div>
            <div class="text-black font-semibold p-2 border-b border-r border-gray-300">{{
              formatFechaHora(pago.horaSalida) }}</div>


            <!-- Money values with right alignment and hiding zeros -->
            <div class="text-green-600 font-semibold p-2 border-b border-r border-gray-300 text-right">{{
              pago.montoEfectivo || '' }}</div>
            <div class="text-green-600 font-semibold p-2 border-b border-r border-gray-300 text-right">{{
              pago.montoTarjeta || '' }}</div>
            <div class="text-green-600 font-semibold p-2 border-b border-r border-gray-300 text-right">{{
              pago.tarjetaNombre || '' }}</div>
            <div class="text-red-600 font-semibold p-2 border-b border-r border-gray-300 text-right">{{
              pago.montoDescuento || '' }}</div>
            <div class="text-green-600 font-semibold p-2 border-b border-r border-gray-300 text-right">{{
              (pago.montoEfectivo + pago.montoTarjeta) || '' }}</div>

            <div class="text-black font-semibold p-2 border-b border-r border-gray-300">{{ pago.observacion }}</div>
          </div>
        </template>

        <!-- Iterate over Egresos -->
        <template v-for="(egreso, index) in selectedEgresos" :key="`egreso-${index}`">
          <div class="contents text-red-500">
            <!-- Egreso Info -->
            <div class="text-red-500 col-span-2 font-semibold p-2 border-b border-r border-gray-300">Egreso</div>
            <div class="text-red-500 p-2 border-b border-r border-gray-300">Egreso {{ index + 1 }}
              <div class="text-gray-500 text-sm">{{ formatFechaHora(egreso.fecha) }}</div>
            </div>
            <div class="text-red-500 font-semibold p-2 border-b border-r border-gray-300 text-right"></div>
            <div class="text-red-500 font-semibold p-2 border-b border-r border-gray-300 text-right"></div>
            <div class="text-red-500 font-semibold p-2 border-b border-r border-gray-300 text-right"></div>
            <div class="text-red-500 font-semibold p-2 border-b border-r border-gray-300"></div>
            <div class="text-red-500 font-semibold p-2 border-b border-r border-gray-300"></div>

            <!-- Money values with right alignment and hiding zeros -->
            <div class="text-red-500 font-semibold p-2 border-b border-r border-gray-300 text-right">{{
              egreso.montoEfectivo || '' }}</div>
            <div class="text-red-500 font-semibold p-2 border-b border-r border-gray-300 text-right">{{
              egreso.montoTarjeta || '' }}</div>
            <div class="text-red-500 font-semibold p-2 border-b border-r border-gray-300 text-right"></div>
            <div class="text-red-500 font-semibold p-2 border-b border-r border-gray-300 text-right">{{
              egreso.montoDescuento || '' }}</div>
            <div class="text-red-500 font-semibold p-2 border-b border-r border-gray-300 text-right">{{
              (egreso.montoEfectivo + egreso.montoTarjeta) || '' }}</div>

            <div class="text-black font-semibold p-2 border-b border-r border-gray-300">{{ egreso.observacion }}</div>
          </div>
        </template>

        <!-- Total values per column -->
        <div class="font-bold col-span-2 text-black p-2 border-t border-r border-gray-300">
          <div class="font-bold text-black p-2 text-left">Habitaciones</div>
          <div v-for="(item, index) in formatCategorias()" :key="index">
            {{ item.categoria }}: {{ item.count }}
          </div>
        </div>
        <div class="font-bold text-black p-2 border-t border-r border-gray-300"></div>
        <div class="font-bold text-green-600 p-2 border-t border-r border-gray-300 text-left">
          <div class="font-bold text-black p-2 text-left">Periodo Total</div>{{ calculatePeriodo() }}
        </div>
        <div class="font-bold text-green-600 p-2 border-t border-r border-gray-300 text-right">
          <div class="font-bold text-black p-2 text-center">Adicional Total</div>{{ calculateAdicional() }}
        </div>
        <div class="font-bold text-green-600 p-2 border-t border-r border-gray-300 text-right">
          <div class="font-bold text-black p-2 text-center">Consumo Total</div>{{ calculateConsumo() }}
        </div>
        <div class="font-bold text-black p-2 border-t border-r border-gray-300"></div>
        <div class="font-bold text-black p-2 border-t border-r border-gray-300"></div>

        <div class="font-bold text-green-600 p-2 border-t border-r border-gray-300 text-right">
          <div class="font-bold text-black p-2 text-center">Efectivo Total</div>{{ calculateEfectivo() }}
        </div>
        <div class="font-bold text-green-600 p-2 border-t border-r border-gray-300 text-right">
          <div class="font-bold text-black p-2 text-center">Tarjeta Total</div>{{ calculateTarjeta() }}
        </div>
        <div class="font-bold text-green-600 p-2 border-t border-r border-gray-300 text-right">
          <div class="font-bold text-black p-2 text-center"> </div>
        </div>
        <div class="font-bold text-red-600 p-2 border-t border-r border-gray-300 text-right">
          <div class="font-bold text-black p-2 text-center">Desc.</div>{{ calculateDescuento().toFixed(2) }}
        </div>
        <div class="font-bold text-green-600 p-2 border-t border-r border-gray-300 text-right">
          <div class="font-bold text-black p-2 text-center">Total</div>{{ calculateTotal().toFixed(2) }}
        </div>
        <div class="font-bold text-black p-2 border-t border-r border-gray-300"></div>
      </div>

      <!-- "Cerrar Caja" Button (only shown if esAbierto is true) -->
      <div v-if="esAbierto" class="mt-4">
        <button @click="showCerrarCajaModal = true"
          class="w-full bg-red-600 text-black p-3 rounded-xl font-semibold hover:bg-red-700">
          Cerrar Caja
        </button>
      </div>
      <div>
        <!-- Imprimir Button -->
        <button @click="imprimirModal"
          class="w-full mt-2 btn-secondary text-black p-3 rounded-xl font-semibold hover:bg-gray-200">
          Imprimir
        </button>
      </div>
    </div>

    <!-- InfoPago Modal -->
    <InfoPago v-if="showInfoModal" :pago="selectedPago" @close="closeInfoModal" />

    <!-- CerrarCaja Modal -->
    <CerrarCajaModal v-if="showCerrarCajaModal" @close="closeCerrarCajaModal" @refresh="refreshPage" />
  </div>


  <!-- Hidden List for Printing -->
  <div id="hidden-print-content" class="hidden">
    <h1>Lista de Pagos</h1>
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
      <div class="font-bold text-black p-2 border-b border-r border-gray-300 text-right">Tarjeta Usada</div>
      <div class="font-bold text-black p-2 border-b border-r border-gray-300 text-right">Descuento</div>
      <div class="font-bold text-black p-2 border-b border-r border-gray-300 text-right">Total</div>
      <div class="font-bold text-black p-2 border-b border-r border-gray-300">Observacion</div>

      <!-- Iterate over Pagos -->
      <div v-for="(pago, index) in listaPagos" :key="pago.pagoId" class="contents bg-red-400">
        <!-- Pago Info -->
        <div class="text-black col-span-2 font-semibold p-2 border-b border-r border-gray-300">{{ pago.tipoHabitacion }}
        </div>
        <div class="text-blue-600 p-2 border-b border-r border-gray-300">
          Pago {{ pago.pagoId }}
          <div class="text-gray-500 text-sm">{{ formatFechaHora(pago.fecha) }}</div>
        </div>
        <div class="text-green-600 font-semibold p-2 border-b border-r border-gray-300 text-right">{{ pago.periodo }}
        </div>
        <div class="text-green-600 font-semibold p-2 border-b border-r border-gray-300 text-right">{{
          pago.montoAdicional || '' }}</div>
        <div class="text-green-600 font-semibold p-2 border-b border-r border-gray-300 text-right">{{
          pago.totalConsumo || '' }}</div>
        <div class="text-black font-semibold p-2 border-b border-r border-gray-300">{{ formatFechaHora(pago.horaIngreso)
          }}</div>
        <div class="text-black font-semibold p-2 border-b border-r border-gray-300">{{ formatFechaHora(pago.horaSalida)
          }}</div>


        <!-- Money values with right alignment and hiding zeros -->
        <div class="text-green-600 font-semibold p-2 border-b border-r border-gray-300 text-right">{{ pago.montoEfectivo
          || '' }}</div>
        <div class="text-green-600 font-semibold p-2 border-b border-r border-gray-300 text-right">{{ pago.montoTarjeta
          || '' }}</div>
        <div class="text-green-600 font-semibold p-2 border-b border-r border-gray-300 text-right">{{ pago.tarjetaNombre
          || '' }}</div>
        <div class="text-red-600 font-semibold p-2 border-b border-r border-gray-300 text-right">{{ pago.montoDescuento
          || '' }}</div>
        <div class="text-green-600 font-semibold p-2 border-b border-r border-gray-300 text-right">{{
          (pago.montoEfectivo + pago.montoBillVirt + pago.montoTarjeta) || '' }}</div>

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
      <div class="font-bold text-green-600 p-2 border-t border-r border-gray-300 text-left">
        <div class="font-bold text-black p-2 text-left">Periodo Total</div>{{ calculatePeriodo() }}
      </div>
      <div class="font-bold text-green-600 p-2 border-t border-r border-gray-300 text-right">
        <div class="font-bold text-black p-2 text-center">Adicional Total</div>{{ calculateAdicional().toFixed(2) }}
      </div>
      <div class="font-bold text-green-600 p-2 border-t border-r border-gray-300 text-right">
        <div class="font-bold text-black p-2 text-center">Consumo Total</div>{{ calculateConsumo() }}
      </div>
      <div class="font-bold text-black p-2 border-t border-r border-gray-300"></div>
      <div class="font-bold text-black p-2 border-t border-r border-gray-300"></div>

      <div class="font-bold text-green-600 p-2 border-t border-r border-gray-300 text-right">
        <div class="font-bold text-black p-2 text-center">Efectivo Total</div>{{ calculateEfectivo() }}
      </div>
      <div class="font-bold text-green-600 p-2 border-t border-r border-gray-300 text-right">
        <div class="font-bold text-black p-2 text-center">Tarjeta Total</div>{{ calculateTarjeta() }}
      </div>
      <div class="font-bold text-green-600 p-2 border-t border-r border-gray-300 text-right">
        <div class="font-bold text-black p-2 text-center"></div>
      </div>
      <div class="font-bold text-red-600 p-2 border-t border-r border-gray-300 text-right">
        <div class="font-bold text-black p-2 text-center">Desc.</div>{{ calculateDescuento().toFixed(2) }}
      </div>
      <div class="font-bold text-green-600 p-2 border-t border-r border-gray-300 text-right">
        <div class="font-bold text-black p-2 text-center">Total</div>{{ calculateTotal().toFixed(2) }}
      </div>
      <div class="font-bold text-black p-2 border-t border-r border-gray-300"></div>
    </div>
  </div>
</template>

<script setup>
import { ref, defineProps, defineEmits, onMounted, onBeforeMount } from 'vue';
import InfoPago from './InfoPago.vue';
import axiosClient from '../axiosClient';
import CerrarCajaModal from './CerrarCajaModal.vue';
const props = defineProps({
  selectedPagos: Array,
  selectedEgresos: Array,
  esAbierto: Boolean,
  idcierre: Number
});

const emit = defineEmits(['close-modal', 'imprimir-modal']);

const closeModal = () => {
  emit('close-modal');
};

const imprimirModal = () => {
  if (cierreCajaRef.value) {
    emit('imprimir-modal', cierreCajaRef.value);
  }
}
const cierreCajaRef = ref(null);
const showInfoModal = ref(false);
const selectedPago = ref(null);
const showCerrarCajaModal = ref(false);
let listaPagos = ref([])

onBeforeMount(() => {
  if (props.selectedPagos.length > 0) {
    listaPagos.value = props.selectedPagos;
  }
});

onMounted(() => {
  if (props.selectedPagos.length > 0) {
    listaPagos.value = props.selectedPagos;
  }
  if (props.idcierre > 0) {
    fetchDetalleCierre()
  }
  // Add egresos to the list after fetching pagos or on initial load
  listaPagos.value = [...listaPagos.value, ...props.selectedEgresos];

})

const fetchDetalleCierre = () => {
  console.log(props.idcierre)
  axiosClient.get(`/api/Caja/GetDetalleCierre?idCierre=${props.idcierre}`)
    .then(({ data }) => {
      console.log(data.data.pagos)
      listaPagos.value = data.data.pagos
    })
    .catch(error => {
      console.error('Error al obtener detalle cierres:', error);
    });
}
const openInfoModal = (pago) => {
  selectedPago.value = pago;
  showInfoModal.value = true;
};

const closeInfoModal = () => {
  showInfoModal.value = false;
  selectedPago.value = null;
};

const closeCerrarCajaModal = () => {
  showCerrarCajaModal.value = false;
};

const refreshPage = () => {
  window.location.reload();
};

const formatFechaHora = (fechaHora) => {
  if (!fechaHora) return '';
  const date = new Date(fechaHora);
  return date.toLocaleString();
};

const calculateTotal = () => {
  return listaPagos.value.reduce((total, item) => {
    if (item.tipoHabitacion === null && item.pagoId === 0) {
      return total - (item.montoEfectivo); // Subtract if it's a specific anulada
    } else {
      return total + item.montoEfectivo + item.montoTarjeta + item.montoBillVirt; // Add otherwise
    }
  }, 0);
};

const calculateEfectivo = () => {
  return listaPagos.value.reduce((total, item) => {
    if (item.tipoHabitacion === null && item.pagoId === 0) {
      return total - item.montoEfectivo; // Subtract if it's a specific anulada
    } else {
      return total + item.montoEfectivo; // Add otherwise
    }
  }, 0);
};

const calculateDescuento = () => {
  return listaPagos.value.reduce((total, item) => {
    return total + item.montoDescuento;
  }, 0);
};

const calculateTarjeta = () => {
  return listaPagos.value.reduce((total, item) => {
    return total + item.montoTarjeta;
  }, 0);
};

const calculateConsumo = () => {
  return listaPagos.value.reduce((total, item) => {
    return total + (item.totalConsumo || 0);
  }, 0);
};

const calculatePeriodo = () => {
  return listaPagos.value.reduce((total, item) => {
    return total + (item.periodo || 0);
  }, 0);
};

const countCategorias = () => {
  return listaPagos.value.reduce((counts, item) => {
    if (item.categoriaNombre) {
      counts[item.categoriaNombre] = (counts[item.categoriaNombre] || 0) + 1;
    }
    return counts;
  }, {});
};

const formatCategorias = () => {
  const categorias = countCategorias();
  return Object.entries(categorias).map(([categoria, count]) => ({ categoria, count }));
};

const calculateAdicional = () => {
  return listaPagos.value.reduce((total, item) => {
    return total + (item.montoAdicional || 0);
  }, 0);
};


</script>

<style scoped>
/* Modal content styles */


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

/* Style for the secondary button */
.btn-secondary {
  background-color: #e2e8f0;
  /* Example: Light gray */
  color: #000;
}

.btn-secondary:hover {
  background-color: #cbd5e1;
}
</style>

<style>
@media print {
  #cierre-caja-content {
    display: grid !important;
    grid-template-columns: repeat(14, minmax(0, 1fr)) !important;
    border: 1px solid black !important;
    font-size: 10px !important;
    width: 100% !important;
    height: auto !important;
  }

  #cierre-caja-content>div {
    border: 1px solid black !important;
    padding: 4px !important;
    color: black !important;
  }

  /* Hide interactive elements */
  button,
  .btn-secondary,
  .absolute {
    display: none !important;
  }

  @page {
    size: A4 landscape;
    margin: 10mm;
  }
}
</style>
