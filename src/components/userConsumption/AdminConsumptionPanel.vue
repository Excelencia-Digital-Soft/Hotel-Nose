<template>
  <div class="space-y-6">
    <!-- Admin Header -->
    <div class="glass-card p-6">
      <div class="flex items-center justify-between">
        <div class="flex items-center">
          <i class="pi pi-users text-red-400 text-xl mr-2"></i>
          <h2 class="text-xl font-bold text-white">👥 Panel de Administración</h2>
        </div>
        <div class="bg-red-500/20 px-3 py-1 rounded-full border border-red-500/30">
          <span class="text-red-300 text-sm font-semibold">🔒 Solo Admin/Director</span>
        </div>
      </div>
      <p class="text-gray-300 mt-2">Gestión avanzada de consumos de todos los usuarios</p>
    </div>

    <!-- Admin Controls -->
    <div class="glass-card p-6">
      <div class="grid grid-cols-1 lg:grid-cols-2 gap-6">
        <!-- User Search -->
        <div class="space-y-4">
          <div class="flex items-center">
            <i class="pi pi-search text-primary-400 text-lg mr-2"></i>
            <h3 class="text-lg font-bold text-white">🔍 Buscar Usuario</h3>
          </div>
          
          <div class="space-y-3">
            <div class="relative">
              <input
                v-model="userSearchTerm"
                type="text"
                placeholder="Buscar por nombre de usuario o ID..."
                class="glass-input w-full px-4 py-3 pr-10"
                @input="searchUsers"
              />
              <i class="pi pi-search absolute right-3 top-1/2 transform -translate-y-1/2 text-gray-400"></i>
            </div>

            <!-- User Dropdown -->
            <div v-if="showUserDropdown && filteredUsers.length > 0" 
                 class="glass-card max-h-48 overflow-y-auto">
              <div
                v-for="user in filteredUsers"
                :key="user.id"
                @click="selectUser(user)"
                class="p-3 hover:bg-white/10 cursor-pointer transition-colors border-b border-white/10 last:border-b-0"
              >
                <div class="flex justify-between items-center">
                  <div>
                    <p class="text-white font-medium">{{ user.fullName }}</p>
                    <p class="text-gray-400 text-sm">Usuario: {{ user.userName }} | ID: {{ user.id }}</p>
                  </div>
                  <div class="text-right">
                    <p class="text-blue-400 text-sm">{{ user.role }}</p>
                  </div>
                </div>
              </div>
            </div>

            <!-- Selected User -->
            <div v-if="selectedUser" class="bg-blue-500/10 p-4 rounded-lg border border-blue-500/30">
              <div class="flex justify-between items-center">
                <div>
                  <p class="text-white font-medium">{{ selectedUser.fullName }}</p>
                  <p class="text-gray-400 text-sm">{{ selectedUser.userName }}</p>
                </div>
                <button
                  @click="clearUser"
                  class="bg-red-500/20 hover:bg-red-500/30 p-2 rounded-full transition-colors"
                >
                  <i class="pi pi-times text-red-400"></i>
                </button>
              </div>
            </div>
          </div>

          <!-- User Action Buttons -->
          <div class="flex space-x-3">
            <button
              @click="fetchUserConsumption"
              :disabled="!selectedUser || loading"
              class="bg-gradient-to-r from-blue-400 to-blue-500 
                     hover:from-blue-500 hover:to-blue-600 
                     disabled:opacity-50 disabled:cursor-not-allowed
                     text-white font-bold py-2 px-4 rounded-lg 
                     transition-all duration-300 transform hover:scale-105 flex-1"
            >
              <i class="pi pi-user mr-2"></i>
              Ver Consumo
            </button>
            
            <button
              @click="fetchUserSummary"
              :disabled="!selectedUser || loading"
              class="bg-gradient-to-r from-purple-400 to-purple-500 
                     hover:from-purple-500 hover:to-purple-600 
                     disabled:opacity-50 disabled:cursor-not-allowed
                     text-white font-bold py-2 px-4 rounded-lg 
                     transition-all duration-300 transform hover:scale-105 flex-1"
            >
              <i class="pi pi-chart-pie mr-2"></i>
              Ver Resumen
            </button>
          </div>
        </div>

        <!-- Global Actions -->
        <div class="space-y-4">
          <div class="flex items-center">
            <i class="pi pi-globe text-green-400 text-lg mr-2"></i>
            <h3 class="text-lg font-bold text-white">🌐 Acciones Globales</h3>
          </div>

          <div class="space-y-3">
            <button
              @click="fetchAllConsumption"
              :disabled="loading"
              class="bg-gradient-to-r from-green-400 to-green-500 
                     hover:from-green-500 hover:to-green-600 
                     disabled:opacity-50 disabled:cursor-not-allowed
                     text-white font-bold py-3 px-6 rounded-lg 
                     transition-all duration-300 transform hover:scale-105 w-full"
            >
              <i :class="loading ? 'pi pi-spinner pi-spin' : 'pi pi-download'" class="mr-2"></i>
              {{ loading ? 'Cargando...' : '📊 Cargar Todos los Consumos' }}
            </button>

            <button
              @click="exportAllData"
              :disabled="!allConsumption || allConsumption.length === 0"
              class="glass-button py-3 px-6 text-white hover:bg-white/20 transform hover:scale-105 transition-all w-full disabled:opacity-50"
            >
              <i class="pi pi-file-excel mr-2"></i>
              📤 Exportar Todos los Datos
            </button>

            <button
              @click="generateReport"
              :disabled="loading"
              class="bg-gradient-to-r from-yellow-400 to-yellow-500 
                     hover:from-yellow-500 hover:to-yellow-600 
                     disabled:opacity-50 disabled:cursor-not-allowed
                     text-white font-bold py-3 px-6 rounded-lg 
                     transition-all duration-300 transform hover:scale-105 w-full"
            >
              <i class="pi pi-file-pdf mr-2"></i>
              📋 Generar Reporte
            </button>

            <button
              @click="showAdminCreateForm = true"
              class="bg-gradient-to-r from-red-400 to-orange-400 
                     hover:from-red-500 hover:to-orange-500 
                     text-white font-bold py-3 px-6 rounded-lg 
                     transition-all duration-300 transform hover:scale-105 w-full"
            >
              <i class="pi pi-user-plus mr-2"></i>
              👥 Crear Consumo para Usuario
            </button>
          </div>
        </div>
      </div>
    </div>

    <!-- Statistics Summary -->
    <div v-if="allConsumption && allConsumption.length > 0" class="glass-card p-6">
      <div class="flex items-center mb-6">
        <i class="pi pi-chart-bar text-accent-400 text-xl mr-2"></i>
        <h3 class="text-xl font-bold text-white">📈 Estadísticas Generales</h3>
      </div>

      <div class="grid grid-cols-2 md:grid-cols-4 gap-4">
        <div class="text-center bg-white/5 p-4 rounded-lg">
          <div class="bg-gradient-to-r from-blue-400 to-blue-500 text-white p-3 rounded-full mx-auto w-12 h-12 flex items-center justify-center mb-2">
            <i class="pi pi-users text-lg"></i>
          </div>
          <p class="text-blue-400 font-bold text-lg">{{ uniqueUsersCount }}</p>
          <p class="text-gray-300 text-xs">Usuarios Únicos</p>
        </div>
        
        <div class="text-center bg-white/5 p-4 rounded-lg">
          <div class="bg-gradient-to-r from-green-400 to-green-500 text-white p-3 rounded-full mx-auto w-12 h-12 flex items-center justify-center mb-2">
            <i class="pi pi-shopping-cart text-lg"></i>
          </div>
          <p class="text-green-400 font-bold text-lg">{{ allConsumption.length }}</p>
          <p class="text-gray-300 text-xs">Total Consumos</p>
        </div>
        
        <div class="text-center bg-white/5 p-4 rounded-lg">
          <div class="bg-gradient-to-r from-purple-400 to-purple-500 text-white p-3 rounded-full mx-auto w-12 h-12 flex items-center justify-center mb-2">
            <i class="pi pi-dollar text-lg"></i>
          </div>
          <p class="text-purple-400 font-bold text-lg">{{ formatCurrency(totalAmount) }}</p>
          <p class="text-gray-300 text-xs">Monto Total</p>
        </div>
        
        <div class="text-center bg-white/5 p-4 rounded-lg">
          <div class="bg-gradient-to-r from-yellow-400 to-yellow-500 text-white p-3 rounded-full mx-auto w-12 h-12 flex items-center justify-center mb-2">
            <i class="pi pi-calendar text-lg"></i>
          </div>
          <p class="text-yellow-400 font-bold text-lg">{{ formatCurrency(averagePerUser) }}</p>
          <p class="text-gray-300 text-xs">Promedio por Usuario</p>
        </div>
      </div>
    </div>

    <!-- Top Users Table -->
    <div v-if="topUsers.length > 0" class="glass-card p-6">
      <div class="flex items-center justify-between mb-6">
        <div class="flex items-center">
          <i class="pi pi-crown text-yellow-400 text-xl mr-2"></i>
          <h3 class="text-xl font-bold text-white">👑 Top Usuarios por Consumo</h3>
        </div>
        <div class="bg-yellow-500/20 px-3 py-1 rounded-full border border-yellow-500/30">
          <span class="text-yellow-300 text-sm font-semibold">Top {{ topUsers.length }}</span>
        </div>
      </div>

      <div class="overflow-hidden rounded-lg">
        <!-- Table Header -->
        <div class="bg-white/5 px-6 py-4 border-b border-white/10">
          <div class="grid grid-cols-5 gap-4 text-sm font-semibold text-gray-300">
            <div>Ranking</div>
            <div>Usuario</div>
            <div class="text-center">Consumos</div>
            <div class="text-right">Total</div>
            <div class="text-center">Acciones</div>
          </div>
        </div>

        <!-- Table Body -->
        <div class="max-h-64 overflow-y-auto">
          <div
            v-for="(user, index) in topUsers"
            :key="user.userId"
            class="px-6 py-4 border-b border-white/5 hover:bg-white/5 transition-colors"
          >
            <div class="grid grid-cols-5 gap-4 items-center">
              <!-- Ranking -->
              <div class="flex items-center">
                <div class="w-8 h-8 rounded-full flex items-center justify-center font-bold text-sm mr-3"
                     :class="getRankingClass(index)">
                  {{ index + 1 }}
                </div>
              </div>

              <!-- User Info -->
              <div>
                <p class="text-white font-medium">{{ user.userFullName }}</p>
                <p class="text-gray-400 text-sm">{{ user.userName }}</p>
              </div>

              <!-- Consumption Count -->
              <div class="text-center">
                <span class="bg-blue-500/20 px-2 py-1 rounded text-blue-300 text-sm font-semibold">
                  {{ user.consumptionCount }}
                </span>
              </div>

              <!-- Total Amount -->
              <div class="text-right">
                <span class="text-green-400 font-bold">{{ formatCurrency(user.totalAmount) }}</span>
              </div>

              <!-- Actions -->
              <div class="text-center">
                <button
                  @click="viewUserDetails(user)"
                  class="glass-button py-1 px-3 text-white hover:bg-white/20 transition-all text-sm"
                >
                  <i class="pi pi-eye mr-1"></i>
                  Ver
                </button>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- Loading State -->
    <div v-if="loading" class="text-center py-8">
      <div class="glass-card p-6">
        <i class="pi pi-spinner pi-spin text-primary-400 text-3xl mb-3"></i>
        <p class="text-white">Cargando datos administrativos...</p>
      </div>
    </div>

    <!-- Empty State -->
    <div v-if="!allConsumption || allConsumption.length === 0" class="text-center py-12">
      <div class="glass-card p-8">
        <div class="mb-4">
          <i class="pi pi-users text-gray-400 text-4xl"></i>
        </div>
        <h3 class="text-white font-bold text-lg mb-2">Panel Administrativo</h3>
        <p class="text-gray-300">Utiliza las herramientas para cargar y analizar datos de consumo</p>
      </div>
    </div>

    <!-- Admin Create Form Modal -->
    <AdminConsumptionForm
      v-if="showAdminCreateForm"
      :createConsumption="createConsumptionForUser"
      :loading="loading"
      @close="handleAdminFormClose"
      @created="handleAdminFormCreated"
    />
  </div>
</template>

<script setup>
import { ref, computed } from 'vue'
import { useToast } from 'primevue/usetoast'
import AdminConsumptionForm from './AdminConsumptionForm.vue'


const emit = defineEmits(['fetchUser', 'fetchAll'])
const toast = useToast()

// Props to receive the createConsumptionForUser method
const props = defineProps({
  allConsumption: {
    type: Array,
    default: () => []
  },
  loading: {
    type: Boolean,
    default: false
  },
  createConsumptionForUser: {
    type: Function,
    required: true
  }
})

// Local state
const userSearchTerm = ref('')
const showUserDropdown = ref(false)
const selectedUser = ref(null)
const showAdminCreateForm = ref(false)

// Mock users data
const availableUsers = ref([
  { id: '001', userName: 'admin', fullName: 'Administrador Sistema', role: 'Admin' },
  { id: '002', userName: 'juan.perez', fullName: 'Juan Pérez García', role: 'Usuario' },
  { id: '003', userName: 'maria.lopez', fullName: 'María López Torres', role: 'Usuario' },
  { id: '004', userName: 'carlos.ruiz', fullName: 'Carlos Ruiz Mendoza', role: 'Director' },
  { id: '005', userName: 'ana.torres', fullName: 'Ana Torres Silva', role: 'Usuario' }
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

const uniqueUsersCount = computed(() => {
  if (!props.allConsumption || props.allConsumption.length === 0) return 0
  const uniqueUserIds = new Set(props.allConsumption.map(c => c.userId))
  return uniqueUserIds.size
})

const totalAmount = computed(() => {
  return props.allConsumption.reduce((total, consumption) => total + (consumption.total || 0), 0)
})

const averagePerUser = computed(() => {
  if (uniqueUsersCount.value === 0) return 0
  return totalAmount.value / uniqueUsersCount.value
})

const topUsers = computed(() => {
  if (!props.allConsumption || props.allConsumption.length === 0) return []
  
  // Group by user
  const userGroups = props.allConsumption.reduce((groups, consumption) => {
    const userId = consumption.userId
    if (!groups[userId]) {
      groups[userId] = {
        userId,
        userName: consumption.userName,
        userFullName: consumption.userFullName,
        consumptions: [],
        totalAmount: 0
      }
    }
    groups[userId].consumptions.push(consumption)
    groups[userId].totalAmount += consumption.total || 0
    return groups
  }, {})
  
  // Convert to array and sort by total amount
  return Object.values(userGroups)
    .map(group => ({
      ...group,
      consumptionCount: group.consumptions.length
    }))
    .sort((a, b) => b.totalAmount - a.totalAmount)
    .slice(0, 10) // Top 10
})

// Methods
const searchUsers = () => {
  showUserDropdown.value = true
}

const selectUser = (user) => {
  selectedUser.value = user
  userSearchTerm.value = user.fullName
  showUserDropdown.value = false
}

const clearUser = () => {
  selectedUser.value = null
  userSearchTerm.value = ''
}

const fetchUserConsumption = () => {
  if (selectedUser.value) {
    emit('fetchUser', selectedUser.value.id)
    toast.add({
      severity: 'info',
      summary: 'Cargando',
      detail: `Cargando consumo de ${selectedUser.value.fullName}`,
      life: 3000
    })
  }
}

const fetchUserSummary = () => {
  if (selectedUser.value) {
    // This would trigger fetching user summary
    toast.add({
      severity: 'info',
      summary: 'Cargando',
      detail: `Cargando resumen de ${selectedUser.value.fullName}`,
      life: 3000
    })
  }
}

const fetchAllConsumption = () => {
  emit('fetchAll')
}

const exportAllData = () => {
  if (!props.allConsumption || props.allConsumption.length === 0) return
  
  const csvData = props.allConsumption.map(item => ({
    Usuario: item.userFullName,
    'Nombre Usuario': item.userName,
    Artículo: item.articuloNombre,
    Cantidad: item.cantidad,
    'Precio Unitario': item.precioUnitario,
    Total: item.total,
    Fecha: new Date(item.fechaConsumo).toLocaleDateString('es-PE'),
    Tipo: item.tipoConsumo,
    Habitación: item.habitacionNumero || 'N/A'
  }))
  
  const headers = Object.keys(csvData[0]).join(',')
  const rows = csvData.map(row => Object.values(row).join(','))
  const csvContent = [headers, ...rows].join('\n')
  
  const blob = new Blob([csvContent], { type: 'text/csv;charset=utf-8;' })
  const link = document.createElement('a')
  
  if (link.download !== undefined) {
    const url = URL.createObjectURL(blob)
    link.setAttribute('href', url)
    link.setAttribute('download', `todos_los_consumos_${new Date().toISOString().split('T')[0]}.csv`)
    link.style.visibility = 'hidden'
    document.body.appendChild(link)
    link.click()
    document.body.removeChild(link)
  }
  
  toast.add({
    severity: 'success',
    summary: 'Exportado',
    detail: 'Datos exportados exitosamente',
    life: 3000
  })
}

const generateReport = () => {
  toast.add({
    severity: 'info',
    summary: 'Generando',
    detail: 'Funcionalidad de reporte en desarrollo',
    life: 3000
  })
}

const viewUserDetails = (user) => {
  toast.add({
    severity: 'info',
    summary: 'Usuario',
    detail: `Mostrando detalles de ${user.userFullName}`,
    life: 3000
  })
}

const formatCurrency = (amount) => {
  if (amount == null || amount == undefined) return 'S/ 0.00'
  return new Intl.NumberFormat('es-PE', {
    style: 'currency',
    currency: 'PEN'
  }).format(amount)
}

const getRankingClass = (index) => {
  switch (index) {
    case 0:
      return 'bg-gradient-to-r from-yellow-400 to-yellow-500 text-white'
    case 1:
      return 'bg-gradient-to-r from-gray-400 to-gray-500 text-white'
    case 2:
      return 'bg-gradient-to-r from-amber-600 to-amber-700 text-white'
    default:
      return 'bg-gradient-to-r from-blue-400 to-blue-500 text-white'
  }
}

// Admin form handlers
const handleAdminFormClose = () => {
  showAdminCreateForm.value = false
}

const handleAdminFormCreated = (result) => {
  showAdminCreateForm.value = false
  toast.add({
    severity: 'success',
    summary: 'Consumo Creado',
    detail: 'El consumo ha sido registrado exitosamente para el usuario',
    life: 5000
  })
  // Refresh data if needed
  emit('fetchAll')
}
</script>

<style scoped>
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