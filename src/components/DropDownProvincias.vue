<template>
  <Listbox as="div" v-model="selected">
    <div class="relative">
      <ListboxButton
        class="relative w-full cursor-pointer bg-white pl-2 pr-8 text-left text-gray-800 shadow-sm ring-1 ring-inset ring-purple-300 focus:outline-none focus:ring-2 sm:text-sm sm:leading-6 focus:ring-purple-500 py-2 border-2  focus hover:shadow-lg hover:shadow-purple-500/50 border-purple-300 rounded-3xl transition duration-150 ease-out md:ease-in">
        <span class="flex items-center">
          <span :class="[selected ? 'font-normal' : 'font-normal', 'ml-3 block truncate']">{{ selected && selected.tipo
            ? selected.tipo : 'Seleccione' }}</span>
        </span>
        <span class="pointer-events-none absolute inset-y-0 right-0 ml-3 flex items-center pr-2">
          <ChevronUpDownIcon class="h-5 w-5 text-purple-400" aria-hidden="true" />
        </span>
      </ListboxButton>

      <transition leave-active-class="transition ease-in duration-100" leave-from-class="opacity-100"
        leave-to-class="opacity-0">
        <ListboxOptions
          class="absolute z-10 mt-1 max-h-56 w-full overflow-auto rounded-3xl bg-white py-1 text-base shadow-lg shadow-purple-300/50 ring-1 ring-black ring-opacity-5 focus:outline-none sm:text-sm">
          <ListboxOption as="template" v-for="tipo in Provincias" :key="tipo.id" :value="tipo"
            v-slot="{ active, selected }">
            <li
              :class="[active ? 'bg-purple-300 text-white' : 'text-gray-900', 'relative cursor-default select-none py-2 pl-3 pr-9']">
              <div class="flex items-center">
                <span :class="[selected ? 'font-semibold' : 'font-normal', 'ml-3 block truncate']">{{ tipo.tipo }}</span>
              </div>

              <span v-if="selected"
                :class="[active ? 'text-white' : 'text-purple-600', 'absolute inset-y-0 right-0 flex items-center pr-4']">
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
import { onMounted, ref, watch } from 'vue';
import { Listbox, ListboxButton, ListboxLabel, ListboxOption, ListboxOptions } from '@headlessui/vue'
import { CheckIcon, ChevronUpDownIcon } from '@heroicons/vue/20/solid'
const props = defineProps({
  tipoProv: String,
});

const emits = defineEmits(['update:selectedId']);
const selected = ref(null)
const Provincias = ref([
  { id: 1, tipo: 'Buenos Aires' },
  { id: 2, tipo: 'CABA' },
  { id: 3, tipo: 'Catamarca' },
  { id: 4, tipo: 'Chaco' },
  { id: 5, tipo: 'Chubut' },
  { id: 6, tipo: 'Córdoba' },
  { id: 7, tipo: 'Corrientes' },
  { id: 8, tipo: 'Entre Ríos' },
  { id: 9, tipo: 'Formosa' },
  { id: 10, tipo: 'Jujuy' },
  { id: 11, tipo: 'La Pampa' },
  { id: 12, tipo: 'La Rioja' },
  { id: 13, tipo: 'Mendoza' },
  { id: 14, tipo: 'Misiones' },
  { id: 15, tipo: 'Neuquén' },
  { id: 16, tipo: 'Río Negro' },
  { id: 17, tipo: 'Salta' },
  { id: 18, tipo: 'San Juan' },
  { id: 19, tipo: 'San Luis' },
  { id: 20, tipo: 'Santa Cruz' },
  { id: 21, tipo: 'Santa Fe' },
  { id: 22, tipo: 'Sgo del Estero' },
  { id: 23, tipo: 'Tierra del Fuego' },
  { id: 24, tipo: 'Tucumán' }
]);
console.log(props.tipoProv)
watch(() => props.tipoProv, (tFact) => {
  selected.value = Provincias.value.find((id) => id.tipo === tFact);
  console.log("imprimo el tipoProv ", selected.value)
}
);



watch(() => selected.value, (newSelected) => {
  if (newSelected) {
    emits('update:selectedId', newSelected.tipo); // Emite el evento con el nuevo valor seleccionado
    console.log("Provincia:", newSelected.tipo)
  }
});

</script>
