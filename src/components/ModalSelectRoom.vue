<template>
  <Teleport v-if="selectedRoom" to="body" class="overflow-hidden">
  <Transition name="modal-outer" appear>
    <div 
      class="fixed w-full h-full overflow-auto z-20 bg-black bg-opacity-40 backdrop-blur-lg top-0 left-0 flex justify-center px-8">
          <Transition name="modal-inner">
      <!-- Panel de la habitación seleccionada con Slider -->

      <div  class="flex flex-col justify-center items-center  p-4  rounded-lg w-10/12 h-auto text-center">
        <!-- <h3 class="text-3xl font-semibold text-white ">{{ selectedRoom.nombre }}</h3> -->
        
        

        <!-- Carrusel de imágenes -->
       <h3 class="text-3xl font-semibold text-white ">{{ selectedRoom.nombreHabitacion }}</h3>
        <Swiper :modules="[Navigation, Pagination]" navigation pagination class="mt-4 w-full h-full text-white">
          <SwiperSlide v-for="(img, index) in selectedRoom.imagenes" :key="index">
            <img :src="img" class="w-full h-full object-cover rounded-lg" />
          </SwiperSlide>
        </Swiper>
        <div class="flex m-8">
        <ul class="mt-2 list-disc pl-4 text-gray-300 text-left mr-4">
          <li class="flex" v-for="(detalle, index) in caract" :key="index">
            <img v-if="detalle.icono" :src="detalle.icono" alt="Icono" class="w-6 h-6 mr-2" />
            {{ detalle.nombre }}
          </li>
        </ul>
        <ReserveRoomLibre class=" " :room="props.selectedRoom" v-if="showFree" @close-modal="toggleModal" @room-reserved="handleRoomReserved"/> 
        <button class="btn-third font-semibold rounded-lg text-2xl p-8"
        @click="toggleModal()">Alquilar ${{ selectedRoom.precio }}</button>
      </div>
        <button type="button"
                class="btn-danger absolute top-6 right-4 z-10 md:right-16 text-md w-20 h-20 rounded-3xl transition-colors border-2 border-purple-200"
                @click="emits('close')">X</button>
      
      </div>
    </Transition>
    </div>
  </Transition>
  </Teleport>
</template>

<script setup>
import { Swiper, SwiperSlide } from 'swiper/vue';
import { ref,watch } from 'vue';
import { Navigation, Pagination } from 'swiper/modules';
import 'swiper/css';
import 'swiper/css/navigation';
import 'swiper/css/pagination';
import { getCharacteristicImage } from '../services/imageService';
import ReserveRoomLibre from './ReserveRoomLibre.vue';
const props = defineProps({
  selectedRoom: Object,
})

const emits = defineEmits(['close', 'room-reserved'])

const caract = ref([])
const showFree = ref(false);
function toggleModal() {
    showFree.value = !showFree.value;
    document.body.style.overflow = showFree.value ? 'hidden' : 'auto';
}

const handleRoomReserved = (roomId) => {
  console.log('🏠 Room reserved in ModalSelectRoom:', roomId);
  // Close the local modal
  showFree.value = false;
  document.body.style.overflow = 'auto';
  // Emit to parent component
  emits('room-reserved', roomId);
}

// const roomToOccupied = ref(
// {
//   "habitacionID": props.selectedRoom.Id,
//   "promocionID": 0,
//   "fechaReserva": "2025-03-31T16:45:14.986Z",
//   "fechaFin": "2025-03-31T16:45:14.986Z",
//   "totalHoras": 0,
//   "totalMinutos": 0,
//   "usuarioID": 0,
//   "esReserva": true,
//   "patenteVehiculo": "string",
//   "numeroTelefono": "string",
//   "identificador": "string"
// });
// const actualizarFechas = () => {
//   const now = new Date();

//   // Convertir la fecha y hora actual a la zona horaria local
//   const localFechaReserva = new Date(now.getTime() - now.getTimezoneOffset() * 60000).toISOString().slice(0, -1);

//   roomToOccupied.value.FechaReserva = localFechaReserva;
//   // Sumar una hora más
//   const fechaConUnaHoraMas = new Date(now);
//   fechaConUnaHoraMas.setHours(fechaConUnaHoraMas.getHours() + 1);

//   // Convertir la nueva fecha con una hora adicional a la zona horaria local
//   const localFechaReservaConUnaHoraMas = new Date(fechaConUnaHoraMas.getTime() - fechaConUnaHoraMas.getTimezoneOffset() * 60000).toISOString().slice(0, -1);

//   // Aquí puedes asignar la nueva fecha si es necesario
//   selectedRoom.value.FechaFin = localFechaReservaConUnaHoraMas;
// };


// //Reservar Habitacion
// const reserveRoom = () => {
//   actualizarFechas()
//   if (numeroError.value || (selectedRoom.PatenteVehiculo == '' && selectedRoom.Identificador == '' && selectedRoom.NumeroTelefono == '')) {
//     // No envíes el formulario si hay errores de validación
//     console.log("faltan datos obligatorios")
//     return;
//   }

//   axiosClient.post(`/ReservarHabitacion?InstitucionID=${InstitucionID.value}&UsuarioID=${UsuarioID.value}`, selectedRoom.value)
//     .then(res => {
//       console.log(res.data);
//       alert("Reservacion Exitosa");
//       emits('close-modal');
//       window.location.reload();
//     })
//     .catch(error => {
//       console.error(error);
//     });
// }

const cargarIconos = async (lista) => {
  return await Promise.all(
    lista.map(async (caracteristica) => {
      if (caracteristica.icono) {
        try {
          const response = await getCharacteristicImage(caracteristica.caracteristicaId);
          return {
            ...caracteristica,
            icono: URL.createObjectURL(response.data)
          };
        } catch (error) {
          console.error(`Error al cargar el icono: ${error}`);
          return {
            ...caracteristica,
            icono: null
          };
        }
      }
      return caracteristica;
    })
  );
};
// Watcher para listaCaracteristicas
watch(() => props.selectedRoom.caracteristicas, async (newList) => {
  console.log("ESTA es :",props.selectedRoom)
  console.log(newList)
  if (newList && newList.length > 0) {
    caract.value = await cargarIconos(newList);
  } else {
    caract.value = []
  }
}, { immediate: true });
</script>