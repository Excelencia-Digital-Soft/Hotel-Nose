<template>
  <Listbox as="div">
    <div class="relative">
      <ListboxLabel class="text-sm font-semibold text-white leading-6">.</ListboxLabel>
      <ListboxButton
        class="relative w-full cursor-pointer ml-1 bg-lime-300 pl-3 pr-3 text-center text-gray-800 shadow-sm ring-1 ring-inset ring-lime-300 focus:outline-none focus:ring-2 sm:text-sm sm:leading-6 focus:ring-lime-500 py-2 border-2  focus hover:shadow-lg hover:shadow-lime-500/50 border-lime-300 rounded-l-lg rounded-3xl transition duration-150 ease-out md:ease-in"
      >
        <span class="flex items-center">
          <span class="block text-xl">
            +
          </span>
        </span>
      </ListboxButton>

      <transition
        leave-active-class="transition ease-in duration-200"
        leave-from-class="opacity-100"
        leave-to-class="opacity-0"
      >
        <ListboxOptions
          class="absolute z-10 mt-1 h-64 w-44 rounded-3xl border-2 border-purple-300 bg-white py-4 px-4 text-base shadow-lg shadow-purple-300/50 ring-1 ring-black ring-opacity-5 focus:outline-none sm:text-sm "
        > 
          <input 
          class="focus:ring-purple-500 border-2 mb-1 text-xs w-full focus hover:shadow-lg hover:shadow-purple-500/50 border-purple-200 rounded-3xl transition duration-150 ease-out md:ease-in" 
          :class="{ 'border-red-400': nombreError }"
          v-model="tag.nombre" type="text" placeholder="Nueva Categoria"
          @input="validarNombre">
          <label class="text-xs font-semibold mt-1 leading-6 text-gray-600"> selecciona un color</label>
          <div class="grid grid-cols-6 gap-1 items-center h-24 w-full mt-1 ">
            <section
              v-for="color in colors"
              :key="color.id"
              class="h-8 w-6 flex cursor-pointer justify-center items-center text-white font-semibold drop-shadow-lg hover:-translate-y-1 duration-300 ease-in-out rounded-b-xl rounded-t-sm"
              :class="{  'selected-color': selectedColorIndex === color.id }"
              :style="{ backgroundColor: color.code }"
              @click="updateTagColor(color.code, color.id)"
            >
            +
            </section>
            <button @click="crearNuevoTAG()" type="button"
                class="flex justify-center items-center mt-3 w-full col-span-6 text-xs shadow-lg h-8 border-2 bg-gradient-to-r from-indigo-300 via-purple-400 to-purple-300 text-white hover:shadow-lg hover:shadow-purple-500/50 transition duration-150 ease-out md:ease-in border-purple-200 rounded-2xl "
                >Agregar Categoria</button>
        </div>
        </ListboxOptions>
      </transition>
    </div>
  </Listbox>
</template>

<script setup>
import {  ref, watch } from 'vue';
import axiosClient from '../axiosClient';
import { Listbox, ListboxButton, ListboxLabel, ListboxOption, ListboxOptions } from '@headlessui/vue';
import { CheckIcon, ChevronUpDownIcon } from '@heroicons/vue/20/solid';
const props = defineProps({
    provId:0
})
const emits = defineEmits(['update:cheatIds']);
let tag = ref({
  idCategoria: 0,
  nombre: '',
  anulado: false,
  color: ''
})
const selectedColorIndex = ref(5);
let nombreError = ref('');
const validarNombre = () => { nombreError.value = !tag.value.nombre.trim() ? 'obligatorio' : '';};
const colors = [
  {
    id: 1,
    name: 'red',
    code:'#ef4444',
  },
  {
    id: 2,
    name: 'orange',
    code:'#f97316'
  },
  {
    id: 3,
    name: 'Amber',
    code:'#fbbf24'
  },
  {
    id: 4,
    name: 'yellow',
    code:'#fde047',},
  {
    id: 5,
    name: 'lime',
    code:'#a3e635',},
  {
    id: 6,
    name: 'green',
    code:'#22c55e',},
  {
    id: 7,
    name: 'Emerald',
    code:'#34d399',},
  {
    id: 8,
    name: 'turquoise',
    code:'#5eead4',},
  {
    id: 9,
    name: 'cyan',
    code:'#22d3ee',},
  {
    id: 10,
    name: 'frenchblue',
    code:'#0369a1'},
  {
    id: 11,
    name: 'blue',
    code:'#3b82f6',
  },
  {
    id: 12,
    name: 'indigo',
    code:'#818cf8'
  },
  {
    id: 13,
    name: 'purple',
    code:'#8b5cf6'
  },
  {
    id: 14,
    name: 'violet',
    code:'#a855f7',},
  {
    id: 15,
    name: 'fucsia',
    code:'#e879f9',},
  {
    id: 16,
    name: 'pink',
    code:'#f472b6',},
  {
    id: 17,
    name: 'rose',
    code:'#f43f5e',},
  {
    id: 18,
    name: 'black',
    code:'#3f3f46',},
]
const updateTagColor = (code,index) => {
  tag.value.color = code;
  selectedColorIndex.value = index;
  console.log(selectedColorIndex)
  console.log("Color actualizado:", tag.color);
  console.log("Nombre del Color:", tag.nombre);
}

//Crear Nuevo TAG
const crearNuevoTAG = () => {
  //VALIDACION
  validarNombre();
  if (nombreError.value )  {
  //No envíes el formulario si hay errores de validación
  console.log("faltan datos obligatorios perra")
  return;
  }
  axiosClient.post('/proveedor/NewTag', tag.value) 
  .then(res => {
    console.log("1nueva etiqueta id:"+res.data+"al proveedor:"+props.provId);
    let pid = parseInt(props.provId); // Convierte props.provId a entero de 32 bits
    let tid = parseInt(res.data);    // Convierte res.data a entero de 32 bits

    alert("Nuevo Tag creado correctamente");
    if (pid !== 0) {
      axiosClient.post(`/proveedor/Etiquetar?pid=${pid}&tid=${tid}`)
    .then(res => {
      console.log("2nueva etiqueta id:"+res.data+"al proveedor:"+props.provId); // Imprime los detalles del error del servidor, si están disponibles.
      emits('update:cheatIds', 0 )//ENVIAMOS EL OK PARA QUE SE ACTUALICEN LOS TAG QUE TIENE ESTE PROVEEDOR
    })
    .catch(error => {
    console.error(error);
    })     
    } 
    else{
      emits('update:cheatIds', tid)
    }
    
  })
  .catch(error => {
    console.error(error);
  });
}
</script>
<style scoped>
.selected-color {
  transform: translateY(-4px); /* Aplica la transformación cuando el elemento está seleccionado */
  border: 2px solid rgb(255, 255, 255)
}
</style>