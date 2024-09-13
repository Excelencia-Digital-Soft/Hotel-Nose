<template>
  <Listbox as="div" v-model="selected">
    <ListboxLabel class=" text-sm font-medium leading-6 text-white">Condicion IVA</ListboxLabel>
    <div class="relative">
      <ListboxButton class="relative w-full cursor-pointer bg-white pl-3 pr-10 text-left text-gray-900 shadow-sm ring-1 ring-inset ring-purple-300 focus:outline-none focus:ring-2 sm:text-sm sm:leading-6 focus:ring-purple-500 py-2 pb-3 border-2  focus hover:shadow-lg hover:shadow-purple-500/50 border-purple-300 rounded-3xl transition duration-150 ease-out md:ease-in">
        <span class="flex items-center" >
          <span :class="[selected ? 'font-semibold' : 'font-normal', 'ml-3 block truncate']">{{ selected && selected.condicion ? selected.condicion : 'Seleccionar Opción'  }}</span>
        </span>
        <span class="pointer-events-none absolute inset-y-0 right-0 ml-3 flex items-center pr-2">
          <ChevronUpDownIcon class="h-5 w-5 text-purple-400" aria-hidden="true" />
        </span>
      </ListboxButton>

      <transition leave-active-class="transition ease-in duration-100" leave-from-class="opacity-100" leave-to-class="opacity-0">
        <ListboxOptions class="absolute z-10 mt-1 max-h-56 w-full overflow-auto rounded-3xl bg-white py-1 text-base shadow-lg shadow-purple-300/50 ring-1 ring-black ring-opacity-5 focus:outline-none sm:text-sm">
          <ListboxOption as="template" v-for="cond in condicionesIva" :key="cond.idCondicion" :value="cond" v-slot="{ active, selected }">
            <li :class="[active ? 'bg-purple-300 text-white' : 'text-gray-900', 'relative cursor-default select-none py-2 pl-3 pr-9']">
              <div class="flex items-center">
                <span :class="[selected ? 'font-semibold' : 'font-normal', 'ml-3 block truncate']">{{ cond.condicion }}</span>
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
</template>

<script setup>
import {onMounted, ref, watch } from 'vue';
import axiosClient from '../axiosClient';
import { Listbox, ListboxButton, ListboxLabel, ListboxOption, ListboxOptions } from '@headlessui/vue'
import { CheckIcon, ChevronUpDownIcon } from '@heroicons/vue/20/solid'
const props = defineProps({
  idTipoIva:0,
});
const condicionesIva = ref([]);
const selected = ref(props.idTipoIva);
const emits = defineEmits(['update:selectedId']);
onMounted(() => {
    axiosClient.get("/parametro/Params")
        .then(({data})=>{
            condicionesIva.value = data.condicionIva;
            selected.value = condicionesIva.value.find(cond => cond.idCondicion === props.idTipoIva);
        });
});
watch(() => selected.value, (newSelected) => {
  if (newSelected) {
    emits('update:selectedId', newSelected); // Emite el evento con el nuevo valor seleccionado
    console.log(newSelected.idCondicion)
  }
});

</script>

