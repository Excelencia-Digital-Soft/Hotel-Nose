<template>
<div class="grid relative w-80">
  <label for="search" class="text-sm font-semibold leading-6 text-gray-800">Proveedor</label>
  <input 
      type="text"
      v-model="keyword"
      @input="showResults = true"
      @keydown.enter="enterProvOn"
      id="search" 
      class="w-full  relative focus hover:shadow-lg hover:shadow-purple-500/50 border-violet-200 rounded-3xl transition-colors focus:ring-purple-300 focus:border-purple-400 border-2 py-2 " 
      placeholder="Buscar proveedor"
      autocomplete="off" 
  />
  <ul v-show="showResults" id="toggleDropDown" style="max-height: 50vh; overflow-y: auto;" class="absolute w-3/4 top-16 mt-1 shadow-xl border-4 border-purple-200 bg-white rounded-xl transition duration-100 ease-out md:ease-in">
    <li v-for="proveedor in computedProveedores" :key="proveedor.idProv" class="p-2 pl-6 pr-6 hover:bg-lime-300 shadow-md cursor-pointer" @click="seleccionarProveedor(proveedor)">
      {{ proveedor.nombre }}
    </li>
  </ul>
  <button @click="showResults = !showResults" id="searcher" class="btn-searcher absolute h-2/3 w-26 flex justify-center items-center cursor-pointer top-6 left-64 px-5 border-2 bg-gradient-to-r from-indigo-300 via-purple-400 to-purple-300  text-white shadow-lg hover:shadow-purple-500/50 border-purple-200 rounded-3xl transition-colors">
    <img src="../assets/search-normal.svg" alt="search">
  </button>
</div>
</template>
<script setup>
import { computed } from "@vue/reactivity";
import { onMounted,onUnmounted, ref } from 'vue';
import axiosClient from '../axiosClient';
const emits = defineEmits(['update:selectedId']);


let idProvFound = null;
const keyword = ref('');
const listaProveedores = ref([]);
const showResults = ref(false); // Estado para mostrar/ocultar el modal
const proveedorSeleccionado = ref(null); // Estado para almacenar el proveedor seleccionado
const computedProveedores = computed(() =>
  listaProveedores.value.filter((i) =>
    i.nombre.toLowerCase().includes(keyword.value.toLowerCase())
  )
);

onMounted(() => {
  axiosClient.get("/proveedor/Listado")
    .then(({ data }) => {
      listaProveedores.value = data;
    })
    .catch((error) => {
      console.error("Error al obtener proveedores:", error);
    });
   
  document.addEventListener('click', cerrarModalClickExterno);//llamo a la funcion para cerrar el modal si se clickeo fuera del modal
});

// Función para seleccionar un proveedor
const seleccionarProveedor = (proveedor) => {
  proveedorSeleccionado.value = proveedor;
  keyword.value = proveedor.nombre; // Cargar el nombre en el input
  showResults.value = false; // Ocultar el modal después de seleccionar
  emitSelectedProv();
};
// Función para cerrar el modal cuando se hace clic fuera de él
const cerrarModalClickExterno = (event) => {
  const buttonSearcher = document.getElementById('searcher');
  const modal = document.getElementById('toggleDropDown'); // Ajusta id del modaldropdown
  if (showResults.value && !modal.contains(event.target) && !buttonSearcher.contains(event.target)) {

    showResults.value = false;

  } 
};
const emitSelectedProv = () => {
  const selectedId = proveedorSeleccionado.value.idProv
  emits('update:selectedId', selectedId); // Emite el evento con los nuevos valores seleccionados

};

//si busca el nombre sin usar el drop down enterProvOn buscarIdProv
const enterProvOn = () => {
  idProvFound = listaProveedores.value.find(proveedor => proveedor.nombre.toLowerCase === keyword.value.toLowerCase)?.idProv ?? null; // Cargar el nombre en el input
  showResults.value = false; // Ocultar el modal después de seleccionar
  emits('update:selectedId', idProvFound);
}
const buscarIdProv = () => {
  
  console.log("el id prov seleccionado es el nº: "+idProvFound)
};
</script>