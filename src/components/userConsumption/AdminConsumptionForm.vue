<template>
  <!-- Modal Overlay -->
  <div class="fixed inset-0 bg-black/60 backdrop-blur-xl z-50 flex items-center justify-center p-4">
    <div class="glass-container max-w-3xl w-full max-h-[90vh] overflow-hidden">
      <!-- Header -->
      <div class="bg-gradient-to-r from-red-400 to-orange-400 p-6 rounded-t-3xl">
        <div class="flex items-center justify-between">
          <div class="flex items-center">
            <div class="bg-white/20 p-3 rounded-full mr-3">
              <i class="pi pi-user-plus text-white text-xl"></i>
            </div>
            <div>
              <h2 class="text-2xl font-bold text-white">👥 Crear Consumo para Usuario</h2>
              <p class="text-white/80">Registra un consumo para cualquier usuario del sistema</p>
            </div>
          </div>
          <button
            @click="$emit('close')"
            class="bg-white/20 hover:bg-white/30 p-2 rounded-full transition-colors"
          >
            <i class="pi pi-times text-white text-lg"></i>
          </button>
        </div>
      </div>

      <!-- Form Content -->
      <div class="p-6 max-h-[calc(90vh-140px)] overflow-y-auto">
        <form @submit.prevent="handleSubmit" class="space-y-6">
          <!-- User Selection (Admin Only) -->
          <div class="space-y-2 bg-orange-500/10 p-4 rounded-lg border border-orange-500/30">
            <label class="block text-white font-semibold">
              <i class="pi pi-users text-orange-400 mr-2"></i>
              Seleccionar Usuario *
            </label>
            <div class="relative">
              <input
                v-model="userSearchTerm"
                type="text"
                placeholder="Buscar usuario por nombre, username o ID..."
                class="glass-input w-full px-4 py-3 pr-10"
                @input="searchUsers"
                @focus="showUserDropdown = true"
                required
              />
              <i class="pi pi-search absolute right-3 top-1/2 transform -translate-y-1/2 text-gray-400"></i>
            </div>
            
            <!-- Users Dropdown -->
            <div v-if="showUserDropdown && filteredUsers.length > 0" 
                 class="glass-card mt-2 max-h-48 overflow-y-auto z-50">
              <div
                v-for="user in filteredUsers"
                :key="user.id"
                @click="selectUser(user)"
                class="p-3 hover:bg-white/10 cursor-pointer transition-colors border-b border-white/10 last:border-b-0"
              >
                <div class="flex justify-between items-center">
                  <div>
                    <p class="text-white font-medium">{{ user.fullName }}</p>
                    <p class="text-gray-400 text-sm">@{{ user.userName }} • ID: {{ user.id }}</p>
                  </div>
                  <div class="text-right">
                    <span class="bg-blue-500/20 text-blue-300 px-2 py-1 rounded text-xs">
                      {{ user.role }}
                    </span>
                  </div>
                </div>
              </div>
            </div>

            <!-- Selected User Display -->
            <div v-if="form.userId" class="glass-card p-4 bg-orange-500/10 border-orange-500/30 mt-3">
              <div class="flex justify-between items-center">
                <div>
                  <p class="text-white font-medium">👤 {{ selectedUser?.fullName }}</p>
                  <p class="text-gray-400 text-sm">@{{ selectedUser?.userName }} • {{ selectedUser?.role }}</p>
                </div>
                <button
                  type="button"
                  @click="clearUser"
                  class="bg-red-500/20 hover:bg-red-500/30 p-2 rounded-full transition-colors"
                >
                  <i class="pi pi-times text-red-400"></i>
                </button>
              </div>
            </div>
          </div>

          <!-- Article Selection -->
          <div class="space-y-2">
            <label class="block text-white font-semibold">
              <i class="pi pi-tag text-primary-400 mr-2"></i>
              Artículo *
            </label>
            <div class="relative">
              <input
                v-model="articleSearchTerm"
                type="text"
                placeholder="Buscar artículo por nombre o código..."
                class="glass-input w-full px-4 py-3 pr-10"
                @input="searchArticles"
                @focus="showArticleDropdown = true"
                required
              />
              <i class="pi pi-search absolute right-3 top-1/2 transform -translate-y-1/2 text-gray-400"></i>
            </div>
            
            <!-- Articles Dropdown -->
            <div v-if="showArticleDropdown && filteredArticles.length > 0" 
                 class="glass-card mt-2 max-h-48 overflow-y-auto">
              <div
                v-for="article in filteredArticles"
                :key="article.id"
                @click="selectArticle(article)"
                class="p-3 hover:bg-white/10 cursor-pointer transition-colors border-b border-white/10 last:border-b-0"
              >
                <div class="flex justify-between items-center">
                  <div>
                    <p class="text-white font-medium">{{ article.nombre }}</p>
                    <p class="text-gray-400 text-sm">Código: {{ article.codigo }}</p>
                  </div>
                  <div class="text-right">
                    <p class="text-green-400 font-semibold">{{ formatCurrency(article.precio) }}</p>
                  </div>
                </div>
              </div>
            </div>

            <!-- Selected Article Display -->
            <div v-if="form.articuloId" class="glass-card p-4 bg-green-500/10 border-green-500/30">
              <div class="flex justify-between items-center">
                <div>
                  <p class="text-white font-medium">{{ selectedArticle?.nombre }}</p>
                  <p class="text-gray-400 text-sm">Código: {{ selectedArticle?.codigo }}</p>
                </div>
                <button
                  type="button"
                  @click="clearArticle"
                  class="bg-red-500/20 hover:bg-red-500/30 p-2 rounded-full transition-colors"
                >
                  <i class="pi pi-times text-red-400"></i>
                </button>
              </div>
            </div>
          </div>

          <!-- Quantity -->
          <div class="space-y-2">
            <label class="block text-white font-semibold">
              <i class="pi pi-box text-secondary-400 mr-2"></i>
              Cantidad *
            </label>
            <input
              v-model.number="form.cantidad"
              type="number"
              min="1"
              step="1"
              placeholder="Ingrese la cantidad"
              class="glass-input w-full px-4 py-3"
              required
            />
          </div>

          <!-- Unit Price (Optional Override) -->
          <div class="space-y-2">
            <label class="block text-white font-semibold">
              <i class="pi pi-dollar text-green-400 mr-2"></i>
              Precio Unitario (Opcional)
            </label>
            <input
              v-model.number="form.precioUnitario"
              type="number"
              min="0"
              step="0.01"
              placeholder="Dejar vacío para usar precio del artículo"
              class="glass-input w-full px-4 py-3"
            />
            <p class="text-gray-400 text-xs">
              💡 Si no se especifica, se usará el precio del artículo: {{ formatCurrency(selectedArticle?.precio || 0) }}
            </p>
          </div>

          <!-- Room Selection (Optional) -->
          <div class="space-y-2">
            <label class="block text-white font-semibold">
              <i class="pi pi-home text-yellow-400 mr-2"></i>
              Habitación (Opcional)
            </label>
            <select
              v-model="form.habitacionId"
              class="glass-input w-full px-4 py-3"
            >
              <option value="">Seleccionar habitación...</option>
              <option
                v-for="room in availableRooms"
                :key="room.id"
                :value="room.id"
              >
                Habitación {{ room.numero }} - {{ room.categoria }}
              </option>
            </select>
          </div>

          <!-- Consumption Type -->
          <div class="space-y-2">
            <label class="block text-white font-semibold">
              <i class="pi pi-tags text-purple-400 mr-2"></i>
              Tipo de Consumo
            </label>
            <select
              v-model="form.tipoConsumo"
              class="glass-input w-full px-4 py-3"
            >
              <option value="">Seleccionar tipo...</option>
              <option value="Servicio">Servicio</option>
              <option value="Habitacion">Habitación</option>
            </select>
          </div>

          <!-- Observations -->
          <div class="space-y-2">
            <label class="block text-white font-semibold">
              <i class="pi pi-comment text-blue-400 mr-2"></i>
              Observaciones (Opcional)
            </label>
            <textarea
              v-model="form.observaciones"
              rows="3"
              placeholder="Agregar comentarios o notas adicionales..."
              class="glass-input w-full px-4 py-3 resize-none"
            ></textarea>
          </div>

          <!-- Admin Notes -->
          <div class="space-y-2 bg-blue-500/10 p-4 rounded-lg border border-blue-500/30">
            <label class="block text-white font-semibold">
              <i class="pi pi-shield text-blue-400 mr-2"></i>
              Notas Administrativas (Opcional)
            </label>
            <textarea
              v-model="form.notasAdmin"
              rows="2"
              placeholder="Motivo o notas internas del registro (solo visible para administradores)..."
              class="glass-input w-full px-4 py-3 resize-none"
            ></textarea>
            <p class="text-blue-300 text-xs">
              🔒 Esta información solo es visible para administradores
            </p>
          </div>

          <!-- Total Preview -->
          <div v-if="totalPreview > 0" class="glass-card p-4 bg-blue-500/10 border-blue-500/30">
            <div class="flex justify-between items-center">
              <div>
                <p class="text-white font-semibold">Total a registrar para {{ selectedUser?.fullName }}:</p>
                <p class="text-gray-400 text-sm">
                  {{ form.cantidad || 0 }} × {{ formatCurrency(effectivePrice) }}
                </p>
              </div>
              <div>
                <p class="text-green-400 font-bold text-2xl">{{ formatCurrency(totalPreview) }}</p>
              </div>
            </div>
          </div>

          <!-- Form Actions -->
          <div class="flex items-center justify-end space-x-4 pt-4 border-t border-white/10">
            <button
              type="button"
              @click="$emit('close')"
              class="glass-button py-3 px-6 text-white hover:text-red-300 transform hover:scale-105 transition-all"
            >
              <i class="pi pi-times mr-2"></i>
              Cancelar
            </button>
            
            <button
              type="submit"
              :disabled="!isFormValid || loading"
              class="bg-gradient-to-r from-orange-400 to-red-400 
                     hover:from-orange-500 hover:to-red-500 
                     disabled:opacity-50 disabled:cursor-not-allowed
                     text-white font-bold py-3 px-6 rounded-lg 
                     transition-all duration-300 transform hover:scale-105"
            >
              <i :class="loading ? 'pi pi-spinner pi-spin' : 'pi pi-user-plus'" class="mr-2"></i>
              {{ loading ? 'Creando...' : '👥 Crear Consumo para Usuario' }}
            </button>
          </div>
        </form>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import { useToast } from 'primevue/usetoast'

const emit = defineEmits(['close', 'created'])
const toast = useToast()

// Props to receive the createConsumption method and loading state
const props = defineProps({
  createConsumption: {
    type: Function,
    required: true
  },
  loading: {
    type: Boolean,
    default: false
  }
})

// Form state
const form = ref({
  userId: null,
  articuloId: null,
  cantidad: 1,
  precioUnitario: null,
  habitacionId: null,
  reservaId: null,
  tipoConsumo: '',
  observaciones: '',
  notasAdmin: ''
})

// UI state
const userSearchTerm = ref('')
const articleSearchTerm = ref('')
const showUserDropdown = ref(false)
const showArticleDropdown = ref(false)
const selectedUser = ref(null)
const selectedArticle = ref(null)

// Mock data - in real app, these would come from API
const availableUsers = ref([
  { id: '001', userName: 'admin', fullName: 'Administrador Sistema', role: 'Admin' },
  { id: '002', userName: 'juan.perez', fullName: 'Juan Pérez García', role: 'Usuario' },
  { id: '003', userName: 'maria.lopez', fullName: 'María López Torres', role: 'Usuario' },
  { id: '004', userName: 'carlos.ruiz', fullName: 'Carlos Ruiz Mendoza', role: 'Director' },
  { id: '005', userName: 'ana.torres', fullName: 'Ana Torres Silva', role: 'Cajero' },
  { id: '006', userName: 'pedro.silva', fullName: 'Pedro Silva Mora', role: 'Usuario' },
  { id: '007', userName: 'laura.garcia', fullName: 'Laura García Vega', role: 'Mucama' },
  { id: '008', userName: 'diego.luna', fullName: 'Diego Luna Castillo', role: 'Cajero Stock' }
])

const availableArticles = ref([
  { id: 1, nombre: 'Coca Cola', codigo: 'CC001', precio: 2.50 },
  { id: 2, nombre: 'Agua Mineral', codigo: 'AM001', precio: 1.50 },
  { id: 3, nombre: 'Cerveza', codigo: 'CV001', precio: 5.00 },
  { id: 4, nombre: 'Sandwich', codigo: 'SW001', precio: 8.00 },
  { id: 5, nombre: 'Pizza Personal', codigo: 'PP001', precio: 15.00 },
  { id: 6, nombre: 'Café', codigo: 'CF001', precio: 3.00 },
  { id: 7, nombre: 'Galletas', codigo: 'GL001', precio: 4.50 },
  { id: 8, nombre: 'Jugo Natural', codigo: 'JN001', precio: 6.00 }
])

const availableRooms = ref([
  { id: 1, numero: '101', categoria: 'Standard' },
  { id: 2, numero: '102', categoria: 'Standard' },
  { id: 3, numero: '201', categoria: 'Superior' },
  { id: 4, numero: '301', categoria: 'Suite' }
])

// Computed properties
const filteredUsers = computed(() => {
  if (!userSearchTerm.value) return availableUsers.value
  
  const term = userSearchTerm.value.toLowerCase()
  return availableUsers.value.filter(user =>
    user.fullName.toLowerCase().includes(term) ||
    user.userName.toLowerCase().includes(term) ||
    user.id.includes(term)
  )
})

const filteredArticles = computed(() => {
  if (!articleSearchTerm.value) return availableArticles.value
  
  const term = articleSearchTerm.value.toLowerCase()
  return availableArticles.value.filter(article =>
    article.nombre.toLowerCase().includes(term) ||
    article.codigo.toLowerCase().includes(term)
  )
})

const effectivePrice = computed(() => {
  return form.value.precioUnitario || selectedArticle.value?.precio || 0
})

const totalPreview = computed(() => {
  return (form.value.cantidad || 0) * effectivePrice.value
})

const isFormValid = computed(() => {
  return form.value.userId && 
         form.value.articuloId && 
         form.value.cantidad && 
         form.value.cantidad > 0 &&
         effectivePrice.value > 0
})

// Methods
const searchUsers = () => {
  showUserDropdown.value = true
}

const searchArticles = () => {
  showArticleDropdown.value = true
}

const selectUser = (user) => {
  form.value.userId = user.id
  selectedUser.value = user
  userSearchTerm.value = user.fullName
  showUserDropdown.value = false
}

const selectArticle = (article) => {
  form.value.articuloId = article.id
  selectedArticle.value = article
  articleSearchTerm.value = article.nombre
  showArticleDropdown.value = false
  
  // Auto-fill price if not manually set
  if (!form.value.precioUnitario) {
    form.value.precioUnitario = article.precio
  }
}

const clearUser = () => {
  form.value.userId = null
  selectedUser.value = null
  userSearchTerm.value = ''
}

const clearArticle = () => {
  form.value.articuloId = null
  selectedArticle.value = null
  articleSearchTerm.value = ''
  form.value.precioUnitario = null
}

const formatCurrency = (amount) => {
  if (amount == null || amount == undefined) return 'S/ 0.00'
  return new Intl.NumberFormat('es-PE', {
    style: 'currency',
    currency: 'PEN'
  }).format(amount)
}

const handleSubmit = async () => {
  if (!isFormValid.value) return

  try {
    const consumptionData = {
      userId: form.value.userId, // This is the key difference - specify target user
      articuloId: form.value.articuloId,
      cantidad: form.value.cantidad,
      precioUnitario: form.value.precioUnitario || selectedArticle.value?.precio,
      habitacionId: form.value.habitacionId || null,
      reservaId: form.value.reservaId || null,
      tipoConsumo: form.value.tipoConsumo || 'Servicio',
      observaciones: form.value.observaciones || null,
      notasAdmin: form.value.notasAdmin || null
    }

    const result = await props.createConsumption(consumptionData)
    
    toast.add({
      severity: 'success',
      summary: 'Éxito',
      detail: `Consumo creado exitosamente para ${selectedUser.value.fullName}`,
      life: 5000
    })

    emit('created', result)
  } catch (error) {
    console.error('Error creating consumption for user:', error)
    toast.add({
      severity: 'error',
      summary: 'Error',
      detail: 'Error al crear el consumo para el usuario',
      life: 5000
    })
  }
}

// Close dropdowns when clicking outside
const handleClickOutside = (event) => {
  if (!event.target.closest('.relative')) {
    showUserDropdown.value = false
    showArticleDropdown.value = false
  }
}

onMounted(() => {
  document.addEventListener('click', handleClickOutside)
})
</script>

<style scoped>
.glass-container {
  @apply bg-white/5 backdrop-blur-2xl border border-white/20 rounded-3xl;
}

.glass-card {
  @apply bg-white/10 backdrop-blur-md border border-white/20 rounded-xl;
}

.glass-button {
  @apply bg-white/10 hover:bg-white/20 backdrop-blur-sm border border-white/30 rounded-lg transition-all;
}

.glass-input {
  @apply bg-white/10 backdrop-blur-sm border border-white/30 rounded-lg text-white placeholder-gray-300;
}

.glass-input:focus {
  @apply ring-2 ring-primary-400 border-primary-400 outline-none;
}

/* Custom scrollbar */
::-webkit-scrollbar {
  width: 6px;
}

::-webkit-scrollbar-track {
  background: rgba(255, 255, 255, 0.1);
  border-radius: 3px;
}

::-webkit-scrollbar-thumb {
  background: rgba(255, 255, 255, 0.3);
  border-radius: 3px;
}

::-webkit-scrollbar-thumb:hover {
  background: rgba(255, 255, 255, 0.5);
}
</style>