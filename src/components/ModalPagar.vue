<template>
  <Teleport to="body" class="overflow-hidden">
    <div class="fixed inset-0 bg-black bg-opacity-80 flex justify-center items-center">
      <div class="relative bg-white rounded-lg p-8 w-1/3 h-auto">
        <button @click="emit('close')" class="absolute top-2 right-2 btn-danger p-4 rounded-md">X</button>

        <h2 class="text-xl font-bold ">Detalles de Pago</h2>
        <table class=" w-full  text-left">
          <tbody>
            <tr>
              <td class="p-1 font-semibold">Periodo</td>
              <td class="p-1 text-right">${{ periodo }}</td>
            </tr>
            <tr>
              <td class="p-1 font-semibold">Consumision</td>
              <td class="p-1 text-right text-teal-400">${{ consumo }}</td>
            </tr>
            <tr>
              <td class="p-1 font-semibold">Adicional</td>
              <td class="p-1 text-right">${{ adicional.toFixed(2) }}</td>
            </tr>
            <tr v-if="recargoMonto > 0">
              <td class="p-1 font-semibold">Recargo</td>
              <td class="p-1 text-right text-red-500">${{ recargoMonto.toFixed(2) }}</td>
            </tr>
            <tr>
              <td class="p-1 font-semibold">Descuento</td>
              <td class="p-1 text-right">
                <input
                  type="number"
                  class="border rounded p-1 w-full"
                  v-model.number="descuento"
                  placeholder="0.00"
                />
              </td>
            </tr>
            <tr>
              <td class="p-1 font-semibold">Efectivo</td>
              <td class="p-1 text-right">
                <input type="number" class="border rounded p-1 w-full" v-model.number="efectivo"
                  placeholder="0.00" />
              </td>
            </tr>
            <tr>
              <td class="p-1 font-semibold">Tarjeta</td>
              <td class="p-1 text-right">
                <input
                  type="number"
                  class="border rounded p-1 w-full"
                  v-model.number="tarjeta"
                  placeholder="0.00"
                  :disabled="selectedTarjeta !== null"
                  :class="{ 'bg-gray-300': selectedTarjeta !== null }"
                />
              </td>
            </tr>
            <tr>
              <td class="p-1 font-semibold">Seleccionar Tarjeta</td>
              <td class="p-1 text-right">
                <select v-model="selectedTarjeta" class="border rounded p-1 w-full" @change="updateRecargo">
                  <option :value="null">Sin tarjeta seleccionada</option>
                  <option v-for="tarjeta in tarjetas" :key="tarjeta.tarjetaID" :value="tarjeta" 
                  >
                    {{ tarjeta.nombre }}
                  </option>
                </select>
              </td>
            </tr>

            <tr v-if="empenoMonto > 0">
              <td class="p-1 font-semibold">Empeño</td>
              <td class="p-1 text-right text-green-500">-${{ empenoMonto.toFixed(2) }}</td>
            </tr>
            <tr>
              <td class="p-1 font-semibold">Total</td>
              <td class="p-1 text-right text-red-500">${{ totalPago.toFixed(2) }}</td>
            </tr>
            <tr>
              <td class="p-1 font-semibold">Falta por pagar</td>
              <td class="p-1 text-right text-red-500">${{ faltaPorPagar.toFixed(2) }}</td>
            </tr>
            <tr>
              <td class="p-1 font-semibold">Comentario</td>
              <td class="p-1 text-right">
                <textarea class="border rounded p-1 w-full" :class="{ 'border-red-500': descuento > 0 && !comentario }"
                  v-model="comentario" placeholder="Escribe un comentario..."></textarea>
              </td>
            </tr>
          </tbody>
        </table>
        <div class="flex justify-end space-x-4">
          <button v-if="!props.pausa" @click.prevent="PausarTimer" class="btn-secondary p-2 rounded-md"><span
              class="material-symbols-outlined">
              pause
            </span>
            Pausar
          </button>
          <button v-if="props.pausa" @click.prevent="RecalcularTimer" class="btn-secondary p-2 rounded-md">
            Recalcular<span class="material-symbols-outlined">
              play_arrow
            </span>
          </button>

          <button @click.prevent="toggleEmpenoModal" class="btn-third p-2 rounded-md">Empeño</button>
          <button @click.prevent="toggleRecargoModal" class="btn-third p-2 rounded-md">Recargo</button>

        </div>
        <button :disabled="faltaPorPagar !== 0 || (descuento > 0 && !comentario)" @click.prevent="crearMovimientoAdicional"
          class="w-full mt-4 rounded-xl p-2" :class="!isButtonDisabled ? ' btn-primary' : 'btn-disabled'">
          Confirmar
        </button>
      </div>

      <RecargoModal v-if="showRecargoModal" @close="showRecargoModal = false" @confirm-recargo="confirmoRecargo" />
      <EmpenoModal v-if="showEmpenoModal" @close="showEmpenoModal = false" @confirm-empeno="confirmoEmpeno" />
    </div>
  </Teleport>
</template>

<script setup>
import { ref, computed, onMounted, watch } from 'vue';
import axiosClient from '../axiosClient';
import EmpenoModal from './EmpenoModal.vue';
import RecargoModal from './RecargoModal.vue';

// Define emits
const emit = defineEmits(['close', 'confirm-payment']);

const props = defineProps({
  periodo: { type: Number, required: true },
  consumo: { type: Number, required: true },
  total: {type: Number, required: true},
  adicional: { type: Number, required: true },
  visitaId: { type: Number, required: true },
  habitacionId: { type: Number, required: true },
  pausa: { type: Boolean, required: true },
});

const descuento = ref(0);
const efectivo = ref(0);
const tarjeta = ref(0);
//const mercadoPago = ref(0); REMOVED
const empenoMonto = ref(0);
const empenoDetalle = ref('');
const recargoMonto = ref(0);
const recargoDetalle = ref('');
const comentario = ref('');
const totalPago = ref(0);
const tarjetas = ref([]);
const selectedTarjeta = ref(null);
const showEmpenoModal = ref(false);
const showRecargoModal = ref(false);
const isButtonDisabled = ref(false);
const extraTarjeta = ref(0);
const porcentajeRecargo = ref(0);

// Computed property to calculate the total correctly
const calculatedTotal = computed(() => {
  return props.periodo + props.consumo + props.adicional + extraTarjeta.value + recargoMonto.value;
});

// Watch the calculated total and update totalPago
watch(calculatedTotal, (newTotal) => {
  totalPago.value = Number(newTotal || 0);
});


const faltaPorPagar = computed(() => {
  const descuentoValue = descuento.value || 0;
  const efectivoValue = efectivo.value || 0;
  const tarjetaValue = tarjeta.value || 0;
  const empenoMontoValue = empenoMonto.value || 0;

  return (
    props.periodo +
    props.consumo +
    props.adicional -
    (descuentoValue + efectivoValue + tarjetaValue + empenoMontoValue - extraTarjeta.value) + recargoMonto.value
  );
});


const fetchTarjetas = async () => {
  try {
    const response = await axiosClient.get(`/GetTarjetas?InstitucionID=${InstitucionID.value}`);
    tarjetas.value = [...response.data.data];
  } catch (error) {
    console.error('Error fetching tarjetas:', error);
  }
};


const calculoInicial = async () => {
  totalPago.value = props.periodo + props.consumo + props.adicional + recargoMonto.value;
};
onMounted(getDatosLogin);
onMounted(fetchTarjetas);
onMounted(calculoInicial);

const updateRecargo = () => {
  const subtotal = props.periodo + props.consumo + props.adicional + recargoMonto.value - descuento.value - efectivo.value - empenoMonto.value;

  if (selectedTarjeta.value) {
    porcentajeRecargo.value = selectedTarjeta.value.montoPorcentual;
    const newTarjetaValue = Number((subtotal * (1 + (selectedTarjeta.value.montoPorcentual / 100))).toFixed(2));
    extraTarjeta.value = Number((newTarjetaValue - subtotal).toFixed(2));
    tarjeta.value = newTarjetaValue;
  } else {
    tarjeta.value = 0;
    extraTarjeta.value = 0;
    porcentajeRecargo.value = 0;
  }
};

watch(
  () => selectedTarjeta.value,
  (newVal, oldVal) => {
    if (newVal !== oldVal) {
      updateRecargo();
    }
  },
  { immediate: true } // This is important to run the watcher on component mount
);

const toggleEmpenoModal = () => {
  showEmpenoModal.value = !showEmpenoModal.value;
};

const toggleRecargoModal = () => {
  showRecargoModal.value = true;
};

const confirmoEmpeno = ({ monto, detalle }) => {
  empenoMonto.value = monto;
  empenoDetalle.value = detalle;
  comentario.value = `Empeño de ${detalle} por un valor de $${monto.toFixed(2)}. ` + comentario.value;
  showEmpenoModal.value = false;
};

const confirmoRecargo = (recargo) => {
  recargoMonto.value = recargo.monto;
  recargoDetalle.value = recargo.detalle;
  comentario.value += `Recargo por ${recargo.detalle} con un valor de $${recargo.monto.toFixed(2)}. `;
  showRecargoModal.value = false;

};

const crearMovimientoAdicional = async () => {
  if (descuento.value > 0 && !comentario.value) {
    console.log("Test");
    alert('El comentario es obligatorio cuando se aplica un descuento.');
    isButtonDisabled.value = false;
    return;
  }
  if (isButtonDisabled.value) return;

  isButtonDisabled.value = true; // Deshabilitar el botón

  if (empenoMonto.value > 0) {
    try {
      await axiosClient.post(
        `api/Empeño/AddEmpeno?institucionID=${InstitucionID.value}&visitaID=${props.visitaId}&detalle=${empenoDetalle.value}&monto=${empenoMonto.value}`
      );
    } catch (error) {
      console.error('Error al crear el empeño:', error);
      isButtonDisabled.value = false;
      return;
    }
  }

  try {
    await axiosClient.post(
      `/MovimientoHabitacion?totalFacturado=${props.adicional}&habitacionId=${props.habitacionId}&visitaId=${props.visitaId}&comentario=${comentario.value}`
    );
    pagarVisita();
  } catch (error) {
    console.error('Error al agregar movimiento:', error);
    isButtonDisabled.value = false;
  }
};

const pagarVisita = async () => {
  try {
    // Helper function to safely convert to number, handling empty strings
    const safeNumber = (value) => {
      if (value === '' || value === null || value === undefined) return 0;
      const num = Number(value);
      return isNaN(num) ? 0 : num;
    };

    // Ensure all values have proper defaults to avoid empty parameters
    const data = {
      visitaId: props.visitaId,
      montoDescuento: safeNumber(descuento.value),
      montoEfectivo: safeNumber(efectivo.value),
      montoTarjeta: safeNumber(tarjeta.value),
      montoBillVirt: 0, //Hardcoded to 0
      adicional: safeNumber(props.adicional),
      medioPagoId: 1,
      comentario: encodeURIComponent(comentario.value || ''),
      montoRecargo: safeNumber(recargoMonto.value),
      descripcionRecargo: encodeURIComponent(recargoDetalle.value || ''),
    };
    
    const tarjetaSeleccionada = selectedTarjeta.value?.tarjetaID || 0;
    
    const url = data.montoRecargo > 0
      ? `/api/Pago/PagarVisita?visitaId=${data.visitaId}&montoDescuento=${data.montoDescuento}&montoEfectivo=${data.montoEfectivo}&montoTarjeta=${data.montoTarjeta}&montoBillVirt=${data.montoBillVirt}&adicional=${data.adicional}&medioPagoId=${data.medioPagoId}&comentario=${data.comentario}&montoRecargo=${data.montoRecargo}&descripcionRecargo=${data.descripcionRecargo}&tarjetaID=${tarjetaSeleccionada}`
      : `/api/Pago/PagarVisita?visitaId=${data.visitaId}&montoDescuento=${data.montoDescuento}&montoEfectivo=${data.montoEfectivo}&montoTarjeta=${data.montoTarjeta}&montoBillVirt=${data.montoBillVirt}&adicional=${data.adicional}&medioPagoId=${data.medioPagoId}&comentario=${data.comentario}&tarjetaID=${tarjetaSeleccionada}`;

    console.log('Payment URL:', url); // Debug log
    await axiosClient.post(url);
    finalizarReserva(props.habitacionId);
  } catch (error) {
    console.error('Error al realizar el pago:', error);
    console.error('Error response:', error.response?.data); // More detailed error info
    isButtonDisabled.value = false;
  }
};

const finalizarReserva = async (idHabitacion) => {
  try {
    await axiosClient.put(`/FinalizarReserva?idHabitacion=${idHabitacion}`);
    emit('confirm-payment', { habitacionId: idHabitacion });
  } catch (error) {
    console.error('Error al finalizar reserva:', error);
    isButtonDisabled.value = false;
  }
}


const PausarTimer = async () => {
  try {
    await axiosClient.put(`/PausarOcupacion?visitaId=${props.visitaId}`);
    emit('confirm-payment', { paused: true });
  } catch (error) {
    console.error('Error al pausar la reserva:', error);
  }
}

const RecalcularTimer = async () => {
  try {
    await axiosClient.put(`/RecalcularOcupacion?visitaId=${props.visitaId}`);
    emit('confirm-payment', { recalculated: true });
  } catch (error) {
    console.error('Error al reanudar la reserva:', error);
  }
};

watch(
  () => [descuento.value, comentario.value],
  () => {
    isButtonDisabled.value = false;
  }
);

import { useAuthStore } from '../store/auth.js'; // Import the auth store

const authStore = useAuthStore();
const InstitucionID = ref(null)
function getDatosLogin(){
    InstitucionID.value = authStore.institucionID;
  }

</script>

<style>
/* Add your styles here */
</style>
