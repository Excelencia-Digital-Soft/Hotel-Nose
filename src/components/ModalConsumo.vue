<template>
  <Teleport to="body" class="overflow-hidden">
    <Transition name="modal-outer" appear>
      <div
        class="fixed w-full h-full overflow-auto z-20 bg-black bg-opacity-80 backdrop-blur-lg top-0 left-0 flex justify-center px-8">
        <Transition name="modal-inner">
          <div class="w-11/12 md:w-4/6 h-[90%] bg-neutral-800 fixed top-6 flex flex-col bg-opacity-60 backdrop-blur-lg justify-evenly items-start p-8 pb-12 rounded-3xl self-start mt-0 border-y-4 border-accent-400">
            
            <section class="flex space-x-1 mb-4 ">
              <div class="pr-4">
              <label class="text-lg font-bold text-white whitespace-nowrap">Lista de Productos</label> 
              <input type="text" v-model="keyword"
                class="focus:ring-purple-500 border-2 w-full focus hover:shadow-lg hover:shadow-purple-500/50 border-purple-200 rounded-3xl transition-colors  "
                placeholder="Buscar productos" />
              </div>
              <!-- Categoría de productos -->
              <div class="flex flex-col items-center">
                <label for="categoryFilter" class="text-white font-semibold whitespace-nowrap">Filtrar por categoría:</label>
                <select id="categoryFilter" v-model="selectedCategory" class="focus:ring-purple-500 border-2 w-full  focus hover:shadow-lg hover:shadow-purple-500/50 border-purple-200 rounded-3xl transition-colors  ">
                  <option :value="null">Todas</option>
                  <option v-for="category in categorias" :key="category.categoriaId" :value="category">
                    {{ category.nombreCategoria }}
                  </option>
                </select>
              </div>
            </section>


            <div class="container mx-auto">
              <!-- Contenedor con overflow-hidden y altura de 500px -->
              <div class="h-80" style="max-height: 40vh; overflow-y: auto;">
                <div class="grid grid-cols-2 md:grid-cols-4 gap-2 mx-2">
                  <!-- Iteramos sobre los productos filtrados -->
                  <div v-for="producto in filteredProductos" :key="producto.articuloId"
                    @click="toggleSeleccion(producto)" :class="{
                      'relative hover:bg-accent-900 hover:bg-opacity-50 cursor-pointer text-white text-center rounded-lg p-4 flex flex-col items-center justify-center transition-colors duration-50 ease-out md:ease-in': true,
                      ' bg-accent-800 border-primary-400 border-x-2':seleccionados.some(p => p.articuloId === producto.articuloId)
                    }">
                    <!-- Imagen del producto -->
                    <div class="w-16 h-16 flex items-center justify-center rounded-md mb-2">
                      <img :src="producto.imageUrl || '../assets/image59.svg'" alt="Imagen del producto"
                        class="w-full h-full object-cover" />
                    </div>
                    <!-- Nombre del producto -->
                    <p>{{ producto.nombreArticulo }}</p>
                    <p>En stock: {{ producto.cantidad }}</p>
                  </div>
                </div>
              </div>
            </div>

            <!-- TABLE CONTENT -->
            <TableRow class="w-full" :selectedList="seleccionados" @update:productList="actualizarSeleccionados"/>
            <h3 class="text-md text-white font-semibold">{{ props.name }}</h3>
            <h3 class="text-md text-white font-semibold">¿Seguro que desea confirmar?</h3>
            <div class="flex">
              <button type="button"
                class="btn-secondary absolute -bottom-8 left-6 md:left-16 text-md w-1/3 h-16 rounded-3xl"
                @click="confirmarAccion">
                <span v-if="!isLoading">Confirmar</span>
                <span v-else>
                  <button type="button"
                    class="cargando w-8 flex justify-center items-center font-semibold text-stone-800" disabled>
                    <svg class="motion-safe:animate-spin" enable-background="new 0 0 24 24" viewBox="0 0 24 24"
                      xmlns="http://www.w3.org/2000/svg">
                      <path
                        d="M5.1,16c-0.3-0.5-0.9-0.6-1.4-0.4c-0.5,0.3-0.6,0.9-0.4,1.4c0.3,0.5,0.9,0.6,1.4,0.4C5.2,17.1,5.3,16.5,5.1,16C5.1,16,5.1,16,5.1,16z M4.7,6.6C4.2,6.4,3.6,6.5,3.3,7C3.1,7.5,3.2,8.1,3.7,8.4C4.2,8.6,4.8,8.5,5.1,8C5.3,7.5,5.2,6.9,4.7,6.6z M20.3,8.4c0.5-0.3,0.6-0.9,0.4-1.4c-0.3-0.5-0.9-0.6-1.4-0.4c-0.5,0.3-0.6,0.9-0.4,1.4C19.2,8.5,19.8,8.6,20.3,8.4z M4,12c0-0.6-0.4-1-1-1s-1,0.4-1,1s0.4,1,1,1S4,12.6,4,12z M7.2,18.8c-0.5,0.1-0.9,0.7-0.7,1.2c0.1,0.5,0.7,0.9,1.2,0.7c0.5-0.1,0.9-0.7,0.7-1.2C8.3,19,7.8,18.7,7.2,18.8z M21,11c-0.6,0-1,0.4-1,1s0.4,1,1,1s1-0.4,1-1S21.6,11,21,11z M20.3,15.6c-0.5-0.3-1.1-0.1-1.4,0.4c-0.3,0.5-0.1,1.1,0.4,1.4c0.5,0.3,1.1,0.1,1.4-0.4c0,0,0,0,0,0C20.9,16.5,20.8,15.9,20.3,15.6z M17,3.3c-0.5-0.3-1.1-0.1-1.4,0.4c-0.3,0.5-0.1,1.1,0.4,1.4c0.5,0.3,1.1-0.1,1.4-0.4c0,0,0,0,0,0C17.6,4.2,17.5,3.6,17,3.3z M16.8,18.8c-0.5-0.1-1.1,0.2-1.2,0.7c-0.1,0.5,0.2,1.1,0.7,1.2c0.5,0.1,1.1-0.2,1.2-0.7C17.6,19.5,17.3,19,16.8,18.8z M12,20c-0.6,0-1,0.4-1,1s0.4,1,1,1s1-0.4,1-1S12.6,20,12,20z M12,2c-0.6,0-1,0.4-1,1s0.4,1,1,1s1-0.4,1-1S12.6,2,12,2z"
                        fill="#ffffff" />
                    </svg>
                  </button>
                </span>
              </button>
              <button type="button"
                class="btn-danger absolute -bottom-8 right-6 md:right-16 text-md w-1/3 h-16 rounded-3xl transition-colors border-2 border-purple-200"
                @click="emits('close')">cancelar</button>
            </div>
            <button class="btn-danger absolute text-md w-12 h-12 -top-6 right-0 rounded-full border-2 border-purple-200"
              @click="emits('close')">X</button>
          </div>
        </Transition>
      </div>
    </Transition>
  </Teleport>
</template>
<script setup>
import { onMounted, ref, computed } from 'vue';
import axiosClient from '../axiosClient';
import { fetchImage } from '../services/imageService';
import TableRow from './TableRow.vue';

const props = defineProps({
  name: String,
  habitacionID: Number,
  consumoHabitacion:Boolean
});

const emits = defineEmits(["close", "confirmaAccion"]);

let isLoading = ref(false);
const productos = ref([]);
const categorias = ref([]); // Store categories with ID-to-name mapping
let seleccionados = ref([]);
const selectedCategory = ref(null); // Reactive variable for selected category
const keyword = ref("");
let getInv = ref("");
onMounted(() => {
  if (!props.consumoHabitacion){
    getInv.value = "/GetInventarioGeneral"
  }
  else{
     getInv.value = `api/Inventario/GetInventario?habitacionID=${props.habitacionID}`
  }
  fetchCategorias(); // Fetch categories when the component mounts
  fetchArticulos();
  console.log(props.habitacionID);
});

const filteredProductos = computed(() => {
  return productos.value.filter(producto => {
    const matchesCategory =
      !selectedCategory.value || producto.categoriaID === selectedCategory.value.categoriaId;
    const matchesKeyword =
      !keyword.value || producto.nombreArticulo.toLowerCase().includes(keyword.value.toLowerCase());
    return matchesCategory && matchesKeyword;
  });
});


// Dropdown options for categories
const uniqueCategories = computed(() => categorias.value.map(c => c.nombreCategoria));

const fetchArticulos = async () => {
  try {
    const response = await axiosClient.get(getInv.value);
    if (response.data && response.data.data) {
      // Filter articles with stock > 0 and fetch their images
      productos.value = await Promise.all(
        response.data.data
          .filter(a => a.cantidad > 0)
          .map(async (articulo) => {
            const imageUrl = await fetchImage(articulo.articulo.articuloId); // Fetch the image URL
            return {
              ...articulo.articulo,
              cantidad: articulo.cantidad,
              imageUrl, // Include the image URL
            };
          })
      );
    } else {
      console.error('Datos de la API no válidos:', response.data);
    }
  } catch (error) {
    console.error('Error al obtener los artículos:', error);
  }
};
// Fetch categories from the API
const fetchCategorias = async () => {
  try {
    const response = await axiosClient.get("/api/CategoriaArticulos/GetCategorias");
    if (response.data && response.data.data) {
      categorias.value = response.data.data; // Store full category data (ID and name)
    } else {
      console.error("Datos de la API de categorías no válidos:", response.data);
    }
  } catch (error) {
    console.error("Error al obtener las categorías:", error);
  }
};

const toggleSeleccion = (producto) => {
  const index = seleccionados.value.findIndex(p => p.articuloId === producto.articuloId);
  if (index === -1) {
    seleccionados.value.push({
      articuloId: producto.articuloId,
      nombreArticulo: producto.nombreArticulo,
      precio: producto.precio,
      cantidad: 1,
      maximo: producto.cantidad,
    });
  } else {
    seleccionados.value.splice(index, 1);
  }
};

const actualizarSeleccionados = (nuevaLista) => {
  seleccionados.value = nuevaLista;
};

const confirmarAccion = () => {
  isLoading.value = true;
  emits("confirmaAccion", seleccionados.value);
  emits("close");
};
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