<template>
  <div class="relative cursor-pointer no-select">
    <button class="menuMobile flex h-full relative mr-4" @click="toggleResults3">
      <section
        id="header"
        class="relative flex justify-center font-bold h-full w-full items-center text-white hover:shadow-lg hover:shadow-cyan-500/50 rounded-full transition-colors py-2"
      >
      <span class="material-symbols-outlined">
        {{ showResults ? 'menu_open' : 'menu' }}
      </span>
      </section>
    </button>
    <ul v-show="showResults" id="optionsSelect" class="menu-column absolute w-60 z-20 text-xl right-2 mt-2">
      <li
        v-for="(item, index) in menuItems"
        :key="index"
        class="p-4 hover:bg-secondary-800 rounded-lg"
        @click="toggleSubmenu(index)"
      >
        {{ item.label }}
        <ul v-show="openSubmenus[index]" class="ml-4">
          <li
            v-for="(subItem, subIndex) in item.submenu"
            :key="subIndex"
          >
            <!-- Cada submenú redirige a una ruta diferente -->
            <router-link
              :to="subItem.route"
              class="w-full m-1  bg-secondary-600 flex items-center justify-start py-2 px-1 pl-3 hover:bg-primary-500 hover:text-white shadow-md rounded-lg"
              @click.native="closeMenu"
              >
              {{ subItem.label }}
            </router-link>
          </li>

        </ul>
      </li>
      <li 
      @click.prevent="logOut"
      class="w-full p-4 flex items-center justify-start  hover:bg-primary-500 hover:text-white shadow-md rounded-lg" 
      >Cerrar Sesión </li>
    </ul>
  </div>
</template>

<script setup>
import { ref, onUnmounted } from 'vue';
import { useAuthStore } from '../../store/auth';
import { useRouter } from 'vue-router';

const router = useRouter();

const logOut = () => {
  const auth = useAuthStore();
  auth.auth = null;
  router.push('/guest');
};
// Estado para el menú principal y submenús
const showResults = ref(false);

// Arreglo para manejar la apertura/cierre de cada submenú
const openSubmenus = ref([]);

// Definir los ítems del menú, sus submenús y las rutas asociadas
const menuItems = ref([
  {
    label: 'Habitaciones',
    submenu: [
      { label: 'Ver Rooms', route: { name: 'Rooms' } },
      { label: 'Crear Room', route: { name: 'RoomCreate' } },
      { label: 'Crear Categoría', route: { name: 'CategoryCreate' } }
    ]
  },
  {
    label: 'Artículos',
    submenu: [
      { label: 'Agregar Artículo', route: { name: 'ArticleCreate' } }
    ]
  },
  {
    label: 'Pedidos',
    submenu: [
      { label: 'Enviar Pedido', route: { name: 'SubmitOrder' } }
    ]
  }
]);

// Alternar el menú principal
const toggleResults3 = () => {
  showResults.value = !showResults.value;
};

// Alternar submenú por índice
const toggleSubmenu = (index) => {
  openSubmenus.value = { ...openSubmenus.value, [index]: !openSubmenus.value[index] };
};

// Función para cerrar el menú completo y resetear submenús
const closeMenu = () => {
  showResults.value = false;
  openSubmenus.value = []; // Cierra todos los submenús
};
// Cerrar menú cuando haces clic fuera
const handleClickOutside = (event) => {
  if (!event.target.closest('.menuMobile') && !event.target.closest('#optionsSelect')) {
    showResults.value = false;
  }
};

// Escuchar el clic global para cerrar
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
.no-select {
    user-select: none; /* Deshabilita la selección de texto */
    -webkit-user-select: none; /* Safari */
    -moz-user-select: none; /* Firefox */
    -ms-user-select: none; /* IE/Edge */
  }

</style>