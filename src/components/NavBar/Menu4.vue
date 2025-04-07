<template>
  <button class="menubtn-4 flex h-full relative w-40" @click="toggleResults4">
    <section
      id="header" 
      class="relative flex justify-center font-bold h-full w-full  items-center text-white hover:shadow-lg hover:shadow-neutral-900/50 rounded-full transition-colors py-2"
    >
      Caja
    </section>
    <ul v-show="showResults" id="optionsSelect" class="menu-column absolute w-40 z-20 text-sm right-2 mt-2">
      <li>
        <router-link v-if="authStore.auth && (authStore.auth.rol === 1 || authStore.auth.rol === 2 || authStore.auth.rol === 3 )" :to="{ name: 'CierresManager' }" class="w-full h-12 flex items-center justify-start py-2 px-1 pl-3 hover:bg-white hover:text-accent-400 shadow-md cursor-pointer rounded-lg">
          Cierres
        </router-link>
      </li>
      <li>
        <router-link v-if="authStore.auth && (authStore.auth.rol === 1 || authStore.auth.rol === 2)" :to="{ name: 'MediosPago' }" class="w-full h-12 flex items-center justify-start py-2 px-1 pl-3 hover:bg-white hover:text-accent-400 shadow-md cursor-pointer rounded-lg">
          Medios de Pago
        </router-link>
      </li>
      <li>
        <router-link v-if="authStore.auth && (authStore.auth.rol === 1 || authStore.auth.rol === 2)" :to="{ name: 'PromocionesManager' }" class="w-full h-12 flex items-center justify-start py-2 px-1 pl-3 hover:bg-white hover:text-accent-400 shadow-md cursor-pointer rounded-lg">
          Promociones
        </router-link>
      </li>
      <li>
        <router-link v-if="authStore.auth && (authStore.auth.rol === 1 || authStore.auth.rol === 2)" :to="{ name: 'Egresos' }" class="w-full h-12 flex items-center justify-start py-2 px-1 pl-3 hover:bg-white hover:text-accent-400 shadow-md cursor-pointer rounded-lg">
          Egresos
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
const toggleResults4 = () => {
  showResults.value = !showResults.value;
};

const handleClickOutside = (event) => {
  if (!event.target.closest('.menubtn-4')) {
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