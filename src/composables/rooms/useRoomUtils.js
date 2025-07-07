/**
 * Room Utilities Composable
 * Contains utility functions for room categorization, formatting, and calculations
 */

import { computed } from 'vue';
import dayjs from 'dayjs';

export function useRoomUtils() {
  /**
   * Extract category from room name
   */
  const getCategoryFromName = (roomName) => {
    if (roomName.includes('HIDROMAX')) return 'HIDROMAX';
    if (roomName.includes('HIDRO')) return 'HIDRO';
    if (roomName.includes('MASTER')) return 'MASTER';
    if (roomName.includes('PENTHOUSE')) return 'PENTHOUSE';
    if (roomName.includes('SUITE')) return 'SUITE';
    if (roomName.includes('CLASICA')) return 'CLÁSICA';
    return 'STANDARD';
  };

  /**
   * Extract room number from room name
   */
  const getRoomNumber = (roomName) => {
    const match = roomName.match(/^(\d+)/);
    return match ? `#${match[1]}` : roomName;
  };

  /**
   * Get descriptive room type
   */
  const getRoomType = (roomName) => {
    const category = getCategoryFromName(roomName);
    const specialNames = {
      'CUARTO ROJO DEL PLACER': 'Temática Especial',
      'BLACK AND WITHE': 'Diseño Exclusivo',
      'PATIO IN': 'Con Patio',
      'ESPEJOS': 'Con Espejos'
    };
    
    for (const [key, value] of Object.entries(specialNames)) {
      if (roomName.includes(key)) {
        return value;
      }
    }
    
    return category;
  };

  /**
   * Get Material Icons icon for room type
   */
  const getRoomIcon = (roomName) => {
    const category = getCategoryFromName(roomName);
    const icons = {
      'CLÁSICA': 'hotel',
      'SUITE': 'hotel_class',
      'MASTER': 'emoji_events',
      'HIDROMAX': 'spa',
      'HIDRO': 'hot_tub',
      'PENTHOUSE': 'apartment'
    };
    return icons[category] || 'hotel';
  };

  /**
   * Calculate time left in minutes for a room reservation
   */
  const getTimeLeftInMinutes = (room) => {
    if (!room.reservaActiva) return 0;
    
    // Handle both old and new API formats
    let endTime;
    if (room.reservaActiva.fechaFin) {
      // New API format with fechaFin
      endTime = dayjs(room.reservaActiva.fechaFin);
    } else if (room.reservaActiva.fechaInicio) {
      // New API format with fechaInicio + totalHoras/totalMinutos
      endTime = dayjs(room.reservaActiva.fechaInicio)
        .add(room.reservaActiva.totalHoras || 0, 'hour')
        .add(room.reservaActiva.totalMinutos || 0, 'minute');
    } else if (room.reservaActiva.fechaReserva) {
      // Old API format
      endTime = dayjs(room.reservaActiva.fechaReserva)
        .add(room.reservaActiva.totalHoras || 0, 'hour')
        .add(room.reservaActiva.totalMinutos || 0, 'minute');
    } else {
      return 0;
    }
    
    return endTime.diff(dayjs(), 'minute');
  };

  /**
   * Format time remaining for display
   */
  const getTimeRemaining = (room) => {
    if (!room.reservaActiva) return 'Sin reserva';
    
    const timeLeft = getTimeLeftInMinutes(room);
    
    if (timeLeft <= 0) {
      const overtime = Math.abs(timeLeft);
      const hours = Math.floor(overtime / 60);
      const minutes = overtime % 60;
      return `+${hours.toString().padStart(2, '0')}:${minutes.toString().padStart(2, '0')}`;
    }
    
    const hours = Math.floor(timeLeft / 60);
    const minutes = timeLeft % 60;
    return `${hours.toString().padStart(2, '0')}:${minutes.toString().padStart(2, '0')}`;
  };

  /**
   * Calculate progress percentage for time elapsed
   */
  const getTimeProgress = (room) => {
    if (!room.reservaActiva) return 0;
    
    const totalMinutes = (room.reservaActiva.totalHoras || 0) * 60 + (room.reservaActiva.totalMinutos || 0);
    
    // If we have fechaInicio and fechaFin, calculate based on actual time range
    if (room.reservaActiva.fechaInicio && room.reservaActiva.fechaFin) {
      const start = dayjs(room.reservaActiva.fechaInicio);
      const end = dayjs(room.reservaActiva.fechaFin);
      const now = dayjs();
      
      const totalDuration = end.diff(start, 'minute');
      const elapsed = now.diff(start, 'minute');
      
      return Math.max(0, Math.min(100, (elapsed / totalDuration) * 100));
    }
    
    // Fallback to the old calculation
    const timeLeft = getTimeLeftInMinutes(room);
    const elapsed = totalMinutes - timeLeft;
    
    return Math.max(0, Math.min(100, (elapsed / totalMinutes) * 100));
  };

  /**
   * Get status text based on time remaining
   */
  const getStatusText = (room) => {
    const timeLeft = getTimeLeftInMinutes(room);
    if (timeLeft <= 0) return 'Tiempo vencido';
    if (timeLeft <= 15) return 'Por vencer';
    return 'En curso';
  };

  /**
   * Get status indicator CSS classes
   */
  const getStatusIndicator = (room) => {
    const timeLeft = getTimeLeftInMinutes(room);
    const baseClasses = 'w-3 h-3 rounded-full animate-pulse shadow-lg';
    
    if (timeLeft <= 0) return `${baseClasses} bg-red-500 shadow-red-500/50`;
    if (timeLeft <= 15) return `${baseClasses} bg-yellow-500 shadow-yellow-500/50`;
    return `${baseClasses} bg-green-500 shadow-green-500/50`;
  };

  /**
   * Get text color classes based on room status
   */
  const getTimeTextColor = (room) => {
    const timeLeft = getTimeLeftInMinutes(room);
    if (timeLeft <= 0) return 'text-red-300';
    if (timeLeft <= 15) return 'text-yellow-300';
    return 'text-gray-300';
  };

  /**
   * Get progress bar color classes
   */
  const getProgressBarColor = (room) => {
    const timeLeft = getTimeLeftInMinutes(room);
    if (timeLeft <= 0) return 'bg-gradient-to-r from-red-500 to-red-600';
    if (timeLeft <= 15) return 'bg-gradient-to-r from-yellow-500 to-amber-500';
    return 'bg-gradient-to-r from-green-500 to-emerald-500';
  };

  /**
   * Get CSS classes for occupied room styling
   */
  const getOccupiedRoomStyles = (room) => {
    const timeLeft = getTimeLeftInMinutes(room);
    const baseClasses = 'border-white/20 bg-gradient-to-br transition-all duration-500';
    
    if (timeLeft <= 0) {
      return `${baseClasses} border-red-500/50 from-red-900/20 to-red-800/20 hover:border-red-400 hover:shadow-red-500/25`;
    }
    if (timeLeft <= 15) {
      return `${baseClasses} border-yellow-500/50 from-yellow-900/20 to-yellow-800/20 hover:border-yellow-400 hover:shadow-yellow-500/25`;
    }
    return `${baseClasses} from-white/10 to-white/5 hover:border-gray-400/50 hover:shadow-white/10`;
  };

  /**
   * Check if room has pending orders
   */
  const hasPendingOrders = (room) => {
    return Boolean(room.pedidosPendientes);
  };

  /**
   * Check if room is about to expire (within 15 minutes)
   */
  const isAboutToExpire = (room) => {
    const timeLeft = getTimeLeftInMinutes(room);
    return timeLeft > 0 && timeLeft <= 15;
  };

  /**
   * Check if room time has expired
   */
  const hasExpired = (room) => {
    const timeLeft = getTimeLeftInMinutes(room);
    return timeLeft <= 0;
  };

  /**
   * Get room priority for sorting (expired > about to expire > normal)
   */
  const getRoomPriority = (room) => {
    if (hasExpired(room)) return 3;
    if (isAboutToExpire(room)) return 2;
    return 1;
  };

  /**
   * Check if room is in maintenance
   */
  const isInMaintenance = (room) => {
    return room.estado && room.estado.toLowerCase() !== 'disponible' && room.disponible === true;
  };

  /**
   * Check if room is completely free (not occupied and not in maintenance)
   */
  const isCompletelyFree = (room) => {
    return room.disponible === true && (!room.estado || room.estado.toLowerCase() === 'disponible');
  };

  /**
   * Get maintenance type from room estado
   */
  const getMaintenanceType = (room) => {
    if (!isInMaintenance(room)) return null;
    
    const estado = room.estado.toLowerCase();
    const maintenanceTypes = {
      'limpieza': 'Limpieza',
      'mantenimiento': 'Mantenimiento',
      'reparacion': 'Reparación',
      'inspeccion': 'Inspección',
      'desinfeccion': 'Desinfección',
      'renovacion': 'Renovación'
    };
    
    return maintenanceTypes[estado] || room.estado;
  };

  /**
   * Get maintenance icon for room estado
   */
  const getMaintenanceIcon = (room) => {
    if (!isInMaintenance(room)) return null;
    
    const estado = room.estado.toLowerCase();
    const icons = {
      'limpieza': 'cleaning_services',
      'mantenimiento': 'build',
      'reparacion': 'handyman',
      'inspeccion': 'search',
      'desinfeccion': 'sanitizer',
      'renovacion': 'home_repair_service'
    };
    
    return icons[estado] || 'engineering';
  };

  /**
   * Get room status for display
   */
  const getRoomStatus = (room) => {
    if (!room.disponible) return 'occupied';
    if (isInMaintenance(room)) return 'maintenance';
    return 'free';
  };

  /**
   * Available room categories for filtering
   */
  const roomCategories = [
    { value: '', label: 'Todas las categorías' },
    { value: 'CLASICA', label: 'Clásica' },
    { value: 'SUITE', label: 'Suite' },
    { value: 'MASTER SUITE', label: 'Master Suite' },
    { value: 'HIDRO SUITE', label: 'Hidro Suite' },
    { value: 'HIDROMAX SUITE', label: 'Hidromax Suite' },
    { value: 'PENTHOUSE', label: 'Penthouse' }
  ];

  /**
   * Available maintenance types for filtering
   */
  const maintenanceTypes = [
    { value: '', label: 'Todos los tipos' },
    { value: 'limpieza', label: 'Limpieza' },
    { value: 'mantenimiento', label: 'Mantenimiento' },
    { value: 'reparacion', label: 'Reparación' },
    { value: 'inspeccion', label: 'Inspección' },
    { value: 'desinfeccion', label: 'Desinfección' },
    { value: 'renovacion', label: 'Renovación' }
  ];

  return {
    // Room information
    getCategoryFromName,
    getRoomNumber,
    getRoomType,
    getRoomIcon,
    roomCategories,
    
    // Time calculations
    getTimeLeftInMinutes,
    getTimeRemaining,
    getTimeProgress,
    
    // Status information
    getStatusText,
    getStatusIndicator,
    getTimeTextColor,
    getProgressBarColor,
    getOccupiedRoomStyles,
    
    // Status checks
    hasPendingOrders,
    isAboutToExpire,
    hasExpired,
    getRoomPriority,
    
    // Maintenance functions
    isInMaintenance,
    isCompletelyFree,
    getMaintenanceType,
    getMaintenanceIcon,
    getRoomStatus,
    maintenanceTypes
  };
}