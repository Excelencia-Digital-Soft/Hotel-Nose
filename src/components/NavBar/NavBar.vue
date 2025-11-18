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
    <nav v-if="authStore.isAuthenticated" class="flex items-center gap-4 mr-6 text-white">
      <!-- User Info Display -->
      <div class="flex items-center gap-3 mr-2 bg-white/10 rounded-lg px-3 py-1">
        <div class="flex flex-col text-sm leading-tight">
          <span class="font-medium">{{ userInfo.hotel }}</span>
          <span class="text-xs opacity-90">{{ userInfo.rol }}</span>
        </div>
        <div class="w-8 h-8 bg-white/20 rounded-full flex items-center justify-center">
          <i class="fas fa-user text-sm"></i>
        </div>
      </div>

      <!-- Institution Selector (if multiple institutions) -->
      <div v-if="hasMultipleInstitutions" class="mr-2">
        <select
          v-model="selectedInstitutionId"
          @change="onInstitutionChange"
          class="bg-white/10 border border-white/20 text-white text-sm rounded-lg px-2 py-1 focus:ring-2 focus:ring-white/30 focus:border-white/30"
        >
          <option
            v-for="institucion in institutions"
            :key="institucion.institucionId"
            :value="institucion.institucionId"
            class="bg-gray-800 text-white"
          >
            {{ institucion.nombre }}
          </option>
        </select>
      </div>

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
import { getMenuConfigForUser, filterMenuItemsByRole } from '../../config/menuConfig'
import { useNavbar } from '../../composables/useNavbar.js'
import BaseDropdownMenuNoTeleport from './BaseDropdownMenuNoTeleport.vue'

// Composables
const router = useRouter()
const authStore = useAuthStore()

// Usar el composable del navbar para información de usuario
const {
  userInfo,
  institutions,
  hasMultipleInstitutions,
  selectedInstitutionId,
  handleInstitutionChange
} = useNavbar()

// Menu configuration based on user roles
const menuConfig = computed(() => {
  return getMenuConfigForUser(authStore.user)
})

// Helper function to filter menu items by user roles
const getFilteredItems = (items) => {
  if (!items) return []
  return filterMenuItemsByRole(items, menuConfig.value.userRoles)
}

// Handle institution change
const onInstitutionChange = () => {
  if (selectedInstitutionId.value) {
    handleInstitutionChange(selectedInstitutionId.value)
  }
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
    router.push({ name: 'Guest' })
  } catch (error) {
    console.error('Error during logout:', error)
    authStore.$reset()
    router.push({ name: 'Guest' })
  }
}

const handleGenerateReports = async () => {
  console.log('Generating general reports...')
  try {
    router.push({ name: 'StatisticsManager' })
  } catch (error) {
    console.warn('Reports route not found, opening in modal or alternative method')
  }
}

const handleGenerateOrderReports = async () => {
  console.log('Generating order reports...')
  try {
    router.push({ name: 'OrderHistory' })
  } catch (error) {
    console.warn('Order History route not found')
  }
}
</script>

<style scoped>
.principal-convination-color {
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
}

nav {
  position: relative;
  z-index: 9999;
}

select:focus {
  outline: none;
}

select option {
  background-color: #374151;
  color: white;
}

@media (max-width: 768px) {
  .lexend-exa {
    @apply text-lg;
  }

  nav {
    @apply gap-2 mr-3;
  }
}
</style>