import axios, { type AxiosInstance, type InternalAxiosRequestConfig, type AxiosResponse, type AxiosError } from 'axios'
import type { Router } from 'vue-router'
// @ts-ignore - router is in JavaScript, will be typed at runtime
import router from './router'

/**
 * Custom axios instance with authentication interceptors
 */
const axiosClient: AxiosInstance = axios.create({
  baseURL: import.meta.env.VITE_API_BASE_URL,
})

/**
 * Request interceptor to add authentication token
 */
axiosClient.interceptors.request.use(
  (config: InternalAxiosRequestConfig): InternalAxiosRequestConfig => {
    const token = localStorage.getItem('auth-token')
    if (token) {
      config.headers.Authorization = `Bearer ${token}`
    }
    return config
  },
  (error: AxiosError): Promise<AxiosError> => {
    return Promise.reject(error)
  }
)

/**
 * Response interceptor to handle authentication errors
 */
axiosClient.interceptors.response.use(
  (response: AxiosResponse): AxiosResponse => {
    return response
  },
  (error: AxiosError): Promise<AxiosError> => {
    if (error.response?.status === 401) {
      // Token expired or invalid
      localStorage.removeItem('auth-token')
      ;(router as Router).push('/guest')
    }
    return Promise.reject(error)
  }
)

export default axiosClient