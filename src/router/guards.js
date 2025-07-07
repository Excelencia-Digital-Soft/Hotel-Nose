import { useAuthStore } from "../store/auth";
import { hasAnyRole } from "../utils/role-mapping.js";
import { showAccessDeniedToast } from "../utils/toast.js";

/**
 * Authentication guard
 * @param {Object} to - Target route
 * @param {Object} from - Current route
 * @param {Function} next - Navigation callback
 */
export function requireAuth(to, from, next) {
  const authStore = useAuthStore();
  const isAuthenticated = authStore.isAuthenticated && authStore.user !== null;

  if (!isAuthenticated && to.meta.requireAuth) {
    next(false) // Cancela la navegación sin redirigir;
    return;
  }

  next();
}

// Note: hasAnyRole function is now imported from role-mapping.js utility

/**
 * Role-based access control guard
 * Supports string-based roles only
 * @param {Object} to - Target route
 * @param {Object} from - Current route
 * @param {Function} next - Navigation callback
 */
export function requireRole(to, from, next) {
  const authStore = useAuthStore();
  const user = authStore.user;
  const allowedRoles = to.meta.roles;

  if (allowedRoles && user) {
    const userRoles = user.roles;

    if (!hasAnyRole(userRoles, allowedRoles)) {
      showAccessDeniedToast();
      next(false) // Cancela la navegación sin redirigir;
      return;
    }
  }

  next();
}

/**
 * Combined authentication and role guard
 * @param {Object} to - Target route
 * @param {Object} from - Current route
 * @param {Function} next - Navigation callback
 */
export function authAndRoleGuard(to, from, next) {
  const authStore = useAuthStore();
  const isAuthenticated = authStore.isAuthenticated && authStore.user !== null;
  const user = authStore.user;
  const needAuth = to.meta.requireAuth;
  const allowedRoles = to.meta.roles;

  // Check authentication first
  if (needAuth && !isAuthenticated) {
    next(false) // Cancela la navegación sin redirigir;
    return;
  }

  // Check role permissions if route has role restrictions
  if (allowedRoles && user) {
    const userRoles = user.roles;

    if (!hasAnyRole(userRoles, allowedRoles)) {
      showAccessDeniedToast();
      next(false) // Cancela la navegación sin redirigir;
      return;
    }
  }

  next();
}

/**
 * Guard for public routes (no authentication required)
 * @param {Object} to - Target route
 * @param {Object} from - Current route
 * @param {Function} next - Navigation callback
 */
export function publicRoute(to, from, next) {
  next();
}
