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
            {{ selectedRoom.nombreHabitacion }}
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
            <section/>
            <section v-if="!selectedRoom.Disponible" class="p-10">
              <div>
  <h1 class="text-xl text-white font-bold mb-4">Consumos</h1>
  <div class="bg-gray-800 rounded-lg p-4 max-h-64 overflow-y-auto">
    <!-- Header row -->
    <div class="grid grid-cols-[2fr_1fr_1fr_1fr_1fr] gap-4 text-white font-semibold mb-2">
      <span>Producto</span>
      <span>Cantidad</span>
      <span>Precio</span>
      <span>Origen</span>
      <span>Total</span>
    </div>
    
    <!-- Ordered list for the consumos -->
    <ol class="space-y-2">
      <li 
        v-for="consumo in consumos" 
        :key="consumo.consumoId" 
        class="grid grid-cols-[2fr_1fr_1fr_1fr_1fr] gap-4 bg-gray-700 p-3 rounded-md text-white"
      >
        <span class="font-semibold">{{ consumo.articleName }}</span>
        <span class="text-sm text-gray-400">{{ consumo.cantidad }}</span>
        <span class="text-sm text-gray-400">${{ consumo.precioUnitario }}</span>
        <span class="text-sm text-gray-400">
          {{ consumo.esHabitacion ? 'Habitacion' : 'Inv. General' }}
        </span>
        <span class="text-sm font-bold text-green-400">${{ consumo.total }}</span>
      </li>
    </ol>
  </div>
</div>
  

  <button type="button" 
          @click="toggleModalConfirm()"
          class="btn-primary w-full h-12 text-base font-semibold tracking-wider rounded-3xl mt-4">
    Consumo general
  </button>
  <button type="button" 
          @click="toggleModalConfirmHabitacion()"
          class="btn-primary w-full h-12 text-base font-semibold tracking-wider rounded-3xl mt-4">
    Consumo habitación
  </button>
  
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
              <button @click="endRoomReserve" type="button"
                class="btn-primary w-2/4 h-16 rounded-2xl ">Desocupar Habitacion</button>
            </div>
          </form>
          <button
            class="absolute text-xl  w-14 h-14 text-white -top-7 right-7 btn-primary rounded-full transition duration-150 ease-out md:ease-in"
            @click="$emit('close-modal')">X</button>
          <ModalConfirm v-if="modalConfirm" 
          :name="selectedRoom.Identificador" 
          @confirmaAccion="confirmAndSend"
          @close="toggleModalConfirm"/>

          <ModalConfirmHabitacion v-if="modalConfirmHabitacion" 
          :name="selectedRoom.Identificador" 
          :habitacionID="selectedRoom.HabitacionID"
          @confirmaAccion="confirmAndSendHabitacion"
          @close="toggleModalConfirmHabitacion"/> 
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
import ModalConfirmHabitacion from './ModalConfirmHabitacion.vue';

const emits = defineEmits(["close-modal"])
const props = defineProps({
  room: Object,
});

onMounted(() => {
  selectedRoom.value.nombreHabitacion = props.room.nombreHabitacion;
  selectedRoom.value.HabitacionID = props.room.habitacionId;
  selectedRoom.value.Disponible = props.room.disponible;
  selectedRoom.value.VisitaID = props.room.visitaID; // Safe access
  selectedRoom.value.Identificador = props.room.visita?.identificador; // Safe access
  selectedRoom.value.NumeroTelefono = props.room.visita?.numeroTelefono; // Safe access
  selectedRoom.value.PatenteVehiculo = props.room.visita?.patenteVehiculo; // Safe access
  console.log(selectedRoom.value.HabitacionID)
  setCurrentDateTime();
  actualizarConsumos();
  document.body.style.overflow = 'hidden';
})

let selectedRoom = ref({
  HabitacionID: 0,
  Disponible:null,
  nombreHabitacion: '',
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
let modalConfirmHabitacion = ref(false);

const currentDate = ref('');
const currentTime = ref('');
const pizza = ref();
const products = ref();
const hours = ref(0);
const minutes = ref(0);
const selectedTags = ref([]);
const consumos = ref([]);
let editTagRel = {}
let cheatRefresh = ref(false);
let idNewTag = ref(0);
let numeroError = ref('');
/* const addToConsumos = (selectedItems) => {
  // Add the selected items to the consumos list
  selectedItems.forEach(item => {
    const total = item.cantidad * item.precio; // Use 'precio' for total calculation

    // Check if the item already exists in the consumos list
    const existingItem = consumos.value.find(consumo => consumo.articuloId === item.articuloId);

    if (existingItem) {
      // If the item exists, update its quantity and total
      existingItem.cantidad += item.cantidad;
      existingItem.total += total;
    } else {
      // If the item does not exist, add it as a new entry
      consumos.value.push({
        consumoId: Date.now(), // Generate a unique ID for the consumption item
        articuloId: item.articuloId,
        articleName: item.nombreArticulo,
        cantidad: item.cantidad,
        precioUnitario: item.precio,
        total
      });
    }
  });
}; */

const agregarConsumos = (selectedItems) => {
  console.log(selectedItems);
  axiosClient.post(
    `/ConsumoGeneral?habitacionId=${selectedRoom.value.HabitacionID}&visitaId=${selectedRoom.value.VisitaID}`,
    selectedItems // Send selectedItems directly as the body
  )
  .then(response => {
    actualizarConsumos();
    console.log('Consumo agregado exitosamente:', response.data);
  })
  .catch(error => {
    console.error('Error al agregar consumo:', error);
  });
};

const agregarConsumosHabitacion = (selectedItems) => {
  console.log(selectedItems);
  axiosClient.post(
    `/ConsumoHabitacion?habitacionId=${selectedRoom.value.HabitacionID}&visitaId=${selectedRoom.value.VisitaID}`,
    selectedItems // Send selectedItems directly as the body
  )
  .then(response => {
    actualizarConsumos();
    console.log('Consumo agregado exitosamente:', response.data);
  })
  .catch(error => {
    console.error('Error al agregar consumo:', error);
  });
};

const actualizarConsumos = () => {
  axiosClient.get(`/GetConsumosVisita?VisitaID=${selectedRoom.value.VisitaID}`)
    .then(({ data }) => {
      if (data && data.data) {
        consumos.value = []; // Clear the array before adding new data
        data.data.forEach(item => {
          // Check if the item already exists in the consumos list
          const existingItem = consumos.value.find(consumo => consumo.articuloId === item.articuloId && consumo.esHabitacion === item.esHabitacion);

          if (existingItem) {
            // Update the existing item’s quantity and recalculate subtotal if it exists
            existingItem.cantidad += item.cantidad;
            existingItem.total = existingItem.cantidad * existingItem.precioUnitario; // Recalculate subtotal
          } else {
            // Add new item if it doesn't already exist
            consumos.value.push({
              consumoId: item.consumoId,
              articuloId: item.articuloId,
              articleName: item.articleName,
              cantidad: item.cantidad,
              precioUnitario: item.precioUnitario,
              esHabitacion: item.esHabitacion,
              total: item.cantidad * item.precioUnitario // Initial subtotal
            });
          }
          console.log(data.data)
        });
      } else {
        console.error('Datos de la API no válidos:', data);
      }
    })
    .catch(error => {
      console.error('Error al obtener los consumos:', error);
    });
};
const toggleModalConfirm = () => {
  modalConfirm.value = !modalConfirm.value;
}
const toggleModalConfirmHabitacion = () => {
  modalConfirmHabitacion.value = !modalConfirmHabitacion.value;
}
const confirmAndSend = (ConfirmedArticles) =>{
  
  console.log(JSON.stringify(ConfirmedArticles) +" Llegamos al ReserveROOM");
  agregarConsumos(ConfirmedArticles)
}

const confirmAndSendHabitacion = (ConfirmedArticles) =>{
  
  console.log(JSON.stringify(ConfirmedArticles) +" Llegamos al ReserveROOM");
  console.log(ConfirmedArticles)
  if(ConfirmedArticles.length > 0){
  agregarConsumosHabitacion(ConfirmedArticles)
}
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
const endRoomReserve = () => {
  axiosClient.put(`/FinalizarReserva?idHabitacion=${selectedRoom.value.HabitacionID}`)
  .then(res => {
      console.log(res.data);
      alert("Se terminó la reserva exitosamente");
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