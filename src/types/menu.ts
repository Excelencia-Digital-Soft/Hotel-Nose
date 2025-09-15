// Menu-related TypeScript interfaces and types

export interface MenuItem {
  label: string
  icon?: string
  route?: string | object
  action?: string
  roles?: string[]
  show?: boolean
}

export interface MenuCoordination {
  activeMenuId: Ref<string | null>
  registerMenu: (id: string) => void
  unregisterMenu: (id: string) => void
  closeAllMenus: () => void
}

export interface DropdownMenuProps {
  label: string
  icon?: string
  items: MenuItem[]
  variant?: 'primary' | 'secondary' | 'user'
  disabled?: boolean
}

export interface DropdownMenuEmits {
  (e: 'action', action: string): void
  (e: 'open'): void
  (e: 'close'): void
}

import { type Ref } from 'vue'