<template>
  <div class="menu-coordination-provider" :class="{ 'menu-overlay-active': hasActiveMenu }">
    <slot />
    
    <!-- Optional: Background overlay cuando hay menú activo -->
    <div 
      v-if="hasActiveMenu && showOverlay" 
      class="menu-background-overlay"
      @click="closeAllMenus"
    />
  </div>
</template>

<script setup>
  import { ref, provide, onUnmounted, readonly, computed } from 'vue'

  // Props
  const props = defineProps({
    showOverlay: {
      type: Boolean,
      default: false
    },
    overlayOpacity: {
      type: String,
      default: '0.1'
    }
  })

  // State
  const activeMenuId = ref(null)
  const registeredMenus = new Set()

  // Computed
  const hasActiveMenu = computed(() => activeMenuId.value !== null)

  // Methods
  const registerMenu = (id) => {
    registeredMenus.add(id)
  }

  const unregisterMenu = (id) => {
    registeredMenus.delete(id)
    if (activeMenuId.value === id) {
      activeMenuId.value = null
    }
  }

  const closeAllMenus = () => {
    activeMenuId.value = null
  }

  const openMenu = (menuId) => {
    activeMenuId.value = menuId
  }

  // Provide the coordination object
  const menuCoordination = {
    activeMenuId,
    hasActiveMenu,
    registerMenu,
    unregisterMenu,
    closeAllMenus,
    openMenu,
    registeredMenus: readonly(registeredMenus)
  }

  provide('menuCoordination', menuCoordination)

  // Global event listeners for better UX
  const handleGlobalKeydown = (event) => {
    if (event.key === 'Escape') {
      closeAllMenus()
    }
  }

  const handleGlobalScroll = () => {
    // Close menus on scroll to prevent positioning issues
    closeAllMenus()
  }

  // Add global listeners
  document.addEventListener('keydown', handleGlobalKeydown)
  document.addEventListener('scroll', handleGlobalScroll, true)

  // Cleanup
  onUnmounted(() => {
    document.removeEventListener('keydown', handleGlobalKeydown)
    document.removeEventListener('scroll', handleGlobalScroll, true)
  })

  // Expose for parent components if needed
  defineExpose({
    closeAllMenus,
    hasActiveMenu,
    activeMenuId: readonly(activeMenuId),
    registeredMenusCount: computed(() => registeredMenus.size)
  })
</script>

<style scoped>
  .menu-coordination-provider {
    /* This wrapper allows proper menu layering */
    position: relative;
    min-height: 100vh;
  }

  .menu-background-overlay {
    position: fixed;
    top: 0;
    left: 0;
    right: 0;
    bottom: 0;
    background: rgba(0, 0, 0, v-bind(overlayOpacity));
    backdrop-filter: blur(2px);
    z-index: 9998;
    transition: all 0.2s ease-in-out;
  }

  .menu-overlay-active {
    /* Optional: Add any special styles when menu is active */
  }

  /* Ensure proper stacking context */
  .menu-coordination-provider > * {
    position: relative;
    z-index: 1;
  }
</style>
