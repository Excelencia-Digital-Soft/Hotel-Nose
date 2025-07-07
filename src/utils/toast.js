/**
 * Toast notification utilities using PrimeVue ToastService
 */

import { useToast } from "primevue/usetoast";
import { getCurrentInstance } from 'vue';

/**
 * Safely get toast instance
 * Returns null if not in Vue component context
 */
function getToastInstance() {
  try {
    const instance = getCurrentInstance();
    if (instance) {
      return useToast();
    }
    return null;
  } catch (error) {
    return null;
  }
}

/**
 * Show success toast notification
 * @param {string} message - Success message to display
 * @param {string} title - Optional title (default: "Éxito")
 * @param {number} life - Duration in milliseconds (default: 3000)
 */
export function showSuccessToast(message, title = "Éxito", life = 3000) {
  const toast = getToastInstance();
  if (toast) {
    toast.add({
      severity: 'success',
      summary: title,
      detail: message,
      life: life
    });
  } else {
    // Fallback to console log if Toast is not available
    console.log(`✅ ${title}: ${message}`);
  }
}

/**
 * Show error toast notification
 * @param {string} message - Error message to display
 * @param {string} title - Optional title (default: "Error")
 * @param {number} life - Duration in milliseconds (default: 5000)
 */
export function showErrorToast(message, title = "Error", life = 5000) {
  const toast = getToastInstance();
  if (toast) {
    toast.add({
      severity: 'error',
      summary: title,
      detail: message,
      life: life
    });
  } else {
    // Fallback to console log if Toast is not available
    console.error(`❌ ${title}: ${message}`);
  }
}

/**
 * Show warning toast notification
 * @param {string} message - Warning message to display
 * @param {string} title - Optional title (default: "Advertencia")
 * @param {number} life - Duration in milliseconds (default: 4000)
 */
export function showWarningToast(message, title = "Advertencia", life = 4000) {
  const toast = getToastInstance();
  if (toast) {
    toast.add({
      severity: 'warn',
      summary: title,
      detail: message,
      life: life
    });
  } else {
    // Fallback to console log if Toast is not available
    console.warn(`⚠️ ${title}: ${message}`);
  }
}

/**
 * Show info toast notification
 * @param {string} message - Info message to display
 * @param {string} title - Optional title (default: "Información")
 * @param {number} life - Duration in milliseconds (default: 3000)
 */
export function showInfoToast(message, title = "Información", life = 3000) {
  const toast = getToastInstance();
  if (toast) {
    toast.add({
      severity: 'info',
      summary: title,
      detail: message,
      life: life
    });
  } else {
    // Fallback to console log if Toast is not available
    console.info(`ℹ️ ${title}: ${message}`);
  }
}

/**
 * Show access denied toast (specialized error toast)
 * @param {string} customMessage - Optional custom message
 */
export function showAccessDeniedToast(customMessage = null) {
  const message = customMessage || "No tienes permisos para acceder a esta página. Puedes continuar navegando desde aquí.";
  showErrorToast(message, "Acceso Denegado", 5000);
}

/**
 * Show login success toast
 * @param {string} userName - User's name for personalized message
 */
export function showLoginSuccessToast(userName = "Usuario") {
  showSuccessToast(`¡Bienvenido, ${userName}!`, "Inicio de sesión exitoso");
}

/**
 * Show logout success toast
 */
export function showLogoutSuccessToast() {
  showInfoToast("Has cerrado sesión correctamente", "Sesión cerrada");
}

/**
 * Show connection error toast
 */
export function showConnectionErrorToast() {
  showErrorToast("No se pudo conectar con el servidor. Verifica tu conexión.", "Error de Conexión");
}

/**
 * Generic toast function for custom notifications
 * @param {Object} options - Toast options
 * @param {string} options.severity - 'success', 'info', 'warn', 'error'
 * @param {string} options.summary - Toast title
 * @param {string} options.detail - Toast message
 * @param {number} options.life - Duration in milliseconds
 */
export function showToast(options) {
  const toast = getToastInstance();
  if (toast) {
    toast.add({
      severity: options.severity || 'info',
      summary: options.summary || 'Notificación',
      detail: options.detail || '',
      life: options.life || 3000
    });
  } else {
    // Fallback to console log if Toast is not available
    const emoji = {
      success: '✅',
      error: '❌', 
      warn: '⚠️',
      info: 'ℹ️'
    }[options.severity] || 'ℹ️';
    console.log(`${emoji} ${options.summary || 'Notificación'}: ${options.detail || ''}`);
  }
}