import RoomCreate from '../../views/RoomCreate.vue'
import CategoryCreate from '../../views/CategoryCreate.vue'
import PromocionesManager from '../../views/PromocionesManager.vue'
import UsersManagement from '../../views/UsersManagement.vue'
import MediosPagoManager from '../../views/MediosPagoManager.vue'
import CaracteristicasManager from '../../views/CaracteristicasManager.vue'
import StatisticsManager from '../../views/StatisticsManager.vue'
import UserConsumptionManager from '../../views/UserConsumptionManager.vue'
import AdminSettings from '../../views/AdminSettings.vue'
import { ROLE_GROUPS } from '../../utils/role-mapping'

/**
 * Administrative routes
 * These routes require admin-level permissions (Supervisor or Administrativo)
 */
export const adminRoutes = [
  {
    path: '/RoomCreate',
    name: 'RoomCreate',
    component: RoomCreate,
    meta: {
      requireAuth: true,
      roles: ROLE_GROUPS.ADMIN_ACCESS,
      description: 'Create new rooms',
      category: 'Room Management',
    },
  },
  {
    path: '/CategoryCreate',
    name: 'CategoryCreate',
    component: CategoryCreate,
    meta: {
      requireAuth: true,
      roles: ROLE_GROUPS.ADMIN_ACCESS,
      description: 'Create new categories',
      category: 'Inventory Management',
    },
  },
  {
    path: '/PromocionesManager',
    name: 'PromocionesManager',
    component: PromocionesManager,
    meta: {
      requireAuth: true,
      roles: ROLE_GROUPS.ADMIN_ACCESS,
      description: 'Manage promotions and discounts',
      category: 'Marketing',
    },
  },
  // Redirección automática de la ruta obsoleta
  {
    path: '/usuarios-manager-legacy',
    redirect: '/UsersManagement',
  },
  {
    path: '/UsersManagement',
    name: 'UsersManagement',
    component: UsersManagement,
    meta: {
      requireAuth: true,
      roles: ROLE_GROUPS.ADMIN_ACCESS,
      description: 'Advanced user management',
      category: 'User Management',
    },
  },
  {
    path: '/MediosPago',
    name: 'MediosPago',
    component: MediosPagoManager,
    meta: {
      requireAuth: true,
      roles: ROLE_GROUPS.ADMIN_ACCESS,
      description: 'Manage payment methods',
      category: 'Financial',
    },
  },
  {
    path: '/CaracteristicasManager',
    name: 'CaracteristicasManager',
    component: CaracteristicasManager,
    meta: {
      requireAuth: true,
      roles: ROLE_GROUPS.ADMIN_ACCESS,
      description: 'Manage room characteristics',
      category: 'Room Management',
    },
  },
  {
    path: '/StatisticsManager',
    name: 'StatisticsManager',
    component: StatisticsManager,
    meta: {
      requireAuth: true,
      roles: ROLE_GROUPS.ADMIN_ACCESS,
      description: 'View detailed statistics and analytics',
      category: 'Analytics',
    },
  },
  {
    path: '/UserConsumptionManager',
    name: 'UserConsumptionManager',
    component: UserConsumptionManager,
    meta: {
      requireAuth: true,
      roles: [...ROLE_GROUPS.STAFF_ACCESS, ...ROLE_GROUPS.ADMIN_ACCESS],
      description: 'Manage and view user consumption data',
      category: 'Financial',
    },
  },
  {
    path: '/AdminSettings',
    name: 'AdminSettings',
    component: AdminSettings,
    meta: {
      requireAuth: true,
      roles: ROLE_GROUPS.ADMIN_ACCESS,
      description: 'System configuration and settings',
      category: 'Administration',
    },
  },
]
