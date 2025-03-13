<template>
  <div class="p-4">
    <!-- Form for Creating/Updating a Room -->
    <div class="mb-6">
      <h2 class="text-xl text-white lexend-exa font-bold mb-4">
        {{ isUpdateMode ? 'Actualizar Habitación' : 'Crear Habitación' }}
      </h2>
      <form @submit.prevent="submitForm">
        <div class="mb-4">
          <label class="block text-white text-sm font-bold mb-2" for="roomName">Nombre:</label>
          <input v-model="roomName"
            class="shadow appearance-none border rounded w-full py-2 px-3 text-gray-700 leading-tight focus:outline-none focus:shadow-outline"
            id="roomName" type="text" placeholder="Nombre de la habitación" required>
        </div>

        <!-- Categoría -->
        <div class="mb-4">
          <label class="block text-white text-sm font-bold mb-2">Categoría:</label>
          <div class="flex flex-wrap gap-2">
            <button v-for="categoria in categorias" :key="categoria.categoriaId"
              @click.prevent="selectCategory(categoria.categoriaId)" :class="{
                'bg-primary-500 text-white': selectedCategory === categoria.categoriaId,
                'bg-gray-300': selectedCategory !== categoria.categoriaId
              }" class="py-2 px-4 rounded">
              {{ categoria.nombreCategoria }}
            </button>
          </div>
        </div>

        <!-- Seleccionar imágenes -->
        <div class="mb-4">
          <label class="block text-white text-sm font-bold mb-2">Imágenes:</label>
          <input type="file" multiple @change="previewImages" class="text-white">
        </div>

        <!-- Vista previa de imágenes -->
        <div v-if="imagePreviews.length" class="flex gap-2 mt-4">
          
          <div v-if="isUpdateMode" v-for="(image, index) in imagePreviews" :key="index" class="relative">
            <img :src="image.url" class="w-36 h-36 object-cover rounded-lg border">
            <button @click.prevent="removeImage(index,image.id)"
              class="absolute top-1 right-1 bg-red-500 text-white text-xs rounded-full h-4 w-4">
              ✕
            </button>
          </div>
          <div v-else v-for="(image, indx) in imagePreviews" :key="indx" class="relative">
            <img :src="image.url" class="w-36 h-36 object-cover rounded-lg border">
            <button @click.prevent="removeNewImage(indx)"
              class="absolute top-1 right-1 bg-red-500 text-white text-xs rounded-full h-4 w-4">
              ✕
            </button>
          </div>
        </div>

        <button class="bg-primary-500 text-white py-2 px-4 rounded hover:bg-primary-700 mt-4" type="submit">
          {{ isUpdateMode ? 'Actualizar' : 'Crear' }}
        </button>
        <button v-if="isUpdateMode" @click="resetForm"
          class="bg-gray-500 text-white py-2 px-4 rounded hover:bg-gray-700 mt-4 ml-2">
          Cancelar
        </button>
      </form>
    </div>

    <!-- List of Rooms -->
    <div>
      <h2 class="text-xl text-white lexend-exa font-bold mb-4">Habitaciones</h2>
      <div class="grid grid-cols-3 gap-4">
        <div v-for="habitacion in habitaciones" :key="habitacion.habitacionId"
          class="p-4 border-4 bg-neutral-800 rounded-md text-lg font-semibold shadow-sm text-white text-center grid">
          {{ habitacion.nombreHabitacion }}
          <button @click="confirmDeleteRoom(habitacion)"
            class="bg-red-500 text-white px-2 py-1 mt-2 rounded hover:bg-red-700">
            Borrar
          </button>
          <button @click="startUpdateRoom(habitacion)"
            class="bg-blue-500 text-white px-2 py-1 mt-2 rounded hover:bg-blue-700">
            Actualizar
          </button>
          <button @click="showInventoryRoom(habitacion)"
            class="bg-blue-500 text-white px-2 py-1 mt-2 rounded hover:bg-blue-700">
            Inventario
          </button>
        </div>
      </div>
    </div>

    <!-- Delete Confirmation Modal -->
    <div v-if="showDeleteModal" class="fixed inset-0 flex items-center justify-center bg-black bg-opacity-50">
      <div class="bg-white p-6 rounded-lg shadow-lg text-center">
        <h3 class="text-xl font-bold mb-4">
          ¿Estás seguro de eliminar la habitación {{ roomToDelete?.nombreHabitacion }}?
        </h3>
        <div class="flex justify-center space-x-4">
          <button @click="deleteRoom(roomToDelete.habitacionId)"
            class="bg-red-500 text-white px-4 py-2 rounded hover:bg-red-700">
            Eliminar
          </button>
          <button @click="cancelDeleteRoom" class="bg-gray-300 px-4 py-2 rounded hover:bg-gray-400">
            Cancelar
          </button>
        </div>
      </div>
    </div>

    <!-- Inventory Modal -->
    <div v-if="showInventoryModal" class="fixed inset-0 flex items-center justify-center bg-black bg-opacity-50">
      <ModalInventory :room="roomToInventory" @close-modal="showInventoryRoom" />
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue';
import axiosClient from '../axiosClient';
import ModalInventory from '../components/ModalInventory.vue';
import { useAuthStore } from '../store/auth.js';
import { fetchImagesAndIds } from '../services/imageService';

const authStore = useAuthStore();

const habitaciones = ref([]);
const categorias = ref([]);
const roomName = ref('');
const selectedCategory = ref(null);
const imageFiles = ref([]);
const imagePreviews = ref([]);
const showDeleteModal = ref(false);
const showInventoryModal = ref(false);
const roomToDelete = ref(null);
const roomToInventory = ref(null);
const isUpdateMode = ref(false); // Flag for update mode
const roomToUpdate = ref(null); // Room being updated
const removedImageIds = ref([]); // Track IDs of removed images

// Fetch rooms and categories
const fetchHabitaciones = () => {
  const institucionID = authStore.institucionID;
  if (!institucionID) {
    console.warn('InstitucionID is not available. Please ensure the user is logged in.');
    return;
  }
  axiosClient.get(`/GetHabitaciones?InstitucionID=${institucionID}`)
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
  const institucionID = authStore.institucionID;
  if (!institucionID) {
    console.warn('InstitucionID is not available. Please ensure the user is logged in.');
    return;
  }
  axiosClient.get(`/api/Objetos/GetCategorias?InstitucionID=${institucionID}`)
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

// Handle image selection
const previewImages = (event) => {
  const files = event.target.files;
  imageFiles.value = [...files]; // Almacena los archivos seleccionados
  imagePreviews.value = []; // Reinicia las vistas previas

  for (let file of files) {
    const reader = new FileReader();
    reader.onload = (e) => {
      // Agrega un objeto con id = 0 y la URL de la imagen
      imagePreviews.value.push({
        id: 0, // ID temporal para imágenes nuevas
        url: e.target.result // URL de la imagen
      });
    };
    reader.readAsDataURL(file); // Lee el archivo como una URL de datos
  }
};

// Remove an image
const removeImage = (index,imageId) => {
  removedImageIds.value.push(imageId); // Add the ID to the list of removed images
  console.log("hola",removedImageIds.value)
  imageFiles.value.splice(index, 1); // Remove the file from the array
  imagePreviews.value.splice(index, 1); // Remove the preview URL from the array
};
const removeNewImage = (index) => {

  imageFiles.value.splice(index, 1); // Remove the file from the array
  imagePreviews.value.splice(index, 1); // Remove the preview URL from the array
};

function showInventoryRoom(room) {
  roomToInventory.value = room; 
  showInventoryModal.value = !showInventoryModal.value; //
  console.log(roomToInventory.value);
}
const startUpdateRoom = async (room) => {
  isUpdateMode.value = true;
  roomToUpdate.value = room;
  roomName.value = room.nombreHabitacion;
  selectedCategory.value = room.categoriaId;

  // Load images for the room
  const imagenIds = room.imagenes || [];
  if (imagenIds.length > 0) {
    try {
      const imagenes = await fetchImagesAndIds(imagenIds); // Fetch image URLs
      imagePreviews.value = imagenes; // Set image objects for preview
      console.log(imagePreviews.value)
    } catch (error) {
      console.error("Error fetching images:", error);
      imagePreviews.value = []; // Fallback to empty array if there's an error
    }
  } else {
    imagePreviews.value = []; // No images to load
  }
};



// Submit form (create or update)
const submitForm = async () => {
  const institucionID = authStore.institucionID;
  if (!institucionID || !selectedCategory.value || !roomName.value) {
    alert("Por favor ingresa todos los datos.");
    return;
  }

  if (isUpdateMode.value) {
    // Update room
    await updateRoom(roomToUpdate.value.habitacionId);
  } else {
    // Create room
    await createRoom();
  }
};

// Create a new room
const createRoom = async () => {
  const formData = new FormData();
  formData.append("institucionID", authStore.institucionID);
  formData.append("nombreHabitacion", roomName.value);
  formData.append("categoriaID", selectedCategory.value);
  imageFiles.value.forEach((file) => {
    formData.append("imagenes", file);
  });

  try {
    const response = await axiosClient.post("/CrearHabitacionConImagenes", formData, {
      headers: { "Content-Type": "multipart/form-data" },
    });
    alert(response.data.message);
    resetForm();
    fetchHabitaciones();
  } catch (error) {
    console.error("Error al crear habitación:", error);
    alert("Hubo un error al crear la habitación.");
  }
};

// Update a room
const updateRoom = async (habitacionID) => {
  const usuarioID = authStore.auth?.usuarioID;
  if (!usuarioID) {
    console.warn('UsuarioID is not available. Please ensure the user is logged in.');
    return;
  }

// Delete removed images via API
if (removedImageIds.value.length > 0) {
    try {
      await Promise.all(
        removedImageIds.value.map(async (imagenId) => {
          await axiosClient.delete(`/EliminarImagenHabitacion?imagenId=${imagenId}`);
        })
      );
      console.log("Removed images deleted successfully.");
    } catch (error) {
      console.error("Error deleting removed images:", error);
    }
  }
  const formData = new FormData();
  formData.append("id", habitacionID);
  formData.append("nuevoNombre", roomName.value);
  formData.append("nuevaCategoria", selectedCategory.value);
  formData.append("usuarioId", usuarioID);

  // Append new images
  imageFiles.value.forEach((file) => {
    formData.append("nuevasImagenes", file);
  });

  try {
    const response = await axiosClient.put("/ActualizarHabitacion", formData, {
      headers: { "Content-Type": "multipart/form-data" },
    });
    alert(response.data.message);
    resetForm();
    fetchHabitaciones();
  } catch (error) {
    console.error('Error updating room:', error);
    alert('Error updating room');
  }
};    
// Reset form
const resetForm = () => {
  isUpdateMode.value = false;
  roomToUpdate.value = null;
  roomName.value = '';
  selectedCategory.value = null;
  imageFiles.value = [];
  imagePreviews.value = [];
  removedImageIds.value = []; 
};

// Delete a room
const deleteRoom = (idHabitacion) => {
  axiosClient.delete(`/AnularHabitacion?idHabitacion=${idHabitacion}&Estado=true`)
    .then(({ data }) => {
      if (data.ok) {
        alert("Habitación eliminada correctamente.");
        fetchHabitaciones();
        showDeleteModal.value = false;
        roomToDelete.value = null;
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