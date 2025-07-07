/**
 * Console filters for development
 * Helps reduce noise from browser extensions and other non-critical errors
 */

// Filter out Chrome extension errors in development
if (import.meta.env.DEV) {
  const originalError = console.error;
  
  console.error = (...args) => {
    // Filter out Chrome extension errors
    const message = args.join(' ');
    
    if (
      message.includes('chrome-extension://') ||
      message.includes('Failed to load resource: net::ERR_FILE_NOT_FOUND') ||
      message.includes('completion_list.html')
    ) {
      // Silently ignore Chrome extension errors
      return;
    }
    
    // Log other errors normally
    originalError.apply(console, args);
  };
  
  const originalWarn = console.warn;
  
  console.warn = (...args) => {
    const message = args.join(' ');
    
    // Filter out specific Vue warnings that are not critical
    if (
      message.includes('inject() can only be used inside setup()') &&
      message.includes('chrome-extension://')
    ) {
      return;
    }
    
    originalWarn.apply(console, args);
  };
}

export default {};