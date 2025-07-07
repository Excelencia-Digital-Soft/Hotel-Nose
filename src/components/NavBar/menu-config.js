/**
 * Menu configuration for the navigation bar
 * Centralized menu structure with role-based access control
 */

import { ROLE_GROUPS } from "../../utils/role-mapping.js";

export const menuConfig = [
  {
    id: 'orders',
    label: 'Pedidos',
    icon: 'receipt_long',
    roles: [...ROLE_GROUPS.BASIC_ACCESS, ...ROLE_GROUPS.CASHIER_ACCESS],
    items: [
      {
        label: 'Recibir Pedido',
        route: { name: 'ReceptionOrder' },
        icon: 'inbox',
        roles: [...ROLE_GROUPS.BASIC_ACCESS, ...ROLE_GROUPS.CASHIER_ACCESS]
      }
    ]
  },
  {
    id: 'inventory',
    label: 'Artículos',
    icon: 'inventory_2',
    roles: [...ROLE_GROUPS.OPERATIONAL, ...ROLE_GROUPS.ADMIN_ACCESS],
    items: [
      {
        label: 'Agregar Artículos',
        route: { name: 'ArticleCreate' },
        icon: 'add_box',
        roles: ROLE_GROUPS.ADMIN_ACCESS
      },
      {
        label: 'Inventario General',
        route: { name: 'InventoryManager' },
        icon: 'warehouse',
        roles: [...ROLE_GROUPS.ADMIN_ACCESS, ...ROLE_GROUPS.CASHIER_ACCESS]
      }
    ]
  },
  {
    id: 'rooms',
    label: 'Habitaciones',
    icon: 'hotel',
    roles: ROLE_GROUPS.FULL_ACCESS,
    items: [
      {
        label: 'Habitaciones',
        route: { name: 'Rooms' },
        icon: 'bed',
        roles: ROLE_GROUPS.FULL_ACCESS
      },
      {
        label: 'Crear Habitaciones',
        route: { name: 'RoomCreate' },
        icon: 'add_home',
        roles: ROLE_GROUPS.ADMIN_ACCESS
      },
      {
        label: 'Características',
        route: { name: 'CaracteristicasManager' },
        icon: 'tune',
        roles: ROLE_GROUPS.ADMIN_ACCESS
      },
      {
        label: 'Categorías',
        route: { name: 'CategoryCreate' },
        icon: 'category',
        roles: ROLE_GROUPS.ADMIN_ACCESS
      },
      {
        label: 'Empeños',
        route: { name: 'EmpenosManager' },
        icon: 'handshake',
        roles: [...ROLE_GROUPS.ADMIN_ACCESS, ...ROLE_GROUPS.CASHIER_ACCESS]
      }
    ]
  },
  {
    id: 'cashier',
    label: 'Caja',
    icon: 'point_of_sale',
    roles: [...ROLE_GROUPS.STAFF_ACCESS, ...ROLE_GROUPS.ADMIN_ACCESS],
    items: [
      {
        label: 'Cierres',
        route: { name: 'CierresManager' },
        icon: 'receipt',
        roles: [...ROLE_GROUPS.ADMIN_ACCESS, ...ROLE_GROUPS.CASHIER_ACCESS]
      },
      {
        label: 'Medios de Pago',
        route: { name: 'MediosPago' },
        icon: 'payment',
        roles: ROLE_GROUPS.ADMIN_ACCESS
      },
      {
        label: 'Promociones',
        route: { name: 'PromocionesManager' },
        icon: 'local_offer',
        roles: ROLE_GROUPS.ADMIN_ACCESS
      },
      {
        label: 'Egresos',
        route: { name: 'Egresos' },
        icon: 'money_off',
        roles: ROLE_GROUPS.ADMIN_ACCESS
      }
    ]
  }
];

/**
 * User profile menu configuration
 */
export const userMenuConfig = {
  id: 'user',
  label: 'Mi Perfil',
  icon: 'account_circle',
  items: [
    {
      label: 'Administración Usuarios',
      route: { name: 'UsersManagement' },
      icon: 'admin_panel_settings',
      roles: ROLE_GROUPS.MANAGEMENT
    },
    {
      label: 'Cerrar Sesión',
      action: 'logout',
      icon: 'logout',
      roles: null // Available to all authenticated users
    }
  ]
};
