<template>
  <div class="mobile-menu relative" ref="mobileMenuContainer">
    <button 
      class="mobile-menu-trigger"
      @click="toggleMobileMenu"
      :aria-expanded="isOpen"
      :aria-label="isOpen ? 'Cerrar menú' : 'Abrir menú'"
    >
      <span class="material-symbols-outlined mobile-menu-icon">
        {{ isOpen ? 'close' : 'menu' }}
      </span>
    </button>

    <Transition name="mobile-dropdown" appear>
      <div v-if="isOpen" class="mobile-menu-overlay">
        <div class="mobile-menu-content">
          <!-- Main Menu Items -->
          <div class="mobile-menu-section">
            <h3 class="mobile-section-title">Menú Principal</h3>
            <div 
              v-for="menu in menus" 
              :key="menu.id"
              class="mobile-menu-group"
            >
              <button 
                class="mobile-group-header"
                @click="toggleGroup(menu.id)"
                :aria-expanded="openGroups[menu.id]"
              >
                <span class="material-symbols-outlined group-icon">{{ menu.icon }}</span>
                <span class="group-label">{{ menu.label }}</span>
                <span 
                  class="material-symbols-outlined expand-icon"
                  :class="{ 'rotate-180': openGroups[menu.id] }"
                >
                  expand_more
                </span>
              </button>

              <Transition name="group-expand">
                <div v-if="openGroups[menu.id]" class="mobile-group-items">
                  <router-link
                    v-for="item in getVisibleItems(menu.items)"
                    :key="item.label"
                    :to="item.route"
                    class="mobile-menu-item"
                    @click="closeMobileMenu"
                  >
                    <span v-if="item.icon" class="material-symbols-outlined item-icon">{{ item.icon }}</span>
                    <span class="item-label">{{ item.label }}</span>
                  </router-link>
                </div>
              </Transition>
            </div>
          </div>

          <!-- User Menu -->
          <div class="mobile-menu-section border-t">
            <h3 class="mobile-section-title">{{ userMenu.label }}</h3>
            <div class="mobile-user-items">
              <router-link
                v-for="item in getVisibleUserItems()"
                :key="item.label"
                :to="item.route"
                class="mobile-menu-item"
                @click="closeMobileMenu"
              >
                <span v-if="item.icon" class="material-symbols-outlined item-icon">{{ item.icon }}</span>
                <span class="item-label">{{ item.label }}</span>
              </router-link>
              
              <button
                v-for="item in getUserActionItems()"
                :key="item.label"
                class="mobile-menu-item mobile-action"
                @click="handleAction(item.action)"
              >
                <span v-if="item.icon" class="material-symbols-outlined item-icon">{{ item.icon }}</span>
                <span class="item-label">{{ item.label }}</span>
              </button>
            </div>
          </div>
        </div>
      </div>
    </Transition>
  </div>
</template>

<script setup>
import { ref, reactive, computed, onMounted, onUnmounted } from 'vue';
import { useAuthStore } from '../../store/auth.js';
import { hasAnyRole } from '../../utils/role-mapping.js';

const props = defineProps({
  menus: {
    type: Array,
    required: true
  },
  userMenu: {
    type: Object,
    required: true
  }
});

const emit = defineEmits(['action']);

const authStore = useAuthStore();
const mobileMenuContainer = ref(null);
const isOpen = ref(false);
const openGroups = reactive({});

// Helper function to check if user has required roles
const hasUserRole = (allowedRoles) => {
  if (!allowedRoles) return true;
  if (!authStore.user) return false;
  
  const userRoles = authStore.user.roles || (authStore.user.rolId ? [authStore.user.rolId] : []);
  return hasAnyRole(userRoles, allowedRoles);
};

// Get visible items for a menu
const getVisibleItems = (items) => {
  return items.filter(item => item.route && hasUserRole(item.roles));
};

// Get visible user menu items (routes only)
const getVisibleUserItems = () => {
  return props.userMenu.items.filter(item => item.route && hasUserRole(item.roles));
};

// Get user action items (actions only)
const getUserActionItems = () => {
  return props.userMenu.items.filter(item => item.action && hasUserRole(item.roles));
};

const toggleMobileMenu = () => {
  isOpen.value = !isOpen.value;
  if (!isOpen.value) {
    // Reset all open groups when closing menu
    Object.keys(openGroups).forEach(key => {
      openGroups[key] = false;
    });
  }
};

const closeMobileMenu = () => {
  isOpen.value = false;
  Object.keys(openGroups).forEach(key => {
    openGroups[key] = false;
  });
};

const toggleGroup = (groupId) => {
  openGroups[groupId] = !openGroups[groupId];
};

const handleAction = (action) => {
  emit('action', action);
  closeMobileMenu();
};

// Handle clicks outside the menu
const handleClickOutside = (event) => {
  if (mobileMenuContainer.value && !mobileMenuContainer.value.contains(event.target)) {
    closeMobileMenu();
  }
};

// Handle escape key
const handleKeydown = (event) => {
  if (event.key === 'Escape') {
    closeMobileMenu();
  }
};

onMounted(() => {
  document.addEventListener('click', handleClickOutside);
  document.addEventListener('keydown', handleKeydown);
});

onUnmounted(() => {
  document.removeEventListener('click', handleClickOutside);
  document.removeEventListener('keydown', handleKeydown);
});
</script>

<style scoped>
.mobile-menu {
  @apply relative h-full flex items-center mr-4;
}

.mobile-menu-trigger {
  @apply flex items-center justify-center w-10 h-10 text-white hover:bg-white/10 rounded-lg transition-colors focus:outline-none focus:ring-2 focus:ring-white/20;
}

.mobile-menu-icon {
  @apply text-2xl;
}

.mobile-menu-overlay {
  @apply fixed inset-0 z-50 bg-black/50 backdrop-blur-sm;
}

.mobile-menu-content {
  @apply absolute top-16 left-0 right-0 bg-white shadow-xl rounded-b-2xl max-h-[calc(100vh-4rem)] overflow-y-auto;
}

.mobile-menu-section {
  @apply p-4 border-gray-100;
}

.mobile-section-title {
  @apply text-sm font-semibold text-gray-500 uppercase tracking-wide mb-3;
}

.mobile-menu-group {
  @apply mb-2;
}

.mobile-group-header {
  @apply w-full flex items-center gap-3 p-3 text-left bg-gray-50 hover:bg-gray-100 rounded-lg transition-colors;
}

.group-icon {
  @apply text-xl text-gray-600;
}

.group-label {
  @apply flex-1 font-medium text-gray-800;
}

.expand-icon {
  @apply text-lg text-gray-500 transition-transform duration-200;
}

.mobile-group-items {
  @apply mt-2 space-y-1 pl-4;
}

.mobile-user-items {
  @apply space-y-1;
}

.mobile-menu-item {
  @apply flex items-center gap-3 p-3 text-gray-700 hover:bg-gray-50 hover:text-gray-900 rounded-lg transition-colors;
}

.mobile-action {
  @apply w-full text-left;
}

.item-icon {
  @apply text-lg text-gray-500;
}

.item-label {
  @apply font-medium;
}

/* Transitions */
.mobile-dropdown-enter-active,
.mobile-dropdown-leave-active {
  @apply transition-all duration-300;
}

.mobile-dropdown-enter-from {
  @apply opacity-0;
}

.mobile-dropdown-enter-from .mobile-menu-content {
  @apply transform -translate-y-4 scale-95;
}

.mobile-dropdown-leave-to {
  @apply opacity-0;
}

.mobile-dropdown-leave-to .mobile-menu-content {
  @apply transform -translate-y-4 scale-95;
}

.group-expand-enter-active,
.group-expand-leave-active {
  @apply transition-all duration-200;
}

.group-expand-enter-from,
.group-expand-leave-to {
  @apply opacity-0 transform -translate-y-2;
}

/* Scrollbar styling for mobile menu */
.mobile-menu-content::-webkit-scrollbar {
  @apply w-1;
}

.mobile-menu-content::-webkit-scrollbar-track {
  @apply bg-transparent;
}

.mobile-menu-content::-webkit-scrollbar-thumb {
  @apply bg-gray-300 rounded-full;
}

.mobile-menu-content::-webkit-scrollbar-thumb:hover {
  @apply bg-gray-400;
}
</style>