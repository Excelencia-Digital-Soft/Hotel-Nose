export default {
  root: ({ props }) => ({
    class: [
      //Size and Shape
      'w-96 rounded-md',

      // Positioning
      {
        '-translate-x-2/4': props.position == 'bottom-center' || props.position == 'bottom-center',
      },
    ],
  }),
  container: ({ props }) => ({
    class: [
      'my-8 rounded-3xl w-full',
      'border-solid border-0 border-l-[6px]',
      'backdrop-blur-[10px] shadow-md',

      // Colors (mantener fondos originales)
      {
        'bg-indigo-500/20': props.message.severity == 'info',
        'bg-purple-400/20': props.message.severity == 'success', // Mantener fondo morado para éxito
        'bg-yellow-500/20': props.message.severity == 'warn',
        'bg-red-500/20': props.message.severity == 'error',
      },
      {
        'border-indigo-400': props.message.severity == 'info',
        'border-lime-400': props.message.severity == 'success',
        'border-yellow-500': props.message.severity == 'warn',
        'border-red-500': props.message.severity == 'error',
      },
      
      // Cambiar solo el texto a blanco para todos los tipos
      'text-white',
    ],
  }),
  content: ({ props }) => ({
    class: [
      'flex p-4',
      {
        'items-start': props.message.summary,
        'items-center': !props.message.summary,
      },
    ],
  }),
  icon: {
    class: [
      // Sizing and Spacing
      'w-6 h-6',
      'text-lg leading-none mr-2 shrink-0',
      // White icon for glassmorphism
      'text-white opacity-90',
    ],
  },
  text: {
    class: [
      // Font and Text
      'text-base leading-none text-white',
      'ml-2',
      'flex-1',
    ],
  },
  summary: {
    class: 'font-bold block text-white font-[Lexend]',
  },
  detail: ({ props }) => ({
    class: ['block text-white font-[Lexend] opacity-95', { 'mt-2': props.message.summary }],
  }),
  closebutton: {
    class: [
      // Flexbox
      'flex items-center justify-center',

      // Size
      'w-8 h-8',

      // Spacing and Misc
      'ml-auto relative',

      // Shape
      'rounded-full',

      // Colors (mantener transparente pero con texto blanco)
      'bg-transparent text-white',

      // Transitions
      'transition duration-200 ease-in-out',

      // States
      'hover:bg-white/20',

      // Misc
      'overflow-hidden',
    ],
  },
  transition: {
    enterFromClass: 'opacity-0 translate-y-2/4',
    enterActiveClass: 'transition-[transform,opacity] duration-300',
    leaveFromClass: 'max-h-[1000px]',
    leaveActiveClass:
      '!transition-[max-height_.45s_cubic-bezier(0,1,0,1),opacity_.3s,margin-bottom_.3s] overflow-hidden',
    leaveToClass: 'max-h-0 opacity-0 mb-0',
  },
}
