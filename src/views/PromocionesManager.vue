<template>
    <div class="text-white min-h-screen p-4">
      <h1 class="text-2xl font-bold mb-6">Gestión de Promociones</h1>
  
      <!-- Dropdown for selecting a category -->
      <div class="mb-6">
        <label class="block text-lg font-semibold mb-2">Seleccionar Categoría</label>
        <select 
          v-model="selectedCategory" 
          class="bg-neutral-800 text-white rounded p-2 w-full"
        >
          <option v-for="categoria in categorias" :key="categoria.categoriaId" :value="categoria">
            {{ categoria.nombreCategoria }}
          </option>
        </select>
      </div>
  
      <!-- Placeholder for modals -->
      <div>
        <h3 class="font-semibold text-lg mb-4">Promociones</h3>
        <ul class="space-y-2">
          <li 
            v-for="(promo, index) in promoPlaceholders" 
            :key="index" 
            class="cursor-pointer hover:text-blue-600 border border-white p-4 h-[15vh] flex items-center justify-center"
          >
            Promoción Placeholder {{ index + 1 }}
          </li>
        </ul>
      </div>
    </div>
  </template>
  
  <script setup>
  import { ref, onMounted } from 'vue';
  import axiosClient from '../axiosClient'; // Adjust to your project's path
  
  // Reactive state for categories and selected category
  const categorias = ref([]);
  const selectedCategory = ref(null);
  
  // Placeholder array for future modals
  const promoPlaceholders = ref(new Array(3).fill(null));
  
  // Fetch categories on mount
  const fetchCategorias = async () => {
    try {
      const response = await axiosClient.get('/GetCategorias');
      if (response.data.ok) {
        categorias.value = response.data.data;
      } else {
        console.error('Error fetching categories:', response.data.message);
      }
    } catch (error) {
      console.error('Error fetching categories:', error);
    }
  };
  
  // Call fetchCategorias on component mount
  onMounted(fetchCategorias);
  </script>
  
  <style scoped>
  /* Add custom styles if needed */
  </style>
  