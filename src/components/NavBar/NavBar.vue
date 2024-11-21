<template>
    <header class=" principal-convination-color h-16 flex justify-between shadow-md rounded-bl-3xl rounded-br-3xl">
        <router-link :to="{ name: 'home' }" class=" lexend-exa text-xl font-bold hover:scale-110 ml-6 flex justify-center items-center h-full text-white transition duration-150 ease-out md:ease-in hover:text-gray-900">
          inRoom<img src="../../assets/pin.png" class="h-8 invert" alt="">
        </router-link>
        <div class="hidden md:flex h-full items-center">
          
          <Menu1 v-if="authStore.auth.rol==3 || authStore.auth.rol==4"/>
         
          <Menu2 v-if="authStore.auth.rol==1 || authStore.auth.rol==2"/>
         
          <Menu3 v-if="authStore.auth.rol==1 || authStore.auth.rol==2"/>
         
          <Logout/> 
          
          
        </div>
        <MenuMobile class="flex md:hidden h-full items-center"/>
        
    </header>
</template>
<script setup>
import Logout from './Logout.vue';
import Menu1 from './Menu1.vue';
import Menu2 from './Menu2.vue';
import Menu3 from './Menu3.vue';
import MenuMobile from './MenuMobile.vue';
import { ref } from "vue";
import { useAuthStore } from "../../store/auth.js";

const authStore = useAuthStore();


</script>
<style>
  .bg-gradient-to-r {
    background-image: linear-gradient(to right, var(--tw-gradient-stops));
  }
</style>hover:shadow-lg hover:shadow-purple-500/50./MenuEjemplo.vue