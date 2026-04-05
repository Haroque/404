<script setup lang="ts">

</script>

<template>
    <div class="navbar-container">
        <div class="navbar-wrapper">
            <h1 class="navbar-title lexend-title"><span class="yellow">Sport</span>Reservation</h1>
            <nav>
                <ul class="navbar-list">
                    <li><RouterLink class="navbar-item" :to="{ name: 'home' }">Sportoviště</RouterLink></li>
                    <li><RouterLink class="navbar-item" :to="{ name: 'home' }">Rezervace</RouterLink></li>
                    <li><RouterLink class="navbar-item" :to="{ name: 'home' }">Účet</RouterLink></li>
                </ul>
            </nav>
        </div>
    </div>
</template>

<style scoped>
.navbar-container {
    position: fixed;
    top: 0px;
    left: 0px;
    right: 0px;
    z-index: 100;
    background-color: var(--color-background);
}
.navbar-wrapper {
    max-width: 1200px;
    margin: 0 auto;
    display: flex;
    justify-content: space-between;
    align-items: center;
    padding: 1.5rem;
}
.navbar-title {
    font-size: 1.5rem;
    font-weight: 600;
}
.navbar-list {
    list-style: none;
    display: flex;
    gap: 1rem;
}
.navbar-item {
    font-size: 1.0rem;
    font-weight: 500;
    transition-duration: 100ms;
    color: var(--color-heading);
}
.navbar-item:hover {
    color: var(--vt-c-yellow);
    background-color: transparent;
}
</style>