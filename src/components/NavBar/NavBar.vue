<template>
    <header class=" bg-gradient-to-r from-blue-800 via-blue-400 to-cyan-300 h-16 flex justify-between shadow-md rounded-bl-3xl rounded-br-3xl">
        <router-link :to="{ name: 'home' }" class="hover:scale-110 ml-6 flex justify-center items-center h-full text-white transition duration-150 ease-out md:ease-in hover:text-gray-900">
            <img src="../../assets/SGCLOGO2WEB.png" alt="UPCN" class="h-10 w-full">
        </router-link>
        <div class="flex h-full items-center">
       
          <Menu1/>
         
          <Menu2/>
         
          <Menu3/>
         
         
          <Logout/>
            
        </div>
    </header>
</template>
<script setup>
import Logout from './Logout.vue';
import Menu1 from './Menu1.vue';
import Menu2 from './Menu2.vue';
import Menu3 from './Menu3.vue';
</script>
<style>
  .bg-gradient-to-r {
    background-image: linear-gradient(to right, var(--tw-gradient-stops));
  }
</style>hover:shadow-lg hover:shadow-purple-500/50./MenuEjemplo.vue