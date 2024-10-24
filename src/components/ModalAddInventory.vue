<template>
    <Teleport to="body" class="overflow-hidden">
      <Transition name="modal-outer" appear>
        <div class="fixed w-full h-full overflow-auto z-20 bg-black bg-opacity-80 backdrop-blur-lg top-0 left-0 flex justify-center px-8">
          <Transition name="modal-inner">
            <div class="w-4/6 bg-surface-900 rounded-3xl p-8 pb-12 flex flex-col">
              <h2 class="text-lg font-bold text-white">Agregar Artículo a Inventario</h2>
              <div class="container mx-auto">
                <!-- Contenedor con overflow-hidden y altura de 500px -->
                <div style="max-height: 80vh; overflow-y: auto;">
                  <div class="grid grid-cols-3 gap-4 mx-2">
                    <div v-for="item in availableItems" :key="item.articuloId" 
                         @click="toggleSeleccion(item)"
                         :class="{
                           'relative cursor-pointer text-white rounded-lg p-4 flex flex-col items-center justify-center border-2 border-transparent transition-colors': true,
                           'bg-secondary-900 border-primary-500 ring-4': seleccionados.includes(item)
                         }">
                      <div class="w-20 h-20 bg-gray-500 flex items-center justify-center rounded-md mb-2">
                        <img src="../assets/image59.svg" alt="Imagen del producto" class="w-full h-full object-cover" />
                      </div>
                      <p>{{ item.nombreArticulo }}</p>
                      <input 
                        v-model.number="itemQuantity[item.articuloId]" 
                        type="number" 
                        placeholder="Cantidad" 
                        class="bg-gray-800 text-white rounded-full border-0 w-full px-4 py-2 placeholder-gray-400 focus:outline-none focus:ring-2 focus:ring-purple-300"
                        min="1"
                      />
                      <button 
                        class="mt-2 bg-green-500 text-white px-2 py-1 rounded hover:bg-green-700"
                        @click.prevent="addItemToInventory(item.articuloId)"
                      >
                        Agregar
                      </button>
                    </div>
                  </div>
                </div>
              </div>
              <div class="flex mt-4 justify-between">
                <button type="button" 
                        class="btn-light text-md w-1/3 h-12 rounded-3xl border-2 border-purple-200"
                        @click="confirmarAccion">
                  <span v-if="!isLoading">Confirmar</span>
                  <span v-else>
                    <button type="button" class="cargando w-8 flex justify-center items-center font-semibold text-stone-800" disabled>
                      <svg class="motion-safe:animate-spin" enable-background="new 0 0 24 24" viewBox="0 0 24 24" xmlns="http://www.w3.org/2000/svg">
                        <path d="M5.1,16c-0.3-0.5-0.9-0.6-1.4-0.4c-0.5,0.3-0.6,0.9-0.4,1.4c0.3,0.5,0.9,0.6,1.4,0.4C5.2,17.1,5.3,16.5,5.1,16C5.1,16,5.1,16,5.1,16z M4.7,6.6C4.2,6.4,3.6,6.5,3.3,7C3.1,7.5,3.2,8.1,3.7,8.4C4.2,8.6,4.8,8.5,5.1,8C5.3,7.5,5.2,6.9,4.7,6.6z M20.3,8.4c0.5-0.3,0.6-0.9,0.4-1.4c-0.3-0.5-0.9-0.6-1.4-0.4c-0.5,0.3-0.6,0.9-0.4,1.4C19.2,8.5,19.8,8.6,20.3,8.4z M4,12c0-0.6-0.4-1-1-1s-1,0.4-1,1s0.4,1,1,1S4,12.6,4,12z M7.2,18.8c-0.5,0.1-0.9,0.7-0.7,1.2c0.1,0.5,0.7,0.9,1.2,0.7c0.5-0.1,0.9-0.7,0.7-1.2C8.3,19,7.8,18.7,7.2,18.8z M21,11c-0.6,0-1,0.4-1,1s0.4,1,1,1s1-0.4,1-1S21.6,11,21,11z M20.3,15.6c-0.5-0.3-1.1-0.1-1.4,0.4c-0.3,0.5-0.1,1.1,0.4,1.4c0.5,0.3,1.1,0.1,1.4-0.4c0,0,0,0,0,0C20.9,16.5,20.8,15.9,20.3,15.6z M17,3.3c-0.5-0.3-1.1-0.1-1.4,0.4c-0.3,0.5-0.1,1.1,0.4,1.4c0.5,0.3,1.1,0.1,1.4-0.4c0,0,0,0,0,0C17.6,4.2,17.5,3.6,17,3.3z M16.8,18.8c-0.5-0.1-1.1,0.2-1.2,0.7c-0.1,0.5,0.2,1.1,0.7,1.2c0.5,0.1,1.1-0.2,1.2-0.7C17.6,19.5,17.3,19,16.8,18.8z M12,20c-0.6,0-1,0.4-1,1s0.4,1,1,1s1-0.4,1-1S12.6,20,12,20z M12,2c-0.6,0-1,0.4-1,1s0.4,1,1,1s1-0.4,1-1S12.6,2,12,2z" fill="#ffffff" />
                      </svg>
                    </button>
                  </span>
                </button>
                <button type="button" 
                        class="btn-danger text-md w-1/3 h-12 rounded-3xl transition-colors border-2 border-purple-200"
                        @click="emits('close')">Cancelar</button>
              </div>
              <button
                class="btn-danger absolute text-md w-12 h-12 -top-6 right-0 rounded-full border-2 border-purple-200"
                @click="emits('close')">X</button>
            </div>
          </Transition>
        </div>
      </Transition>
    </Teleport>
  </template>
  
  <script setup>
  import { ref, defineProps, defineEmits, onMounted, watch } from 'vue';
  import axiosClient from '../axiosClient';
  
  const props = defineProps({
    roomId: {
      type: Number,
      required: true
    },
    currentInventory: {
      type: Array,
      required: true
    }
  });
  
  const emits = defineEmits(["close", "refreshInventory"]);
  const itemQuantity = ref({})
  const isLoading = ref(false);
  const allItems = ref([]);
  const seleccionados = ref([]); // Array to store selected items
  const availableItems = ref([]);
  
  // Fetch all items
  const fetchAllItems = () => {
    isLoading.value = true;
    axiosClient.get("/api/Articulos/GetArticulos")
      .then(({ data }) => {
        allItems.value = data?.data || [];
        filterAvailableItems();
      })
      .catch(error => {
        console.error("Error fetching items:", error);
      })
      .finally(() => {
        isLoading.value = false;
      });
  };
  
  // Filter items that are not in the current inventory
  const filterAvailableItems = () => {
    if(props.currentInventory != null){
    const currentItemIds = props.currentInventory.map(item => item.articulo.articuloId);
    availableItems.value = allItems.value.filter(item => 
        !currentItemIds.includes(item.articuloId));
    }
  };

  const toggleSeleccion = (item) => {
  const index = seleccionados.value.findIndex(selectedItem => selectedItem.articuloId === item.articuloId);
  
  // If the item is already selected, remove it
  if (index !== -1) {
    seleccionados.value.splice(index, 1);
  } else {
    // Otherwise, add it to the selection
    seleccionados.value.push(item);
  }
};
  
  // Add item to inventory
  const addItemToInventory = (itemId) => {
    const cantidad = itemQuantity.value[itemId]; 
    axiosClient.post(`/api/Inventario/AddInventario?Cantidad=${cantidad}&ArticuloID=${itemId}&HabitacionID=${props.roomId}`)
      .then(() => {
        emits('refreshInventory'); // Notify parent to refresh inventory
        emits('close');
      })
      .catch(error => {
        console.error("Error adding item to inventory:", error);
        emits('close');
      });
  };
  
  // Fetch all items on component mount
  onMounted(() => {
    fetchAllItems();
  });
  
  // Re-filter items whenever the inventory changes
  watch(() => props.currentInventory, filterAvailableItems, { immediate: true });
  
  const closeModal = () => {
    emits('close');
  };
  </script>
  