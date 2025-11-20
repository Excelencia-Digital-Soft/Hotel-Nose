import axiosClient from '../axiosClient'
import type {
  ConfiguracionDto,
  ConfiguracionCreateDto,
  ConfiguracionUpdateDto,
  TimerUpdateIntervalDto,
  TimerUpdateIntervalResponseDto,
  ConfigurationResponse,
  ConfigurationsResponse,
  TimerUpdateIntervalResponse,
  ApiResponse
} from '../types'

/**
 * Configuration Service
 * Handles all configuration-related API calls
 * Base endpoint: /api/v1/configuration
 */
export class ConfigurationService {
  private static readonly BASE_URL = '/api/v1/configuration'

  // Timer Update Interval Methods
  static async getTimerUpdateInterval(): Promise<TimerUpdateIntervalResponse> {
    const response = await axiosClient.get(`${this.BASE_URL}/timer-update-interval`)
    return response.data
  }

  static async updateTimerUpdateInterval(data: TimerUpdateIntervalDto): Promise<TimerUpdateIntervalResponse> {
    const response = await axiosClient.put(`${this.BASE_URL}/timer-update-interval`, data)
    return response.data
  }

  // General Configuration Methods
  static async getAllConfigurations(categoria?: string): Promise<ConfigurationsResponse> {
    const params = categoria ? { categoria } : undefined
    const response = await axiosClient.get(this.BASE_URL, { params })
    return response.data
  }

  static async getConfiguration(clave: string): Promise<ConfigurationResponse> {
    const response = await axiosClient.get(`${this.BASE_URL}/${encodeURIComponent(clave)}`)
    return response.data
  }

  static async createConfiguration(data: ConfiguracionCreateDto): Promise<ConfigurationResponse> {
    const response = await axiosClient.post(this.BASE_URL, data)
    return response.data
  }

  static async updateConfiguration(clave: string, data: ConfiguracionUpdateDto): Promise<ConfigurationResponse> {
    const response = await axiosClient.put(`${this.BASE_URL}/${encodeURIComponent(clave)}`, data)
    return response.data
  }

  static async deleteConfiguration(clave: string): Promise<ApiResponse> {
    const response = await axiosClient.delete(`${this.BASE_URL}/${encodeURIComponent(clave)}`)
    return response.data
  }

  // Utility Methods
  static async testEndpoint(): Promise<ApiResponse<any>> {
    const response = await axiosClient.get(`${this.BASE_URL}/test`)
    return response.data
  }

  static async getHealthStatus(): Promise<{ status: string; timestamp: string }> {
    const response = await axiosClient.get(`${this.BASE_URL}/health`)
    return response.data
  }

  static async getAuthStatus(): Promise<ApiResponse<any>> {
    const response = await axiosClient.get(`${this.BASE_URL}/auth-status`)
    return response.data
  }

  // Configuration Categories Helper
  static async getConfigurationsByCategory(categoria: string): Promise<ConfigurationsResponse> {
    return this.getAllConfigurations(categoria)
  }

  // Batch operations
  static async updateMultipleConfigurations(
    updates: Array<{ clave: string; data: ConfiguracionUpdateDto }>
  ): Promise<ConfigurationResponse[]> {
    const promises = updates.map(update => 
      this.updateConfiguration(update.clave, update.data)
    )
    return Promise.all(promises)
  }

  // Validation helpers
  static validateConfigurationKey(clave: string): boolean {
    // Basic validation for configuration key
    return /^[A-Z_][A-Z0-9_]*$/.test(clave)
  }

  static validateTimerInterval(intervalMinutos: number): boolean {
    // Timer interval should be between 1 and 1440 minutes (24 hours)
    return intervalMinutos >= 1 && intervalMinutos <= 1440
  }
}

// Export singleton pattern for consistency  
export const configurationService = ConfigurationService