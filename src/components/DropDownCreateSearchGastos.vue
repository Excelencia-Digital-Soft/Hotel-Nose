<template>
  <Listbox as="div" v-model="selected">
    <ListboxLabel class=" text-sm font-medium leading-6 text-white">Cuenta Gasto</ListboxLabel>
    <div class="relative">
      <ListboxButton class="relative w-full cursor-pointer bg-white pl-3 pr-10 text-left text-gray-900 shadow-sm ring-1 ring-inset ring-purple-300 focus:outline-none focus:ring-2 sm:text-sm sm:leading-6 focus:ring-purple-500 py-2 pb-3 border-2  focus hover:shadow-lg hover:shadow-purple-500/50 border-purple-300 rounded-3xl transition duration-150 ease-out md:ease-in">
        <span class="flex items-center" >
          <span :class="[selected ? 'font-semibold' : 'font-normal', 'ml-3 text-sm block truncate']">{{ selected && selected.nombre ? selected.nombre : 'Select'  }}</span>
        </span>
        <span class="pointer-events-none absolute inset-y-0 right-0 ml-3 flex items-center pr-2">
          <ChevronUpDownIcon class="h-5 w-5 text-purple-400" aria-hidden="true" />
        </span>
      </ListboxButton>

      <transition leave-active-class="transition ease-in duration-100" leave-from-class="opacity-100" leave-to-class="opacity-0">
        <ListboxOptions class="absolute z-10 top-0 max-h-56 w-full overflow-auto rounded-3xl border-2 border-purple-300 bg-white pb-1 text-base shadow-lg shadow-purple-300/50 ring-1 ring-black ring-opacity-5 focus:outline-none sm:text-sm">
          <input 
            v-model="keyword"
            class="w-full rounded-3xl h-8 border-2 py-4 pb-5  mb-4 border-purple-300 focus:ring-purple-300 focus:border-purple-400" 
            />
            <button @click.prevent="validarGasto" id="searcher" class="btn-primary p-1 top-0 right-0 absolute  flex justify-center items-center cursor-pointer border-2 bg-gradient-to-r from-indigo-300 via-purple-400 to-purple-300  text-white shadow-lg hover:shadow-purple-500/50 border-purple-200 rounded-3xl transition-colors">
              <img src="../assets/add-circle.svg" alt="search">
            </button>
            
          <ListboxOption as="template" v-for="tipo in computedListaGastos" :key="tipo.id" :value="tipo" v-slot="{ active, selected }" style="max-height: 50vh; overflow-y: auto;">
            <li :class="[active ? 'bg-purple-300 text-white' : 'text-gray-900', 'relative cursor-default select-none py-1 pl-3 pr-9']">
              <div class="flex items-center">
                <span :class="[selected ? 'font-semibold' : 'font-normal', 'ml-3 block truncate']">{{ tipo.nombre }}</span>
              </div>

              <span v-if="selected" :class="[active ? 'text-white' : 'text-purple-600', 'absolute inset-y-0 right-0 flex items-center pr-4']">
                <CheckIcon class="h-5 w-5" aria-hidden="true" />
              </span>
            </li>
          </ListboxOption>
        </ListboxOptions>
      </transition>
    </div>
  </Listbox>
<ModalConfirm v-if="modalConfirmar" :name="keyword" @confirmaAccion="confirmAndSend" @close="toggleModalConfirmar"/>
</template>

<script setup>
import {onMounted, ref, watch, computed } from 'vue';
import axiosClient from '../axiosClient';
import { Listbox, ListboxButton, ListboxLabel, ListboxOption, ListboxOptions } from '@headlessui/vue'
import { CheckIcon, ChevronUpDownIcon } from '@heroicons/vue/20/solid'
import ModalConfirm from '../components/ModalConfirm.vue'
const props = defineProps({
  idTipoGasto:0,
});
const computedListaGastos = computed(() =>
  tiposCuentaGastos.value.filter((i) =>
    i.nombre.toLowerCase().includes(keyword.value.toLowerCase())
  )
);
const keyword = ref('');
const tiposCuentaGastos = ref([]);
const selected = ref(props.idTipoGasto);
const emits = defineEmits(['update:selectedId']);
const modalConfirmar = ref(false)
const toggleModalConfirmar = () =>{ 
  modalConfirmar.value = !modalConfirmar.value
}
const confirmAndSend = () =>{
  const nuevoGasto = {
    idCGasto: 0,
    nombre: keyword.value,
    cod: 0,               // Por si alguna vez le quieren poner un codigo especial a cada gasto.. 
    anulado: false
  }
  console.log("this is the spend what we have to send "+nuevoGasto.value)
  
  axiosClient.post('/cgasto/NewCGasto', nuevoGasto) 
  .then(res => {
    console.log("nuevo Gasto creado idGASTO:"+res.data);
    let tid = parseInt(res.data);    // Convierte res.data a entero de 32 bits TODO! necesito que la API DEVUELVA
    nuevoGasto.idCGasto = tid;
    tiposCuentaGastos.value.push(nuevoGasto)  // el id asi lo mando.. al emits 
    selected.value = nuevoGasto  
  })
  .catch(error => {
    console.error(error);
  });
}
onMounted(() => {
  
  axiosClient.get("/parametro/Params")
        .then(({data})=>{
          tiposCuentaGastos.value = data.cuentaGasto;
          selected.value = tiposCuentaGastos.value.find(tipos => tipos.idCGasto === props.idTipoGasto);
        });


});
watch(() => selected.value, (newSelected) => {
  if (newSelected) {
    emits('update:selectedId', newSelected); // Emite el evento con el nuevo valor seleccionado
    console.log("Se seleccionó" + newSelected.nombre + "con id" + newSelected.idCGasto)
  }
});



const verificaNombreEnLista = computed(() =>
  tiposCuentaGastos.value.some((i) =>
    i.nombre.toLowerCase() === keyword.value.toLowerCase()
  )
);
const existeElNombre = () => { 
  if (verificaNombreEnLista.value){//comprobamos que no exista el nombre del nuevo gasto
  return true
  } 
  else {
  return false
  }
}
const validarGasto = () => {
  //VALIDACION
  if (existeElNombre())  {
  //No envíes el formulario si hay errores de validación
  console.log("Ya existe este nombre buscate otro")
  return;
  }
  toggleModalConfirmar()
}
</script>
<!-- {
  "idCGasto": 0,
  "nombre": "string",
  "cod": 0,
  "anulado": true
} -->