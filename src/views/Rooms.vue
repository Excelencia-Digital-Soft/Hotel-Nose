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
    :class="[
        'relative',
        'flex',
        'justify-center',
        'items-center',
        'py-3',
        'px-4',
        'border-4',
        'rounded-md',
        'text-xs',
        'font-semibold',
        'shadow-sm',
        'text-white',
        'text-center',
        'cursor-pointer',
        'hover:bg-secondary-400',
        'border-secondary-500',
        occupiedRoomClasses[habitacion.habitacionId] // Dynamic background color
    ]">
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
import { ref, onMounted, computed } from 'vue' // Import computed
import axiosClient from '../axiosClient';
import ReserveRoom from '../components/ReserveRoom.vue';
import ReserveRoomLibre from '../components/ReserveRoomLibre.vue';
import { useAuthStore } from '../store/auth.js';

let habitaciones = []
const habitacionesLibres = ref([])
const habitacionesOcupadas = ref([])
const room = ref(null);
const show = ref(false)
const showFree = ref(false)

const authStore = useAuthStore();

const fetchHabitaciones = () => {
    const institucionID = authStore.institucionID;

    if (institucionID == null) {
        console.warn('InstitucionID is not available.  Please ensure the user is logged in.');
        return;
    }

    axiosClient.get(`/GetHabitaciones?InstitucionID=${institucionID}`)
        .then(({ data }) => {
            if (data && data.data) {
                habitaciones = data.data;
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

function updateRoom(updatedRoom) { }
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

onMounted(fetchHabitaciones)

// Helper function to calculate reservation end time
const calculateReservationEnd = (fechaReserva, totalHoras, totalMinutos) => {
    const startDate = new Date(fechaReserva);
    const endDate = new Date(startDate.getTime() + (totalHoras * 60 + totalMinutos) * 60000); // Convert hours and minutes to milliseconds
    return endDate;
};

// Helper function to determine background color
const getBackgroundColor = (fechaReserva, totalHoras, totalMinutos) => {
    const now = new Date();
    const endDate = calculateReservationEnd(fechaReserva, totalHoras, totalMinutos);
    const timeLeft = endDate.getTime() - now.getTime(); // Time left in milliseconds

    if (timeLeft <= 0) {
        return 'bg-red-500'; // Past the end time
    } else if (timeLeft <= 15 * 60000) { // 15 minutes in milliseconds
        return 'bg-yellow-500'; // Within 15 minutes of end time
    } else {
        return 'bg-surface-800'; // Default background
    }
};

// Computed property to dynamically apply background color class
const occupiedRoomClasses = computed(() => {
    return habitacionesOcupadas.value.reduce((acc, habitacion) => {
        if (habitacion.reservaActiva) {
            acc[habitacion.habitacionId] = getBackgroundColor(
                habitacion.reservaActiva.fechaReserva,
                habitacion.reservaActiva.totalHoras,
                habitacion.reservaActiva.totalMinutos
            );
        } else {
            acc[habitacion.habitacionId] = 'bg-surface-800'; // Default if no reservaActiva
        }
        return acc;
    }, {});
});
</script>

<style>
/* Aquí puedes añadir estilos adicionales si es necesario */
</style>
