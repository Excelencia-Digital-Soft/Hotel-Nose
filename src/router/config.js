/**
 * Router configuration constants and settings
 */

export const ROUTER_CONFIG = {
  // Base path for the application
  BASE_PATH: '/hotel/',

  // Default routes
  DEFAULT_ROUTE: '/',
  LOGIN_ROUTE: '/guest',
  FALLBACK_ROUTE: '/guest',

  // Route names for programmatic navigation
  ROUTE_NAMES: {
    HOME: 'Home',
    LOGIN: 'Guest',
    ROOMS: 'Rooms',
    ROOM_CREATE: 'RoomCreate',
    CATEGORY_CREATE: 'CategoryCreate',
    ARTICLE_CREATE: 'ArticleCreate',
    SUBMIT_ORDER: 'SubmitOrder',
    RECEPTION_ORDER: 'ReceptionOrder',
    INVENTORY_MANAGER: 'InventoryManager',
    // V1 Inventory System Routes
    INVENTORY_ALERTS: 'InventoryAlerts',
    INVENTORY_TRANSFERS: 'InventoryTransfers',
    // Legacy and other routes
    CIERRES_MANAGER: 'CierresManager',
    EMPENOS_MANAGER: 'EmpenosManager',
    PROMOCIONES_MANAGER: 'PromocionesManager',
    EGRESOS: 'Egresos',
    USUARIOS_MANAGER: 'UsuariosManager',
    MEDIOS_PAGO: 'MediosPago',
    CARACTERISTICAS_MANAGER: 'CaracteristicasManager',
    SELECT_ROOM: 'SelectRoom',
  },

  // Navigation menu categories for UI generation
  MENU_CATEGORIES: {
    DASHBOARD: 'Dashboard',
    ROOM_MANAGEMENT: 'Room Management',
    INVENTORY_MANAGEMENT: 'Inventory Management',
    FINANCIAL: 'Financial',
    ORDERS: 'Orders',
    MARKETING: 'Marketing',
    USER_MANAGEMENT: 'User Management',
  },
}

/**
 * Route metadata helpers
 */
export const createRouteMeta = ({
  requireAuth = true,
  roles = null,
  description = '',
  category = '',
  icon = null,
}) => ({
  requireAuth,
  roles,
  description,
  category,
  icon,
})

/**
 * Route generation helpers
 */
export const createRoute = (path, name, component, meta = {}) => ({
  path,
  name,
  component,
  meta: createRouteMeta(meta),
})

