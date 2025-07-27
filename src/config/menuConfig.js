import { ROLE_GROUPS } from '../utils/role-mapping.js'

/**
 * Operational menu configuration
 * Business-specific functionality organized by domain
 */
export const operationalMenuConfig = [
  {
    id: 'orders',
    label: 'Pedidos',
    icon: 'receipt_long',
    variant: 'secondary',
    roles: [...ROLE_GROUPS.BASIC_ACCESS, ...ROLE_GROUPS.CASHIER_ACCESS],
    items: [
      {
        label: 'Recibir Pedido',
        route: { name: 'ReceptionOrder' },
        icon: 'inbox',
        roles: [...ROLE_GROUPS.BASIC_ACCESS, ...ROLE_GROUPS.CASHIER_ACCESS],
      },
      {
        label: 'Historial Pedidos',
        route: { name: 'OrderHistory' },
        icon: 'history',
        roles: [...ROLE_GROUPS.ADMIN_ACCESS, ...ROLE_GROUPS.CASHIER_ACCESS],
      },
      {
        label: 'Reportes Pedidos',
        action: 'generateOrderReports',
        icon: 'analytics',
        roles: ROLE_GROUPS.ADMIN_ACCESS,
      },
    ],
  },
  {
    id: 'inventory',
    label: 'Inventario',
    icon: 'inventory_2',
    variant: 'secondary',
    roles: [...ROLE_GROUPS.OPERATIONAL, ...ROLE_GROUPS.ADMIN_ACCESS],
    items: [
      {
        label: 'Agregar Artículos',
        route: { name: 'ArticleCreate' },
        icon: 'add_box',
        roles: ROLE_GROUPS.ADMIN_ACCESS,
      },
      {
        label: 'Inventario General',
        route: { name: 'InventoryManager' },
        icon: 'warehouse',
        roles: [...ROLE_GROUPS.ADMIN_ACCESS, ...ROLE_GROUPS.CASHIER_ACCESS],
        badge: 'V1',
        description: 'Gestión completa del inventario general con API V1'
      },
      {
        label: 'Inventario por Habitación',
        route: { name: 'RoomInventoryManager' },
        icon: 'hotel',
        roles: [...ROLE_GROUPS.ADMIN_ACCESS, ...ROLE_GROUPS.CASHIER_ACCESS],
        badge: 'V1',
        description: 'Administración de inventario específico por habitación'
      },
      {
        label: 'Alertas de Stock',
        route: { name: 'InventoryAlerts' },
        icon: 'notifications_active',
        roles: [...ROLE_GROUPS.ADMIN_ACCESS, ...ROLE_GROUPS.CASHIER_ACCESS],
        badge: 'Nuevo',
        description: 'Monitoreo de alertas de stock bajo y crítico'
      },
      {
        label: 'Transferencias',
        route: { name: 'InventoryTransfers' },
        icon: 'swap_horizontal_circle',
        roles: [...ROLE_GROUPS.ADMIN_ACCESS, ...ROLE_GROUPS.CASHIER_ACCESS],
        badge: 'V1',
        description: 'Transferencias entre habitaciones y lotes'
      },
    ],
  },
  {
    id: 'rooms',
    label: 'Habitaciones',
    icon: 'hotel',
    variant: 'secondary',
    roles: ROLE_GROUPS.FULL_ACCESS,
    items: [
      {
        label: 'Ver Habitaciones',
        route: { name: 'Rooms' },
        icon: 'bed',
        roles: ROLE_GROUPS.FULL_ACCESS,
      },
      {
        label: 'Crear Habitaciones',
        route: { name: 'RoomCreate' },
        icon: 'add_home',
        roles: ROLE_GROUPS.ADMIN_ACCESS,
      },
      {
        label: 'Características',
        route: { name: 'CaracteristicasManager' },
        icon: 'tune',
        roles: ROLE_GROUPS.ADMIN_ACCESS,
      },
      {
        label: 'Categorías',
        route: { name: 'CategoryCreate' },
        icon: 'category',
        roles: ROLE_GROUPS.ADMIN_ACCESS,
      },
      {
        label: 'Empeños',
        route: { name: 'PawnManager' },
        icon: 'handshake',
        roles: [...ROLE_GROUPS.ADMIN_ACCESS, ...ROLE_GROUPS.CASHIER_ACCESS],
      },
    ],
  },
  {
    id: 'cashier',
    label: 'Caja',
    icon: 'point_of_sale',
    variant: 'secondary',
    roles: [...ROLE_GROUPS.STAFF_ACCESS, ...ROLE_GROUPS.ADMIN_ACCESS],
    items: [
      {
        label: 'Cierres',
        route: { name: 'CierresManager' },
        icon: 'receipt',
        roles: [...ROLE_GROUPS.ADMIN_ACCESS, ...ROLE_GROUPS.CASHIER_ACCESS],
      },
      {
        label: 'Medios de Pago',
        route: { name: 'MediosPago' },
        icon: 'payment',
        roles: ROLE_GROUPS.ADMIN_ACCESS,
      },
      {
        label: 'Promociones',
        route: { name: 'PromocionesManager' },
        icon: 'local_offer',
        roles: ROLE_GROUPS.ADMIN_ACCESS,
      },
      {
        label: 'Egresos',
        route: { name: 'Egresos' },
        icon: 'money_off',
        roles: ROLE_GROUPS.ADMIN_ACCESS,
      },

      {
        label: 'Consumo Usuario',
        route: { name: 'UserConsumptionManager' },
        icon: 'receipt_long',
        roles: [...ROLE_GROUPS.STAFF_ACCESS, ...ROLE_GROUPS.ADMIN_ACCESS],
      },
    ],
  },
]

/**
 * Admin menu configuration
 * System administration and management features
 */
export const adminMenuConfig = [
  {
    id: 'admin',
    label: 'Administración',
    icon: 'admin_panel_settings',
    variant: 'secondary',
    roles: ROLE_GROUPS.ADMIN_ACCESS,
    items: [
      {
        label: 'Estadísticas',
        route: { name: 'StatisticsManager' },
        icon: 'analytics',
        roles: ROLE_GROUPS.ADMIN_ACCESS,
      },
      {
        label: 'Gestión Usuarios',
        route: { name: 'UsersManagement' },
        icon: 'group',
        roles: ROLE_GROUPS.MANAGEMENT,
      },
      {
        label: 'Configuración Sistema',
        route: { name: 'AdminSettings' },
        icon: 'settings',
        roles: ROLE_GROUPS.MANAGEMENT,
      },
    ],
  },
]

/**
 * User menu configuration
 * Personal account and session management
 */
export const userMenuConfig = {
  id: 'user',
  label: 'Usuario', // This will be replaced with actual user name
  icon: 'account_circle',
  variant: 'user',
  items: [
    {
      label: 'Administración Usuarios',
      route: { name: 'UsersManagement' },
      icon: 'admin_panel_settings',
      roles: ROLE_GROUPS.MANAGEMENT,
    },
    {
      label: 'Cerrar Sesión',
      action: 'logout',
      icon: 'logout',
      roles: null, // Available to all authenticated users
    },
  ],
}

/**
 * Get all menu configurations
 * @returns {Array} Combined menu configurations
 */
export function getAllMenuConfigs() {
  return [...operationalMenuConfig, ...adminMenuConfig]
}

/**
 * Get user menu configuration with dynamic label
 * @param {string} userName - User display name
 * @returns {Object} User menu config with dynamic label
 */
export function getUserMenuConfig(userName = 'Usuario') {
  return {
    ...userMenuConfig,
    label: userName,
  }
}

/**
 * Filter menu items based on user roles
 * @param {Array} menuItems - Menu items to filter
 * @param {Array} userRoles - User's roles
 * @returns {Array} Filtered menu items
 */
export function filterMenuItemsByRole(menuItems, userRoles) {
  if (!Array.isArray(userRoles)) {
    userRoles = userRoles ? [userRoles] : []
  }

  return menuItems.filter((item) => {
    // If no roles specified, item is available to everyone
    if (!item.roles || item.roles.length === 0) return true

    // Check if user has any of the required roles
    return userRoles.some((role) => item.roles.includes(role))
  })
}

/**
 * Get menu configuration for specific user
 * @param {Object} user - User object with roles
 * @returns {Object} Complete menu configuration for user
 */
export function getMenuConfigForUser(user) {
  const userRoles = user?.roles || (user?.rolId ? [user.rolId] : [])
  const userName = user?.name || user?.email || 'Usuario'

  // Filter operational menus based on user roles
  const availableOperationalMenus = operationalMenuConfig.filter((menu) => {
    if (!menu.roles || menu.roles.length === 0) return true
    return userRoles.some((role) => menu.roles.includes(role))
  })

  // Filter admin menus based on user roles
  const availableAdminMenus = adminMenuConfig.filter((menu) => {
    if (!menu.roles || menu.roles.length === 0) return true
    return userRoles.some((role) => menu.roles.includes(role))
  })

  return {
    operational: availableOperationalMenus,
    admin: availableAdminMenus,
    user: getUserMenuConfig(userName),
    userRoles,
  }
}
