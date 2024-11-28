<template>
  <div class="p-4">
    <!-- Form to Create a New Room -->
    <div class="mb-6">
      <h2 class="text-xl text-white lexend-exa font-bold mb-4">Crear articulo</h2>
      <form @submit.prevent="createArticulo">
        <div class="mb-4">
          <label class="block text-white text-sm font-bold mb-2" for="articuloName">Nombre:</label>
          <input v-model="newArticuloName"
            class="shadow appearance-none border rounded w-full py-2 px-3 text-gray-700 leading-tight focus:outline-none focus:shadow-outline"
            id="articuloName" type="text" placeholder="Nombre del artículo" required>
        </div>
        <div class="mb-4">
          <label class="block text-white text-sm font-bold mb-2">Precio:</label>
          <input v-model="newArticuloPrice"
            class="shadow appearance-none border rounded w-full py-2 px-3 text-gray-700 leading-tight focus:outline-none focus:shadow-outline"
            id="articuloPrice" type="text" placeholder="0" required>
        </div>
        <div class="mb-4">
          <label class="block text-white text-sm font-bold mb-2">Imagen:</label>
          <input @change="onImageChange"
            class="shadow appearance-none border rounded w-full py-2 px-3 text-gray-700 leading-tight focus:outline-none focus:shadow-outline"
            id="articuloImage" type="file" accept="image/*">
        </div>
        <button class="bg-primary-500 text-white py-2 px-4 rounded hover:bg-primary-700" type="submit">
          Crear
        </button>
      </form>
    </div>

    <!-- List of Articulos -->
    <div>
      <h2 class="text-xl text-white lexend-exa font-bold mb-4">Articulos</h2>
      <div class="grid grid-cols-3 gap-4">
        <div v-for="articulo in articulos" :key="articulo.articuloId"
          class="p-4 border-4 bg-surface-800 rounded-md text-lg font-semibold shadow-sm text-white text-center">
          {{ articulo.nombreArticulo }}
          <button @click="confirmDeleteArticulo(articulo)"
            class="bg-red-500 text-white px-2 py-1 mt-2 rounded hover:bg-red-700">
            Borrar
          </button>

          <button @click="confirmUpdateArticulo(articulo)"
            class="bg-blue-500 text-white px-2 py-1 mt-2 rounded hover:bg-blue-700">
            Actualizar
          </button>
        </div>
      </div>
      <div v-if="showDeleteModal" class="fixed inset-0 flex items-center justify-center bg-black bg-opacity-50">

        <div class="bg-white p-6 rounded-lg shadow-lg text-center">
          <h3 class="text-xl font-bold mb-4">
            ¿Estás seguro de eliminar el articulo {{ articuloToDelete?.nombreArticulo }}?
          </h3>
          <div class="flex justify-center space-x-4">
            <button @click="deleteArticulo(articuloToDelete?.articuloId)"
              class="bg-red-500 text-white px-4 py-2 rounded hover:bg-red-700">
              Eliminar
            </button>
            <button @click="cancelDeleteArticulo" class="bg-gray-300 px-4 py-2 rounded hover:bg-gray-400">
              Cancelar
            </button>
          </div>
        </div>
      </div>
      <div v-if="showUpdateModal" class="fixed inset-0 flex items-center justify-center bg-black bg-opacity-50">

        <div class="bg-white p-6 rounded-lg shadow-lg text-center">
          <h2 class="text-xl text-black lexend-exa font-bold mb-4">Actualizar artículo</h2>
          <form @submit.prevent="updateArticulo(articuloToUpdate?.articuloId)">
            <div class="mb-4">
              <label class="block text-black text-sm font-bold mb-2" for="articuloName">Nombre:</label>
              <input v-model="updateArticuloName"
                class="shadow appearance-none border rounded w-full py-2 px-3 text-gray-700 leading-tight focus:outline-none focus:shadow-outline"
                id="articuloName" type="text" placeholder="Nombre del articulo" required>
            </div>
            <div class="mb-4">
              <label class="block text-black text-sm font-bold mb-2">Precio:</label>
              <input v-model="updateArticuloPrice"
                class="shadow appearance-none border rounded w-full py-2 px-3 text-gray-700 leading-tight focus:outline-none focus:shadow-outline"
                id="articuloPrice" type="text" placeholder="0" required>
            </div>
            <button class="bg-blue-500 text-white py-2 px-4 rounded hover:bg-blue-700" type="submit">
              Actualizar
            </button>
          </form>
          <button @click="cancelUpdateArticulo" class="bg-gray-300 px-4 py-2 rounded hover:bg-gray-400">
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

const articulos = ref([]);
const newArticuloName = ref('');
const newArticuloPrice = ref('');
const updateArticuloName = ref('');
const updateArticuloPrice = ref('');
const showDeleteModal = ref(false);
const showUpdateModal = ref(null);
const articuloToDelete = ref(null);
const articuloToUpdate = ref(null);
const newArticuloImage = ref(null);

const onImageChange = (event) => {
  if (event.target.files && event.target.files[0]) {
    newArticuloImage.value = event.target.files[0];
  }
};
function confirmDeleteArticulo(articulo) {
  articuloToDelete.value = articulo;
  showDeleteModal.value = true;
}

function cancelDeleteArticulo() {
  articuloToDelete.value = null;
  showDeleteModal.value = false;
}

function confirmUpdateArticulo(articulo) {
  articuloToUpdate.value = articulo;
  updateArticuloName.value = articulo.nombreArticulo;
  updateArticuloPrice.value = articulo.precio;
  showUpdateModal.value = true;
}

function cancelUpdateArticulo() {
  articuloToUpdate.value = null;
  showUpdateModal.value = false;
}
// Fetch rooms and categories initially
const fetchArticulos = () => {
  axiosClient.get("/api/Articulos/GetArticulos")
    .then(({ data }) => {
      if (data && data.ok) {
        articulos.value = data.data;
      } else {
        console.error('Failed to fetch articulos:', data.message);
      }
    })
    .catch(error => {
      console.error('Error fetching articulos:', error);
    });
};


// Create a new room
const createArticulo = async () => {
  if (newArticuloName.value && newArticuloPrice.value && newArticuloImage.value) {
    try {
      const formData = new FormData();
      formData.append("nombre", newArticuloName.value);
      formData.append("precio", newArticuloPrice.value);
      formData.append("imagen", newArticuloImage.value); // Agregamos la imagen al FormData

      const response = await axiosClient.post(`/api/Articulos/CreateArticuloWithImage`, formData, {
        headers: {
          "Content-Type": "multipart/form-data", // Indicar al servidor que enviamos un archivo
        },
      });

      alert(response.data.message);
      fetchArticulos(); // Refrescar la lista de artículos
      newArticuloName.value = '';
      newArticuloPrice.value = null;
      newArticuloImage.value = null;
    } catch (error) {
      console.error('Error creating articulo:', error);
      alert('Error creando artículo');
    }
  } else {
    alert("Por favor, ingresa el nombre, precio e imagen");
  }
};
const updateArticulo = async (articuloID) => {
  if (updateArticuloName.value && updateArticuloPrice.value) {
    try {
      // Make sure to format the URL correctly
      const response = await axiosClient.put(
        `/api/Articulos/UpdateArticulo?id=${articuloID}&nombre=${encodeURIComponent(updateArticuloName.value)}&precio=${updateArticuloPrice.value}`
      );
      alert(response.data.message);
      fetchArticulos(); // Refrescar categorias
      updateArticuloName.value = '';
      updateArticuloPrice.value = null;
      cancelUpdateArticulo();
    } catch (error) {
      console.error('Error updating articulo:', error);
      alert('Error updating articulo');
    }
  } else {
    alert("Por favor ingresa el nombre y precio");
  }
};

// Delete a room
const deleteArticulo = (idArticulo) => {
  axiosClient.delete(`/api/Articulos/AnularArticulo?id=${idArticulo}&estado=true`)
    .then(({ data }) => {
      if (data.ok) {
        alert("Articulo eliminado correctamente.");
        fetchArticulos(); // Refresh the list of rooms
        cancelDeleteArticulo();
      } else {
        alert("Failed to delete articulo: " + data.message + idArticulo);
      }
    })
    .catch(error => {
      console.error('Error deleting articulo:', error);
    });
};



// Fetch rooms and categories on component mount
onMounted(() => {
  fetchArticulos();
});
</script>

<style scoped>
/* Add custom styles if needed */
</style>