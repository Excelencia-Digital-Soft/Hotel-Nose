<template>
  <div class="relative h-full" ref="menuContainer">
    <!-- Menu Button -->
    <button 
      :class="buttonClasses"
      @click="handleToggle"
      :aria-expanded="isOpen"
      :aria-haspopup="true"
      :disabled="!hasVisibleItems"
    >
      <div class="button-content">
        <!-- Icon -->
        <div v-if="icon" class="icon-wrapper">
          <span class="material-symbols-outlined menu-icon">{{ icon }}</span>
        </div>
        
        <!-- Label -->
        <span class="menu-label">{{ label }}</span>
        
        <!-- Arrow -->
        <div v-if="hasVisibleItems" class="arrow-wrapper">
          <span class="material-symbols-outlined dropdown-arrow">
            {{ isOpen ? 'keyboard_arrow_up' : 'keyboard_arrow_down' }}
          </span>
        </div>
      </div>
    </button>

    <!-- Simple Dropdown without transitions -->
    <div
      v-show="isOpen && hasVisibleItems"
      class="dropdown-menu glass-container"
    >
      <!-- Menu Header -->
      <div class="menu-header glass-card">
        <div class="header-content">
          <span v-if="icon" class="material-symbols-outlined header-icon">{{ icon }}</span>
          <span class="header-title">{{ label }}</span>
        </div>
      </div>
      
      <!-- Menu Items -->
      <div class="menu-items">
        <div 
          v-for="item in visibleItems" 
          :key="item.label"
          class="menu-item-wrapper"
        >
          <!-- Router Link Item -->
          <router-link 
            v-if="item.route"
            :to="item.route"
            class="menu-item glass-button"
            @click="handleItemClick(item)"
          >
            <span v-if="item.icon" class="material-symbols-outlined item-icon">{{ item.icon }}</span>
            <span class="item-label">{{ item.label }}</span>
          </router-link>
          
          <!-- Action Button Item -->
          <button 
            v-else-if="item.action"
            class="menu-item glass-button menu-action"
            @click="handleAction(item)"
          >
            <span v-if="item.icon" class="material-symbols-outlined item-icon">{{ item.icon }}</span>
            <span class="item-label">{{ item.label }}</span>
          </button>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted, onUnmounted, watch, nextTick } from 'vue'
import { useAuthStore } from '../../store/auth.js'
import { hasAnyRole } from '../../utils/role-mapping.js'

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
    default: 'primary',
    validator: (value) => ['primary', 'secondary', 'accent', 'user'].includes(value)
  },
  size: {
    type: String,
    default: 'medium',
    validator: (value) => ['small', 'medium', 'large'].includes(value)
  }
})

const emit = defineEmits(['action', 'item-click'])

// Reactive state
const authStore = useAuthStore()
const menuContainer = ref(null)
const isOpen = ref(false)

// Computed properties
const buttonClasses = computed(() => [
  'glass-dropdown-button',
  `variant-${props.variant}`,
  `size-${props.size}`,
  {
    'button-open': isOpen.value,
    'button-disabled': !hasVisibleItems.value
  }
])

// Check user roles
const hasUserRole = (allowedRoles) => {
  if (!allowedRoles) return true
  if (!authStore.user) return false
  
  const userRoles = authStore.user.roles || (authStore.user.rolId ? [authStore.user.rolId] : [])
  return hasAnyRole(userRoles, allowedRoles)
}

// Filter visible items
const visibleItems = computed(() => {
  return props.items.filter(item => hasUserRole(item.roles))
})

const hasVisibleItems = computed(() => visibleItems.value.length > 0)

// Methods  
const handleToggle = () => {
  if (hasVisibleItems.value) {
    isOpen.value = !isOpen.value
    console.log('Toggle clicked, isOpen:', isOpen.value)
  }
}

const closeMenu = () => {
  isOpen.value = false
}

const handleItemClick = (item) => {
  emit('item-click', item)
  closeMenu()
}

const handleAction = (item) => {
  emit('action', item.action)
  closeMenu()
}

// Click outside handler 
const handleClickOutside = (event) => {
  if (isOpen.value && menuContainer.value && !menuContainer.value.contains(event.target)) {
    closeMenu()
  }
}

// Keyboard handler
const handleKeydown = (event) => {
  if (event.key === 'Escape' && isOpen.value) {
    closeMenu()
  }
}

// Lifecycle - simplified event handling
onMounted(() => {
  document.addEventListener('keydown', handleKeydown)
})

onUnmounted(() => {
  isOpen.value = false
  document.removeEventListener('keydown', handleKeydown)
})

// Watchers
watch(() => authStore.isAuthenticated, () => {
  closeMenu()
})
</script>

<style scoped>
/* Glass Effects Base */
.glass-container {
  @apply bg-white/20 backdrop-blur-2xl border border-white/30 rounded-2xl;
  background: rgba(255, 255, 255, 0.15);
  box-shadow: 0 8px 32px 0 rgba(31, 38, 135, 0.37);
}

.glass-card {
  @apply bg-white/10 backdrop-blur-md border border-white/20 rounded-xl;
}

.glass-button {
  @apply bg-white/10 hover:bg-white/20 backdrop-blur-sm border border-white/30 rounded-lg;
}

/* Button Styles */
.glass-dropdown-button {
  @apply relative h-full flex items-center justify-center font-bold;
  @apply focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-offset-transparent;
  @apply glass-button;
}

/* Button Variants */
.variant-primary {
  @apply text-white hover:text-primary-200 focus:ring-primary-400;
}

.variant-secondary {
  @apply text-white hover:text-secondary-200 focus:ring-secondary-400;
}

.variant-accent {
  @apply text-white hover:text-accent-200 focus:ring-accent-400;
}

.variant-user {
  @apply text-white hover:text-purple-200 focus:ring-purple-400;
}

/* Button Sizes */
.size-small {
  @apply px-2 py-1 text-sm rounded-lg;
}

.size-medium {
  @apply px-3 py-2 text-base rounded-xl;
}

.size-large {
  @apply px-4 py-3 text-lg rounded-2xl;
}

/* Button States */
.button-open {
  @apply bg-white/20 shadow-lg;
}

.button-disabled {
  @apply opacity-50 cursor-not-allowed;
}

/* Button Content */
.button-content {
  @apply flex items-center gap-2;
}

.icon-wrapper {
  @apply flex items-center justify-center;
}

.menu-icon {
  @apply text-xl;
}

.menu-label {
  @apply font-medium;
}

.arrow-wrapper {
  @apply flex items-center justify-center ml-1;
}

.dropdown-arrow {
  @apply text-lg;
}

/* Dropdown Menu */
.dropdown-menu {
  @apply absolute top-full left-0 w-72 shadow-2xl z-50 mt-2;
}

/* Menu Header */
.menu-header {
  @apply p-3 border-b border-white/10;
}

.header-content {
  @apply flex items-center gap-2;
}

.header-icon {
  @apply text-xl text-white;
}

.header-title {
  @apply text-base font-semibold text-white;
}

/* Menu Items */
.menu-items {
  @apply p-2 space-y-1;
}

.menu-item-wrapper {
  @apply relative;
}

.menu-item {
  @apply relative w-full p-3 text-left flex items-center gap-3;
}

.item-icon {
  @apply text-lg text-white/80;
}

.item-label {
  @apply text-white font-medium;
}

.menu-action {
  @apply cursor-pointer;
}

/* Responsive */
@media (max-width: 768px) {
  .dropdown-menu {
    @apply w-64;
  }
  
  .menu-label {
    @apply hidden;
  }
  
  .button-content {
    @apply px-2;
  }
}
</style>