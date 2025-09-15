<template>
  <div id="app">
    <!-- Menu Coordination Provider con overlay opcional -->
    <MenuCoordinationProvider :show-overlay="true" overlay-opacity="0.1">
      <!-- Main content area with router view -->
      <router-view />
    </MenuCoordinationProvider>
    
    <!-- Global Toast component -->
    <Toast />
    
    <!-- Performance Indicator (configurable by environment) -->
    <div v-if="showPerformanceIndicator" class="perf-indicator">
      {{ performanceLevel.toUpperCase() }} MODE
    </div>
    
    <!-- Performance Panel (development only) -->
    <PerformancePanel v-if="showPerformancePanel" />
  </div>
</template>

<script setup lang="ts">
  import { onMounted } from 'vue'
  import { useToast } from 'primevue/usetoast'
  import Toast from 'primevue/toast'
  import { useAuthStore } from './store/auth.js'
  import MenuCoordinationProvider from './components/NavBar/MenuCoordinationProvider.vue'
  import PerformancePanel from './components/PerformancePanel.vue'
  import { setGlobalToastInstance } from './utils/toast'
  import { usePerformanceOptimization } from './composables/usePerformanceOptimization.js'

  const authStore = useAuthStore()
  const toast = useToast()
  
  // Initialize performance optimization
  const { performanceLevel, initializeOptimization } = usePerformanceOptimization()
  
  // Environment-based feature flags
  const showPerformanceIndicator = import.meta.env.VITE_ENABLE_PERFORMANCE_INDICATOR === 'true'
  const showPerformancePanel = import.meta.env.VITE_ENABLE_PERFORMANCE_PANEL === 'true'

  onMounted(() => {
    // Add production class to body if in production mode
    if (import.meta.env.PROD) {
      document.body.classList.add('production-mode')
    }
    
    // Initialize global toast instance for use from services/stores
    setGlobalToastInstance(toast)
    
    // Initialize performance optimization system
    initializeOptimization()
    
    // Initialize auth store if needed
    if (localStorage.getItem('token')) {
      authStore.checkAuth()
    }
  })
</script>

<style>
  /* Material Symbols Icons - asegúrate de tener esta fuente cargada */
  @import url('https://fonts.googleapis.com/css2?family=Material+Symbols+Outlined:opsz,wght,FILL,GRAD@20..48,100..700,0..1,-50..200');

  /* Lexend font - si la usas */
  @import url('https://fonts.googleapis.com/css2?family=Lexend+Exa:wght@100;200;300;400;500;600;700;800;900&display=swap');

  /* Global styles */
  #app {
    @apply font-sans antialiased;
  }

  .main-content {
    @apply min-h-screen;
  }

  .lexend-exa {
    font-family: 'Lexend Exa', sans-serif;
  }

  /* Scrollbar personalizado */
  ::-webkit-scrollbar {
    @apply w-2;
  }

  ::-webkit-scrollbar-track {
    @apply bg-gray-100 rounded;
  }

  ::-webkit-scrollbar-thumb {
    @apply bg-gray-300 rounded hover:bg-gray-400;
  }

  /* Focus states */
  *:focus {
    @apply outline-none;
  }

  *:focus-visible {
    @apply ring-2 ring-blue-500 ring-offset-2;
  }

  /* Transition utilities */
  .fade-enter-active,
  .fade-leave-active {
    @apply transition-opacity duration-200;
  }

  .fade-enter-from,
  .fade-leave-to {
    @apply opacity-0;
  }

  /* Loading states */
  .loading {
    @apply animate-pulse;
  }

  /* Error states */
  .error-input {
    @apply border-red-400 focus:border-red-500 focus:ring-red-500;
  }

  .error-text {
    @apply text-red-600 text-sm;
  }

  /* Success states */
  .success-input {
    @apply border-green-400 focus:border-green-500 focus:ring-green-500;
  }

  .success-text {
    @apply text-green-600 text-sm;
  }
</style>
