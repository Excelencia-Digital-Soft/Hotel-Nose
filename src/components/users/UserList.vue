<template>
  <div class="p-0">
    <!-- Header with filters and actions -->
    <div class="p-6 bg-gradient-to-r from-primary-600/10 via-secondary-600/10 to-accent-600/10 border-b border-white/10">
      <div class="flex flex-col lg:flex-row justify-between items-start lg:items-center gap-6">
        <div class="flex items-center gap-4">
          <div class="w-12 h-12 bg-gradient-to-r from-blue-400 to-purple-500 rounded-2xl flex items-center justify-center shadow-lg">
            <i class="fas fa-list text-white"></i>
          </div>
          <div>
            <h3 class="text-2xl font-bold text-white lexend-exa">Lista de Usuarios</h3>
            <p class="text-gray-400 text-sm">{{ users.length }} usuarios registrados</p>
          </div>
        </div>
        
        <button
          @click="$emit('create')"
          class="group bg-gradient-to-r from-primary-500 to-secondary-500 hover:from-primary-600 hover:to-secondary-600 px-6 py-3 rounded-2xl text-white font-medium transition-all duration-300 transform hover:scale-105 shadow-lg hover:shadow-xl"
        >
          <i class="fas fa-plus mr-2 group-hover:rotate-90 transition-transform duration-300"></i>
          Nuevo Usuario
        </button>
      </div>

      <!-- Enhanced filters -->
      <div class="mt-6 grid grid-cols-1 md:grid-cols-3 gap-4">
        <div class="relative">
          <i class="fas fa-search absolute left-4 top-1/2 transform -translate-y-1/2 text-gray-400"></i>
          <input
            v-model="searchQuery"
            @input="handleSearch"
            type="text"
            placeholder="Buscar usuarios..."
            class="w-full pl-12 pr-4 py-3 rounded-2xl bg-white/5 backdrop-blur-md border border-white/10 text-white placeholder-gray-400 focus:outline-none focus:ring-2 focus:ring-primary-500 focus:border-transparent transition-all duration-300"
          />
        </div>
        <div class="relative">
          <i class="fas fa-user-tag absolute left-4 top-1/2 transform -translate-y-1/2 text-gray-400"></i>
          <select
            v-model="selectedRole"
            @change="handleRoleFilter"
            class="w-full pl-12 pr-4 py-3 rounded-2xl bg-white/5 backdrop-blur-md border border-white/10 text-white focus:outline-none focus:ring-2 focus:ring-primary-500 appearance-none cursor-pointer transition-all duration-300"
          >
            <option value="">Todos los roles</option>
            <option v-for="role in roles" :key="role.id" :value="role.name">
              {{ role.name }}
            </option>
          </select>
          <i class="fas fa-chevron-down absolute right-4 top-1/2 transform -translate-y-1/2 text-gray-400 pointer-events-none"></i>
        </div>
        <div class="relative">
          <i class="fas fa-toggle-on absolute left-4 top-1/2 transform -translate-y-1/2 text-gray-400"></i>
          <select
            v-model="selectedStatus"
            @change="handleStatusFilter"
            class="w-full pl-12 pr-4 py-3 rounded-2xl bg-white/5 backdrop-blur-md border border-white/10 text-white focus:outline-none focus:ring-2 focus:ring-primary-500 appearance-none cursor-pointer transition-all duration-300"
          >
            <option value="">Todos los estados</option>
            <option value="true">Activos</option>
            <option value="false">Inactivos</option>
          </select>
          <i class="fas fa-chevron-down absolute right-4 top-1/2 transform -translate-y-1/2 text-gray-400 pointer-events-none"></i>
        </div>
      </div>
    </div>

    <!-- Content area -->
    <div class="p-6">
      <!-- Loading state -->
      <div v-if="loading" class="flex items-center justify-center py-20">
        <div class="text-center">
          <div class="inline-block animate-spin rounded-full h-12 w-12 border-b-2 border-primary-400 mb-4"></div>
          <p class="text-gray-400">Cargando usuarios...</p>
        </div>
      </div>

      <!-- Error state -->
      <div v-else-if="error" class="text-center py-20">
        <div class="bg-red-500/10 border border-red-500/20 rounded-2xl p-8 backdrop-blur-md max-w-md mx-auto">
          <i class="fas fa-exclamation-triangle text-red-400 text-4xl mb-4"></i>
          <p class="text-red-300 font-medium mb-4">Error al cargar usuarios</p>
          <p class="text-red-200/70 text-sm mb-6">{{ error }}</p>
          <button
            @click="$emit('refresh')"
            class="bg-gradient-to-r from-red-500 to-red-600 hover:from-red-600 hover:to-red-700 px-6 py-3 rounded-2xl text-white font-medium transition-all duration-300 transform hover:scale-105"
          >
            <i class="fas fa-redo mr-2"></i>
            Reintentar
          </button>
        </div>
      </div>

      <!-- Empty state -->
      <div v-else-if="!users.length" class="text-center py-20">
        <div class="bg-white/5 backdrop-blur-md border border-white/10 rounded-2xl p-8 max-w-md mx-auto">
          <i class="fas fa-users text-gray-400 text-4xl mb-4"></i>
          <p class="text-gray-300 font-medium mb-2">No se encontraron usuarios</p>
          <p class="text-gray-400 text-sm">Comienza creando tu primer usuario</p>
        </div>
      </div>

      <!-- Users grid -->
      <div v-else class="grid gap-4">
        <div
          v-for="user in users"
          :key="user.id"
          class="group bg-white/5 backdrop-blur-md border border-white/10 rounded-2xl p-6 hover:bg-white/10 transition-all duration-300 hover:scale-[1.02] hover:shadow-lg"
        >
          <div class="flex items-center justify-between">
            <!-- User info -->
            <div class="flex items-center gap-4 flex-1">
              <!-- Avatar -->
              <div class="w-12 h-12 bg-gradient-to-r from-primary-400 to-secondary-400 rounded-2xl flex items-center justify-center shadow-lg">
                <i class="fas fa-user text-white"></i>
              </div>
              
              <!-- Details -->
              <div class="flex-1">
                <div class="flex items-center gap-3 mb-1">
                  <h4 class="text-white font-medium text-lg">{{ user.userName || user.username }}</h4>
                  <div class="flex items-center gap-2">
                    <!-- Status indicator -->
                    <div 
                      :class="user.isActive 
                        ? 'w-3 h-3 bg-green-400 rounded-full animate-pulse' 
                        : 'w-3 h-3 bg-red-400 rounded-full'"
                    ></div>
                    <span 
                      :class="user.isActive ? 'text-green-400' : 'text-red-400'"
                      class="text-xs font-medium"
                    >
                      {{ user.isActive ? 'Activo' : 'Inactivo' }}
                    </span>
                  </div>
                </div>
                
                <div class="text-gray-400 text-sm mb-2">
                  {{ user.email }}
                  <span v-if="user.firstName || user.lastName" class="ml-2">
                    • {{ [user.firstName, user.lastName].filter(Boolean).join(' ') }}
                  </span>
                </div>

                <!-- Roles -->
                <div class="flex flex-wrap gap-2 mb-2">
                  <span
                    v-for="role in user.roles"
                    :key="role"
                    class="px-3 py-1 text-xs rounded-full font-medium transition-all duration-300"
                    :class="getRoleClass(role)"
                  >
                    {{ getRoleName(role) }}
                  </span>
                  <span
                    v-if="!user.roles || user.roles.length === 0"
                    class="px-3 py-1 text-xs rounded-full bg-gray-500/30 border border-gray-500/50 text-gray-300"
                  >
                    Sin roles
                  </span>
                </div>

                <!-- Email verification status -->
                <div class="flex items-center gap-2">
                  <i
                    :class="user.emailConfirmed 
                      ? 'fas fa-check-circle text-green-400' 
                      : 'fas fa-times-circle text-orange-400'"
                  ></i>
                  <span 
                    :class="user.emailConfirmed ? 'text-green-400' : 'text-orange-400'"
                    class="text-xs"
                  >
                    {{ user.emailConfirmed ? 'Email verificado' : 'Email pendiente' }}
                  </span>
                  <button
                    v-if="!user.emailConfirmed"
                    @click="$emit('resend-verification', user)"
                    class="ml-2 text-xs text-accent-400 hover:text-accent-300 hover:underline transition-colors"
                  >
                    Reenviar
                  </button>
                </div>
              </div>
            </div>

            <!-- Actions -->
            <div class="flex items-center gap-2 opacity-0 group-hover:opacity-100 transition-opacity duration-300">
              <button
                @click="$emit('edit', user)"
                class="p-3 bg-blue-500/20 hover:bg-blue-500/30 border border-blue-500/30 hover:border-blue-500/50 rounded-xl text-blue-400 hover:text-blue-300 transition-all duration-300 transform hover:scale-110"
                title="Editar usuario"
              >
                <i class="fas fa-edit"></i>
              </button>
              <button
                @click="$emit('change-password', user)"
                class="p-3 bg-yellow-500/20 hover:bg-yellow-500/30 border border-yellow-500/30 hover:border-yellow-500/50 rounded-xl text-yellow-400 hover:text-yellow-300 transition-all duration-300 transform hover:scale-110"
                title="Cambiar contraseña"
              >
                <i class="fas fa-key"></i>
              </button>
              <button
                @click="$emit('delete', user)"
                class="p-3 bg-red-500/20 hover:bg-red-500/30 border border-red-500/30 hover:border-red-500/50 rounded-xl text-red-400 hover:text-red-300 transition-all duration-300 transform hover:scale-110"
                title="Eliminar usuario"
              >
                <i class="fas fa-trash"></i>
              </button>
            </div>
          </div>
        </div>
      </div>

      <!-- Pagination -->
      <div v-if="pagination.totalPages > 1" class="mt-8 flex justify-center">
        <nav class="flex items-center gap-2">
          <button
            @click="$emit('page-change', pagination.currentPage - 1)"
            :disabled="pagination.currentPage === 1"
            class="p-3 bg-white/5 backdrop-blur-md border border-white/10 rounded-xl text-white disabled:opacity-50 disabled:cursor-not-allowed hover:bg-white/10 transition-all duration-300"
          >
            <i class="fas fa-chevron-left"></i>
          </button>
          
          <button
            v-for="page in visiblePages"
            :key="page"
            @click="$emit('page-change', page)"
            :class="[
              'px-4 py-3 rounded-xl font-medium transition-all duration-300',
              page === pagination.currentPage
                ? 'bg-gradient-to-r from-primary-500 to-secondary-500 text-white shadow-lg'
                : 'bg-white/5 backdrop-blur-md border border-white/10 text-gray-300 hover:bg-white/10 hover:text-white'
            ]"
          >
            {{ page }}
          </button>
          
          <button
            @click="$emit('page-change', pagination.currentPage + 1)"
            :disabled="pagination.currentPage === pagination.totalPages"
            class="p-3 bg-white/5 backdrop-blur-md border border-white/10 rounded-xl text-white disabled:opacity-50 disabled:cursor-not-allowed hover:bg-white/10 transition-all duration-300"
          >
            <i class="fas fa-chevron-right"></i>
          </button>
        </nav>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed } from 'vue';

const props = defineProps({
  users: {
    type: Array,
    default: () => []
  },
  roles: {
    type: Array,
    default: () => []
  },
  loading: {
    type: Boolean,
    default: false
  },
  error: {
    type: String,
    default: null
  },
  pagination: {
    type: Object,
    default: () => ({
      currentPage: 1,
      totalPages: 1,
      pageSize: 10,
      totalItems: 0
    })
  }
});

const emit = defineEmits([
  'create',
  'edit',
  'delete',
  'change-password',
  'resend-verification',
  'refresh',
  'page-change',
  'filter'
]);

const searchQuery = ref('');
const selectedRole = ref('');
const selectedStatus = ref('');

const handleSearch = () => {
  emit('filter', {
    search: searchQuery.value,
    roleId: selectedRole.value || null,
    isActive: selectedStatus.value ? selectedStatus.value === 'true' : null
  });
};

const handleRoleFilter = () => {
  handleSearch();
};

const handleStatusFilter = () => {
  handleSearch();
};

const getRoleName = (roleId) => {
  const role = props.roles.find(r => r.id === roleId);
  return role?.name || roleId || 'Sin rol';
};

const getRoleClass = (roleId) => {
  // Get role name from roles array to match with classes
  const role = props.roles.find(r => r.id === roleId);
  const roleName = role?.name || roleId;
  
  const roleClasses = {
    'Administrator': 'bg-purple-500/30 border border-purple-500/50 text-purple-300',
    'Director': 'bg-red-500/30 border border-red-500/50 text-red-300',
    'Cajero': 'bg-green-500/30 border border-green-500/50 text-green-300',
    'Cajero Stock': 'bg-blue-500/30 border border-blue-500/50 text-blue-300',
    'Mucama': 'bg-yellow-500/30 border border-yellow-500/50 text-yellow-300'
  };
  return roleClasses[roleName] || 'bg-gray-500/30 border border-gray-500/50 text-gray-300';
};

const visiblePages = computed(() => {
  const pages = [];
  const total = props.pagination.totalPages;
  const current = props.pagination.currentPage;
  const maxVisible = 5;

  if (total <= maxVisible) {
    for (let i = 1; i <= total; i++) {
      pages.push(i);
    }
  } else {
    let start = Math.max(1, current - Math.floor(maxVisible / 2));
    let end = Math.min(total, start + maxVisible - 1);

    if (end - start < maxVisible - 1) {
      start = Math.max(1, end - maxVisible + 1);
    }

    for (let i = start; i <= end; i++) {
      pages.push(i);
    }
  }

  return pages;
});
</script>