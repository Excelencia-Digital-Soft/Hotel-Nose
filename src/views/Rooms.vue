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
    <ReserveRoom :room="room" v-if="show" @close-modal="toggleModal" @update-room="updateRoom" @update-tiempo="agregarTiempoExtra" @room-checkout="handleRoomCheckout">
    </ReserveRoom>
  
    <ReserveRoomLibre :room="room" v-if="showFree" @close-modal="toggleModalLibre">
    </ReserveRoomLibre>
  
    <div v-if="!authStore.auth">
      Please log in to view room information.
    </div>
  
  </template>
<script setup>
import { ref, onMounted, computed, onUnmounted } from 'vue';
import axiosClient from '../axiosClient';
import ReserveRoom from '../components/ReserveRoom.vue';
import ReserveRoomLibre from '../components/ReserveRoomLibre.vue';
import { useAuthStore } from '../store/auth.js';
import { useWebSocketStore } from '../store/websocket.js';

let habitaciones = [];
const habitacionesLibres = ref([]);
const habitacionesOcupadas = ref([]);
const room = ref(null);
const show = ref(false);
const showFree = ref(false);
const authStore = useAuthStore();
const websocketStore = useWebSocketStore();

const fetchHabitaciones = () => {
    console.log("🔄 Fetching habitaciones...");
    const institucionID = authStore.institucionID;

    if (!institucionID) {
        console.warn('InstitucionID is not available. Please ensure the user is logged in.');
        return;
    }

    axiosClient.get(`/GetHabitaciones?InstitucionID=${institucionID}`)
        .then(({ data }) => {
            if (data && data.data) {
                habitaciones = data.data;
                habitacionesLibres.value = habitaciones.filter(habitacion => habitacion.disponible === true);
                habitacionesOcupadas.value = habitaciones.filter(habitacion => habitacion.disponible === false);
                console.log("Libres", habitacionesLibres.value);
                console.log("Ocupadas", habitacionesOcupadas.value);
            } else {
                console.error('Datos de la API no válidos:', data);
            }
        })
        .catch(error => {
            console.error('Error al obtener las habitaciones:', error);
        });
};
// Recalculate room background colors
const calculateReservationEnd = (fechaReserva, totalHoras, totalMinutos) => {
    const startDate = new Date(fechaReserva);
    return new Date(startDate.getTime() + (totalHoras * 60 + totalMinutos) * 60000);
};

const getBackgroundColor = (fechaReserva, totalHoras, totalMinutos) => {
    console.log("Colores!")
    const now = new Date();
    const endDate = calculateReservationEnd(fechaReserva, totalHoras, totalMinutos);
    const timeLeft = endDate.getTime() - now.getTime();

    if (timeLeft <= 0) {
        return 'bg-red-500';
    } else if (timeLeft <= 15 * 60000) {
        return 'bg-yellow-500';
    } else {
        return 'bg-surface-800';
    }
};

// Compute background color dynamically
const occupiedRoomClasses = computed(() => {
    return habitacionesOcupadas.value.reduce((acc, habitacion) => {
        if (habitacion.reservaActiva) {
            acc[habitacion.habitacionId] = getBackgroundColor(
                habitacion.reservaActiva.fechaReserva,
                habitacion.reservaActiva.totalHoras,
                habitacion.reservaActiva.totalMinutos
            );
        } else {
            acc[habitacion.habitacionId] = 'bg-surface-800';
        }
        return acc;
    }, {});
});

// WebSocket event handler
const handleWebhookEvent = (data) => {
    console.log("Webhook event received in this component:", data);

    // Check if the notification is about a room update
    if (data.type === "warning" || data.type == "ended") {
        fetchHabitaciones(); // Refresh room status when a relevant event occurs
    }
};

onMounted(() => {
    fetchHabitaciones();
    console.log("🔹 Registering WebSocket event listener in RoomComponent");
    websocketStore.registerEventCallback("RoomComponent", handleWebhookEvent);
});

onUnmounted(() => {
    console.log("❌ Unregistering WebSocket event listener in RoomComponent");
    websocketStore.unregisterEventCallback("RoomComponent");
});

const agregarTiempoExtra = (reservaID, horas, minutos) => {
  const index = habitacionesOcupadas.value.findIndex(h => h.reservaActiva.reservaId === reservaID);
  if (index === -1) {
    console.warn(`No se encontró una habitación con reservaID ${reservaID}`);
    return;
  }

  const habitacion = habitacionesOcupadas.value[index];

  let newHoras = habitacion.reservaActiva.totalHoras + horas;
  let newMinutos = habitacion.reservaActiva.totalMinutos + minutos;

  while (newMinutos >= 60) {
    newHoras += 1;
    newMinutos -= 60;
  }

  habitacionesOcupadas.value[index] = {
    ...habitacion, 
    reservaActiva: {
      ...habitacion.reservaActiva,
      totalHoras: newHoras,
      totalMinutos: newMinutos
    }
  };
  console.log(habitacionesOcupadas.value[index]);
};

const updateRoom = (updatedRoom) => {
  console.log('🔄 Updating room:', updatedRoom);
  
  // Find the room in occupied rooms
  const occupiedIndex = habitacionesOcupadas.value.findIndex(h => h.habitacionId === updatedRoom.habitacionId);
  
  if (occupiedIndex !== -1) {
    // Update the room in occupied rooms
    habitacionesOcupadas.value[occupiedIndex] = { ...habitacionesOcupadas.value[occupiedIndex], ...updatedRoom };
    console.log('✅ Room updated in occupied rooms');
    return;
  }
  
  // Find the room in free rooms
  const freeIndex = habitacionesLibres.value.findIndex(h => h.habitacionId === updatedRoom.habitacionId);
  
  if (freeIndex !== -1) {
    // Update the room in free rooms
    habitacionesLibres.value[freeIndex] = { ...habitacionesLibres.value[freeIndex], ...updatedRoom };
    console.log('✅ Room updated in free rooms');
    return;
  }
  
  console.warn('⚠️ Room not found for update:', updatedRoom.habitacionId);
};

const handleRoomCheckout = (roomId) => {
  console.log('🏠 Handling room checkout for room:', roomId);
  
  // Find the room in occupied rooms
  const occupiedIndex = habitacionesOcupadas.value.findIndex(h => h.habitacionId === roomId);
  
  if (occupiedIndex !== -1) {
    const room = habitacionesOcupadas.value[occupiedIndex];
    
    // Remove from occupied rooms
    habitacionesOcupadas.value.splice(occupiedIndex, 1);
    
    // Add to free rooms (reset the room state)
    const freeRoom = {
      ...room,
      disponible: true,
      reservaActiva: null,
      visita: null,
      visitaID: null,
      pedidosPendientes: false
    };
    
    habitacionesLibres.value.push(freeRoom);
    
    console.log('✅ Room moved from occupied to free');
    
    // Close the modal
    show.value = false;
    document.body.style.overflow = 'auto';
  } else {
    console.warn('⚠️ Room not found in occupied rooms for checkout:', roomId);
  }
};

// Toggle Modals
function toggleModal(Room) {
    show.value = !show.value;
    room.value = Room;
    document.body.style.overflow = show.value ? 'hidden' : 'auto';
}

function toggleModalLibre(Room) {
    showFree.value = !showFree.value;
    room.value = Room;
    console.log("selecciono habitacion:",Room)
    document.body.style.overflow = showFree.value ? 'hidden' : 'auto';
}

</script>

<style>
/* Aquí puedes añadir estilos adicionales si es necesario */
</style>