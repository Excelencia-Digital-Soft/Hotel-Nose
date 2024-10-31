<template>

  <div class=" w-full relative  flex flex-col justify-evenly items-start p-4 pb-12 rounded-3xl self-start mt-0">
    <h2 class="text-lg font-bold text-white">Pedidos por Habitacion</h2>
    <div class="container mx-auto">
      <!-- Contenedor con overflow-hidden y altura de 500px -->
      <div style="max-height: 70vh; overflow-y: auto;">
        <div class="grid grid-cols-1 text-xl gap-4 m-2">
          <!-- Iteramos sobre los productos -->
          <div v-for="producto in productos" :key="producto.habitacionId" @click="toggleTable(producto.encargos)" class="relative border-4  hover:bg-surface-700 cursor-pointer text-white rounded-lg p-4 flex flex-col items-center justify-between">

            <!-- Nombre de habitacion -->
            <p>{{ producto.nombreHabitacion }}</p>
            
          </div>
        </div>
      </div>
    </div>

    <!-- TABLE CONTENT -->
    <TablePedidos v-show="show" :selectedList="seleccionados" @update:productList="actualizarSeleccionados"
      @close="toggleTable" />
  </div>


</template>

<script setup>
import { onMounted, ref, computed } from 'vue';
import axiosClient from '../axiosClient';
import TablePedidos from '../components/TablePedidos.vue';

let isLoading = ref(false)
const productos = ref([])
const show = ref(false)

onMounted(() => {
  fetchArticulos();
  seleccionados = ref([])// le asignamos como variable reactiva en el montado para luego 
})
let seleccionados = null;


// Método para actualizar seleccionados cuando se emite desde TableRow
const actualizarSeleccionados = (nuevaLista) => {
  seleccionados.value = nuevaLista;
};

const toggleTable = (encargos) => {
  seleccionados.value = encargos
  console.log(seleccionados.value)
  show.value = !show.value
  
}

const fetchArticulos = () => {
  axiosClient.get("/api/Encargos/Habitaciones")
    .then(({ data }) => {
      if (data && data.data) {
        console.log(data.data)
        productos.value = data.data

      } else {
        console.error('Datos de la API no válidos:', data);
      }
    })
    .catch(error => {
      console.error('Error al obtener las habitaciones:', error);
    });
}
</script>

<style scoped></style>