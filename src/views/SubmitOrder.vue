<template>
  <Transition name="modal-outer" appear>
    <Transition name="modal-inner">
      <div class=" w-full relative  flex flex-col justify-evenly items-start p-4 pb-12 rounded-3xl self-start mt-0">
        <h2 class="text-lg font-bold text-white">Lista de Productos</h2>
        <input type="text" v-model="keyword"
          class="focus:ring-purple-500 border-2 w-full focus hover:shadow-lg hover:shadow-purple-500/50 border-purple-200 rounded-3xl transition-colors mb-4 "
          placeholder="Buscar productos" />

        <!-- Category Filter - Made more mobile-friendly -->
        <div class="flex flex-wrap items-center bg-surface-700 rounded-xl p-2 mb-4">
          <div class="text-white mr-2">Filtrar por:</div>
          <button v-for="category in categories" :key="category.categoriaId"
            @click="filterByCategory(category.categoriaId)"
            :class="['py-2 px-4 m-1 rounded hover:bg-secondary-100 flex items-center text-sm', selectedCategory === category.categoriaId ? 'bg-secondary-500 text-white' : 'bg-gray-300']">
            {{ category.nombreCategoria }}
            <span v-if="selectedCategory === category.categoriaId && category.categoriaId!=null && category.categoriaId!=1" class="material-symbols-outlined ml-1">
            </span>
          </button>
        </div>

        <div class="container mx-auto">
          <!-- Contenedor con overflow-hidden y altura de 500px -->
          <div style="max-height: 65vh; overflow-y: auto;">
            <div v-if="isLoading" class="text-center text-gray-500 py-4">
              Loading...
            </div>
            <div v-else class="grid grid-cols-2 sm:grid-cols-3 md:grid-cols-4 lg:grid-cols-5 gap-4 m-2">
              <div v-for="producto in filteredProductos" :key="producto.articuloId" @click="toggleSeleccion(producto)"
                :class="{
                  'relative hover:bg-surface-700 cursor-pointer text-white rounded-lg p-4 flex flex-col items-center justify-between': true,
                  'ring-4 bg-secondary-900 ring-primary-500': seleccionados.includes(producto),
                }">
                <!-- Product Image -->
                <div class="w-20 h-20 bg-gray-500 flex items-center justify-center rounded-md mb-2">
                  <img :src="producto.imageUrl || '/assets/image59.svg'" alt="Product Image"
                    class="w-full h-full object-cover" />
                </div>
                <!-- Product Details -->
                <p>{{ producto.nombreArticulo }}</p>
                <p class="text-sm text-green-600">${{ producto.precio }}</p>
              </div>
            </div>
          </div>

        </div>

        <!-- TABLE CONTENT -->
        <TableRowModal v-show="show" :selectedList="seleccionados" :visitaId="visitaId"
          @update:productList="actualizarSeleccionados" @close="toggleTable" />
        <button @click="toggleTable"
          class="w-full text-white font-bold principal-convination-color rounded-2xl  flex items-center justify-evenly cursor-pointer  px-5 h-12  mt-4"
          id="signUp">
          Ver Listado<span class="material-symbols-outlined">arrow_forward</span>
        </button>
      </div>

    </Transition>
  </Transition>
</template>

<script setup>
import { onMounted, ref, computed, watch } from 'vue';
import { fetchArticulos } from '../services/imageService.js';
import axiosClient from '../axiosClient';
import TableRowModal from '../components/TableRowModal.vue';
import { useRoute } from 'vue-router';
import { useRouter } from 'vue-router';

const router = useRouter();
const route = useRoute(); // Captura de la ruta actual
const habitacionId = ref(route.params.habitacionId);  // Obtener habitacionId desde la URL
let visitaId = ref()
const keyword = ref('');
const productos = ref([])
const show = ref(false)
const isLoading = ref(false); // Add loading state
const categories = ref([]); // Categories array
const selectedCategory = ref(null); // Selected category ID
const InstitucionID = ref(null);

const computedProductos = computed(() => productos.value.filter(i => i.nombreArticulo.toLowerCase().includes(keyword.value.toLowerCase())))

// Filtered products based on selected category and search keyword
const filteredProductos = computed(() => {
  let filtered = computedProductos.value;

  if (selectedCategory.value) {
    filtered = filtered.filter(producto => producto.categoriaID === selectedCategory.value);
  }

  return filtered;
});

watch(productos, (newVal, oldVal) => {
  if (newVal !== oldVal) {
    console.log("Productos array updated:", newVal);
  }
});

onMounted(async () => {
  isLoading.value = true; // Set loading to true on mount
  fetchVisitaId();
  await fetchCategories();
  productos.value = await fetchArticulos();
  console.log('Productos in component:', productos.value);
  isLoading.value = false; // Set loading to false after data is fetched
  seleccionados = ref([])// le asignamos como variable reactiva en el montado para luego 
  getDatosLogin();
})
let seleccionados = null;

// Método para agregar o quitar productos del array 'seleccionados'
const toggleSeleccion = (producto) => {
  const index = seleccionados.value.indexOf(producto);
  if (index === -1) {
    // Si no está seleccionado, lo agregamos
    seleccionados.value.push(producto);
    console.log(seleccionados.value)
  } else {
    // Si ya está seleccionado, lo quitamos
    seleccionados.value.splice(index, 1);
  }
};

// Método para actualizar seleccionados cuando se emite desde TableRow
const actualizarSeleccionados = (nuevaLista) => {
  seleccionados.value = nuevaLista;
};

const toggleTable = () => {
  show.value = !show.value
}
// Función para obtener el VisitaID de la habitación seleccionada
const fetchVisitaId = () => {
  console.log(habitacionId.value)
  if (habitacionId.value) {
    axiosClient.get(`/GetVisitaId?idHabitacion=${habitacionId.value}`)
      .then(response => {
        visitaId.value = response.data.data;
        console.log('VisitaID:', visitaId.value);
        if (!response.data.ok) {
          alert('Esta habitacion no tiene permitido hacer pedidos')
          router.push({ name: 'Home' });
        }
      })
      .catch(error => {
        console.error('Error al obtener la VisitaID:', error);

      });

  } else {
    alert('Esta habitacion no tiene permitido hacer pedidos')
    router.push({ name: 'Home' });
  }
};

const fetchCategories = () => {
  axiosClient.get(`/api/CategoriaArticulos/GetCategorias?InstitucionID=1`)
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

// Function to filter products by category
const filterByCategory = (categoriaId) => {
  selectedCategory.value = categoriaId;
};

// Function to toggle edit category modal (Placeholder, implement your logic here)
const toggleEditarCategoriaModal = (categoriaId) => {
  console.log('Edit category:', categoriaId);
  // Implement your edit category modal logic here
};

import { useAuthStore } from '../store/auth.js'; // Import the auth store

const authStore = useAuthStore();

function getDatosLogin(){
    InstitucionID.value = authStore.institucionID;
}
</script>

<style scoped>
.modal-outer-enter-active,
.modal-outer-leave-active {
  transition: opacity 0.3s cubic-bezier(0.52, 0.02, 0.19, 1.02);
}

.modal-outer-enter-from,
.modal-outer-leave-to {
  opacity: 0;
}

.modal-inner-enter-active {
  transition: all 0.3s cubic-bezier(0.52, 0.02, 0.19, 1.02) 0.10s;
}

.modal-inner-leave-active {
  transition: all 0.3s cubic-bezier(0.52, 0.02, 0.19, 1.02);
}

.modal-inner-enter-from {
  opacity: 0;
  transform: scale(0.8);
}

.modal-inner-leave-to {
  transform: scale(0.8);
}
</style>
