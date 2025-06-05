<template>
  <button class="menubtn-3 flex h-full relative w-40" @click="toggleResults5">
    <section
      id="header" 
      class="relative font-bold flex justify-center h-full w-full  items-center text-white hover:shadow-lg hover:shadow-neutral-900/50 rounded-full transition-colors py-2"
    >
      Habitaciones
    </section>
    <ul v-show="showResults" id="optionsSelect" class="menu-column absolute w-40 z-20 text-sm right-2 mt-2 ">
      <li>
        <router-link :to="{ name: 'Rooms' }" class="w-full h-12 flex items-center justify-start py-2 px-1 pl-3 hover:bg-white hover:text-accent-400 shadow-md cursor-pointer rounded-lg">
          Habitaciones
        </router-link>
        <router-link v-if="authStore.auth && (authStore.auth.rol === 1 || authStore.auth.rol === 2)" :to="{ name: 'RoomCreate' }" class="w-full h-12 flex items-center justify-start py-2 px-1 pl-3 hover:bg-white hover:text-accent-400 shadow-md cursor-pointer rounded-lg">
          Crear Habitaciones
        </router-link>
        <router-link v-if="authStore.auth && (authStore.auth.rol === 1 || authStore.auth.rol === 2)" :to="{ name: 'CaracteristicasManager' }" class="w-full h-12 flex items-center justify-start py-2 px-1 pl-3 hover:bg-white hover:text-accent-400 shadow-md cursor-pointer rounded-lg">
          Caracteristicas
        </router-link>
        <router-link v-if="authStore.auth && (authStore.auth.rol === 1 || authStore.auth.rol === 2)" :to="{ name: 'CategoryCreate' }" class="w-full h-12 flex items-center justify-start py-2 px-1 pl-3 hover:bg-white hover:text-accent-400 shadow-md cursor-pointer rounded-lg">
          Categorías
        </router-link>
        <router-link :to="{ name: 'EmpenosManager' }" class="w-full h-12 flex items-center justify-start py-2 px-1 pl-3 hover:bg-white hover:text-accent-400 shadow-md cursor-pointer rounded-lg">
          Empeños
        </router-link>
      </li>
    </ul>
  </button>

</template>

<script setup>
import { ref, onUnmounted } from 'vue';
import { UserCircleIcon } from '@heroicons/vue/20/solid';
import { useAuthStore } from "../../store/auth.js";

const authStore = useAuthStore();
const showResults = ref(false);
const toggleResults5 = () => {
  showResults.value = !showResults.value;
};

const handleClickOutside = (event) => {
  if (!event.target.closest('.menubtn-3')) {
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