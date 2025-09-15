<template>
  <div class="min-h-screen bg-gradient-to-br from-surface-950 via-surface-900 to-surface-950 relative overflow-hidden">
    <!-- Decorative background effects -->
    <div class="absolute inset-0 overflow-hidden pointer-events-none">
      <div class="absolute top-1/4 -left-48 w-96 h-96 bg-primary-500/5 rounded-full blur-3xl"></div>
      <div class="absolute bottom-1/4 -right-48 w-96 h-96 bg-secondary-500/5 rounded-full blur-3xl"></div>
      <div class="absolute top-3/4 left-1/3 w-64 h-64 bg-accent-500/3 rounded-full blur-2xl"></div>
    </div>

    <div class="relative z-10 p-6">
      <!-- Enhanced header with glassmorphism -->
      <div class="mb-8">
        <div class="bg-white/5 backdrop-blur-xl border border-white/10 rounded-3xl p-8 shadow-2xl">
          <div class="flex flex-col lg:flex-row justify-between items-start lg:items-center gap-6">
            <!-- Main title -->
            <div class="flex items-center gap-4">
              <div class="bg-gradient-to-r from-primary-400 via-secondary-400 to-accent-400 rounded-2xl w-16 h-16 flex items-center justify-center shadow-lg">
                <i class="fas fa-users text-white text-2xl"></i>
              </div>
              <div>
                <h1 class="text-4xl font-bold bg-gradient-to-r from-primary-300 via-secondary-300 to-accent-300 bg-clip-text text-transparent lexend-exa">
                  Administración de Usuarios
                </h1>
                <p class="text-gray-400 text-sm mt-1">Gestión completa de usuarios y permisos</p>
              </div>
            </div>
            
            <!-- Quick stats -->
            <div class="flex gap-4">
              <div class="bg-white/5 backdrop-blur-md rounded-2xl p-4 border border-white/10">
                <div class="text-2xl font-bold text-white">{{ filteredUsers.length }}</div>
                <div class="text-xs text-gray-400">Usuarios Filtrados</div>
              </div>
              <div class="bg-white/5 backdrop-blur-md rounded-2xl p-4 border border-white/10">
                <div class="text-2xl font-bold text-green-400">{{ activeUsersCount }}</div>
                <div class="text-xs text-gray-400">Activos</div>
              </div>
              <div class="bg-white/5 backdrop-blur-md rounded-2xl p-4 border border-white/10">
                <div class="text-2xl font-bold text-blue-400">{{ roles.length }}</div>
                <div class="text-xs text-gray-400">Roles</div>
              </div>
            </div>
          </div>
        </div>
      </div>

      <!-- Error display -->
      <div v-if="error" class="mb-6">
        <div class="bg-red-500/10 border border-red-500/20 rounded-2xl p-4 backdrop-blur-md">
          <div class="flex items-center gap-3">
            <i class="fas fa-exclamation-triangle text-red-400"></i>
            <div>
              <h3 class="text-red-300 font-medium">Error en la gestión de usuarios</h3>
              <p class="text-red-200/70 text-sm">{{ error }}</p>
            </div>
            <button 
              @click="clearError"
              class="ml-auto text-red-400 hover:text-red-300 transition-colors"
            >
              <i class="fas fa-times"></i>
            </button>
          </div>
        </div>
      </div>

      <!-- Loading state -->
      <div v-if="loading && !filteredUsers.length" class="flex items-center justify-center py-20">
        <div class="text-center">
          <div class="inline-block animate-spin rounded-full h-12 w-12 border-b-2 border-primary-400 mb-4"></div>
          <p class="text-gray-400">Cargando usuarios...</p>
        </div>
      </div>

      <!-- Main content -->
      <div v-else class="bg-white/5 backdrop-blur-xl border border-white/10 rounded-3xl shadow-2xl overflow-hidden">
        <UserList
          :users="filteredUsers"
          :roles="roles"
          :loading="loading"
          :error="error"
          :pagination="pagination"
          @create="openUserModal"
          @edit="openUserModal"
          @delete="openDeleteModal"
          @change-password="openPasswordModal"
          @resend-verification="handleResendVerification"
          @refresh="loadUsers"
          @page-change="changePage"
          @filter="setFilters"
        />
      </div>
    </div>

    <!-- Modals -->
    <UserFormModal
      :show="showUserModal"
      :user="selectedUser"
      :roles="roles"
      :loading="loading"
      :api-error="error"
      @close="closeUserModal"
      @submit="handleUserSubmit"
    />

    <UserDeleteModal
      :show="showDeleteModal"
      :user="userToDelete"
      :loading="loading"
      :api-error="error"
      @close="closeDeleteModal"
      @confirm="handleDeleteUser"
    />

    <ChangePasswordModal
      :show="showPasswordModal"
      :user="selectedUser"
      :loading="loading"
      :api-error="error"
      @close="closePasswordModal"
      @submit="handlePasswordChange"
    />
  </div>
</template>

<script setup>
import { onMounted, ref, computed } from 'vue';
import { useUsers } from '../composables/useUsers';
import { useUsersStore } from '../store/modules/users';
import UserList from '../components/users/UserList.vue';
import UserFormModal from '../components/users/UserFormModal.vue';
import UserDeleteModal from '../components/users/UserDeleteModal.vue';
import ChangePasswordModal from '../components/users/ChangePasswordModal.vue';

const usersStore = useUsersStore();

const {
  users,
  filteredUsers,
  roles,
  loading,
  error,
  pagination,
  showUserModal,
  showDeleteModal,
  showPasswordModal,
  selectedUser,
  userToDelete,
  loadUsers,
  loadRoles,
  createUser,
  updateUser,
  deleteUser,
  changePassword,
  openUserModal,
  closeUserModal,
  openDeleteModal,
  closeDeleteModal,
  openPasswordModal,
  closePasswordModal,
  setFilters,
  changePage,
  resendVerification
} = useUsers();


const clearError = () => {
  error.value = null;
};

// Computed properties for stats
const activeUsersCount = computed(() => {
  return filteredUsers.value.filter(user => user.isActive).length;
});

const handleUserSubmit = async (userData) => {
  try {
    let result;
    
    if (selectedUser.value) {
      // Use the unified updateUser endpoint
      result = await updateUser(userData.userId, {
        firstName: userData.firstName,
        lastName: userData.lastName,
        roles: userData.roles,
        isActive: userData.isActive
      });
    } else {
      result = await createUser(userData);
    }
    
    if (result.success) {
      closeUserModal();
      await loadUsers();
    }
  } catch (err) {
    console.error('Error submitting user:', err);
  }
};

const handleDeleteUser = async (user) => {
  try {
    const result = await deleteUser(user.id);
    
    if (result.success) {
      closeDeleteModal();
      await loadUsers();
    }
  } catch (err) {
    console.error('Error deleting user:', err);
  }
};

const handlePasswordChange = async (passwordData) => {
  try {
    const result = await changePassword(passwordData.userId, passwordData.newPassword);
    
    if (result.success) {
      closePasswordModal();
    }
  } catch (err) {
    console.error('Error changing password:', err);
  }
};

const handleResendVerification = async (user) => {
  try {
    const result = await resendVerification(user.id);
    
    if (result.success) {
      console.log('Verification email sent successfully');
    }
  } catch (err) {
    console.error('Error sending verification email:', err);
  }
};

onMounted(async () => {
  await Promise.all([
    loadRoles(),
    loadUsers()
  ]);
});
</script>

<style scoped>
/* Enhanced glassmorphism effects */
.backdrop-blur-xl {
  backdrop-filter: blur(20px);
}

.backdrop-blur-md {
  backdrop-filter: blur(12px);
}

/* Custom scrollbar styling */
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
</style>