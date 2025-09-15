export default {
  root: {
    class: [
      // Flex
      'flex items-center justify-center',

      // Shape
      'first:rounded-l-md',
      'last:rounded-r-md',
      'border-y',

      'last:border-r',
      'border-l',
      'border-r-0',

      // Space
      'p-3',

      // Size
      'min-w-[3rem]',

      // Color
      'bg-surface-700 dark:bg-surface-200',
      'text-surface-400 dark:text-surface-600',
      'border-surface-600 dark:border-surface-300'
    ]
  }
};
