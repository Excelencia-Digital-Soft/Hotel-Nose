<template>
  <div class="flex">
    <!-- Panel Izquierdo: Habitaciones Libres -->
    <div class="w-1/2 p-4 flex flex-col items-center">
      <h2 class="text-xl text-white lexend-exa font-bold mb-4">HABITACIONES LIBRES</h2>
      <div class="grid grid-cols-3 gap-4">
        <div
          v-for="habitacion in habitacionesLibres"
          :key="habitacion.habitacionId"
          @click="toggleModal(habitacion)"
          class="p-4 border-4 bg-surface-800  rounded-md text-lg font-semibold shadow-sm text-white text-center cursor-pointer hover:bg-primary-400 border-primary-500"
        >
          {{ habitacion.nombreHabitacion }}
        </div>
      </div>
    </div>

    <!-- Panel Derecho: Habitaciones Ocupadas -->
    <div class="w-1/2 p-4 flex flex-col items-center">
      <h2 class="text-xl text-white lexend-exa font-bold mb-4">HABITACIONES OCUPADAS</h2>
      <div class="grid grid-cols-3 gap-4">
        <div
          v-for="habitacion in habitacionesOcupadas"
          :key="habitacion.habitacionId"
          class="p-4 border-4 bg-surface-800  rounded-md text-lg font-semibold shadow-sm text-white text-center cursor-pointer hover:bg-secondary-400 border-secondary-500"
        >
          {{ habitacion.nombreHabitacion }}
        </div>
      </div>
    </div>
  </div>
  <ReserveRoom
            :room="room" 
            v-if="show" 
            @close-modal="toggleModal">
  </ReserveRoom>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import axiosClient from '../axiosClient'; // Ajusta la ruta según tu estructura de proyecto
import ReserveRoom from '../components/ReserveRoom.vue';
let habitaciones = []
const habitacionesLibres = ref([])
const habitacionesOcupadas = ref([])
const room = ref(null);
const show = ref(false)
const fetchHabitaciones = () => {
  axiosClient.get("/GetHabitaciones")
    .then(({ data }) => {
      if (data && data.data) {
        habitaciones= data.data;

        // Dividir habitaciones en libres y ocupadas
        habitacionesLibres.value = habitaciones.filter(habitacion => habitacion.disponible === true);
        habitacionesOcupadas.value = habitaciones.filter(habitacion => habitacion.disponible === false);
      } else {
        console.error('Datos de la API no válidos:', data);
      }
    })
    .catch(error => {
      console.error('Error al obtener las habitaciones:', error);
    });
}

function toggleModal(Room){
    show.value = !show.value
    room.value=Room
}
// Llamar a la API al montar el componente
onMounted(fetchHabitaciones)
</script>

<style>
/* Aquí puedes añadir estilos adicionales si es necesario */
</style>