import { createRouter, createWebHistory } from "vue-router";
//LAYOUTS
import DefaultLayout from "../layouts/DefaultLayout.vue";
import GuestLayout from "../layouts/GuestLayout.vue";
import ExternalLayout from "../layouts/ExternalLayout.vue";
//VISTAS
import Home from "../views/Home.vue";
import Rooms from "../views/Rooms.vue";
import RoomCreate from "../views/RoomCreate.vue";
import CategoryCreate from "../views/CategoryCreate.vue";
import ArticuloCreate from "../views/ArticuloCreate.vue";
import SubmitOrder from "../views/SubmitOrder.vue";
import ReceptionOrder from "../views/ReceptionOrder.vue";
import InventoryManager from "../views/InventoryManager.vue";
import CierresManager from "../views/CierresManager.vue";
import PromocionesManager from "../views/PromocionesManager.vue";
import MediosPagoManager from "../views/MediosPagoManager.vue";
import UsuariosManager from "../views/UsuariosManager.vue";
import Egresos from "../views/Egresos.vue";
import EmpeñosManager from "../views/EmpeñosManager.vue";
import SelectRoom from "../views/SelectRoom.vue"
import CaracteristicasManager from "../views/CaracteristicasManager.vue"

import { useAuthStore } from "../store/auth";

const routes = [
  {
    path: "/",
    component: DefaultLayout,
    meta: {
      requireAuth: true,
    },
    children: [
      {
        path: "/",
        name: "home",
        component: Home,
        meta: {
          requireAuth: true,
        },
      },
      {
        path: "/Rooms",
        name: "Rooms",
        component: Rooms,
        meta: {
          requireAuth: true,
          roles: [1, 2, 3, 5], // Solo Supervisores y Administrativos (idRol 1 y 2)
        },
      },
      {
        path: "/RoomCreate",
        name: "RoomCreate",
        component: RoomCreate,
        meta: {
          requireAuth: true,
          roles: [1, 2], // Solo Supervisores y Administrativos (idRol 1 y 2)
        },
      },
      {
        path: "/CategoryCreate",
        name: "CategoryCreate",
        component: CategoryCreate,
        meta: {
          requireAuth: true,
          roles: [1, 2], // Solo Supervisores y Administrativos (idRol 1 y 2)
        },
      },
      {
        path: "/ArticleCreate",
        name: "ArticleCreate",
        component: ArticuloCreate,
        meta: {
          requireAuth: true,
          roles: [1, 2, 5], // Solo Supervisores y Administrativos (idRol 1 y 2)
        },
      },
      {
        path: "/SubmitOrder/:habitacionId?",
        name: "SubmitOrder",
        component: SubmitOrder,
        meta: {
          requireAuth: false,
        },
      },
      {
        path: "/ReceptionOrder",
        name: "ReceptionOrder",
        component: ReceptionOrder,
        meta: {
          requireAuth: true,
          roles: [2, 3, 4, 5],
        },
      },
      {
        path: "/InventoryManager",
        name: "InventoryManager",
        component: InventoryManager,
        meta: {
          requireAuth: true,
          roles: [1, 2, 5], // Solo Supervisores y Administrativos (idRol 1 y 2)
        },
      },
      {
        path: "/CierresManager",
        name: "CierresManager",
        component: CierresManager,
        meta: {
          requireAuth: true,
          roles: [1, 2, 3, 5], // Solo Supervisores y Administrativos (idRol 1 y 2)
        },
      },
      {
        path: "/EmpenosManager",
        name: "EmpenosManager",
        component: EmpeñosManager,
        meta: {
          requireAuth: true,
          roles: [1, 2, 3, 5], // Solo Supervisores y Administrativos (idRol 1 y 2)
        },
      },
      {
        path: "/PromocionesManager",
        name: "PromocionesManager",
        component: PromocionesManager,
        meta: {
          requireAuth: true,
          roles: [1, 2], // Solo Supervisores y Administrativos (idRol 1 y 2)
        },
      },
      {
        path: "/Egresos",
        name: "Egresos",
        component: Egresos,
        meta: {
          requireAuth: true,
          roles: [1, 2, 3, 5], // Solo Supervisores y Administrativos (idRol 1 y 2)
        },
      },
      {
        path: "/UsuariosManager",
        name: "UsuariosManager",
        component: UsuariosManager,
        meta: { requireAuth: true, roles: [1, 2] }, // Solo accesible para admins
      },
      {
        path: "/MediosPago",
        name: "MediosPago",
        component: MediosPagoManager,
        meta: {
          requireAuth: true,
          roles: [1, 2], // Solo Supervisores y Administrativos (idRol 1 y 2)
        },
      },
      {
        path: "/CaracteristicasManager",
        name: "CaracteristicasManager",
        component: CaracteristicasManager,
        meta: {
          requireAuth: true,
          roles: [1, 2], // Solo Supervisores y Administrativos (idRol 1 y 2)
        },
      },
    ],
  },

  {
    path: "/external",
    component: ExternalLayout,
    meta: {
      requireAuth: false,
    },
    children: [
      {
        path: "/SelectRoom",
        name: "SelectRoom",
        component: SelectRoom,
        meta: {
          requireAuth: false,
        },
      },
    ],
  },
  {
    path: "/guest",
    component: GuestLayout,
    meta: {
      requireAuth: false,
    },
  },
];

const router = createRouter({
  history: createWebHistory("/hotel/"),
  routes,
});

router.beforeEach((to, from, next) => {
  // to and from are both route objects. must call `next`.
  const authStore = useAuthStore(); //si el usuario está logueado y guardado en el state
  const authVerif = authStore.auth !== null && authStore.auth !== undefined;
  const userRoleId = authStore.auth?.rol; // Obtén el ID del rol del usuario
  const needAuth = to.meta.requireAuth;

  if (needAuth && !authVerif) {
    next("/guest");
  }
  // Si la ruta tiene roles específicos y el rol del usuario no está incluido
  if (to.meta.roles && !to.meta.roles.includes(userRoleId)) {
    alert("Acceso denegado");
    next("/guest"); // Redirige a una página de acceso denegado
    return;
  }

  next(); // Permite continuar si todo está bien
});

export default router;
