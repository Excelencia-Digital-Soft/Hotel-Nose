<template>
  <Teleport to="body" class="overflow-hidden">
    <Transition name="modal-outer" appear>
      <div class="fixed w-full h-full overflow-auto z-20 bg-black bg-opacity-80 backdrop-blur-lg top-0 left-0 flex justify-center px-8">
        <Transition name="modal-inner">
          <div
            class=" w-4/6  bg-surface-900 fixed top-6 flex flex-col justify-evenly items-start p-8 pb-12 rounded-3xl self-start mt-0  border-8 border-purple-300/75 ">
            <h2 class="text-lg font-bold text-white">Lista de Productos</h2>
            <input 
                type="text"
                v-model="keyword" 
                class="focus:ring-purple-500 border-2 w-full focus hover:shadow-lg hover:shadow-purple-500/50 border-purple-200 rounded-3xl transition-colors " 
                placeholder="Buscar productos"
            />
            <div class="container mx-auto">
              <!-- Contenedor con overflow-hidden y altura de 500px -->
              <div style="max-height: 50vh; overflow-y: auto;">
                <div class="grid grid-cols-3 gap-4 mx-2">
                  <!-- Iteramos sobre los productos -->
                  <div v-for="producto in computedProductos" :key="producto.articuloId" @click="toggleSeleccion(producto)"
                    :class="{
                    'relative  hover:bg-surface-700 cursor-pointer text-white rounded-lg p-4 flex flex-col items-center justify-center': true,
                    'ring-4 bg-secondary-900 ring-primary-500': seleccionados.includes(producto)
                  }">
                    <!-- Imagen del producto, en este caso un placeholder -->
                    <div class="w-20 h-20 bg-gray-500 flex items-center justify-center rounded-md mb-2">
                      <img src="../assets/image59.svg" alt="Imagen del producto" class="w-full h-full object-cover" />
                    </div>
                    <!-- Nombre del producto -->
                    <p>{{ producto.nombreArticulo }}</p>
                  </div>
                </div>
              </div>
            </div>

                <!-- TABLE CONTENT -->
                <TableRow class="w-full" :selectedList="seleccionados" @update:productList="actualizarSeleccionados"></TableRow>
            <h3 class="text-md text-white font-semibold">"{{ props.name }}"</h3>
            <h3 class="text-md text-white font-semibold">¿Seguro que desea confirmar?</h3>
            <div class="flex">
              <button type="button"
                class="btn-light absolute -bottom-8 left-8 text-md w-1/3 h-16 rounded-3xl border-2 border-purple-200"
                @click="confirmarAccion">
                <span v-if="!isLoading">Confirmar</span>
                <span v-else>
                  <button type="button" class="cargando w-8 flex justify-center items-center font-semibold text-stone-800" disabled>
                    <svg class="motion-safe:animate-spin" enable-background="new 0 0 24 24" viewBox="0 0 24 24"
                      xmlns="http://www.w3.org/2000/svg">
                      <path
                        d="M5.1,16c-0.3-0.5-0.9-0.6-1.4-0.4c-0.5,0.3-0.6,0.9-0.4,1.4c0.3,0.5,0.9,0.6,1.4,0.4C5.2,17.1,5.3,16.5,5.1,16C5.1,16,5.1,16,5.1,16z M4.7,6.6C4.2,6.4,3.6,6.5,3.3,7C3.1,7.5,3.2,8.1,3.7,8.4C4.2,8.6,4.8,8.5,5.1,8C5.3,7.5,5.2,6.9,4.7,6.6z M20.3,8.4c0.5-0.3,0.6-0.9,0.4-1.4c-0.3-0.5-0.9-0.6-1.4-0.4c-0.5,0.3-0.6,0.9-0.4,1.4C19.2,8.5,19.8,8.6,20.3,8.4z M4,12c0-0.6-0.4-1-1-1s-1,0.4-1,1s0.4,1,1,1S4,12.6,4,12z M7.2,18.8c-0.5,0.1-0.9,0.7-0.7,1.2c0.1,0.5,0.7,0.9,1.2,0.7c0.5-0.1,0.9-0.7,0.7-1.2C8.3,19,7.8,18.7,7.2,18.8z M21,11c-0.6,0-1,0.4-1,1s0.4,1,1,1s1-0.4,1-1S21.6,11,21,11z M20.3,15.6c-0.5-0.3-1.1-0.1-1.4,0.4c-0.3,0.5-0.1,1.1,0.4,1.4c0.5,0.3,1.1,0.1,1.4-0.4c0,0,0,0,0,0C20.9,16.5,20.8,15.9,20.3,15.6z M17,3.3c-0.5-0.3-1.1-0.1-1.4,0.4c-0.3,0.5-0.1,1.1,0.4,1.4c0.5,0.3,1.1,0.1,1.4-0.4c0,0,0,0,0,0C17.6,4.2,17.5,3.6,17,3.3z M16.8,18.8c-0.5-0.1-1.1,0.2-1.2,0.7c-0.1,0.5,0.2,1.1,0.7,1.2c0.5,0.1,1.1-0.2,1.2-0.7C17.6,19.5,17.3,19,16.8,18.8z M12,20c-0.6,0-1,0.4-1,1s0.4,1,1,1s1-0.4,1-1S12.6,20,12,20z M12,2c-0.6,0-1,0.4-1,1s0.4,1,1,1s1-0.4,1-1S12.6,2,12,2z"
                        fill="#ffffff" />
                    </svg>
                  </button>
                </span>
              </button>
              <button type="button"
                class="btn-danger absolute -bottom-8 right-8 text-md w-1/3 h-16  rounded-3xl transition-colors border-2 border-purple-200"
                @click="emits('close')">cancelar</button>
            </div>
            <button
              class="btn-danger absolute text-md  w-12 h-12 -top-6 right-0 rounded-full border-2 border-purple-200"
              @click="emits('close')">X</button>
          </div>
        </Transition>
      </div>
    </Transition>
  </Teleport>
</template>

<script setup>
import { onMounted, ref,computed } from 'vue';
import axiosClient from '../axiosClient';
import TableRow from './TableRow.vue';
const props = defineProps({
  name: String
});
const emits = defineEmits(["close", "confirmaAccion"]);
let isLoading = ref(false)
const keyword = ref('');
const productos = ref([])
const computedProductos = computed(() => productos.value.filter(i => i.nombreArticulo.toLowerCase().includes(keyword.value.toLowerCase())))


const confirmarAccion = () => {

  isLoading.value = true;
  emits("confirmaAccion",seleccionados.value)
  emits("close")
}
onMounted(() => {
  fetchArticulos();
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

const fetchArticulos = () => {
  axiosClient.get("/api/Articulos/GetArticulos")
    .then(({ data }) => {
      if (data && data.data) {
        console.log(data.data)
        productos.value = data.data.map(articulo => {
          return {
            ...articulo,  // Copia las propiedades del objeto original
            cantidad: 1   // Añade el campo 'cantidad' con valor 1
          };
        });

      } else {
        console.error('Datos de la API no válidos:', data);
      }
    })
    .catch(error => {
      console.error('Error al obtener las habitaciones:', error);
    });
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