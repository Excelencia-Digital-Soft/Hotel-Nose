import { ref } from 'vue'
import { useToast } from 'primevue/usetoast'
import { useConfirm } from 'primevue/useconfirm'
import { ConsumosService } from '../services/consumosService'

export function useConsumos(selectedRoom, useV1Api = false) {
  const toast = useToast()
  const confirm = useConfirm()
  
  const consumos = ref([])
  const editingConsumoId = ref(null)
  const editedCantidad = ref(0)

  // Get consumos for a visita
  const actualizarConsumos = async () => {
    try {
      let response
      if (useV1Api) {
        response = await ConsumosService.getConsumosByVisita(selectedRoom.value.VisitaID)
      } else {
        response = await ConsumosService.getLegacyConsumos(selectedRoom.value.VisitaID)
      }

      if (response && response.data) {
        consumos.value = []
        response.data.forEach(item => {
          const existingItem = consumos.value.find(
            consumo => consumo.articuloId === item.articuloId && consumo.esHabitacion === item.esHabitacion
          )

          if (existingItem) {
            existingItem.cantidad += item.cantidad
            existingItem.total = existingItem.cantidad * existingItem.precioUnitario
          } else {
            consumos.value.push({
              consumoId: item.consumoId,
              articuloId: item.articuloId,
              articleName: item.articleName,
              cantidad: item.cantidad,
              precioUnitario: item.precioUnitario,
              esHabitacion: item.esHabitacion,
              total: item.cantidad * item.precioUnitario
            })
          }
        })
      }
    } catch (error) {
      console.error('Error al obtener los consumos:', error)
      toast.add({
        severity: 'error',
        summary: 'Error',
        detail: 'Error al cargar los consumos',
        life: 10000
      })
    }
  }

  // Add general consumos
  const agregarConsumos = async (selectedItems) => {
    try {
      if (useV1Api) {
        await ConsumosService.addGeneralConsumos(
          selectedRoom.value.VisitaID,
          selectedRoom.value.HabitacionID,
          selectedItems
        )
      } else {
        await ConsumosService.addLegacyGeneralConsumos(
          selectedRoom.value.HabitacionID,
          selectedRoom.value.VisitaID,
          selectedItems
        )
      }
      
      await actualizarConsumos()
      toast.add({
        severity: 'success',
        summary: 'Éxito',
        detail: 'Consumos agregados exitosamente',
        life: 10000
      })
    } catch (error) {
      console.error('Error al agregar consumo general:', error)
      toast.add({
        severity: 'error',
        summary: 'Error',
        detail: 'Error al agregar consumos generales',
        life: 10000
      })
    }
  }

  // Add room consumos
  const agregarConsumosHabitacion = async (selectedItems) => {
    try {
      if (useV1Api) {
        await ConsumosService.addRoomConsumos(
          selectedRoom.value.VisitaID,
          selectedRoom.value.HabitacionID,
          selectedItems
        )
      } else {
        await ConsumosService.addLegacyRoomConsumos(
          selectedRoom.value.HabitacionID,
          selectedRoom.value.VisitaID,
          selectedItems
        )
      }
      
      await actualizarConsumos()
      toast.add({
        severity: 'success',
        summary: 'Éxito',
        detail: 'Consumos de habitación agregados exitosamente',
        life: 10000
      })
    } catch (error) {
      console.error('Error al agregar consumo habitación:', error)
      toast.add({
        severity: 'error',
        summary: 'Error',
        detail: 'Error al agregar consumos de habitación',
        life: 10000
      })
    }
  }

  // Cancel/Delete consumo
  const anularConsumo = (consumoId) => {
    confirm.require({
      message: '¿Está seguro que desea anular este consumo?',
      header: 'Confirmar Anulación',
      icon: 'pi pi-exclamation-triangle',
      acceptLabel: 'Sí, anular',
      rejectLabel: 'Cancelar',
      acceptClass: 'p-button-danger',
      accept: async () => {
        try {
          if (useV1Api) {
            await ConsumosService.cancelConsumo(consumoId)
          } else {
            await ConsumosService.cancelLegacyConsumo(consumoId)
          }
          
          await actualizarConsumos()
          toast.add({
            severity: 'success',
            summary: 'Éxito',
            detail: 'Consumo anulado exitosamente',
            life: 10000
          })
        } catch (error) {
          console.error('Error al anular consumo:', error)
          toast.add({
            severity: 'error',
            summary: 'Error',
            detail: 'Error al anular el consumo',
            life: 10000
          })
        }
      },
      reject: () => {
        toast.add({
          severity: 'info',
          summary: 'Cancelado',
          detail: 'Operación cancelada',
          life: 5000
        })
      }
    })
  }

  // Start editing consumo
  const startEditConsumo = (consumoId) => {
    editingConsumoId.value = consumoId
    const consumo = consumos.value.find(c => c.consumoId === consumoId)
    if (consumo) {
      editedCantidad.value = consumo.cantidad
    }
  }

  // Cancel editing
  const cancelEditConsumo = () => {
    editingConsumoId.value = null
  }

  // Save edited consumo
  const saveConsumo = async (consumoId) => {
    if (editingConsumoId.value === consumoId) {
      try {
        if (useV1Api) {
          await ConsumosService.updateConsumoQuantity(consumoId, editedCantidad.value)
        } else {
          await ConsumosService.updateLegacyConsumo(consumoId, editedCantidad.value)
        }
        
        // Update local state
        const consumo = consumos.value.find(c => c.consumoId === consumoId)
        if (consumo) {
          consumo.cantidad = editedCantidad.value
          consumo.total = consumo.cantidad * consumo.precioUnitario
        }
        
        editingConsumoId.value = null
        toast.add({
          severity: 'success',
          summary: 'Éxito',
          detail: 'Consumo actualizado exitosamente',
          life: 10000
        })
      } catch (error) {
        console.error('Error updating consumo:', error)
        toast.add({
          severity: 'error',
          summary: 'Error',
          detail: 'Error al actualizar el consumo',
          life: 10000
        })
      }
    }
  }

  return {
    consumos,
    editingConsumoId,
    editedCantidad,
    actualizarConsumos,
    agregarConsumos,
    agregarConsumosHabitacion,
    anularConsumo,
    startEditConsumo,
    cancelEditConsumo,
    saveConsumo
  }
}