<template>
  <div class="glass-container p-6 space-y-6">
    <!-- Header with gradient -->
    <div class="flex items-center justify-between mb-6">
      <h2 class="text-2xl font-bold bg-gradient-to-r from-primary-400 via-secondary-400 to-accent-400 
                 bg-clip-text text-transparent">
        Características de la Habitación
      </h2>
      <div class="flex items-center gap-2">
        <span class="text-white/60 text-sm">{{ caracteristicas.length }} seleccionadas</span>
        <div class="w-2 h-2 rounded-full bg-gradient-to-r from-primary-400 to-secondary-400 animate-pulse"></div>
      </div>
    </div>

    <!-- Add characteristic section -->
    <div class="glass-card p-4 space-y-3">
      <label class="text-sm font-medium text-white/90">Agregar Característica</label>
      <DropDownCreateSearchCaracters @addCaracteristica="agregarCaracteristica" />
    </div>

    <!-- Characteristics grid -->
    <div v-if="caracteristicas.length > 0" class="space-y-4">
      <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4 max-h-[400px] overflow-y-auto 
                  scrollbar-thin scrollbar-thumb-white/20 scrollbar-track-transparent pr-2">
        <TransitionGroup name="list" tag="div" class="contents">
          <div
            v-for="(caracteristica, index) in caracteristicas"
            :key="caracteristica.caracteristicaId"
            class="glass-card p-4 hover:bg-white/15 transition-all duration-300 group"
          >
            <div class="flex items-center justify-between">
              <div class="flex items-center space-x-3">
                <!-- Icon with glow effect -->
                <div class="relative">
                  <div v-if="caracteristica.icono" 
                       class="absolute inset-0 bg-gradient-to-r from-primary-400 to-secondary-400 
                              rounded-lg blur-lg opacity-50 group-hover:opacity-75 transition-opacity"></div>
                  <img v-if="caracteristica.icono" 
                       :src="caracteristica.icono" 
                       :alt="caracteristica.nombre"
                       class="relative w-10 h-10 object-cover rounded-lg bg-white/10 p-1" />
                  <div v-else 
                       class="relative w-10 h-10 rounded-lg bg-gradient-to-br from-primary-400/20 to-secondary-400/20 
                              flex items-center justify-center">
                    <svg class="w-6 h-6 text-white/60" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" 
                            d="M9.663 17h4.673M12 3v1m6.364 1.636l-.707.707M21 12h-1M4 12H3m3.343-5.657l-.707-.707m2.828 9.9a5 5 0 117.072 0l-.548.547A3.374 3.374 0 0014 18.469V19a2 2 0 11-4 0v-.531c0-.895-.356-1.754-.988-2.386l-.548-.547z" />
                    </svg>
                  </div>
                </div>
                
                <!-- Name -->
                <div>
                  <p class="text-white font-medium">{{ caracteristica.nombre }}</p>
                  <p v-if="caracteristica.descripcion" class="text-white/60 text-xs mt-1">
                    {{ caracteristica.descripcion }}
                  </p>
                </div>
              </div>
              
              <!-- Delete button -->
              <button
                @click="eliminarCaracteristica(index)"
                class="p-2 rounded-lg bg-red-500/10 hover:bg-red-500/20 border border-red-500/30 
                       hover:border-red-500/50 transition-all duration-200 group/btn"
                title="Eliminar"
              >
                <svg class="w-4 h-4 text-red-400 group-hover/btn:text-red-300" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" 
                        d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16" />
                </svg>
              </button>
            </div>
          </div>
        </TransitionGroup>
      </div>
    </div>

    <!-- Empty state -->
    <div v-else class="glass-card p-12 text-center">
      <div class="w-20 h-20 mx-auto mb-4 rounded-full bg-gradient-to-br from-primary-400/20 to-secondary-400/20 
                  flex items-center justify-center">
        <svg class="w-10 h-10 text-white/40" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" 
                d="M12 6v6m0 0v6m0-6h6m-6 0H6" />
        </svg>
      </div>
      <p class="text-white/60">No hay características seleccionadas</p>
      <p class="text-white/40 text-sm mt-2">Agrega características desde el selector superior</p>
    </div>

    <!-- Loading overlay -->
    <Transition name="fade">
      <div v-if="loading" class="fixed inset-0 bg-black/50 backdrop-blur-sm z-50 flex items-center justify-center">
        <div class="glass-card p-6 flex items-center space-x-3">
          <div class="w-5 h-5 border-2 border-primary-400 border-t-transparent rounded-full animate-spin"></div>
          <span class="text-white">{{ loadingMessage }}</span>
        </div>
      </div>
    </Transition>
  </div>
</template>

<script setup>
import { ref, watch, onUnmounted } from 'vue'
import { CaracteristicasService } from '../services/caracteristicasService.ts'
import DropDownCreateSearchCaracters from './DropDownCreateSearchCaracters.vue'
import { useToast } from 'primevue/usetoast'

const toast = useToast()

const props = defineProps({
  trigger: Boolean,
  idHabitacion: Number,
  listaCaracteristicas: Array
})

const emit = defineEmits(['caracteristicasActualizadas'])

const caracteristicas = ref([])
const loading = ref(false)
const loadingMessage = ref('Cargando...')

// Watch for initial characteristics list
watch(() => props.listaCaracteristicas, async (newList) => {
  if (newList && newList.length > 0) {
    loading.value = true
    loadingMessage.value = 'Cargando características...'
    try {
      caracteristicas.value = await CaracteristicasService.processCaracteristicasWithImages(newList)
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
  } else {
    // Cleanup any existing blob URLs
    CaracteristicasService.cleanupBlobUrls(caracteristicas.value)
    caracteristicas.value = []
  }
}, { immediate: true })

// Watch for save trigger
watch(() => props.trigger, (newVal) => {
  if (newVal === true) {
    asignarCaracteristicasAHabitacion()
  }
})

// Clean up blob URLs on unmount
onUnmounted(() => {
  CaracteristicasService.cleanupBlobUrls(caracteristicas.value)
})

// Add characteristic
const agregarCaracteristica = async (nuevaCaracteristica) => {
  if (!nuevaCaracteristica) return
  
  const existente = caracteristicas.value.some(
    c => c.caracteristicaId === nuevaCaracteristica.caracteristicaId
  )
  
  if (existente) {
    toast.add({
      severity: 'warn',
      summary: 'Característica duplicada',
      detail: 'Esta característica ya fue agregada',
      life: 3000
    })
    return
  }

  // Process the new characteristic with its image
  if (nuevaCaracteristica.icono && nuevaCaracteristica.caracteristicaId) {
    try {
      const processedList = await CaracteristicasService.processCaracteristicasWithImages([nuevaCaracteristica])
      caracteristicas.value.push(processedList[0])
    } catch (error) {
      console.error('Error processing characteristic:', error)
      caracteristicas.value.push(nuevaCaracteristica)
    }
  } else {
    caracteristicas.value.push(nuevaCaracteristica)
  }

  toast.add({
    severity: 'success',
    summary: 'Característica agregada',
    detail: `${nuevaCaracteristica.nombre} fue agregada`,
    life: 2000
  })
}

// Remove characteristic
const eliminarCaracteristica = (index) => {
  const removed = caracteristicas.value[index]
  
  // Clean up blob URL if exists
  if (removed.icono && typeof removed.icono === 'string' && removed.icono.startsWith('blob:')) {
    URL.revokeObjectURL(removed.icono)
  }
  
  caracteristicas.value.splice(index, 1)
  
  toast.add({
    severity: 'info',
    summary: 'Característica eliminada',
    detail: `${removed.nombre} fue eliminada`,
    life: 2000
  })
}

// Assign characteristics to room
const asignarCaracteristicasAHabitacion = async () => {
  if (!props.idHabitacion || caracteristicas.value.length === 0) {
    toast.add({
      severity: 'warn',
      summary: 'Datos incompletos',
      detail: 'Debe seleccionar características y una habitación válida',
      life: 3000
    })
    return
  }

  loading.value = true
  loadingMessage.value = 'Guardando características...'
  
  try {
    const caracteristicaIds = caracteristicas.value.map(c => c.caracteristicaId)
    const response = await CaracteristicasService.assignCaracteristicasToRoom(
      props.idHabitacion,
      caracteristicaIds
    )
    
    if (response.isSuccess) {
      toast.add({
        severity: 'success',
        summary: 'Éxito',
        detail: response.message || 'Características asignadas correctamente',
        life: 3000
      })
      
      // Clean up and reset
      CaracteristicasService.cleanupBlobUrls(caracteristicas.value)
      caracteristicas.value = []
      emit('caracteristicasActualizadas')
    } else {
      throw new Error(response.message || 'Error al asignar características')
    }
  } catch (error) {
    console.error('Error:', error)
    toast.add({
      severity: 'error',
      summary: 'Error',
      detail: error.message || 'Error al guardar las características',
      life: 3000
    })
  } finally {
    loading.value = false
  }
}
</script>

<style scoped>
/* Glassmorphism classes following project guidelines */
.glass-container {
  @apply bg-white/5 backdrop-blur-2xl border border-white/20 rounded-3xl;
}

.glass-card {
  @apply bg-white/10 backdrop-blur-md border border-white/20 rounded-xl;
}

/* Animations */
.list-enter-active,
.list-leave-active {
  transition: all 0.3s ease;
}

.list-enter-from {
  opacity: 0;
  transform: translateX(-30px);
}

.list-leave-to {
  opacity: 0;
  transform: translateX(30px);
}

.fade-enter-active,
.fade-leave-active {
  transition: opacity 0.3s ease;
}

.fade-enter-from,
.fade-leave-to {
  opacity: 0;
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
}
</style>