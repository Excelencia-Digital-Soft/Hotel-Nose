<template>
    <div class="table-body mt-4" style="max-height: 150px; overflow-y: auto;">
      <table class="w-full text-white">
        <thead >
          <tr>
            <th class=" bg-secondary-800 rounded-sm text-sm shadow-sm">Item</th>
            <th class=" bg-secondary-800 rounded-sm text-sm shadow-sm">Producto</th>
            <th class=" bg-secondary-800 rounded-sm text-sm shadow-sm">Precio</th>
            <th class=" bg-secondary-800 rounded-sm text-sm shadow-sm">Cantidad</th>
            <th class=" bg-secondary-800 rounded-sm text-sm shadow-sm">Total</th>
          </tr>
        </thead>
        <tbody>
          <tr class="hover:shadow-md text-white" v-for="(detalle, index) in props.selectedList" :key="detalle.articuloId">
            <td class="rounded-md text-center shadow-sm">
              {{ index + 1 }}
            </td>
            <td class="rounded-md shadow-sm">
              {{detalle.nombreArticulo}}  
            </td>
            <td class="rounded-md w-1/5 pl-2 shadow-sm">
              {{ detalle.precio}}
            </td>
            <td class="rounded-md shadow-sm  w-24">
              <input v-model="detalle.cantidad" :max="detalle.maximo" :min="1" type="number"   @input="validateQuantity(detalle)"   class="w-full bg-inherit border-0" >
            </td>
            <td class="rounded-md w-1/5 pl-2 shadow-sm">
              {{ detalle.precio * detalle.cantidad}}
            </td>
  
            <button @click="quitarRegistro(index)" type="button"
              class="btn-danger rounded-xl text-xl h-12 text-white w-full  flex justify-center items-center mt-1  material-symbols-outlined">delete</button>
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
    if (newList) {
      emits('update:productList', props.selectedList);
    }
  }, { deep: true });  // Activa el modo 'deep' para escuchar cambios en las propiedades internas
                      //Como "cantidad" de productos
  
  // Método para quitar un registro
  const quitarRegistro = (index) => {
    // Quitar el producto de productList
    props.selectedList.splice(index, 1);
    
    // Emitir el cambio hacia el padre
    emits('update:productList',  props.selectedList);
  };
  
  const validateQuantity = (detalle) => {
    if (detalle.cantidad > detalle.maximo) {
      detalle.cantidad = detalle.maximo;  // Limit to max stock
    } else if (detalle.cantidad < 1) {
      detalle.cantidad = 1;  // Prevent quantity from going below 1
    }
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