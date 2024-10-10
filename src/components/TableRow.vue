<template>
  <div class="table-body" style="max-height: 150px; overflow-y: auto;">
    <table class="w-full text-white">
      <thead>
        <tr>
          <th class="border bg-secondary-800 rounded-sm text-sm shadow-sm">Item</th>
          <th class="border bg-secondary-800 rounded-sm text-sm shadow-sm">Producto</th>
          <th class="border bg-secondary-800 rounded-sm text-sm shadow-sm">Precio</th>
          <th class="border bg-secondary-800 rounded-sm text-sm shadow-sm">Cantidad</th>
          <th class="border bg-secondary-800 rounded-sm text-sm shadow-sm">Total</th>
          <th class="w-10 bg-white"></th>
          <th class="w-10 bg-white"></th>
        </tr>
      </thead>
      <tbody>
        <tr class="hover:shadow-md text-white" v-for="(detalle, index) in productList" :key="detalle.articuloId">
          <td class="rounded-md text-center shadow-sm">
            {{ index + 1 }}
          </td>
          <td class="rounded-md shadow-sm">
            <input v-model="detalle.nombreArticulo" type="text" class="w-full bg-inherit border-0" placeholder="Detalles">
          </td>
          <td class="rounded-md w-1/5 pl-2 shadow-sm">
            <input v-model="detalle.precio" type="number" inputmode="numeric" class="min-w-[80px] w-full bg-inherit border-0" placeholder="$0">
          </td>
          <td class="rounded-md shadow-sm  w-24">
            <input  type="text" class="w-full  bg-inherit border-0" placeholder="1">
          </td>


          <button @click="quitarRegistro(index)" type="button"
            class="text-xl h-12 text-white w-full  flex justify-center items-center border-2 bg-red-300 hover:shadow-lg hover:shadow-red-500/50 hover:bg-red-400 border-red-200 rounded-sm shadow-sm transition duration-150 ease-out md:ease-in material-symbols-outlined">delete</button>
        </tr>

      </tbody>
    </table>
  </div>
</template>

<script setup>
import { computed, ref, watch } from 'vue';

const props = defineProps({
  selectedList: Array,
});
let productList = ref([]);
const emits = defineEmits(['update:productList']);

watch(() => props.selectedList, (newList) => {
  console.log("Nuevo listado detectado:", newList);
  if (newList) {
    productList.value = newList;
    console.log("escuchamos el cambio", productList.value);
  }
});

// Método para quitar un registro
const quitarRegistro = (index) => {
  // Quitar el producto de productList
  productList.value.splice(index, 1);
  
  // Emitir el cambio hacia el padre
  emits('update:productList', productList.value);
};
</script>
<!-- {
  "idFact": 0,
  "idCDeuda": 0,
  "idAlicuota": 0,
  "detalle": "string",
  "precio": 0,
  "anulado": true
} -->
<style scoped>
h1 {
  font-family: Inter var;
  margin-top: 12px;
  font-weight: 600;
}

h3 {
  font-family: Inter var;
  font-size: 1rem;
}

.addB {
  margin-top: 12px;
}
</style>