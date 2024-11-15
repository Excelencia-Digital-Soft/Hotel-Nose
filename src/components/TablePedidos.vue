<template>

  <Transition name="modal-outer" appear>
    <div
      class="fixed w-full h-full overflow-auto z-20 bg-black bg-opacity-80 backdrop-blur-lg top-0 left-0 flex justify-center p-2">
      <Transition name="modal-inner">
        <div class="w-full flex flex-col justify-between  mt-4" style="max-height: 90vh; overflow-y: auto;">
          <table class="w-full text-xs text-white">
            <thead>
              <tr>
                <th class=" bg-secondary-800 border-white border-r-2  text-lg shadow-sm">Articulo</th>
                <th class=" bg-secondary-800 border-white border-r-2  text-lg shadow-sm">Producto</th>
                <th class=" bg-secondary-800 border-white border-r-2 w-1/6  text-lg shadow-sm ">Cantidad</th>
                <th class=" bg-secondary-800 w-12  text-sm shadow-sm"></th>
                <th class=" bg-secondary-800 w-12  text-sm shadow-sm"></th>
              </tr>
            </thead>
            <tbody>
              <tr class="hover:shadow-md border-2 h-14 text-lg border-primary-500 hover:shadow-secondary-400 text-white m-2 rounded-xl" v-for="(detalle, index) in props.selectedList"
                :key="detalle.articuloId">
                <td class="rounded-md text-center shadow-sm border-r-2">
                  {{ detalle.articuloId }}
                </td>
                <td class="rounded-md shadow-sm border-r-2 pl-4">
                  {{ detalle.nombreArt }}
                </td>
                <td class="rounded-md shadow-sm   text-center">
                  {{ detalle.cantidadArt }}
                </td>
                <td><button @click="quitarRegistro(index)" type="button"
                class="btn-danger rounded-xl text-xl h-12 text-white w-full  flex justify-center items-center mt-1  material-symbols-outlined">delete</button>
                </td>
                <button @click="entregarRegistro(index)" type="button"
                class="btn-primary rounded-xl text-xl h-12 text-white w-full  flex justify-center items-center mt-1 p-2 ">Entregar<span class="material-symbols-outlined">arrow_forward</span></button>
              </tr>

            </tbody>
          </table>
          <div class="flex w-full items-center">

            <button type="button"
              class="btn-danger -bottom-8 right-8 text-md w-1/3 h-12  rounded-2xl transition-colors border-2 border-purple-200 mx-4"
              @click="emits('close')">volver</button>
            <button
              class="w-full text-white font-bold btn-primary rounded-2xl  flex items-center justify-evenly cursor-pointer  px-5 h-12 mr-4 border-2 border-purple-200"
              id="signUp">
              Terminar entrega
              <span class="material-symbols-outlined">arrow_forward</span>
            </button>
          </div>



        </div>
      </Transition>
    </div>
  </Transition>

</template>

<script setup>
import { computed, ref, watch } from 'vue';
import axiosClient from '../axiosClient';

const props = defineProps({
  selectedList: Array,
  habitacion: Object,
});
let productList = ref([]);
let selectedItem = ref(null);
let habitacionSeleccionada = ref(null);
const emits = defineEmits(['close', 'update:productList']);


watch(() => props.habitacion, (newHabitacion) => {
  if (newHabitacion) {
    habitacionSeleccionada.value = newHabitacion;
    console.log("Selected Habitacion:", habitacionSeleccionada.value);
  }
});

watch(() => props.selectedList, (newList) => {
  if (newList) {
    console.log("Listado de Productos",props.selectedList)
    emits('update:productList', props.selectedList);
  }
}, { deep: true });  // Activa el modo 'deep' para escuchar cambios en las propiedades internas
//Como "cantidad" de productos

// Método para quitar un registro
const quitarRegistro = (index) => {
  // Quitar el producto de productList
  props.selectedList.splice(index, 1);

  // Emitir el cambio hacia el padre
  emits('update:productList', props.selectedList);
};

const entregarRegistro = async (index) => {
  if (!habitacionSeleccionada.value) {
    await new Promise((resolve) => {
      const stopWatching = watch(
        () => habitacionSeleccionada.value,
        (newVal) => {
          if (newVal) {
            stopWatching(); // Stop watching after habitacionSeleccionada is updated
            resolve();
          }
        }
      );
    });
  }
  const encargoId = props.selectedList[index].encargosId; // Extract the encargoId
  selectedItem.value = {
    articuloId: props.selectedList[index].articuloId,
    cantidad: props.selectedList[index].cantidadArt,
  };

  agregarConsumos([selectedItem.value], encargoId);

  props.selectedList.splice(index, 1);
  emits("update:productList", props.selectedList);
};

const agregarConsumos = (selectedItems, encargoId) => {
  console.log("huh ", habitacionSeleccionada.value)
  axiosClient.post(
    `/ConsumoGeneral?habitacionId=${habitacionSeleccionada.value.habitacionId}&visitaId=${habitacionSeleccionada.value.visitaId}`,
    selectedItems // Send selectedItems directly as the body
  )
  .then(response => {
    console.log('Consumo agregado exitosamente:', response.data);
    setEncargoEntregado(encargoId);  // Only pass articuloId of the selected item
  })
  .catch(error => {
    console.error('Error al agregar consumo:', error);
  });
};

const setEncargoEntregado = (encargoId) => {
    // Send a POST request to mark the encargo as delivered
    axiosClient.post(`/api/Encargos/EntregarEncargo?encargoId=${encargoId}`)
      .then(response => {
        console.log('Encargo entregado exitosamente:', response.data);
      })
      .catch(error => {
        console.error('Error al entregar el encargo:', error);
      });

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