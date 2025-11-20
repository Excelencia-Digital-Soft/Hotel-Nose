<template>
  <div class="auth-form">
    <div class="auth-header">
      <h1 class="auth-title">¡Bienvenido!</h1>
      <p class="auth-subtitle">Inicia sesión en tu cuenta</p>
    </div>
    
    <form @submit.prevent="handleLogin" class="auth-form-container">
      <!-- Error Messages -->
      <div v-if="hasErrors" class="auth-error">
        <div v-for="error in errors" :key="error" class="error-item">
          {{ error }}
        </div>
      </div>
      
      <div class="input-group">
        <input 
          v-model="loginForm.username"
          class="auth-input" 
          type="text"
          placeholder="Usuario"
          :disabled="isLoading"
          required
        />
      </div>
      
      <div class="input-group">
        <div class="password-input-container">
          <input 
            v-model="loginForm.password"
            class="auth-input password-input" 
            :type="showPassword ? 'text' : 'password'"
            placeholder="Contraseña"
            autocomplete="current-password"
            :disabled="isLoading"
            required
          />
          <button
            type="button"
            @click="showPassword = !showPassword"
            class="password-toggle-btn"
            :disabled="isLoading"
          >
            <i :class="showPassword ? 'fas fa-eye-slash' : 'fas fa-eye'"></i>
          </button>
        </div>
      </div>
      
      <div class="forgot-password-container">
        <a href="#" class="auth-link forgot-password-link">
          ¿Olvidaste tu contraseña?
        </a>
      </div>
      
      <button 
        type="submit"
        class="auth-button"
        :disabled="isLoading"
      >
        <div v-if="isLoading" class="auth-loading-container">
          <ProgressSpinner 
            style="width: 20px; height: 20px" 
            strokeWidth="8" 
            fill="transparent"
            animationDuration=".5s" 
            aria-label="Cargando..." 
          />
        </div>
        <span :class="{ 'opacity-0': isLoading }">
          Iniciar Sesión
        </span>
      </button>
    </form>
    
    <!-- Modal para seleccionar institución -->
    <InstitutionSelector 
      v-if="showInstitutionModal"
      :institutions="institutions"
      @select="handleInstitutionSelect"
      @close="showInstitutionModal = false"
    />
  </div>
</template>

<script setup>
import { ref } from 'vue'
import ProgressSpinner from 'primevue/progressspinner'
import { useAuth } from '../../composables/useAuth'
import InstitutionSelector from './InstitutionSelector.vue'

const { 
  loginForm, 
  isLoading, 
  errors, 
  hasErrors, 
  institutions,
  login,
  selectInstitution,
  clearErrors
} = useAuth()

const showInstitutionModal = ref(false)
const showPassword = ref(false)

const handleLogin = async () => {
  clearErrors()
  
  const result = await login()
  
  if (result.success) {
    // Store user name for toast after redirect
    if (result.userName) {
      sessionStorage.setItem('loginSuccessUser', result.userName)
    }
    
    if (result.multipleInstitutions) {
      showInstitutionModal.value = true
    }
    // Si solo hay una institución, useAuth redirige automáticamente a "/"
  }
}

const handleInstitutionSelect = (institutionId) => {
  selectInstitution(institutionId)
  showInstitutionModal.value = false
  
  // Store user name for toast after redirect (for multiple institutions case)
  const loginUser = sessionStorage.getItem('loginSuccessUser')
  if (loginUser) {
    // Keep the user name for toast after institution selection redirect
    sessionStorage.setItem('loginSuccessUser', loginUser)
  }
}
</script>

<style scoped>

.auth-form {
  display: flex;
  flex-direction: column;
  justify-content: center;
  align-items: center;
  padding: 2rem;
  height: 100%;
  text-align: center;
}

.auth-header {
  margin-bottom: 2rem;
}

.auth-title {
  font-size: 2rem;
  font-weight: 700;
  color: white;
  margin-bottom: 0.5rem;
  text-shadow: 0 2px 4px rgba(0, 0, 0, 0.3);
}

.auth-subtitle {
  color: var(--auth-text-muted);
  font-size: 0.875rem;
  margin: 0;
  font-weight: 400;
}

.input-group {
  margin-bottom: 1rem;
}

.forgot-password-container {
  text-align: right;
  margin-bottom: 2rem;
}

.forgot-password-link {
  font-size: 0.8rem;
  color: var(--auth-text-light);
  font-weight: 500;
  text-shadow: 0 1px 2px rgba(0, 0, 0, 0.3);
}

.forgot-password-link:hover {
  color: var(--auth-primary);
  text-shadow: 0 0 8px var(--auth-input-focus);
}

.error-item {
  margin-bottom: 0.25rem;
}

/* Password input container */
.password-input-container {
  position: relative;
  display: flex;
  align-items: center;
}

.password-input {
  padding-right: 3rem !important;
}

.password-toggle-btn {
  position: absolute;
  right: 0.75rem;
  background: none;
  border: none;
  color: var(--auth-text-muted);
  cursor: pointer;
  padding: 0.5rem;
  font-size: 1rem;
  transition: color 0.3s ease;
  z-index: 1;
}

.password-toggle-btn:hover:not(:disabled) {
  color: var(--auth-text-light);
}

.password-toggle-btn:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}

.password-toggle-btn:focus {
  outline: none;
  color: var(--auth-primary);
}

/* Responsive */
@media (max-width: 640px) {
  .auth-form {
    padding: 1.5rem;
  }
  
  .auth-title {
    font-size: 1.75rem;
  }
}
</style>