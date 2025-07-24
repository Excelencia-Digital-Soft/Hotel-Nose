<template>
  <header
    class="principal-convination-color h-16 flex justify-between shadow-md rounded-bl-3xl rounded-br-3xl"
  >
    <!-- Logo -->
    <router-link
      :to="{ name: 'Home' }"
      class="lexend-exa text-xl font-bold hover:scale-110 ml-6 flex justify-center items-center h-full text-white transition duration-150 ease-out md:ease-in hover:text-gray-900"
    >
      inRoom <img src="../../assets/pin.png" class="h-8 invert" alt="" />
    </router-link>

    <!-- Navigation Menu System -->
    <nav v-if="authStore.isAuthenticated" class="flex items-center gap-2 mr-6">
      <!-- Dynamic Menu System -->
      <!-- Operational Menus -->
      <BaseDropdownMenuNoTeleport
        v-for="menu in menuConfig.operational"
        :key="menu.id"
        :label="menu.label"
        :icon="menu.icon"
        :items="getFilteredItems(menu.items)"
        :variant="menu.variant"
        @action="handleMenuAction"
      />

      <!-- Admin Menus -->
      <BaseDropdownMenuNoTeleport
        v-for="menu in menuConfig.admin"
        :key="menu.id"
        :label="menu.label"
        :icon="menu.icon"
        :items="getFilteredItems(menu.items)"
        :variant="menu.variant"
        @action="handleMenuAction"
      />

      <!-- User Menu -->
      <BaseDropdownMenuNoTeleport
        :label="menuConfig.user.label"
        :icon="menuConfig.user.icon"
        :items="getFilteredItems(menuConfig.user.items)"
        :variant="menuConfig.user.variant"
        @action="handleUserAction"
      />
    </nav>
  </header>
</template>

<script setup>
  import { computed } from 'vue'
  import { useRouter } from 'vue-router'
  import { useAuthStore } from '../../store/auth.js'
  import { hasAnyRole } from '../../utils/role-mapping.js'
  import { getMenuConfigForUser, filterMenuItemsByRole } from '../../config/menuConfig.js'
  import BaseDropdownMenuNoTeleport from './BaseDropdownMenuNoTeleport.vue'

  // Composables
  const router = useRouter()
  const authStore = useAuthStore()

  // Menu configuration based on user roles
  const menuConfig = computed(() => {
    return getMenuConfigForUser(authStore.user)
  })

  // Helper function to filter menu items by user roles
  const getFilteredItems = (items) => {
    if (!items) return []
    return filterMenuItemsByRole(items, menuConfig.value.userRoles)
  }

  // Event handlers
  const handleUserAction = async (action) => {
    switch (action) {
      case 'logout':
        await handleLogout()
        break
      default:
        console.log('User action:', action)
    }
  }

  const handleMenuAction = async (action) => {
    switch (action) {
      case 'generateOrderReports':
        await handleGenerateOrderReports()
        break
      case 'generateReports':
        await handleGenerateReports()
        break
      default:
        console.log('Menu action:', action)
    }
  }

  const handleLogout = async () => {
    try {
      await authStore.logout()
      router.push({ name: 'Login' })
    } catch (error) {
      console.error('Error during logout:', error)
      // Forzar logout local si hay error
      authStore.$reset()
      router.push({ name: 'Login' })
    }
  }

  const handleGenerateReports = async () => {
    console.log('Generating general reports...')
    // You can implement actual report generation logic here
    // For now, navigate to a reports page or show a modal
    try {
      router.push({ name: 'StatisticsManager' })
    } catch (error) {
      console.warn('Reports route not found, opening in modal or alternative method')
    }
  }

  const handleGenerateOrderReports = async () => {
    console.log('Generating order reports...')
    // You can implement order-specific report generation logic here
    try {
      router.push({ name: 'OrderHistory' })
    } catch (error) {
      console.warn('Order History route not found')
    }
  }
</script>

<style scoped>
  .principal-convination-color {
    /* Tu gradiente personalizado */
    background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  }

  nav {
    /* Asegurar que el nav tenga suficiente espacio para los dropdowns */
    position: relative;
    z-index: 9999;
  }

  /* Responsive adjustments */
  @media (max-width: 768px) {
    .lexend-exa {
      @apply text-lg;
    }

    nav {
      @apply gap-1 mr-3;
    }
  }
</style>
