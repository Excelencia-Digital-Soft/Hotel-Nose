<template>
  <div class="glass-responsive-menu">
    <!-- Desktop Menu -->
    <div class="hidden md:flex h-full items-center gap-2">
      <SimpleGlassDropdown
        v-for="menu in visibleMenus"
        :key="menu.id"
        :label="menu.label"
        :icon="menu.icon"
        :items="menu.items"
        :variant="getMenuVariant(menu.id)"
        size="medium"
        @action="handleMenuAction"
        @item-click="handleItemClick"
      />
      
      <!-- User Menu -->
      <SimpleGlassDropdown
        :label="userMenuConfig.label"
        :icon="userMenuConfig.icon"
        :items="userMenuConfig.items"
        variant="user"
        size="medium"
        @action="handleMenuAction"
        @item-click="handleItemClick"
      />
    </div>

    <!-- Mobile Menu -->
    <div class="flex md:hidden h-full items-center">
      <GlassMobileMenu 
        :menus="visibleMenus"
        :user-menu="userMenuConfig"
        @action="handleMenuAction"
        @item-click="handleItemClick"
      />
    </div>
  </div>
</template>

<script setup>
import { computed } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '../../store/auth.js'
import { useAuth } from '../../composables/useAuth.js'
import { showLogoutSuccessToast } from '../../utils/toast.js'
import { hasAnyRole } from '../../utils/role-mapping.js'
import { menuConfig, userMenuConfig } from './menu-config.js'
import SimpleGlassDropdown from './SimpleGlassDropdown.vue'
import GlassMobileMenu from './GlassMobileMenu.vue'

const router = useRouter()
const authStore = useAuthStore()
const { logout } = useAuth()

// Helper function to check if user has required roles
const hasUserRole = (allowedRoles) => {
  if (!allowedRoles) return true
  if (!authStore.user) return false
  
  const userRoles = authStore.user.roles || (authStore.user.rolId ? [authStore.user.rolId] : [])
  return hasAnyRole(userRoles, allowedRoles)
}

// Filter visible menus
const visibleMenus = computed(() => {
  return menuConfig.filter(menu => hasUserRole(menu.roles))
})

// Menu variant mapping
const getMenuVariant = (menuId) => {
  const variants = {
    'rooms': 'primary',
    'inventory': 'secondary', 
    'reports': 'accent',
    'settings': 'primary'
  }
  return variants[menuId] || 'primary'
}

// Event handlers
const handleMenuAction = async (action) => {
  console.log('Menu action triggered:', action)
  
  switch (action) {
    case 'logout':
      try {
        await logout()
        showLogoutSuccessToast()
        router.push('/auth/login')
      } catch (error) {
        console.error('Logout failed:', error)
      }
      break
      
    default:
      console.warn('Unknown action:', action)
  }
}

const handleItemClick = (item) => {
  console.log('Menu item clicked:', item)
  
  // Handle special item actions
  if (item.action) {
    handleMenuAction(item.action)
  }
}
</script>

<style scoped>
.glass-responsive-menu {
  @apply h-full;
}

/* Enhanced glassmorphism effects for better visual hierarchy */
.glass-responsive-menu::before {
  content: '';
  @apply absolute inset-0 pointer-events-none;
  background: linear-gradient(135deg, 
    rgba(139, 92, 246, 0.1) 0%,
    rgba(219, 39, 119, 0.08) 50%,
    rgba(59, 130, 246, 0.1) 100%
  );
  border-radius: inherit;
}

/* Responsive adjustments */
@media (max-width: 768px) {
  .glass-responsive-menu {
    @apply flex justify-center;
  }
}
</style>