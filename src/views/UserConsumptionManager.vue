<template>
  <div
    class="min-h-screen bg-gradient-to-br from-neutral-900 via-neutral-800 to-neutral-900 p-4 lg:p-6"
  >
    <!-- Header Section -->
    <div class="glass-container mb-6 p-6 transform hover:scale-[1.02] transition-all duration-300">
      <div
        class="flex flex-col lg:flex-row lg:items-center lg:justify-between space-y-4 lg:space-y-0"
      >
        <div class="text-center lg:text-left">
          <div class="flex items-center justify-center lg:justify-start mb-2">
            <div class="bg-gradient-to-r from-primary-400 to-accent-400 p-3 rounded-full mr-3">
              <i class="pi pi-user text-white text-2xl"></i>
            </div>
            <h1 class="text-3xl lg:text-4xl font-bold text-white">👤 Gestión de Consumo Usuario</h1>
          </div>
          <p class="text-gray-300 text-lg">
            Control completo de consumos personales y administración 📊
          </p>
        </div>

        <!-- Service Health -->
        <div class="glass-card p-4" v-if="serviceHealth">
          <div class="text-center">
            <div
              class="bg-gradient-to-r from-green-400 to-green-500 text-white p-3 rounded-full mx-auto w-12 h-12 flex items-center justify-center mb-2"
            >
              <i class="pi pi-check-circle text-lg"></i>
            </div>
            <p class="text-white font-bold text-sm">{{ serviceHealth.service }}</p>
            <p class="text-gray-300 text-xs">{{ serviceHealth.status }}</p>
          </div>
        </div>
      </div>
    </div>

    <!-- Date Range Selector -->
    <div class="glass-container mb-6 p-6">
      <div class="flex items-center mb-4">
        <i class="pi pi-filter text-accent-400 text-xl mr-2"></i>
        <h3 class="text-xl font-bold text-white">📅 Filtros de Período</h3>
      </div>

      <div class="grid grid-cols-1 lg:grid-cols-3 gap-6">
        <!-- Predefined Ranges -->
        <div class="lg:col-span-2">
          <label class="block text-white font-semibold mb-3">
            <i class="pi pi-clock text-primary-400 mr-2"></i>
            Períodos predefinidos
          </label>
          <div class="grid grid-cols-2 md:grid-cols-4 gap-3">
            <button
              v-for="(range, key) in predefinedRanges"
              :key="key"
              @click="setPredefinedRange(key)"
              class="glass-button py-3 px-4 text-white hover:bg-white/20 transform hover:scale-105 transition-all"
              :class="{ 'bg-primary-500/50 border-primary-400': selectedRange === key }"
            >
              <div class="text-center">
                <p class="font-semibold text-sm">{{ range.label }}</p>
              </div>
            </button>
          </div>
        </div>

        <!-- Custom Date Range -->
        <div>
          <label class="block text-white font-semibold mb-3">
            <i class="pi pi-calendar text-accent-400 mr-2"></i>
            Período personalizado
          </label>
          <div class="space-y-3">
            <div>
              <label class="block text-gray-300 text-sm mb-1">Fecha inicio</label>
              <input
                v-model="customStartDate"
                type="date"
                class="glass-input w-full px-3 py-2 text-sm"
                @change="updateCustomRange"
              />
            </div>
            <div>
              <label class="block text-gray-300 text-sm mb-1">Fecha fin</label>
              <input
                v-model="customEndDate"
                type="date"
                class="glass-input w-full px-3 py-2 text-sm"
                @change="updateCustomRange"
              />
            </div>
          </div>
        </div>
      </div>

      <!-- Action Buttons -->
      <div class="flex items-center justify-between mt-6">
        <div class="flex items-center space-x-3">
          <button
            @click="refreshAllData"
            :disabled="loading || !dateFilter.startDate"
            class="bg-gradient-to-r from-primary-400 via-secondary-400 to-accent-400 hover:from-primary-500 hover:via-secondary-500 hover:to-accent-500 disabled:opacity-50 disabled:cursor-not-allowed text-white font-bold py-3 px-6 rounded-lg transition-all duration-300 transform hover:scale-105"
          >
            <i :class="loading ? 'pi pi-spinner pi-spin' : 'pi pi-refresh'" class="mr-2"></i>
            {{ loading ? '🔄 Cargando...' : '🚀 Cargar Consumos' }}
          </button>

          <button
            @click="clearAllData"
            class="glass-button py-3 px-4 text-white hover:text-red-300 transform hover:scale-105 transition-all"
          >
            <i class="pi pi-trash mr-2"></i>
            🗑️ Limpiar
          </button>

          <button
            @click="showCreateForm = true"
            class="bg-gradient-to-r from-green-400 to-green-500 hover:from-green-500 hover:to-green-600 text-white font-bold py-3 px-6 rounded-lg transition-all duration-300 transform hover:scale-105"
          >
            <i class="pi pi-plus mr-2"></i>
            ➕ Nuevo Consumo
          </button>
        </div>

        <!-- Loading Indicator -->
        <div v-if="loading" class="flex items-center space-x-2">
          <div class="bg-blue-500/20 px-3 py-1 rounded-full border border-blue-500/30">
            <span class="text-blue-300 text-sm font-semibold flex items-center">
              <i class="pi pi-spinner pi-spin mr-2"></i>
              Procesando...
            </span>
          </div>
        </div>
      </div>
    </div>

    <!-- Dashboard Summary -->
    <div v-if="dashboardStats" class="glass-container mb-6 p-6">
      <div class="flex items-center mb-4">
        <i class="pi pi-chart-bar text-green-400 text-xl mr-2"></i>
        <h3 class="text-xl font-bold text-white">📈 Mi Resumen de Consumo</h3>
      </div>

      <div class="grid grid-cols-2 md:grid-cols-4 gap-4">
        <div
          class="glass-card p-4 text-center transform hover:scale-105 transition-all duration-200"
        >
          <div
            class="bg-gradient-to-r from-blue-400 to-blue-500 text-white p-3 rounded-full mx-auto w-12 h-12 flex items-center justify-center mb-2"
          >
            <i class="pi pi-shopping-cart text-lg"></i>
          </div>
          <p class="text-blue-400 font-bold text-lg">{{ dashboardStats.totalItems || 0 }}</p>
          <p class="text-gray-300 text-xs">Total Items</p>
        </div>

        <div
          class="glass-card p-4 text-center transform hover:scale-105 transition-all duration-200"
        >
          <div
            class="bg-gradient-to-r from-green-400 to-green-500 text-white p-3 rounded-full mx-auto w-12 h-12 flex items-center justify-center mb-2"
          >
            <i class="pi pi-dollar text-lg"></i>
          </div>
          <p class="text-green-400 font-bold text-lg">{{ dashboardStats.totalAmountFormatted }}</p>
          <p class="text-gray-300 text-xs">Monto Total</p>
        </div>

        <div
          class="glass-card p-4 text-center transform hover:scale-105 transition-all duration-200"
        >
          <div
            class="bg-gradient-to-r from-purple-400 to-purple-500 text-white p-3 rounded-full mx-auto w-12 h-12 flex items-center justify-center mb-2"
          >
            <i class="pi pi-box text-lg"></i>
          </div>
          <p class="text-purple-400 font-bold text-lg">{{ dashboardStats.totalQuantity || 0 }}</p>
          <p class="text-gray-300 text-xs">Cantidad Total</p>
        </div>

        <div
          class="glass-card p-4 text-center transform hover:scale-105 transition-all duration-200"
        >
          <div
            class="bg-gradient-to-r from-yellow-400 to-yellow-500 text-white p-3 rounded-full mx-auto w-12 h-12 flex items-center justify-center mb-2"
          >
            <i class="pi pi-calendar text-lg"></i>
          </div>
          <p class="text-yellow-400 font-bold text-lg">{{ formatDateRange() }}</p>
          <p class="text-gray-300 text-xs">Período</p>
        </div>
      </div>
    </div>

    <!-- Main Content Tabs -->
    <div class="glass-container p-6">
      <!-- Tab Navigation -->
      <div class="flex space-x-2 mb-6 overflow-x-auto">
        <button
          v-for="tab in tabs"
          :key="tab.key"
          @click="activeTab = tab.key"
          class="glass-button px-6 py-3 text-white hover:bg-white/20 transform hover:scale-105 transition-all whitespace-nowrap"
          :class="{ 'bg-primary-500/50 border-primary-400': activeTab === tab.key }"
        >
          <i :class="tab.icon" class="mr-2"></i>
          {{ tab.label }}
        </button>
      </div>

      <!-- Tab Content -->
      <div class="min-h-[400px]">
        <!-- Personal Consumption List -->
        <div v-if="activeTab === 'consumption'" class="space-y-4">
          <ConsumptionList
            :data="formattedMyConsumption"
            :loading="loading"
            title="📋 Mi Historial de Consumos"
            @export="exportConsumption"
          />
        </div>

        <!-- Consumption Summary -->
        <div v-if="activeTab === 'summary'" class="space-y-4">
          <ConsumptionSummary
            :summary="mySummary"
            :byService="myConsumptionByService"
            :loading="loading"
          />
        </div>

        <!-- Consumption Charts -->
        <div v-if="activeTab === 'charts'" class="space-y-6">
          <ConsumptionCharts
            :consumptionByService="consumptionByServiceChart"
            :consumptionByDate="consumptionByDateChart"
            :topItems="topConsumedItems"
            :loading="loading"
          />
        </div>

        <!-- Admin Section (if user has permissions) -->
        <div v-if="activeTab === 'admin'" class="space-y-4">
          <AdminConsumptionPanel
            :allConsumption="allConsumption"
            :loading="loading"
            :createConsumptionForUser="createConsumptionForUser"
            @fetchUser="fetchUserData"
            @fetchAll="fetchAllConsumption"
          />
        </div>
      </div>
    </div>

    <!-- Create Consumption Modal -->
    <ConsumptionForm
      v-if="showCreateForm"
      @close="showCreateForm = false"
      @created="handleConsumptionCreated"
    />

    <!-- Empty State -->
    <div v-if="!hasAnyData && !loading" class="text-center py-16">
      <div class="glass-card max-w-md mx-auto p-8">
        <div class="mb-6">
          <div
            class="bg-gradient-to-r from-primary-400 to-accent-400 p-6 rounded-full mx-auto w-24 h-24 flex items-center justify-center mb-4"
          >
            <i class="pi pi-user text-white text-4xl"></i>
          </div>
          <h3 class="text-2xl text-white font-bold mb-2">👤 ¡Bienvenido!</h3>
          <p class="text-gray-300 mb-6">
            Comienza registrando tu primer consumo o consulta tu historial
          </p>
        </div>

        <div class="space-y-3">
          <button
            @click="showCreateForm = true"
            class="bg-gradient-to-r from-green-400 to-green-500 hover:from-green-500 hover:to-green-600 text-white font-bold py-3 px-6 rounded-lg transform hover:scale-105 transition-all duration-300 w-full"
          >
            <i class="pi pi-plus mr-2"></i>
            ➕ Crear Primer Consumo
          </button>

          <button
            @click="refreshAllData"
            :disabled="!dateFilter.startDate"
            class="bg-gradient-to-r from-primary-400 to-accent-400 hover:from-primary-500 hover:to-accent-500 disabled:opacity-50 disabled:cursor-not-allowed text-white font-bold py-3 px-6 rounded-lg transform hover:scale-105 transition-all duration-300 w-full"
          >
            <i class="pi pi-search mr-2"></i>
            🔍 Buscar Consumos
          </button>
        </div>
      </div>
    </div>

    <!-- Toast & Dialogs -->
    <Toast />
    <ConfirmDialog />
  </div>
</template>

<script setup>
  import { ref, computed, onMounted, watch } from 'vue'
  import { useUserConsumption } from '../composables/useUserConsumption'
  import { useUserConsumptionStore } from '../store/userConsumption'
  import ConsumptionList from '../components/userConsumption/ConsumptionList.vue'
  import ConsumptionSummary from '../components/userConsumption/ConsumptionSummary.vue'
  import ConsumptionCharts from '../components/userConsumption/ConsumptionCharts.vue'
  import ConsumptionForm from '../components/userConsumption/ConsumptionForm.vue'
  import AdminConsumptionPanel from '../components/userConsumption/AdminConsumptionPanel.vue'
  import Toast from 'primevue/toast'
  import ConfirmDialog from 'primevue/confirmdialog'

  // Store
  const store = useUserConsumptionStore()

  // Composable
  const {
    // State
    myConsumption,
    mySummary,
    myConsumptionByService,
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
    fetchAllConsumption,
    createConsumption,
    createConsumptionForUser,
    setDateFilter,
    clearData,
    getConsumptionByDateRange,
    exportToCSV,
    checkHealth,
  } = useUserConsumption()

  // Local reactive state
  const activeTab = ref('consumption')
  const showCreateForm = ref(false)
  const selectedRange = ref('thisMonth')
  const customStartDate = ref('')
  const customEndDate = ref('')
  const serviceHealth = ref(null)

  // Predefined date ranges
  const predefinedRanges = {
    today: {
      label: 'Hoy',
      getRange: () => {
        const today = new Date()
        return {
          startDate: today.toISOString().split('T')[0],
          endDate: today.toISOString().split('T')[0],
        }
      },
    },
    thisWeek: {
      label: 'Esta Semana',
      getRange: () => {
        const today = new Date()
        const firstDay = new Date(today.setDate(today.getDate() - today.getDay()))
        const lastDay = new Date(today.setDate(today.getDate() - today.getDay() + 6))
        return {
          startDate: firstDay.toISOString().split('T')[0],
          endDate: lastDay.toISOString().split('T')[0],
        }
      },
    },
    thisMonth: {
      label: 'Este Mes',
      getRange: () => {
        const today = new Date()
        const firstDay = new Date(today.getFullYear(), today.getMonth(), 1)
        const lastDay = new Date(today.getFullYear(), today.getMonth() + 1, 0)
        return {
          startDate: firstDay.toISOString().split('T')[0],
          endDate: lastDay.toISOString().split('T')[0],
        }
      },
    },
    lastMonth: {
      label: 'Mes Pasado',
      getRange: () => {
        const today = new Date()
        const firstDay = new Date(today.getFullYear(), today.getMonth() - 1, 1)
        const lastDay = new Date(today.getFullYear(), today.getMonth(), 0)
        return {
          startDate: firstDay.toISOString().split('T')[0],
          endDate: lastDay.toISOString().split('T')[0],
        }
      },
    },
    last3Months: {
      label: 'Últimos 3 Meses',
      getRange: () => {
        const today = new Date()
        const threeMonthsAgo = new Date(today.getFullYear(), today.getMonth() - 3, 1)
        return {
          startDate: threeMonthsAgo.toISOString().split('T')[0],
          endDate: today.toISOString().split('T')[0],
        }
      },
    },
  }

  // Tab configuration
  const tabs = [
    { key: 'consumption', label: 'Mi Consumo', icon: 'pi pi-list' },
    { key: 'summary', label: 'Resumen', icon: 'pi pi-chart-pie' },
    { key: 'charts', label: 'Gráficos', icon: 'pi pi-chart-bar' },
    { key: 'admin', label: 'Administración', icon: 'pi pi-users' },
  ]

  // Computed properties
  const dashboardStats = computed(() => store.dashboardStats)
  const consumptionByServiceChart = computed(() => store.consumptionByServiceChart)
  const consumptionByDateChart = computed(() => store.consumptionByDateChart)
  const topConsumedItems = computed(() => store.topConsumedItems)

  const hasAnyData = computed(() => {
    return (
      myConsumption.value?.length > 0 || mySummary.value || myConsumptionByService.value?.length > 0
    )
  })

  // Methods
  const setPredefinedRange = (rangeKey) => {
    selectedRange.value = rangeKey
    const range = predefinedRanges[rangeKey].getRange()
    setDateFilter(range.startDate, range.endDate)
    customStartDate.value = range.startDate
    customEndDate.value = range.endDate
  }

  const updateCustomRange = () => {
    if (customStartDate.value && customEndDate.value) {
      selectedRange.value = 'custom'
      setDateFilter(customStartDate.value, customEndDate.value)
    }
  }

  const refreshAllData = async () => {
    if (dateFilter.value.startDate && dateFilter.value.endDate) {
      await getConsumptionByDateRange(dateFilter.value.startDate, dateFilter.value.endDate)
    }
  }

  const clearAllData = () => {
    clearData()
    store.clearData()
  }

  const handleConsumptionCreated = (newConsumption) => {
    showCreateForm.value = false
    // Refresh data to include new consumption
    refreshAllData()
  }

  const exportConsumption = () => {
    exportToCSV(
      formattedMyConsumption.value,
      `mi_consumo_${dateFilter.value.startDate}_${dateFilter.value.endDate}.csv`
    )
  }

  const fetchUserData = async (userId) => {
    // This would be implemented for admin users
    console.log('Fetch user data:', userId)
  }

  const formatDateRange = () => {
    if (!dateFilter.value.startDate || !dateFilter.value.endDate) return 'Sin período'

    const start = new Date(dateFilter.value.startDate).toLocaleDateString('es-AR')
    const end = new Date(dateFilter.value.endDate).toLocaleDateString('es-AR')

    if (start === end) return start
    return `${start} - ${end}`
  }

  // Initialize component
  onMounted(async () => {
    // Set default date range
    setPredefinedRange('thisMonth')

    // Check service health
    try {
      serviceHealth.value = await checkHealth()
    } catch (error) {
      console.warn('Could not check service health:', error)
    }

    // Load initial data
    await refreshAllData()
  })

  // Watch for store updates
  watch(
    () => store.dashboardStats,
    (newStats) => {
      console.log('Dashboard stats updated:', newStats)
    },
    { deep: true }
  )
</script>

<style scoped>
  /* Glass effect styles */
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
    width: 8px;
  }

  ::-webkit-scrollbar-track {
    background: rgba(255, 255, 255, 0.1);
    border-radius: 4px;
  }

  ::-webkit-scrollbar-thumb {
    background: rgba(255, 255, 255, 0.3);
    border-radius: 4px;
  }

  ::-webkit-scrollbar-thumb:hover {
    background: rgba(255, 255, 255, 0.5);
  }
</style>

