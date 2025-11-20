import { ref, onMounted, onUnmounted } from 'vue'
import dayjs from 'dayjs'

export function useTimer(selectedRoom, ignorarTiempo, Pausa, overtime) {
  const formattedTime = ref('')
  const timerUpdateInterval = ref(10)
  
  let timerInterval = null
  let additionalCalculationInterval = null

  // Timer calculation logic for display
  const calculateRemainingTime = () => {
    if (selectedRoom.value.PausaHoras == 0 && selectedRoom.value.PausaMinutos == 0) {
      const endTime = dayjs(selectedRoom.value.FechaReserva)
        .add(selectedRoom.value.TotalHoras, 'hour')
        .add(selectedRoom.value.TotalMinutos, 'minute')
      const now = dayjs()
      const diffInMinutes = endTime.diff(now, 'minute')
      const isOvertime = diffInMinutes < 0
      
      const hours = Math.floor(Math.abs(diffInMinutes) / 60)
      const minutes = Math.abs(diffInMinutes) % 60
      
      if (isOvertime && ignorarTiempo.value == false) {
        formattedTime.value = `-${String(hours).padStart(2, '0')}:${String(minutes).padStart(2, '0')}`
      } else if (isOvertime) {
        formattedTime.value = `00:00`
      } else {
        formattedTime.value = `${String(hours).padStart(2, '0')}:${String(minutes).padStart(2, '0')}`
      }
    } else {
      const absolutePausaHoras = Math.abs(selectedRoom.value.PausaHoras)
      const absolutePausaMinutos = Math.abs(selectedRoom.value.PausaMinutos)

      if (selectedRoom.value.PausaHoras < 0 || selectedRoom.value.PausaMinutos < 0) {
        Pausa.value = true
        if (ignorarTiempo.value == false) {
          formattedTime.value = `-${String(absolutePausaHoras).padStart(2, '0')}:${String(absolutePausaMinutos).padStart(2, '0')}`
        } else {
          formattedTime.value = `00:00`
        }
      } else if (selectedRoom.value.PausaHoras > 0 || selectedRoom.value.PausaMinutos > 0) {
        Pausa.value = true
        formattedTime.value = `${String(absolutePausaHoras).padStart(2, '0')}:${String(absolutePausaMinutos).padStart(2, '0')}`
      }
    }
  }

  // Calculate overtime for billing
  const calculateOvertimeForBilling = () => {
    if (selectedRoom.value.PausaHoras == 0 && selectedRoom.value.PausaMinutos == 0) {
      const endTime = dayjs(selectedRoom.value.FechaReserva)
        .add(selectedRoom.value.TotalHoras, 'hour')
        .add(selectedRoom.value.TotalMinutos, 'minute')
      const now = dayjs()
      const diffInMinutes = endTime.diff(now, 'minute')
      const isOvertime = diffInMinutes < 0
      
      if (isOvertime && ignorarTiempo.value == false) {
        return diffInMinutes * (-1)
      } else if (isOvertime) {
        return 0
      } else {
        return 0
      }
    } else {
      const absolutePausaHoras = Math.abs(selectedRoom.value.PausaHoras)
      const absolutePausaMinutos = Math.abs(selectedRoom.value.PausaMinutos)

      if (selectedRoom.value.PausaHoras < 0 || selectedRoom.value.PausaMinutos < 0) {
        Pausa.value = true
        if (ignorarTiempo.value == false) {
          return absolutePausaHoras * 60 + absolutePausaMinutos
        } else {
          return 0
        }
      }
    }
    return 0
  }

  // Get timer interval from localStorage
  const getTimerUpdateInterval = () => {
    const storedInterval = localStorage.getItem('timerUpdateInterval')
    const intervalMinutes = storedInterval ? parseInt(storedInterval) : 10
    timerUpdateInterval.value = intervalMinutes
    return intervalMinutes
  }

  // Start timer intervals
  const startTimerIntervals = () => {
    if (timerInterval) clearInterval(timerInterval)
    if (additionalCalculationInterval) clearInterval(additionalCalculationInterval)
    
    timerInterval = setInterval(calculateRemainingTime, 1000)
    
    const intervalMs = timerUpdateInterval.value * 60 * 1000
    additionalCalculationInterval = setInterval(() => {
      const overtimeValue = calculateOvertimeForBilling()
      overtime.value = overtimeValue
    }, intervalMs)
    
    calculateRemainingTime()
    // Calculate initial overtime
    overtime.value = calculateOvertimeForBilling()
  }

  // Stop timer intervals
  const stopTimerIntervals = () => {
    if (timerInterval) clearInterval(timerInterval)
    if (additionalCalculationInterval) clearInterval(additionalCalculationInterval)
  }

  onMounted(() => {
    getTimerUpdateInterval()
  })

  onUnmounted(() => {
    stopTimerIntervals()
  })

  return {
    formattedTime,
    timerUpdateInterval,
    calculateRemainingTime,
    calculateOvertimeForBilling,
    startTimerIntervals,
    stopTimerIntervals,
    getTimerUpdateInterval
  }
}