<template>
  <div class="min-h-screen bg-gradient-to-br from-surface-950 via-surface-900 to-surface-950 relative overflow-hidden">
    <!-- Efectos de fondo decorativos -->
    <div class="absolute inset-0 overflow-hidden pointer-events-none">
      <div class="absolute top-1/4 -left-48 w-96 h-96 bg-primary-500/5 rounded-full blur-3xl"></div>
      <div class="absolute bottom-1/4 -right-48 w-96 h-96 bg-secondary-500/5 rounded-full blur-3xl"></div>
      <div class="absolute top-3/4 left-1/3 w-64 h-64 bg-accent-500/3 rounded-full blur-2xl"></div>
    </div>

    <div class="relative z-10 p-6">
      <!-- Header premium con glassmorphism -->
      <div class="mb-8">
        <div class="bg-white/5 backdrop-blur-xl border border-white/10 rounded-3xl shadow-2xl transition-all duration-300" :class="compactMode ? 'p-4' : 'p-6'">
          <div class="flex flex-col lg:flex-row justify-between items-start lg:items-center" :class="compactMode ? 'gap-3' : 'gap-6'">
            <!-- Título principal -->
            <div class="flex items-center" :class="compactMode ? 'gap-3' : 'gap-4'">
              <div class="bg-gradient-to-r from-primary-400 via-secondary-400 to-accent-400 rounded-2xl flex items-center justify-center shadow-lg transition-all duration-300" :class="compactMode ? 'w-12 h-12' : 'w-16 h-16'">
                <span class="material-symbols-outlined text-white" :class="compactMode ? 'text-xl' : 'text-2xl'">hotel</span>
              </div>
              <div v-if="!compactMode">
                <h1 class="text-4xl font-bold bg-gradient-to-r from-primary-300 via-secondary-300 to-accent-300 bg-clip-text text-transparent lexend-exa">
                  Panel de Habitaciones
                </h1>
                <p class="text-gray-400 text-sm mt-1">Gestión completa de alojamiento</p>
              </div>
              <div v-else>
                <h1 class="text-2xl font-bold bg-gradient-to-r from-primary-300 via-secondary-300 to-accent-300 bg-clip-text text-transparent lexend-exa">
                  Habitaciones
                </h1>
              </div>
            </div>
            
            <!-- Controles mejorados -->
            <div class="flex flex-wrap" :class="compactMode ? 'gap-2' : 'gap-3'">
              <!-- Búsqueda con efecto glassmorphism -->
              <div class="relative group">
                <div class="absolute inset-0 bg-gradient-to-r from-primary-500/20 to-secondary-500/20 rounded-2xl blur opacity-0 group-hover:opacity-100 transition-opacity duration-300"></div>
                <div class="relative bg-white/10 backdrop-blur-md border border-white/20 rounded-2xl flex items-center hover:bg-white/15 transition-all duration-300" :class="compactMode ? 'px-3 py-2 gap-2' : 'px-4 py-3 gap-3'">
                  <span class="material-symbols-outlined text-primary-300" :class="compactMode ? 'text-lg' : ''">search</span>
                  <input 
                    v-model="searchTerm" 
                    type="text" 
                    :placeholder="compactMode ? 'Buscar...' : 'Buscar habitación...'" 
                    class="bg-transparent text-white placeholder-gray-400 border-none outline-none text-sm" :class="compactMode ? 'w-32' : 'w-40'"
                  >
                </div>
              </div>
              
              <!-- Filtro de categoría -->
              <div class="relative group">
                <div class="absolute inset-0 bg-gradient-to-r from-secondary-500/20 to-accent-500/20 rounded-2xl blur opacity-0 group-hover:opacity-100 transition-opacity duration-300"></div>
                <div class="relative bg-white/10 backdrop-blur-md border border-white/20 rounded-2xl flex items-center hover:bg-white/15 transition-all duration-300" :class="compactMode ? 'px-3 py-2 gap-2' : 'px-4 py-3 gap-3'">
                  <span class="material-symbols-outlined text-secondary-300" :class="compactMode ? 'text-lg' : ''">filter_list</span>
                  <select v-model="filtroCategoria" class="bg-transparent text-gray-800 border-none outline-none text-sm cursor-pointer">
                    <option value="">{{ compactMode ? 'Todas' : 'Todas las categorías' }}</option>
                    <option value="CLASICA">Clásica</option>
                    <option value="SUITE">Suite</option>
                    <option value="MASTER SUITE">Master Suite</option>
                    <option value="HIDRO SUITE">Hidro Suite</option>
                    <option value="HIDROMAX SUITE">Hidromax Suite</option>
                    <option value="PENTHOUSE">Penthouse</option>
                  </select>
                </div>
              </div>
              
              <!-- Filtro solo ocupadas -->
              <div class="relative group">
                <div class="absolute inset-0 bg-gradient-to-r from-red-500/20 to-rose-500/20 rounded-2xl blur opacity-0 group-hover:opacity-100 transition-opacity duration-300"></div>
                <div class="relative bg-white/10 backdrop-blur-md border border-white/20 rounded-2xl flex items-center hover:bg-white/15 transition-all duration-300" :class="compactMode ? 'px-3 py-2 gap-2' : 'px-4 py-3 gap-3'">
                  <span class="material-symbols-outlined text-red-300" :class="compactMode ? 'text-lg' : ''">hotel</span>
                  <label class="flex items-center gap-2 cursor-pointer">
                    <input 
                      type="checkbox" 
                      v-model="soloOcupadas" 
                      class="w-4 h-4 text-red-500 bg-transparent border-2 border-red-400 rounded focus:ring-red-500 focus:ring-2"
                    >
                    <span class="text-white text-sm font-medium">{{ compactMode ? 'Ocupadas' : 'Filtrar habitaciones ocupadas' }}</span>
                  </label>
                </div>
              </div>
              
              <!-- Toggle de vista -->
              <button 
                @click="toggleView" 
                class="relative group bg-gradient-to-r from-primary-500 to-secondary-500 hover:from-primary-400 hover:to-secondary-400 rounded-2xl flex items-center transition-all duration-300 shadow-lg hover:shadow-primary-500/25 hover:scale-105" :class="compactMode ? 'px-4 py-2 gap-2' : 'px-6 py-3 gap-3'"
              >
                <span class="material-symbols-outlined text-white" :class="compactMode ? 'text-lg' : ''">{{ viewMode === 'grid' ? 'view_list' : 'grid_view' }}</span>
                <span v-if="!compactMode" class="text-white font-medium">{{ viewMode === 'grid' ? 'Lista' : 'Grid' }}</span>
              </button>
              
              <!-- Toggle modo compacto -->
              <button 
                @click="toggleCompact" 
                class="relative group bg-gradient-to-r from-purple-500 to-violet-500 hover:from-purple-400 hover:to-violet-400 rounded-2xl flex items-center transition-all duration-300 shadow-lg hover:shadow-purple-500/25 hover:scale-105" :class="compactMode ? 'px-4 py-2 gap-2' : 'px-6 py-3 gap-3'"
              >
                <span class="material-symbols-outlined text-white" :class="compactMode ? 'text-lg' : ''">{{ compactMode ? 'unfold_more' : 'unfold_less' }}</span>
                <span v-if="!compactMode" class="text-white font-medium">{{ compactMode ? 'Expandir' : 'Compacto' }}</span>
              </button>
            </div>
          </div>

          <!-- Dashboard de estadísticas mejorado -->
          <div v-if="!compactMode" class="grid grid-cols-2 md:grid-cols-3 gap-4 mt-8">
            <!-- Estadísticas normales -->
            <div class="group relative">
              <div class="absolute inset-0 bg-gradient-to-r from-green-500/20 to-emerald-500/20 rounded-2xl blur opacity-75 group-hover:opacity-100 transition-opacity duration-300"></div>
              <div class="relative bg-white/5 backdrop-blur-md border border-green-500/20 rounded-2xl p-6 hover:bg-white/10 transition-all duration-300">
                <div class="flex items-center gap-4">
                  <div class="w-12 h-12 bg-green-500/20 backdrop-blur-md rounded-xl flex items-center justify-center border border-green-500/30">
                    <span class="material-symbols-outlined text-green-300">hotel_class</span>
                  </div>
                  <div>
                    <p class="text-3xl font-bold text-green-300">{{ habitacionesLibres.length }}</p>
                    <p class="text-green-200/70 text-sm font-medium">Habitaciones Libres</p>
                  </div>
                </div>
              </div>
            </div>
            
            <div class="group relative">
              <div class="absolute inset-0 bg-gradient-to-r from-red-500/20 to-rose-500/20 rounded-2xl blur opacity-75 group-hover:opacity-100 transition-opacity duration-300"></div>
              <div class="relative bg-white/5 backdrop-blur-md border border-red-500/20 rounded-2xl p-6 hover:bg-white/10 transition-all duration-300">
                <div class="flex items-center gap-4">
                  <div class="w-12 h-12 bg-red-500/20 backdrop-blur-md rounded-xl flex items-center justify-center border border-red-500/30">
                    <span class="material-symbols-outlined text-red-300">hotel</span>
                  </div>
                  <div>
                    <p class="text-3xl font-bold text-red-300">{{ habitacionesOcupadas.length }}</p>
                    <p class="text-red-200/70 text-sm font-medium">Habitaciones Ocupadas</p>
                  </div>
                </div>
              </div>
            </div>
            
            <div class="group relative">
              <div class="absolute inset-0 bg-gradient-to-r from-yellow-500/20 to-amber-500/20 rounded-2xl blur opacity-75 group-hover:opacity-100 transition-opacity duration-300"></div>
              <div class="relative bg-white/5 backdrop-blur-md border border-yellow-500/20 rounded-2xl p-6 hover:bg-white/10 transition-all duration-300">
                <div class="flex items-center gap-4">
                  <div class="w-12 h-12 bg-yellow-500/20 backdrop-blur-md rounded-xl flex items-center justify-center border border-yellow-500/30">
                    <span class="material-symbols-outlined text-yellow-300">schedule</span>
                  </div>
                  <div>
                    <p class="text-3xl font-bold text-yellow-300">{{ habitacionesProximasVencer.length }}</p>
                    <p class="text-yellow-200/70 text-sm font-medium">Por Vencer</p>
                  </div>
                </div>
              </div>
            </div>
          </div>
          
          <!-- Dashboard compacto -->
          <div v-else class="flex flex-wrap gap-3 mt-4">
            <div class="bg-white/5 backdrop-blur-md border border-green-500/20 rounded-xl px-4 py-2 flex items-center gap-3">
              <span class="material-symbols-outlined text-green-300 text-lg">hotel_class</span>
              <div>
                <span class="text-xl font-bold text-green-300">{{ habitacionesLibres.length }}</span>
                <span class="text-green-200/70 text-xs ml-1">Libres</span>
              </div>
            </div>
            
            <div class="bg-white/5 backdrop-blur-md border border-red-500/20 rounded-xl px-4 py-2 flex items-center gap-3">
              <span class="material-symbols-outlined text-red-300 text-lg">hotel</span>
              <div>
                <span class="text-xl font-bold text-red-300">{{ habitacionesOcupadas.length }}</span>
                <span class="text-red-200/70 text-xs ml-1">Ocupadas</span>
              </div>
            </div>
            
            <div class="bg-white/5 backdrop-blur-md border border-yellow-500/20 rounded-xl px-4 py-2 flex items-center gap-3">
              <span class="material-symbols-outlined text-yellow-300 text-lg">schedule</span>
              <div>
                <span class="text-xl font-bold text-yellow-300">{{ habitacionesProximasVencer.length }}</span>
                <span class="text-yellow-200/70 text-xs ml-1">Por Vencer</span>
              </div>
            </div>
          </div>
        </div>
      </div>

      <!-- Vista Grid Premium -->
      <div v-if="viewMode === 'grid'" class="grid grid-cols-2 gap-8">
        <!-- Panel Habitaciones Libres -->
        <div class="space-y-6">
          <div class="bg-white/5 backdrop-blur-xl border border-white/10 rounded-3xl p-6 shadow-2xl">
            <div class="flex items-center gap-4 mb-6">
              <div class="w-12 h-12 bg-gradient-to-r from-green-400 to-emerald-500 rounded-2xl flex items-center justify-center shadow-lg">
                <span class="material-symbols-outlined text-white">hotel_class</span>
              </div>
              <div>
                <h2 class="text-2xl text-white lexend-exa font-bold">Habitaciones Libres</h2>
                <p class="text-gray-400 text-sm">{{ habitacionesLibresFiltradas.length }} disponibles</p>
              </div>
            </div>
            
            <div class="grid grid-cols-2 lg:grid-cols-3 xl:grid-cols-3 2xl:grid-cols-3 gap-4 min-h-[100px] overflow-y-auto pr-2 custom-scrollbar">
              <div 
                v-for="habitacion in habitacionesLibresFiltradas" 
                :key="habitacion.habitacionId"
                @click="toggleModalLibre(habitacion)"
                class="group relative overflow-hidden rounded-2xl bg-gradient-to-br from-white/10 to-white/5 border border-white/20 hover:border-green-400/50 transition-all duration-500 cursor-pointer backdrop-blur-md hover:scale-105 hover:shadow-2xl hover:shadow-green-500/25"
              >
                <!-- Efecto de brillo en hover -->
                <div class="absolute inset-0 bg-gradient-to-r from-transparent via-green-400/10 to-transparent -translate-x-full group-hover:translate-x-full transition-transform duration-1000"></div>
                
                <!-- Badge de categoría -->
                <div class="absolute top-3 right-3 z-10">
                  <span class="px-2 py-1 bg-green-500/20 border border-green-400/30 rounded-lg text-green-300 text-xs font-medium backdrop-blur-sm">
                    {{ getCategoryFromName(habitacion.nombreHabitacion) }}
                  </span>
                </div>

                <!-- Indicador de estado -->
                <div class="absolute top-3 left-3 z-10">
                  <div class="w-3 h-3 bg-green-400 rounded-full animate-pulse shadow-lg shadow-green-400/50"></div>
                </div>
                
                <div class="relative p-5 h-full flex flex-col">
                  <!-- Icono de la habitación -->
                  <div class="w-12 h-12 mb-4 bg-gradient-to-r from-green-400/20 to-emerald-500/20 rounded-xl flex items-center justify-center border border-green-400/30 group-hover:scale-110 transition-transform duration-300">
                    <span class="material-symbols-outlined text-green-300 text-xl">{{ getRoomIcon(habitacion.nombreHabitacion) }}</span>
                  </div>

                  <!-- Nombre de la habitación -->
                  <h3 class="text-white font-bold text-lg mb-2 group-hover:text-green-300 transition-colors duration-300">
                    {{ getRoomNumber(habitacion.nombreHabitacion) }}
                  </h3>
                  
                  <div class="text-sm text-gray-400 mb-1">
                    {{ getRoomType(habitacion.nombreHabitacion) }}
                  </div>

                  <!-- Precio -->
                  <div class="mt-auto pt-4 border-t border-white/10">
                    <div class="flex items-center justify-between">
                      <div class="text-gray-400 text-sm">Precio/hora</div>
                      <div class="text-green-300 font-bold text-lg">${{ habitacion.precio }}</div>
                    </div>
                  </div>

                  <!-- Botón de acción sutil -->
                  <div class="mt-3 opacity-0 group-hover:opacity-100 transition-opacity duration-300">
                    <div class="flex items-center justify-center gap-2 text-green-300 text-sm font-medium py-2 bg-green-500/10 rounded-xl border border-green-400/30">
                      <span class="material-symbols-outlined text-sm">add_circle</span>
                      <span>Ocupar</span>
                    </div>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>

        <!-- Panel Habitaciones Ocupadas -->
        <div class="space-y-6">
          <div class="bg-white/5 backdrop-blur-xl border border-white/10 rounded-3xl p-6 shadow-2xl">
            <div class="flex items-center gap-4 mb-6">
              <div class="w-12 h-12 bg-gradient-to-r from-red-400 to-rose-500 rounded-2xl flex items-center justify-center shadow-lg">
                <span class="material-symbols-outlined text-white">hotel</span>
              </div>
              <div>
                <h2 class="text-2xl text-white lexend-exa font-bold">Habitaciones Ocupadas</h2>
                <p class="text-gray-400 text-sm">{{ habitacionesOcupadasFiltradas.length }} en uso</p>
              </div>
            </div>

            <div class="grid grid-cols-2 lg:grid-cols-3 xl:grid-cols-3 2xl:grid-cols-3 gap-4 min-h-[100px] overflow-y-auto pr-2 custom-scrollbar">
              <div 
                v-for="habitacion in habitacionesOcupadasFiltradas" 
                :key="habitacion.habitacionId" 
                @click="toggleModal(habitacion)"
                class="group relative overflow-hidden rounded-2xl transition-all duration-500 cursor-pointer backdrop-blur-md hover:scale-105 hover:shadow-2xl"
                :class="getOccupiedRoomStyles(habitacion)"
              >
                <!-- Indicador de pedidos pendientes -->
                <div v-if="habitacion.pedidosPendientes" 
                     class="absolute -top-1 -right-1 z-20">
                  <div class="bg-red-500 text-white p-2 rounded-full animate-bounce shadow-lg">
                    <span class="material-symbols-outlined text-sm">notifications_active</span>
                  </div>
                </div>

                <!-- Badge de categoría -->
                <div class="absolute top-3 right-3 z-10">
                  <span class="px-2 py-1 bg-black/30 border border-white/20 rounded-lg text-white text-xs font-medium backdrop-blur-sm">
                    {{ getCategoryFromName(habitacion.nombreHabitacion) }}
                  </span>
                </div>

                <!-- Indicador de estado con animación -->
                <div class="absolute top-3 left-3 z-10">
                  <div :class="getStatusIndicator(habitacion)"></div>
                </div>

                <!-- Efecto de brillo en hover -->
                <div class="absolute inset-0 bg-gradient-to-r from-transparent via-white/10 to-transparent -translate-x-full group-hover:translate-x-full transition-transform duration-1000"></div>
                
                <div class="relative p-5 h-full flex flex-col">
                  <!-- Icono de la habitación -->
                  <div class="w-12 h-12 mb-4 bg-black/20 backdrop-blur-sm rounded-xl flex items-center justify-center border border-white/20 group-hover:scale-110 transition-transform duration-300">
                    <span class="material-symbols-outlined text-white text-xl">{{ getRoomIcon(habitacion.nombreHabitacion) }}</span>
                  </div>

                  <!-- Nombre de la habitación -->
                  <h3 class="text-white font-bold text-lg mb-2">
                    {{ getRoomNumber(habitacion.nombreHabitacion) }}
                  </h3>
                  
                  <div class="text-sm text-gray-300 mb-3">
                    {{ getRoomType(habitacion.nombreHabitacion) }}
                  </div>

                  <!-- Información del cliente -->
                  <div class="space-y-2 mb-4 flex-1">
                    <div v-if="habitacion.visita?.identificador" class="flex items-center gap-2 text-gray-300 text-sm">
                      <span class="material-symbols-outlined text-xs">person</span>
                      <span class="truncate">{{ habitacion.visita.identificador }}</span>
                    </div>
                    <div class="flex items-center gap-2 text-sm" :class="getTimeTextColor(habitacion)">
                      <span class="material-symbols-outlined text-xs">schedule</span>
                      <span class="font-medium">{{ getTimeRemaining(habitacion) }}</span>
                    </div>
                  </div>

                  <!-- Barra de progreso mejorada -->
                  <div class="mb-4">
                    <div class="w-full bg-black/30 rounded-full h-2 overflow-hidden">
                      <div 
                        class="h-2 rounded-full transition-all duration-1000 ease-out" 
                        :class="getProgressBarColor(habitacion)"
                        :style="{ width: getTimeProgress(habitacion) + '%' }"
                      ></div>
                    </div>
                    <div class="flex justify-between text-xs text-gray-400 mt-1">
                      <span>{{ getStatusText(habitacion) }}</span>
                      <span>{{ Math.round(getTimeProgress(habitacion)) }}%</span>
                    </div>
                  </div>

                  <!-- Botón de gestión -->
                  <div class="opacity-0 group-hover:opacity-100 transition-opacity duration-300">
                    <div class="flex items-center justify-center gap-2 text-white text-sm font-medium py-2 bg-black/20 rounded-xl border border-white/20 hover:bg-black/30 transition-colors">
                      <span class="material-symbols-outlined text-sm">manage_accounts</span>
                      <span>Gestionar</span>
                    </div>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>

      <!-- Vista Lista Premium (alternativa) -->
      <div v-else class="space-y-6 grid grid-cols-2 gap-4 content-start">
        <!-- Lista de habitaciones libres -->
        <div class="bg-white/5 backdrop-blur-xl border border-white/10 rounded-3xl overflow-hidden shadow-2xl">
          <div class="p-6 bg-gradient-to-r from-green-600/20 to-emerald-600/20 border-b border-green-500/30">
            <h3 class="text-white font-bold text-xl flex items-center gap-3">
              <span class="material-symbols-outlined">hotel_class</span>
              Habitaciones Libres ({{ habitacionesLibresFiltradas.length }})
            </h3>
          </div>
          <div class="p-6">
            <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 xl:grid-cols-3 gap-4">
              <div 
                v-for="habitacion in habitacionesLibresFiltradas" 
                :key="habitacion.habitacionId"
                @click="toggleModalLibre(habitacion)"
                class="group flex items-center gap-4 p-4 bg-white/5 backdrop-blur-md rounded-2xl hover:bg-green-500/10 transition-all duration-300 cursor-pointer border border-white/10 hover:border-green-400/50 hover:scale-105"
              >
                <div class="w-3 h-3 bg-green-400 rounded-full animate-pulse"></div>
                <div class="flex-1">
                  <span class="text-white font-medium block">{{ getRoomNumber(habitacion.nombreHabitacion) }}</span>
                  <span class="text-gray-400 text-sm">{{ getCategoryFromName(habitacion.nombreHabitacion) }}</span>
                </div>
                <span class="text-green-300 text-sm font-bold">${{ habitacion.precio }}/h</span>
              </div>
            </div>
          </div>
        </div>

        <!-- Lista de habitaciones ocupadas -->
        <div class="bg-white/5 backdrop-blur-xl border border-white/10 rounded-3xl overflow-hidden shadow-2xl content-start">
          <div class="p-6 bg-gradient-to-r from-red-600/20 to-rose-600/20 border-b border-red-500/30">
            <h3 class="text-white font-bold text-xl flex items-center gap-3">
              <span class="material-symbols-outlined">hotel</span>
              Habitaciones Ocupadas ({{ habitacionesOcupadasFiltradas.length }})
            </h3>
          </div>
          <div class="p-6">
            <div class="space-y-3 overflow-y-auto custom-scrollbar">
              <div 
                v-for="habitacion in habitacionesOcupadasFiltradas" 
                :key="habitacion.habitacionId" 
                @click="toggleModal(habitacion)"
                class="group flex items-center gap-4 p-4 bg-white/5 backdrop-blur-md rounded-2xl hover:bg-white/10 transition-all duration-300 cursor-pointer border border-white/10 hover:border-gray-400/50 hover:scale-[1.02]"
              >
                <div :class="getStatusIndicator(habitacion)"></div>
                <div class="flex-1">
                  <div class="flex items-center gap-3 mb-1">
                    <span class="text-white font-medium">{{ getRoomNumber(habitacion.nombreHabitacion) }}</span>
                    <span v-if="habitacion.pedidosPendientes" class="text-red-400 animate-pulse">
                      <span class="material-symbols-outlined text-sm">notifications_active</span>
                    </span>
                  </div>
                  <div class="text-gray-400 text-sm">
                    {{ habitacion.visita?.identificador || 'Sin cliente' }} • {{ getCategoryFromName(habitacion.nombreHabitacion) }}
                  </div>
                </div>
                <div class="text-right">
                  <div class="text-sm font-medium" :class="getTimeTextColor(habitacion)">{{ getTimeRemaining(habitacion) }}</div>
                  <div class="text-xs text-gray-500">{{ getStatusText(habitacion) }}</div>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- Modales (mantener los existentes) -->
    <ReserveRoom 
      :room="room" 
      v-if="show && !showFree" 
      @close-modal="toggleModal" 
      @update-room="updateRoom" 
      @update-tiempo="agregarTiempoExtra" 
      @room-checkout="handleRoomCheckout"
    />

    <ReserveRoomLibre 
      :room="room" 
      v-if="showFree && !show" 
      @close-modal="toggleModalLibre"
    />
  </div>
</template>

<script setup>
import { ref, onMounted, computed, onUnmounted } from 'vue';
import axiosClient from '../axiosClient';
import ReserveRoom from '../components/ReserveRoom.vue';
import ReserveRoomLibre from '../components/ReserveRoomLibre.vue';
import { useAuthStore } from '../store/auth.js';
import { useWebSocketStore } from '../store/websocket.js';
import dayjs from 'dayjs';

// Estados existentes (mantener todos)
let habitaciones = [];
const habitacionesLibres = ref([]);
const habitacionesOcupadas = ref([]);
const room = ref(null);
const show = ref(false);
const showFree = ref(false);
const authStore = useAuthStore();
const websocketStore = useWebSocketStore();

// Nuevos estados para mejoras visuales
const searchTerm = ref('');
const filtroCategoria = ref('');
const soloOcupadas = ref(false);
const viewMode = ref('grid');
const compactMode = ref(false);
const ingresosDiarios = ref(15420); // Mock data

// Computed properties con nueva lógica de filtrado
const habitacionesLibresFiltradas = computed(() => {
  // Si no hay filtros activos, mostrar todas las habitaciones libres
  const hayFiltros = searchTerm.value.trim() !== '' || filtroCategoria.value !== '';
  
  if (!hayFiltros) {
    return habitacionesLibres.value;
  }
  
  // Si hay filtros, aplicar según el estado del checkbox
  return habitacionesLibres.value.filter(habitacion => {
    const matchesSearch = searchTerm.value === '' || habitacion.nombreHabitacion.toLowerCase().includes(searchTerm.value.toLowerCase());
    const matchesCategory = !filtroCategoria.value || habitacion.nombreHabitacion.includes(filtroCategoria.value);
    return matchesSearch && matchesCategory;
  });
});

const habitacionesOcupadasFiltradas = computed(() => {
  // Si el checkbox NO está marcado, mostrar todas las ocupadas sin filtrar
  if (!soloOcupadas.value) {
    return habitacionesOcupadas.value;
  }
  
  // Si el checkbox SÍ está marcado, aplicar filtros a las ocupadas
  return habitacionesOcupadas.value.filter(habitacion => {
    const matchesSearch = searchTerm.value === '' || habitacion.nombreHabitacion.toLowerCase().includes(searchTerm.value.toLowerCase());
    const matchesCategory = !filtroCategoria.value || habitacion.nombreHabitacion.includes(filtroCategoria.value);
    return matchesSearch && matchesCategory;
  });
});

const habitacionesProximasVencer = computed(() => {
  // Filtrar habitaciones próximas a vencer
  const proximasVencer = habitacionesOcupadas.value.filter(habitacion => {
    if (!habitacion.reservaActiva) return false;
    const timeLeft = getTimeLeftInMinutes(habitacion);
    return timeLeft > 0 && timeLeft <= 15;
  });
  
  // Si el checkbox NO está marcado, mostrar todas las próximas a vencer sin filtrar
  if (!soloOcupadas.value) {
    return proximasVencer;
  }
  
  // Si el checkbox SÍ está marcado, aplicar filtros a las próximas a vencer
  return proximasVencer.filter(habitacion => {
    const matchesSearch = searchTerm.value === '' || habitacion.nombreHabitacion.toLowerCase().includes(searchTerm.value.toLowerCase());
    const matchesCategory = !filtroCategoria.value || habitacion.nombreHabitacion.includes(filtroCategoria.value);
    return matchesSearch && matchesCategory;
  });
});

// Funciones de utilidad para el diseño mejorado
const getCategoryFromName = (nombreHabitacion) => {
  if (nombreHabitacion.includes('HIDROMAX')) return 'HIDROMAX';
  if (nombreHabitacion.includes('HIDRO')) return 'HIDRO';
  if (nombreHabitacion.includes('MASTER')) return 'MASTER';
  if (nombreHabitacion.includes('PENTHOUSE')) return 'PENTHOUSE';
  if (nombreHabitacion.includes('SUITE')) return 'SUITE';
  if (nombreHabitacion.includes('CLASICA')) return 'CLÁSICA';
  return 'STANDARD';
};

const getRoomNumber = (nombreHabitacion) => {
  const match = nombreHabitacion.match(/^(\d+)/);
  return match ? `#${match[1]}` : nombreHabitacion;
};

const getRoomType = (nombreHabitacion) => {
  const category = getCategoryFromName(nombreHabitacion);
  const specialNames = {
    'CUARTO ROJO DEL PLACER': 'Temática Especial',
    'BLACK AND WITHE': 'Diseño Exclusivo',
    'PATIO IN': 'Con Patio',
    'ESPEJOS': 'Con Espejos'
  };
  
  for (const [key, value] of Object.entries(specialNames)) {
    if (nombreHabitacion.includes(key)) {
      return value;
    }
  }
  
  return category;
};

const getRoomIcon = (nombreHabitacion) => {
  const category = getCategoryFromName(nombreHabitacion);
  const icons = {
    'CLÁSICA': 'hotel',
    'SUITE': 'hotel_class',
    'MASTER': 'emoji_events',
    'HIDROMAX': 'spa',
    'HIDRO': 'hot_tub',
    'PENTHOUSE': 'apartment'
  };
  return icons[category] || 'hotel';
};

const toggleView = () => {
  viewMode.value = viewMode.value === 'grid' ? 'list' : 'grid';
  localStorage.setItem('roomsViewMode', viewMode.value);
};

const toggleCompact = () => {
  compactMode.value = !compactMode.value;
  localStorage.setItem('roomsCompactMode', compactMode.value.toString());
};

const loadViewPreference = () => {
  const savedViewMode = localStorage.getItem('roomsViewMode');
  if (savedViewMode && (savedViewMode === 'grid' || savedViewMode === 'list')) {
    viewMode.value = savedViewMode;
  }
};

const loadCompactPreference = () => {
  const savedCompactMode = localStorage.getItem('roomsCompactMode');
  if (savedCompactMode !== null) {
    compactMode.value = savedCompactMode === 'true';
  }
};

// Funciones de tiempo y estado (mantener las existentes)
const getTimeLeftInMinutes = (habitacion) => {
  if (!habitacion.reservaActiva) return 0;
  const endTime = dayjs(habitacion.reservaActiva.fechaReserva)
    .add(habitacion.reservaActiva.totalHoras, 'hour')
    .add(habitacion.reservaActiva.totalMinutos, 'minute');
  return endTime.diff(dayjs(), 'minute');
};

const getTimeRemaining = (habitacion) => {
  if (!habitacion.reservaActiva) return 'Sin reserva';
  
  const timeLeft = getTimeLeftInMinutes(habitacion);
  
  if (timeLeft <= 0) {
    const overtime = Math.abs(timeLeft);
    const hours = Math.floor(overtime / 60);
    const minutes = overtime % 60;
    return `+${hours.toString().padStart(2, '0')}:${minutes.toString().padStart(2, '0')}`;
  }
  
  const hours = Math.floor(timeLeft / 60);
  const minutes = timeLeft % 60;
  return `${hours.toString().padStart(2, '0')}:${minutes.toString().padStart(2, '0')}`;
};

const getTimeProgress = (habitacion) => {
  if (!habitacion.reservaActiva) return 0;
  
  const totalMinutes = habitacion.reservaActiva.totalHoras * 60 + habitacion.reservaActiva.totalMinutos;
  const timeLeft = getTimeLeftInMinutes(habitacion);
  const elapsed = totalMinutes - timeLeft;
  
  return Math.max(0, Math.min(100, (elapsed / totalMinutes) * 100));
};

const getStatusText = (habitacion) => {
  const timeLeft = getTimeLeftInMinutes(habitacion);
  if (timeLeft <= 0) return 'Tiempo vencido';
  if (timeLeft <= 15) return 'Por vencer';
  return 'En curso';
};

const getStatusIndicator = (habitacion) => {
  const timeLeft = getTimeLeftInMinutes(habitacion);
  if (timeLeft <= 0) return 'w-3 h-3 bg-red-500 rounded-full animate-pulse shadow-lg shadow-red-500/50';
  if (timeLeft <= 15) return 'w-3 h-3 bg-yellow-500 rounded-full animate-pulse shadow-lg shadow-yellow-500/50';
  return 'w-3 h-3 bg-green-500 rounded-full animate-pulse shadow-lg shadow-green-500/50';
};

const getStatusTextColor = (habitacion) => {
  const timeLeft = getTimeLeftInMinutes(habitacion);
  if (timeLeft <= 0) return 'text-red-300';
  if (timeLeft <= 15) return 'text-yellow-300';
  return 'text-green-300';
};

const getTimeTextColor = (habitacion) => {
  const timeLeft = getTimeLeftInMinutes(habitacion);
  if (timeLeft <= 0) return 'text-red-300';
  if (timeLeft <= 15) return 'text-yellow-300';
  return 'text-gray-300';
};

const getProgressBarColor = (habitacion) => {
  const timeLeft = getTimeLeftInMinutes(habitacion);
  if (timeLeft <= 0) return 'bg-gradient-to-r from-red-500 to-red-600';
  if (timeLeft <= 15) return 'bg-gradient-to-r from-yellow-500 to-amber-500';
  return 'bg-gradient-to-r from-green-500 to-emerald-500';
};

const getOccupiedRoomStyles = (habitacion) => {
  const timeLeft = getTimeLeftInMinutes(habitacion);
  if (timeLeft <= 0) {
    return 'border-red-500/50 bg-gradient-to-br from-red-900/20 to-red-800/20 hover:border-red-400 hover:shadow-red-500/25';
  }
  if (timeLeft <= 15) {
    return 'border-yellow-500/50 bg-gradient-to-br from-yellow-900/20 to-yellow-800/20 hover:border-yellow-400 hover:shadow-yellow-500/25';
  }
  return 'border-white/20 bg-gradient-to-br from-white/10 to-white/5 hover:border-gray-400/50 hover:shadow-white/10';
};

// MANTENER TODAS LAS FUNCIONES EXISTENTES (no modificar la lógica)
const fetchHabitaciones = () => {
  console.log("🔄 Fetching habitaciones...");
  const institucionID = authStore.institucionID;

  if (!institucionID) {
    console.warn('InstitucionID is not available. Please ensure the user is logged in.');
    return;
  }

  axiosClient.get(`/GetHabitaciones?InstitucionID=${institucionID}`)
    .then(({ data }) => {
      if (data && data.data) {
        habitaciones = data.data;
        habitacionesLibres.value = habitaciones.filter(habitacion => habitacion.disponible === true);
        habitacionesOcupadas.value = habitaciones.filter(habitacion => habitacion.disponible === false);
        console.log("Libres", habitacionesLibres.value);
        console.log("Ocupadas", habitacionesOcupadas.value);
      } else {
        console.error('Datos de la API no válidos:', data);
      }
    })
    .catch(error => {
      console.error('Error al obtener las habitaciones:', error);
    });
};

const handleWebhookEvent = (data) => {
  console.log("Webhook event received in this component:", data);
  if (data.type === "warning" || data.type == "ended") {
    fetchHabitaciones();
  }
};

const getTimerUpdateInterval = async () => {
  try {
    const response = await axiosClient.get('/GetTimerUpdateInterval');
    const intervalMinutes = response.data?.interval || 10;
    localStorage.setItem('timerUpdateInterval', intervalMinutes.toString());
    console.log('Timer interval set to:', intervalMinutes, 'minutes');
    return intervalMinutes;
  } catch (error) {
    if (error.response?.status === 404) {
      console.log('Timer interval API not implemented yet, using default value');
    } else {
      console.error('Error fetching timer update interval:', error);
    }
    const defaultInterval = 10;
    localStorage.setItem('timerUpdateInterval', defaultInterval.toString());
    console.log('Timer interval set to default:', defaultInterval, 'minutes');
    return defaultInterval;
  }
};

onMounted(async () => {
  loadViewPreference();
  loadCompactPreference();
  fetchHabitaciones();
  await getTimerUpdateInterval();
  console.log("🔹 Registering WebSocket event listener in RoomComponent");
  websocketStore.registerEventCallback("RoomComponent", handleWebhookEvent);
});

onUnmounted(() => {
  console.log("❌ Unregistering WebSocket event listener in RoomComponent");
  websocketStore.unregisterEventCallback("RoomComponent");
});

// MANTENER TODAS LAS DEMÁS FUNCIONES EXISTENTES SIN CAMBIOS
const agregarTiempoExtra = (reservaID, horas, minutos) => {
  const index = habitacionesOcupadas.value.findIndex(h => h.reservaActiva.reservaId === reservaID);
  if (index === -1) {
    console.warn(`No se encontró una habitación con reservaID ${reservaID}`);
    return;
  }

  const habitacion = habitacionesOcupadas.value[index];
  let newHoras = habitacion.reservaActiva.totalHoras + horas;
  let newMinutos = habitacion.reservaActiva.totalMinutos + minutos;

  while (newMinutos >= 60) {
    newHoras += 1;
    newMinutos -= 60;
  }

  habitacionesOcupadas.value[index] = {
    ...habitacion, 
    reservaActiva: {
      ...habitacion.reservaActiva,
      totalHoras: newHoras,
      totalMinutos: newMinutos
    }
  };
};

const updateRoom = (updatedRoom) => {
  console.log('🔄 Updating room:', updatedRoom);
  
  const occupiedIndex = habitacionesOcupadas.value.findIndex(h => h.habitacionId === updatedRoom.habitacionId);
  
  if (occupiedIndex !== -1) {
    habitacionesOcupadas.value[occupiedIndex] = { ...habitacionesOcupadas.value[occupiedIndex], ...updatedRoom };
    console.log('✅ Room updated in occupied rooms');
    return;
  }
  
  const freeIndex = habitacionesLibres.value.findIndex(h => h.habitacionId === updatedRoom.habitacionId);
  
  if (freeIndex !== -1) {
    habitacionesLibres.value[freeIndex] = { ...habitacionesLibres.value[freeIndex], ...updatedRoom };
    console.log('✅ Room updated in free rooms');
    return;
  }
  
  console.warn('⚠️ Room not found for update:', updatedRoom.habitacionId);
};

const handleRoomCheckout = (roomId) => {
  console.log('🏠 Handling room checkout for room:', roomId);
  
  const occupiedIndex = habitacionesOcupadas.value.findIndex(h => h.habitacionId === roomId);
  
  if (occupiedIndex !== -1) {
    const room = habitacionesOcupadas.value[occupiedIndex];
    habitacionesOcupadas.value.splice(occupiedIndex, 1);
    
    const freeRoom = {
      ...room,
      disponible: true,
      reservaActiva: null,
      visita: null,
      visitaID: null,
      pedidosPendientes: false
    };
    
    habitacionesLibres.value.push(freeRoom);
    console.log('✅ Room moved from occupied to free');
    
    show.value = false;
    document.body.style.overflow = 'auto';
  } else {
    console.warn('⚠️ Room not found in occupied rooms for checkout:', roomId);
  }
};

const handleRoomReserved = (roomId) => {
  console.log('🏠 Handling room reservation for room:', roomId);
  
  showFree.value = false;
  show.value = false;
  document.body.style.overflow = 'auto';
  room.value = null;
  
  const freeIndex = habitacionesLibres.value.findIndex(h => h.habitacionId === roomId);
  
  if (freeIndex !== -1) {
    habitacionesLibres.value.splice(freeIndex, 1);
    console.log('✅ Room removed from free rooms');
  } else {
    console.warn('⚠️ Room not found in free rooms for reservation:', roomId);
  }
  
  setTimeout(() => {
    fetchHabitaciones();
  }, 1000);
};

function toggleModal(Room) {
  showFree.value = false;
  show.value = !show.value;
  room.value = Room;
  console.log("selecciono habitacion ocupada:", Room);
  document.body.style.overflow = show.value ? 'hidden' : 'auto';
}

function toggleModalLibre(Room) {
  show.value = false;
  showFree.value = !showFree.value;
  room.value = Room;
  console.log("selecciono habitacion libre:", Room);
  document.body.style.overflow = showFree.value ? 'hidden' : 'auto';
}
</script>

<style scoped>
/* Scrollbar personalizado premium */
.custom-scrollbar::-webkit-scrollbar {
  width: 8px;
}

.custom-scrollbar::-webkit-scrollbar-track {
  background: rgba(255, 255, 255, 0.05);
  border-radius: 10px;
}

.custom-scrollbar::-webkit-scrollbar-thumb {
  background: linear-gradient(45deg, rgba(168, 85, 247, 0.5), rgba(236, 72, 153, 0.5));
  border-radius: 10px;
  border: 2px solid transparent;
  background-clip: content-box;
}

.custom-scrollbar::-webkit-scrollbar-thumb:hover {
  background: linear-gradient(45deg, rgba(168, 85, 247, 0.8), rgba(236, 72, 153, 0.8));
  background-clip: content-box;
}

/* Efectos de cristal mejorados */
.backdrop-blur-xl {
  backdrop-filter: blur(20px);
}

.backdrop-blur-md {
  backdrop-filter: blur(12px);
}

/* Animaciones suaves */
@keyframes fadeInUp {
  from {
    opacity: 0;
    transform: translateY(20px);
  }
  to {
    opacity: 1;
    transform: translateY(0);
  }
}

.slide-in {
  animation: fadeInUp 0.6s ease-out;
}

/* Efectos de hover mejorados */
.group:hover .group-hover\:scale-110 {
  transform: scale(1.1);
}

.group:hover .group-hover\:translate-x-full {
  transform: translateX(100%);
}

/* Responsive adjustments */
@media (max-width: 1024px) {
  .grid-cols-4 {
    grid-template-columns: repeat(3, minmax(0, 1fr));
  }
}

@media (max-width: 768px) {
  .grid-cols-4,
  .grid-cols-3 {
    grid-template-columns: repeat(2, minmax(0, 1fr));
  }
  
  .text-4xl {
    font-size: 2rem;
  }
  
  .text-3xl {
    font-size: 1.5rem;
  }
}

@media (max-width: 640px) {
  .grid-cols-2 {
    grid-template-columns: repeat(1, minmax(0, 1fr));
  }
}

/* Efectos especiales para elementos premium */
.shadow-2xl {
  box-shadow: 0 25px 50px -12px rgba(0, 0, 0, 0.25);
}

.shadow-lg {
  box-shadow: 0 10px 15px -3px rgba(0, 0, 0, 0.1), 0 4px 6px -2px rgba(0, 0, 0, 0.05);
}

/* Animación de pulso personalizada */
@keyframes pulse-custom {
  0%, 100% {
    opacity: 1;
    transform: scale(1);
  }
  50% {
    opacity: 0.8;
    transform: scale(1.05);
  }
}

.animate-pulse-custom {
  animation: pulse-custom 2s cubic-bezier(0.4, 0, 0.6, 1) infinite;
}
</style>
