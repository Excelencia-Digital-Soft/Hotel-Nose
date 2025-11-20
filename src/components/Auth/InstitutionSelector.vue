<template>
  <div class="absolute institucionModalSelector inset-0 bg-black bg-opacity-50 flex justify-center items-center z-50">
    <div class="bg-white p-6 rounded-lg max-w-md w-full mx-4">
      <div class="flex justify-between items-center mb-4">
        <h2 class="text-xl font-bold text-gray-800">Selecciona una institución</h2>
        <button 
          @click="$emit('close')"
          class="text-gray-500 hover:text-gray-700 text-xl font-semibold"
        >
          ×
        </button>
      </div>
      
      <div class="max-h-60 overflow-y-auto">
        <div 
          v-for="institution in institutions" 
          :key="institution.institucionId"
          class="mb-2"
        >
          <button 
            @click="selectInstitution(institution.institucionId)"
            class="w-full text-left p-3 hover:bg-gray-100 rounded-md transition-colors duration-200 border border-gray-200"
          >
            <div class="font-medium text-gray-800">{{ institution.nombre }}</div>
            <div v-if="institution.descripcion" class="text-sm text-gray-600 mt-1">
              {{ institution.descripcion }}
            </div>
          </button>
        </div>
      </div>
      
      <div class="mt-4 pt-4 border-t border-gray-200">
        <button 
          @click="$emit('close')"
          class="w-full px-4 py-2 text-gray-600 border border-gray-300 rounded-md hover:bg-gray-50 transition-colors duration-200"
        >
          Cancelar
        </button>
      </div>
    </div>
  </div>
</template>

<script setup>
defineProps({
  institutions: {
    type: Array,
    required: true,
    default: () => []
  }
})

const emit = defineEmits(['select', 'close'])

const selectInstitution = (institutionId) => {
  emit('select', institutionId)
}
</script>

<style scoped>
.institucionModalSelector {
  position: fixed;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  z-index: 1000;
}
</style>