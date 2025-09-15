<template>
  <div class="flex flex-col items-center justify-center py-16 px-6">
    <!-- Animated Security Icon Container -->
    <div class="relative mb-8">
      <!-- Main container with glass effect -->
      <div class="glass-card p-12 relative overflow-hidden">
        <!-- Background warning pattern -->
        <div class="absolute inset-0 bg-gradient-to-br from-red-500/10 to-orange-500/5"></div>
        
        <!-- Animated warning stripes -->
        <div class="absolute inset-0 opacity-20">
          <div class="h-full w-full bg-gradient-to-r from-transparent via-red-500/20 to-transparent transform skew-x-12 animate-slide"></div>
        </div>
        
        <!-- Main security icon -->
        <div class="relative z-10 flex items-center justify-center">
          <div class="bg-gradient-to-br from-red-500 to-orange-500 p-6 rounded-full shadow-2xl animate-pulse-gentle">
            <i class="pi pi-lock text-6xl text-white"></i>
          </div>
        </div>
        
        <!-- Floating warning elements -->
        <div class="absolute top-4 right-4 w-4 h-4 bg-red-400/40 rounded-full animate-warning-1"></div>
        <div class="absolute bottom-6 left-6 w-3 h-3 bg-orange-400/40 rounded-full animate-warning-2"></div>
        <div class="absolute top-1/2 left-4 w-2 h-2 bg-yellow-400/40 rounded-full animate-warning-3"></div>
      </div>
    </div>

    <!-- Main Message -->
    <div class="text-center space-y-4 max-w-md">
      <h3 class="text-2xl font-bold text-white mb-2">
        🔒 Acceso Restringido
      </h3>
      <p class="text-gray-300 text-lg leading-relaxed">
        No tienes permisos para ver el historial de cierres. Esta funcionalidad está disponible solo para <strong class="text-red-400">Administradores</strong> y <strong class="text-orange-400">Directores</strong>.
      </p>
      
      <!-- Current Session Indicator -->
      <div class="glass-card p-4 mt-6 border border-blue-500/30">
        <div class="flex items-center justify-center text-blue-400 text-sm">
          <i class="pi pi-info-circle mr-2"></i>
          <span>Puedes ver únicamente la sesión actual de caja</span>
        </div>
      </div>
    </div>

    <!-- Required Permissions -->
    <div class="mt-8 max-w-lg">
      <div class="glass-card p-6 border border-red-500/20">
        <h4 class="text-lg font-semibold text-white mb-4 flex items-center">
          <i class="pi pi-shield text-red-400 mr-2"></i>
          Permisos Requeridos
        </h4>
        <div class="space-y-3">
          <!-- Administrator Role -->
          <div class="flex items-center p-3 bg-red-500/10 rounded-lg border border-red-500/20">
            <i class="pi pi-crown text-red-400 text-lg mr-3"></i>
            <div>
              <p class="text-red-400 font-semibold">Administrador</p>
              <p class="text-gray-300 text-sm">Acceso completo al historial de cierres</p>
            </div>
          </div>
          
          <!-- Director Role -->
          <div class="flex items-center p-3 bg-orange-500/10 rounded-lg border border-orange-500/20">
            <i class="pi pi-star text-orange-400 text-lg mr-3"></i>
            <div>
              <p class="text-orange-400 font-semibold">Director</p>
              <p class="text-gray-300 text-sm">Supervisión y control de cierres</p>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- Action Buttons -->
    <div class="flex flex-col sm:flex-row gap-4 mt-8">
      <!-- Contact Admin Button -->
      <button
        @click="contactAdmin"
        class="glass-button px-6 py-3 text-white hover:bg-white/20 transform hover:scale-105 transition-all duration-300 flex items-center justify-center min-w-[160px]"
      >
        <i class="pi pi-envelope mr-2"></i>
        Contactar Admin
      </button>

      <!-- Go Back Button -->
      <button
        @click="goBack"
        class="glass-button px-6 py-3 text-white hover:bg-white/20 transform hover:scale-105 transition-all duration-300 flex items-center justify-center min-w-[160px]"
      >
        <i class="pi pi-arrow-left mr-2"></i>
        Volver
      </button>
    </div>

    <!-- Help Section -->
    <div class="mt-8 max-w-lg">
      <div class="glass-card p-4 border border-blue-500/20">
        <div class="flex items-start text-blue-400 text-sm">
          <i class="pi pi-question-circle mr-2 mt-0.5"></i>
          <div>
            <p class="font-semibold mb-1">¿Necesitas acceso?</p>
            <p class="text-gray-300">
              Contacta con tu administrador del sistema para solicitar los permisos necesarios para acceder al historial de cierres.
            </p>
          </div>
        </div>
      </div>
    </div>

    <!-- Background Security Pattern -->
    <div class="absolute inset-0 pointer-events-none overflow-hidden opacity-10">
      <div class="absolute top-1/4 left-1/4 w-32 h-32 border-2 border-red-500 rounded-full animate-pulse-slow"></div>
      <div class="absolute bottom-1/3 right-1/4 w-24 h-24 border-2 border-orange-500 rounded-full animate-pulse-slow" style="animation-delay: 1.5s"></div>
      <div class="absolute top-1/2 right-1/3 w-16 h-16 border-2 border-yellow-500 rounded-full animate-pulse-slow" style="animation-delay: 3s"></div>
    </div>
  </div>
</template>

<script setup>
import { useRouter } from 'vue-router';
import { useToast } from 'primevue/usetoast';

const router = useRouter();
const toast = useToast();

// Methods
const contactAdmin = () => {
  toast.add({
    severity: 'info',
    summary: 'Contactar Administrador',
    detail: 'Por favor, contacta con tu administrador del sistema para solicitar permisos de acceso.',
    life: 5000,
  });
};

const goBack = () => {
  router.go(-1); // Go back to previous page
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

/* Custom warning animations */
@keyframes warning-1 {
  0%, 100% { 
    transform: translateY(0px) scale(1); 
    opacity: 0.4;
  }
  50% { 
    transform: translateY(-10px) scale(1.3); 
    opacity: 0.8;
  }
}

@keyframes warning-2 {
  0%, 100% { 
    transform: translateY(0px) translateX(0px); 
    opacity: 0.3;
  }
  50% { 
    transform: translateY(-8px) translateX(-5px); 
    opacity: 0.7;
  }
}

@keyframes warning-3 {
  0%, 100% { 
    transform: translateY(0px) rotate(0deg); 
    opacity: 0.5;
  }
  50% { 
    transform: translateY(-12px) rotate(180deg); 
    opacity: 0.9;
  }
}

.animate-warning-1 {
  animation: warning-1 2s ease-in-out infinite;
}

.animate-warning-2 {
  animation: warning-2 2.5s ease-in-out infinite;
}

.animate-warning-3 {
  animation: warning-3 3s ease-in-out infinite;
}

/* Gentle pulse for main icon */
@keyframes pulse-gentle {
  0%, 100% {
    transform: scale(1);
    opacity: 1;
  }
  50% {
    transform: scale(1.05);
    opacity: 0.9;
  }
}

.animate-pulse-gentle {
  animation: pulse-gentle 3s ease-in-out infinite;
}

/* Sliding warning stripe */
@keyframes slide {
  0% {
    transform: translateX(-100%) skewX(12deg);
  }
  100% {
    transform: translateX(100%) skewX(12deg);
  }
}

.animate-slide {
  animation: slide 3s ease-in-out infinite;
}

/* Slow pulse for security borders */
@keyframes pulse-slow {
  0%, 100% {
    opacity: 0.1;
    transform: scale(1);
  }
  50% {
    opacity: 0.3;
    transform: scale(1.1);
  }
}

.animate-pulse-slow {
  animation: pulse-slow 4s ease-in-out infinite;
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