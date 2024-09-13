<template>
  <button class="menubtn-1 flex h-full relative w-40" @click="toggleResults3">
    <section
      id="header" 
      class="relative flex justify-center font-bold h-full w-full  items-center text-white hover:shadow-lg hover:shadow-cyan-500/50 rounded-full transition-colors py-2"
    >
      Cargas
    </section>
    <ul v-show="showResults" id="optionsSelect" class="absolute w-40 z-20 text-sm right-2 mt-2 shadow-xl text-white border-4 border-cyan-200 bg-cyan-300 rounded-xl transition duration-200 ease-out md:ease-in">
      <li>
      </li>
    </ul>
  </button>

</template>

<script setup>
import { ref, onUnmounted } from 'vue';
import { UserCircleIcon } from '@heroicons/vue/20/solid';

const showResults = ref(false);
const toggleResults3 = () => {
  showResults.value = !showResults.value;
};

const handleClickOutside = (event) => {
  if (!event.target.closest('.menubtn-1')) {
    showResults.value = false;
  }
};

// Manejar el clic fuera del componente
window.addEventListener('click', handleClickOutside);

// Limpiar el evento al desmontar el componente
onUnmounted(() => {
  window.removeEventListener('click', handleClickOutside);
});
</script>

<style scoped>
#optionsSelect{
  top:96%
}
</style>