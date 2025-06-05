 <template>
  <Teleport to="body">
    <!-- PrimeVue Toast for notifications -->
    <Toast position="top-right" />
    <Transition name="modal-outer" appear>
      <div
        class="fixed w-full h-full bg-black  z-30 bg-opacity-80 backdrop-blur-lg top-0 left-0 flex justify-center items-center px-8">
        <Transition name="modal-inner">
          <div
            class="w-full md:w-2/4 h-auto flex flex-col justify-center fixed mt-4 p-8 pt-6 border-x-8 border-secondary-400 rounded-xl bg-neutral-900">
            <h1 class="self-center text-2xl text-white lexend-exa font-bold mt-5 mb-5">
              {{ room.nombreHabitacion }}
            </h1>

            <form class="w-full grid-cols-1">
              <section v-if="selectedRoom.Disponible" class="grid  place-items-center mb-3">
                <label class="text-sm font-semibold text-white">Tiempo de Reserva</label>
                <div class="flex flex-col  space-y-4 w-full border-y-2 border-accent-500 rounded-xl p-4 ">
                  <div class="grid ">
                    <label class="text-xs font-semibold text-white">Horas</label>
                    <InputNumber v-model="hours" :min="0" :max="99" showButtons />
                  </div>
                  <div class="grid">
                    <label class="text-xs font-semibold text-white">Minutos</label>
                    <InputNumber v-model="minutes" :min="0" :max="59" showButtons />
                  </div>
                </div>
                <div class="mt-4 w-full">
                  <label class="text-xs font-semibold text-white">Seleccionar Promoción</label>
                  <select v-model="selectedPromocion" class="w-full p-2 mt-2 rounded-lg">
                    <option :value="null">Sin Promoción</option>
                    <option v-if="promociones.length > 0" v-for="promo in promociones" :key="promo.promocionID"
                      :value="promo">
                      {{ promo.detalle }}
                    </option>
                  </select>
                </div>
                <div class="flex justify-between items-center mt-4">
                  <label class="text-xs font-semibold text-white">Precio de reserva:</label>
                  <span class="text-green-500 font-bold text-sm">{{ periodoCost }}</span>
                </div>
              </section>
              <section v-if="isSpecificRoute" class="grid grid-cols-2 gap-3 mb-2">
                <!-- Other inputs like Identificador, Telefono, Patente -->
                <div class="grid col-span-2 relative mb-3">
                  <label for="nombre" class="text-sm font-semibold leading-6 text-white">Identificador</label>
                  <input type="text" v-model="selectedRoom.Identificador" maxlength="40"
                    class="focus:ring-purple-500 border-2 w-full focus hover:shadow-lg hover:shadow-purple-500/50 border-purple-200 rounded-3xl transition duration-150 ease-out md:ease-in"
                    placeholder="Identificador">
                </div>
                <div class="grid relative mb-3" >
                  <label for="telefono" class="text-sm font-semibold leading-6 text-white">Teléfono</label>
                  <input type="text" v-model="selectedRoom.NumeroTelefono" maxlength="11"
                    class="focus:ring-purple-500 border-2 w-full focus hover:shadow-lg hover:shadow-purple-500/50 border-purple-200 rounded-3xl transition duration-150 ease-out md:ease-in"
                    placeholder="Ingresa número de teléfono">
                </div>
                <div class="grid relative">
                  <label for="patente" class="text-sm font-semibold leading-6 text-white">Patente</label>
                  <input type="text" v-model="selectedRoom.PatenteVehiculo" maxlength="11"
                    class="focus:ring-purple-500 border-2 w-full focus hover:shadow-lg hover:shadow-purple-500/50 border-purple-200 rounded-3xl transition duration-150 ease-out md:ease-in"
                    placeholder="Ingrese el número de patente">
                </div>
              </section>
              <div class="col-span-3 flex justify-center items-center w-full">
                <button @click="reserveRoom" type="button" class="relative btn-primary w-full md:w-2/4 h-16 rounded-2xl">Ocupar
                  Habitación
                  <div class="card absolute top-4 right-8">
							<ProgressSpinner class="absolute " v-if="isLoading" style="width: 25px; height: 25px" strokeWidth="8" fill="transparent"
								animationDuration=".5s" aria-label="Custom ProgressSpinner" />
						</div>
          </button>
                  
              </div>
            </form>
            <button
              class="absolute text-xl w-12 h-12 lg:w-14 lg:h-14 text-white top-0 -right-2 lg:top-6 lg:right-6 btn-danger rounded-xl lg:rounded-full"
              @click="$emit('close-modal')">X</button>
          </div>
        </Transition>
      </div>
    </Transition>
  </Teleport>
</template>


<script setup>
import { computed } from 'vue';
import { onMounted, ref, watch } from 'vue';
import axiosClient from '../axiosClient';
import InputNumber from 'primevue/inputnumber';
import Checkbox from 'primevue/checkbox';
import { useRoute } from 'vue-router';
import ProgressSpinner from 'primevue/progressspinner';
import Toast from 'primevue/toast';
import { useToast } from 'primevue/usetoast';
import { useAuthStore } from '../store/auth.js'; // Import the auth store

const isSpecificRoute = computed(() => route.path.startsWith('/Rooms'));
const UsuarioID = ref(null);
const InstitucionID = ref(null);
const authStore = useAuthStore();
const route = useRoute();
const toast = useToast();
let isLoading = ref(false);
function getDatosLogin() {
  InstitucionID.value = authStore.institucionID;
  UsuarioID.value = authStore.usuarioID || 14; // Fallback to 14 if no user ID
}

const hours = ref(0);
const periodoCost = ref(0);
const minutes = ref(0);
const promociones = ref([]);
const selectedPromocion = ref(null);

const emits = defineEmits(["close-modal", "room-reserved"])
const props = defineProps({
  room: Object,
});


onMounted(async () => {
  selectedRoom.value.HabitacionID = props.room.habitacionId
  selectedRoom.value.Disponible = props.room.disponible
  selectedRoom.value.Precio = props.room.precio
  try {
    const response = await axiosClient.get(`/api/Promociones/GetPromocionesCategoria?categoriaID=${props.room.categoriaId}`);
    promociones.value = response.data.data || [];
    console.log(promociones.value)
  } catch (error) {
    console.error('Error fetching promociones:', error);
  }
  console.log("Se aplicó");
  updatePeriodoCost(); // Initial calculation    setCurrentDateTime();
  document.body.style.overflow = 'hidden';
})



watch([selectedPromocion], () => {
  if (selectedPromocion.value != null) {
    hours.value = selectedPromocion.value.cantidadHoras;
    periodoCost.value = Math.round(selectedPromocion.value.tarifa * selectedPromocion.value.cantidadHoras);
    selectedRoom.value.PromocionID = selectedPromocion.value.promocionID;
  }
  else updatePeriodoCost();
});

let selectedRoom = ref({
  HabitacionID: 0,
  Disponible: null,
  FechaReserva: '',
  FechaFin: '',
  TotalHoras: 0,
  TotalMinutos: 0,
  Precio: 0,
  PromocionID: 0,
  TotalHoras: 0,
  TotalMinutos: 0,
  UsuarioID: 14,
  PatenteVehiculo: '',
  NumeroTelefono: '',
  Identificador: '',
  esReserva: true,
})









// Objeto con los valores de la tabla
const tableData = ref({
  descuento: 0,
  tarjeta: 0,
  recargos: 0,
  empenos: '',
  total: 0,
});


const currentDate = ref('');
const currentTime = ref('');
let editTagRel = {}
let cheatRefresh = ref(false);
let idNewTag = ref(0);
let numeroError = ref('');

onMounted(() => {
  getDatosLogin();
});

console.log("tagselected : " + editTagRel.value)

//SECTOR DE VALIDACIONES DE FORMULARIO

watch([hours, minutes], ([newHours, newMinutes]) => {
  selectedRoom.value.TotalHoras = newHours;
  selectedRoom.value.TotalMinutos = newMinutes;
});
// Función para calcular el total automáticamente
watch([() => tableData.value.descuento, () => tableData.value.tarjeta, () => tableData.value.recargos], () => {
  tableData.value.total = tableData.value.descuento + tableData.value.tarjeta + tableData.value.recargos;
});
// Función para obtener la fecha y la hora actuales
const setCurrentDateTime = () => {
  const now = new Date();

  // Formato de la fecha en yyyy-mm-dd
  currentDate.value = now.toISOString().substr(0, 10);

  // Formato de la hora en hh:mm
  currentTime.value = now.toTimeString().substr(0, 5);
};

const updatePeriodoCost = () => {
  const totalHours = hours || 0;
  const totalMinutes = minutes || 0;
  console.log(selectedPromocion.value);
  if (selectedPromocion.value == null) {
    const hourlyRate = selectedRoom.value.Precio || 0;
    const totalPeriod = totalHours.value + totalMinutes.value / 60;
    periodoCost.value = (totalPeriod * hourlyRate).toFixed(2);
    console.log(totalPeriod)
    console.log(totalHours.value, totalMinutes.value, hourlyRate)
  }
  else {
    periodoCost.value = Math.round(selectedPromocion.value.tarifa * totalHours.value + selectedPromocion.value.tarifa * totalMinutes.value / 60);
    console.log(periodoCost)
  }
};

watch([hours, minutes], updatePeriodoCost);


const validarNumero = (num) => {
  const numero = num;
  if (!numero) {
    numeroError.value = 'Este Campo es obligatorio'
  } else if (!/^\d+$/.test(numero)) {
    numeroError.value = 'Solo se permiten números';
  } else {
    numeroError.value = '';
  }
}

const actualizarFechas = () => {
  const now = new Date();

  // Convertir la fecha y hora actual a la zona horaria local
  const localFechaReserva = new Date(now.getTime() - now.getTimezoneOffset() * 60000).toISOString().slice(0, -1);

  selectedRoom.value.FechaReserva = localFechaReserva;
  // Sumar una hora más
  const fechaConUnaHoraMas = new Date(now);
  fechaConUnaHoraMas.setHours(fechaConUnaHoraMas.getHours() + 1);

  // Convertir la nueva fecha con una hora adicional a la zona horaria local
  const localFechaReservaConUnaHoraMas = new Date(fechaConUnaHoraMas.getTime() - fechaConUnaHoraMas.getTimezoneOffset() * 60000).toISOString().slice(0, -1);

  // Aquí puedes asignar la nueva fecha si es necesario
  selectedRoom.value.FechaFin = localFechaReservaConUnaHoraMas;
};


//Reservar Habitacion
const reserveRoom = () => {
  if (isLoading.value) return; // Evitar múltiples solicitudes
  isLoading.value = true;
  actualizarFechas()
  
  // Validación corregida usando .value
  if (numeroError.value || (selectedRoom.value.PatenteVehiculo == '' && selectedRoom.value.Identificador == '' && selectedRoom.value.NumeroTelefono == '')) {
    // No envíes el formulario si hay errores de validación
    console.log("faltan datos obligatorios");
    toast.add({
      severity: 'warn',
      summary: 'Datos incompletos',
      detail: 'Por favor complete todos los campos obligatorios',
      life: 10000
    });
    isLoading.value = false; // Resetear loading state
    return;
  }

  console.log('Enviando reserva:', selectedRoom.value);
  console.log('InstitucionID:', InstitucionID.value, 'UsuarioID:', UsuarioID.value);

  axiosClient.post(`/ReservarHabitacion?InstitucionID=${InstitucionID.value}&UsuarioID=${UsuarioID.value}`, selectedRoom.value)
    .then(res => {
      console.log('Reserva exitosa:', res.data);
      toast.add({
        severity: 'success',
        summary: 'Éxito',
        detail: 'Habitación ocupada exitosamente',
        life: 10000
      });
      isLoading.value = false; // Desactivar indicador de carga
      
      // Emit event to update parent component instead of reloading page
      setTimeout(() => {
        emits('room-reserved', selectedRoom.value.HabitacionID);
        emits('close-modal');
      }, 1500);
    })
    .catch(error => {
      console.error('Error en reserva:', error);
      toast.add({
        severity: 'error',
        summary: 'Error',
        detail: 'Error al reservar la habitación. Por favor, intente nuevamente.',
        life: 10000
      });
      isLoading.value = false; // Resetear loading state
    });
}



</script>
<style scoped>
.modal-outer-enter-active,
.modal-outer-leave-active {
  transition: opacity 0.3s cubic-bezier(0.52, 0.02, 0.19, 1.02);
}

.modal-outer-enter-from,
.modal-outer-leave-to {
  opacity: 0;
}

.modal-inner-enter-active {
  transition: all 0.3s cubic-bezier(0.52, 0.02, 0.19, 1.02) 0.10s;
}

.modal-inner-leave-active {
  transition: all 0.3s cubic-bezier(0.52, 0.02, 0.19, 1.02);
}

.modal-inner-enter-from {
  opacity: 0;
  transform: scale(0.8);
}

.modal-inner-leave-to {
  transform: scale(0.8);
}
</style>