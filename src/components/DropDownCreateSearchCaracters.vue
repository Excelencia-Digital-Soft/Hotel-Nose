<template>
  <Listbox as="div" v-model="selected">
    <ListboxLabel class="text-sm font-medium leading-6 text-white">Seleccionar:</ListboxLabel>
    <div class="relative">
      <ListboxButton class="relative w-full cursor-pointer bg-neutral-600 pl-3 pr-10 text-left text-white shadow-sm ring-1 ring-inset ring-purple-300 focus:outline-none focus:ring-2 sm:text-sm sm:leading-6 focus:ring-purple-500 py-2 pb-3 border-2 focus hover:shadow-lg hover:shadow-purple-500/50 border-purple-300 rounded-3xl transition duration-150 ease-out md:ease-in">
        <span class="flex items-center">
          <!-- Mostrar el icono si está disponible -->
          <img v-if="selected && selected.icono" :src="selected.icono" alt="Icono" class="w-6 h-6 mr-2" />
          <span :class="[selected ? 'font-semibold' : 'font-normal', 'ml-3 text-sm block truncate']">
            {{ selected && selected.nombre ? selected.nombre : 'Select' }}
          </span>
        </span>
        <span class="pointer-events-none absolute inset-y-0 right-0 ml-3 flex items-center pr-2">
          <ChevronUpDownIcon class="h-5 w-5 text-purple-400" aria-hidden="true" />
        </span>
      </ListboxButton>

      <transition leave-active-class="transition ease-in duration-100" leave-from-class="opacity-100" leave-to-class="opacity-0">
        <ListboxOptions class="absolute z-10 top-0 max-h-56 w-full overflow-auto rounded-3xl border-2 border-purple-300 bg-neutral-600 pb-1 text-base shadow-lg shadow-purple-300/50 ring-1 ring-black ring-opacity-5 focus:outline-none sm:text-sm">
          <input 
            v-model="keyword"
            class="w-full rounded-3xl h-8 border-2 py-4 pb-5 mb-4 border-purple-300 focus:ring-purple-300 focus:border-purple-400" 
          />

          <ListboxOption as="template" v-for="tipo in computedListaGastos" :key="tipo.caracteristicaId" :value="tipo" v-slot="{ active, selected }" style="max-height: 50vh; overflow-y: auto;">
            <li :class="[active ? 'bg-primary-300 text-white' : 'text-white', 'relative cursor-default select-none py-1 pl-3 pr-9']">
              <div class="flex items-center">
                <!-- Mostrar el icono si está disponible -->
                <img v-if="tipo.icono" :src="tipo.icono" alt="Icono" class="w-6 h-6 mr-2" />
                <span :class="[selected ? 'font-semibold' : 'font-normal', 'ml-3 block truncate']">{{ tipo.nombre }}</span>
              </div>

              <span v-if="selected" :class="[active ? 'text-white' : 'text-primary-400', 'absolute inset-y-0 right-0 flex items-center pr-4']">
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
const computedListaGastos = computed(() => {
  if (!keyword.value) return tiposCuentaGastos.value;
  return tiposCuentaGastos.value.filter(tipo =>
    tipo.nombre.toLowerCase().includes(keyword.value.toLowerCase())
  );
});
const keyword = ref('');
const tiposCuentaGastos = ref([]);
const selected = ref(props.idTipoGasto);
const emits = defineEmits(['addGasto']);
const modalConfirmar = ref(false)
const toggleModalConfirmar = () =>{ 
  modalConfirmar.value = !modalConfirmar.value
}
onMounted(async () => {
  try {
    // Obtener las características
    const { data } = await axiosClient.get("/api/Caracteristicas/GetCaracteristicas");

    // Convertir las rutas de los iconos a ObjectURL
    const caracteristicasConIconos = await Promise.all(
      data.data.map(async (caracteristica) => {
        if (caracteristica.icono) {
          try {
            // Obtener la imagen como Blob y convertirla a ObjectURL
            const response = await axiosClient.get(`/api/Caracteristicas/GetImage/${caracteristica.caracteristicaId}`, {
              responseType: 'blob',
            });
            caracteristica.icono = URL.createObjectURL(response.data);
          } catch (error) {
            console.error(`Error al cargar el icono de ${caracteristica.nombre}:`, error);
            caracteristica.icono = null; // Si hay un error, establecer el icono como null
          }
        }
        return caracteristica;
      })
    );

    // Asignar las características con los iconos convertidos
    tiposCuentaGastos.value = caracteristicasConIconos;

  
  } catch (error) {
    console.error("Error al obtener las características:", error);
  }
});
watch(() => selected.value, (newSelected) => {
  if (newSelected) {
    emits('addGasto', newSelected); // Emite el evento con el nuevo valor seleccionado
    console.log("Se seleccionó" + newSelected.nombre + "con id" + newSelected.caracteristicaId)
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