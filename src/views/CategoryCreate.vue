<template>
    <div class="p-4">
      <!-- Form to Create a New Room -->
      <div class="mb-6">
        <h2 class="text-xl text-white lexend-exa font-bold mb-4">Crear categoría</h2>
        <form @submit.prevent="createCategory">
          <div class="mb-4">
            <label class="block text-white text-sm font-bold mb-2" for="categoryName">Nombre:</label>
            <input
              v-model="newCategoryName"
              class="shadow appearance-none border rounded w-full py-2 px-3 text-gray-700 leading-tight focus:outline-none focus:shadow-outline"
              id="categoryName"
              type="text"
              placeholder="Nombre de la categoría"
              required
            >
          </div>
          <div class="mb-4">
            <label class="block text-white text-sm font-bold mb-2">Precio:</label>
            <input
              v-model="newCategoryPrice"
              class="shadow appearance-none border rounded w-full py-2 px-3 text-gray-700 leading-tight focus:outline-none focus:shadow-outline"
              id="categoryPrice"
              type="text"
              placeholder="0"
              required
            >
          </div>
          <div class="mb-4">
            <label class="block text-white text-sm font-bold mb-2">Capacidad máxima:</label>
            <input
              v-model="newCategoryCapacity"
              class="shadow appearance-none border rounded w-full py-2 px-3 text-gray-700 leading-tight focus:outline-none focus:shadow-outline"
              id="categoryCapacity"
              type="text"
              placeholder="0"
              required
            >
          </div>
          <div class="mb-4">
            <label class="block text-white text-sm font-bold mb-2">Porcentaje por persona:</label>
            <input
              v-model="newCategoryPercentaje"
              class="shadow appearance-none border rounded w-full py-2 px-3 text-gray-700 leading-tight focus:outline-none focus:shadow-outline"
              id="categoryPercentaje"
              type="text"
              placeholder="0"
              required
            >
          </div>
          <button
            class="bg-primary-500 text-white py-2 px-4 rounded hover:bg-primary-700"
            type="submit"
          >
            Crear
          </button>
        </form>
      </div>
  
      <!-- List of Categories -->
      <div>
        <h2 class="text-xl text-white lexend-exa font-bold mb-4">Categorias</h2>
        <div class="grid grid-cols-3 gap-4">
          <div
            v-for="categoria in categorias"
            :key="categoria.categoriaId"
            class="p-4 border-4 bg-surface-800 rounded-md text-lg font-semibold shadow-sm text-white text-center"
          >
            {{ categoria.nombreCategoria }}
            <button
              @click="confirmDeleteCategory(categoria)"
              class="bg-red-500 text-white px-2 py-1 mt-2 rounded hover:bg-red-700"
            >
              Borrar
            </button>

            <button
              @click="confirmUpdateCategory(categoria)"
              class="bg-blue-500 text-white px-2 py-1 mt-2 rounded hover:bg-blue-700"
            >
              Actualizar
            </button>
          </div>
        </div>
        <div
        v-if="showDeleteModal"
        class="fixed inset-0 flex items-center justify-center bg-black bg-opacity-50"
        >

      <div class="bg-white p-6 rounded-lg shadow-lg text-center">
    <h3 class="text-xl font-bold mb-4">
      ¿Estás seguro de eliminar la categoria {{ categoryToDelete?.nombreCategoria }}?
    </h3>
    <div class="flex justify-center space-x-4">
      <button
        @click="deleteCategory(categoryToDelete?.categoriaId)"
        class="bg-red-500 text-white px-4 py-2 rounded hover:bg-red-700"
      >
        Eliminar
      </button>
      <button
        @click="cancelDeleteCategory"
        class="bg-gray-300 px-4 py-2 rounded hover:bg-gray-400"
      >
        Cancelar
      </button>
    </div>
    </div>
  </div>
  <div
        v-if="showUpdateModal"
        class="fixed inset-0 flex items-center justify-center bg-black bg-opacity-50"
        >

      <div class="bg-white p-6 rounded-lg shadow-lg text-center">
        <h2 class="text-xl text-black lexend-exa font-bold mb-4">Actualizar categoría</h2>
        <form @submit.prevent="updateCategory(categoryToUpdate?.categoriaId)">
          <div class="mb-4">
            <label class="block text-black text-sm font-bold mb-2" for="categoryName">Nombre:</label>
            <input
              v-model="updateCategoryName"
              class="shadow appearance-none border rounded w-full py-2 px-3 text-gray-700 leading-tight focus:outline-none focus:shadow-outline"
              id="categoryName"
              type="text"
              placeholder="Nombre de la categoría"
              required
            >
          </div>  
          <div class="mb-4">
            <label class="block text-black text-sm font-bold mb-2">Precio:</label>
            <input
              v-model="updateCategoryPrice"
              class="shadow appearance-none border rounded w-full py-2 px-3 text-gray-700 leading-tight focus:outline-none focus:shadow-outline"
              id="categoryPrice"
              type="text"
              placeholder="0"
              required
            >
          </div>
          <div class="mb-4">
            <label class="block text-black text-sm font-bold mb-2">Capacidad máxima:</label>
            <input
              v-model="updateCategoryCapacity"
              class="shadow appearance-none border rounded w-full py-2 px-3 text-gray-700 leading-tight focus:outline-none focus:shadow-outline"
              id="categoryCapacity"
              type="text"
              placeholder="0"
              required
            >
          </div>
          <div class="mb-4">
            <label class="block text-black text-sm font-bold mb-2">Porcentaje por persona:</label>
            <input
              v-model="updateCategoryPercentaje"
              class="shadow appearance-none border rounded w-full py-2 px-3 text-gray-700 leading-tight focus:outline-none focus:shadow-outline"
              id="categoryPercentaje"
              type="text"
              placeholder="0"
              required
            >
          </div>
          <button
            class="bg-blue-500 text-white py-2 px-4 rounded hover:bg-blue-700"
            type="submit"
          >
            Actualizar
          </button>
          </form>
          <button
        @click="cancelUpdateCategory"
        class="bg-gray-300 px-4 py-2 rounded hover:bg-gray-400"
      >
        Cancelar
      </button>
      <div class="flex justify-center space-x-4">

    </div>
    </div>
  </div>
</div>
    </div>
  </template>
  
  <script setup>
  import { ref, onMounted } from 'vue'
  import axiosClient from '../axiosClient'; // Adjust the path according to your project structure
  import { useAuthStore } from '../store/auth.js'; // Import the auth store

  const authStore = useAuthStore();

  const categorias = ref([]);
  const newCategoryCapacity = ref('');
  const newCategoryName = ref('');
  const newCategoryPrice = ref('');
  const newCategoryPercentaje = ref('');
  const updateCategoryCapacity = ref('');
  const updateCategoryName = ref('');
  const updateCategoryPrice = ref('');
  const updateCategoryPercentaje = ref('');
  const showDeleteModal = ref(false);
  const showUpdateModal= ref(null);
  const categoryToDelete = ref(null);
  const categoryToUpdate = ref(null);
  const UsuarioID = ref(null);
  const InstitucionID = ref(null);

  function getDatosLogin(){
    InstitucionID.value = authStore.auth?.institucionID;
    UsuarioID.value = authStore.auth?.usuarioID;
  }
  function confirmDeleteCategory(category){
    categoryToDelete.value = category;
    showDeleteModal.value = true;
  }
  
  function cancelDeleteCategory(){
    categoryToDelete.value = null;
    showDeleteModal.value = false;
  }

  function confirmUpdateCategory(category){
    categoryToUpdate.value = category;
    updateCategoryName.value = category.nombreCategoria;
    updateCategoryCapacity.value = category.capacidadMaxima;
    updateCategoryPrice.value = category.precioNormal;
    updateCategoryPercentaje.value = category.porcentajeXPersona;
    showUpdateModal.value = true;
  }
  
  function cancelUpdateCategory(){
    categoryToUpdate.value = null;
    showUpdateModal.value = false;
  }
  // Fetch rooms and categories initially
  const fetchCategorias = () => {
  if (InstitucionID.value == null) {
  console.warn('InstitucionID is not available.  Please ensure the user is logged in.');
  return; }
    axiosClient.get(`/api/Objetos/GetCategorias?InstitucionID=${InstitucionID.value}`)
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
  const createCategory = async () => {
  if (InstitucionID.value== null) {
  console.warn('InstitucionID is not available.  Please ensure the user is logged in.');
  return; }
    if (newCategoryName.value && newCategoryPrice.value) {
      try {
        // Make sure to format the URL correctly
        const response = await axiosClient.post(
          `/api/Objetos/CrearCategoria?nombreCategoria=${encodeURIComponent(newCategoryName.value)}&UsuarioID=${UsuarioID.value}&InstitucionID=${InstitucionID.value}&precio=${newCategoryPrice.value}&capacidadmaxima=${newCategoryCapacity.value}&Porcentaje=${newCategoryPercentaje.value}`
        );
        alert(response.data.message);
        fetchCategorias(); // Refresh the list of rooms
        // Reset the fields after the request
        newCategoryName.value = '';
        newCategoryPrice.value = null;
        newCategoryCapacity.value = null;
        newCategoryPercentaje.value = null;
      } catch (error) {
        console.error('Error creating category:', error);
        alert('Error creating category');
      }
    } else {
      alert("Por favor ingresa el nombre, precio y capacidad máxima");
    }
  };
  
    const updateCategory = async (categoriaID) => {
    if (updateCategoryName.value && updateCategoryPrice.value) {
      try {
        // Make sure to format the URL correctly
        const response = await axiosClient.put(
          `/api/Objetos/ActualizarCategoria?id=${categoriaID}&UsuarioID=${UsuarioID.value}&nuevoNombre=${encodeURIComponent(updateCategoryName.value)}&nuevaCapacidad=${updateCategoryCapacity.value}&Precio=${updateCategoryPrice.value}&Porcentaje=${updateCategoryPercentaje.value}`
        );
        alert(response.data.message);
        fetchCategorias(); // Refrescar categorias
        updateCategoryName.value = '';
        updateCategoryPrice.value = null;
        updateCategoryCapacity.value = null;
        updateCategoryPercentaje.value = null;
        cancelUpdateCategory();
      } catch (error) {
        console.error('Error updating category:', error);
        alert('Error updating category');
      }
    } else {
      alert("Por favor ingresa el nombre, precio y capacidad máxima");
    }
  };
  
  // Delete a room
  const deleteCategory = (idCategory) => {
    axiosClient.delete(`/api/Objetos/AnularCategoria?id=${idCategory}&Estado=true`)
      .then(({ data }) => {
        if (data.ok) {
          alert("Categoria eliminada correctamente.");
          fetchCategorias(); // Refresh the list of rooms
          cancelDeleteCategory();
        } else {
          alert("Failed to delete category: " + data.message + idCategory);
        }
      })
      .catch(error => {
        console.error('Error deleting category:', error);
      });
  };
  

  
  // Fetch rooms and categories on component mount
  onMounted(() => {
    getDatosLogin();
    fetchCategorias();
  });
  </script>
  
  <style scoped>
  /* Add custom styles if needed */
  </style>
  