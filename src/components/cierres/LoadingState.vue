<template>
  <div class="flex flex-col items-center justify-center py-12">
    <!-- Animated Loading Icon -->
    <div class="relative mb-6">
      <div class="glass-card p-8">
        <div class="relative">
          <!-- Main spinner -->
          <div class="w-16 h-16 border-4 border-primary-400/30 border-t-primary-400 rounded-full animate-spin"></div>
          
          <!-- Inner pulse effect -->
          <div class="absolute inset-0 w-16 h-16 border-2 border-secondary-400/20 rounded-full animate-pulse"></div>
          
          <!-- Center dot -->
          <div class="absolute inset-0 flex items-center justify-center">
            <div class="w-2 h-2 bg-gradient-to-r from-primary-400 to-accent-400 rounded-full animate-bounce"></div>
          </div>
        </div>
      </div>
      
      <!-- Floating particles -->
      <div class="absolute -top-2 -left-2 w-3 h-3 bg-primary-400/60 rounded-full animate-float-1"></div>
      <div class="absolute -top-1 -right-3 w-2 h-2 bg-secondary-400/60 rounded-full animate-float-2"></div>
      <div class="absolute -bottom-2 -left-1 w-2 h-2 bg-accent-400/60 rounded-full animate-float-3"></div>
    </div>

    <!-- Loading Text -->
    <div class="text-center space-y-2">
      <h3 class="text-xl font-semibold text-white flex items-center justify-center">
        <i class="pi pi-spin pi-spinner mr-2 text-primary-400"></i>
        Cargando cierres...
      </h3>
      <p class="text-gray-300 text-sm">
        Obteniendo información de la base de datos 🔄
      </p>
    </div>

    <!-- Progress indicators -->
    <div class="mt-6 flex space-x-2">
      <div class="w-2 h-2 bg-primary-400 rounded-full animate-pulse"></div>
      <div class="w-2 h-2 bg-secondary-400 rounded-full animate-pulse" style="animation-delay: 0.2s"></div>
      <div class="w-2 h-2 bg-accent-400 rounded-full animate-pulse" style="animation-delay: 0.4s"></div>
    </div>
  </div>
</template>

<script setup>
// No props or logic needed for this simple loading component
</script>

<style scoped>
.glass-card {
  background-color: rgba(255, 255, 255, 0.1);
  backdrop-filter: blur(16px);
  border: 1px solid rgba(255, 255, 255, 0.2);
  border-radius: 12px;
}

/* Custom floating animations */
@keyframes float-1 {
  0%, 100% { 
    transform: translateY(0px) translateX(0px); 
    opacity: 0.6;
  }
  50% { 
    transform: translateY(-10px) translateX(-5px); 
    opacity: 1;
  }
}

@keyframes float-2 {
  0%, 100% { 
    transform: translateY(0px) translateX(0px); 
    opacity: 0.4;
  }
  50% { 
    transform: translateY(-15px) translateX(8px); 
    opacity: 0.8;
  }
}

@keyframes float-3 {
  0%, 100% { 
    transform: translateY(0px) translateX(0px); 
    opacity: 0.5;
  }
  50% { 
    transform: translateY(-8px) translateX(-3px); 
    opacity: 0.9;
  }
}

.animate-float-1 {
  animation: float-1 3s ease-in-out infinite;
}

.animate-float-2 {
  animation: float-2 2.5s ease-in-out infinite;
}

.animate-float-3 {
  animation: float-3 3.5s ease-in-out infinite;
}

/* Enhanced pulse animation */
.animate-pulse {
  animation: pulse 2s cubic-bezier(0.4, 0, 0.6, 1) infinite;
}

@keyframes pulse {
  0%, 100% {
    opacity: 1;
  }
  50% {
    opacity: .5;
  }
}

/* Enhanced bounce animation */
.animate-bounce {
  animation: bounce 1s infinite;
}

@keyframes bounce {
  0%, 100% {
    transform: translateY(-25%);
    animation-timing-function: cubic-bezier(0.8, 0, 1, 1);
  }
  50% {
    transform: none;
    animation-timing-function: cubic-bezier(0, 0, 0.2, 1);
  }
}
</style>