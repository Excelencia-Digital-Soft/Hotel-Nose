<template>
  <div class="fixed inset-0 bg-black bg-opacity-80 flex justify-center items-center">
    <div class="bg-white rounded-lg p-8 w-1/3">
      <h2 class="text-xl font-bold mb-4 text-black">Detalles de Pago</h2>
      <table class="w-full mb-4 text-left">
        <tbody>
          <tr>
            <td class="p-2 font-semibold text-black">Total</td>
            <td class="p-2 text-right text-black">${{ totalConAjustes.toFixed(2) }}</td>
          </tr>
          <tr>
            <td class="p-2 font-semibold text-black">Descuento</td>
            <td class="p-2 text-right text-black">
              <input type="number" class="border rounded p-1 w-full" v-model.number="descuento" placeholder="0.00" />
            </td>
          </tr>
          <tr>
            <td class="p-2 font-semibold text-black">Recargo</td>
            <td class="p-2 text-right text-black">
              <input type="number" class="border rounded p-1 w-full" v-model.number="recargo" placeholder="0.00" />
            </td>
          </tr>
          <tr>
            <td class="p-2 font-semibold text-black">Efectivo</td>
            <td class="p-2 text-right text-black">
              <input type="number" class="border rounded p-1 w-full" v-model.number="efectivo" placeholder="0.00" />
            </td>
          </tr>
          <tr>
            <td class="p-2 font-semibold text-black">Tarjeta</td>
            <td class="p-2 text-right text-black">
              <input type="number" class="border rounded p-1 w-full" v-model.number="tarjeta" placeholder="0.00" />
            </td>
          </tr>
          <tr>
              <td class="p-1 font-semibold text-black">Seleccionar Tarjeta</td>
              <td class="p-1 text-right text-black">
                <select v-model="selectedTarjeta" class="border rounded p-1 w-full" @change="updateRecargo">
                  <option :value="null">Sin tarjeta seleccionada</option>
                  <option v-for="tarjeta in tarjetas" :key="tarjeta.tarjetaID" :value="tarjeta" 
                  >
                    {{ tarjeta.nombre }}
                  </option>
                </select>
              </td>
            </tr>
          <tr>
            <td class="p-2 font-semibold text-black">Falta por pagar</td>
            <td class="p-2 text-right text-red-500">${{ faltaPorPagar.toFixed(2) }}</td>
          </tr>
          <tr>
            <td class="p-2 font-semibold text-black">Observación</td>
            <td class="p-2 text-right text-black">
              <textarea class="border rounded p-1 w-full" v-model="observacion"></textarea>
            </td>
          </tr>
        </tbody>
      </table>
      <div class="flex justify-end space-x-4">
        <button @click="$emit('close')" class="btn-danger">Cancelar</button>
        <button :disabled="faltaPorPagar !== 0" @click.prevent="pagarEmpeno" class="btn-primary">Confirmar</button>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, watch, onMounted } from 'vue';
import axiosClient from '../axiosClient';
const emit = defineEmits(["close"])
const props = defineProps({
  total: { type: Number, required: true },
  empenoId: { type: Number, required: true },
  visitaId: { type: Number, required: true },
});
const extraTarjeta = ref(0);
const efectivo = ref(0);
const tarjeta = ref(0);
const descuento = ref(0);
const recargo = ref(0);
const observacion = ref(`Pago de empeño correspondiente a la visita ${props.visitaId}`);
console.log(props);
const totalConAjustes = computed(() => props.total - descuento.value + recargo.value);
const faltaPorPagar = computed(() => totalConAjustes.value + extraTarjeta.value - (efectivo.value + tarjeta.value));
watch(() => faltaPorPagar, (newValue) => {
  console.log(totalConAjustes.value);
}, { deep: true });
const pagarEmpeno = async () => {
  try {
    if (descuento.value !== 0 || recargo.value !== 0) {
      await axiosClient.post(`/api/Empeño/ActualizarEmpeño`, {}, {
        params: {
          empeñoID: props.empenoId,
          nuevoMonto: totalConAjustes.value,
        },
      });
    }

    const paymentData = {
      empenoId: props.empenoId,
      montoEfectivo: efectivo.value,
      montoTarjeta: tarjeta.value,
      observacion: observacion.value,
    };

    const response = await axiosClient.post(
      `/api/Empeño/PagarEmpeno`,
      {},
      {
        params: {
          empeñoId: paymentData.empenoId,
          observacion: paymentData.observacion,
          montoEfectivo: paymentData.montoEfectivo,
          montoTarjeta: paymentData.montoTarjeta,
        },
      }
    );
    
    console.log('Pago realizado exitosamente:', response.data);
    window.location.reload();
  } catch (error) {
    console.error('Error al realizar el pago:', error);
  }
};

// tarjetas logic
import { useAuthStore } from '../store/auth.js'; // Import the auth store
const selectedTarjeta = ref(null);
const tarjetas = ref([])
const porcentajeRecargo = ref(0);

const fetchTarjetas = async () => {
  try {
    const response = await axiosClient.get(`/GetTarjetas?InstitucionID=${InstitucionID.value}`);
    tarjetas.value = [...response.data.data];
  } catch (error) {
    console.error('Error fetching tarjetas:', error);
  }
};


const updateRecargo = () => {
  const subtotal = totalConAjustes.value - efectivo.value;
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

onMounted(getDatosLogin);
onMounted(fetchTarjetas);
const authStore = useAuthStore();
const InstitucionID = ref(null)
function getDatosLogin(){
    InstitucionID.value = authStore.institucionID;
  }

</script>

<style>
/* Add your styles here */
</style>
