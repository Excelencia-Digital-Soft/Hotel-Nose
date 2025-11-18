import { ref, watch, onMounted, type Ref } from 'vue'
import { PromocionesService } from '../services/promocionesService'
import { ReservasService } from '../services/reservasService'
import type { ApiResponse, PromocionDto, Habitacion } from '../types'

// Component props interface
interface UsePromocionesProps {
  room: Habitacion & {
    categoriaId?: number
    reservaActiva?: {
      promocionId?: number | null
    }
  }
}

// Component emits interface
interface UsePromocionesEmits {
  (event: 'update-room', room: any): void
}

// Extended room interface with promocion data
interface ExtendedRoom extends Habitacion {
  PromocionID?: number | null
  promocionId?: number | null
}

export function usePromociones(
  props: UsePromocionesProps,
  selectedRoom: Ref<ExtendedRoom>,
  emits: UsePromocionesEmits
) {
  // Reactive state
  const selectedPromocion = ref<PromocionDto | null>(null)
  const promociones = ref<PromocionDto[]>([])
  const promocionActiva = ref<boolean>(false)

  // Load promotions
  const loadPromociones = async (): Promise<void> => {
    try {
      // Validate categoriaId exists
      if (!props.room.categoriaId) {
        console.warn('No categoriaId available for loading promociones')
        promociones.value = []
        return
      }

      // Use V1 API only
      const response: ApiResponse<PromocionDto[]> =
        await PromocionesService.getPromocionesByCategoria(props.room.categoriaId)

      // Handle V1 API response with proper typing
      if (response?.isSuccess && response?.data) {
        promociones.value = response.data
      } else {
        console.warn('No promociones found or invalid response', response)
        promociones.value = []
      }

      console.log('Promociones loaded:', promociones.value.length, 'items')
    } catch (error) {
      console.error('Error fetching promociones:', error)
      promociones.value = []
    }

    // Set active promotion if exists
    if (selectedRoom.value.PromocionID != null) {
      const matchedPromo = promociones.value.find(
        (promo: PromocionDto) => promo.promocionId === selectedRoom.value.PromocionID
      )

      if (matchedPromo) {
        selectedPromocion.value = matchedPromo
        promocionActiva.value = true
        console.log('Active promocion set:', matchedPromo.nombrePromocion)
      }
    }
  }

  // Update promotion
  const actualizarPromocion = async (): Promise<void> => {
    if (!selectedRoom.value || !selectedRoom.value.ReservaID) {
      console.error('Reserva or HabitacionID is not set.')
      return
    }

    const reservaId: number = selectedRoom.value.ReservaID
    const promocionId: number | null = selectedPromocion.value
      ? selectedPromocion.value.promocionId
      : null

    try {
      // Use V1 API only
      await ReservasService.updatePromotion(reservaId, promocionId)

      console.log(
        'Promoción actualizada correctamente',
        promocionId ? `ID: ${promocionId}` : 'Removed'
      )

      // Update room data with new promocion
      const updatedRoom = {
        ...props.room,
        promocionId: promocionId,
      }
      emits('update-room', updatedRoom)

      promocionActiva.value = promocionId !== null
    } catch (error) {
      console.error('Error actualizando la promoción:', error)
    }
  }

  // Watch for promotion changes
  watch(selectedPromocion, (newVal: PromocionDto | null) => {
    promocionActiva.value = newVal !== null

    // Update reserva activa if exists
    if (props.room?.reservaActiva) {
      if (promocionActiva.value && selectedPromocion.value?.promocionId) {
        props.room.reservaActiva.promocionId = selectedPromocion.value.promocionId
      } else {
        props.room.reservaActiva.promocionId = null
      }
    }

    // Trigger promotion update
    actualizarPromocion()
  })

  // Lifecycle hooks
  onMounted(() => {
    loadPromociones()
  })

  // Return composable interface
  return {
    selectedPromocion,
    promociones,
    promocionActiva,
    loadPromociones,
    actualizarPromocion,
  }
}

// Export types for component usage
export type UsePromocionesReturn = ReturnType<typeof usePromociones>

