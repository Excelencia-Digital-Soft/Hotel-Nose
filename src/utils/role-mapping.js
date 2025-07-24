/**
 * Role mapping utilities for string-based role management
 */

/**
 * All possible roles in the system
 */
export const ROLES = {
  DIRECTOR: 'Director',
  ADMINISTRATOR: 'Administrator',
  CAJERO: 'Cajero',
  CAJERO_STOCK: 'Cajero Stock',
  MUCAMA: 'Mucama',
  USER: 'User',
}

/**
 * Role hierarchy for permissions (higher number = more permissions)
 */
export const ROLE_HIERARCHY = {
  [ROLES.USER]: 1,
  [ROLES.MUCAMA]: 2,
  [ROLES.CAJERO]: 3,
  [ROLES.CAJERO_STOCK]: 4,
  [ROLES.ADMINISTRATOR]: 5,
  [ROLES.DIRECTOR]: 6,
}

/**
 * Role groups for menu access control
 */
export const ROLE_GROUPS = {
  // Basic access - can see basic features
  BASIC_ACCESS: [ROLES.USER, ROLES.MUCAMA],

  // Cashier access - can handle transactions
  CASHIER_ACCESS: [ROLES.CAJERO, ROLES.CAJERO_STOCK],

  // Administrative access - can manage system
  ADMIN_ACCESS: [ROLES.ADMINISTRATOR, ROLES.DIRECTOR],

  // Full access - combination of all roles
  FULL_ACCESS: [
    ROLES.DIRECTOR,
    ROLES.ADMINISTRATOR,
    ROLES.CAJERO_STOCK,
    ROLES.CAJERO,
    ROLES.MUCAMA,
    ROLES.USER,
  ],

  // Management access - admin + director
  MANAGEMENT: [ROLES.DIRECTOR, ROLES.ADMINISTRATOR],

  // Operational staff - cashiers + cleaning
  OPERATIONAL: [ROLES.CAJERO, ROLES.CAJERO_STOCK, ROLES.MUCAMA],

  // All staff except basic users
  STAFF_ACCESS: [
    ROLES.DIRECTOR,
    ROLES.ADMINISTRATOR,
    ROLES.CAJERO_STOCK,
    ROLES.CAJERO,
    ROLES.MUCAMA,
  ],
}

/**
 * Check if user has any of the required roles
 * @param {Array|string} userRoles - User's roles (array of strings or single string)
 * @param {Array} allowedRoles - Allowed roles for access
 * @returns {boolean} - True if user has access
 */
export function hasAnyRole(userRoles, allowedRoles) {
  if (!userRoles || !allowedRoles || allowedRoles.length === 0) return false

  // Normalize userRoles to array
  const rolesArray = Array.isArray(userRoles) ? userRoles : [userRoles]

  // Check if any user role matches any allowed role
  return rolesArray.some((role) => allowedRoles.includes(role))
}

/**
 * Check if user has higher or equal role level
 * @param {string} userRole - User's primary role
 * @param {string} requiredRole - Required minimum role
 * @returns {boolean} - True if user has sufficient permissions
 */
export function hasRoleLevel(userRole, requiredRole) {
  const userLevel = ROLE_HIERARCHY[userRole] || 0
  const requiredLevel = ROLE_HIERARCHY[requiredRole] || 0
  return userLevel >= requiredLevel
}

/**
 * Get user's highest role level
 * @param {Array} userRoles - Array of user roles
 * @returns {string} - Highest level role
 */
export function getHighestRole(userRoles) {
  if (!userRoles || userRoles.length === 0) return ROLES.USER

  return userRoles.reduce((highest, current) => {
    const currentLevel = ROLE_HIERARCHY[current] || 0
    const highestLevel = ROLE_HIERARCHY[highest] || 0
    return currentLevel > highestLevel ? current : highest
  }, userRoles[0])
}

/**
 * Get display name for role (for UI purposes)
 * @param {string} role - Role name
 * @returns {string} - Display name
 */
export function getRoleDisplayName(role) {
  const displayNames = {
    [ROLES.DIRECTOR]: 'Director',
    [ROLES.ADMINISTRATOR]: 'Administrador',
    [ROLES.CAJERO]: 'Cajero',
    [ROLES.CAJERO_STOCK]: 'Cajero Stock',
    [ROLES.MUCAMA]: 'Mucama',
    [ROLES.USER]: 'Usuario',
  }

  return displayNames[role] || role
}
