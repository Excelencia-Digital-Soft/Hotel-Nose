/**
 * Room Filters Composable
 * Manages filtering and search functionality for rooms
 */

import { ref, computed } from 'vue';

export function useRoomFilters(rooms) {
  // Filter states
  const searchTerm = ref('');
  const selectedCategory = ref('');
  const selectedMaintenanceType = ref('');
  const showOnlyOccupied = ref(false);
  const showOnlyMaintenance = ref(false);

  /**
   * Filter free rooms based on current filter criteria
   */
  const filteredFreeRooms = computed(() => {
    if (!rooms.freeRooms) return [];
    
    const hasActiveFilters = searchTerm.value.trim() !== '' || selectedCategory.value !== '';
    
    // If no filters are active, show all free rooms
    if (!hasActiveFilters) {
      return rooms.freeRooms;
    }
    
    // Apply filters
    return rooms.freeRooms.filter(room => {
      const matchesSearch = !searchTerm.value || 
        room.nombreHabitacion.toLowerCase().includes(searchTerm.value.toLowerCase());
      const matchesCategory = !selectedCategory.value || 
        room.nombreHabitacion.includes(selectedCategory.value);
      
      return matchesSearch && matchesCategory;
    });
  });

  /**
   * Filter maintenance rooms based on current filter criteria
   */
  const filteredMaintenanceRooms = computed(() => {
    if (!rooms.maintenanceRooms) return [];
    
    const hasActiveFilters = searchTerm.value.trim() !== '' || selectedCategory.value !== '' || selectedMaintenanceType.value !== '';
    
    // If no filters are active, show all maintenance rooms
    if (!hasActiveFilters) {
      return rooms.maintenanceRooms;
    }
    
    // Apply filters
    return rooms.maintenanceRooms.filter(room => {
      const matchesSearch = !searchTerm.value || 
        room.nombreHabitacion.toLowerCase().includes(searchTerm.value.toLowerCase());
      const matchesCategory = !selectedCategory.value || 
        room.nombreHabitacion.includes(selectedCategory.value);
      const matchesMaintenanceType = !selectedMaintenanceType.value || 
        (room.estado && room.estado.toLowerCase().includes(selectedMaintenanceType.value.toLowerCase()));
      
      return matchesSearch && matchesCategory && matchesMaintenanceType;
    });
  });

  /**
   * Filter occupied rooms based on current filter criteria
   */
  const filteredOccupiedRooms = computed(() => {
    if (!rooms.occupiedRooms) return [];
    
    // If "show only occupied" is not checked, return all occupied rooms
    if (!showOnlyOccupied.value) {
      return rooms.occupiedRooms;
    }
    
    // Apply search and category filters to occupied rooms
    return rooms.occupiedRooms.filter(room => {
      const matchesSearch = !searchTerm.value || 
        room.nombreHabitacion.toLowerCase().includes(searchTerm.value.toLowerCase());
      const matchesCategory = !selectedCategory.value || 
        room.nombreHabitacion.includes(selectedCategory.value);
      
      return matchesSearch && matchesCategory;
    });
  });

  /**
   * Get rooms that are about to expire, with filters applied
   */
  const filteredRoomsAboutToExpire = computed(() => {
    if (!rooms.roomsAboutToExpire) return [];
    
    // If "show only occupied" is not checked, return all about to expire
    if (!showOnlyOccupied.value) {
      return rooms.roomsAboutToExpire;
    }
    
    // Apply filters
    return rooms.roomsAboutToExpire.filter(room => {
      const matchesSearch = !searchTerm.value || 
        room.nombreHabitacion.toLowerCase().includes(searchTerm.value.toLowerCase());
      const matchesCategory = !selectedCategory.value || 
        room.nombreHabitacion.includes(selectedCategory.value);
      
      return matchesSearch && matchesCategory;
    });
  });

  /**
   * Update search term
   */
  const updateSearchTerm = (newTerm) => {
    searchTerm.value = newTerm;
  };

  /**
   * Update selected category
   */
  const updateSelectedCategory = (newCategory) => {
    selectedCategory.value = newCategory;
  };

  /**
   * Update selected maintenance type
   */
  const updateSelectedMaintenanceType = (newType) => {
    selectedMaintenanceType.value = newType;
  };

  /**
   * Toggle show only occupied filter
   */
  const toggleShowOnlyOccupied = () => {
    showOnlyOccupied.value = !showOnlyOccupied.value;
  };

  /**
   * Toggle show only maintenance filter
   */
  const toggleShowOnlyMaintenance = () => {
    showOnlyMaintenance.value = !showOnlyMaintenance.value;
  };

  /**
   * Clear all filters
   */
  const clearFilters = () => {
    searchTerm.value = '';
    selectedCategory.value = '';
    selectedMaintenanceType.value = '';
    showOnlyOccupied.value = false;
    showOnlyMaintenance.value = false;
  };

  /**
   * Check if any filters are active
   */
  const hasActiveFilters = computed(() => {
    return searchTerm.value.trim() !== '' || 
           selectedCategory.value !== '' || 
           selectedMaintenanceType.value !== '' ||
           showOnlyOccupied.value ||
           showOnlyMaintenance.value;
  });

  /**
   * Get filter summary for display
   */
  const filterSummary = computed(() => {
    const filters = [];
    
    if (searchTerm.value.trim()) {
      filters.push(`Búsqueda: "${searchTerm.value}"`);
    }
    
    if (selectedCategory.value) {
      filters.push(`Categoría: ${selectedCategory.value}`);
    }
    
    if (showOnlyOccupied.value) {
      filters.push('Solo ocupadas');
    }
    
    return filters.join(' | ');
  });

  return {
    // Filter states
    searchTerm,
    selectedCategory,
    selectedMaintenanceType,
    showOnlyOccupied,
    showOnlyMaintenance,
    
    // Filtered results
    filteredFreeRooms,
    filteredMaintenanceRooms,
    filteredOccupiedRooms,
    filteredRoomsAboutToExpire,
    
    // Filter actions
    updateSearchTerm,
    updateSelectedCategory,
    updateSelectedMaintenanceType,
    toggleShowOnlyOccupied,
    toggleShowOnlyMaintenance,
    clearFilters,
    
    // Filter information
    hasActiveFilters,
    filterSummary
  };
}