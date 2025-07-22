<template>
  <div class="glass-mobile-menu">
    <!-- Mobile Menu Button -->
    <button 
      @click="toggleMobileMenu"
      class="mobile-menu-button glass-button"
      :class="{ 'menu-open': isOpen }"
    >
      <span class="material-symbols-outlined menu-icon">
        {{ isOpen ? 'close' : 'menu' }}
      </span>
    </button>

    <!-- Mobile Menu Overlay -->
    <div
      v-show="isOpen"
      class="mobile-menu-overlay"
      :class="{ 'overlay-visible': isOpen }"
    >
      <!-- Backdrop -->
      <div class="mobile-backdrop" @click="closeMobileMenu"></div>
      
      <!-- Menu Content -->
      <div class="mobile-menu-content glass-container">
        <!-- Header -->
        <div class="mobile-menu-header glass-card">
          <div class="header-content">
            <span class="material-symbols-outlined header-icon">dashboard</span>
            <span class="header-title">Menú Principal</span>
          </div>
          <button @click="closeMobileMenu" class="close-button glass-button">
            <span class="material-symbols-outlined">close</span>
          </button>
        </div>
        
        <!-- Menu Sections -->
        <div class="mobile-menu-sections">
          <!-- Regular Menus -->
          <div v-for="menu in menus" :key="menu.id" class="menu-section">
            <div class="section-header glass-card">
              <span v-if="menu.icon" class="material-symbols-outlined section-icon">{{ menu.icon }}</span>
              <span class="section-title">{{ menu.label }}</span>
            </div>
            
            <div class="section-items">
              <div v-for="item in getVisibleItems(menu.items)" :key="item.label" class="mobile-menu-item">
                <!-- Router Link -->
                <router-link 
                  v-if="item.route"
                  :to="item.route"
                  class="item-link glass-button"
                  @click="handleItemClick(item)"
                >
                  <span v-if="item.icon" class="material-symbols-outlined item-icon">{{ item.icon }}</span>
                  <div class="item-text">
                    <span class="item-label">{{ item.label }}</span>
                    <span v-if="item.description" class="item-description">{{ item.description }}</span>
                  </div>
                </router-link>
                
                <!-- Action Button -->
                <button 
                  v-else-if="item.action"
                  class="item-button glass-button"
                  @click="handleAction(item)"
                >
                  <span v-if="item.icon" class="material-symbols-outlined item-icon">{{ item.icon }}</span>
                  <div class="item-text">
                    <span class="item-label">{{ item.label }}</span>
                    <span v-if="item.description" class="item-description">{{ item.description }}</span>
                  </div>
                </button>
              </div>
            </div>
          </div>
          
          <!-- User Menu -->
          <div class="menu-section user-section">
            <div class="section-header glass-card user-header">
              <span v-if="userMenu.icon" class="material-symbols-outlined section-icon">{{ userMenu.icon }}</span>
              <span class="section-title">{{ userMenu.label }}</span>
            </div>
            
            <div class="section-items">
              <div v-for="item in getVisibleItems(userMenu.items)" :key="item.label" class="mobile-menu-item">
                <!-- Router Link -->
                <router-link 
                  v-if="item.route"
                  :to="item.route"
                  class="item-link glass-button user-item"
                  @click="handleItemClick(item)"
                >
                  <span v-if="item.icon" class="material-symbols-outlined item-icon">{{ item.icon }}</span>
                  <span class="item-label">{{ item.label }}</span>
                </router-link>
                
                <!-- Action Button -->
                <button 
                  v-else-if="item.action"
                  class="item-button glass-button user-item"
                  @click="handleAction(item)"
                >
                  <span v-if="item.icon" class="material-symbols-outlined item-icon">{{ item.icon }}</span>
                  <span class="item-label">{{ item.label }}</span>
                </button>
              </div>
            </div>
          </div>
        </div>
        
        <!-- Footer -->
        <div class="mobile-menu-footer glass-card">
          <div class="footer-gradient"></div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted, onUnmounted } from 'vue'
import { useAuthStore } from '../../store/auth.js'
import { hasAnyRole } from '../../utils/role-mapping.js'

const props = defineProps({
  menus: {
    type: Array,
    default: () => []
  },
  userMenu: {
    type: Object,
    default: () => ({})
  }
})

const emit = defineEmits(['action', 'item-click'])

// State
const authStore = useAuthStore()
const isOpen = ref(false)

// Methods
const toggleMobileMenu = () => {
  isOpen.value = !isOpen.value
}

const closeMobileMenu = () => {
  isOpen.value = false
}

const hasUserRole = (allowedRoles) => {
  if (!allowedRoles) return true
  if (!authStore.user) return false
  
  const userRoles = authStore.user.roles || (authStore.user.rolId ? [authStore.user.rolId] : [])
  return hasAnyRole(userRoles, allowedRoles)
}

const getVisibleItems = (items) => {
  return items ? items.filter(item => hasUserRole(item.roles)) : []
}

const handleItemClick = (item) => {
  emit('item-click', item)
  closeMobileMenu()
}

const handleAction = (item) => {
  emit('action', item.action)
  closeMobileMenu()
}

// Event handlers
const handleEscape = (event) => {
  if (event.key === 'Escape' && isOpen.value) {
    closeMobileMenu()
  }
}

// Lifecycle
onMounted(() => {
  document.addEventListener('keydown', handleEscape)
})

onUnmounted(() => {
  isOpen.value = false
  document.removeEventListener('keydown', handleEscape)
})
</script>

<style scoped>
/* Glass Effects */
.glass-container {
  @apply bg-white/5 backdrop-blur-2xl border border-white/20 rounded-3xl;
}

.glass-card {
  @apply bg-white/10 backdrop-blur-md border border-white/20 rounded-xl;
}

.glass-button {
  @apply bg-white/10 hover:bg-white/20 backdrop-blur-sm border border-white/30 rounded-lg transition-all duration-300;
}

/* Mobile Menu Button */
.mobile-menu-button {
  @apply p-3 text-white;
  @apply transform hover:scale-110 active:scale-95;
}

.menu-open {
  @apply bg-white/20 shadow-lg;
  transform: scale(1.1) rotate(180deg);
}

.menu-icon {
  @apply text-2xl;
}

/* Mobile Menu Overlay */
.mobile-menu-overlay {
  @apply fixed inset-0 z-50;
  opacity: 0;
  visibility: hidden;
  transition: all 0.3s cubic-bezier(0.4, 0, 0.2, 1);
}

.overlay-visible {
  opacity: 1;
  visibility: visible;
}

.mobile-backdrop {
  @apply absolute inset-0 bg-black/30 backdrop-blur-sm;
}

/* Menu Content */
.mobile-menu-content {
  @apply absolute top-4 left-4 right-4 bottom-4;
  @apply overflow-y-auto;
  transform: translateY(-20px) scale(0.95);
  transition: transform 0.3s cubic-bezier(0.4, 0, 0.2, 1);
}

.overlay-visible .mobile-menu-content {
  transform: translateY(0) scale(1);
}

/* Header */
.mobile-menu-header {
  @apply flex items-center justify-between p-4 mb-4;
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

.close-button {
  @apply p-2 text-white;
}

/* Menu Sections */
.mobile-menu-sections {
  @apply space-y-4 px-2;
}

.menu-section {
  @apply space-y-2;
}

.section-header {
  @apply flex items-center gap-3 p-3;
}

.user-header {
  @apply bg-gradient-to-r from-purple-500/20 to-pink-500/20;
}

.section-icon {
  @apply text-xl text-white;
}

.section-title {
  @apply font-semibold text-white;
}

/* Menu Items */
.section-items {
  @apply space-y-1 ml-2;
}

.mobile-menu-item {
  @apply relative;
}

.item-link,
.item-button {
  @apply w-full p-3 text-left flex items-center gap-3;
  @apply transform hover:scale-105 active:scale-100;
}

.user-item {
  @apply bg-gradient-to-r from-purple-500/10 to-pink-500/10;
  @apply hover:from-purple-500/20 hover:to-pink-500/20;
}

.item-icon {
  @apply text-lg text-white/80;
}

.item-text {
  @apply flex flex-col;
}

.item-label {
  @apply text-white font-medium;
}

.item-description {
  @apply text-xs text-white/60;
}

/* Footer */
.mobile-menu-footer {
  @apply mt-6 h-4 overflow-hidden;
}

.footer-gradient {
  @apply h-full bg-gradient-to-r from-primary-400/20 via-secondary-400/20 to-accent-400/20;
}

/* Custom Scrollbar */
.mobile-menu-content {
  scrollbar-width: thin;
  scrollbar-color: rgba(255, 255, 255, 0.3) transparent;
}

.mobile-menu-content::-webkit-scrollbar {
  width: 4px;
}

.mobile-menu-content::-webkit-scrollbar-track {
  background: transparent;
}

.mobile-menu-content::-webkit-scrollbar-thumb {
  background: rgba(255, 255, 255, 0.3);
  border-radius: 2px;
}

.mobile-menu-content::-webkit-scrollbar-thumb:hover {
  background: rgba(255, 255, 255, 0.5);
}
</style>