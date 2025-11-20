<template>
  <div class="app-layout">
    <!-- Navigation Bar -->
    <NavBar />
    <!-- Main Content -->
    <main class="main-content">
      <router-view />
    </main>
  </div>
</template>

<script setup>
  import { onMounted } from 'vue'
  import NavBar from '../components/NavBar/NavBar.vue'
  import { showLoginSuccessToast } from '../utils/toast'

  onMounted(() => {
    // Check for login success message
    const loginUser = sessionStorage.getItem('loginSuccessUser')
    if (loginUser) {
      // Small delay to ensure Toast is ready
      setTimeout(() => {
        showLoginSuccessToast(loginUser)
        sessionStorage.removeItem('loginSuccessUser')
      }, 100)
    }
  })
</script>

<style scoped>
  .main-content {
    @apply flex-1 pt-4 pb-8 px-4;
    min-height: calc(100vh - 4rem); /* Subtract navbar height */
  }

  /* Glassmorphism background pattern */
  .app-layout::before {
    content: '';
    position: fixed;
    top: 0;
    left: 0;
    right: 0;
    bottom: 0;
    background:
      radial-gradient(circle at 20% 30%, rgba(129, 140, 248, 0.1) 0%, transparent 50%),
      radial-gradient(circle at 80% 70%, rgba(167, 139, 250, 0.1) 0%, transparent 50%),
      radial-gradient(circle at 40% 80%, rgba(244, 114, 182, 0.1) 0%, transparent 50%);
    pointer-events: none;
    z-index: -1;
  }

  /* Responsive adjustments */
  @media (max-width: 768px) {
    .main-content {
      @apply pt-2 px-2;
    }
  }
</style>
