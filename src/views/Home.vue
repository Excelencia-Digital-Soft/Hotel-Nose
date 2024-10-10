<template>
  <div class=" flex flex-col w-full h-screen justify-center items-center  p-8">

        <Transition>
          <div v-if="show" alt="MOTEL X" class="h-1/3 bg-gradient-to-l from-accent-200 via-secondary-500 to-primary-300 bg bg-clip-text text-transparent lexend-exa text-[70px] font-bold">NOSE</div>
        </Transition>

  </div>
</template>
<script setup>
import { ref, onMounted } from 'vue';
const show = ref(false)
onMounted(() => {
show.value = !show.value
})



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