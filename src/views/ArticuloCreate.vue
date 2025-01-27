<template>
  <div class="px-12 py-6">
    <!-- Categories Tab -->

    <div v-if="showEditarCategoriaModal" class="fixed inset-0 flex items-center justify-center bg-black bg-opacity-50">
      <EditarCategoriaModal :categoria="selectedCategory" @close-modal="toggleEditarCategoriaModal(null)"
        @category-updated="fetchCategories" />
    </div>

    <div class="mb-6">
      
      <form @submit.prevent="createArticulo" class="relative pt-16 w-full grid grid-cols-1 sm:grid-cols-2 place-items-center gap-1 border-2 p-8 rounded-3xl border-secondary-600 ">
        <h2 class="absolute top-2 text-xl text-white lexend-exa text-center font-bold mb-4">CREAR ARTICULOS</h2>
        <div class="flex flex-col w-full justify-center">
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
            <label class="block text-white text-sm font-bold mb-2" for="categoriaSelect">Categoría:</label>
            <select v-model="newArticuloCategoriaId"
              class="shadow appearance-none border rounded w-full py-2 px-3 text-gray-700 leading-tight focus:outline-none focus:shadow-outline"
              id="categoriaSelect" required>
              <option v-for="category in categories" :key="category.categoriaId" :value="category.categoriaId">
                {{ category.nombreCategoria }}
              </option>
            </select>
          </div>

        </div>
        <section class="grid place-items-start rounded-xl mb-6 px-24">
          <!-- Input de archivo -->
          <div class="mb-4 grid ">
            <label class="block text-white text-sm font-bold mb-2">Imagen:</label>
            <input @change="onImageChange"
              class=" shadow flex rounded w-56 sm:w-72 h-full file:p-4 file:bg-accent-300 file:text-white file:hover:bg-accent-200 file:hover:cursor-pointer file:rounded-xl text-gray-700 leading-tight focus:outline-none focus:shadow-outline"
              id="articuloImage" type="file" accept="image/*" />
          </div>

          <!-- Vista previa de la imagen -->
          <div class="mb-4">
            <label class="block text-white text-sm font-bold mb-2">Vista previa:</label>
            <img :src="previewImage" alt="Preview" class="w-80 md:w-96 h-44 object-cover rounded" />
          </div>
        </section>
        <button class="w-full sm:col-span-2 sm:w-2/5 bg-primary-500 text-white py-2 px-4 rounded hover:bg-primary-700" type="submit">
          Crear
        </button>
      </form>

    </div>
    <section class="flex mb-4">
      <div class="flex space-x-4 bg-surface-700 rounded-l-xl h-24 p-4 overflow-x-auto snap-x snap-center">
        <!-- Render categories -->
        <button v-for="category in categories" :key="category.categoriaId"
          @click="selectedCategory === category.categoriaId ? toggleEditarCategoriaModal(category.categoriaId) : filterByCategory(category.categoriaId)"
          :class="['py-2 px-2 snap-start rounded hover:bg-secondary-100 flex items-center', selectedCategory === category.categoriaId ? 'bg-secondary-500 text-white' : 'bg-gray-300']">
          {{ category.nombreCategoria }}
          <span v-if="selectedCategory === category.categoriaId" class="material-symbols-outlined ml-2">edit
          </span>
        </button>

        <!-- Add Category Button -->

      </div>
      <button @click="toggleCreateCategoryModal"
        class="btn-third py-2 px-4 rounded-r-xl bg-primary-500 shadow-lg hover:shadow-primary-500 text-white flex items-center justify-center ">
        <span class="text-xl font-bold">+</span>
      </button>
    </section>
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

            <!-- Category Dropdown -->
            <div class="mb-4">
              <label class="block text-black text-sm font-bold mb-2" for="categoria">Categoría:</label>
              <select v-model="updateArticuloCategoriaId"
                class="shadow appearance-none border rounded w-full py-2 px-3 text-gray-700 leading-tight focus:outline-none focus:shadow-outline">
                <option v-for="categoria in categories" :key="categoria.categoriaId" :value="categoria.categoriaId">{{
                  categoria.nombreCategoria }}</option>
              </select>
            </div>

            <div class="mb-4">
              <label class="block text-black text-sm font-bold mb-2">Imagen (opcional):</label>
              <input ref="updateArticuloImage" class="block w-full text-sm text-gray-500" type="file" accept="image/*">
            </div>

            <div class="mt-6">
              <button type="submit"
                class="bg-blue-500 hover:bg-blue-700 text-white font-bold py-2 px-4 rounded focus:outline-none focus:shadow-outline">
                Actualizar
              </button>
            </div>
          </form>
        </div>
      </div>

      <!-- Create Category Modal -->
      <div v-if="showCreateCategoryModal" class="fixed inset-0 flex items-center justify-center bg-black bg-opacity-50">
        <CrearCategoriaModal @close-modal="toggleCreateCategoryModal" @category-created="fetchCategories" />
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted, watch } from 'vue'
import CrearCategoriaModal from '../components/CrearCategoriaModal.vue'; // Import the modal component
import EditarCategoriaModal from '../components/EditarCategoriaModal.vue';
import axiosClient from '../axiosClient'; // Adjust the path according to your project structure
const updateArticuloImage = ref(null);
const showCreateCategoryModal = ref(false);
const showEditarCategoriaModal = ref(false);
const articulos = ref([]);
const newArticuloCategoriaId = ref(1);  // Store the selected category ID
const categories = ref([{ categoriaId: null, nombreCategoria: "Todos" }]); // Add "Todos" as default
const selectedCategory = ref(null);
const newArticuloName = ref('');
const newArticuloPrice = ref('');
const updateArticuloName = ref('');
const updateArticuloPrice = ref('');
const updateArticuloCategoriaId = ref(1);
const showDeleteModal = ref(false);
const showUpdateModal = ref(null);
const articuloToDelete = ref(null);
const articuloToUpdate = ref(null);
const defaultImage = "https://placehold.co/400"; // URL de la imagen por defecto
const previewImage = ref(defaultImage); // Inicia con la imagen por defecto
const newArticuloImage = ref(null);

// Manejar el cambio de imagen
const onImageChange = (event) => {
  const file = event.target.files[0]; // Obtener el archivo
  if (file) {
    newArticuloImage.value = file; // Guardar el archivo en la variable
    previewImage.value = URL.createObjectURL(file); // Generar la URL para la vista previa
  } else {
    previewImage.value = defaultImage; // Restaurar la imagen por defecto
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
  updateArticuloCategoriaId.value = articulo.categoriaID;
  console.log(updateArticuloCategoriaId)
  updateArticuloPrice.value = articulo.precio;
  showUpdateModal.value = true;
}

function cancelUpdateArticulo() {
  articuloToUpdate.value = null;
  showUpdateModal.value = false;
}
// Fetch rooms and categories initially
const fetchArticulos = () => {
  console.log(selectedCategory?.value);
  const categoriaID = selectedCategory?.value || null; // Use the selected category ID if available
  const url = categoriaID
    ? `/api/Articulos/GetArticulos?categoriaID=${categoriaID}`
    : "/api/Articulos/GetArticulos";

  articulos.value = []; // Clear the articles list before fetching

  axiosClient.get(url)
    .then(({ data }) => {
      if (data && data.ok) {
        articulos.value = data.data;
      } else {
        console.error("Failed to fetch articulos:", data.message);
      }
    })
    .catch((error) => {
      console.error("Error fetching articulos:", error);
    });
};


// Create a new room
const createArticulo = async () => {
  if (newArticuloName.value && newArticuloPrice.value && newArticuloImage.value) {
    try {
      const formData = new FormData();
      formData.append("nombre", newArticuloName.value);
      formData.append("precio", newArticuloPrice.value);
      formData.append("categoriaID", newArticuloCategoriaId.value);
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
      newArticuloCategoriaId.value = null;
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
  console.log(updateArticuloName.value, updateArticuloPrice.value, updateArticuloCategoriaId.value);
  if (updateArticuloName.value && updateArticuloPrice.value && updateArticuloCategoriaId.value) {
    try {
      // Step 1: Update article details (name, price, and category)
      const updateResponse = await axiosClient.put(
        `/api/Articulos/UpdateArticulo?id=${articuloID}&nombre=${encodeURIComponent(updateArticuloName.value)}&precio=${updateArticuloPrice.value}&categoriaID=${updateArticuloCategoriaId.value}`
      );

      alert(updateResponse.data.message);

      // Step 2: Check if an image is provided for upload
      const imageFile = updateArticuloImage.value?.files[0];

      // Only proceed with image update if a file is selected
      if (imageFile) {
        const formData = new FormData();
        formData.append('articuloID', articuloID);
        formData.append('nuevaImagen', imageFile);

        // Step 3: Send the image to the UpdateArticuloImage endpoint
        const imageResponse = await axiosClient.put('/api/Articulos/UpdateArticuloImage', formData, {
          headers: {
            'Content-Type': 'multipart/form-data',
          },
        });

        alert(imageResponse.data.message);
      }

      // Step 4: Refresh the list of articles and reset the form
      fetchArticulos();
      updateArticuloName.value = '';
      updateArticuloPrice.value = null;
      updateArticuloCategoriaId.value = null; // Reset the category selection
      updateArticuloImage.value.value = null; // Reset the image input
      cancelUpdateArticulo();
    } catch (error) {
      console.error('Error updating articulo:', error);
      alert('Error updating articulo');
    }
  } else {
    alert("Por favor ingresa el nombre, precio y categoría");
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
  fetchCategories();
  fetchArticulos();
});

// LOGICA CATEGORIAS

const fetchCategories = () => {
  axiosClient.get("/api/CategoriaArticulos/GetCategorias")
    .then(({ data }) => {
      if (data && data.ok) {
        categories.value = [{ categoriaId: null, nombreCategoria: "Todos" }, ...data.data];
      } else {
        console.error('Failed to fetch categories:', data.message);
      }
    })
    .catch(error => {
      console.error('Error fetching categories:', error);
    });
};

// Toggle modal visibility
const toggleCreateCategoryModal = () => {
  showCreateCategoryModal.value = !showCreateCategoryModal.value;
};

const toggleEditarCategoriaModal = () => {
  showEditarCategoriaModal.value = !showEditarCategoriaModal.value;
};


watch(selectedCategory, (newCategory) => {
  const categoriaId = newCategory === "Todos" ? null : categories.value.find(c => c.nombreCategoria === newCategory)?.categoriaId;
  fetchArticulos();
});

const filterByCategory = (categoriaID) => {
  selectedCategory.value = categoriaID;
  fetchArticulos();
};

</script>

<style scoped>
/* Add custom styles if needed */
</style>