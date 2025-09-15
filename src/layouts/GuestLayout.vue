<template>
  <div class="father w-full flex justify-center items-center">
    <div 
      class="container backdrop-filter backdrop-blur-md bg-opacity-30" 
      :class="{ 'right-panel-active': isSignUp }"
    >
      <!-- Register Form Container -->
      <div class="form-container sign-up-container">
        <RegisterForm />
      </div>
      
      <!-- Login Form Container -->
      <div class="form-container sign-in-container">
        <LoginForm />
      </div>
      
      <!-- Auth Panel -->
      <AuthPanel 
        @toggle-to-login="toggleToLogin"
        @toggle-to-register="toggleToRegister"
      />
    </div>
    
    <!-- Toast notifications -->
    <Toast />
  </div>
</template>
<script setup>
import { ref, onMounted } from 'vue'
import Toast from 'primevue/toast'
import LoginForm from '../components/Auth/LoginForm.vue'
import RegisterForm from '../components/Auth/RegisterForm.vue'
import AuthPanel from '../components/Auth/AuthPanel.vue'
import { useAuthStore } from '../store/auth'

const isSignUp = ref(false)
const authStore = useAuthStore()

onMounted(() => {
  // Clear any existing errors when guest layout loads
  authStore.clearErrorsAndState()
})

const toggleToLogin = () => {
  isSignUp.value = false
}

const toggleToRegister = () => {
  isSignUp.value = true
}
</script>

<style scoped>

.father {
  height: 100vh;
  background: linear-gradient(135deg, #1a1a1a 0%, #2d1b69 50%, #8e2fe7 100%);
  position: relative;
  overflow: hidden;
}

.father::before {
  content: '';
  position: absolute;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background: radial-gradient(circle at 30% 70%, rgba(217, 70, 239, 0.3) 0%, transparent 50%),
              radial-gradient(circle at 70% 30%, rgba(236, 72, 153, 0.2) 0%, transparent 50%);
  pointer-events: none;
}

.container {
  border-radius: 20px;
  box-shadow: 
    0 25px 50px -12px rgba(0, 0, 0, 0.4),
    0 0 0 1px rgba(255, 255, 255, 0.05);
  position: relative;
  overflow: hidden;
  width: 900px;
  max-width: 95%;
  min-height: 600px;
  background: rgba(255, 255, 255, 0.02);
  backdrop-filter: blur(20px);
  border: 1px solid rgba(255, 255, 255, 0.1);
}

.form-container {
  position: absolute;
  top: 0;
  height: 100%;
  transition: all 0.6s cubic-bezier(0.4, 0, 0.2, 1);
  background: rgba(0, 0, 0, 0.3);
  backdrop-filter: blur(10px);
}

.sign-in-container {
  left: 0;
  width: 50%;
  z-index: 2;
}

.container.right-panel-active .sign-in-container {
  transform: translateX(100%);
  opacity: 0;
}

.sign-up-container {
  left: 0;
  width: 50%;
  opacity: 0;
  z-index: 1;
}

.container.right-panel-active .sign-up-container {
  transform: translateX(100%);
  opacity: 1;
  z-index: 5;
  animation: slideInAuth 0.6s ease-out;
}

@keyframes slideInAuth {
  0% {
    opacity: 0;
    transform: translateX(50%) scale(0.95);
  }
  50% {
    opacity: 0.5;
  }
  100% {
    opacity: 1;
    transform: translateX(100%) scale(1);
  }
}

/* Deep selectors for AuthPanel animations */
.container.right-panel-active :deep(.overlay-container) {
  transform: translateX(-100%);
}

.container.right-panel-active :deep(.overlay) {
  transform: translateX(50%);
}

.container.right-panel-active :deep(.overlay-left) {
  transform: translateX(0);
}

.container.right-panel-active :deep(.overlay-right) {
  transform: translateX(20%);
}

/* Responsive Design */
@media (max-width: 1024px) {
  .container {
    width: 800px;
    min-height: 550px;
  }
}

@media (max-width: 768px) {
  .father {
    height: 100vh;
  }
  
  .container {
    width: 100%;
    height: 100vh;
    border-radius: 0;
    min-height: 100vh;
    max-width: 100%;
  }
  
  .form-container {
    width: 100% !important;
    position: relative;
  }
  
  .sign-in-container,
  .sign-up-container {
    width: 100% !important;
    position: relative;
    transform: none !important;
    opacity: 1 !important;
  }
  
  .container.right-panel-active .sign-in-container {
    display: none;
  }
  
  .sign-up-container {
    display: none;
  }
  
  .container.right-panel-active .sign-up-container {
    display: block;
  }
}

@media (max-width: 480px) {
  .container {
    box-shadow: none;
    background: rgba(0, 0, 0, 0.2);
  }
}

/* Accessibility improvements */
@media (prefers-reduced-motion: reduce) {
  .container,
  .form-container,
  .container.right-panel-active .sign-in-container,
  .container.right-panel-active .sign-up-container,
  .container.right-panel-active :deep(.overlay-container),
  .container.right-panel-active :deep(.overlay),
  .container.right-panel-active :deep(.overlay-left),
  .container.right-panel-active :deep(.overlay-right) {
    transition: none !important;
    animation: none !important;
  }
}
</style>
