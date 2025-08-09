/**
 * Toast notification utilities using PrimeVue ToastService
 */

import { useToast } from 'primevue/usetoast'
import { getCurrentInstance } from 'vue'
import type { ToastServiceMethods } from 'primevue/toastservice'

// Global toast instance - will be set from App.vue or main component
let globalToastInstance: ToastServiceMethods | null = null

// Types
type ToastSeverity = 'success' | 'info' | 'warn' | 'error'

interface ToastOptions {
  severity?: ToastSeverity
  summary?: string
  detail?: string
  life?: number
  group?: string
  closable?: boolean
  sticky?: boolean
}

interface SignalRNotificationData {
  type?: string
  roomId?: number
  visitaId?: number
  [key: string]: any
}

/**
 * Set global toast instance - call this from a Vue component (like App.vue)
 */
export function setGlobalToastInstance(toast: ToastServiceMethods): void {
  globalToastInstance = toast
}

/**
 * Safely get toast instance
 * First tries global instance, then component-based instance
 */
function getToastInstance(): ToastServiceMethods | null {
  // Try global instance first (works from anywhere)
  if (globalToastInstance) {
    return globalToastInstance
  }

  // Fallback to component-based instance
  try {
    const instance = getCurrentInstance()
    if (instance) {
      const toast = useToast()
      // Cache it as global instance for future use
      if (toast) {
        globalToastInstance = toast
      }
      return toast
    }
    return null
  } catch (error) {
    return null
  }
}

/**
 * Show success toast notification
 */
export function showSuccessToast(
  message: string,
  title: string = 'Éxito',
  life: number = 3000
): void {
  const toast = getToastInstance()
  if (toast) {
    toast.add({
      severity: 'success',
      summary: title,
      detail: message,
      life: life,
    })
  } else {
    // Fallback to console log if Toast is not available
    console.log(`✅ ${title}: ${message}`)
  }
}

/**
 * Show error toast notification
 */
export function showErrorToast(
  message: string,
  title: string = 'Error',
  life: number = 5000
): void {
  const toast = getToastInstance()
  if (toast) {
    toast.add({
      severity: 'error',
      summary: title,
      detail: message,
      life: life,
    })
  } else {
    // Fallback to console log if Toast is not available
    console.error(`❌ ${title}: ${message}`)
  }
}

/**
 * Show warning toast notification
 */
export function showWarningToast(
  message: string,
  title: string = 'Advertencia',
  life: number = 4000
): void {
  const toast = getToastInstance()
  if (toast) {
    toast.add({
      severity: 'warn',
      summary: title,
      detail: message,
      life: life,
    })
  } else {
    // Fallback to console log if Toast is not available
    console.warn(`⚠️ ${title}: ${message}`)
  }
}

/**
 * Show info toast notification
 */
export function showInfoToast(
  message: string,
  title: string = 'Información',
  life: number = 3000
): void {
  const toast = getToastInstance()
  if (toast) {
    toast.add({
      severity: 'info',
      summary: title,
      detail: message,
      life: life,
    })
  } else {
    // Fallback to console log if Toast is not available
    console.info(`ℹ️ ${title}: ${message}`)
  }
}

/**
 * Show access denied toast (specialized error toast)
 */
export function showAccessDeniedToast(customMessage?: string | null): void {
  const message =
    customMessage ||
    'No tienes permisos para acceder a esta página. Puedes continuar navegando desde aquí.'
  showErrorToast(message, 'Acceso Denegado', 5000)
}

/**
 * Show login success toast
 */
export function showLoginSuccessToast(userName: string = 'Usuario'): void {
  showSuccessToast(`¡Bienvenido, ${userName}!`, 'Inicio de sesión exitoso')
}

/**
 * Show logout success toast
 */
export function showLogoutSuccessToast(): void {
  showInfoToast('Has cerrado sesión correctamente', 'Sesión cerrada')
}

/**
 * Show connection error toast
 */
export function showConnectionErrorToast(): void {
  showErrorToast('No se pudo conectar con el servidor. Verifica tu conexión.', 'Error de Conexión')
}

/**
 * Generic toast function for custom notifications
 */
export function showToast(options: ToastOptions): void {
  const toast = getToastInstance()
  if (toast) {
    toast.add({
      severity: options.severity || 'info',
      summary: options.summary || 'Notificación',
      detail: options.detail || '',
      life: options.life || 3000,
      group: options.group || undefined,
    })
  } else {
    // Fallback to console log if Toast is not available
    const emojiMap: Record<string, string> = {
      success: '✅',
      error: '❌',
      warn: '⚠️',
      info: 'ℹ️',
    }
    const emoji = emojiMap[options.severity || 'info'] || 'ℹ️'
    console.log(`${emoji} ${options.summary || 'Notificación'}: ${options.detail || ''}`)
  }
}

/**
 * Show SignalR notification with custom styling and titles
 */
export function showSignalRToast(
  type: string,
  message: string,
  data: SignalRNotificationData = {}
): void {
  const severity = mapSignalRTypeToToastSeverity(type)
  const title = getSignalRNotificationTitle(type, data)
  const cleanMessage = cleanSignalRMessage(message)
  const life = getSignalRNotificationLifetime(type)

  showToast({
    severity,
    summary: title,
    detail: cleanMessage,
    life,
    group: 'signalr',
  })
}

/**
 * Map SignalR notification type to PrimeVue Toast severity
 */
function mapSignalRTypeToToastSeverity(type: string): ToastSeverity {
  switch (type.toLowerCase()) {
    case 'success':
      return 'success'
    case 'error':
      return 'error'
    case 'warning':
      return 'warn'
    case 'info':
    default:
      return 'info'
  }
}

/**
 * Get appropriate title for SignalR notification
 */
function getSignalRNotificationTitle(type: string, data: SignalRNotificationData = {}): string {
  if (data.type) {
    const titleMap: Record<string, string> = {
      reservation_created: '🏨 Nueva Reserva',
      reservation_warning: '⚠️ Alerta de Reserva',
      reservation_expired: '⏰ Reserva Expirada',
      room_status_changed: '🚪 Estado de Habitación',
      payment_received: '💰 Pago Recibido',
      checkout_completed: '🏁 Check-out Completado',
      maintenance_alert: '🔧 Mantenimiento',
      inventory_alert: '📦 Inventario',
    }

    if (titleMap[data.type]) {
      return titleMap[data.type]
    }
  }

  return getDefaultSignalRTitle(type)
}

/**
 * Get default title based on notification type
 */
function getDefaultSignalRTitle(type: string): string {
  switch (type.toLowerCase()) {
    case 'success':
      return '✅ Operación Exitosa'
    case 'error':
      return '❌ Error del Sistema'
    case 'warning':
      return '⚠️ Advertencia'
    case 'info':
    default:
      return '🔔 Notificación'
  }
}

/**
 * Clean SignalR message by removing emoji prefixes and notification headers
 */
function cleanSignalRMessage(message: string): string {
  return message
    .replace(/^[✅❌⚠️ℹ️🔔📢🏨⏰🚪💰🔧📦🏁]\s*/, '') // Remove emoji prefixes
    .replace(/^(NOTIFICATION \[.*?\]:\s*)/, '') // Remove "NOTIFICATION [TYPE]:" prefix
    .replace(/^📢\s*/, '') // Remove broadcast emoji
    .trim()
}

/**
 * Get notification lifetime based on type and importance
 */
function getSignalRNotificationLifetime(type: string): number {
  switch (type.toLowerCase()) {
    case 'error':
      return 18000 // Errors stay longer
    case 'warning':
      return 16000 // Warnings stay medium time
    case 'success':
      return 14000 // Success messages
    case 'info':
    default:
      return 14000 // Info messages
  }
}

