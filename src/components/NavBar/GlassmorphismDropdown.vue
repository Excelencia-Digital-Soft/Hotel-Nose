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
          <span class="material-symbols-outlined dropdown-arrow" :class="{ 'rotate-180': isOpen }">
            keyboard_arrow_down
          </span>
        </div>
      </div>
    </button>

    <!-- Glassmorphism Dropdown -->
    <Teleport to="body">
      <div
        v-show="isOpen && hasVisibleItems"
        class="dropdown-menu glass-container"
        :style="dropdownStyle"
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
            class="menu-item glass-button group"
            @click="handleItemClick(item)"
          >
            <div class="item-content">
              <span v-if="item.icon" class="material-symbols-outlined item-icon">{{ item.icon }}</span>
              <div class="item-text">
                <span class="item-label">{{ item.label }}</span>
                <span v-if="item.description" class="item-description">{{ item.description }}</span>
              </div>
            </div>
            <div class="item-glow"></div>
          </router-link>
          
          <!-- Action Button Item -->
          <button 
            v-else-if="item.action"
            class="menu-item glass-button menu-action group"
            @click="handleAction(item)"
          >
            <div class="item-content">
              <span v-if="item.icon" class="material-symbols-outlined item-icon">{{ item.icon }}</span>
              <div class="item-text">
                <span class="item-label">{{ item.label }}</span>
                <span v-if="item.description" class="item-description">{{ item.description }}</span>
              </div>
            </div>
            <div class="item-glow"></div>
          </button>
        </div>
      </div>
      
      <!-- Menu Footer -->
      <div class="menu-footer glass-card">
        <div class="footer-gradient"></div>
      </div>
      </div>
    </Teleport>
  </div>
</template>

<script setup>
import { ref, computed, onMounted, onUnmounted, watch } from 'vue'
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
const dropdownPosition = ref({ top: 0, left: 0, width: 0 })

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

// Calculate dropdown position for Teleport
const dropdownStyle = computed(() => ({
  position: 'fixed',
  top: `${dropdownPosition.value.top}px`,
  left: `${dropdownPosition.value.left}px`,
  minWidth: `${Math.max(dropdownPosition.value.width, 320)}px`,
  zIndex: 9999
}))

// Update dropdown position
const updateDropdownPosition = () => {
  if (!menuContainer.value) return
  
  const rect = menuContainer.value.getBoundingClientRect()
  dropdownPosition.value = {
    top: rect.bottom + window.scrollY + 8,
    left: rect.left + window.scrollX,
    width: rect.width
  }
}

// Methods
const handleToggle = () => {
  if (hasVisibleItems.value) {
    if (!isOpen.value) {
      updateDropdownPosition()
    }
    isOpen.value = !isOpen.value
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

// Lifecycle
onMounted(() => {
  document.addEventListener('click', handleClickOutside)
  document.addEventListener('keydown', handleKeydown)
})

onUnmounted(() => {
  isOpen.value = false
  document.removeEventListener('click', handleClickOutside)
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
  @apply bg-white/20 backdrop-blur-2xl border border-white/30 rounded-3xl;
  background: rgba(255, 255, 255, 0.15);
  box-shadow: 0 8px 32px 0 rgba(31, 38, 135, 0.37);
}

.glass-card {
  @apply bg-white/10 backdrop-blur-md border border-white/20 rounded-xl;
}

.glass-button {
  @apply bg-white/10 hover:bg-white/20 backdrop-blur-sm border border-white/30 rounded-lg transition-all duration-300;
}

/* Button Styles */
.glass-dropdown-button {
  @apply relative h-full flex items-center justify-center font-bold transition-all duration-300 ease-out;
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
  transform: scale(1.02);
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
  @apply text-lg transition-transform duration-300;
}

/* Dropdown Menu */
.dropdown-menu {
  @apply w-80 max-w-sm shadow-2xl;
}

/* Menu Header */
.menu-header {
  @apply p-4 border-b border-white/10;
}

.header-content {
  @apply flex items-center gap-3;
}

.header-icon {
  @apply text-2xl text-white;
}

.header-title {
  @apply text-lg font-bold text-white;
}

/* Menu Items */
.menu-items {
  @apply p-2 space-y-1;
}

.menu-item-wrapper {
  @apply relative;
}

.menu-item {
  @apply relative w-full p-4 text-left overflow-hidden;
  @apply transform hover:scale-105 active:scale-100;
}

.item-content {
  @apply relative z-10 flex items-center gap-3;
}

.item-icon {
  @apply text-xl text-white/80 group-hover:text-white transition-colors;
}

.item-text {
  @apply flex flex-col;
}

.item-label {
  @apply text-white font-medium group-hover:text-white transition-colors;
}

.item-description {
  @apply text-xs text-white/60 group-hover:text-white/80 transition-colors;
}

.item-glow {
  @apply absolute inset-0 bg-gradient-to-r opacity-0 group-hover:opacity-100 transition-opacity duration-300;
  background: linear-gradient(135deg, rgba(139, 92, 246, 0.1), rgba(219, 39, 119, 0.1), rgba(59, 130, 246, 0.1));
}

.menu-action {
  @apply cursor-pointer;
}

/* Menu Footer */
.menu-footer {
  @apply relative h-4 border-t border-white/10 overflow-hidden;
}

.footer-gradient {
  @apply absolute inset-0 bg-gradient-to-r from-primary-400/20 via-secondary-400/20 to-accent-400/20;
}

/* Animations */
@keyframes glow {
  0%, 100% { 
    box-shadow: 0 0 20px rgba(139, 92, 246, 0.3);
  }
  50% { 
    box-shadow: 0 0 30px rgba(219, 39, 119, 0.4), 0 0 40px rgba(59, 130, 246, 0.3);
  }
}

.menu-item:hover {
  animation: glow 2s ease-in-out infinite;
}

/* Responsive */
@media (max-width: 768px) {
  .dropdown-menu {
    @apply w-72 right-0 left-auto;
    transform: translateX(0);
  }
  
  .menu-label {
    @apply hidden;
  }
  
  .button-content {
    @apply px-2;
  }
}

@media (max-width: 640px) {
  .dropdown-menu {
    @apply w-64;
    right: -2rem;
  }
}

@media (max-width: 480px) {
  .dropdown-menu {
    @apply w-56;
    right: -4rem;
  }
}
</style>
