<template>
  <Teleport to="body">
    <Transition name="modal-outer" appear>
      <div class="fixed w-full h-full bg-black bg-opacity-80 backdrop-blur-lg top-0 left-0 flex justify-center items-center px-8">
        <Transition name="modal-inner">
          <div class="w-2/4 h-3/6 flex flex-col justify-center fixed mt-4 p-8 pt-6 border-x-8 border-secondary-400 rounded-xl bg-neutral-900">
            <h1 class="self-center text-2xl text-white lexend-exa font-bold mt-5 mb-5">
              {{ room.nombreHabitacion }}
            </h1>
            
            <form class="grid-cols-1">
              <section v-if="selectedRoom.Disponible" class="grid place-items-center mb-3">
                <label class="text-sm font-semibold text-white">Tiempo de Reserva</label>
                <div class="flex space-x-4">
                  <div>
                    <label class="text-xs font-semibold text-white">Horas</label>
                    <InputNumber v-model="hours" :min="0" :max="99" showButtons />
                  </div>
                  <div>
                    <label class="text-xs font-semibold text-white">Minutos</label>
                    <InputNumber v-model="minutes" :min="0" :max="59" showButtons />
                  </div>
                </div>
                <div class="mt-4 w-full">
                  <label class="text-xs font-semibold text-white">Seleccionar Promoción</label>
                  <select v-model="selectedPromocion" class="w-full p-2 mt-2 rounded-lg">
                    <option :value="null">Sin Promoción</option>
                    <option v-if="promociones.length > 0" v-for="promo in promociones" :key="promo.promocionID" :value="promo">
                      {{ promo.detalle }}
                    </option>
                  </select>
                </div>
                <div class="flex justify-between items-center mt-4">
                  <label class="text-xs font-semibold text-white">Precio de reserva:</label>
                  <span class="text-green-500 font-bold text-sm">{{ periodoCost }}</span>
                </div>
              </section>
              <section class="grid grid-cols-2 gap-3 mb-2">
                <!-- Other inputs like Identificador, Telefono, Patente -->
                <div class="grid col-span-2 relative mb-3">
                  <label for="nombre" class="text-sm font-semibold leading-6 text-white">Identificador</label>
                  <input type="text" v-model="selectedRoom.Identificador" maxlength="40" 
                         class="focus:ring-purple-500 border-2 w-full focus hover:shadow-lg hover:shadow-purple-500/50 border-purple-200 rounded-3xl transition duration-150 ease-out md:ease-in"
                         placeholder="Identificador">
                </div>
                <div class="grid relative mb-3">
                  <label for="telefono" class="text-sm font-semibold leading-6 text-white">Teléfono</label>
                  <input type="text" v-model="selectedRoom.NumeroTelefono" maxlength="11"
                         class="focus:ring-purple-500 border-2 w-full focus hover:shadow-lg hover:shadow-purple-500/50 border-purple-200 rounded-3xl transition duration-150 ease-out md:ease-in"
                         placeholder="Ingresa número de teléfono">
                </div>
                <div class="grid relative mb-3">
                  <label for="patente" class="text-sm font-semibold leading-6 text-white">Patente</label>
                  <input type="text" v-model="selectedRoom.PatenteVehiculo" maxlength="11"
                         class="focus:ring-purple-500 border-2 w-full focus hover:shadow-lg hover:shadow-purple-500/50 border-purple-200 rounded-3xl transition duration-150 ease-out md:ease-in"
                         placeholder="Ingrese el número de patente">
                </div>
              </section>
              <div class="col-span-3 flex justify-center items-center w-full">
                <button @click="reserveRoom" type="button" class="btn-primary w-2/4 h-16 rounded-2xl">Reservar Habitación</button>
              </div>
            </form>
            <button class="absolute text-xl w-14 h-14 text-white -top-7 right-7 btn-primary rounded-full"
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
  import ModalConfirm from './ModalConfirm.vue';
  
  const hours = ref(0);
  const periodoCost = ref(0);
  const minutes = ref(0);
  const promociones = ref([]);
  const selectedPromocion = ref(null);

  const emits = defineEmits(["close-modal"])
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
  updatePeriodCost(); // Initial calculation    setCurrentDateTime();
    document.body.style.overflow = 'hidden';
  })

  function updatePeriodCost() {
  if (selectedPromocion.value) {
    selectedRoom.value.PromocionID = selectedPromocion.value.promocionID;
    // Use the promotional rate
    hours.value = selectedPromocion.value.cantidadHoras;
    periodoCost.value = Math.round(selectedPromocion.value.tarifa * selectedPromocion.value.cantidadHoras);
  } else {
    // Use the default room price
    const totalHours = hours.value + (minutes.value / 60);
    periodoCost.value = Math.round(selectedRoom.value.Precio * totalHours);
  }
}

  watch([hours, minutes, selectedPromocion], updatePeriodCost);

  let selectedRoom = ref({
    HabitacionID: 0,
    Disponible:null,
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
  
  let modalConfirm = ref(false);
  const currentDate = ref('');
  const currentTime = ref('');
  let editTagRel = {}
  let cheatRefresh = ref(false);
  let idNewTag = ref(0);
  let numeroError = ref('');
  
  const toggleModalConfirm = () => {
    modalConfirm.value = !modalConfirm.value;
  }
  
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
      const hourlyRate = selectedRoom.value.Precio || 0;
      const totalPeriod = totalHours.value + totalMinutes.value / 60;
      periodoCost.value = (totalPeriod * hourlyRate).toFixed(2);
      console.log(totalPeriod)
      console.log(totalHours.value, totalMinutes.value, hourlyRate)
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
     actualizarFechas()
    if (numeroError.value || (selectedRoom.PatenteVehiculo == '' && selectedRoom.Identificador == '' && selectedRoom.NumeroTelefono == '')) {
      // No envíes el formulario si hay errores de validación
      console.log("faltan datos obligatorios")
      return;
    }
    console.log("loquese Envía", selectedRoom.value)
    debugger
    axiosClient.post('/ReservarHabitacion', selectedRoom.value)
      .then(res => {
        console.log(res.data);
        alert("Reservacion Exitosa");
        emits('close-modal');
        window.location.reload();
      })
      .catch(error => {
        console.error(error);
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