<template>
  <Teleport to="body">
    <Transition name="modal">
      <div class="fixed inset-0 z-50 flex items-center justify-center p-4" @click.self="close">
        <!-- Backdrop -->
        <div class="absolute inset-0 bg-black/60 backdrop-blur-xl"></div>
        
        <!-- Modal content -->
        <div class="relative w-full max-w-md glass-modal">
          <!-- Header -->
          <div class="flex items-center justify-between p-6 border-b border-white/10">
            <h3 class="text-xl font-bold bg-gradient-to-r from-primary-400 via-secondary-400 to-accent-400 
                       bg-clip-text text-transparent">
              Nueva Característica
            </h3>
            <button @click="close" 
                    class="p-2 rounded-lg hover:bg-white/10 transition-colors">
              <svg class="w-5 h-5 text-white/60" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12" />
              </svg>
            </button>
          </div>
          
          <!-- Form -->
          <form @submit.prevent="handleSubmit" class="p-6 space-y-4">
            <!-- Name field -->
            <div>
              <label class="block text-sm font-medium text-white/90 mb-2">
                Nombre
              </label>
              <input
                v-model="form.nombre"
                type="text"
                required
                class="glass-input w-full px-4 py-2"
                placeholder="Ej: WiFi, Aire acondicionado..."
              />
            </div>
            
            <!-- Description field -->
            <div>
              <label class="block text-sm font-medium text-white/90 mb-2">
                Descripción
              </label>
              <textarea
                v-model="form.descripcion"
                rows="3"
                class="glass-input w-full px-4 py-2 resize-none"
                placeholder="Descripción opcional..."
              />
            </div>
            
            <!-- Icon upload -->
            <div>
              <label class="block text-sm font-medium text-white/90 mb-2">
                Icono
              </label>
              <div class="space-y-2">
                <!-- File input -->
                <label class="glass-input flex items-center justify-center px-4 py-3 cursor-pointer 
                              hover:bg-white/15 transition-all group">
                  <input
                    type="file"
                    accept="image/*"
                    @change="handleFileSelect"
                    class="hidden"
                  />
                  <div class="flex items-center gap-3">
                    <svg class="w-5 h-5 text-primary-400 group-hover:text-primary-300" 
                         fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" 
                            d="M4 16l4.586-4.586a2 2 0 012.828 0L16 16m-2-2l1.586-1.586a2 2 0 012.828 0L20 14m-6-6h.01M6 20h12a2 2 0 002-2V6a2 2 0 00-2-2H6a2 2 0 00-2 2v12a2 2 0 002 2z" />
                    </svg>
                    <span class="text-white/60 text-sm">
                      {{ form.icono ? form.icono.name : 'Seleccionar imagen...' }}
                    </span>
                  </div>
                </label>
                
                <!-- Preview -->
                <div v-if="preview" class="relative">
                  <img :src="preview" alt="Preview" 
                       class="w-full h-32 object-cover rounded-lg border border-white/20" />
                  <button
                    type="button"
                    @click="removeImage"
                    class="absolute top-2 right-2 p-1 rounded-lg bg-red-500/80 hover:bg-red-500 transition-colors"
                  >
                    <svg class="w-4 h-4 text-white" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12" />
                    </svg>
                  </button>
                </div>
              </div>
            </div>
            
            <!-- Actions -->
            <div class="flex gap-3 pt-4">
              <button
                type="button"
                @click="close"
                class="flex-1 px-4 py-2 glass-button-secondary"
              >
                Cancelar
              </button>
              <button
                type="submit"
                :disabled="loading || !form.nombre"
                class="flex-1 px-4 py-2 glass-button-gradient disabled:opacity-50 disabled:cursor-not-allowed"
              >
                <span v-if="!loading">Crear</span>
                <span v-else class="flex items-center justify-center gap-2">
                  <div class="w-4 h-4 border-2 border-white border-t-transparent rounded-full animate-spin"></div>
                  Creando...
                </span>
              </button>
            </div>
          </form>
        </div>
      </div>
    </Transition>
  </Teleport>
</template>

<script setup>
import { ref, reactive, watch } from 'vue'
import { CaracteristicasService } from '../services/caracteristicasService.ts'
import { useToast } from 'primevue/usetoast'

const toast = useToast()

const props = defineProps({
  nombre: {
    type: String,
    default: ''
  }
})

const emit = defineEmits(['caracteristicaCreada', 'close'])

const form = reactive({
  nombre: props.nombre,
  descripcion: '',
  icono: null
})

const preview = ref(null)
const loading = ref(false)

// Watch for name changes from props
watch(() => props.nombre, (newValue) => {
  form.nombre = newValue
})

// Handle file selection
const handleFileSelect = (event) => {
  const file = event.target.files[0]
  if (file) {
    form.icono = file
    
    // Create preview
    const reader = new FileReader()
    reader.onload = (e) => {
      preview.value = e.target.result
    }
    reader.readAsDataURL(file)
  }
}

// Remove selected image
const removeImage = () => {
  form.icono = null
  preview.value = null
}

// Handle form submission
const handleSubmit = async () => {
  if (!form.nombre.trim()) {
    toast.add({
      severity: 'warn',
      summary: 'Campo requerido',
      detail: 'El nombre es obligatorio',
      life: 3000
    })
    return
  }
  
  loading.value = true
  
  try {
    const response = await CaracteristicasService.createCaracteristica({
      nombre: form.nombre.trim(),
      descripcion: form.descripcion.trim(),
      icono: form.icono
    })
    
    if (response.isSuccess) {
      toast.add({
        severity: 'success',
        summary: 'Éxito',
        detail: 'Característica creada correctamente',
        life: 3000
      })
      
      emit('caracteristicaCreada', response.data)
      close()
    } else {
      throw new Error(response.message || 'Error al crear característica')
    }
  } catch (error) {
    console.error('Error creating characteristic:', error)
    toast.add({
      severity: 'error',
      summary: 'Error',
      detail: error.message || 'No se pudo crear la característica',
      life: 3000
    })
  } finally {
    loading.value = false
  }
}

// Close modal
const close = () => {
  if (!loading.value) {
    emit('close')
  }
}
</script>

<style scoped>
/* Glassmorphism modal */
.glass-modal {
  @apply bg-neutral-900/95 backdrop-blur-2xl border border-white/20 
         rounded-3xl shadow-2xl shadow-black/50;
}

/* Glassmorphism inputs */
.glass-input {
  @apply bg-white/10 backdrop-blur-sm border border-white/30 rounded-lg 
         text-white placeholder-gray-400 focus:outline-none focus:ring-2 
         focus:ring-primary-400 focus:border-transparent transition-all;
}

/* Gradient button */
.glass-button-gradient {
  @apply bg-gradient-to-r from-primary-400 via-secondary-400 to-accent-400 
         hover:from-primary-500 hover:via-secondary-500 hover:to-accent-500 
         text-white font-medium rounded-lg transition-all duration-200 
         shadow-lg shadow-primary-400/20 hover:shadow-primary-400/40;
}

/* Secondary button */
.glass-button-secondary {
  @apply bg-white/10 hover:bg-white/20 backdrop-blur-sm border border-white/30 
         text-white font-medium rounded-lg transition-all duration-200;
}

/* Modal animations */
.modal-enter-active,
.modal-leave-active {
  transition: all 0.3s ease;
}

.modal-enter-from,
.modal-leave-to {
  opacity: 0;
}

.modal-enter-from .glass-modal,
.modal-leave-to .glass-modal {
  transform: scale(0.9);
}
</style>