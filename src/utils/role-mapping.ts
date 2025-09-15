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
} as const

// Type for role values
export type Role = typeof ROLES[keyof typeof ROLES]

// Type for role keys
export type RoleKey = keyof typeof ROLES

/**
 * Role hierarchy for permissions (higher number = more permissions)
 */
export const ROLE_HIERARCHY: Record<Role, number> = {
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
  BASIC_ACCESS: [ROLES.USER, ROLES.MUCAMA] as Role[],

  // Cashier access - can handle transactions
  CASHIER_ACCESS: [ROLES.CAJERO, ROLES.CAJERO_STOCK] as Role[],

  // Administrative access - can manage system
  ADMIN_ACCESS: [ROLES.ADMINISTRATOR, ROLES.DIRECTOR] as Role[],

  // Full access - combination of all roles
  FULL_ACCESS: [
    ROLES.DIRECTOR,
    ROLES.ADMINISTRATOR,
    ROLES.CAJERO_STOCK,
    ROLES.CAJERO,
    ROLES.MUCAMA,
    ROLES.USER,
  ] as Role[],

  // Operational staff - cashiers + cleaning
  INVENTORY_ACCESS: [ROLES.CAJERO_STOCK] as Role[],

  // All staff except basic users
  STAFF_ACCESS: [
    ROLES.DIRECTOR,
    ROLES.ADMINISTRATOR,
    ROLES.CAJERO_STOCK,
    ROLES.CAJERO,
    ROLES.MUCAMA,
  ] as Role[],
} as const

// Type for role group keys
export type RoleGroupKey = keyof typeof ROLE_GROUPS

/**
 * Check if user has any of the required roles
 * @param userRoles - User's roles (array of strings or single string)
 * @param allowedRoles - Allowed roles for access
 * @returns True if user has access
 */
export function hasAnyRole(
  userRoles: string[] | string | null | undefined,
  allowedRoles: string[]
): boolean {
  if (!userRoles || !allowedRoles || allowedRoles.length === 0) return false

  // Normalize userRoles to array
  const rolesArray: string[] = Array.isArray(userRoles) ? userRoles : [userRoles]

  // Check if any user role matches any allowed role
  return rolesArray.some((role) => allowedRoles.includes(role))
}

/**
 * Check if user has higher or equal role level
 * @param userRole - User's primary role
 * @param requiredRole - Required minimum role
 * @returns True if user has sufficient permissions
 */
export function hasRoleLevel(userRole: string, requiredRole: string): boolean {
  const userLevel = ROLE_HIERARCHY[userRole as Role] || 0
  const requiredLevel = ROLE_HIERARCHY[requiredRole as Role] || 0
  return userLevel >= requiredLevel
}

/**
 * Get user's highest role level
 * @param userRoles - Array of user roles
 * @returns Highest level role
 */
export function getHighestRole(userRoles: string[] | null | undefined): string {
  if (!userRoles || userRoles.length === 0) return ROLES.USER

  return userRoles.reduce((highest, current) => {
    const currentLevel = ROLE_HIERARCHY[current as Role] || 0
    const highestLevel = ROLE_HIERARCHY[highest as Role] || 0
    return currentLevel > highestLevel ? current : highest
  }, userRoles[0])
}

/**
 * Display names for roles
 */
const ROLE_DISPLAY_NAMES: Record<Role, string> = {
  [ROLES.DIRECTOR]: 'Director',
  [ROLES.ADMINISTRATOR]: 'Administrador',
  [ROLES.CAJERO]: 'Cajero',
  [ROLES.CAJERO_STOCK]: 'Cajero Stock',
  [ROLES.MUCAMA]: 'Mucama',
  [ROLES.USER]: 'Usuario',
}

/**
 * Get display name for role (for UI purposes)
 * @param role - Role name
 * @returns Display name
 */
export function getRoleDisplayName(role: string): string {
  return ROLE_DISPLAY_NAMES[role as Role] || role
}

/**
 * Type guard to check if a string is a valid Role
 * @param role - String to check
 * @returns True if the string is a valid Role
 */
export function isValidRole(role: string): role is Role {
  return Object.values(ROLES).includes(role as Role)
}

/**
 * Type guard to check if all strings in an array are valid Roles
 * @param roles - Array of strings to check
 * @returns True if all strings are valid Roles
 */
export function areValidRoles(roles: string[]): roles is Role[] {
  return roles.every(isValidRole)
}