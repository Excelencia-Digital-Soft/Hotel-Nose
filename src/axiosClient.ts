import axios, { type AxiosInstance, type InternalAxiosRequestConfig, type AxiosResponse, type AxiosError } from 'axios'
import type { Router } from 'vue-router'
// @ts-ignore - router is in JavaScript, will be typed at runtime
import router from './router'

/**
 * Custom axios instance with authentication interceptors
 * Configured independently from SignalR to avoid API blocking
 */
const axiosClient: AxiosInstance = axios.create({
  baseURL: import.meta.env.VITE_API_BASE_URL,
  // ✅ Set timeout to prevent hanging requests that could conflict with SignalR
  timeout: 10000, // 10 seconds
  headers: {
    'Content-Type': 'application/json',
    // Add custom header to identify API requests vs SignalR
    'X-Client-Type': 'API-Request'
  }
})

/**
 * Request interceptor to add authentication token
 * Optimized to work alongside SignalR without conflicts
 */
axiosClient.interceptors.request.use(
  (config: InternalAxiosRequestConfig): InternalAxiosRequestConfig => {
    // Get token from localStorage or sessionStorage
    const token = localStorage.getItem('auth-token') || localStorage.getItem('jwt_token') || sessionStorage.getItem('jwt_token')
    
    if (token) {
      config.headers.Authorization = `Bearer ${token}`
    }
    
    // Log API requests in development for debugging
    if (import.meta.env.DEV) {
      console.log(`🌐 [API] ${config.method?.toUpperCase()} ${config.url}`)
    }
    
    return config
  },
  (error: AxiosError): Promise<AxiosError> => {
    console.error('🌐 [API] Request interceptor error:', error)
    return Promise.reject(error)
  }
)

/**
 * Response interceptor to handle authentication errors
 * Enhanced error handling to prevent conflicts with SignalR
 */
axiosClient.interceptors.response.use(
  (response: AxiosResponse): AxiosResponse => {
    // Log successful API responses in development
    if (import.meta.env.DEV) {
      console.log(`🌐 [API] ✅ ${response.config.method?.toUpperCase()} ${response.config.url} (${response.status})`)
    }
    return response
  },
  (error: AxiosError): Promise<AxiosError> => {
    // Enhanced error logging
    const method = error.config?.method?.toUpperCase() || 'UNKNOWN'
    const url = error.config?.url || 'unknown-url'
    const status = error.response?.status || 'No Status'
    
    console.error(`🌐 [API] ❌ ${method} ${url} (${status})`, error.message)
    
    // Handle authentication errors
    if (error.response?.status === 401) {
      // Token expired or invalid - clean up and redirect
      localStorage.removeItem('auth-token')
      localStorage.removeItem('jwt_token')
      sessionStorage.removeItem('jwt_token')
      
      // Avoid redirecting if already on guest page to prevent loops
      if (window.location.pathname !== '/guest') {
        ;(router as Router).push('/guest')
      }
    }
    
    // Handle timeout errors specifically (important for SignalR coexistence)
    if (error.code === 'ECONNABORTED' || error.message.includes('timeout')) {
      console.warn('🌐 [API] Request timeout - this should not affect SignalR connection')
    }
    
    return Promise.reject(error)
  }
)

export default axiosClient