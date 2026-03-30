<script setup lang="ts">
import { ref } from 'vue'
import ArealItem from "./ArealItem.vue"
const items = ref([{ message: 'Tenisový kurt' }, { message: 'Posilovna' }, { message: 'Posilovna' }, { message: 'Basketballový kurt' }, { message: 'Tenisový kurt' }, { message: 'Tenisový kurt' }])
</script>

<template>
    <div class="areal-list-container">
        <ArealItem class="subheader-item" v-for="item in items" :facility="item">
        </ArealItem>
    </div>
</template>

<style scoped>
.areal-list-container {
    height: fit-content;
    overflow-y: hidden;
    width: 100%;
    display: grid;
    grid: auto / auto;
    gap: 1rem;
}
.header-item {
    display: flex;
    grid-area: 1 / 1 / span 1 / span 1;
}

@media (min-width: 1024px) {
    .areal-list-container {
        grid: auto / auto auto;
    }
    .header-item {
        grid-area: 1 / 1 / span 2 / span 2;
    }
}
</style>