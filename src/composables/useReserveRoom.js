import { ref } from 'vue'

export function useReserveRoom(props) {

  // Room data
  const selectedRoom = ref({
    HabitacionID: 0,
    Disponible: null,
    nombreHabitacion: '',
    FechaReserva: '',
    FechaFin: '',
    PromocionID: 0,
    ReservaID: 0,
    pedidosPendientes: false,
    TotalHoras: 0,
    TotalMinutos: 0,
    UsuarioID: 14,
    PatenteVehiculo: '',
    NumeroTelefono: '',
    Identificador: '',
    esReserva: true,
    Precio: 0,
    VisitaID: 0,
    PausaHoras: 0,
    PausaMinutos: 0
  })

  const horaEntrada = ref('')
  const overtime = ref(0)
  const ignorarTiempo = ref(false)
  const Pausa = ref(false)

  // Initialize room data
  const initializeRoom = () => {
    if (!props.room) return

    selectedRoom.value.nombreHabitacion = props.room.nombreHabitacion
    selectedRoom.value.HabitacionID = props.room.habitacionId
    selectedRoom.value.Disponible = props.room.disponible
    selectedRoom.value.TotalHoras = props.room.reservaActiva?.totalHoras || 0
    selectedRoom.value.TotalMinutos = props.room.reservaActiva?.totalMinutos || 0
    selectedRoom.value.FechaReserva = props.room.reservaActiva?.fechaInicio
    selectedRoom.value.Precio = props.room.precio
    selectedRoom.value.PromocionID = props.room.reservaActiva?.promocionId || 0
    selectedRoom.value.pedidosPendientes = props.room.pedidosPendientes
    selectedRoom.value.ReservaID = props.room.reservaActiva?.reservaId || 0
    selectedRoom.value.VisitaID = props.room.visitaID
    selectedRoom.value.Identificador = props.room.visita?.nombreCompleto || ''
    selectedRoom.value.NumeroTelefono = props.room.visita?.numeroTelefono || ''
    selectedRoom.value.PatenteVehiculo = props.room.visita?.patenteVehiculo || ''
    selectedRoom.value.PausaHoras = props.room.reservaActiva?.pausaHoras ?? 0
    selectedRoom.value.PausaMinutos = props.room.reservaActiva?.pausaMinutos ?? 0

    // Format reservation date for datetime-local input
    if (selectedRoom.value.FechaReserva) {
      const fecha = new Date(selectedRoom.value.FechaReserva)
      fecha.setHours(fecha.getHours() - 3)
      horaEntrada.value = fecha.toISOString().slice(0, 16)
    }
  }

  // Toggle ignore overtime
  const ignorarTiempoExtra = () => {
    ignorarTiempo.value = !ignorarTiempo.value
  }

  return {
    selectedRoom,
    horaEntrada,
    overtime,
    ignorarTiempo,
    Pausa,
    initializeRoom,
    ignorarTiempoExtra
  }
}
