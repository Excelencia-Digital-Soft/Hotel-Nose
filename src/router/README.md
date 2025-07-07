# Router Structure Documentation

## Overview
This router implementation follows a modular approach, separating concerns by responsibility and improving maintainability.

## File Structure

```
src/router/
├── index.js                 # Main router configuration
├── config.js               # Router constants and helpers
├── guards.js               # Route guards and permission logic
├── routes/
│   ├── index.js            # Main route aggregator
│   ├── auth.js             # Authentication and public routes
│   ├── admin.js            # Administrative routes
│   └── management.js       # Management and operational routes
└── README.md               # This documentation
```

## Modules Description

### guards.js
Contains all route protection logic:
- **ROLES**: Role constants for better maintainability
- **ROLE_GROUPS**: Common role combinations
- **requireAuth**: Authentication guard
- **requireRole**: Role-based access control
- **authAndRoleGuard**: Combined auth and role guard

### routes/auth.js
Public and authentication routes:
- Guest layout (login/register)
- External layout (public room selection)

### routes/admin.js
Administrative routes requiring high-level permissions:
- Room creation
- Category management
- User management
- Payment methods
- Characteristics management
- Promotions

### routes/management.js
Day-to-day operational routes:
- Dashboard
- Room viewing
- Order management
- Inventory operations
- Financial operations

### config.js
Configuration constants and helpers:
- Route paths and names
- Menu categories
- Helper functions for route generation

## Usage Examples

### Adding a New Route

1. **Determine the category**: Admin, Management, or Auth
2. **Add to appropriate route file**:

```javascript
// In routes/admin.js
{
  path: "/NewFeature",
  name: "NewFeature",
  component: NewFeatureComponent,
  meta: {
    requireAuth: true,
    roles: ROLE_GROUPS.ADMIN_ONLY,
    description: "Manage new feature",
    category: "Feature Management"
  }
}
```

### Using Route Guards

```javascript
// Custom guard example
export function customGuard(to, from, next) {
  // Your logic here
  next();
}

// In router/index.js
router.beforeEach(customGuard);
```

### Role-Based Navigation

```javascript
// In a component
import { getRoutesByRole } from '@/router/routes';

export default {
  computed: {
    availableRoutes() {
      return getRoutesByRole(this.userRole);
    }
  }
}
```

## Role Definitions

- **SUPERVISOR (1)**: Highest level access
- **ADMINISTRATIVO (2)**: Administrative functions
- **RECEPCIONISTA (3)**: Reception operations
- **EMPLEADO (4)**: Basic employee access
- **GERENTE (5)**: Management level access

## Role Groups

- **ADMIN_ONLY**: [1, 2] - Supervisor and Administrative
- **ADMIN_MANAGER**: [1, 2, 5] - Admin + Manager
- **RECEPTION_STAFF**: [2, 3, 4, 5] - All except Supervisor
- **ALL_STAFF**: [1, 2, 3, 4, 5] - Everyone

## Benefits

1. **Maintainability**: Clear separation of concerns
2. **Scalability**: Easy to add new routes and guards
3. **Type Safety**: Better IntelliSense and error detection
4. **Documentation**: Self-documenting code structure
5. **Testing**: Individual modules can be tested in isolation
6. **Performance**: Tree-shakeable imports

## Migration Notes

The old router structure has been completely refactored but maintains the same functionality. All existing route names and paths remain unchanged to prevent breaking changes.