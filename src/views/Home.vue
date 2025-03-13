<template>
  <div class=" flex flex-col w-full h-screen justify-center items-center  p-8">

        <Transition>
          <div v-if="show" alt="MOTEL X" class="h-1/3 text-[40px] flex bg-gradient-to-l from-accent-200 via-secondary-500 to-primary-300 bg bg-clip-text text-transparent lexend-exa md:text-[70px] font-bold"
          >inRoom<img src="../assets/pin.png" class="h-16 md:h-24 invert" alt="">
        </div>

        </Transition>
        <router-link :to="{ name: 'SelectRoom' }" class="w-32 h-12 flex items-center justify-start py-2 px-1 pl-3 hover:bg-white hover:text-accent-400 shadow-md cursor-pointer rounded-lg">
          ir a externo
        </router-link>
  </div>
</template>
<script setup>
import { ref, onMounted } from 'vue';
import { useAuthStore } from "../store/auth.js";
import SelectRoom from '../views/SelectRoom.vue';
const show = ref(false)
onMounted(() => {
show.value = !show.value
})


const authStore = useAuthStore();
console.log("Del Home",authStore.auth);
</script>
<style scoped>
.backg{

background-color: #363636;
background-size: cover;
height:91vh;

}

.v-enter-active,
.v-leave-active {
transition: opacity 1.5s ease;
}

.v-enter-from,
.v-leave-to {
opacity: 0;
}
</style>