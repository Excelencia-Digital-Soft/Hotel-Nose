import { ref, computed } from 'vue'
import { useToast } from 'primevue/usetoast'
import { useConfirm } from 'primevue/useconfirm'
import { UserConsumptionService } from '../services/userConsumptionService'

export function useUserConsumption() {
  const toast = useToast()
  const confirm = useConfirm()

  // Reactive state
  const myConsumption = ref([])
  const mySummary = ref(null)
  const myConsumptionByService = ref([])
  const userConsumption = ref([])
  const userSummary = ref(null)
  const allConsumption = ref([])
  const loading = ref(false)
  const dateFilter = ref({
    startDate: null,
    endDate: null
  })

  // Computed properties
  const formattedMyConsumption = computed(() => {
    return myConsumption.value.map(consumption => 
      UserConsumptionService.formatConsumptionData(consumption)
    )
  })

  const totalMyConsumption = computed(() => {
    return myConsumption.value.reduce((total, item) => total + item.total, 0)
  })

  const totalMyConsumptionFormatted = computed(() => {
    return new Intl.NumberFormat('es-PE', {
      style: 'currency',
      currency: 'PEN'
    }).format(totalMyConsumption.value)
  })

  // Fetch my personal consumption
  const fetchMyConsumption = async (startDate = null, endDate = null) => {
    loading.value = true
    try {
      const response = await UserConsumptionService.getMyConsumption(startDate, endDate)
      if (response.isSuccess) {
        myConsumption.value = response.data
      } else {
        throw new Error('Failed to fetch consumption data')
      }
    } catch (error) {
      console.error('Error fetching my consumption:', error)
      toast.add({
        severity: 'error',
        summary: 'Error',
        detail: 'Error al cargar mi consumo personal',
        life: 5000
      })
    } finally {
      loading.value = false
    }
  }

  // Fetch my consumption summary
  const fetchMySummary = async (startDate = null, endDate = null) => {
    loading.value = true
    try {
      const response = await UserConsumptionService.getMySummary(startDate, endDate)
      if (response.isSuccess) {
        mySummary.value = response.data
      } else {
        throw new Error('Failed to fetch summary data')
      }
    } catch (error) {
      console.error('Error fetching my summary:', error)
      toast.add({
        severity: 'error',
        summary: 'Error',
        detail: 'Error al cargar resumen de consumo',
        life: 5000
      })
    } finally {
      loading.value = false
    }
  }

  // Fetch my consumption by service type
  const fetchMyConsumptionByService = async (startDate = null, endDate = null) => {
    loading.value = true
    try {
      const response = await UserConsumptionService.getMyConsumptionByService(startDate, endDate)
      if (response.isSuccess) {
        myConsumptionByService.value = response.data
      } else {
        throw new Error('Failed to fetch consumption by service data')
      }
    } catch (error) {
      console.error('Error fetching my consumption by service:', error)
      toast.add({
        severity: 'error',
        summary: 'Error',
        detail: 'Error al cargar consumo por tipo de servicio',
        life: 5000
      })
    } finally {
      loading.value = false
    }
  }

  // Fetch specific user consumption (Admin/Director only)
  const fetchUserConsumption = async (userId, startDate = null, endDate = null) => {
    loading.value = true
    try {
      const response = await UserConsumptionService.getUserConsumption(userId, startDate, endDate)
      if (response.isSuccess) {
        userConsumption.value = response.data
      } else {
        throw new Error('Failed to fetch user consumption data')
      }
    } catch (error) {
      console.error('Error fetching user consumption:', error)
      toast.add({
        severity: 'error',
        summary: 'Error',
        detail: 'Error al cargar consumo del usuario',
        life: 5000
      })
    } finally {
      loading.value = false
    }
  }

  // Fetch specific user summary (Admin/Director only)
  const fetchUserSummary = async (userId, startDate = null, endDate = null) => {
    loading.value = true
    try {
      const response = await UserConsumptionService.getUserSummary(userId, startDate, endDate)
      if (response.isSuccess) {
        userSummary.value = response.data
      } else {
        throw new Error('Failed to fetch user summary data')
      }
    } catch (error) {
      console.error('Error fetching user summary:', error)
      toast.add({
        severity: 'error',
        summary: 'Error',
        detail: 'Error al cargar resumen del usuario',
        life: 5000
      })
    } finally {
      loading.value = false
    }
  }

  // Fetch all users consumption (Admin/Director only)
  const fetchAllConsumption = async (startDate = null, endDate = null) => {
    loading.value = true
    try {
      const response = await UserConsumptionService.getAllConsumption(startDate, endDate)
      if (response.isSuccess) {
        allConsumption.value = response.data
      } else {
        throw new Error('Failed to fetch all consumption data')
      }
    } catch (error) {
      console.error('Error fetching all consumption:', error)
      toast.add({
        severity: 'error',
        summary: 'Error',
        detail: 'Error al cargar todos los consumos',
        life: 5000
      })
    } finally {
      loading.value = false
    }
  }

  // Create new consumption
  const createConsumption = async (consumptionData) => {
    loading.value = true
    try {
      const response = await UserConsumptionService.createConsumption(consumptionData)
      if (response.isSuccess) {
        toast.add({
          severity: 'success',
          summary: 'Éxito',
          detail: 'Consumo registrado exitosamente',
          life: 5000
        })
        // Refresh data
        await fetchMyConsumption(dateFilter.value.startDate, dateFilter.value.endDate)
        return response.data
      } else {
        throw new Error('Failed to create consumption')
      }
    } catch (error) {
      console.error('Error creating consumption:', error)
      toast.add({
        severity: 'error',
        summary: 'Error',
        detail: 'Error al registrar consumo',
        life: 5000
      })
      throw error
    } finally {
      loading.value = false
    }
  }

  // Create new consumption for another user (Admin only)
  const createConsumptionForUser = async (consumptionData) => {
    loading.value = true
    try {
      const response = await UserConsumptionService.createConsumptionForUser(consumptionData)
      if (response.isSuccess) {
        toast.add({
          severity: 'success',
          summary: 'Éxito',
          detail: `Consumo registrado exitosamente para el usuario`,
          life: 5000
        })
        // Refresh all consumption data if loaded
        if (allConsumption.value.length > 0) {
          await fetchAllConsumption(dateFilter.value.startDate, dateFilter.value.endDate)
        }
        return response.data
      } else {
        throw new Error('Failed to create consumption for user')
      }
    } catch (error) {
      console.error('Error creating consumption for user:', error)
      toast.add({
        severity: 'error',
        summary: 'Error',
        detail: 'Error al registrar consumo para el usuario',
        life: 5000
      })
      throw error
    } finally {
      loading.value = false
    }
  }

  // Set date filter
  const setDateFilter = (startDate, endDate) => {
    dateFilter.value = { startDate, endDate }
  }

  // Clear all data
  const clearData = () => {
    myConsumption.value = []
    mySummary.value = null
    myConsumptionByService.value = []
    userConsumption.value = []
    userSummary.value = null
    allConsumption.value = []
  }

  // Get consumption by date range
  const getConsumptionByDateRange = async (startDate, endDate) => {
    setDateFilter(startDate, endDate)
    await Promise.all([
      fetchMyConsumption(startDate, endDate),
      fetchMySummary(startDate, endDate),
      fetchMyConsumptionByService(startDate, endDate)
    ])
  }

  // Export to CSV utility
  const exportToCSV = (data, filename = 'consumption_export.csv') => {
    if (!data || data.length === 0) {
      toast.add({
        severity: 'warn',
        summary: 'Advertencia',
        detail: 'No hay datos para exportar',
        life: 3000
      })
      return
    }

    const headers = Object.keys(data[0]).join(',')
    const rows = data.map(row => Object.values(row).join(','))
    const csvContent = [headers, ...rows].join('\n')
    
    const blob = new Blob([csvContent], { type: 'text/csv;charset=utf-8;' })
    const link = document.createElement('a')
    
    if (link.download !== undefined) {
      const url = URL.createObjectURL(blob)
      link.setAttribute('href', url)
      link.setAttribute('download', filename)
      link.style.visibility = 'hidden'
      document.body.appendChild(link)
      link.click()
      document.body.removeChild(link)
    }
  }

  // Health check
  const checkHealth = async () => {
    try {
      const response = await UserConsumptionService.healthCheck()
      return response
    } catch (error) {
      console.error('Health check failed:', error)
      return null
    }
  }

  return {
    // State
    myConsumption,
    mySummary,
    myConsumptionByService,
    userConsumption,
    userSummary,
    allConsumption,
    loading,
    dateFilter,

    // Computed
    formattedMyConsumption,
    totalMyConsumption,
    totalMyConsumptionFormatted,

    // Methods
    fetchMyConsumption,
    fetchMySummary,
    fetchMyConsumptionByService,
    fetchUserConsumption,
    fetchUserSummary,
    fetchAllConsumption,
    createConsumption,
    createConsumptionForUser,
    setDateFilter,
    clearData,
    getConsumptionByDateRange,
    exportToCSV,
    checkHealth
  }
}