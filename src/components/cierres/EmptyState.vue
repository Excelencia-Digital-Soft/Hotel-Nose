<template>
  <div class="flex flex-col items-center justify-center py-16 px-6">
    <!-- Animated Icon Container -->
    <div class="relative mb-8">
      <!-- Main container with glass effect -->
      <div class="glass-card p-12 relative overflow-hidden">
        <!-- Background decoration -->
        <div class="absolute inset-0 bg-gradient-to-br from-primary-400/5 to-secondary-400/5 animate-pulse"></div>
        
        <!-- Main icon -->
        <div class="relative z-10 flex items-center justify-center">
          <div class="bg-gradient-to-br from-neutral-600 to-neutral-700 p-6 rounded-full shadow-2xl">
            <i class="pi pi-inbox text-6xl text-gray-400"></i>
          </div>
        </div>
        
        <!-- Floating decorative elements -->
        <div class="absolute top-4 right-4 w-4 h-4 bg-primary-400/30 rounded-full animate-float-slow"></div>
        <div class="absolute bottom-6 left-6 w-3 h-3 bg-secondary-400/30 rounded-full animate-float-slower"></div>
        <div class="absolute top-1/2 left-4 w-2 h-2 bg-accent-400/30 rounded-full animate-float-fast"></div>
      </div>
    </div>

    <!-- Main Message -->
    <div class="text-center space-y-4 max-w-md">
      <h3 class="text-2xl font-bold text-white mb-2">
        📭 No hay cierres disponibles
      </h3>
      <p class="text-gray-300 text-lg leading-relaxed">
        Aún no se han registrado cierres de caja en el sistema. Los cierres aparecerán aquí una vez que se hayan completado.
      </p>
      
      <!-- Additional Info -->
      <div class="glass-card p-4 mt-6">
        <div class="flex items-center justify-center text-gray-400 text-sm">
          <i class="pi pi-info-circle mr-2"></i>
          <span>Los cierres se generan automáticamente al finalizar una sesión de caja</span>
        </div>
      </div>
    </div>

    <!-- Action Buttons -->
    <div class="flex flex-col sm:flex-row gap-4 mt-8">
      <!-- Refresh Button -->
      <button
        @click="$emit('refresh')"
        class="glass-button px-6 py-3 text-white hover:bg-white/20 transform hover:scale-105 transition-all duration-300 flex items-center justify-center min-w-[140px]"
      >
        <i class="pi pi-refresh mr-2"></i>
        Actualizar
      </button>

      <!-- Help Button -->
      <button
        @click="showHelp"
        class="glass-button px-6 py-3 text-white hover:bg-white/20 transform hover:scale-105 transition-all duration-300 flex items-center justify-center min-w-[140px]"
      >
        <i class="pi pi-question-circle mr-2"></i>
        Ayuda
      </button>
    </div>

    <!-- Help Section (Hidden by default) -->
    <div v-if="showHelpSection" class="mt-8 max-w-lg">
      <div class="glass-card p-6">
        <h4 class="text-lg font-semibold text-white mb-4 flex items-center">
          <i class="pi pi-lightbulb text-yellow-400 mr-2"></i>
          ¿Cómo generar un cierre?
        </h4>
        <div class="space-y-3 text-gray-300 text-sm">
          <div class="flex items-start">
            <span class="text-primary-400 font-bold mr-2">1.</span>
            <span>Realizar transacciones durante el día (reservas, consumos, etc.)</span>
          </div>
          <div class="flex items-start">
            <span class="text-primary-400 font-bold mr-2">2.</span>
            <span>Ir a la sección de gestión de caja</span>
          </div>
          <div class="flex items-start">
            <span class="text-primary-400 font-bold mr-2">3.</span>
            <span>Hacer clic en "Cerrar Caja" y completar la información requerida</span>
          </div>
          <div class="flex items-start">
            <span class="text-primary-400 font-bold mr-2">4.</span>
            <span>El cierre aparecerá automáticamente en esta lista</span>
          </div>
        </div>
        
        <!-- Close Help Button -->
        <button
          @click="hideHelp"
          class="mt-4 w-full glass-button px-4 py-2 text-white hover:bg-white/20 transition-all"
        >
          <i class="pi pi-times mr-2"></i>
          Cerrar ayuda
        </button>
      </div>
    </div>

    <!-- Animated Background Decoration -->
    <div class="absolute inset-0 pointer-events-none overflow-hidden">
      <div class="absolute top-1/4 left-1/4 w-32 h-32 bg-primary-400/5 rounded-full animate-pulse"></div>
      <div class="absolute bottom-1/3 right-1/4 w-24 h-24 bg-secondary-400/5 rounded-full animate-pulse" style="animation-delay: 1s"></div>
      <div class="absolute top-1/2 right-1/3 w-16 h-16 bg-accent-400/5 rounded-full animate-pulse" style="animation-delay: 2s"></div>
    </div>
  </div>
</template>

<script setup>
import { ref, defineEmits } from 'vue';

const emit = defineEmits(['refresh']);

// Local state for help section
const showHelpSection = ref(false);

// Methods
const showHelp = () => {
  showHelpSection.value = true;
};

const hideHelp = () => {
  showHelpSection.value = false;
};
</script>

<style scoped>
.glass-card {
  background-color: rgba(255, 255, 255, 0.1);
  backdrop-filter: blur(16px);
  border: 1px solid rgba(255, 255, 255, 0.2);
  border-radius: 12px;
}

.glass-button {
  background-color: rgba(255, 255, 255, 0.1);
  backdrop-filter: blur(8px);
  border: 1px solid rgba(255, 255, 255, 0.3);
  border-radius: 8px;
  transition: all 0.3s ease;
}

.glass-button:hover {
  background-color: rgba(255, 255, 255, 0.2);
}

/* Custom floating animations */
@keyframes float-slow {
  0%, 100% { 
    transform: translateY(0px) rotate(0deg); 
    opacity: 0.3;
  }
  50% { 
    transform: translateY(-15px) rotate(180deg); 
    opacity: 0.6;
  }
}

@keyframes float-slower {
  0%, 100% { 
    transform: translateY(0px) translateX(0px); 
    opacity: 0.2;
  }
  50% { 
    transform: translateY(-10px) translateX(-8px); 
    opacity: 0.5;
  }
}

@keyframes float-fast {
  0%, 100% { 
    transform: translateY(0px) scale(1); 
    opacity: 0.4;
  }
  50% { 
    transform: translateY(-20px) scale(1.2); 
    opacity: 0.8;
  }
}

.animate-float-slow {
  animation: float-slow 4s ease-in-out infinite;
}

.animate-float-slower {
  animation: float-slower 6s ease-in-out infinite;
}

.animate-float-fast {
  animation: float-fast 2s ease-in-out infinite;
}

/* Enhanced pulse for background elements */
.animate-pulse {
  animation: pulse 3s cubic-bezier(0.4, 0, 0.6, 1) infinite;
}

@keyframes pulse {
  0%, 100% {
    opacity: 0.1;
  }
  50% {
    opacity: 0.3;
  }
}

/* Smooth transitions */
.transition-all {
  transition: all 0.3s cubic-bezier(0.4, 0, 0.2, 1);
}

/* Responsive adjustments */
@media (max-width: 640px) {
  .glass-card {
    margin: 0 1rem;
  }
}
</style>