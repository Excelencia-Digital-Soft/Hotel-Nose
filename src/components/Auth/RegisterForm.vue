<template>
  <div class="auth-form">
    <div class="auth-header">
      <h1 class="auth-title">¡Crea tu cuenta!</h1>
      <p class="auth-subtitle">Únete a inROOM ahora</p>
    </div>
    
    <form @submit.prevent="handleRegister" class="auth-form-container">
      <!-- Error Messages -->
      <div v-if="hasErrors" class="auth-error">
        <div v-for="error in errors" :key="error" class="error-item">
          {{ error }}
        </div>
      </div>
      
      <!-- Success Message -->
      <div v-if="successMessage" class="auth-success">
        {{ successMessage }}
      </div>
      
      <div class="input-group">
        <input 
          v-model="registerForm.username"
          class="auth-input" 
          type="text" 
          placeholder="Usuario"
          :disabled="isLoading"
          required
        />
      </div>
      
      <div class="input-group">
        <input 
          v-model="registerForm.email"
          class="auth-input" 
          type="email" 
          placeholder="Correo electrónico"
          :disabled="isLoading"
          required
        />
      </div>
      
      <div class="input-group">
        <input 
          v-model="registerForm.name"
          class="auth-input" 
          type="text" 
          placeholder="Nombre completo"
          :disabled="isLoading"
          required
        />
      </div>
      
      <div class="input-group">
        <input 
          v-model="registerForm.password"
          class="auth-input" 
          type="password" 
          placeholder="Contraseña"
          autocomplete="new-password"
          :disabled="isLoading"
          required
          minlength="6"
        />
      </div>
      
      <div class="input-group">
        <input 
          v-model="registerForm.confirmPassword"
          class="auth-input" 
          type="password"
          placeholder="Confirmar contraseña"
          autocomplete="new-password"
          :disabled="isLoading"
          required
        />
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
            aria-label="Creando cuenta..." 
          />
        </div>
        <span :class="{ 'opacity-0': isLoading }">
          Crear nueva cuenta
        </span>
      </button>
      
      <div class="terms-container">
        <p class="terms-text">
          Al crear tu cuenta, aceptas nuestros 
          <a href="#" class="auth-link">Términos y Condiciones</a>
        </p>
      </div>
    </form>
  </div>
</template>

<script setup>
import { ref } from 'vue'
import ProgressSpinner from 'primevue/progressspinner'
import { useAuth } from '../../composables/useAuth'

const { 
  registerForm, 
  isLoading, 
  errors, 
  hasErrors,
  register,
  clearErrors,
  validateRegisterForm
} = useAuth()

const successMessage = ref('')

const handleRegister = async () => {
  clearErrors()
  successMessage.value = ''
  
  // Validar formulario
  const validation = validateRegisterForm()
  if (!validation.isValid) {
    // Mostrar errores de validación
    return
  }
  
  const result = await register()
  
  if (result.success) {
    successMessage.value = result.message || 'Cuenta creada exitosamente'
  }
}
</script>

<style scoped>

.auth-form {
  display: flex;
  flex-direction: column;
  justify-content: center;
  align-items: center;
  padding: 1.5rem;
  height: 100%;
  text-align: center;
  overflow-y: auto;
}

.auth-header {
  margin-bottom: 1.5rem;
}

.auth-title {
  font-size: 1.75rem;
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
  margin-bottom: 0.55rem;
}

.terms-container {
  margin-top: 1rem;
}

.terms-text {
  font-size: 0.75rem;
  color: var(--auth-text-muted);
  margin: 0;
  line-height: 1.4;
}

.error-item {
  margin-bottom: 0.25rem;
}

/* Responsive */
@media (max-width: 640px) {
  .auth-form {
    padding: 1rem;
  }
  
  .auth-title {
    font-size: 1.5rem;
  }
  
  .input-group {
    margin-bottom: 0.5rem;
  }
}
</style>
