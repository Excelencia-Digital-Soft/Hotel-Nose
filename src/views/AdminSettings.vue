<template>
  <div class="admin-settings-container">
    <!-- Header -->
    <div class="glass-card p-6 mb-6">
      <h1 class="text-3xl font-bold text-white mb-2 flex items-center gap-3">
        <span class="material-symbols-outlined text-4xl">settings</span>
        Configuración del Sistema
      </h1>
      <p class="text-gray-300">
        Gestiona las configuraciones generales de la plataforma
      </p>
    </div>

    <!-- Error Display -->
    <div v-if="error" class="glass-card p-4 mb-6 border-l-4 border-red-500">
      <div class="flex items-center gap-3">
        <span class="material-symbols-outlined text-red-400">error</span>
        <div>
          <p class="text-red-300 font-medium">Error</p>
          <p class="text-red-200 text-sm">{{ error }}</p>
        </div>
        <button @click="clearError" class="ml-auto text-red-300 hover:text-red-200">
          <span class="material-symbols-outlined">close</span>
        </button>
      </div>
    </div>

    <!-- Timer Configuration Section -->
    <div class="glass-card p-6 mb-6">
      <h2 class="text-xl font-semibold text-white mb-4 flex items-center gap-3">
        <span class="material-symbols-outlined text-2xl text-primary-400">schedule</span>
        Configuración de Timer
      </h2>
      
      <div class="grid grid-cols-1 md:grid-cols-2 gap-6">
        <div>
          <label class="block text-sm font-medium text-gray-300 mb-2">
            Intervalo de Actualización (minutos)
          </label>
          <input
            v-model.number="timerForm.intervalMinutos"
            type="number"
            min="1"
            max="1440"
            class="glass-input w-full"
            :disabled="timerLoading"
          />
          <p class="text-xs text-gray-400 mt-1">
            Entre 1 y 1440 minutos (24 horas)
          </p>
        </div>

        <div>
          <label class="block text-sm font-medium text-gray-300 mb-2">
            Descripción
          </label>
          <input
            v-model="timerForm.descripcion"
            type="text"
            class="glass-input w-full"
            :disabled="timerLoading"
            placeholder="Descripción de la configuración"
          />
        </div>
      </div>

      <div class="flex justify-end mt-6">
        <button
          @click="saveTimerConfig"
          :disabled="timerLoading || !isTimerFormValid"
          class="glass-button px-6 py-2 bg-primary-500 hover:bg-primary-600 text-white rounded-lg disabled:opacity-50 disabled:cursor-not-allowed flex items-center gap-2"
        >
          <span v-if="timerLoading" class="material-symbols-outlined animate-spin">refresh</span>
          <span v-else class="material-symbols-outlined">save</span>
          {{ timerLoading ? 'Guardando...' : 'Guardar Configuración' }}
        </button>
      </div>
    </div>

    <!-- Configuration Categories -->
    <div class="glass-card p-6 mb-6">
      <h2 class="text-xl font-semibold text-white mb-4 flex items-center gap-3">
        <span class="material-symbols-outlined text-2xl text-secondary-400">tune</span>
        Filtrar Configuraciones
      </h2>
      
      <div class="flex flex-wrap gap-2">
        <button
          @click="setCategory('ALL')"
          :class="[
            'px-4 py-2 rounded-lg text-sm font-medium transition-all',
            selectedCategory === 'ALL' 
              ? 'bg-white/20 text-white' 
              : 'bg-white/5 text-gray-300 hover:bg-white/10'
          ]"
        >
          Todas ({{ configurations.length }})
        </button>
        <button
          v-for="category in availableCategories"
          :key="category"
          @click="setCategory(category)"
          :class="[
            'px-4 py-2 rounded-lg text-sm font-medium transition-all',
            selectedCategory === category 
              ? 'bg-white/20 text-white' 
              : 'bg-white/5 text-gray-300 hover:bg-white/10'
          ]"
        >
          {{ category }} ({{ configurationsByCategory[category]?.length || 0 }})
        </button>
      </div>
    </div>

    <!-- Configurations List -->
    <div class="glass-card p-6">
      <div class="flex justify-between items-center mb-4">
        <h2 class="text-xl font-semibold text-white flex items-center gap-3">
          <span class="material-symbols-outlined text-2xl text-accent-400">list</span>
          Configuraciones
        </h2>
        <button
          @click="showCreateModal = true"
          class="glass-button px-4 py-2 bg-accent-500 hover:bg-accent-600 text-white rounded-lg flex items-center gap-2"
        >
          <span class="material-symbols-outlined">add</span>
          Nueva Configuración
        </button>
      </div>

      <!-- Loading State -->
      <div v-if="loading" class="text-center py-8">
        <div class="inline-flex items-center gap-2 text-gray-300">
          <span class="material-symbols-outlined animate-spin">refresh</span>
          Cargando configuraciones...
        </div>
      </div>

      <!-- Empty State -->
      <div v-else-if="filteredConfigurations.length === 0" class="text-center py-8">
        <span class="material-symbols-outlined text-4xl text-gray-500 mb-2">settings_suggest</span>
        <p class="text-gray-400">No hay configuraciones disponibles</p>
      </div>

      <!-- Configurations Table -->
      <div v-else class="overflow-hidden rounded-lg">
        <div class="overflow-x-auto">
          <table class="w-full">
            <thead class="bg-white/10">
              <tr>
                <th class="px-4 py-3 text-left text-xs font-medium text-gray-300 uppercase">Clave</th>
                <th class="px-4 py-3 text-left text-xs font-medium text-gray-300 uppercase">Valor</th>
                <th class="px-4 py-3 text-left text-xs font-medium text-gray-300 uppercase">Categoría</th>
                <th class="px-4 py-3 text-left text-xs font-medium text-gray-300 uppercase">Estado</th>
                <th class="px-4 py-3 text-left text-xs font-medium text-gray-300 uppercase">Modificado</th>
                <th class="px-4 py-3 text-center text-xs font-medium text-gray-300 uppercase">Acciones</th>
              </tr>
            </thead>
            <tbody class="divide-y divide-white/10">
              <tr 
                v-for="config in filteredConfigurations" 
                :key="config.configuracionId"
                class="hover:bg-white/5 transition-colors"
              >
                <td class="px-4 py-3">
                  <div class="text-white font-medium">{{ config.clave }}</div>
                  <div class="text-xs text-gray-400">{{ config.descripcion }}</div>
                </td>
                <td class="px-4 py-3">
                  <div class="text-gray-300 font-mono text-sm break-all max-w-xs">
                    {{ config.valor }}
                  </div>
                </td>
                <td class="px-4 py-3">
                  <span class="px-2 py-1 rounded-full text-xs font-medium bg-primary-500/20 text-primary-300">
                    {{ config.categoria }}
                  </span>
                </td>
                <td class="px-4 py-3">
                  <span :class="[
                    'px-2 py-1 rounded-full text-xs font-medium',
                    config.activo 
                      ? 'bg-green-500/20 text-green-300' 
                      : 'bg-red-500/20 text-red-300'
                  ]">
                    {{ config.activo ? 'Activo' : 'Inactivo' }}
                  </span>
                </td>
                <td class="px-4 py-3 text-gray-400 text-sm">
                  {{ formatDate(config.fechaModificacion) }}
                </td>
                <td class="px-4 py-3">
                  <div class="flex justify-center gap-2">
                    <button
                      @click="editConfiguration(config)"
                      class="text-primary-400 hover:text-primary-300 p-1 rounded"
                      title="Editar"
                    >
                      <span class="material-symbols-outlined text-sm">edit</span>
                    </button>
                    <button
                      @click="confirmDelete(config)"
                      class="text-red-400 hover:text-red-300 p-1 rounded"
                      title="Eliminar"
                    >
                      <span class="material-symbols-outlined text-sm">delete</span>
                    </button>
                  </div>
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>
    </div>

    <!-- Create/Edit Modal -->
    <div v-if="showCreateModal || showEditModal" class="fixed inset-0 bg-black/60 backdrop-blur-xl z-50 flex items-center justify-center p-4">
      <div class="glass-container max-w-md w-full p-6">
        <h3 class="text-lg font-semibold text-white mb-4">
          {{ showEditModal ? 'Editar Configuración' : 'Nueva Configuración' }}
        </h3>

        <form @submit.prevent="saveConfiguration" class="space-y-4">
          <div>
            <label class="block text-sm font-medium text-gray-300 mb-2">Clave</label>
            <input
              v-model="configForm.clave"
              type="text"
              class="glass-input w-full"
              :disabled="showEditModal || saving"
              required
            />
          </div>

          <div>
            <label class="block text-sm font-medium text-gray-300 mb-2">Valor</label>
            <textarea
              v-model="configForm.valor"
              class="glass-input w-full h-20"
              :disabled="saving"
              required
            ></textarea>
          </div>

          <div>
            <label class="block text-sm font-medium text-gray-300 mb-2">Descripción</label>
            <input
              v-model="configForm.descripcion"
              type="text"
              class="glass-input w-full"
              :disabled="saving"
            />
          </div>

          <div>
            <label class="block text-sm font-medium text-gray-300 mb-2">Categoría</label>
            <select v-model="configForm.categoria" class="glass-input w-full" :disabled="saving" required>
              <option value="">Seleccionar categoría</option>
              <option value="SYSTEM">Sistema</option>
              <option value="UI">Interfaz</option>
              <option value="SECURITY">Seguridad</option>
              <option value="BUSINESS">Negocio</option>
              <option value="INTEGRATIONS">Integraciones</option>
            </select>
          </div>

          <div v-if="showEditModal" class="flex items-center gap-2">
            <input
              v-model="configForm.activo"
              type="checkbox"
              id="activo"
              class="rounded border-gray-300 text-primary-600 focus:ring-primary-500"
              :disabled="saving"
            />
            <label for="activo" class="text-sm text-gray-300">Activo</label>
          </div>

          <div class="flex justify-end gap-2 pt-4">
            <button
              type="button"
              @click="closeModal"
              class="glass-button px-4 py-2 bg-gray-500 hover:bg-gray-600 text-white rounded-lg"
              :disabled="saving"
            >
              Cancelar
            </button>
            <button
              type="submit"
              :disabled="saving || !isConfigFormValid"
              class="glass-button px-4 py-2 bg-accent-500 hover:bg-accent-600 text-white rounded-lg disabled:opacity-50 disabled:cursor-not-allowed flex items-center gap-2"
            >
              <span v-if="saving" class="material-symbols-outlined animate-spin">refresh</span>
              <span v-else class="material-symbols-outlined">save</span>
              {{ saving ? 'Guardando...' : 'Guardar' }}
            </button>
          </div>
        </form>
      </div>
    </div>

    <!-- Delete Confirmation Modal -->
    <div v-if="showDeleteModal" class="fixed inset-0 bg-black/60 backdrop-blur-xl z-50 flex items-center justify-center p-4">
      <div class="glass-container max-w-sm w-full p-6">
        <h3 class="text-lg font-semibold text-white mb-4">Confirmar Eliminación</h3>
        <p class="text-gray-300 mb-6">
          ¿Estás seguro de que deseas eliminar la configuración <strong>{{ configToDelete?.clave }}</strong>?
        </p>
        <div class="flex justify-end gap-2">
          <button
            @click="showDeleteModal = false"
            class="glass-button px-4 py-2 bg-gray-500 hover:bg-gray-600 text-white rounded-lg"
            :disabled="deleting"
          >
            Cancelar
          </button>
          <button
            @click="deleteConfigurationConfirmed"
            :disabled="deleting"
            class="glass-button px-4 py-2 bg-red-500 hover:bg-red-600 text-white rounded-lg disabled:opacity-50 disabled:cursor-not-allowed flex items-center gap-2"
          >
            <span v-if="deleting" class="material-symbols-outlined animate-spin">refresh</span>
            <span v-else class="material-symbols-outlined">delete</span>
            {{ deleting ? 'Eliminando...' : 'Eliminar' }}
          </button>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, reactive, computed } from 'vue'
import { useConfiguration } from '../composables/useConfiguration'

// Composables
const {
  configurations,
  loading,
  error,
  saving,
  deleting,
  selectedCategory,
  timerConfig,
  timerLoading,
  filteredConfigurations,
  configurationsByCategory,
  availableCategories,
  createConfiguration,
  updateConfiguration,
  deleteConfiguration,
  setCategory,
  updateTimerConfig,
  validateConfiguration,
  validateTimerInterval,
  clearError
} = useConfiguration()

// Modal states
const showCreateModal = ref(false)
const showEditModal = ref(false)
const showDeleteModal = ref(false)

// Forms
const configForm = reactive({
  clave: '',
  valor: '',
  descripcion: '',
  categoria: '',
  activo: true
})

const timerForm = reactive({
  intervalMinutos: 10,
  descripcion: 'Timer update interval configuration'
})

// Configuration to edit/delete
const configToEdit = ref(null)
const configToDelete = ref(null)

// Computed
const isConfigFormValid = computed(() => {
  return configForm.clave && 
         configForm.valor && 
         configForm.categoria &&
         validateConfiguration(configForm).length === 0
})

const isTimerFormValid = computed(() => {
  return validateTimerInterval(timerForm.intervalMinutos).length === 0
})

// Watch timer config changes
const unwatchTimer = computed(() => timerConfig.value)
unwatchTimer.value && Object.assign(timerForm, timerConfig.value)

// Methods
const editConfiguration = (config) => {
  configToEdit.value = config
  Object.assign(configForm, {
    clave: config.clave,
    valor: config.valor,
    descripcion: config.descripcion || '',
    categoria: config.categoria,
    activo: config.activo
  })
  showEditModal.value = true
}

const confirmDelete = (config) => {
  configToDelete.value = config
  showDeleteModal.value = true
}

const saveConfiguration = async () => {
  if (showEditModal.value && configToEdit.value) {
    // Update existing configuration
    const success = await updateConfiguration(configToEdit.value.clave, {
      valor: configForm.valor,
      descripcion: configForm.descripcion || undefined,
      categoria: configForm.categoria,
      activo: configForm.activo
    })
    if (success) {
      closeModal()
    }
  } else {
    // Create new configuration
    const success = await createConfiguration({
      clave: configForm.clave,
      valor: configForm.valor,
      descripcion: configForm.descripcion,
      categoria: configForm.categoria
    })
    if (success) {
      closeModal()
    }
  }
}

const deleteConfigurationConfirmed = async () => {
  if (configToDelete.value) {
    const success = await deleteConfiguration(configToDelete.value.clave)
    if (success) {
      showDeleteModal.value = false
      configToDelete.value = null
    }
  }
}

const saveTimerConfig = async () => {
  const success = await updateTimerConfig({
    intervalMinutos: timerForm.intervalMinutos,
    descripcion: timerForm.descripcion
  })
  if (success) {
    console.log('Timer configuration updated successfully')
  }
}

const closeModal = () => {
  showCreateModal.value = false
  showEditModal.value = false
  configToEdit.value = null
  
  // Reset form
  Object.assign(configForm, {
    clave: '',
    valor: '',
    descripcion: '',
    categoria: '',
    activo: true
  })
}

const formatDate = (dateString) => {
  if (!dateString) return 'N/A'
  return new Date(dateString).toLocaleDateString('es-ES', {
    year: 'numeric',
    month: 'short',
    day: 'numeric',
    hour: '2-digit',
    minute: '2-digit'
  })
}
</script>

<style scoped>
.admin-settings-container {
  @apply container mx-auto px-4 py-6;
}

.glass-input {
  @apply bg-white/10 backdrop-blur-sm border border-white/30 rounded-lg text-white placeholder-gray-400;
  @apply focus:border-white/50 focus:ring-2 focus:ring-white/20;
  @apply transition-all duration-200;
  background: rgba(255, 255, 255, 0.1);
}

.glass-input:focus {
  background: rgba(255, 255, 255, 0.15);
}

.glass-input:disabled {
  @apply opacity-50 cursor-not-allowed;
}
</style>