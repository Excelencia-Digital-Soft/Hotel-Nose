<template>
  <div class="relative h-full" ref="menuContainer">
    <button 
      :class="menuButtonClass"
      @click="toggleMenu"
      :aria-expanded="isOpen"
      :aria-haspopup="true"
    >
      <div class="menu-button-content">
        <span v-if="icon" class="material-symbols-outlined menu-icon">{{ icon }}</span>
        <span class="menu-label">{{ label }}</span>
        <span v-if="hasVisibleItems" class="material-symbols-outlined dropdown-arrow" :class="{ 'rotate-180': isOpen }">
          keyboard_arrow_down
        </span>
      </div>
    </button>

    <Teleport to="body">
      <Transition name="dropdown">
        <ul 
          v-if="isOpen && hasVisibleItems" 
          class="dropdown-menu"
          role="menu"
          :aria-labelledby="menuId"
          :style="dropdownStyle"
        >
        <li 
          v-for="item in visibleItems" 
          :key="item.label"
          role="none"
        >
          <router-link 
            v-if="item.route"
            :to="item.route"
            class="menu-item"
            role="menuitem"
            @click="closeMenu"
          >
            <span v-if="item.icon" class="material-symbols-outlined item-icon">{{ item.icon }}</span>
            <span class="item-label">{{ item.label }}</span>
          </router-link>
          
          <button 
            v-else-if="item.action"
            class="menu-item menu-action"
            role="menuitem"
            @click="handleAction(item.action)"
          >
            <span v-if="item.icon" class="material-symbols-outlined item-icon">{{ item.icon }}</span>
            <span class="item-label">{{ item.label }}</span>
          </button>
        </li>
        </ul>
      </Transition>
    </Teleport>
  </div>
</template>

<script setup>
import { ref, computed, onMounted, onUnmounted, watch, nextTick, inject } from 'vue';
import { useAuthStore } from '../../store/auth.js';
import { hasAnyRole } from '../../utils/role-mapping.js';

const props = defineProps({
  label: {
    type: String,
    required: true
  },
  icon: {
    type: String,
    default: null
  },
  items: {
    type: Array,
    default: () => []
  },
  variant: {
    type: String,
    default: 'primary', // 'primary', 'secondary', 'user'
    validator: (value) => ['primary', 'secondary', 'user'].includes(value)
  },
  width: {
    type: String,
    default: 'w-40'
  }
});

const emit = defineEmits(['action']);

const authStore = useAuthStore();
const menuContainer = ref(null);
const isOpen = ref(false);
const dropdownPosition = ref({ top: 0, left: 0, width: 0 });
let transitionTimer = null;

// Menu coordination
const menuCoordination = inject('menuCoordination', null);
const menuId = computed(() => `menu-${props.label.toLowerCase().replace(/\s+/g, '-')}`);

const menuButtonClass = computed(() => [
  'menu-button',
  'flex h-full relative',
  props.width,
  {
    'menu-button--primary': props.variant === 'primary',
    'menu-button--secondary': props.variant === 'secondary', 
    'menu-button--user': props.variant === 'user',
    'menu-button--open': isOpen.value,
    'menu-button--disabled': !hasVisibleItems.value
  }
]);

// Helper function to check if user has required roles
const hasUserRole = (allowedRoles) => {
  if (!allowedRoles) return true; // No role restriction
  if (!authStore.user) return false;
  
  const userRoles = authStore.user.roles || (authStore.user.rolId ? [authStore.user.rolId] : []);
  return hasAnyRole(userRoles, allowedRoles);
};

// Filter items based on user roles
const visibleItems = computed(() => {
  return props.items.filter(item => hasUserRole(item.roles));
});

const hasVisibleItems = computed(() => visibleItems.value.length > 0);

// Calculate dropdown position
const dropdownStyle = computed(() => ({
  position: 'fixed',
  top: `${dropdownPosition.value.top}px`,
  left: `${dropdownPosition.value.left}px`,
  minWidth: `${dropdownPosition.value.width}px`,
  zIndex: 9999
}));

// Update dropdown position
const updateDropdownPosition = () => {
  if (!menuContainer.value) return;
  
  const rect = menuContainer.value.getBoundingClientRect();
  dropdownPosition.value = {
    top: rect.bottom + window.scrollY,
    left: rect.left + window.scrollX,
    width: rect.width
  };
};

const toggleMenu = () => {
  if (!hasVisibleItems.value || !menuContainer.value) {
    return;
  }

  // Clear any existing timer
  if (transitionTimer) {
    clearTimeout(transitionTimer);
  }

  // If menu is currently closed, open it and close others
  if (!isOpen.value) {
    // Update position before opening
    updateDropdownPosition();
    
    // Close all other menus first
    if (menuCoordination) {
      menuCoordination.closeAllMenus();
    }
    
    // Then open this one after a short delay
    setTimeout(() => {
      if (menuCoordination) {
        menuCoordination.openMenu(menuId.value);
      }
      isOpen.value = true;
    }, 10);
  } else {
    // If menu is open, close it
    if (menuCoordination) {
      menuCoordination.closeAllMenus();
    }
    isOpen.value = false;
  }
};

const closeMenu = () => {
  isOpen.value = false;
};

const handleAction = (action) => {
  emit('action', action);
  closeMenu();
};

// Handle clicks outside the menu
const handleClickOutside = (event) => {
  if (isOpen.value && menuContainer.value && event.target && !menuContainer.value.contains(event.target)) {
    closeMenu();
  }
};

// Close menu on escape key
const handleKeydown = (event) => {
  if (event.key === 'Escape' && isOpen.value) {
    closeMenu();
  }
};

onMounted(() => {
  document.addEventListener('click', handleClickOutside);
  document.addEventListener('keydown', handleKeydown);
});

onUnmounted(() => {
  // Clear any pending timers
  if (transitionTimer) {
    clearTimeout(transitionTimer);
    transitionTimer = null;
  }
  
  // Close menu immediately to prevent transition issues
  isOpen.value = false;
  
  // Remove event listeners
  document.removeEventListener('click', handleClickOutside);
  document.removeEventListener('keydown', handleKeydown);
});

// Watch for route changes to close menu
watch(() => authStore.isAuthenticated, () => {
  closeMenu();
});

// Watch for menu coordination changes
if (menuCoordination) {
  watch(() => menuCoordination.activeMenuId.value, (newActiveId) => {
    if (newActiveId !== menuId.value && isOpen.value) {
      // Another menu was opened, close this one immediately
      isOpen.value = false;
    }
  });
}
</script>

<style scoped>
.menu-button {
  @apply transition-all duration-200 ease-in-out focus:outline-none focus:ring-2 focus:ring-offset-2;
}

.menu-button--primary {
  @apply text-white hover:bg-white/10 rounded-lg focus:ring-primary-300;
}

.menu-button--secondary {
  @apply text-white hover:bg-white/10 rounded-lg focus:ring-secondary-300;
}

.menu-button--user {
  @apply text-white hover:bg-white/10 rounded-lg focus:ring-accent-300;
}

.menu-button--open {
  @apply bg-white/10;
}

.menu-button--disabled {
  @apply opacity-50 cursor-not-allowed;
}

.menu-button-content {
  @apply relative font-bold flex justify-center items-center h-full w-full px-3 py-2 gap-2;
}

.menu-icon {
  @apply text-xl;
}

.menu-label {
  @apply text-sm font-medium;
}

.dropdown-arrow {
  @apply text-lg transition-transform duration-200;
}

.dropdown-menu {
  @apply bg-white border border-gray-200 rounded-lg shadow-lg overflow-hidden;
  @apply min-w-[200px] max-w-[280px];
}

.menu-item {
  @apply flex items-center gap-3 px-4 py-3 text-sm text-gray-700 hover:bg-gray-50 hover:text-gray-900 transition-colors cursor-pointer;
  @apply border-b border-gray-100 last:border-b-0;
}

.menu-action {
  @apply w-full text-left;
}

.item-icon {
  @apply text-lg text-gray-500;
}

.item-label {
  @apply flex-1 font-medium;
}

/* Transition styles */
.dropdown-enter-active {
  @apply transition-all duration-150 ease-out;
}

.dropdown-leave-active {
  @apply transition-all duration-100 ease-in;
}

.dropdown-enter-from {
  @apply opacity-0 transform scale-95 -translate-y-1;
}

.dropdown-leave-to {
  @apply opacity-0 transform scale-95 -translate-y-1;
}

/* Responsive adjustments */
@media (max-width: 768px) {
  .dropdown-menu {
    @apply left-0 right-0 min-w-full;
  }
  
  .menu-label {
    @apply hidden;
  }
  
  .menu-button-content {
    @apply px-2;
  }
}
</style>
