<template>
    <div class="flex overflow-auto">
      <!-- Panel Izquierdo: Habitaciones Libres -->
      <div class="w-1/2 p-4 flex flex-col items-center">
        <h2 class="text-xl text-white lexend-exa font-bold mb-4">HABITACIONES LIBRES</h2>
        <div class="grid grid-cols-1 sm:grid-cols-2 md:grid-cols-4 gap-4">
          <div v-for="habitacion in habitacionesLibres" :key="habitacion.habitacionId"
            @click="toggleModalLibre(habitacion)"
            class="p-3 border-4 bg-surface-800 rounded-md text-xs font-semibold shadow-sm text-white text-center cursor-pointer hover:bg-primary-400 border-primary-500">
            {{ habitacion.nombreHabitacion }}
          </div>
        </div>
      </div>
  
      <!-- Panel Derecho: Habitaciones Ocupadas -->
      <div class="w-1/2 p-4 flex flex-col items-center">
        <h2 class="text-xl text-white lexend-exa font-bold mb-4">HABITACIONES OCUPADAS</h2>
        <div class="grid grid-cols-1 sm:grid-cols-2 md:grid-cols-4 gap-4">
          <div v-for="habitacion in habitacionesOcupadas" :key="habitacion.habitacionId" 
            @click="toggleModal(habitacion)"
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
              class="absolute -top-2 -left-2 p-1 flex justify-center items-center rounded-full bg-red-600">
              <span class="material-symbols-outlined" style="font-size: 15px;">notifications_active</span>
            </div>
            {{ habitacion.nombreHabitacion }}
          </div>
        </div>
      </div>
    </div>
  
    <ReserveRoom :room="room" v-if="show" @close-modal="toggleModal"></ReserveRoom>
    <ReserveRoomLibre :room="room" v-if="showFree" @close-modal="toggleModalLibre"></ReserveRoomLibre>
  
    <div v-if="!authStore.auth">
      Please log in to view room information.
    </div>
  </template>
  
  <script setup>
  import { ref, computed, onMounted, onUnmounted } from 'vue';
  import axiosClient from '../axiosClient';
  import ReserveRoom from '../components/ReserveRoom.vue';
  import ReserveRoomLibre from '../components/ReserveRoomLibre.vue';
  import { useAuthStore } from '../store/auth.js';
  import { useWebSocketStore } from '../store/websocket.js';
  
  const habitacionesLibres = ref([]);
  const habitacionesOcupadas = ref([]);
  const room = ref(null);
  const show = ref(false);
  const showFree = ref(false);
  const authStore = useAuthStore();
  const websocketStore = useWebSocketStore();
  
  // Fetch room data initially
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
                  habitacionesLibres.value = data.data.filter(habitacion => habitacion.disponible === true);
                  habitacionesOcupadas.value = data.data.filter(habitacion => habitacion.disponible === false);
                  console.log("Libres:", habitacionesLibres.value);
                  console.log("Ocupadas:", habitacionesOcupadas.value);
              } else {
                  console.error('Invalid API data:', data);
              }
          })
          .catch(error => {
              console.error('Error fetching rooms:', error);
          });
  };
  
  // Helper function to get end time of a reservation
  const calculateReservationEnd = (fechaReserva, totalHoras, totalMinutos) => {
      const startDate = new Date(fechaReserva);
      return new Date(startDate.getTime() + (totalHoras * 60 + totalMinutos) * 60000);
  };
  
  // Function to determine room background color
  const getBackgroundColor = (fechaReserva, totalHoras, totalMinutos) => {
      console.log("🎨 Updating color dynamically...");
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
  
  // Compute background colors dynamically
  const occupiedRoomClasses = computed(() => {
      console.log("🟡 Recomputing room colors...");
      
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
      console.log("🔔 WebSocket event received:", data);
   
      if (data.type === "RoomUpdated") {
          console.log("✅ Updating room:", data.roomId);
  
          const index = habitacionesOcupadas.value.findIndex(h => h.habitacionId === data.roomId);
          if (index !== -1) {
              console.log("🔄 Updating reservaActiva for:", habitacionesOcupadas.value[index]);
  
              // Update only the reservaActiva field
              habitacionesOcupadas.value[index] = {
                  ...habitacionesOcupadas.value[index],
                  reservaActiva: data.reservaActiva
              };
  
              // Trigger Vue's reactivity manually
              habitacionesOcupadas.value = [...habitacionesOcupadas.value];
          } else {
              console.warn("⚠ Room not found in habitacionesOcupadas:", data.roomId);
          }
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
  
  // Toggle Modals
  const toggleModal = (Room) => {
      show.value = !show.value;
      room.value = Room;
      document.body.style.overflow = show.value ? 'hidden' : 'auto';
  };
  
  const toggleModalLibre = (Room) => {
      showFree.value = !showFree.value;
      room.value = Room;
      document.body.style.overflow = showFree.value ? 'hidden' : 'auto';
  };
  </script>
  