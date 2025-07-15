import { ref, watch, onMounted } from 'vue'
import { PromocionesService } from '../services/promocionesService'
import { ReservasService } from '../services/reservasService'

export function usePromociones(props, selectedRoom, emits, useV1Api = false) {
  const selectedPromocion = ref(null)
  const promociones = ref([])
  const promocionActiva = ref(false)

  // Load promotions
  const loadPromociones = async () => {
    try {
      let response
      if (useV1Api) {
        response = await PromocionesService.getPromocionesByCategoria(props.room.categoriaId)
      } else {
        response = await PromocionesService.getLegacyPromocionesByCategoria(props.room.categoriaId)
      }
      
      if (response.data?.isSuccess && response.data?.data) {
        promociones.value = response.data.data
      } else if (response.data?.data) {
        promociones.value = response.data.data
      } else if (Array.isArray(response.data)) {
        promociones.value = response.data
      } else {
        promociones.value = []
      }
      
      console.log('Promociones loaded:', promociones.value)
    } catch (error) {
      console.error('Error fetching promociones:', error)
      promociones.value = []
    }

    // Set active promotion if exists
    if (selectedRoom.value.PromocionID != null) {
      const matchedPromo = promociones.value.find(
        (promo) => promo.promocionID === selectedRoom.value.PromocionID
      )

      if (matchedPromo) {
        selectedPromocion.value = matchedPromo
        promocionActiva.value = true
      }
    }
  }

  // Update promotion
  const actualizarPromocion = async () => {
    if (!selectedRoom.value || !selectedRoom.value.ReservaID) {
      console.error("Reserva or HabitacionID is not set.")
      return
    }

    const reservaId = selectedRoom.value.ReservaID
    const promocionId = selectedPromocion.value ? selectedPromocion.value.promocionID : null

    try {
      if (useV1Api) {
        await ReservasService.updatePromotion(reservaId, promocionId)
      } else {
        await ReservasService.legacyUpdatePromotion(reservaId, promocionId)
      }
      
      console.log("Promoción actualizada correctamente")
      
      const updatedRoom = { ...props.room, promocionID: promocionId }
      emits('update-room', updatedRoom)

      promocionActiva.value = promocionId !== null
    } catch (error) {
      console.error("Error actualizando la promoción:", error)
    }
  }

  // Watch for promotion changes
  watch(selectedPromocion, (newVal) => {
    promocionActiva.value = newVal !== null
    
    if (props.room?.reservaActiva) {
      if (promocionActiva.value && selectedPromocion.value?.promocionID) {
        props.room.reservaActiva.promocionId = selectedPromocion.value.promocionID
      } else {
        props.room.reservaActiva.promocionId = null
      }
    }
    
    actualizarPromocion()
  })

  onMounted(() => {
    loadPromociones()
  })

  return {
    selectedPromocion,
    promociones,
    promocionActiva,
    loadPromociones,
    actualizarPromocion
  }
}