<template>
  <div class="flex">
    <!-- Panel Izquierdo: Habitaciones Libres -->
    <div class="w-1/2 p-4">
      <h2 class="text-lg text-white font-bold mb-4">Habitaciones Libres</h2>
      <div class="grid grid-cols-3 gap-4">
        <div
          v-for="habitacion in habitacionesLibres"
          :key="habitacion.habitacionId"
          @click="toggleModal(habitacion)"
          class="p-4 border bg-green-500 rounded-md shadow-sm text-center cursor-pointer hover:bg-green-400"
        >
          {{ habitacion.nombreHabitacion }}
        </div>
      </div>
    </div>

    <!-- Panel Derecho: Habitaciones Ocupadas -->
    <div class="w-1/2 p-4">
      <h2 class="text-lg text-white font-bold mb-4">Habitaciones Ocupadas</h2>
      <div class="grid grid-cols-3 gap-4">
        <div
          v-for="habitacion in habitacionesOcupadas"
          :key="habitacion.habitacionId"
          class="p-4 border bg-red-500 rounded-md shadow-sm text-center"
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