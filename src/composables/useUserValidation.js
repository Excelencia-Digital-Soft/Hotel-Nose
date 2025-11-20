import { ref, computed } from 'vue';

export function useUserValidation() {
  const errors = ref({});

  const validateEmail = (email) => {
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    if (!email) {
      return 'El correo electrónico es requerido';
    }
    if (!emailRegex.test(email)) {
      return 'El correo electrónico no es válido';
    }
    return null;
  };

  const validatePassword = (password) => {
    if (!password) {
      return 'La contraseña es requerida';
    }
    if (password.length < 6) {
      return 'La contraseña debe tener al menos 6 caracteres';
    }
    if (!/(?=.*[a-z])/.test(password)) {
      return 'La contraseña debe contener al menos una letra minúscula';
    }
    if (!/(?=.*[A-Z])/.test(password)) {
      return 'La contraseña debe contener al menos una letra mayúscula';
    }
    if (!/(?=.*\d)/.test(password)) {
      return 'La contraseña debe contener al menos un número';
    }
    return null;
  };

  const validatePasswordConfirmation = (password, confirmation) => {
    if (!confirmation) {
      return 'La confirmación de contraseña es requerida';
    }
    if (password !== confirmation) {
      return 'Las contraseñas no coinciden';
    }
    return null;
  };

  const validateUsername = (username) => {
    if (!username) {
      return 'El nombre de usuario es requerido';
    }
    if (username.length < 3) {
      return 'El nombre de usuario debe tener al menos 3 caracteres';
    }
    return null;
  };

  const validateFullName = (fullName) => {
    if (!fullName) {
      return 'El nombre completo es requerido';
    }
    if (fullName.length < 3) {
      return 'El nombre completo debe tener al menos 3 caracteres';
    }
    return null;
  };

  const validateRole = (roles) => {
    if (!roles || (Array.isArray(roles) && roles.length === 0)) {
      return 'El rol es requerido';
    }
    return null;
  };

  const validateUserForm = (formData, isEditing = false) => {
    const newErrors = {};

    if (!isEditing) {
      const emailError = validateEmail(formData.email);
      if (emailError) newErrors.email = emailError;

      const passwordError = validatePassword(formData.password);
      if (passwordError) newErrors.password = passwordError;

      if (formData.passwordConfirmation !== undefined) {
        const confirmError = validatePasswordConfirmation(formData.password, formData.passwordConfirmation);
        if (confirmError) newErrors.passwordConfirmation = confirmError;
      }
    }

    const usernameError = validateUsername(formData.userName || formData.username);
    if (usernameError) newErrors.username = usernameError;

    if (formData.firstName !== undefined) {
      const firstNameError = validateFullName(formData.firstName);
      if (firstNameError) newErrors.firstName = firstNameError;
    }

    if (formData.lastName !== undefined) {
      const lastNameError = validateFullName(formData.lastName);
      if (lastNameError) newErrors.lastName = lastNameError;
    }

    if (formData.fullName !== undefined) {
      const fullNameError = validateFullName(formData.fullName);
      if (fullNameError) newErrors.fullName = fullNameError;
    }

    const roleError = validateRole(formData.roles);
    if (roleError) newErrors.role = roleError;

    errors.value = newErrors;
    return Object.keys(newErrors).length === 0;
  };

  const validatePasswordChange = (passwordData) => {
    const newErrors = {};

    // Solo validar contraseña actual si está definida y requerida
    console.log("CURRENT PASSWORD: ", passwordData.currentPassword);
    if (passwordData.currentPassword !== undefined) {
      if (!passwordData.currentPassword) {
        newErrors.currentPassword = 'La contraseña actual es requerida';
      }
    }

    // Siempre validar nueva contraseña (campo requerido)
    if (!passwordData.newPassword) {
      newErrors.newPassword = 'La nueva contraseña es requerida';
    } else {
      const newPasswordError = validatePassword(passwordData.newPassword);
      if (newPasswordError) newErrors.newPassword = newPasswordError;
    }

    // Siempre validar confirmación (campo requerido)
    if (!passwordData.confirmPassword) {
      newErrors.confirmPassword = 'La confirmación de contraseña es requerida';
    } else if (passwordData.newPassword && passwordData.confirmPassword) {
      const confirmError = validatePasswordConfirmation(
        passwordData.newPassword,
        passwordData.confirmPassword
      );
      if (confirmError) newErrors.confirmPassword = confirmError;
    }

    errors.value = newErrors;
    return Object.keys(newErrors).length === 0;
  };

  const clearErrors = () => {
    errors.value = {};
  };

  const hasErrors = computed(() => Object.keys(errors.value).length > 0);

  return {
    errors,
    hasErrors,
    validateEmail,
    validatePassword,
    validatePasswordConfirmation,
    validateUsername,
    validateFullName,
    validateRole,
    validateUserForm,
    validatePasswordChange,
    clearErrors
  };
}
