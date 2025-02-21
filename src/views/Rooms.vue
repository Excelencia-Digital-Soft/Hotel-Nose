<template>
  <div class="flex overflow-auto">
    <!-- Panel Izquierdo: Habitaciones Libres -->
    <div class="w-1/2 p-4 flex flex-col items-center">
      <h2 class="text-xl text-white lexend-exa font-bold mb-4">HABITACIONES LIBRES</h2>
      <div class="grid grid-cols-1 sm:grid-cols-2 md:grid-cols-4  gap-4">
        <div v-for="habitacion in habitacionesLibres" :key="habitacion.habitacionId"
          @click="toggleModalLibre(habitacion)"
          class="p-3 border-4 bg-surface-800  rounded-md text-xs font-semibold shadow-sm text-white text-center cursor-pointer hover:bg-primary-400 border-primary-500">
          {{ habitacion.nombreHabitacion }}
        </div>
      </div>
    </div>

    <!-- Panel Derecho: Habitaciones Ocupadas -->
    <div class="w-1/2 p-4 flex flex-col items-center">
      <h2 class="text-xl text-white lexend-exa font-bold mb-4">HABITACIONES OCUPADAS</h2>
      <div class="grid grid-cols-1 sm:grid-cols-2 md:grid-cols-4 gap-4">
        <div v-for="habitacion in habitacionesOcupadas" :key="habitacion.habitacionId" @click="toggleModal(habitacion)"
          class="relative flex justify-center items-center py-3 px-4 border-4 bg-surface-800  rounded-md text-xs font-semibold shadow-sm text-white text-center cursor-pointer hover:bg-secondary-400 border-secondary-500">
          <div v-if="habitacion.pedidosPendientes"
            class="absolute -top-2 -left-2 p-1 flex justify-center items-center rounded-full bg-red-600"><span
              class="material-symbols-outlined" style="font-size: 15px;">
              notifications_active
            </span></div>
          {{ habitacion.nombreHabitacion }}
        </div>
      </div>
    </div>
  </div>
  <ReserveRoom :room="room" v-if="show" @close-modal="toggleModal" @update-room="updateRoom">
  </ReserveRoom>

  <ReserveRoomLibre :room="room" v-if="showFree" @close-modal="toggleModalLibre">
  </ReserveRoomLibre>

  <div v-if="!authStore.auth">
    Please log in to view room information.
  </div>

</template>

<script setup>
import { ref, onMounted } from 'vue'
import axiosClient from '../axiosClient'; // Ajusta la ruta según tu estructura de proyecto
import ReserveRoom from '../components/ReserveRoom.vue';
import ReserveRoomLibre from '../components/ReserveRoomLibre.vue';
import { useAuthStore } from '../store/auth.js'; // Import the auth store

let habitaciones = []
const habitacionesLibres = ref([])
const habitacionesOcupadas = ref([])
const room = ref(null);
const show = ref(false)
const showFree = ref(false)

// Access the auth store
const authStore = useAuthStore();

const fetchHabitaciones = () => {
  // Use the institucionID from the auth store
  const institucionID = authStore.auth?.institucionID;

  if (institucionID == null) {
    console.warn('InstitucionID is not available.  Please ensure the user is logged in.');
    // Optionally, you could redirect the user to the login page here.
    return; // Or handle the case where the user is not logged in appropriately
  }

  axiosClient.get(`/GetHabitaciones?InstitucionID=${institucionID}`)
    .then(({ data }) => {
      if (data && data.data) {
        habitaciones = data.data;

        // Dividir habitaciones en libres y ocupadas
        habitacionesLibres.value = habitaciones.filter(habitacion => habitacion.disponible === true);
        habitacionesOcupadas.value = habitaciones.filter(habitacion => habitacion.disponible === false);
        console.log("Libres", habitacionesLibres.value)
        console.log("OCUPADAS", habitacionesOcupadas.value)
      } else {
        console.error('Datos de la API no válidos:', data);
      }
    })
    .catch(error => {
      console.error('Error al obtener las habitaciones:', error);
    });
}

function updateRoom(updatedRoom) {

}
function toggleModal(Room) {
  show.value = !show.value
  room.value = Room
  
  document.body.style.overflow = !show.value ? 'auto' : 'hidden';
  console.log(room.value)
}
function toggleModalLibre(Room) {
  showFree.value = !showFree.value
  room.value = Room
  document.body.style.overflow = 'hidden';
}
// Llamar a la API al montar el componente
onMounted(fetchHabitaciones)
</script>

<style>
/* Aquí puedes añadir estilos adicionales si es necesario */
</style>
