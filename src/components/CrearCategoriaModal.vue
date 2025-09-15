<template>
    <div class="bg-white p-6 rounded-lg shadow-lg text-center w-96">
      <h3 class="text-xl font-bold mb-4">Crear nueva categoría</h3>
      <form @submit.prevent="createCategory">
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
          <button type="submit" class="bg-blue-500 hover:bg-blue-700 text-white font-bold py-2 px-4 rounded">
            Crear
          </button>
          <button 
            type="button" 
            @click="$emit('close-modal')" 
            class="bg-gray-300 px-4 py-2 rounded hover:bg-gray-400">
            Cancelar
          </button>
        </div>
      </form>
    </div>
  </template>
  
  <script setup>
  import { ref, onMounted } from 'vue';
  import axiosClient from '../axiosClient';
  import { defineEmits } from 'vue';
  const emit = defineEmits(['category-created', 'close-modal']);

  const categoryName = ref('');
  
  const createCategory = async () => {
    if (categoryName.value.trim()) {
      try {
        const response = await axiosClient.post(`/api/CategoriaArticulos/CrearCategoria`, {
          InstitucionID: InstitucionID.value,
          NombreCategoria: categoryName.value,
        });
  
        if (response.data && response.data.ok) {
          alert(response.data.message);
          emit('category-created'); // Emit event to refresh categories
          emit('close-modal'); // Close modal
        } else {
          alert('Error creating category: ' + response.data.message);
        }
      } catch (error) {
        console.error('Error creating category:', error);
        alert('Error creating category');
      }
    } else {
      alert('Por favor, ingresa un nombre para la categoría');
    }
  };
  // logica login
import { useAuthStore } from '../store/auth.js'; // Import the auth store
const InstitucionID = ref(null);
const authStore = useAuthStore();
function getDatosLogin(){
    InstitucionID.value = authStore.institucionID;
  }
  onMounted(() => {
  getDatosLogin();
});

  </script>
  
  <style scoped>
  /* Add your styles here */
  </style>
  