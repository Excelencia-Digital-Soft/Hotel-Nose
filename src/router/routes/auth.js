import GuestLayout from "../../layouts/GuestLayout.vue";
import ExternalLayout from "../../layouts/ExternalLayout.vue";
import SelectRoom from "../../views/SelectRoom.vue";
import UsersManagement from "../../views/UsersManagement.vue";

/**
 * Authentication and public routes
 * These routes don't require authentication
 */
export const authRoutes = [
  {
    path: "/guest",
    name: "Guest",
    component: GuestLayout,
    meta: {
      requireAuth: false,
      description: "Login and registration page"
    }
  },
  {
    path: "/external",
    name: "External",
    component: ExternalLayout,
    meta: {
      requireAuth: false,
      description: "External access layout"
    },
    children: [
      {
        path: "/SelectRoom",
        name: "SelectRoom",
        component: SelectRoom,
        meta: {
          requireAuth: false,
          description: "Room selection for external users"
        }
      }
    ]
  },
  {
    path: "/UsersManagement",
    name: "UsersManagementPublic",
    component: UsersManagement,
    meta: {
      requireAuth: false,
      description: "Public access to user management"
    }
  }
];