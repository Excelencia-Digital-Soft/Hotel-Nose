<template>
  <div class="overlay-container">
    <div class="overlay">
      <div class="overlay-panel overlay-left">
        <div class="panel-content">
          <div class="welcome-back">
            <h2 class="panel-title">¡Bienvenido de vuelta!</h2>
            <p class="panel-description">
              Para mantenerte conectado con nosotros, inicia sesión con tu información personal
            </p>
          </div>
          <button 
            @click="toggleToLogin" 
            class="auth-button-outline switch-button"
            aria-label="Cambiar a iniciar sesión"
          >
            <span>Iniciar sesión</span>
          </button>
        </div>
      </div>
      
      <div class="overlay-panel overlay-right">
        <div class="panel-content">
          <div class="brand-section">
            <div class="brand-logo">
              <span class="brand-text">inROOM</span>
              <div class="logo-icon">
                <img src="../../assets/pin.png" class="logo-image" alt="inROOM">
              </div>
            </div>
            <p class="brand-tagline">
              Tu plataforma de gestión hotelera
            </p>
          </div>
          <button 
            @click="toggleToRegister" 
            class="auth-button-outline switch-button"
            aria-label="Cambiar a crear cuenta"
          >
            <span>Crear nueva cuenta</span>
          </button>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
const emit = defineEmits(['toggle-to-login', 'toggle-to-register'])

const toggleToLogin = () => {
  emit('toggle-to-login')
}

const toggleToRegister = () => {
  emit('toggle-to-register')
}
</script>

<style scoped>

.overlay-container {
  position: absolute;
  top: 0;
  left: 50%;
  width: 50%;
  height: 100%;
  overflow: hidden;
  transition: transform 0.6s cubic-bezier(0.4, 0, 0.2, 1);
  z-index: 100;
}

.overlay {
  background: linear-gradient(135deg, var(--auth-gradient-start), var(--auth-gradient-end));
  background-size: 400% 400%;
  animation: gradientShift 15s ease infinite;
  position: relative;
  left: -100%;
  height: 100%;
  width: 200%;
  transform: translateX(0);
  transition: transform 0.6s cubic-bezier(0.4, 0, 0.2, 1);
  box-shadow: inset 0 0 100px rgba(0, 0, 0, 0.2);
}

@keyframes gradientShift {
  0% { background-position: 0% 50%; }
  50% { background-position: 100% 50%; }
  100% { background-position: 0% 50%; }
}

.overlay-panel {
  position: absolute;
  display: flex;
  align-items: center;
  justify-content: center;
  flex-direction: column;
  padding: 3rem 2.5rem;
  text-align: center;
  top: 0;
  height: 100%;
  width: 50%;
  transform: translateX(0);
  transition: transform 0.6s cubic-bezier(0.4, 0, 0.2, 1);
}

.overlay-left {
  transform: translateX(-20%);
}

.overlay-right {
  right: 0;
  transform: translateX(0);
}

.panel-content {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 2.5rem;
  max-width: 320px;
}

.welcome-back,
.brand-section {
  text-align: center;
}

.panel-title {
  font-size: 2rem;
  font-weight: 800;
  color: white;
  margin-bottom: 1rem;
  text-shadow: 0 4px 8px rgba(0, 0, 0, 0.3);
  line-height: 1.2;
}

.panel-description {
  color: rgba(255, 255, 255, 0.9);
  font-size: 1rem;
  line-height: 1.6;
  margin: 0;
  font-weight: 300;
}

.brand-section {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 1.5rem;
}

.brand-logo {
  display: flex;
  align-items: center;
  gap: 1rem;
  margin-bottom: 0.5rem;
}

.brand-text {
  font-size: 3.5rem;
  font-weight: 900;
  color: white;
  text-shadow: 0 4px 12px rgba(0, 0, 0, 0.4);
  letter-spacing: -0.02em;
}

.logo-icon {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 4rem;
  height: 4rem;
  background: rgba(255, 255, 255, 0.1);
  border-radius: 50%;
  backdrop-filter: blur(10px);
  border: 1px solid rgba(255, 255, 255, 0.2);
}

.logo-image {
  width: 2.5rem;
  height: 2.5rem;
  filter: invert(1) brightness(1.2);
  transition: transform 0.3s ease;
}

.brand-tagline {
  color: rgba(255, 255, 255, 0.95);
  font-size: 1rem;
  margin: 0;
  font-weight: 500;
  font-style: italic;
  text-shadow: 0 1px 3px rgba(0, 0, 0, 0.3);
}

.switch-button {
  min-width: 200px;
  padding: 1rem 2rem;
  font-size: 1rem;
  font-weight: 600;
  border-width: 2px;
  transition: all 0.3s cubic-bezier(0.4, 0, 0.2, 1);
}

.switch-button:hover {
  transform: translateY(-2px);
  box-shadow: 0 8px 25px rgba(255, 255, 255, 0.2);
  background: rgba(255, 255, 255, 0.15);
}

/* Responsive Design */
@media (max-width: 1024px) {
  .panel-content {
    gap: 2rem;
    max-width: 280px;
  }
  
  .panel-title {
    font-size: 1.75rem;
  }
  
  .brand-text {
    font-size: 3rem;
  }
}

@media (max-width: 768px) {
  .overlay-panel {
    padding: 2rem 1.5rem;
  }
  
  .panel-content {
    gap: 1.5rem;
    max-width: 250px;
  }
  
  .panel-title {
    font-size: 1.5rem;
  }
  
  .panel-description {
    font-size: 0.9rem;
  }
  
  .brand-text {
    font-size: 2.5rem;
  }
  
  .logo-icon {
    width: 3rem;
    height: 3rem;
  }
  
  .logo-image {
    width: 2rem;
    height: 2rem;
  }
  
  .switch-button {
    min-width: 160px;
    padding: 0.875rem 1.5rem;
    font-size: 0.9rem;
  }
}

@media (max-width: 480px) {
  .overlay-panel {
    padding: 1.5rem 1rem;
  }
  
  .brand-text {
    font-size: 2rem;
  }
  
  .panel-title {
    font-size: 1.25rem;
  }
}

/* Accessibility */
@media (prefers-reduced-motion: reduce) {
  .overlay,
  .logo-image,
  .switch-button {
    animation: none !important;
    transition: none !important;
  }
}

/* High contrast mode */
@media (prefers-contrast: high) {
  .overlay {
    background: linear-gradient(135deg, #000000, #333333);
  }
  
  .panel-title,
  .brand-text {
    text-shadow: 2px 2px 4px #000000;
  }
}
</style>
