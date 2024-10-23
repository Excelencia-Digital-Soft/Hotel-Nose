<template>
  <Teleport to="body" >
  <Transition name="modal-outer" appear>
    <div class="fixed w-full h-full  bg-black bg-opacity-80 backdrop-blur-lg top-0 left-0 flex justify-center items-center px-8">
      <Transition name="modal-inner">
        <div
        :class="selectedRoom.Disponible ? 'w-2/4 h-3/6 flex flex-col justify-center fixed mt-4 p-8 pt-6 border-x-8  border-secondary-400 rounded-xl bg-neutral-900':' w-3/4 h-5/6 flex flex-col justify-center fixed mt-4 p-8 pt-6 border-x-8  border-secondary-400 rounded-xl bg-neutral-900'">
          <i class="fa-thin fa-circle-xmark"></i>
          <!-- Modal Content -->
          <h1 class="self-center text-2xl text-white lexend-exa font-bold mt-5 mb-5">
            {{ room.nombreHabitacion }}
          </h1>

          <form :class="!selectedRoom.Disponible ? 'grid grid-cols-3 gap-3 mb-2' : 'grid-cols-1'">
            <section class="grid grid-cols-2 gap-3 mb-2">
              <div class="grid col-span-2 relative mb-3">
                <label for="nombre" class="text-sm font-semibold leading-6 text-white">Identificador</label>
                <input type="text"
                  class="focus:ring-purple-500 border-2 w-full focus hover:shadow-lg hover:shadow-purple-500/50 border-purple-200 rounded-3xl transition duration-150 ease-out md:ease-in"
                  v-model="selectedRoom.Identificador" placeholder="Identificador" maxlength="40">
              </div>
              <div class="grid relative mb-3">
                <label for="cuit" class="text-sm font-semibold leading-6 text-white">Telefono</label>
                <input type="text"
                  class="focus:ring-purple-500 border-2 w-full focus hover:shadow-lg hover:shadow-purple-500/50 border-purple-200 rounded-3xl transition duration-150 ease-out md:ease-in"
                  maxlength="11" v-model="selectedRoom.NumeroTelefono" placeholder="Ingresa Marca y modelo de vehiculo">
              </div>
              <div class="grid relative mb-3">
                <label for="cuit" class="text-sm font-semibold leading-6 text-white">Patente</label>
                <input type="text"
                  class="focus:ring-purple-500 border-2 w-full focus hover:shadow-lg hover:shadow-purple-500/50 border-purple-200 rounded-3xl transition duration-150 ease-out md:ease-in"
                  maxlength="11" v-model="selectedRoom.PatenteVehiculo" placeholder="Ingrese el numero de Patente">
              </div>
            </section>
            <section v-if="!selectedRoom.Disponible" class="grid place-items-center  relative mb-3">
              <label class="text-sm font-semibold leading-6 text-white">Tiempo de Reserva</label>
              <div class="card flex">
                <div class="grid mb-3 mr-4">
                  <label for="hours" class="text-xs font-semibold leading-6 text-white">Horas</label>

                  <InputNumber v-model="hours" showButtons buttonLayout="vertical" style="width: 3rem" :min="0"
                    :max="99">
                    <template #incrementbuttonicon>
                      <span class="pi pi-plus" />
                    </template>
                    <template #decrementbuttonicon>
                      <span class="pi pi-minus" />
                    </template>
                  </InputNumber>
                </div>
                <div class="grid mb-3">
                  <label for="minutes" class="text-xs font-semibold leading-6 text-white">Minutos</label>
                  <InputNumber v-model="minutes" showButtons buttonLayout="vertical" style="width: 3rem" :min="0"
                    :max="99">
                    <template #incrementbuttonicon>
                      <span class="pi pi-plus" />
                    </template>
                    <template #decrementbuttonicon>
                      <span class="pi pi-minus" />
                    </template>
                  </InputNumber>
                </div>
              </div>
            </section>
            <section v-if="!selectedRoom.Disponible" class="grid grid-cols-2">
              <div class="max-w-sm mx-auto mt-4 space-y-4">
                <!-- Input para la fecha actual -->
                <div>
                  <label for="fecha" class="block text-white">Fecha Actual</label>
                  <input id="fecha" type="date" v-model="currentDate"
                    class="w-full p-2 border border-gray-700 bg-gray-900 text-white" readonly />
                </div>

                <!-- Input para la hora actual -->
                <div>
                  <label for="hora" class="block text-white">Hora Actual</label>
                  <input id="hora" type="time" v-model="currentTime"
                    class="w-full p-2 border border-gray-700 bg-gray-900 text-white" readonly />
                </div>
              </div>
              <div class="card flex flex-col flex-wrap justify-center gap-4">
                <div class="flex items-center">
                  <Checkbox v-model="pizza" inputId="ingredient1" name="pizza" value="Avisar Hora Salida" />
                  <label for="ingredient1" class="ml-2 text-white"> Avisar Hora Salida </label>
                </div>
                <div class="flex items-center">
                  <Checkbox v-model="pizza" inputId="ingredient2" name="pizza" value="Precio por siesta" />
                  <label for="ingredient2" class="ml-2 text-white"> Precio por siesta </label>
                </div>
                <div class="flex items-center">
                  <Checkbox v-model="pizza" inputId="ingredient3" name="pizza" value="Precio por dormir" />
                  <label for="ingredient3" class="ml-2 text-white"> Precio por dormir </label>
                </div>
                <div class="flex items-center">
                  <Checkbox v-model="pizza" inputId="ingredient4" name="pizza" value="Pareja" />
                  <label for="ingredient4" class="ml-2 text-white"> Pareja </label>
                </div>
              </div>
            </section>
            <section v-if="!selectedRoom.Disponible" class="p-10">

              <button type="button" 
              @click="toggleModalConfirm()"
                class="btn-primary w-full h-full text-lg font-bold  tracking-wider rounded-3xl">Agregar Consumisión</button>

            </section>
            <section v-if="!selectedRoom.Disponible">
              <div class="max-w-sm mx-auto border border-gray-800 bg-gray-800 text-white">
                <table class="w-full text-left">
                  <tbody>
                    <tr class="border-b border-gray-700">
                      <td class="p-4">Consumision</td>
                      <td class="p-4 text-right">$4500</td>
                    </tr>
                    <tr class="border-b border-gray-700">
                      <td class="p-4">Periodo</td>
                      <td class="p-4 text-right">$9500</td>
                    </tr>
                    <tr class="border-b border-gray-700">
                      <td class="p-4">Adicional</td>
                      <td class="p-4 text-right">$1500</td>
                    </tr>
                    <tr>
                      <td class="p-4">Total</td>
                      <td class="p-4 text-right">$15500</td>
                    </tr>
                  </tbody>
                </table>
              </div>
            </section>
            <section v-if="!selectedRoom.Disponible">
              <div class="max-w-sm mx-auto border border-gray-800 bg-gray-800 text-white">
                <table class="w-full text-left">
                  <tbody>
                    <tr class="border-b border-gray-700">
                      <td class="p-2">Descuento</td>
                      <td class="p-2">
                        <input type="number" v-model="tableData.descuento"
                          class="w-full p-2 bg-gray-900 text-white border border-gray-600" />
                      </td>
                    </tr>
                    <tr class="border-b border-gray-700">
                      <td class="p-2">Tarjeta</td>
                      <td class="p-2">
                        <input type="number" v-model="tableData.tarjeta"
                          class="w-full p-2 bg-gray-900 text-white border border-gray-600" />
                      </td>
                    </tr>
                    <tr class="border-b border-gray-700">
                      <td class="p-2">Recargos</td>
                      <td class="p-2">
                        <input type="number" v-model="tableData.recargos"
                          class="w-full p-2 bg-gray-900 text-white border border-gray-600" />
                      </td>
                    </tr>
                    <tr class="border-b border-gray-700">
                      <td class="p-2">Empeños</td>
                      <td class="p-2">
                        <input type="text" v-model="tableData.empenos"
                          class="w-full p-2 bg-gray-900 text-white border border-gray-600" />
                      </td>
                    </tr>
                    <tr>
                      <td class="p-2">Total a cobrar</td>
                      <td class="p-2">
                        <input type="number" v-model="tableData.total"
                          class="w-full p-2 bg-gray-900 text-white border border-gray-600" readonly />
                      </td>
                    </tr>
                  </tbody>
                </table>
              </div>
            </section>
            <div class="col-span-3 flex justify-center items-center w-full">
              <button @click="reserveRoom" type="button"
                class="btn-primary w-2/4 h-16 rounded-2xl ">Ocupar
                Habitacion</button>
            </div>
          </form>
          <button
            class="absolute text-xl  w-14 h-14 text-white -top-7 right-7 btn-primary rounded-full transition duration-150 ease-out md:ease-in"
            @click="$emit('close-modal')">X</button>
          <ModalConfirm v-if="modalConfirm" 
          :name="selectedRoom.Identificador" 
          @confirmaAccion="confirmAndSend"
          @close="toggleModalConfirm"/>
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

const emits = defineEmits(["close-modal"])
const props = defineProps({
  room: Object,
});


onMounted(() => {
  selectedRoom.value.HabitacionID = props.room.habitacionId
  selectedRoom.value.Disponible = props.room.disponible
  if (!selectedRoom.value.Disponible){
    selectedRoom.value.Identificador = props.room.visita.identificador
    selectedRoom.value.PatenteVehiculo = props.room.visita.patenteVehiculo
  }
  setCurrentDateTime();
  document.body.style.overflow = 'hidden';
})

let selectedRoom = ref({
  HabitacionID: 0,
  Disponible:null,
  FechaReserva: '',
  FechaFin: '',
  TotalHoras: 0,
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
const pizza = ref();
const products = ref();
const hours = ref(0);
const minutes = ref(0);
const selectedTags = ref([]);
let editTagRel = {}
let cheatRefresh = ref(false);
let idNewTag = ref(0);
let numeroError = ref('');

const toggleModalConfirm = () => {
  modalConfirm.value = !modalConfirm.value;
}
const confirmAndSend = (ConfirmedArticles) =>{
  
  console.log(ConfirmedArticles+" Llegamos al ReserveROOM")
  const consumos = [];
}
const handleCheat = (cheatIds) => {
  //le avisamos al componente DropDownTag que actualice para agregar los nuevos datos
  cheatRefresh.value = true
  idNewTag.value = cheatIds
  console.log("1 el numero", cheatIds)

};

console.log("tagselected : " + editTagRel.value)

//SECTOR DE VALIDACIONES DE FORMULARIO


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