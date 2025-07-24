<template>
  <div class="relative h-full" ref="menuContainer">
    <!-- Menu Button -->
    <button
      :class="menuButtonClass"
      @click="toggleMenu"
      @click.stop
      :aria-expanded="isOpen"
      :aria-haspopup="true"
      :disabled="!hasVisibleItems"
    >
      <div class="menu-button-content">
        <span v-if="icon" class="material-symbols-outlined menu-icon">{{ icon }}</span>
        <span class="menu-label">{{ label }}</span>
        <span
          v-if="hasVisibleItems"
          class="material-symbols-outlined dropdown-arrow"
          :class="{ 'rotate-180': isOpen }"
        >
          keyboard_arrow_down
        </span>
      </div>
    </button>

    <!-- Dropdown Menu WITHOUT Teleport -->
    <Transition name="dropdown">
      <ul
        v-if="isOpen && hasVisibleItems"
        class="dropdown-menu-no-teleport"
        role="menu"
        :aria-labelledby="menuId"
        @click.stop
      >
        <li v-for="item in visibleItems" :key="item.label" role="none">
          <router-link
            v-if="item.route"
            :to="item.route"
            :class="['menu-item', { 'menu-item--danger': item.action === 'logout' }]"
            role="menuitem"
            @click="closeMenu"
          >
            <span v-if="item.icon" class="material-symbols-outlined item-icon">
              {{ item.icon }}
            </span>
            <span class="item-label">{{ item.label }}</span>
          </router-link>

          <button
            v-else-if="item.action"
            :class="['menu-item', 'menu-action', { 'menu-item--danger': item.action === 'logout' }]"
            role="menuitem"
            @click="handleAction(item.action)"
          >
            <span v-if="item.icon" class="material-symbols-outlined item-icon">
              {{ item.icon }}
            </span>
            <span class="item-label">{{ item.label }}</span>
          </button>
        </li>
      </ul>
    </Transition>
  </div>
</template>

<script setup>
import { ref, computed, onMounted, onUnmounted, watch, inject } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '../../store/auth.js'
import { hasAnyRole } from '../../utils/role-mapping.js'

// Props
const props = defineProps({
  label: {
    type: String,
    required: true,
  },
  icon: {
    type: String,
    default: null,
  },
  items: {
    type: Array,
    default: () => [],
  },
  variant: {
    type: String,
    default: 'primary',
    validator: (value) => ['primary', 'secondary', 'user'].includes(value),
  },
  disabled: {
    type: Boolean,
    default: false,
  },
})

// Emits
const emit = defineEmits(['action', 'open', 'close'])

// Composables
const router = useRouter()
const authStore = useAuthStore()

// Template refs
const menuContainer = ref(null)

// State
const isOpen = ref(false)

// Menu coordination (optional injection)
const menuCoordination = inject('menuCoordination', null)

// Unique menu ID
const menuId = `dropdown-menu-${Math.random().toString(36).substr(2, 9)}`

// Computed properties
const hasVisibleItems = computed(() => visibleItems.value.length > 0)

const visibleItems = computed(() => {
  return props.items.filter((item) => {
    // Check if item should be shown
    if (item.show === false) return false

    // Check role permissions
    if (item.roles && item.roles.length > 0) {
      const userRoles =
        authStore.user?.roles || (authStore.user?.rolId ? [authStore.user.rolId] : [])
      return hasAnyRole(userRoles, item.roles)
    }

    return true
  })
})

const menuButtonClass = computed(() => {
  return [
    'menu-button',
    `menu-button--${props.variant}`,
    {
      'menu-button--open': isOpen.value,
      'menu-button--disabled': props.disabled || !hasVisibleItems.value,
    },
  ]
})

// Methods
const toggleMenu = () => {
  if (props.disabled || !hasVisibleItems.value) return

  if (isOpen.value) {
    closeMenu()
  } else {
    openMenu()
  }
}

const openMenu = () => {
  // Close other menus if coordination is available
  if (menuCoordination?.closeAllMenus) {
    menuCoordination.closeAllMenus()
  }

  // Register and open this menu
  if (menuCoordination?.openMenu) {
    menuCoordination.openMenu(menuId)
  }

  isOpen.value = true
  emit('open')
}

const closeMenu = () => {
  isOpen.value = false
  emit('close')
}

const handleAction = (action) => {
  emit('action', action)
  closeMenu()
}

// Event handlers
const handleClickOutside = (event) => {
  if (
    isOpen.value &&
    menuContainer.value &&
    event.target &&
    !menuContainer.value.contains(event.target)
  ) {
    closeMenu()
  }
}

const handleKeydown = (event) => {
  if (event.key === 'Escape' && isOpen.value) {
    closeMenu()
  }
}

// Lifecycle hooks
onMounted(() => {
  // Add event listeners
  document.addEventListener('click', handleClickOutside)
  document.addEventListener('keydown', handleKeydown)

  // Register menu with coordination system
  if (menuCoordination?.registerMenu) {
    menuCoordination.registerMenu(menuId)
  }
})

onUnmounted(() => {
  // Clean up event listeners
  document.removeEventListener('click', handleClickOutside)
  document.removeEventListener('keydown', handleKeydown)

  // Unregister from coordination system
  if (menuCoordination?.unregisterMenu) {
    menuCoordination.unregisterMenu(menuId)
  }

  // Force close menu
  isOpen.value = false
})

// Watchers
watch(
  () => authStore.isAuthenticated,
  () => {
    closeMenu()
  }
)

// Watch for menu coordination changes
if (menuCoordination?.activeMenuId) {
  watch(
    () => menuCoordination.activeMenuId.value,
    (newActiveId) => {
      if (newActiveId !== menuId && isOpen.value) {
        isOpen.value = false
      }
    }
  )
}

// Watch for route changes
watch(
  () => router.currentRoute.value,
  () => {
    closeMenu()
  }
)
</script>

<style scoped>
/* Base menu button styles with enhanced glassmorphism */
.menu-button {
  @apply relative h-full transition-all duration-300 ease-out;
  @apply focus:outline-none focus:ring-2 focus:ring-offset-2;
  @apply disabled:opacity-50 disabled:cursor-not-allowed;
  @apply backdrop-blur-sm border border-white/20 rounded-xl;
  background: rgba(255, 255, 255, 0.05);
  box-shadow: 
    0 8px 32px rgba(31, 38, 135, 0.15),
    inset 0 1px 0 rgba(255, 255, 255, 0.1),
    inset 0 -1px 0 rgba(0, 0, 0, 0.1);
}

.menu-button--primary {
  @apply text-white focus:ring-primary-300;
  background: linear-gradient(135deg, 
    rgba(99, 102, 241, 0.15) 0%, 
    rgba(129, 140, 248, 0.1) 50%,
    rgba(99, 102, 241, 0.05) 100%);
  border: 1px solid rgba(99, 102, 241, 0.3);
}

.menu-button--primary:hover {
  background: linear-gradient(135deg, 
    rgba(99, 102, 241, 0.25) 0%, 
    rgba(129, 140, 248, 0.2) 50%,
    rgba(99, 102, 241, 0.15) 100%);
  border-color: rgba(99, 102, 241, 0.4);
  transform: translateY(-2px);
  box-shadow: 
    0 12px 40px rgba(99, 102, 241, 0.3),
    inset 0 1px 0 rgba(255, 255, 255, 0.2),
    inset 0 -1px 0 rgba(0, 0, 0, 0.1);
}

.menu-button--secondary {
  @apply text-white focus:ring-secondary-300;
  background: linear-gradient(135deg, 
    rgba(139, 92, 246, 0.15) 0%, 
    rgba(167, 139, 250, 0.1) 50%,
    rgba(139, 92, 246, 0.05) 100%);
  border: 1px solid rgba(139, 92, 246, 0.3);
}

.menu-button--secondary:hover {
  background: linear-gradient(135deg, 
    rgba(139, 92, 246, 0.25) 0%, 
    rgba(167, 139, 250, 0.2) 50%,
    rgba(139, 92, 246, 0.15) 100%);
  border-color: rgba(139, 92, 246, 0.4);
  transform: translateY(-2px);
  box-shadow: 
    0 12px 40px rgba(139, 92, 246, 0.3),
    inset 0 1px 0 rgba(255, 255, 255, 0.2),
    inset 0 -1px 0 rgba(0, 0, 0, 0.1);
}

.menu-button--user {
  @apply text-white focus:ring-accent-300;
  background: linear-gradient(135deg, 
    rgba(236, 72, 153, 0.15) 0%, 
    rgba(244, 114, 182, 0.1) 50%,
    rgba(236, 72, 153, 0.05) 100%);
  border: 1px solid rgba(236, 72, 153, 0.3);
}

.menu-button--user:hover {
  background: linear-gradient(135deg, 
    rgba(236, 72, 153, 0.25) 0%, 
    rgba(244, 114, 182, 0.2) 50%,
    rgba(236, 72, 153, 0.15) 100%);
  border-color: rgba(236, 72, 153, 0.4);
  transform: translateY(-2px);
  box-shadow: 
    0 12px 40px rgba(236, 72, 153, 0.3),
    inset 0 1px 0 rgba(255, 255, 255, 0.2),
    inset 0 -1px 0 rgba(0, 0, 0, 0.1);
}

.menu-button--open {
  transform: translateY(-2px);
  box-shadow: 
    0 12px 40px rgba(99, 102, 241, 0.4),
    inset 0 1px 0 rgba(255, 255, 255, 0.2),
    inset 0 -1px 0 rgba(0, 0, 0, 0.1);
}

.menu-button--disabled {
  @apply opacity-30 cursor-not-allowed;
  background: rgba(255, 255, 255, 0.02) !important;
  border-color: rgba(255, 255, 255, 0.1) !important;
  transform: none !important;
}

.menu-button-content {
  @apply relative font-bold flex justify-center items-center h-full w-full px-4 py-2 gap-3;
  text-shadow: 0 1px 2px rgba(0, 0, 0, 0.5);
}

.menu-icon {
  @apply text-xl;
  filter: drop-shadow(0 1px 2px rgba(0, 0, 0, 0.3));
}

.menu-label {
  @apply text-sm font-semibold tracking-wide;
  filter: drop-shadow(0 1px 2px rgba(0, 0, 0, 0.3));
}

.dropdown-arrow {
  @apply text-lg transition-transform duration-300;
  filter: drop-shadow(0 1px 2px rgba(0, 0, 0, 0.3));
}

/* Dropdown menu styles WITHOUT Teleport - Enhanced Glassmorphism */
.dropdown-menu-no-teleport {
  @apply absolute top-full right-0 mt-3;
  @apply min-w-[220px] max-w-[300px];
  /* CLAVE: z-index muy alto para estar por encima de todo */
  z-index: 99999;
  
  /* Advanced glassmorphism effect */
  background: linear-gradient(145deg, 
    rgba(255, 255, 255, 0.12) 0%,
    rgba(255, 255, 255, 0.08) 50%,
    rgba(255, 255, 255, 0.05) 100%);
  backdrop-filter: blur(20px);
  border: 1px solid rgba(255, 255, 255, 0.2);
  border-radius: 20px;
  overflow: hidden;
  
  /* Multi-layered shadows for depth */
  box-shadow: 
    0 25px 50px -12px rgba(0, 0, 0, 0.4),
    0 8px 32px rgba(31, 38, 135, 0.37),
    inset 0 1px 0 rgba(255, 255, 255, 0.15),
    inset 0 -1px 0 rgba(0, 0, 0, 0.1),
    0 0 0 1px rgba(255, 255, 255, 0.05);
  
  /* Subtle animation entrance */
  animation: dropdownAppear 0.3s cubic-bezier(0.4, 0, 0.2, 1) forwards;
}

@keyframes dropdownAppear {
  from {
    opacity: 0;
    transform: translateY(-10px) scale(0.95);
    filter: blur(10px);
  }
  to {
    opacity: 1;
    transform: translateY(0) scale(1);
    filter: blur(0);
  }
}

.menu-item {
  @apply flex items-center gap-4 px-5 py-4 text-sm;
  @apply transition-all duration-200 cursor-pointer;
  @apply border-b border-white/10 last:border-b-0;
  @apply w-full text-left;
  
  /* Text styling for better contrast */
  color: rgba(255, 255, 255, 0.9);
  text-shadow: 0 1px 2px rgba(0, 0, 0, 0.5);
  
  /* Subtle background */
  background: rgba(255, 255, 255, 0.02);
  position: relative;
  overflow: hidden;
}

.menu-item::before {
  content: '';
  position: absolute;
  top: 0;
  left: -100%;
  width: 100%;
  height: 100%;
  background: linear-gradient(90deg, 
    transparent, 
    rgba(255, 255, 255, 0.1), 
    transparent);
  transition: left 0.3s ease;
}

.menu-item:hover {
  background: linear-gradient(135deg, 
    rgba(255, 255, 255, 0.15) 0%,
    rgba(255, 255, 255, 0.1) 50%,
    rgba(255, 255, 255, 0.05) 100%);
  color: rgba(255, 255, 255, 1);
  transform: translateX(4px);
  box-shadow: 
    inset 0 1px 0 rgba(255, 255, 255, 0.1),
    4px 0 8px rgba(0, 0, 0, 0.1);
}

.menu-item:hover::before {
  left: 100%;
}

.menu-item:active {
  transform: translateX(2px);
}

.item-icon {
  @apply text-lg;
  color: rgba(255, 255, 255, 0.7);
  filter: drop-shadow(0 1px 2px rgba(0, 0, 0, 0.3));
  transition: all 0.2s ease;
}

.menu-item:hover .item-icon {
  color: rgba(255, 255, 255, 0.9);
  transform: scale(1.1);
}

.item-label {
  @apply flex-1 font-semibold tracking-wide;
  filter: drop-shadow(0 1px 2px rgba(0, 0, 0, 0.3));
}

/* Danger item styles (Cerrar Sesión) with red glassmorphism */
.menu-item--danger {
  background: linear-gradient(135deg, 
    rgba(239, 68, 68, 0.1) 0%,
    rgba(220, 38, 38, 0.05) 50%,
    rgba(185, 28, 28, 0.02) 100%);
  border-color: rgba(239, 68, 68, 0.2);
  color: rgba(248, 113, 113, 0.95);
  
  /* Red glow effect */
  box-shadow: 
    inset 0 1px 0 rgba(248, 113, 113, 0.1),
    0 0 0 1px rgba(239, 68, 68, 0.1);
}

.menu-item--danger .item-icon {
  color: rgba(248, 113, 113, 0.8);
  filter: drop-shadow(0 1px 3px rgba(239, 68, 68, 0.5));
}

.menu-item--danger .item-label {
  color: rgba(248, 113, 113, 0.95);
  filter: drop-shadow(0 1px 3px rgba(239, 68, 68, 0.5));
}

.menu-item--danger::before {
  background: linear-gradient(90deg, 
    transparent, 
    rgba(248, 113, 113, 0.15), 
    transparent);
}

.menu-item--danger:hover {
  background: linear-gradient(135deg, 
    rgba(239, 68, 68, 0.2) 0%,
    rgba(220, 38, 38, 0.15) 50%,
    rgba(185, 28, 28, 0.1) 100%);
  color: rgba(254, 226, 226, 1);
  border-color: rgba(248, 113, 113, 0.4);
  
  /* Enhanced red glow on hover */
  box-shadow: 
    inset 0 1px 0 rgba(248, 113, 113, 0.2),
    4px 0 12px rgba(239, 68, 68, 0.3),
    0 0 0 1px rgba(239, 68, 68, 0.2),
    0 4px 20px rgba(239, 68, 68, 0.15);
  
  transform: translateX(6px);
}

.menu-item--danger:hover .item-icon {
  color: rgba(254, 226, 226, 1);
  transform: scale(1.15);
  filter: drop-shadow(0 2px 8px rgba(239, 68, 68, 0.6));
}

.menu-item--danger:hover .item-label {
  color: rgba(254, 226, 226, 1);
  filter: drop-shadow(0 2px 8px rgba(239, 68, 68, 0.6));
}

.menu-item--danger:hover::before {
  background: linear-gradient(90deg, 
    transparent, 
    rgba(248, 113, 113, 0.25), 
    transparent);
}

.menu-item--danger:active {
  background: linear-gradient(135deg, 
    rgba(220, 38, 38, 0.25) 0%,
    rgba(185, 28, 28, 0.2) 50%,
    rgba(153, 27, 27, 0.15) 100%);
  transform: translateX(4px) scale(0.98);
  box-shadow: 
    inset 0 2px 4px rgba(185, 28, 28, 0.3),
    2px 0 8px rgba(239, 68, 68, 0.2);
}

/* Separator before danger items */
.menu-item--danger {
  position: relative;
  margin-top: 8px;
}

.menu-item--danger::after {
  content: '';
  position: absolute;
  top: -4px;
  left: 20px;
  right: 20px;
  height: 1px;
  background: linear-gradient(90deg, 
    transparent,
    rgba(248, 113, 113, 0.3),
    transparent);
  box-shadow: 0 0 4px rgba(239, 68, 68, 0.2);
}

/* Enhanced transition styles */
.dropdown-enter-active {
  @apply transition-all duration-300 ease-out;
}

.dropdown-leave-active {
  @apply transition-all duration-200 ease-in;
}

.dropdown-enter-from {
  opacity: 0;
  transform: translateY(-15px) scale(0.9) rotateX(-10deg);
  filter: blur(10px);
}

.dropdown-leave-to {
  opacity: 0;
  transform: translateY(-10px) scale(0.95) rotateX(-5deg);
  filter: blur(5px);
}

/* Responsive adjustments */
@media (max-width: 768px) {
  .dropdown-menu-no-teleport {
    @apply left-0 right-0 min-w-full mt-2;
    border-radius: 16px;
    backdrop-filter: blur(15px);
    z-index: 999999;
  }

  .menu-label {
    @apply hidden;
  }

  .menu-button {
    border-radius: 12px;
  }

  .menu-button-content {
    @apply px-3 py-2 gap-2;
  }

  .menu-item {
    @apply px-4 py-3 text-base;
  }

  .item-icon {
    @apply text-xl;
  }
}

/* Enhanced mobile experience */
@media (max-width: 480px) {
  .dropdown-menu-no-teleport {
    border-radius: 12px;
    margin-left: -8px;
    margin-right: -8px;
  }
  
  .menu-item {
    @apply py-4;
  }
}
</style>