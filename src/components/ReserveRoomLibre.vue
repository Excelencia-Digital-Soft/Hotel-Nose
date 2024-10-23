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
                <!-- Form content for available room -->
                <div class="col-span-3 flex justify-center items-center w-full">
                  <button @click="reserveRoom" type="button" class="btn-primary w-2/4 h-16 rounded-2xl">Reservar Habitación</button>
                </div>
              </form>
              <button
                class="absolute text-xl w-14 h-14 text-white -top-7 right-7 btn-primary rounded-full"
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
  
  const emits = defineEmits(["close-modal"])
  const props = defineProps({
    room: Object,
  });
  
  
  onMounted(() => {
    selectedRoom.value.HabitacionID = props.room.habitacionId
    selectedRoom.value.Disponible = props.room.disponible
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
  let editTagRel = {}
  let cheatRefresh = ref(false);
  let idNewTag = ref(0);
  let numeroError = ref('');
  
  const toggleModalConfirm = () => {
    modalConfirm.value = !modalConfirm.value;
  }
  
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