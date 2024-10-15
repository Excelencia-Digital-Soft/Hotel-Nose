<template>
  <div class="p-4">
    <!-- Form to Create a New Room -->
    <div class="mb-6">
      <h2 class="text-xl text-white lexend-exa font-bold mb-4">Crear habitación</h2>
      <form @submit.prevent="createRoom">
        <div class="mb-4">
          <label class="block text-white text-sm font-bold mb-2" for="roomName">Nombre:</label>
          <input
            v-model="newRoomName"
            class="shadow appearance-none border rounded w-full py-2 px-3 text-gray-700 leading-tight focus:outline-none focus:shadow-outline"
            id="roomName"
            type="text"
            placeholder="Nombre de la habitación"
            required
          >
        </div>
        <div class="mb-4">
          <label class="block text-white text-sm font-bold mb-2">Categoría:</label>
          <div class="flex flex-wrap gap-2">
            <button
              v-for="categoria in categorias"
              :key="categoria.categoriaId"
              @click="selectCategory(categoria.categoriaId)"
              :class="{
                'bg-primary-500 text-white': selectedCategory === categoria.categoriaId,
                'bg-gray-300': selectedCategory !== categoria.categoriaId
              }"
              class="py-2 px-4 rounded"
            >
              {{ categoria.nombreCategoria }}
            </button>
          </div>
        </div>
        <button
          class="bg-primary-500 text-white py-2 px-4 rounded hover:bg-primary-700"
          type="submit"
        >
          Crear
        </button>
      </form>
    </div>

    <!-- List of Rooms -->
    <div>
      <h2 class="text-xl text-white lexend-exa font-bold mb-4">Habitaciones</h2>
      <div class="grid grid-cols-3 gap-4">
        <div
          v-for="habitacion in habitaciones"
          :key="habitacion.habitacionId"
          class="p-4 border-4 bg-surface-800 rounded-md text-lg font-semibold shadow-sm text-white text-center"
        >
          {{ habitacion.nombreHabitacion }}
          <button
            @click="deleteRoom(habitacion.habitacionId)"
            class="bg-red-500 text-white px-2 py-1 mt-2 rounded hover:bg-red-700"
          >
            Borrar
          </button>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import axiosClient from '../axiosClient'; // Adjust the path according to your project structure

const habitaciones = ref([]);
const categorias = ref([]);
const newRoomName = ref('');
const selectedCategory = ref(null);

// Fetch rooms and categories initially
const fetchHabitaciones = () => {
  axiosClient.get("/GetHabitaciones")
    .then(({ data }) => {
      if (data && data.ok) {
        habitaciones.value = data.data;
      } else {
        console.error('Failed to fetch rooms:', data.message);
      }
    })
    .catch(error => {
      console.error('Error fetching rooms:', error);
    });
};

const fetchCategorias = () => {
  axiosClient.get("/GetCategorias")
    .then(({ data }) => {
      if (data && data.ok) {
        categorias.value = data.data;
      } else {
        console.error('Failed to fetch categories:', data.message);
      }
    })
    .catch(error => {
      console.error('Error fetching categories:', error);
    });
};

// Create a new room
const createRoom = async () => {
  if (selectedCategory.value && newRoomName.value) {
    try {
      // Make sure to format the URL correctly
      const response = await axiosClient.post(
        `/CrearHabitacion?nombreHabitacion=${encodeURIComponent(newRoomName.value)}&categoriaID=${selectedCategory.value}`
      );
      alert(response.data.message);
      fetchHabitaciones(); // Refresh the list of rooms
      // Reset the fields after the request
      newRoomName.value = '';
      selectedCategory.value = null;
      await fetchCategorias(); // Optionally refresh categories if needed
    } catch (error) {
      console.error('Error creating room:', error);
      alert('Error creating room');
    }
  } else {
    alert("Por favor ingresa el nombre y la categoría.");
  }
};

// Delete a room
const deleteRoom = (idHabitacion) => {
  axiosClient.delete(`/AnularHabitacion?idHabitacion=${idHabitacion}&Estado=true`)
    .then(({ data }) => {
      if (data.ok) {
        alert("Habitación eliminada correctamente.");
        fetchHabitaciones(); // Refresh the list of rooms
      } else {
        alert("Failed to delete room: " + data.message);
      }
    })
    .catch(error => {
      console.error('Error deleting room:', error);
    });
};

// Select a category
const selectCategory = (categoriaId) => {
  selectedCategory.value = categoriaId;
};

// Fetch rooms and categories on component mount
onMounted(() => {
  fetchHabitaciones();
  fetchCategorias();
});
</script>

<style scoped>
/* Add custom styles if needed */
</style>
