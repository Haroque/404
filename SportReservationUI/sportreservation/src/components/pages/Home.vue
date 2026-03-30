<script setup lang="ts">
import NavBar from "../views/NavBar.vue";
import ArealList from "../views/ArealList.vue";
</script>

<template>
    <NavBar></NavBar>
    <div class="scroll-container">
        <ArealList></ArealList>
    </div>
</template>

<style scoped>
.scroll-container {
    position: absolute;
    display: flex;
    flex-direction: column;
    gap: 1.5rem;
    overflow-y: auto;
    height: 100vh;
    max-width: 1200px;
    padding: 1.5rem;
    margin: auto;
    padding-top: 6rem;
    top: 0rem;
    left: 0rem;
    right: 0rem;
}
</style>