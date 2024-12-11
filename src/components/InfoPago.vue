<template>
    <a 
        :href="href" 
        target="_blank" 
        class="px-5 py-2 my-3 mr-2 border-2  border-purple-400 text-purple-500 hover:bg-gradient-to-r from-indigo-300 via-purple-400 to-purple-300  hover:text-white hover:shadow-lg hover:shadow-purple-500/50 hover:border-purple-200 rounded-lg transition-colors"
        >
            <slot></slot>
        </a>
</template>

<script setup>
const props = defineProps({
    href:String
})
</script>