<template>
  <Transition name="modal-outer" appear>
    <div class="h-full min-h-screen absolute w-full bg-black bg-opacity-40 top-0 left-0 flex justify-center px-8">
      <Transition name="modal-inner">
        <div
          class="flex flex-col justify-center fixed p-8 pt-6 border-l-8 border-indigo-200 rounded-xl bg-gray-500 self-start mt-6 w-3/4 h-3/4">
          <i class="fa-thin fa-circle-xmark"></i>
          <!-- Modal Content -->
          <h1 class="self-center text-2xl text-stone-800 font-bold mt-5 mb-5">
            {{ room.nombreHabitacion }}
          </h1>

          <form class="grid grid-cols-3 gap-3 mb-2">
            <div class="grid relative mb-3">
              <label for="nombre" class="text-sm font-semibold leading-6 text-gray-800">Identificador</label>
              <input type="text"
                class="focus:ring-purple-500 border-2 w-full focus hover:shadow-lg hover:shadow-purple-500/50 border-purple-200 rounded-3xl transition duration-150 ease-out md:ease-in"
                v-model="selectedRoom.Identificador" placeholder="Nombre"
                maxlength="40">
            </div>
            <div class="grid relative mb-3">
              <label for="cuit" class="text-sm font-semibold leading-6 text-gray-800">Auto/Modelo</label>
              <input type="text"
                class="focus:ring-purple-500 border-2 w-full focus hover:shadow-lg hover:shadow-purple-500/50 border-purple-200 rounded-3xl transition duration-150 ease-out md:ease-in"
                maxlength="11"
                v-model="selectedRoom.Identificador" placeholder="Ingresa Marca y modelo de vehiculo">
            </div>
            <div class="grid relative mb-3">
              <label for="cuit" class="text-sm font-semibold leading-6 text-gray-800">Patente</label>
              <input type="text"
                class="focus:ring-purple-500 border-2 w-full focus hover:shadow-lg hover:shadow-purple-500/50 border-purple-200 rounded-3xl transition duration-150 ease-out md:ease-in"
                maxlength="11"
                v-model="selectedRoom.PatenteVehiculo" placeholder="Ingrese el numero de Patente">
            </div>
            <div class="grid relative mb-3">
              <label for="cuit" class="text-sm font-semibold leading-6 text-gray-800">Numero de Telefono</label>
              <input type="text"
                class="focus:ring-purple-500 border-2 w-full focus hover:shadow-lg hover:shadow-purple-500/50 border-purple-200 rounded-3xl transition duration-150 ease-out md:ease-in"
                maxlength="11"
                v-model="selectedRoom.NumeroTelefono" placeholder="Ingrese su numero de telefono" @input="validarNumero(selectedRoom.NumeroTelefono)">
              <span class="text-red-400 font-bold text-xs absolute -bottom-5 ">{{ numeroError }}</span>
            </div>
            <div class="grid relative mb-3">
              <label for="cuit" class="text-sm font-semibold leading-6 text-gray-800">Tiempo de Reserva</label>
              <div class="card flex">
                <div class="grid mb-3">
                  <label for="hours" class="text-xs font-semibold leading-6 text-gray-800">Horas</label>

                <InputNumber v-model="hours" showButtons buttonLayout="vertical" style="width: 3rem" :min="0" :max="99">
                    <template #incrementbuttonicon>
                        <span class="pi pi-plus" />
                    </template>
                    <template #decrementbuttonicon>
                        <span class="pi pi-minus" />
                    </template>
                </InputNumber>
              </div>
              <div class="grid mb-3">
                <label for="minutes" class="text-xs font-semibold leading-6 text-gray-800">Minutos</label>
                <InputNumber v-model="minutes" showButtons buttonLayout="vertical" style="width: 3rem" :min="0" :max="99">
                    <template #incrementbuttonicon>
                        <span class="pi pi-plus" />
                    </template>
                    <template #decrementbuttonicon>
                        <span class="pi pi-minus" />
                    </template>
                </InputNumber>
              </div>
              </div>
              <!-- <input type="text"
                class="focus:ring-purple-500 border-2 w-full focus hover:shadow-lg hover:shadow-purple-500/50 border-purple-200 rounded-3xl transition duration-150 ease-out md:ease-in"
                maxlength="11"
                v-model="selectedRoom.TotalHoras" placeholder="1:30" @input="validarNumero(selectedRoom.TotalHoras)">
              <span class="text-red-400 font-bold text-xs absolute -bottom-5 ">{{ numeroError }}</span> -->
            </div>
            
            <div class="col-span-3 flex justify-center items-center w-full">
              <button @click="reserveRoom" type="button"
                class="flex justify-center items-center w-2/4 text-xl shadow-lg h-16 mt-8 border-2 bg-gradient-to-r from-indigo-300 via-purple-400 to-purple-300 font-bold tracking-wider text-white hover:shadow-lg hover:shadow-purple-500/50 transition duration-150 ease-out md:ease-in border-purple-200 rounded-2xl ">Ocupar Habitacion</button>
            </div>
          </form>
          <button
            class="absolute text-xl  w-14 h-14 text-white -top-7 right-7 flex justify-center items-center border-2 bg-gradient-to-r from-indigo-300 via-purple-400 to-purple-300  hover:shadow-lg hover:shadow-purple-500/50 border-purple-200 rounded-full transition duration-150 ease-out md:ease-in"
            @click="$emit('close-modal')">X</button>
        </div>
      </Transition>
    </div>
  </Transition>
</template>
    
<script setup>
import InputNumber from 'primevue/inputnumber';
import { computed } from 'vue';
import { onMounted, ref } from 'vue';
import axiosClient from '../axiosClient';
const emits = defineEmits(["close-modal"])
const props = defineProps({
  room: Object,
});



onMounted(() => {
  selectedRoom.value.HabitacionID = props.room.habitacionId
})

let selectedRoom = ref({
  HabitacionID:0,
  FechaReserva:'',
  FechaFin:'',
  TotalHoras:0,
  UsuarioID:14,
  PatenteVehiculo:'',
  NumeroTelefono:'',
  Identificador:''
})


const hours = ref(0);
const minutes = ref(0);
const selectedTags = ref([]);
let editTagRel = {}
let cheatRefresh = ref(false);
let idNewTag = ref(0);
const handleCheat = (cheatIds) => {
  //le avisamos al componente DropDownTag que actualice para agregar los nuevos datos
  cheatRefresh.value = true
  idNewTag.value = cheatIds
  console.log("1 el numero", cheatIds)

};

console.log("tagselected : " + editTagRel.value)

//SECTOR DE VALIDACIONES DE FORMULARIO
let numeroError = ref('');

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

  // Sumar TotalHoras a FechaReserva
  const fechaFin = new Date(now.getTime() + selectedRoom.value.TotalHoras * 60 * 60 * 1000);
  const localFechaFin = new Date(fechaFin.getTime() - fechaFin.getTimezoneOffset() * 60000).toISOString().slice(0, -1);

  selectedRoom.value.FechaFin = localFechaFin;
};


//Reservar Habitacion
const reserveRoom = () => {
  actualizarFechas()
  if (numeroError.value || (selectedRoom.PatenteVehiculo == '' && selectedRoom.Identificador == '' && selectedRoom.NumeroTelefono == '')) {
    // No envíes el formulario si hay errores de validación
    console.log("faltan datos obligatorios perra")
    return;
  }
  console.log("loquese Envía",selectedRoom.value)
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
    