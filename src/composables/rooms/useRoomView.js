/**
 * Room View Composable
 * Manages view settings, layout preferences, and UI state for rooms
 */

import { ref, computed, onMounted } from 'vue';
import { useRoomsStore } from '../../store/modules/roomsStore';

export function useRoomView() {
  const roomsStore = useRoomsStore();

  // View mode settings
  const viewMode = ref('grid'); // 'grid' | 'list'
  const compactMode = ref(false);

  // UI state
  const isHeaderCollapsed = ref(false);
  const showStatsPanel = ref(true);

  /**
   * Toggle between grid and list view
   */
  const toggleViewMode = () => {
    viewMode.value = viewMode.value === 'grid' ? 'list' : 'grid';

    // Update store and persist preference
    roomsStore.updateViewSettings({ mode: viewMode.value });
  };

  /**
   * Toggle compact mode
   */
  const toggleCompactMode = () => {
    compactMode.value = !compactMode.value;

    // Update store and persist preference
    roomsStore.updateViewSettings({ compactMode: compactMode.value });
  };

  /**
   * Toggle header collapsed state
   */
  const toggleHeaderCollapse = () => {
    isHeaderCollapsed.value = !isHeaderCollapsed.value;
  };

  /**
   * Toggle stats panel visibility
   */
  const toggleStatsPanel = () => {
    showStatsPanel.value = !showStatsPanel.value;
  };

  /**
   * Load saved view preferences
   */
  const loadViewPreferences = () => {
    // Load from store first
    roomsStore.loadViewPreferences();

    // Update local refs
    viewMode.value = roomsStore.viewSettings.mode;
    compactMode.value = roomsStore.viewSettings.compactMode;

    // Load other UI preferences
    const savedHeaderState = localStorage.getItem('roomsHeaderCollapsed');
    if (savedHeaderState !== null) {
      isHeaderCollapsed.value = savedHeaderState === 'true';
    }

    const savedStatsPanel = localStorage.getItem('roomsShowStatsPanel');
    if (savedStatsPanel !== null) {
      showStatsPanel.value = savedStatsPanel === 'true';
    }
  };

  /**
   * Save UI preferences
   */
  const saveUIPreferences = () => {
    localStorage.setItem('roomsHeaderCollapsed', isHeaderCollapsed.value.toString());
    localStorage.setItem('roomsShowStatsPanel', showStatsPanel.value.toString());
  };

  /**
   * Computed styles for responsive grid
   */
  const gridColumns = computed(() => {
    if (viewMode.value === 'list') return 1;

    if (compactMode.value) {
      return {
        'grid-cols-2': true,
        'lg:grid-cols-4': true,
        'xl:grid-cols-5': true,
        '2xl:grid-cols-6': true
      };
    }

    return {
      'grid-cols-2': true,
      'lg:grid-cols-3': true,
      'xl:grid-cols-4': true,
      '2xl:grid-cols-4': true
    };
  });

  /**
   * Computed classes for room cards
   */
  const roomCardClasses = computed(() => {
    const baseClasses = 'transition-all duration-300 cursor-pointer backdrop-blur-md hover:scale-105';

    if (compactMode.value) {
      return `${baseClasses} p-3 rounded-xl`;
    }

    return `${baseClasses} p-5 rounded-2xl`;
  });

  /**
   * Header classes based on state
   */
  const headerClasses = computed(() => {
    const baseClasses = 'bg-white/5 backdrop-blur-xl border border-white/10 rounded-3xl shadow-2xl transition-all duration-300';

    if (isHeaderCollapsed.value) {
      return `${baseClasses} p-4`;
    }

    return `${baseClasses} ${compactMode.value ? 'p-4' : 'p-6'}`;
  });

  /**
   * Stats panel classes
   */
  const statsPanelClasses = computed(() => {
    if (!showStatsPanel.value) return 'hidden';

    if (compactMode.value) {
      return 'flex flex-wrap gap-3 mt-4';
    }

    return 'grid grid-cols-2 md:grid-cols-3 gap-4 mt-8';
  });

  /**
   * Get view mode icon
   */
  const viewModeIcon = computed(() => {
    return viewMode.value === 'grid' ? 'view_list' : 'grid_view';
  });

  /**
   * Get view mode label
   */
  const viewModeLabel = computed(() => {
    return viewMode.value === 'grid' ? 'Lista' : 'Grid';
  });

  /**
   * Get compact mode icon
   */
  const compactModeIcon = computed(() => {
    return compactMode.value ? 'unfold_more' : 'unfold_less';
  });

  /**
   * Get compact mode label
   */
  const compactModeLabel = computed(() => {
    return compactMode.value ? 'Expandir' : 'Compacto';
  });

  /**
   * Room display settings for different view modes
   */
  const roomDisplaySettings = computed(() => {
    if (viewMode.value === 'list') {
      return {
        showImages: false,
        showFullDetails: true,
        layout: 'horizontal'
      };
    }

    if (compactMode.value) {
      return {
        showImages: false,
        showFullDetails: false,
        layout: 'vertical-compact'
      };
    }

    return {
      showImages: true,
      showFullDetails: true,
      layout: 'vertical'
    };
  });

  // Initialize view preferences on mount
  onMounted(() => {
    loadViewPreferences();
  });

  // Watch for changes and save preferences
  const savePreferencesDebounced = (() => {
    let timeout;
    return () => {
      clearTimeout(timeout);
      timeout = setTimeout(saveUIPreferences, 500);
    };
  })();

  // Auto-save preferences when they change
  const watchPreferences = () => {
    // This would typically use Vue's watch API, but for simplicity we'll handle it manually
    savePreferencesDebounced();
  };

  return {
    // View state
    viewMode,
    compactMode,
    isHeaderCollapsed,
    showStatsPanel,

    // View actions
    toggleViewMode,
    toggleCompactMode,
    toggleHeaderCollapse,
    toggleStatsPanel,

    // Preferences management
    loadViewPreferences,
    saveUIPreferences,

    // Computed styles and classes
    gridColumns,
    roomCardClasses,
    headerClasses,
    statsPanelClasses,

    // View information
    viewModeIcon,
    viewModeLabel,
    compactModeIcon,
    compactModeLabel,
    roomDisplaySettings
  };
}
