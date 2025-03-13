<template>
  <Teleport v-if="selectedRoom" to="body" class="overflow-hidden">
  <Transition name="modal-outer" appear>
    <div 
      class="fixed w-full h-full overflow-auto z-20 bg-black bg-opacity-40 backdrop-blur-lg top-0 left-0 flex justify-center px-8">
          <Transition name="modal-inner">
      <!-- Panel de la habitación seleccionada con Slider -->

      <div  class="flex flex-col justify-center items-center mb-24 p-4  rounded-lg w-10/12 h-auto text-center">
        
        <h3 class="text-3xl font-semibold text-white ">{{ selectedRoom.nombre }}</h3>
        <ul class="mt-2 list-disc pl-4 text-gray-300 text-left">
          <li v-for="(detalle, index) in selectedRoom.descripcion" :key="index">
            {{ detalle }}
          </li>
        </ul>

        <!-- Carrusel de imágenes -->
        <Swiper :modules="[Navigation, Pagination]" navigation pagination class="mt-4 w-full">
          <SwiperSlide v-for="(img, index) in selectedRoom.imagenes" :key="index">
            <img :src="img" class="w-full h-96 object-cover rounded-lg" />
          </SwiperSlide>
        </Swiper>
        <button type="button"
                class="btn-danger absolute bottom-8 right-6 md:right-16 text-md w-1/3 h-16 rounded-3xl transition-colors border-2 border-purple-200"
                @click="emits('close')">cancelar</button>
      </div>
    </Transition>
    </div>
  </Transition>
  </Teleport>
</template>

<script setup>
import { Swiper, SwiperSlide } from 'swiper/vue';
import { Navigation, Pagination } from 'swiper/modules';
import 'swiper/css';
import 'swiper/css/navigation';
import 'swiper/css/pagination';
const props = defineProps({
  selectedRoom: Object,
})
const emits = defineEmits(["close"]);
</script>