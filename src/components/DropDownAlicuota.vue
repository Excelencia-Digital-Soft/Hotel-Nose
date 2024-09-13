<template>
  <Listbox as="div" v-model="selected">
    <div class="relative">
      <ListboxButton class="relative w-full cursor-pointer">
        <span class="flex items-center" >
          <span :class="[selected && selected.valorAlicuota ? 'font-semibold' : 'font-normal', 'ml-3 text-sm block truncate']">{{ selected && selected.valorAlicuota ? selected.valorAlicuota : 'Seleccione' }}</span>
        </span>
        <span class="pointer-events-none absolute inset-y-0 right-0 ml-3 flex items-center pr-2">
          <ChevronUpDownIcon class="h-5 w-5 text-purple-400" aria-hidden="true" />
        </span>
      </ListboxButton>

      <transition leave-active-class="transition ease-in duration-100" leave-from-class="opacity-100" leave-to-class="opacity-0">
        <ListboxOptions class="absolute z-10 mt-1 w-full rounded-3xl bg-white py-1 text-base shadow-lg shadow-purple-300/50 ring-1 ring-black ring-opacity-5 focus:outline-none sm:text-sm">
          <ListboxOption as="template" v-for="tipo in tiposAlicuota" :key="tipo.id" :value="tipo" v-slot="{ active, selected }">
            <li :class="[active ? 'bg-purple-300 text-white' : 'text-gray-900', 'relative cursor-default select-none py-1 pl-3 pr-9']">
              <div class="flex items-center">
                <span :class="[selected ? 'font-semibold' : 'font-normal', 'ml-3 block truncate']">{{ tipo.valorAlicuota }}</span>
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
  tipoAlicuota:Number,
});
const tiposAlicuota = ref([]);
const selected = ref(props.tipoAlicuota);
const emits = defineEmits(['update:selectedId']);
onMounted(() => {
  
  axiosClient.get("/parametro/Params")
        .then(({data})=>{
          tiposAlicuota.value = data.alicuotum;
          selected.value = tiposAlicuota.value.find(tipos => tipos.idAlicuota === props.tipoAlicuota);
        });
});
watch(() => selected.value, (newSelected) => {
  if (newSelected) {
    emits('update:selectedId', newSelected); // Emite el evento con el nuevo valor seleccionado
    console.log(newSelected.valorAlicuota)
  }
});

</script>
