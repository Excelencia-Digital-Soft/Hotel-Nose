import { ref } from 'vue'
import { useToast } from 'primevue/usetoast'
import { useConfirm } from 'primevue/useconfirm'
import { ReservasService } from '../services/reservasService'

export function useReservas(selectedRoom, useV1Api = false) {
  const toast = useToast()
  const confirm = useConfirm()
  
  const modalAnular = ref(false)
  const isProcessingPayment = ref(false)

  // Pause ocupacion
  const pauseOcupacion = async () => {
    try {
      if (useV1Api) {
        await ReservasService.pauseOcupacion(selectedRoom.value.VisitaID)
      } else {
        await ReservasService.legacyPauseOcupacion(selectedRoom.value.VisitaID)
      }
      
      console.log("Habitación pausada exitosamente")
      return true
    } catch (error) {
      console.error("Error al pausar la habitación:", error)
      throw error
    }
  }

  // Resume ocupacion
  const resumeOcupacion = async () => {
    try {
      if (useV1Api) {
        await ReservasService.resumeOcupacion(selectedRoom.value.VisitaID)
      } else {
        // Legacy method doesn't exist, would need to be implemented
        throw new Error('Resume ocupacion not available in legacy API')
      }
      
      console.log("Habitación reanudada exitosamente")
      return true
    } catch (error) {
      console.error("Error al reanudar la habitación:", error)
      throw error
    }
  }

  // Finalize reserva
  const finalizeReserva = async (paymentData = null) => {
    try {
      if (useV1Api) {
        await ReservasService.finalizeReserva(paymentData)
      } else {
        await ReservasService.legacyFinalizeReserva(selectedRoom.value.HabitacionID)
      }
      
      toast.add({
        severity: 'success',
        summary: 'Éxito',
        detail: 'Reserva finalizada exitosamente',
        life: 10000
      })
      
      return true
    } catch (error) {
      console.error('Error al finalizar reserva:', error)
      toast.add({
        severity: 'error',
        summary: 'Error',
        detail: 'Error al finalizar la reserva',
        life: 10000
      })
      throw error
    }
  }

  // Extend time
  const extendTime = async (horas, minutos) => {
    try {
      if (useV1Api) {
        const extensionData = {
          horas,
          minutos,
          reservaId: selectedRoom.value.ReservaID
        }
        await ReservasService.extendTime(selectedRoom.value.ReservaID, extensionData)
      } else {
        // Legacy: Update local state directly as original code does
        selectedRoom.value.TotalHoras = selectedRoom.value.TotalHoras + horas
        selectedRoom.value.TotalMinutos = selectedRoom.value.TotalMinutos + minutos
      }
      
      toast.add({
        severity: 'success',
        summary: 'Éxito',
        detail: 'Tiempo extendido exitosamente',
        life: 10000
      })
      
      return true
    } catch (error) {
      console.error('Error al extender tiempo:', error)
      toast.add({
        severity: 'error',
        summary: 'Error',
        detail: 'Error al extender el tiempo',
        life: 10000
      })
      throw error
    }
  }

  // Cancel reserva
  const cancelReserva = async () => {
    return new Promise((resolve, reject) => {
      confirm.require({
        message: '¿Está seguro que desea anular esta reserva?',
        header: 'Confirmar Anulación',
        icon: 'pi pi-exclamation-triangle',
        acceptLabel: 'Sí, anular',
        rejectLabel: 'Cancelar',
        acceptClass: 'p-button-danger',
        accept: async () => {
          try {
            if (useV1Api) {
              await ReservasService.cancelReserva(selectedRoom.value.ReservaID)
            } else {
              // Legacy method would need to be called through AnularOcupacionModal
              console.log('Using legacy modal for cancellation')
            }
            
            toast.add({
              severity: 'success',
              summary: 'Éxito',
              detail: 'Reserva anulada exitosamente',
              life: 10000
            })
            
            resolve(true)
          } catch (error) {
            console.error('Error al anular reserva:', error)
            toast.add({
              severity: 'error',
              summary: 'Error',
              detail: 'Error al anular la reserva',
              life: 10000
            })
            reject(error)
          }
        },
        reject: () => {
          toast.add({
            severity: 'info',
            summary: 'Cancelado',
            detail: 'Operación cancelada',
            life: 5000
          })
          resolve(false)
        }
      })
    })
  }

  // Update promotion
  const updatePromotion = async (promocionId) => {
    try {
      if (useV1Api) {
        await ReservasService.updatePromotion(selectedRoom.value.ReservaID, promocionId)
      } else {
        await ReservasService.legacyUpdatePromotion(selectedRoom.value.ReservaID, promocionId)
      }
      
      console.log("Promoción actualizada correctamente")
      return true
    } catch (error) {
      console.error("Error actualizando la promoción:", error)
      throw error
    }
  }

  // Modal handlers
  const toggleAnularOcupacionModal = () => {
    modalAnular.value = !modalAnular.value
  }

  const handleOcupacionAnulada = (reservaId) => {
    console.log('Ocupación anulada para reserva:', reservaId)
    modalAnular.value = false
    
    // Reload page to update rooms
    setTimeout(() => {
      window.location.reload()
    }, 1500)
  }

  return {
    modalAnular,
    isProcessingPayment,
    pauseOcupacion,
    resumeOcupacion,
    finalizeReserva,
    extendTime,
    cancelReserva,
    updatePromotion,
    toggleAnularOcupacionModal,
    handleOcupacionAnulada
  }
}