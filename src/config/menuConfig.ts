import { ROLE_GROUPS, type Role } from '../utils/role-mapping'
import { type RouteLocationRaw } from 'vue-router'

/**
 * Menu variant types for styling
 */
export type MenuVariant = 'primary' | 'secondary' | 'user'

/**
 * Badge type for menu items
 */
export type MenuBadge = 'V1' | 'Nuevo' | string

/**
 * Menu item interface
 */
export interface MenuItem {
  label: string
  route?: RouteLocationRaw
  action?: string
  icon: string
  roles: Role[] | null
  badge?: MenuBadge
  description?: string
}

/**
 * Menu configuration interface
 */
export interface MenuConfig {
  id: string
  label: string
  icon: string
  variant: MenuVariant
  roles: Role[]
  items: MenuItem[]
}

/**
 * User interface for menu configuration
 */
export interface User {
  name?: string
  email?: string
  roles?: string[]
  rolId?: string
}

/**
 * Menu configuration for user interface
 */
export interface UserMenuConfigResult {
  operational: MenuConfig[]
  admin: MenuConfig[]
  user: MenuConfig
  userRoles: string[]
}

/**
 * Operational menu configuration
 * Business-specific functionality organized by domain
 */
export const operationalMenuConfig: MenuConfig[] = [
  {
    id: 'inventory',
    label: 'Inventario',
    icon: 'inventory_2',
    variant: 'secondary',
    roles: [...ROLE_GROUPS.ADMIN_ACCESS, ...ROLE_GROUPS.INVENTORY_ACCESS],
    items: [
      {
        label: 'Agregar Artículos',
        route: { name: 'ArticleCreate' },
        icon: 'add_box',
        roles: [...ROLE_GROUPS.ADMIN_ACCESS, ...ROLE_GROUPS.INVENTORY_ACCESS],
      },
      {
        label: 'Inventario General',
        route: { name: 'InventoryManagerV1' },
        icon: 'warehouse',
        roles: [...ROLE_GROUPS.ADMIN_ACCESS, ...ROLE_GROUPS.INVENTORY_ACCESS],
        badge: 'V1',
        description: 'Gestión completa del inventario general con API V1',
      },
      {
        label: 'Alertas de Stock',
        route: { name: 'InventoryAlerts' },
        icon: 'notifications_active',
        roles: [...ROLE_GROUPS.ADMIN_ACCESS, ...ROLE_GROUPS.INVENTORY_ACCESS],
        badge: 'Nuevo',
        description: 'Monitoreo de alertas de stock bajo y crítico',
      },
      {
        label: 'Transferencias',
        route: { name: 'InventoryTransfers' },
        icon: 'swap_horizontal_circle',
        roles: [...ROLE_GROUPS.ADMIN_ACCESS, ...ROLE_GROUPS.INVENTORY_ACCESS],
        badge: 'V1',
        description: 'Transferencias entre habitaciones y lotes',
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
export const adminMenuConfig: MenuConfig[] = [
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
        roles: ROLE_GROUPS.ADMIN_ACCESS,
      },
      {
        label: 'Configuración Sistema',
        route: { name: 'AdminSettings' },
        icon: 'settings',
        roles: ROLE_GROUPS.ADMIN_ACCESS,
      },
    ],
  },
]

/**
 * User menu configuration
 * Personal account and session management
 */
export const userMenuConfig: MenuConfig = {
  id: 'user',
  label: 'Usuario', // This will be replaced with actual user name
  icon: 'account_circle',
  variant: 'user',
  roles: [], // User menu doesn't need role restrictions at top level
  items: [
    {
      label: 'Administración Usuarios',
      route: { name: 'UsersManagement' },
      icon: 'admin_panel_settings',
      roles: ROLE_GROUPS.ADMIN_ACCESS,
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
 * @returns Combined menu configurations
 */
export function getAllMenuConfigs(): MenuConfig[] {
  return [...operationalMenuConfig, ...adminMenuConfig]
}

/**
 * Get user menu configuration with dynamic label
 * @param userName - User display name
 * @returns User menu config with dynamic label
 */
export function getUserMenuConfig(userName: string = 'Usuario'): MenuConfig {
  return {
    ...userMenuConfig,
    label: userName,
  }
}

/**
 * Filter menu items based on user roles
 * @param menuItems - Menu items to filter
 * @param userRoles - User's roles
 * @returns Filtered menu items
 */
export function filterMenuItemsByRole(
  menuItems: MenuItem[],
  userRoles: string[] | string
): MenuItem[] {
  const rolesArray: string[] = Array.isArray(userRoles) ? userRoles : userRoles ? [userRoles] : []

  return menuItems.filter((item) => {
    // If no roles specified, item is available to everyone
    if (!item.roles || item.roles.length === 0) return true

    // Check if user has any of the required roles
    return rolesArray.some((role) => item.roles?.includes(role as Role))
  })
}

/**
 * Get menu configuration for specific user
 * @param user - User object with roles
 * @returns Complete menu configuration for user
 */
export function getMenuConfigForUser(user: User | null | undefined): UserMenuConfigResult {
  const userRoles: string[] = user?.roles || (user?.rolId ? [user.rolId] : [])
  const userName: string = user?.name || user?.email || 'Usuario'

  // Filter operational menus based on user roles
  const availableOperationalMenus = operationalMenuConfig.filter((menu) => {
    if (!menu.roles || menu.roles.length === 0) return true
    return userRoles.some((role) => menu.roles.includes(role as Role))
  })

  // Filter admin menus based on user roles
  const availableAdminMenus = adminMenuConfig.filter((menu) => {
    if (!menu.roles || menu.roles.length === 0) return true
    return userRoles.some((role) => menu.roles.includes(role as Role))
  })

  return {
    operational: availableOperationalMenus,
    admin: availableAdminMenus,
    user: getUserMenuConfig(userName),
    userRoles,
  }
}
