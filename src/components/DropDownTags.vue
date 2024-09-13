<template>
  <Listbox as="div">
    <ListboxLabel class="text-sm font-semibold leading-6 text-gray-800">Categorías</ListboxLabel>
    <div class="relative">
      <ListboxButton
        class="relative w-full  cursor-pointer bg-white pl-3 pr-16 text-left text-gray-900 shadow-sm ring-1 ring-inset ring-lime-300 focus:outline-none focus:ring-2 sm:text-sm sm:leading-6 focus:ring-lime-500 py-2 border-2  focus hover:shadow-lg hover:shadow-lime-500/50 border-lime-300 rounded-r-lg rounded-3xl transition duration-150 ease-out md:ease-in"
      >
        <span class="flex items-center">
          <span class="ml-3 block truncate">
            Ver Lista de Tags
          </span>
        </span>
        <span class="pointer-events-none absolute inset-y-0 right-0 ml-3 flex items-center pr-2">
          <ChevronUpDownIcon class="h-5 w-5 text-purple-400" aria-hidden="true" />
        </span>
      </ListboxButton>

      <transition
        leave-active-class="transition ease-in duration-200"
        leave-from-class="opacity-100"
        leave-to-class="opacity-0"
      >
        <ListboxOptions
          class="absolute z-10 mt-1 max-h-56 w-full overflow-auto rounded-3xl bg-white py-1 text-base shadow-lg shadow-purple-300/50 ring-1 ring-black ring-opacity-5 focus:outline-none sm:text-sm "
        >
          <ListboxOption
            as="template"
            v-for="tag in listaTags"
            :key="tag.idTag"
            :value="tag"
            v-slot="{ active }"
          >
            <li
              @click="toggleTag(tag)"
              :class="[active ? 'bg-purple-400 text-white' : 'text-gray-900', 'relative cursor-pointer select-none py-2 pl-3 pr-9 transition duration-100 ease-out md:ease-in']"
            >
              <div class="flex items-center">
                <span :class="[selectedTags.includes(tag) ? 'font-semibold ' : 'font-normal','ml-3 block truncate']">{{ tag.nombre }}</span>
              </div>

              <span v-if="selectedTags.includes(tag)" :class="[active ? 'text-white' : 'text-purple-600', 'absolute inset-y-0 right-0 flex items-center pr-4']">
                <CheckIcon class="h-5 w-5 text-lime-400 font-bold" aria-hidden="true" />
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
import axiosClient from '../axiosClient';
import { Listbox, ListboxButton, ListboxLabel, ListboxOption, ListboxOptions } from '@headlessui/vue';
import { CheckIcon, ChevronUpDownIcon } from '@heroicons/vue/20/solid';

const props = defineProps({
  provId:0,
  refresh:false,
  idNewTag:0
});

console.log("el proveedor es " +props.provId)
const listaTags = ref([]);
const selectedTags = ref([]);
let provTags = []
const emits = defineEmits(['update:selectedIds']);

onMounted(() => {
  axiosClient.get("/proveedor/Tags")
        .then(({ data }) => {
          listaTags.value = data;

      });
})

// Escucha el evento 'refresh' y llama a fetchData si es verdadero
watch(() => props.refresh, (newRefresh) => {
  if (newRefresh) {
    console.log("llego al WATCH: ", props.idNewTag)
    fetchData();
  }
});

const fetchData = () => {
  let params = { pid: props.provId };
  if (props.provId == 0 ) {
    axiosClient.get("/proveedor/Tags")
        .then(({ data }) => {
          let listAllTags = data;
          let tagToAdd= listAllTags.filter(tag => props.idNewTag==tag.idTag);
          tagToAdd = tagToAdd[0]
          console.log("el tag a agregar es:", tagToAdd)
          toggleTag(tagToAdd)
        });
  }
  else if (props.provId !== 0) {
    axiosClient.get("/proveedor/Tagged", { params })
      .then(({ data }) => {
        console.log("SECAMBIOOLO" + props.provId)
        provTags = data.map(item => item.idCategoria);
        axiosClient.get("/proveedor/Tags")
          .then(({ data }) => {
            listaTags.value = data;
            selectedTags.value = listaTags.value.filter(tag => provTags.includes(tag.idTag));
            console.log("seleetaged:", selectedTags);
            const selectedIdsString = JSON.stringify(selectedTags.value.map(tag => tag), null, 2);
            emits('update:selectedIds', selectedIdsString)
          });
      });
  }
};

watch(() => props.provId, (newProvId) => {
  let params = { pid: props.provId }; 
  if (newProvId == 0 ) {
    axiosClient.get("/proveedor/Tags")
        .then(({ data }) => {
          console.log(res.data);
          listaTags.value = data;
      });
  }
  else if(props.provId!==0){
//Peticion Para saber las Tag relacionadas al proveedor esto en caso de que editemos porq en caso de Crear nuevo sera 0
    axiosClient.get("/proveedor/Tagged",{params})
    .then(({ data }) => {
      console.log("proveedor nº"+props.provId)
      provTags = data.map(item => item.idCategoria);       
      //Peticion que trae todas las tags
      axiosClient.get("/proveedor/Tags")
        .then(({ data }) => {
          listaTags.value = data;
          selectedTags.value = listaTags.value.filter(tag => provTags.includes(tag.idTag));
          console.log("seleetaged:", selectedTags);
          const selectedIdsString = JSON.stringify(selectedTags.value.map(tag => tag), null, 2);
          emits('update:selectedIds', selectedIdsString)
        });  
    });
  } 
});
const toggleTag = (tag) => {
  const index = selectedTags.value.findIndex(t => t.idTag === tag.idTag);
  if (index === -1) {
    // Si no está en la lista, lo agregamos
    selectedTags.value.push(tag);
    emitSelectedTags();
    console.log("agrego:", selectedTags.value);
  } else {
    // Si está en la lista, lo eliminamos
    selectedTags.value.splice(index, 1);
    emitSelectedTags();
    console.log("quito:", selectedTags.value);
  }
};

const emitSelectedTags = () => {
  const selectedIdsString = JSON.stringify(selectedTags.value.map(tag => tag), null, 2);
  // const selectedIdsString = selectedTags.value.map(tag => tag);
  emits('update:selectedIds', selectedIdsString); // Emite el evento con los nuevos valores seleccionados
  console.log("mando:", selectedIdsString);
};

</script>