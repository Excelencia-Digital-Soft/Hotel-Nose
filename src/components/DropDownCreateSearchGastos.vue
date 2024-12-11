<!-- DropDownCreateSearchGastos.vue -->
<template>
  <Listbox as="div" v-model="selected">
    <ListboxLabel class="text-sm font-medium leading-6 text-white">Cuenta Gasto</ListboxLabel>
    <div class="relative">
      <ListboxButton class="relative w-full cursor-pointer bg-white pl-3 pr-10 text-left text-gray-900 shadow-sm ring-1 ring-inset ring-purple-300 focus:outline-none focus:ring-2 sm:text-sm sm:leading-6 focus:ring-purple-500 py-2 pb-3 border-2 hover:shadow-lg hover:shadow-purple-500/50 border-purple-300 rounded-3xl transition duration-150 ease-out md:ease-in">
        <span class="flex items-center">
          <span :class="[selected ? 'font-semibold' : 'font-normal', 'ml-3 text-sm block truncate']">
            {{ selected && selected.nombre ? selected.nombre : 'Select' }}
          </span>
        </span>
        <span class="pointer-events-none absolute inset-y-0 right-0 ml-3 flex items-center pr-2">
          <ChevronUpDownIcon class="h-5 w-5 text-purple-400" aria-hidden="true" />
        </span>
      </ListboxButton>

      <transition leave-active-class="transition ease-in duration-100" leave-from-class="opacity-100" leave-to-class="opacity-0">
        <ListboxOptions class="absolute z-10 top-0 max-h-56 w-full overflow-auto rounded-3xl border-2 border-purple-300 bg-white pb-1 text-base shadow-lg shadow-purple-300/50 ring-1 ring-black ring-opacity-5 focus:outline-none sm:text-sm">
          <input 
            v-model="keyword"
            class="w-full rounded-3xl h-8 border-2 py-4 pb-5 mb-4 border-purple-300 focus:ring-purple-300 focus:border-purple-400" 
            placeholder="Buscar..."
          />
          <button @click.prevent="validarGasto" id="searcher" class="btn-primary p-1 top-0 right-0 absolute flex justify-center items-center cursor-pointer border-2 bg-gradient-to-r from-indigo-300 via-purple-400 to-purple-300 text-white shadow-lg hover:shadow-purple-500/50 border-purple-200 rounded-3xl transition-colors">
            <img src="../assets/add-circle.svg" alt="search">
          </button>

          <ListboxOption
            as="template"
            v-for="tipo in computedListaGastos"
            :key="tipo.tipoEgresoId"
            :value="tipo"
            v-slot="{ active, selected }"
          >
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

  <ModalAcept v-if="ModalAceptar" :name="keyword" @confirmaAccion="confirmAndSend" @close="toggleModalAceptar"/>
</template>

<script setup>
import { ref, computed, onMounted, watch } from 'vue';
import axiosClient from '../axiosClient';
import { Listbox, ListboxButton, ListboxLabel, ListboxOption, ListboxOptions } from '@headlessui/vue';
import { CheckIcon, ChevronUpDownIcon } from '@heroicons/vue/20/solid';
import ModalAcept from '../components/ModalAcept.vue';

const keyword = ref('');
const tiposCuentaGastos = ref([]);
const selected = ref(null);
const ModalAceptar = ref(false);
const emits = defineEmits(['addGasto']);

// Fetch the list of TipoEgresos from the API
const fetchTipoEgresos = async () => {
  try {
    const response = await axiosClient.get('/GetTipoEgresos');
    if (response.data.ok) {
      tiposCuentaGastos.value = response.data.data;
    } else {
      console.error('Error fetching TipoEgresos:', response.data.message);
    }
  } catch (error) {
    console.error('Error fetching TipoEgresos:', error);
  }
};

onMounted(fetchTipoEgresos);

watch(() => selected.value, (newValue) => {
  console.log(newValue);
  const nuevoGasto = {
    TipoId: newValue.tipoEgresoId, // Generate a temporary unique ID
    nombre: newValue.nombre,
  };
  emits('addGasto', nuevoGasto); // Emit the new gasto to the parent
}, { deep: true });

const computedListaGastos = computed(() =>
  tiposCuentaGastos.value.filter((i) =>
    i.nombre.toLowerCase().includes(keyword.value.toLowerCase())
  )
);

const toggleModalAceptar = () => {
  ModalAceptar.value = !ModalAceptar.value;
};

const confirmAndSend = () => {
  fetchTipoEgresos();
  const nuevoGasto = {
    TipoId: Date.now(), // Generate a temporary unique ID
    nombre: keyword.value,
  };

  emits('addGasto', nuevoGasto); // Emit the new gasto to the parent
  toggleModalAceptar(); // Close the modal
  keyword.value = ''; // Clear the input field
};

const validarGasto = () => {
  const existe = tiposCuentaGastos.value.some((gasto) =>
    gasto.nombre.toLowerCase() === keyword.value.toLowerCase()
  );

  if (existe) {
    alert('Este gasto ya existe.');
    return;
  }

  toggleModalAceptar();

  
};
</script>
