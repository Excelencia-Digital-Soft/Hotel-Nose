<template>
  <Transition name="modal-outer" appear>
    <div
      class="fixed w-full h-full overflow-auto z-20 bg-black bg-opacity-80 backdrop-blur-lg top-0 left-0 flex justify-center p-2">
      <Transition name="modal-inner">
        <div class="w-full flex flex-col justify-between mt-4" style="max-height: 90vh; overflow-y: auto;">
          <table class="w-full text-xs text-white">
            <thead>
              <tr>
                <th class=" bg-secondary-800 border-white border-r-2 text-sm shadow-sm">Item</th>
                <th class=" bg-secondary-800 border-white border-r-2 text-sm shadow-sm">Producto</th>
                <th class=" bg-secondary-800 border-white border-r-2 text-sm shadow-sm">Precio</th>
                <th class=" bg-secondary-800 border-white border-r-2 w-1/6 text-sm shadow-sm">Cantidad</th>
                <th class=" bg-secondary-800 border-white border-r-2 text-sm shadow-sm">Total</th>
                <th class=" bg-secondary-800 w-12 text-sm shadow-sm"></th>
              </tr>
            </thead>
            <tbody>
              <tr class="hover:shadow-md hover:shadow-secondary-400 text-white m-2" 
                  v-for="(detalle, index) in productList" 
                  :key="detalle.articuloId">
                <td class="rounded-md text-center shadow-sm">
                  {{ index + 1 }}
                </td>
                <td class="rounded-md shadow-sm">
                  {{ detalle.nombreArticulo }}
                </td>
                <td class="rounded-md pl-2 shadow-sm">
                  {{ detalle.precio }}
                </td>
                <td class="rounded-md shadow-sm">
                  <!-- Quantity Input -->
                  <input 
                    v-model.number="detalle.cantidad" 
                    type="number" 
                    :max="detalle.maxCantidad" 
                    :min="1" 
                    class="w-full bg-inherit border-0">
                </td>
                <td class="rounded-md pl-2 shadow-sm">
                  {{ detalle.precio * detalle.cantidad }}
                </td>

                <button @click="quitarRegistro(index)" type="button"
                  class="btn-danger rounded-xl text-xl h-12 text-white w-full flex justify-center items-center mt-1 material-symbols-outlined">delete</button>
              </tr>
            </tbody>
          </table>
          <div class="flex w-full items-center">
            <button type="button"
              class="btn-danger -bottom-8 right-8 text-md w-1/3 h-12 rounded-2xl transition-colors border-2 border-purple-200 mx-4"
              @click="emits('close')">volver</button>
            <button @click="EnviarPedido"
              class="w-full text-white font-bold btn-primary rounded-2xl flex items-center justify-evenly cursor-pointer px-5 h-12 mr-4 border-2 border-purple-200"
              id="signUp">
              Encargar
              <span class="material-symbols-outlined">arrow_forward</span>
            </button>
          </div>
        </div>
      </Transition>
    </div>
  </Transition>
</template>

<script setup>
import { ref, watch } from 'vue';
import axiosClient from '../axiosClient';

// Props and Emits
const props = defineProps({
  selectedList: Array, // Input list from parent
  visitaId: Number,
});
const emits = defineEmits(['close', 'update:productList']);

// Reactive State
const productList = ref([]);

// Initialize productList with default quantities
watch(
  () => props.selectedList,
  (newList) => {
    if (newList) {
      productList.value = newList.map((item) => ({
        ...item,
        cantidad: 1, // Default quantity is 1
        maxCantidad: item.cantidad, // Preserve the original max cantidad
      }));
      emits('update:productList', props.selectedList);
    }
  },
  { immediate: true, deep: true }
);

// Watch for over-limit quantities and reset
watch(
  () => productList.value,
  (newList) => {
    newList.forEach((item) => {
      if (item.cantidad > item.maxCantidad) {
        item.cantidad = item.maxCantidad;
      }
    });
  },
  { deep: true }
);

// Send the Encargo
const EnviarPedido = () => {
  const dataToSend = productList.value.map((item) => ({
    cantidad: item.cantidad,
    articuloId: item.articuloId,
    visitaId: props.visitaId,
  }));

  axiosClient
    .post('/api/Encargos/AddEncargos', dataToSend)
    .then((res) => {
      console.log(res.data);
      alert('Encargado con éxito, tu pedido será entregado en breve');
      emits('close');
      window.location.reload();
    })
    .catch((error) => {
      console.error(error);
    });
};

// Remove a product from the list
const quitarRegistro = (index) => {
  productList.value.splice(index, 1);
  emits('update:productList', productList.value);
};
</script>

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