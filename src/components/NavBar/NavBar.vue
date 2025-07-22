<template>
  <header class="principal-convination-color h-16 flex justify-between shadow-md rounded-bl-3xl rounded-br-3xl">
    <router-link :to="{ name: 'Home' }"
      class="lexend-exa text-xl font-bold hover:scale-110 ml-6 flex justify-center items-center h-full text-white transition duration-150 ease-out md:ease-in hover:text-gray-900">
      inRoom<img src="../../assets/pin.png" class="h-8 invert" alt="">
    </router-link>
    
    <!-- Glassmorphism Menu System (Alternative to avoid insertBefore errors) -->
    <GlassResponsiveMenu v-if="authStore.isAuthenticated" />

  </header>
</template>
<script setup>
import { useAuthStore } from "../../store/auth.js";
import GlassResponsiveMenu from "./GlassResponsiveMenu.vue";

const authStore = useAuthStore();
</script>
<style>
.bg-gradient-to-r {
  background-image: linear-gradient(to right, var(--tw-gradient-stops));
}
</style>hover:shadow-lg hover:shadow-purple-500/50./MenuEjemplo.vue