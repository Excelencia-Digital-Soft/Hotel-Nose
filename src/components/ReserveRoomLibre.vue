 <template>
  <Teleport to="body">
    <!-- PrimeVue Toast for notifications -->
    <Toast position="top-right" />
    <Transition name="modal-outer" appear>
      <div
        v-if="room"
        class="fixed w-full h-full bg-black z-30 bg-opacity-80 backdrop-blur-lg top-0 left-0 flex justify-center items-center px-4 py-4">
        <Transition name="modal-inner">
          <div
            class="relative w-full max-w-5xl h-[90vh] flex flex-col bg-neutral-900 border-x-8 border-secondary-400 rounded-xl overflow-hidden">
            
            <!-- Header Compacto -->
            <div class="flex-shrink-0 px-6 py-4 bg-neutral-900 border-b border-neutral-700">
              <div class="flex justify-between items-center">
                <h1 class="text-2xl text-white lexend-exa font-bold">
                  {{ room?.nombreHabitacion || 'Habitación' }}
                </h1>
                <button
                  class="text-xl w-12 h-12 text-white btn-danger rounded-xl transition-all hover:scale-105"
                  @click="$emit('close-modal')">
                  ✕
                </button>
              </div>
            </div>

            <!-- Content Area -->
            <div class="grid grid-cols-1 md:grid-cols-2 gap-4 md:gap-2 p-4 min-h-0">

              <!-- Promociones -->
              <div class="flex col-span-1 md:col-span-2 w-full justify-center m-1">
                  <div class="flex flex-row w-full text-white z-[1] opacity-95 rounded-xl inset-0.5 bg-gradient-to-br from-neutral-800 to-neutral-900 p-2">
                    <div class="hidden md:flex items-center gap-2 mb-3 pb-2 border-b border-purple-500/30">
                      <span class="material-symbols-outlined text-purple-400">local_offer</span>
                      <h3 class="text-white font-semibold">Promociones Disponibles</h3>
                    </div>
                    <select v-model="selectedPromocion" 
                      class="w-full text-sm p-3 rounded-xl border-2 border-purple-300 focus:border-purple-500 focus:ring-2 focus:ring-purple-500/20 transition duration-200 bg-white text-gray-900">
                      <option :value="null">Sin Promoción</option>
                      <option v-if="promociones.length > 0" v-for="promo in promociones" :key="promo.promocionID" :value="promo">
                        {{ promo.detalle }}
                      </option>
                      <option v-if="promociones.length === 0" disabled>Sin promociones disponibles</option>
                    </select>
                  </div>
              </div>
              
              <!-- Column 1: Tiempo -->
              <div class="flex flex-col gap-4">
                
                <!-- Tiempo de Reserva -->
                <section v-if="selectedRoom.Disponible" class="drop-shadow-xl overflow-hidden rounded-xl bg-gradient-to-br from-primary-900/40 to-secondary-900/40 border border-primary-500/30">
                  <div class="flex flex-col text-white z-[1] opacity-95 rounded-xl inset-0.5 bg-gradient-to-br from-neutral-800 to-neutral-900 p-4 h-full">
                    <div class="flex items-center gap-2 mb-4 pb-2 border-b border-primary-500/30">
                      <span class="material-symbols-outlined text-primary-400">schedule</span>
                      <h3 class="text-white font-semibold">Tiempo de Reserva</h3>
                    </div>
                    
                    <div class="flex-1 flex flex-col justify-center space-y-4">
                      <!-- Horas -->
                      <div class="flex flex-col space-y-2">
                        <label class="text-sm font-semibold text-white flex items-center gap-2">
                          <span class="material-symbols-outlined text-sm">schedule</span>
                          Horas
                        </label>
                        <div class="bg-white rounded-lg p-1">
                          <InputNumber v-model="hours" :min="0" :max="99" showButtons 
                            class="w-full [&_.p-inputnumber-input]:text-center [&_.p-inputnumber-input]:font-bold [&_.p-inputnumber-input]:text-lg [&_.p-inputnumber-input]:text-gray-900" />
                        </div>
                      </div>
                      
                      <!-- Minutos -->
                      <div class="flex flex-col space-y-2">
                        <label class="text-sm font-semibold text-white flex items-center gap-2">
                          <span class="material-symbols-outlined text-sm">timer</span>
                          Minutos
                        </label>
                        <div class="bg-white rounded-lg p-1">
                          <InputNumber v-model="minutes" :min="0" :max="59" showButtons 
                            class="w-full [&_.p-inputnumber-input]:text-center [&_.p-inputnumber-input]:font-bold [&_.p-inputnumber-input]:text-lg [&_.p-inputnumber-input]:text-gray-900" />
                        </div>
                      </div>

                      <!-- Precio de reserva -->
                      <div class="flex justify-between items-center p-4 bg-gradient-to-r from-green-600/30 to-emerald-600/30 rounded-xl border border-green-500/40 mt-4">
                        <div class="flex items-center gap-2">
                          <span class="material-symbols-outlined text-green-400">paid</span>
                          <span class="font-bold text-lg text-white">Precio Total:</span>
                        </div>
                        <span class="font-bold text-2xl text-green-300">${{ periodoCost }}</span>
                      </div>
                    </div>
                  </div>
                </section>
              </div>

              <!-- Column 2: Información del Cliente -->
              <div class="flex flex-col">
                <section v-if="isSpecificRoute" class="drop-shadow-xl overflow-hidden rounded-xl bg-gradient-to-br from-green-900/40 to-teal-900/40 border border-green-500/30">
                  <div class="flex flex-col text-white z-[1] opacity-95 rounded-xl inset-0.5 bg-gradient-to-br from-neutral-800 to-neutral-900 p-4 h-full">
                    <div class="flex items-center gap-2 mb-4 pb-2 border-b border-green-500/30">
                      <span class="material-symbols-outlined text-green-400">person</span>
                      <h3 class="text-white font-semibold">Información del Cliente</h3>
                    </div>

                    <div class="flex-1 flex flex-col justify-center space-y-4">
                      <!-- Identificador -->
                      <div class="flex flex-col space-y-2">
                        <label for="nombre" class="text-sm font-semibold leading-6 text-white flex items-center gap-2">
                          <span class="material-symbols-outlined text-sm">badge</span>
                          Identificador
                        </label>
                        <input type="text" v-model="selectedRoom.Identificador" maxlength="40"
                          class="focus:ring-purple-500 border-2 w-full focus hover:shadow-lg hover:shadow-purple-500/50 border-purple-200 rounded-xl py-3 px-4 transition duration-150 ease-out md:ease-in text-lg text-gray-900 bg-white"
                          placeholder="Nombre o identificador del cliente">
                      </div>

                      <!-- Grid para Teléfono y Patente -->
                      <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
                        <!-- Teléfono -->
                        <div class="flex flex-col space-y-2">
                          <label for="telefono" class="text-sm font-semibold leading-6 text-white flex items-center gap-2">
                            <span class="material-symbols-outlined text-sm">phone</span>
                            Teléfono
                          </label>
                          <input type="text" v-model="selectedRoom.NumeroTelefono" maxlength="11"
                            class="focus:ring-purple-500 border-2 w-full focus hover:shadow-lg hover:shadow-purple-500/50 border-purple-200 rounded-xl py-3 px-4 transition duration-150 ease-out md:ease-in text-gray-900 bg-white"
                            placeholder="264 123-4567">
                        </div>

                        <!-- Patente -->
                        <div class="flex flex-col space-y-2">
                          <label for="patente" class="text-sm font-semibold leading-6 text-white flex items-center gap-2">
                            <span class="material-symbols-outlined text-sm">directions_car</span>
                            Patente
                          </label>
                          <input type="text" v-model="selectedRoom.PatenteVehiculo" maxlength="11"
                            class="focus:ring-purple-500 border-2 w-full focus hover:shadow-lg hover:shadow-purple-500/50 border-purple-200 rounded-xl py-3 px-4 transition duration-150 ease-out md:ease-in text-gray-900 bg-white"
                            placeholder="ABC123">
                        </div>
                      </div>
                    </div>
                  </div>
                </section>

                <!-- Placeholder cuando no es ruta específica -->
                <div v-else class="flex-1 flex items-center justify-center">
                  <div class="text-center text-white/60">
                    <span class="material-symbols-outlined text-6xl mb-4 block">hotel</span>
                    <p class="text-lg">Información del cliente disponible en vista detallada</p>
                  </div>
                </div>

              </div>

            </div>

            <!-- Footer: Botón de Acción -->
            <div class="flex-shrink-0 px-6 py-4 bg-neutral-900 border-t border-neutral-700">
              <div class="flex justify-center">
                <button @click="reserveRoom" type="button" 
                  :disabled="isLoading"
                  class="relative btn-primary w-full max-w-md h-14 rounded-2xl text-lg font-bold transition-all duration-200 hover:scale-105 disabled:opacity-50 disabled:cursor-not-allowed disabled:hover:scale-100">
                  <span v-if="!isLoading" class="flex items-center justify-center gap-3">
                    <span class="material-symbols-outlined">door_open</span>
                    Ocupar Habitación
                  </span>
                  <span v-else class="flex items-center justify-center gap-3">
                    <ProgressSpinner style="width: 25px; height: 25px" strokeWidth="8" fill="transparent"
                      animationDuration=".5s" aria-label="Loading" />
                    Procesando...
                  </span>
                </button>
              </div>
            </div>
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
  
  // Validación: solo el tiempo es obligatorio
  const hasTime = hours.value > 0 || minutes.value > 0;
  
  if (numeroError.value !== '' || !hasTime) {
    // No envíes el formulario si hay errores de validación
    console.log("faltan datos obligatorios");
    toast.add({
      severity: 'warn',
      summary: 'Datos incompletos',
      detail: !hasTime ? 'Por favor ingrese al menos horas o minutos' : 'Hay errores en la validación',
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
        life: 9000
      });
      isLoading.value = false; // Desactivar indicador de carga
      
      // Reload page to update rooms
      setTimeout(() => {
        window.location.reload();
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

/* Custom styles for PrimeVue InputNumber */
:deep(.p-inputnumber) {
  width: 100%;
}

:deep(.p-inputnumber-input) {
  width: 100%;
  text-align: center;
  font-weight: bold;
  font-size: 1.125rem;
}

:deep(.p-inputnumber-button) {
  width: 2.5rem;
}
</style>
