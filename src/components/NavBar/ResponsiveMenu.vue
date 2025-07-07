<template>
  <div class="responsive-menu">
    <!-- Desktop Menu -->
    <div class="hidden md:flex h-full items-center gap-2">
      <BaseDropdownMenu
        v-for="menu in visibleMenus"
        :key="menu.id"
        :label="menu.label"
        :icon="menu.icon"
        :items="menu.items"
        :variant="getMenuVariant(menu.id)"
        @action="handleMenuAction"
      />
      
      <!-- User Menu -->
      <BaseDropdownMenu
        :label="userMenuConfig.label"
        :icon="userMenuConfig.icon"
        :items="userMenuConfig.items"
        variant="user"
        @action="handleMenuAction"
      />
    </div>

    <!-- Mobile Menu -->
    <div class="flex md:hidden h-full items-center">
      <MobileMenu 
        :menus="visibleMenus"
        :user-menu="userMenuConfig"
        @action="handleMenuAction"
      />
    </div>
  </div>
</template>

<script setup>
import { computed } from 'vue';
import { useAuthStore } from '../../store/auth.js';
import { useAuth } from '../../composables/useAuth.js';
import { showLogoutSuccessToast } from '../../utils/toast.js';
import { hasAnyRole } from '../../utils/role-mapping.js';
import { menuConfig, userMenuConfig } from './menu-config.js';
import BaseDropdownMenu from './BaseDropdownMenu.vue';
import MobileMenu from './MobileMenu.vue';

const authStore = useAuthStore();
const { logout } = useAuth();

// Helper function to check if user has required roles
const hasUserRole = (allowedRoles) => {
  if (!allowedRoles) return true;
  if (!authStore.user) return false;
  
  const userRoles = authStore.user.roles || (authStore.user.rolId ? [authStore.user.rolId] : []);
  return hasAnyRole(userRoles, allowedRoles);
};

// Filter menus based on user roles
const visibleMenus = computed(() => {
  if (!authStore.isAuthenticated) return [];
  
  return menuConfig.filter(menu => {
    // Check if user has access to the menu itself
    if (!hasUserRole(menu.roles)) return false;
    
    // Check if user has access to at least one item in the menu
    return menu.items.some(item => hasUserRole(item.roles));
  });
});

// Get menu variant based on menu type
const getMenuVariant = (menuId) => {
  const variants = {
    'orders': 'primary',
    'inventory': 'secondary', 
    'rooms': 'primary',
    'cashier': 'secondary'
  };
  return variants[menuId] || 'primary';
};

// Handle menu actions
const handleMenuAction = async (action) => {
  switch (action) {
    case 'logout':
      await logout();
      showLogoutSuccessToast();
      break;
    default:
      console.warn(`Unknown menu action: ${action}`);
  }
};
</script>

<style scoped>
.responsive-menu {
  @apply h-full;
}
</style>