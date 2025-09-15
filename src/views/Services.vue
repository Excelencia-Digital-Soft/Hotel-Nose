<template>
  <div class="services-container">
    <!-- Header -->
    <div class="glass-card p-6 mb-6">
      <h1 class="text-2xl font-bold text-white mb-2 flex items-center gap-3">
        <span class="material-symbols-outlined text-3xl">room_service</span>
        Servicios del Hotel
      </h1>
      <p class="text-gray-300">
        Gestiona y visualiza todos los servicios disponibles en el hotel
      </p>
    </div>

    <!-- Services Grid -->
    <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
      <!-- Service Card: Room Service -->
      <div class="glass-card p-6 hover:bg-white/15 transition-all cursor-pointer">
        <div class="flex items-center gap-4 mb-4">
          <div class="w-12 h-12 rounded-full bg-primary-400/20 flex items-center justify-center">
            <span class="material-symbols-outlined text-primary-400">restaurant</span>
          </div>
          <div>
            <h3 class="text-lg font-semibold text-white">Room Service</h3>
            <p class="text-sm text-gray-300">Pedidos a habitación</p>
          </div>
        </div>
        <p class="text-gray-300 text-sm">
          Servicio de comida y bebida directamente a la habitación del huésped.
        </p>
      </div>

      <!-- Service Card: Housekeeping -->
      <div class="glass-card p-6 hover:bg-white/15 transition-all cursor-pointer">
        <div class="flex items-center gap-4 mb-4">
          <div class="w-12 h-12 rounded-full bg-secondary-400/20 flex items-center justify-center">
            <span class="material-symbols-outlined text-secondary-400">cleaning_services</span>
          </div>
          <div>
            <h3 class="text-lg font-semibold text-white">Housekeeping</h3>
            <p class="text-sm text-gray-300">Limpieza de habitaciones</p>
          </div>
        </div>
        <p class="text-gray-300 text-sm">
          Servicio de limpieza y mantenimiento de habitaciones.
        </p>
      </div>

      <!-- Service Card: Concierge -->
      <div class="glass-card p-6 hover:bg-white/15 transition-all cursor-pointer">
        <div class="flex items-center gap-4 mb-4">
          <div class="w-12 h-12 rounded-full bg-accent-400/20 flex items-center justify-center">
            <span class="material-symbols-outlined text-accent-400">support_agent</span>
          </div>
          <div>
            <h3 class="text-lg font-semibold text-white">Concierge</h3>
            <p class="text-sm text-gray-300">Asistencia al huésped</p>
          </div>
        </div>
        <p class="text-gray-300 text-sm">
          Información turística y reservas de actividades.
        </p>
      </div>

      <!-- Service Card: Laundry -->
      <div class="glass-card p-6 hover:bg-white/15 transition-all cursor-pointer">
        <div class="flex items-center gap-4 mb-4">
          <div class="w-12 h-12 rounded-full bg-primary-400/20 flex items-center justify-center">
            <span class="material-symbols-outlined text-primary-400">local_laundry_service</span>
          </div>
          <div>
            <h3 class="text-lg font-semibold text-white">Lavandería</h3>
            <p class="text-sm text-gray-300">Servicio de lavado</p>
          </div>
        </div>
        <p class="text-gray-300 text-sm">
          Servicio de lavado y planchado de ropa para huéspedes.
        </p>
      </div>

      <!-- Service Card: Spa -->
      <div class="glass-card p-6 hover:bg-white/15 transition-all cursor-pointer">
        <div class="flex items-center gap-4 mb-4">
          <div class="w-12 h-12 rounded-full bg-secondary-400/20 flex items-center justify-center">
            <span class="material-symbols-outlined text-secondary-400">spa</span>
          </div>
          <div>
            <h3 class="text-lg font-semibold text-white">Spa & Wellness</h3>
            <p class="text-sm text-gray-300">Relajación y bienestar</p>
          </div>
        </div>
        <p class="text-gray-300 text-sm">
          Servicios de spa, masajes y tratamientos de bienestar.
        </p>
      </div>

      <!-- Service Card: Business Center -->
      <div class="glass-card p-6 hover:bg-white/15 transition-all cursor-pointer">
        <div class="flex items-center gap-4 mb-4">
          <div class="w-12 h-12 rounded-full bg-accent-400/20 flex items-center justify-center">
            <span class="material-symbols-outlined text-accent-400">business_center</span>
          </div>
          <div>
            <h3 class="text-lg font-semibold text-white">Business Center</h3>
            <p class="text-sm text-gray-300">Servicios empresariales</p>
          </div>
        </div>
        <p class="text-gray-300 text-sm">
          Centro de negocios con servicios de impresión y reuniones.
        </p>
      </div>
    </div>

    <!-- Service Stats -->
    <div class="glass-card p-6 mt-6">
      <h2 class="text-xl font-semibold text-white mb-4">
        Estadísticas de Servicios
      </h2>
      <div class="grid grid-cols-2 md:grid-cols-4 gap-4">
        <div class="text-center">
          <div class="text-2xl font-bold text-primary-400">24</div>
          <div class="text-sm text-gray-300">Servicios Activos</div>
        </div>
        <div class="text-center">
          <div class="text-2xl font-bold text-secondary-400">156</div>
          <div class="text-sm text-gray-300">Pedidos Hoy</div>
        </div>
        <div class="text-center">
          <div class="text-2xl font-bold text-accent-400">98%</div>
          <div class="text-sm text-gray-300">Satisfacción</div>
        </div>
        <div class="text-center">
          <div class="text-2xl font-bold text-primary-400">12</div>
          <div class="text-sm text-gray-300">Avg. Tiempo (min)</div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'

// Reactive state
const services = ref([])
const loading = ref(false)

// Lifecycle
onMounted(() => {
  // Initialize services data if needed
  console.log('Services page mounted')
})
</script>

<style scoped>
.services-container {
  @apply container mx-auto px-4 py-6;
}
</style>