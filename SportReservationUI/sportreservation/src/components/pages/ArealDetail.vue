<script lang="ts" setup>
import '@/assets/main.css';
import NavBar from "../views/NavBar.vue";

</script>
<template>
    <NavBar></NavBar>
    <div class="areal-detail-container">
        <h1>Název sportoviště</h1>
        <p>Lorem ipsum dolor sit amet, consectetur adipiscing elit. Donecca diam lectus.</p>
    </div>
</template>
<style scoped>
h1 {
    color: var(--color-heading);
    font-size: 4rem;
    font-weight: 600;
}
.areal-detail-container {
    display: flex;
    flex-direction: column;
    max-width: 1200px;
    padding: 1.5rem;
    padding-top: 6rem;
    overflow-y: scroll;
}
</style>