import { ref, computed, onMounted, reactive } from 'vue'
import { configurationService as ConfigurationService } from '../services/configurationService'
import type {
  ConfiguracionDto,
  ConfiguracionCreateDto,
  ConfiguracionUpdateDto,
  TimerUpdateIntervalDto,
  TimerConfigFormData,
  ConfigurationCategory,
  ConfigurationState
} from '../types'

/**
 * Configuration management composable
 * Handles configuration state and operations
 */
export function useConfiguration() {
  // State
  const state = reactive<ConfigurationState>({
    configurations: [],
    loading: false,
    error: null,
    selectedCategory: 'ALL'
  })

  const saving = ref(false)
  const deleting = ref(false)

  // Computed
  const filteredConfigurations = computed(() => {
    if (state.selectedCategory === 'ALL') {
      return state.configurations
    }
    return state.configurations.filter(config => config.categoria === state.selectedCategory)
  })

  const configurationsByCategory = computed(() => {
    const categories: Record<string, ConfiguracionDto[]> = {}
    state.configurations.forEach(config => {
      if (!categories[config.categoria]) {
        categories[config.categoria] = []
      }
      categories[config.categoria].push(config)
    })
    return categories
  })

  const availableCategories = computed(() => {
    const categories = new Set(state.configurations.map(config => config.categoria))
    return Array.from(categories).sort()
  })

  // Timer Configuration State
  const timerConfig = ref<TimerConfigFormData>({
    intervalMinutos: 10,
    descripcion: 'Timer update interval configuration'
  })

  const timerLoading = ref(false)

  // Methods
  const loadConfigurations = async (categoria?: string) => {
    state.loading = true
    state.error = null

    try {
      const response = await ConfigurationService.getAllConfigurations(categoria)
      if (response.isSuccess && response.data) {
        state.configurations = Array.isArray(response.data) ? response.data : []
      } else {
        state.error = response.message || 'Error loading configurations'
        state.configurations = []
      }
    } catch (error) {
      console.error('Error loading configurations:', error)
      state.error = 'Error de conexión al cargar configuraciones'
      state.configurations = []
    } finally {
      state.loading = false
    }
  }

  const loadConfiguration = async (clave: string): Promise<ConfiguracionDto | null> => {
    try {
      const response = await ConfigurationService.getConfiguration(clave)
      if (response.isSuccess && response.data) {
        return response.data
      }
      return null
    } catch (error) {
      console.error(`Error loading configuration ${clave}:`, error)
      return null
    }
  }

  const createConfiguration = async (data: ConfiguracionCreateDto): Promise<boolean> => {
    saving.value = true
    state.error = null

    try {
      const response = await ConfigurationService.createConfiguration(data)
      if (response.isSuccess && response.data) {
        // Add to local state
        state.configurations.push(response.data)
        return true
      } else {
        state.error = response.message || 'Error creating configuration'
        return false
      }
    } catch (error) {
      console.error('Error creating configuration:', error)
      state.error = 'Error de conexión al crear configuración'
      return false
    } finally {
      saving.value = false
    }
  }

  const updateConfiguration = async (clave: string, data: ConfiguracionUpdateDto): Promise<boolean> => {
    saving.value = true
    state.error = null

    try {
      const response = await ConfigurationService.updateConfiguration(clave, data)
      if (response.isSuccess && response.data) {
        // Update local state
        const index = state.configurations.findIndex(config => config.clave === clave)
        if (index !== -1) {
          state.configurations[index] = response.data
        }
        return true
      } else {
        state.error = response.message || 'Error updating configuration'
        return false
      }
    } catch (error) {
      console.error('Error updating configuration:', error)
      state.error = 'Error de conexión al actualizar configuración'
      return false
    } finally {
      saving.value = false
    }
  }

  const deleteConfiguration = async (clave: string): Promise<boolean> => {
    deleting.value = true
    state.error = null

    try {
      const response = await ConfigurationService.deleteConfiguration(clave)
      if (response.isSuccess) {
        // Remove from local state
        state.configurations = state.configurations.filter(config => config.clave !== clave)
        return true
      } else {
        state.error = response.message || 'Error deleting configuration'
        return false
      }
    } catch (error) {
      console.error('Error deleting configuration:', error)
      state.error = 'Error de conexión al eliminar configuración'
      return false
    } finally {
      deleting.value = false
    }
  }

  // Timer-specific methods
  const loadTimerConfig = async () => {
    timerLoading.value = true
    try {
      const response = await ConfigurationService.getTimerUpdateInterval()
      if (response.isSuccess && response.data) {
        timerConfig.value = {
          intervalMinutos: response.data.intervalMinutos,
          descripcion: response.data.descripcion || 'Timer update interval configuration'
        }
      }
    } catch (error) {
      console.error('Error loading timer config:', error)
      state.error = 'Error al cargar configuración del timer'
    } finally {
      timerLoading.value = false
    }
  }

  const updateTimerConfig = async (data: TimerUpdateIntervalDto): Promise<boolean> => {
    timerLoading.value = true
    state.error = null

    try {
      const response = await ConfigurationService.updateTimerUpdateInterval(data)
      if (response.isSuccess && response.data) {
        timerConfig.value = {
          intervalMinutos: response.data.intervalMinutos,
          descripcion: response.data.descripcion || data.descripcion || 'Timer update interval configuration'
        }
        return true
      } else {
        state.error = response.message || 'Error updating timer configuration'
        return false
      }
    } catch (error) {
      console.error('Error updating timer config:', error)
      state.error = 'Error de conexión al actualizar configuración del timer'
      return false
    } finally {
      timerLoading.value = false
    }
  }

  // Category filter
  const setCategory = (category: ConfigurationCategory | 'ALL') => {
    state.selectedCategory = category
  }

  // Validation helpers
  const validateConfiguration = (data: Partial<ConfiguracionCreateDto>): string[] => {
    const errors: string[] = []

    if (!data.clave) {
      errors.push('La clave es requerida')
    } else if (!ConfigurationService.validateConfigurationKey(data.clave)) {
      errors.push('La clave debe contener solo letras mayúsculas, números y guiones bajos')
    }

    if (!data.valor) {
      errors.push('El valor es requerido')
    }

    if (!data.categoria) {
      errors.push('La categoría es requerida')
    }

    return errors
  }

  const validateTimerInterval = (intervalMinutos: number): string[] => {
    const errors: string[] = []

    if (!ConfigurationService.validateTimerInterval(intervalMinutos)) {
      errors.push('El intervalo debe estar entre 1 y 1440 minutos (24 horas)')
    }

    return errors
  }

  // Utility methods
  const refreshConfigurations = () => {
    loadConfigurations()
  }

  const clearError = () => {
    state.error = null
  }

  // Auto-load configurations on mount
  onMounted(() => {
    loadConfigurations()
    loadTimerConfig()
  })

  return {
    // State
    configurations: computed(() => state.configurations),
    loading: computed(() => state.loading),
    error: computed(() => state.error),
    saving,
    deleting,
    selectedCategory: computed(() => state.selectedCategory),

    // Timer state
    timerConfig,
    timerLoading,

    // Computed
    filteredConfigurations,
    configurationsByCategory,
    availableCategories,

    // Methods
    loadConfigurations,
    loadConfiguration,
    createConfiguration,
    updateConfiguration,
    deleteConfiguration,
    refreshConfigurations,
    clearError,
    setCategory,

    // Timer methods
    loadTimerConfig,
    updateTimerConfig,

    // Validation
    validateConfiguration,
    validateTimerInterval
  }
}