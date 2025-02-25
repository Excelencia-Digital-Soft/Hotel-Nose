<template>
  <div class="bg-white p-6 rounded-lg shadow-lg text-center w-96 z-20">
    <h3 class="text-xl font-bold mb-4">Editar categoría</h3>
    <form @submit.prevent="editCategory">
      <div class="mb-4">
        <label class="block text-sm font-bold mb-2" for="categoryName">Nombre de la categoría:</label>
        <input
          v-model="categoryName"
          class="shadow appearance-none border rounded w-full py-2 px-3 text-gray-700 leading-tight focus:outline-none focus:shadow-outline"
          id="categoryName"
          type="text"
          placeholder="Nombre de la categoría"
          required
        />
      </div>
      <div class="mt-6 flex space-x-4 justify-center">
        <!-- Edit Button -->
        <button
          type="submit"
          class="bg-blue-500 hover:bg-blue-700 text-white font-bold py-2 px-4 rounded"
        >
          Editar
        </button>
        <!-- Cancel Button -->
        <button
          type="button"
          @click="$emit('close-modal')"
          class="bg-gray-300 px-4 py-2 rounded hover:bg-gray-400"
        >
          Cancelar
        </button>
        <!-- Delete Button -->
        <button
          type="button"
          @click="deleteCategory"
          class="bg-red-500 hover:bg-red-700 text-white font-bold py-2 px-4 rounded"
        >
           <span class="material-symbols-outlined">delete</span>
        </button>
      </div>
    </form>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue';
import axiosClient from '../axiosClient';

const emit = defineEmits(['category-updated', 'close-modal']);
const props = defineProps({
  categoria: { type: Number, required: true },
});

const categoryName = ref('');

// Function to edit category
const editCategory = async () => {
  if (categoryName.value.trim()) {
    try {
      const response = await axiosClient.put(`/api/CategoriaArticulos/ActualizarCategoria?id=${props.categoria}&UsuarioID=${UsuarioID.value}`, {
        NombreCategoria: categoryName.value,
      });

      if (response.data && response.data.ok) {
        alert(response.data.message);
        emit('category-updated'); // Emit event to refresh categories
        emit('close-modal'); // Close modal
      } else {
        alert('Error editando la categoría: ' + response.data.message);
      }
    } catch (error) {
      console.error('Error editando la categoría:', error);
      alert('Error editando la categoría');
    }
  } else {
    alert('Por favor, ingresa un nombre para la categoría');
  }
};

// Function to delete category
const deleteCategory = async () => {
  if (confirm('¿Estás seguro de que deseas eliminar esta categoría?')) {
    try {
      const response = await axiosClient.delete(`/api/CategoriaArticulos/AnularCategoria`, {
        params: {
          id: props.categoria,
          estado: true,
        },
      });
      if (response.data && response.data.ok) {
        alert(response.data.message);
        emit('category-updated'); // Emit event to refresh categories
        emit('close-modal'); // Close modal
      } else {
        alert('Error eliminando la categoría: ' + response.data.message);
      }
    } catch (error) {
      console.error('Error eliminando la categoría:', error);
      alert('Error eliminando la categoría');
    }
  }
};
import { useAuthStore } from '../store/auth.js'; // Import the auth store
const UsuarioID = ref(null);
const authStore = useAuthStore();
function getDatosLogin(){
    UsuarioID.value = authStore.auth?.usuarioID;
  }
  onMounted(() => {
  getDatosLogin();
});
</script>

<style scoped>
/* Add your styles here */
</style>
