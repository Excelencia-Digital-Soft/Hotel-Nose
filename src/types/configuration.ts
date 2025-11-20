import { type ApiResponse } from './common'

/**
 * Configuration-related TypeScript interfaces
 * Based on API endpoints: api/v1/configuration
 */

// Base configuration interface
export interface ConfiguracionDto {
  configuracionId: number
  clave: string
  valor: string
  descripcion: string | null
  categoria: string
  fechaCreacion: string
  fechaModificacion: string | null
  activo: boolean
  institucionId: number | null
}

// Timer update interval specific interfaces
export interface TimerUpdateIntervalResponseDto {
  intervalMinutos: number
  descripcion: string | null
  fechaModificacion: string | null
}

export interface TimerUpdateIntervalDto {
  intervalMinutos: number
  descripcion?: string
}

// Configuration creation interface
export interface ConfiguracionCreateDto {
  clave: string
  valor: string
  descripcion: string
  categoria: string
  institucionId?: number | null
}

// Configuration update interface
export interface ConfiguracionUpdateDto {
  valor: string
  descripcion?: string
  categoria?: string
  activo?: boolean
}

// Configuration categories (based on backend categories)
export enum ConfigurationCategory {
  SYSTEM = 'SYSTEM',
  UI = 'UI',
  SECURITY = 'SECURITY',
  BUSINESS = 'BUSINESS',
  INTEGRATIONS = 'INTEGRATIONS',
}

// Configuration keys (commonly used)
export enum ConfigurationKey {
  TIMER_UPDATE_INTERVAL = 'TIMER_UPDATE_INTERVAL',
  MAX_LOGIN_ATTEMPTS = 'MAX_LOGIN_ATTEMPTS',
  SESSION_TIMEOUT = 'SESSION_TIMEOUT',
  DEFAULT_CURRENCY = 'DEFAULT_CURRENCY',
  COMPANY_NAME = 'COMPANY_NAME',
  EMAIL_NOTIFICATIONS = 'EMAIL_NOTIFICATIONS',
}

// Form data interfaces for the UI
export interface ConfigurationFormData {
  clave: string
  valor: string
  descripcion: string
  categoria: ConfigurationCategory
  activo: boolean
}

export interface TimerConfigFormData {
  intervalMinutos: number
  descripcion: string
}

// API Response types
export type ConfigurationResponse = ApiResponse<ConfiguracionDto>
export type ConfigurationsResponse = ApiResponse<ConfiguracionDto[]>
export type TimerUpdateIntervalResponse = ApiResponse<TimerUpdateIntervalResponseDto>

// UI state interfaces
export interface ConfigurationState {
  configurations: ConfiguracionDto[]
  loading: boolean
  error: string | null
  selectedCategory: ConfigurationCategory | 'ALL'
}

// Validation interfaces
export interface ConfigurationValidation {
  clave: {
    required: boolean
    minLength: number
    maxLength: number
    pattern?: RegExp
  }
  valor: {
    required: boolean
    maxLength: number
  }
  descripcion: {
    maxLength: number
  }
}

// Configuration display metadata
export interface ConfigurationMeta {
  key: string
  displayName: string
  description: string
  category: ConfigurationCategory
  type: 'string' | 'number' | 'boolean' | 'select'
  validation?: Partial<ConfigurationValidation>
  options?: { label: string; value: string | number }[]
}

