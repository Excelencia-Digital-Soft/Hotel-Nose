import { createRouter, createWebHistory } from "vue-router";
import { routes } from "./routes/index.js";
import { authAndRoleGuard } from "./guards.js";

/**
 * Router configuration with improved structure
 */
const router = createRouter({
  history: createWebHistory("/hotel/"),
  routes,
  // Optional: Add scroll behavior for better UX
  scrollBehavior(to, from, savedPosition) {
    if (savedPosition) {
      return savedPosition;
    } else {
      return { top: 0 };
    }
  }
});

/**
 * Global navigation guard
 * Uses the combined authentication and role guard
 */
router.beforeEach(authAndRoleGuard);

export default router;
