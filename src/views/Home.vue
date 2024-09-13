<template>
  <div class="backg flex flex-col w-full h-screen justify-center items-center  p-8">

        <Transition>
          <img v-if="show" src="../assets/SGC LOGO CENTRADO.png" alt="SGC" class="h-1/2">
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