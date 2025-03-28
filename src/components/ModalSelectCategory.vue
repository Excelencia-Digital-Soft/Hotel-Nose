<template>
  <Teleport to="body" class="overflow-hidden">
  <Transition name="modal-outer" appear>
    <div 
      class="fixed w-full h-full overflow-auto z-20 bg-black bg-opacity-40 backdrop-blur-lg top-0 left-0 flex justify-center px-8">
          <Transition name="modal-inner">
      <!-- Panel de la habitación seleccionada con Slider -->

      <div  class="flex flex-col justify-center items-center  p-4  rounded-lg w-full h-auto text-center">
       
        <section class="bg-neutral-700 max-h-[1500px] overflow-y-auto w-11/12 h-auto rounded-3xl ">
          <h3 class="mt-6 text-lg text-white">Selecciona tu Habitación:</h3>
          <div class="flex flex-col justify-center items-center " v-for="habitacion in selectedCategory">
            <button 
            @click="seleccionarHabitacion(habitacion)"
            class=" relative h-36 w-11/12 rounded-2xl border-2 border-white mb-4 flex justify-center items-center">
                  <!-- Mostrar imagen o imagen por defecto -->
                  <img
                    :src="habitacion.imagenes.length > 0 ? habitacion.imagenes[0] : '../assets/image59.svg'"
                    alt="Imagen de la habitación"
                    class="w-full h-full object-cover rounded-2xl"
                  />
                  <span class="absolute w-48 font-semibold border-2 border-white p-4 text-white">{{ habitacion.nombreHabitacion }}</span>
                </button>          
              </div>
              <button type="button"
                class="btn-danger absolute top-4 right-4 md:right-16 text-md w-16 h-16 rounded-3xl transition-colors border-2 border-purple-200"
                @click="emits('close')">X</button>
        </section>
    
                <ModalSelectRoom v-if="modalSRoom" :selectedRoom="habSeleccionada" @close="toggleModalSelectRoom" />
      </div>
      
    </Transition>
    </div>
  </Transition>
  </Teleport>
</template>

<script setup>
import { ref, onMounted } from 'vue';
import ModalSelectRoom from '../components/ModalSelectRoom.vue';
const props = defineProps({
  selectedCategory: Object,
})
const emits = defineEmits(["close"]);
let modalSRoom = ref(false);
const habSeleccionada = ref(null);
const seleccionarHabitacion = (habitacion) => {
  console.log("SELECCIONADA:",habitacion)
  habSeleccionada.value = habitacion;
  modalSRoom.value = true
};
const toggleModalSelectRoom = () => {
  modalSRoom.value = !modalSRoom.value
}
</script>