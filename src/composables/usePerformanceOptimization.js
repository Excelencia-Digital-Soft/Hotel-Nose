/**
 * Performance Optimization Composable
 * Detects device capabilities and adjusts glassmorphism effects automatically
 */

import { ref, onMounted, watch, readonly } from 'vue'

export function usePerformanceOptimization() {
  // Performance levels
  const PERFORMANCE_LEVELS = {
    HIGH: 'high',
    MEDIUM: 'medium', 
    LOW: 'low',
    MINIMAL: 'minimal'
  }

  // Reactive state
  const performanceLevel = ref(PERFORMANCE_LEVELS.HIGH)
  const isOptimizing = ref(false)
  const deviceInfo = ref({
    userAgent: '',
    memory: null,
    cores: null,
    connection: null,
    supportsCSSBackdropFilter: false,
    supportsWebGL: false,
    gpu: null
  })

  /**
   * Detect device capabilities
   */
  const detectDeviceCapabilities = () => {
    const info = {
      userAgent: navigator.userAgent,
      memory: navigator.deviceMemory || null,
      cores: navigator.hardwareConcurrency || null,
      connection: navigator.connection || null,
      supportsCSSBackdropFilter: CSS.supports('backdrop-filter', 'blur(1px)'),
      supportsWebGL: checkWebGLSupport(),
      gpu: getGPUInfo()
    }

    deviceInfo.value = info
    return info
  }

  /**
   * Check WebGL support and performance
   */
  const checkWebGLSupport = () => {
    try {
      const canvas = document.createElement('canvas')
      const gl = canvas.getContext('webgl') || canvas.getContext('experimental-webgl')
      return !!gl
    } catch (e) {
      return false
    }
  }

  /**
   * Get GPU information
   */
  const getGPUInfo = () => {
    try {
      const canvas = document.createElement('canvas')
      const gl = canvas.getContext('webgl') || canvas.getContext('experimental-webgl')
      if (!gl) return null

      const debugInfo = gl.getExtension('WEBGL_debug_renderer_info')
      if (!debugInfo) return null

      return {
        vendor: gl.getParameter(debugInfo.UNMASKED_VENDOR_WEBGL),
        renderer: gl.getParameter(debugInfo.UNMASKED_RENDERER_WEBGL)
      }
    } catch (e) {
      return null
    }
  }

  /**
   * Calculate performance score based on device capabilities
   */
  const calculatePerformanceScore = () => {
    let score = 0

    // Memory check (0-30 points)
    const memory = deviceInfo.value.memory
    if (memory >= 8) score += 30
    else if (memory >= 4) score += 20
    else if (memory >= 2) score += 10
    else score += 5

    // CPU cores check (0-20 points)
    const cores = deviceInfo.value.cores
    if (cores >= 8) score += 20
    else if (cores >= 4) score += 15
    else if (cores >= 2) score += 10
    else score += 5

    // Browser capabilities (0-20 points)
    if (deviceInfo.value.supportsCSSBackdropFilter) score += 10
    if (deviceInfo.value.supportsWebGL) score += 10

    // Connection speed (0-15 points)
    const connection = deviceInfo.value.connection
    if (connection) {
      if (connection.effectiveType === '4g') score += 15
      else if (connection.effectiveType === '3g') score += 10
      else if (connection.effectiveType === '2g') score += 5
    } else {
      score += 10 // Default assumption for desktop
    }

    // GPU check (0-15 points)
    const gpu = deviceInfo.value.gpu
    if (gpu) {
      const renderer = gpu.renderer.toLowerCase()
      if (renderer.includes('nvidia') || renderer.includes('amd') || renderer.includes('intel')) {
        if (renderer.includes('rtx') || renderer.includes('gtx') || renderer.includes('rx')) score += 15
        else if (renderer.includes('intel')) score += 8
        else score += 12
      }
    } else {
      score += 8 // Default assumption
    }

    return Math.min(score, 100) // Cap at 100
  }

  /**
   * Determine performance level based on score
   */
  const determinePerformanceLevel = (score) => {
    if (score >= 80) return PERFORMANCE_LEVELS.HIGH
    if (score >= 60) return PERFORMANCE_LEVELS.MEDIUM
    if (score >= 40) return PERFORMANCE_LEVELS.LOW
    return PERFORMANCE_LEVELS.MINIMAL
  }

  /**
   * Apply CSS custom properties based on performance level
   */
  const applyCSSOptimizations = (level) => {
    const root = document.documentElement

    switch (level) {
      case PERFORMANCE_LEVELS.HIGH:
        root.style.setProperty('--glass-blur', 'var(--glass-blur-high)')
        root.style.setProperty('--glass-opacity', 'var(--glass-opacity-high)')
        root.style.setProperty('--glass-border-opacity', 'var(--glass-border-high)')
        root.style.setProperty('--glass-shadow', 'var(--glass-shadow-complex)')
        break

      case PERFORMANCE_LEVELS.MEDIUM:
        root.style.setProperty('--glass-blur', 'var(--glass-blur-medium)')
        root.style.setProperty('--glass-opacity', 'var(--glass-opacity-medium)')
        root.style.setProperty('--glass-border-opacity', 'var(--glass-border-medium)')
        root.style.setProperty('--glass-shadow', 'var(--glass-shadow-medium)')
        break

      case PERFORMANCE_LEVELS.LOW:
        root.style.setProperty('--glass-blur', 'var(--glass-blur-low)')
        root.style.setProperty('--glass-opacity', 'var(--glass-opacity-low)')
        root.style.setProperty('--glass-border-opacity', 'var(--glass-border-low)')
        root.style.setProperty('--glass-shadow', 'var(--glass-shadow-simple)')
        break

      case PERFORMANCE_LEVELS.MINIMAL:
        root.style.setProperty('--glass-blur', 'var(--glass-blur-minimal)')
        root.style.setProperty('--glass-opacity', 'var(--glass-opacity-minimal)')
        root.style.setProperty('--glass-border-opacity', 'var(--glass-border-minimal)')
        root.style.setProperty('--glass-shadow', 'var(--glass-shadow-minimal)')
        break
    }

    // Add performance class to body for debugging
    document.body.className = document.body.className
      .replace(/perf-(high|medium|low|minimal)/g, '')
      .trim() + ` perf-${level}`
  }

  /**
   * Monitor performance and adjust if needed
   */
  const monitorPerformance = () => {
    if (!window.performance || !window.performance.getEntriesByType) return

    // Monitor paint metrics
    const paintEntries = performance.getEntriesByType('paint')
    const navigationEntries = performance.getEntriesByType('navigation')

    if (paintEntries.length > 0) {
      const fcp = paintEntries.find(entry => entry.name === 'first-contentful-paint')
      const lcp = paintEntries.find(entry => entry.name === 'largest-contentful-paint')

      // If performance is poor, downgrade
      if (fcp && fcp.startTime > 3000) { // FCP > 3s
        downgradePerformance()
      }
    }
  }

  /**
   * Downgrade performance level
   */
  const downgradePerformance = () => {
    const levels = Object.values(PERFORMANCE_LEVELS)
    const currentIndex = levels.indexOf(performanceLevel.value)
    
    if (currentIndex < levels.length - 1) {
      const newLevel = levels[currentIndex + 1]
      setPerformanceLevel(newLevel)
      
      if (import.meta.env.VITE_ENABLE_PERFORMANCE_LOGS === 'true') {
        console.warn(`🔽 Performance downgraded to: ${newLevel}`)
      }
    }
  }

  /**
   * Set performance level manually
   */
  const setPerformanceLevel = (level) => {
    if (Object.values(PERFORMANCE_LEVELS).includes(level)) {
      performanceLevel.value = level
      applyCSSOptimizations(level)
      
      // Store in localStorage for persistence
      localStorage.setItem('hotel-app-performance-level', level)
      
      if (import.meta.env.VITE_ENABLE_PERFORMANCE_LOGS === 'true') {
        console.log(`⚡ Performance level set to: ${level}`)
      }
    }
  }

  /**
   * Get stored performance level
   */
  const getStoredPerformanceLevel = () => {
    return localStorage.getItem('hotel-app-performance-level')
  }

  /**
   * Initialize performance optimization
   */
  const initializeOptimization = () => {
    isOptimizing.value = true

    try {
      // 1. Detect device capabilities
      detectDeviceCapabilities()

      // 2. Check for stored preference
      const storedLevel = getStoredPerformanceLevel()
      
      if (storedLevel && Object.values(PERFORMANCE_LEVELS).includes(storedLevel)) {
        setPerformanceLevel(storedLevel)
      } else {
        // 3. Calculate performance score and set level
        const score = calculatePerformanceScore()
        const recommendedLevel = determinePerformanceLevel(score)
        setPerformanceLevel(recommendedLevel)
        
        if (import.meta.env.VITE_ENABLE_PERFORMANCE_LOGS === 'true') {
          console.log(`🎯 Device score: ${score}/100, recommended level: ${recommendedLevel}`)
        }
      }

      // 4. Start performance monitoring
      setTimeout(monitorPerformance, 5000)

    } catch (error) {
      console.error('Performance optimization failed:', error)
      // Fallback to medium performance
      setPerformanceLevel(PERFORMANCE_LEVELS.MEDIUM)
    } finally {
      isOptimizing.value = false
    }
  }

  /**
   * Get performance info for debugging
   */
  const getPerformanceInfo = () => {
    return {
      level: performanceLevel.value,
      deviceInfo: deviceInfo.value,
      score: calculatePerformanceScore(),
      isOptimizing: isOptimizing.value
    }
  }

  /**
   * Force high performance mode (for modern devices)
   */
  const enableHighPerformance = () => {
    setPerformanceLevel(PERFORMANCE_LEVELS.HIGH)
  }

  /**
   * Force low performance mode (for old devices)
   */
  const enableLowPerformance = () => {
    setPerformanceLevel(PERFORMANCE_LEVELS.LOW)
  }

  // Initialize on mount
  onMounted(() => {
    initializeOptimization()
  })

  // Watch for manual changes
  watch(performanceLevel, (newLevel) => {
    applyCSSOptimizations(newLevel)
  })

  return {
    // State
    performanceLevel: readonly(performanceLevel),
    isOptimizing: readonly(isOptimizing),
    deviceInfo: readonly(deviceInfo),
    
    // Methods
    setPerformanceLevel,
    initializeOptimization,
    enableHighPerformance,
    enableLowPerformance,
    getPerformanceInfo,
    
    // Constants
    PERFORMANCE_LEVELS
  }
}