/** @type {import('tailwindcss').Config} */
const defaultTheme = require('tailwindcss/defaultTheme')

export default {
  content: [
    "./index.html",
    "./src/**/*.{vue,js,ts,jsx,tsx}",
  ],
  theme: {
    extend: {      
      textColor: {
        DEFAULT: '#ffffff', // Esto establece el color de texto por defecto a blanco
      },
      colors: {
        primary: {
          50: '#FDE8F0',
          100: '#FBC9E0',
          200: '#F99DCE',
          300: '#F66CC2',
          400: '#F43FB8',
          500: '#FF49D1',
          600: '#E639B3',
          700: '#D4319E',
          800: '#B92A8B',
          900: '#A32175',
        },
        secondary: {
          50: '#E6D7FF',
          100: '#C2A3FF',
          200: '#9F6DFF',
          300: '#7B3BFF',
          400: '#5F00FF',
          500: '#8D1CFF',
          600: '#7817D1',
          700: '#6214B7',
          800: '#4D0F9E',
          900: '#3D0A8D',
        },
        accent: {
          50: '#E0E8FF',
          100: '#B3C5FF',
          200: '#8DA1FF',
          300: '#5F7BFF',
          400: '#3B5CFF',
          500: '#5494FB',
          600: '#4681E2',
          700: '#2D65B4',
          800: '#1D4A8E',
          900: '#0F2E6B',
        },
        'primary-50': 'rgb(var(--primary-50))',
        'primary-100': 'rgb(var(--primary-100))',
        'primary-200': 'rgb(var(--primary-200))',
        'primary-300': 'rgb(var(--primary-300))',
        'primary-400': 'rgb(var(--primary-400))',
        'primary-500': 'rgb(var(--primary-500))',
        'primary-600': 'rgb(var(--primary-600))',
        'primary-700': 'rgb(var(--primary-700))',
        'primary-800': 'rgb(var(--primary-800))',
        'primary-900': 'rgb(var(--primary-900))',
        'primary-950': 'rgb(var(--primary-950))',
        'surface-0': 'rgb(var(--surface-0))',
        'surface-50': 'rgb(var(--surface-50))',
        'surface-100': 'rgb(var(--surface-100))',
        'surface-200': 'rgb(var(--surface-200))',
        'surface-300': 'rgb(var(--surface-300))',
        'surface-400': 'rgb(var(--surface-400))',
        'surface-500': 'rgb(var(--surface-500))',
        'surface-600': 'rgb(var(--surface-600))',
        'surface-700': 'rgb(var(--surface-700))',
        'surface-800': 'rgb(var(--surface-800))',
        'surface-900': 'rgb(var(--surface-900))',
        'surface-950': 'rgb(var(--surface-950))'
    },
      
      plugins: [
        function ({ addUtilities }) {
          const newUtilities = {
            '.bg-fixed': {
              'background-attachment': 'fixed',
            },
          };
          addUtilities(newUtilities, ['responsive', 'hover']);
        },
      ],
      fontFamily: {
        sans: ['Inter var', ...defaultTheme.fontFamily.sans],
      },
      backgroundColor: {
        'gradient-indigo': 'linear-gradient(to right, #93C5FD, #ACB6E5, #D6A4A4)',
      },
      
    },
  },
  plugins: [
    require('@tailwindcss/forms')
  ],
}


