<template>
  <Transition name="modal-outer" appear>
    <Transition name="modal-inner">
      <div class=" w-full relative  flex flex-col justify-evenly items-start p-4 pb-12 rounded-3xl self-start mt-0">
        <h2 class="text-lg font-bold text-white">Lista de Productos</h2>
        <input type="text" v-model="keyword"
          class="focus:ring-purple-500 border-2 w-full focus hover:shadow-lg hover:shadow-purple-500/50 border-purple-200 rounded-3xl transition-colors mb-4 "
          placeholder="Buscar productos" />
        <div class="container mx-auto">
          <!-- Contenedor con overflow-hidden y altura de 500px -->
          <div style="max-height: 65vh; overflow-y: auto;">
            <div class="grid grid-cols-3 md:grid-cols-4 lg:grid-cols-5 gap-4 m-2">
              <!-- Iteramos sobre los productos -->
              <div v-for="producto in computedProductos" :key="producto.articuloId" @click="toggleSeleccion(producto)"
                :class="{
          'relative  hover:bg-surface-700 cursor-pointer text-white rounded-lg p-4 flex flex-col items-center justify-between': true,
          'ring-4 bg-secondary-900 ring-primary-500': seleccionados.includes(producto)
        }">
                <!-- Imagen del producto, en este caso un placeholder -->
                <div class="w-20 h-20 bg-gray-500 flex items-center justify-center rounded-md mb-2">
                  <img :src="producto.imageUrl || '/assets/image59.svg'" alt="Imagen del producto" class="w-full h-full object-cover" />
                </div>
                <!-- Nombre del producto -->
                <p>{{ producto.nombreArticulo }}</p>
                <p class="text-sm text-green-600">${{ producto.precio }}</p>
              </div>
            </div>
          </div>
        </div>

        <!-- TABLE CONTENT -->
        <TableRowModal v-show="show" :selectedList="seleccionados" :visitaId="visitaId" @update:productList="actualizarSeleccionados"
          @close="toggleTable" />
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
import { onMounted, ref, computed } from 'vue';
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
const computedProductos = computed(() => productos.value.filter(i => i.nombreArticulo.toLowerCase().includes(keyword.value.toLowerCase())))
const show = ref(false)

onMounted(() => {
  fetchVisitaId();
  fetchArticulos();
  console.log(productos);
  seleccionados = ref([])// le asignamos como variable reactiva en el montado para luego 
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
        if (!response.data.ok){
          alert('Esta habitacion no tiene permitido hacer pedidos')
          router.push({ name: 'home' });
        }
      })
      .catch(error => {
        console.error('Error al obtener la VisitaID:', error);
        
      });
      
  }else{
    alert('Esta habitacion no tiene permitido hacer pedidos')
    router.push({ name: 'home' });
  }
};

const fetchImage = async (articuloId) => {
  try {
    const response = await axiosClient.get(`api/Articulos/GetImage/${articuloId}`, {
      responseType: 'blob', // Specify response type as blob to handle binary data
    });

    // Convert blob to a usable object URL
    const imageUrl = URL.createObjectURL(response.data);
    return imageUrl; // Return the object URL for the image
  } catch (error) {
    console.error(`Error al obtener imagen para articuloId ${articuloId}:`, error);
    return null; // Return default placeholder if error occurs
  }
};
const fetchArticulos = async () => {
  try {
    const response = await axiosClient.get("/GetInventarioGeneral");
    if (response.data && response.data.data) {
      // Filter out items with cantidad = 0
      const validItems = response.data.data.filter(item => item.cantidad > 0);

      // Fetch images and construct the products array
      productos.value = await Promise.all(
        validItems.map(async (item) => {
          const imageUrl = await fetchImage(item.articulo.articuloId); // Fetch image
          return {
            ...item.articulo, // Spread properties of 'articulo'
            cantidad: item.cantidad, // Add 'cantidad'
            imageUrl, // Add fetched image URL
          };
        })
      );

      console.log("Productos with Images (cantidad > 0):", productos.value); // Verify the structure
    } else {
      console.error("Datos de la API no válidos:", response.data);
    }
  } catch (error) {
    console.error("Error al obtener los productos:", error);
  }
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