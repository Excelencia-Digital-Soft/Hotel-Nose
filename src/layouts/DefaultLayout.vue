<template>
    <NavBar />
    <main>
        <router-view />
    </main>
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
