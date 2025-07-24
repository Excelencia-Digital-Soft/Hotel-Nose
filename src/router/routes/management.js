import Home from "../../views/Home.vue";
import RoomsNew from "../../views/RoomsNew.vue";
import Services from "../../views/Services.vue";
import ArticleCreate from "../../views/ArticleCreate.vue";
import SubmitOrder from "../../views/SubmitOrder.vue";
import ReceptionOrder from "../../views/ReceptionOrder.vue";
import InventoryManager from "../../views/InventoryManager.vue";
import RoomInventory from "../../views/RoomInventory.vue";
import CierresManager from "../../views/CierresManager.vue";
import Egresos from "../../views/Egresos.vue";
import PawnManager from "../../views/PawnManager.vue";
import { ROLE_GROUPS } from "../../utils/role-mapping.js";

/**
 * Management and operational routes
 * These routes are for day-to-day operations and management
 */
export const managementRoutes = [
  {
    path: "/",
    name: "Home",
    component: Home,
    meta: {
      requireAuth: true,
      description: "Dashboard and home page",
      category: "Dashboard"
    }
  },
  {
    path: "/Rooms",
    name: "Rooms",
    component: RoomsNew,
    meta: {
      requireAuth: true,
      roles: ROLE_GROUPS.FULL_ACCESS,
      description: "View and manage rooms",
      category: "Room Management"
    }
  },
  {
    path: "/Services",
    name: "Services",
    component: Services,
    meta: {
      requireAuth: true,
      description: "View and manage hotel services",
      category: "Services Management"
    }
  },
  {
    path: "/ArticleCreate",
    name: "ArticleCreate",
    component: ArticleCreate,
    meta: {
      requireAuth: true,
      roles: ROLE_GROUPS.ADMIN_ACCESS,
      description: "Create new articles/products",
      category: "Inventory Management"
    }
  },
  {
    path: "/SubmitOrder/:habitacionId?",
    name: "SubmitOrder",
    component: SubmitOrder,
    meta: {
      requireAuth: false,
      description: "Submit orders for rooms",
      category: "Orders"
    }
  },
  {
    path: "/ReceptionOrder",
    name: "ReceptionOrder",
    component: ReceptionOrder,
    meta: {
      requireAuth: true,
      roles: [...ROLE_GROUPS.CASHIER_ACCESS, ...ROLE_GROUPS.BASIC_ACCESS],
      description: "Manage reception orders",
      category: "Orders"
    }
  },
  {
    path: "/OrderHistory",
    name: "OrderHistory",
    component: ReceptionOrder, // Using same component for now, can be changed later
    meta: {
      requireAuth: true,
      roles: [...ROLE_GROUPS.CASHIER_ACCESS, ...ROLE_GROUPS.ADMIN_ACCESS],
      description: "View order history",
      category: "Orders"
    }
  },
  {
    path: "/InventoryManager",
    name: "InventoryManager",
    component: InventoryManager,
    meta: {
      requireAuth: true,
      roles: [...ROLE_GROUPS.ADMIN_ACCESS, ...ROLE_GROUPS.CASHIER_ACCESS],
      description: "Manage inventory and stock",
      category: "Inventory Management"
    }
  },
  {
    path: "/RoomInventory",
    name: "RoomInventory",
    component: RoomInventory,
    meta: {
      requireAuth: true,
      roles: [...ROLE_GROUPS.ADMIN_ACCESS, ...ROLE_GROUPS.CASHIER_ACCESS],
      description: "Manage room-specific inventory",
      category: "Inventory Management"
    }
  },
  {
    path: "/CierresManager",
    name: "CierresManager",
    component: CierresManager,
    meta: {
      requireAuth: true,
      roles: [...ROLE_GROUPS.ADMIN_ACCESS, ...ROLE_GROUPS.CASHIER_ACCESS],
      description: "Manage daily closures",
      category: "Financial"
    }
  },
  {
    path: "/PawnManager",
    name: "PawnManager",
    component: PawnManager,
    meta: {
      requireAuth: true,
      roles: [...ROLE_GROUPS.ADMIN_ACCESS, ...ROLE_GROUPS.CASHIER_ACCESS],
      description: "Manage pawning operations",
      category: "Financial"
    }
  },
  // Redirect old EmpenosManager route to new PawnManager
  {
    path: "/EmpenosManager",
    redirect: "/PawnManager"
  },
  {
    path: "/Egresos",
    name: "Egresos",
    component: Egresos,
    meta: {
      requireAuth: true,
      roles: [...ROLE_GROUPS.ADMIN_ACCESS, ...ROLE_GROUPS.CASHIER_ACCESS],
      description: "Manage expenses and outgoings",
      category: "Financial"
    }
  }
];
