<template>
  <div class="fixed inset-0 bg-black bg-opacity-80 flex justify-center items-center">
    <div class="bg-white rounded-lg p-8 w-1/3">
      <h2 class="text-xl font-bold mb-4">Detalles de Pago</h2>
      <table class="w-full mb-4 text-left">
        <tbody>
          <tr>
            <td class="p-2 font-semibold">Total</td>
            <td class="p-2 text-right">${{ total.toFixed(2) }}</td>
          </tr>
          <tr>
            <td class="p-2 font-semibold">Adicional</td>
            <td class="p-2 text-right">${{ adicional.toFixed(2) }}</td>
          </tr>
          <tr>
            <td class="p-2 font-semibold">Descuento</td>
            <td class="p-2 text-right">
              <input
                type="number"
                class="border rounded p-1 w-full"
                v-model.number="descuento"
                @input="updateFalta"
                placeholder="0.00"
              />
            </td>
          </tr>
          <tr>
            <td class="p-2 font-semibold">Efectivo</td>
            <td class="p-2 text-right">
              <input
                type="number"
                class="border rounded p-1 w-full"
                v-model.number="efectivo"
                @input="updateFalta"
                placeholder="0.00"
              />
            </td>
          </tr>
          <tr>
            <td class="p-2 font-semibold">Tarjeta</td>
            <td class="p-2 text-right">
              <input
                type="number"
                class="border rounded p-1 w-full"
                v-model.number="tarjeta"
                @input="updateFalta"
                placeholder="0.00"
              />
            </td>
          </tr>
          <tr>
            <td class="p-2 font-semibold">MercadoPago</td>
            <td class="p-2 text-right">
              <input
                type="number"
                class="border rounded p-1 w-full"
                v-model.number="mercadoPago"
                @input="updateFalta"
                placeholder="0.00"
              />
            </td>
          </tr>
          <tr v-if="empenoMonto > 0">
            <td class="p-2 font-semibold">Empeño</td>
            <td class="p-2 text-right text-green-500">-${{ empenoMonto.toFixed(2) }}</td>
          </tr>
          <tr v-if="recargoMonto > 0">
            <td class="p-2 font-semibold">Recargo</td>
            <td class="p-2 text-right text-red-500">${{ recargoMonto.toFixed(2) }}</td>
          </tr>
          <tr>
            <td class="p-2 font-semibold">Total</td>
            <td class="p-2 text-right text-red-500">${{ (total + adicional).toFixed(2) }}</td>
          </tr>
          <tr>
            <td class="p-2 font-semibold">Falta por pagar</td>
            <td class="p-2 text-right text-red-500">${{ faltaPorPagar.toFixed(2) }}</td>
          </tr>
          <tr>
            <td class="p-2 font-semibold">Comentario</td>
            <td class="p-2 text-right">
              <textarea
                class="border rounded p-1 w-full"
                :class="{'border-red-500': descuento > 0 && !comentario}"
                v-model="comentario"
                placeholder="Escribe un comentario..."
              ></textarea>
            </td>
          </tr>
        </tbody>
      </table>
      <div class="flex justify-end space-x-4">
        <button @click="$emit('close')" class="btn-danger">Cancelar</button>
        <button @click.prevent="toggleEmpenoModal" class="btn-third">Empeño</button>
        <button @click.prevent="toggleRecargoModal" class="btn-third">Recargo</button>
        <button 
          :disabled="faltaPorPagar !== 0"
          @click.prevent="crearMovimientoAdicional"
          class="btn-primary">
          Confirmar
        </button>
      </div>
    </div>

    <RecargoModal
      v-if="showRecargoModal"
      @close="showRecargoModal = false"
      @confirm-recargo="confirmoRecargo"
    />
    <EmpenoModal
      v-if="showEmpenoModal"
      @close="showEmpenoModal = false"
      @confirm-empeno="confirmoEmpeno"
    />
  </div>
</template>

<script setup>
import { ref, computed } from 'vue';
import axiosClient from '../axiosClient';
import EmpenoModal from './EmpenoModal.vue';
import RecargoModal from './RecargoModal.vue';

const props = defineProps({
  total: { type: Number, required: true },
  adicional: { type: Number, required: true },
  visitaId: { type: Number, required: true },
  habitacionId: { type: Number, required: true },
});

const descuento = ref(0);
const efectivo = ref(0);
const tarjeta = ref(0);
const mercadoPago = ref(0);
const empenoMonto = ref(0);
const empenoDetalle = ref('');
const recargoMonto = ref(0);
const recargoDetalle = ref('');
const comentario = ref('');
const showEmpenoModal = ref(false);
const showRecargoModal = ref(false);
const isButtonDisabled = ref(false);
const faltaPorPagar = computed(() => {
  return (
    props.total +
    props.adicional -
    (descuento.value + efectivo.value + tarjeta.value + mercadoPago.value + empenoMonto.value) +
    recargoMonto.value
  );
});

const updateFalta = () => {};

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
};

const confirmoRecargo = (recargo) => {
  recargoMonto.value = recargo.monto;
  recargoDetalle.value = recargo.detalle;
  comentario.value += `Recargo por ${recargo.detalle} con un valor de $${recargo.monto.toFixed(2)}. `;
};

const crearMovimientoAdicional = async () => {
  if (isButtonDisabled.value) return;

  isButtonDisabled.value = true; // Deshabilitar el botón
  if (descuento.value > 0 && !comentario.value) {
    alert('El comentario es obligatorio cuando se aplica un descuento.');
    return;
  }

  if (empenoMonto.value > 0) {
    try {
      await axiosClient.post(
        `api/Empeño/AddEmpeno?visitaID=${props.visitaId}&detalle=${empenoDetalle.value}&monto=${empenoMonto.value}`
      );
    } catch (error) {
      console.error('Error al crear el empeño:', error);
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
  }
};

const pagarVisita = async () => {
  try {
    const data = {
      visitaId: props.visitaId,
      montoDescuento: descuento.value,
      montoEfectivo: efectivo.value,
      montoTarjeta: tarjeta.value,
      montoBillVirt: mercadoPago.value,
      medioPagoId: 1,
      comentario: comentario.value,
      montoRecargo: recargoMonto.value,
      descripcionRecargo: recargoDetalle.value,
    };

    const url = recargoMonto.value > 0
      ? `/api/Pago/PagarVisita?visitaId=${data.visitaId}&montoDescuento=${data.montoDescuento}&montoEfectivo=${data.montoEfectivo}&montoTarjeta=${data.montoTarjeta}&montoBillVirt=${data.montoBillVirt}&medioPagoId=${data.medioPagoId}&comentario=${data.comentario}&montoRecargo=${data.montoRecargo}&descripcionRecargo=${data.descripcionRecargo}`
      : `/api/Pago/PagarVisita?visitaId=${data.visitaId}&montoDescuento=${data.montoDescuento}&montoEfectivo=${data.montoEfectivo}&montoTarjeta=${data.montoTarjeta}&montoBillVirt=${data.montoBillVirt}&medioPagoId=${data.medioPagoId}&comentario=${data.comentario}`;

    await axiosClient.post(url);
    finalizarReserva(props.habitacionId);
  } catch (error) {
    console.error('Error al realizar el pago:', error);
  }
};

const finalizarReserva = async (idHabitacion) => {
  try {
    await axiosClient.put(`/FinalizarReserva?idHabitacion=${idHabitacion}`);
    window.location.reload();
  } catch (error) {
    console.error('Error al finalizar reserva:', error);
  }
};
</script>
  
  <style>
  /* Add your styles here */
  </style>
  