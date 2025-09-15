import DefaultLayout from "../../layouts/DefaultLayout.vue";
import { authRoutes } from "./auth.js";
import { adminRoutes } from "./admin.js";
import { managementRoutes } from "./management.js";

/**
 * Main route configuration
 * Combines all route modules into a single structure
 */
export const routes = [
  // Main authenticated layout with all protected routes
  {
    path: "/",
    component: DefaultLayout,
    meta: {
      requireAuth: true,
      description: "Main application layout"
    },
    children: [
      ...managementRoutes,
      ...adminRoutes
    ]
  },
  
  // Authentication and public routes
  ...authRoutes,
  
  // Catch-all redirect
  {
    path: "/:pathMatch(.*)*",
    redirect: "/guest"
  }
];

/**
 * Route helper functions
 */
export const getRoutesByCategory = (category) => {
  const allRoutes = [...managementRoutes, ...adminRoutes];
  return allRoutes.filter(route => route.meta?.category === category);
};

export const getRoutesByRole = (userRole) => {
  const allRoutes = [...managementRoutes, ...adminRoutes];
  return allRoutes.filter(route => {
    if (!route.meta?.roles) return true;
    return route.meta.roles.includes(userRole);
  });
};

export const getPublicRoutes = () => {
  return authRoutes;
};