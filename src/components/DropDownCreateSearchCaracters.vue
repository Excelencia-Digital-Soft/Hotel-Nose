<template>
  <div class="relative">
    <Listbox as="div" v-model="selected">
      <ListboxLabel class="text-sm font-medium text-white/90 mb-2">
        Seleccionar Característica:
      </ListboxLabel>
      
      <div class="relative">
        <!-- Main button with glassmorphism -->
        <ListboxButton class="glass-input w-full flex items-center justify-between px-4 py-3 
                              hover:bg-white/15 transition-all duration-200 cursor-pointer group">
          <span class="flex items-center gap-3">
            <!-- Icon display -->
            <div class="relative">
              <div v-if="selected?.icono" 
                   class="absolute inset-0 bg-gradient-to-r from-primary-400 to-secondary-400 
                          rounded-lg blur-md opacity-30 group-hover:opacity-50 transition-opacity"></div>
              <img v-if="selected?.icono" 
                   :src="selected.icono" 
                   :alt="selected.nombre"
                   class="relative w-8 h-8 object-cover rounded-lg bg-white/10 p-1" />
              <div v-else 
                   class="relative w-8 h-8 rounded-lg bg-gradient-to-br from-primary-400/20 to-secondary-400/20 
                          flex items-center justify-center">
                <svg class="w-5 h-5 text-white/60" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" 
                        d="M12 6v6m0 0v6m0-6h6m-6 0H6" />
                </svg>
              </div>
            </div>
            
            <!-- Text -->
            <span :class="[selected ? 'text-white font-medium' : 'text-white/60', 'block truncate']">
              {{ selected?.nombre || 'Selecciona una característica' }}
            </span>
          </span>
          
          <!-- Chevron icon -->
          <ChevronUpDownIcon class="w-5 h-5 text-primary-400 group-hover:text-primary-300 transition-colors" />
        </ListboxButton>

        <!-- Dropdown options -->
        <Transition 
          enter-active-class="transition duration-200 ease-out"
          enter-from-class="transform scale-95 opacity-0"
          enter-to-class="transform scale-100 opacity-100"
          leave-active-class="transition duration-150 ease-in"
          leave-from-class="transform scale-100 opacity-100"
          leave-to-class="transform scale-95 opacity-0">
          
          <ListboxOptions class="absolute z-50 mt-2 w-full glass-dropdown-menu">
            <!-- Search input -->
            <div class="p-3 border-b border-white/10">
              <div class="relative">
                <input 
                  v-model="searchKeyword"
                  type="text"
                  placeholder="Buscar característica..."
                  class="glass-input w-full pl-10 pr-4 py-2 text-sm"
                  @keydown.enter.prevent="handleEnterKey"
                />
                <svg class="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-white/40" 
                     fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" 
                        d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z" />
                </svg>
              </div>
              
              <!-- Create new button if not exists -->
              <button v-if="searchKeyword && !existeElNombre" 
                      @click="mostrarModalCrear"
                      class="mt-2 w-full glass-button-gradient py-2 text-sm font-medium">
                <span class="flex items-center justify-center gap-2">
                  <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" 
                          d="M12 4v16m8-8H4" />
                  </svg>
                  Crear "{{ searchKeyword }}"
                </span>
              </button>
            </div>

            <!-- Options list -->
            <div class="max-h-60 overflow-y-auto scrollbar-thin scrollbar-thumb-white/20 scrollbar-track-transparent">
              <ListboxOption
                v-for="caracteristica in caracteristicasFiltradas"
                :key="caracteristica.caracteristicaId"
                :value="caracteristica"
                v-slot="{ active, selected }"
                class="relative">
                
                <li :class="[
                  active ? 'bg-gradient-to-r from-primary-400/20 to-secondary-400/20' : '',
                  'cursor-pointer select-none px-3 py-2 hover:bg-white/5 transition-colors'
                ]">
                  <div class="flex items-center justify-between">
                    <div class="flex items-center gap-3">
                      <!-- Icon -->
                      <div class="relative">
                        <div v-if="caracteristica.icono && active" 
                             class="absolute inset-0 bg-gradient-to-r from-primary-400 to-secondary-400 
                                    rounded blur-md opacity-30"></div>
                        <img v-if="caracteristica.icono" 
                             :src="caracteristica.icono" 
                             :alt="caracteristica.nombre"
                             class="relative w-7 h-7 object-cover rounded bg-white/10 p-0.5" />
                        <div v-else 
                             class="relative w-7 h-7 rounded bg-white/5 flex items-center justify-center">
                          <svg class="w-4 h-4 text-white/40" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" 
                                  d="M9.663 17h4.673M12 3v1m6.364 1.636l-.707.707M21 12h-1M4 12H3m3.343-5.657l-.707-.707m2.828 9.9a5 5 0 117.072 0l-.548.547A3.374 3.374 0 0014 18.469V19a2 2 0 11-4 0v-.531c0-.895-.356-1.754-.988-2.386l-.548-.547z" />
                          </svg>
                        </div>
                      </div>
                      
                      <!-- Name and description -->
                      <div>
                        <span :class="[selected ? 'font-semibold' : 'font-normal', 'text-white block']">
                          {{ caracteristica.nombre }}
                        </span>
                        <span v-if="caracteristica.descripcion" class="text-white/50 text-xs">
                          {{ caracteristica.descripcion }}
                        </span>
                      </div>
                    </div>
                    
                    <!-- Check icon if selected -->
                    <CheckIcon v-if="selected" 
                               :class="[active ? 'text-primary-300' : 'text-primary-400', 'w-5 h-5']" />
                  </div>
                </li>
              </ListboxOption>
              
              <!-- Empty state -->
              <div v-if="caracteristicasFiltradas.length === 0" class="p-8 text-center">
                <svg class="w-12 h-12 mx-auto mb-3 text-white/20" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" 
                        d="M9.172 16.172a4 4 0 015.656 0M9 10h.01M15 10h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
                </svg>
                <p class="text-white/60 text-sm">No se encontraron características</p>
                <p v-if="searchKeyword" class="text-white/40 text-xs mt-1">
                  Presiona Enter para crear "{{ searchKeyword }}"
                </p>
              </div>
            </div>
          </ListboxOptions>
        </Transition>
      </div>
    </Listbox>

    <!-- Create modal -->
    <ModalCreateCaracteristica 
      v-if="modalCrear" 
      :nombre="searchKeyword"
      @caracteristicaCreada="handleCaracteristicaCreada"
      @close="modalCrear = false"
    />
  </div>
</template>

<script setup>
import { ref, computed, onMounted, watch, onUnmounted } from 'vue'
import { Listbox, ListboxButton, ListboxLabel, ListboxOption, ListboxOptions } from '@headlessui/vue'
import { CheckIcon, ChevronUpDownIcon } from '@heroicons/vue/20/solid'
import { CaracteristicasService } from '../services/caracteristicasService.ts'
import ModalCreateCaracteristica from './ModalCreateCaracteristica.vue'
import { useToast } from 'primevue/usetoast'

const toast = useToast()

const props = defineProps({
  modelValue: {
    type: Object,
    default: null
  }
})

const emit = defineEmits(['update:modelValue', 'addCaracteristica'])

const searchKeyword = ref('')
const caracteristicas = ref([])
const selected = ref(props.modelValue)
const modalCrear = ref(false)
const loading = ref(false)

// Computed filtered list
const caracteristicasFiltradas = computed(() => {
  if (!searchKeyword.value) return caracteristicas.value
  
  return caracteristicas.value.filter(c =>
    c.nombre.toLowerCase().includes(searchKeyword.value.toLowerCase())
  )
})

// Check if name exists
const existeElNombre = computed(() =>
  caracteristicas.value.some(c =>
    c.nombre.toLowerCase() === searchKeyword.value.toLowerCase()
  )
)

// Load characteristics on mount
onMounted(async () => {
  await cargarCaracteristicas()
})

// Clean up blob URLs on unmount
onUnmounted(() => {
  CaracteristicasService.cleanupBlobUrls(caracteristicas.value)
})

// Watch for selection changes
watch(selected, (newValue) => {
  if (newValue) {
    emit('update:modelValue', newValue)
    emit('addCaracteristica', newValue)
  }
})

// Load characteristics from API
const cargarCaracteristicas = async () => {
  loading.value = true
  try {
    const response = await CaracteristicasService.getCaracteristicas()
    
    if (response.isSuccess && response.data) {
      // Process with images
      caracteristicas.value = await CaracteristicasService.processCaracteristicasWithImages(response.data)
    } else {
      throw new Error(response.message || 'Error al cargar características')
    }
  } catch (error) {
    console.error('Error loading characteristics:', error)
    toast.add({
      severity: 'error',
      summary: 'Error',
      detail: 'No se pudieron cargar las características',
      life: 3000
    })
  } finally {
    loading.value = false
  }
}

// Handle Enter key press
const handleEnterKey = () => {
  if (searchKeyword.value && !existeElNombre.value) {
    mostrarModalCrear()
  }
}

// Show create modal
const mostrarModalCrear = () => {
  modalCrear.value = true
}

// Handle new characteristic created
const handleCaracteristicaCreada = async (nuevaCaracteristica) => {
  modalCrear.value = false
  
  // Reload characteristics
  await cargarCaracteristicas()
  
  // Find and select the new characteristic
  const created = caracteristicas.value.find(c => c.nombre === nuevaCaracteristica.nombre)
  if (created) {
    selected.value = created
  }
  
  // Clear search
  searchKeyword.value = ''
  
  toast.add({
    severity: 'success',
    summary: 'Característica creada',
    detail: `${nuevaCaracteristica.nombre} fue creada exitosamente`,
    life: 3000
  })
}
</script>

<style scoped>
/* Glassmorphism classes */
.glass-input {
  @apply bg-white/10 backdrop-blur-sm border border-white/30 rounded-lg 
         text-white placeholder-gray-300 focus:outline-none focus:ring-2 
         focus:ring-primary-400 focus:border-transparent transition-all;
}

.glass-dropdown-menu {
  @apply bg-neutral-800/95 backdrop-blur-2xl border border-white/20 
         rounded-2xl shadow-2xl shadow-black/50 overflow-hidden;
}


.glass-button-gradient {
  @apply bg-gradient-to-r from-primary-400 via-secondary-400 to-accent-400 
         hover:from-primary-500 hover:via-secondary-500 hover:to-accent-500 
         text-white rounded-lg transition-all duration-200 
         shadow-lg shadow-primary-400/20 hover:shadow-primary-400/40;
}

/* Custom scrollbar */
.scrollbar-thin {
  scrollbar-width: thin;
}

.scrollbar-thumb-white\/20::-webkit-scrollbar-thumb {
  background-color: rgba(255, 255, 255, 0.2);
  border-radius: 9999px;
}

.scrollbar-track-transparent::-webkit-scrollbar-track {
  background-color: transparent;
}

::-webkit-scrollbar {
  width: 6px;
  height: 6px;
}
</style>