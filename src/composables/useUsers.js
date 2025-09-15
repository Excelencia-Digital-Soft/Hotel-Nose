import { ref, computed } from 'vue';
import { useUsersStore } from '../store/modules/users';
import { storeToRefs } from 'pinia';

export function useUsers() {
  const usersStore = useUsersStore();
  const { 
    users, 
    roles, 
    currentUser, 
    loading, 
    error, 
    pagination,
    filters,
    filteredUsers,
    hasUsers,
    hasRoles,
    isLoading,
    hasError
  } = storeToRefs(usersStore);

  const showUserModal = ref(false);
  const showDeleteModal = ref(false);
  const showPasswordModal = ref(false);
  const selectedUser = ref(null);
  const userToDelete = ref(null);

  const loadUsers = async () => {
    await usersStore.fetchUsers();
  };

  const loadRoles = async () => {
    await usersStore.fetchRoles();
  };

  const createUser = async (userData) => {
    const result = await usersStore.createUser(userData);
    if (result.success) {
      showUserModal.value = false;
      selectedUser.value = null;
    }
    return result;
  };

  const updateUser = async (userId, userData) => {
    const result = await usersStore.updateUser(userId, userData);
    if (result.success) {
      showUserModal.value = false;
      selectedUser.value = null;
    }
    return result;
  };

  const deleteUser = async (userId) => {
    const result = await usersStore.deleteUser(userId);
    if (result.success) {
      showDeleteModal.value = false;
      userToDelete.value = null;
    }
    return result;
  };

  const changePassword = async (userId, newPassword) => {
    const result = await usersStore.changeUserPassword(userId, newPassword);
    if (result.success) {
      showPasswordModal.value = false;
    }
    return result;
  };

  const openUserModal = (user = null) => {
    selectedUser.value = user;
    showUserModal.value = true;
  };

  const closeUserModal = () => {
    showUserModal.value = false;
    selectedUser.value = null;
    usersStore.clearError();
  };

  const openDeleteModal = (user) => {
    userToDelete.value = user;
    showDeleteModal.value = true;
  };

  const closeDeleteModal = () => {
    showDeleteModal.value = false;
    userToDelete.value = null;
  };

  const openPasswordModal = (user) => {
    selectedUser.value = user;
    showPasswordModal.value = true;
  };

  const closePasswordModal = () => {
    showPasswordModal.value = false;
    selectedUser.value = null;
    usersStore.clearError();
  };

  const setFilters = (newFilters) => {
    usersStore.setFilters(newFilters);
  };

  const clearFilters = () => {
    usersStore.clearFilters();
  };

  const changePage = (page) => {
    usersStore.setPage(page);
  };

  const resendVerification = async (userId) => {
    return await usersStore.resendVerificationEmail(userId);
  };

  return {
    users,
    roles,
    currentUser,
    loading,
    error,
    pagination,
    filters,
    filteredUsers,
    hasUsers,
    hasRoles,
    isLoading,
    hasError,
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
    clearFilters,
    changePage,
    resendVerification
  };
}