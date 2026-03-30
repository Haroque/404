<script setup lang="ts">
import Splash from '../../assets/login_splash.jpg'
const props = defineProps(['facility'])
</script>

<template>
    <RouterLink :to="{ name: 'register' }" class="areal-item-container">
        <img :src="Splash" />
        <div class="areal-item-text-container">
            <span class="facility-type">{{ props.facility.message }}</span>
            <h2>Název sportoviště</h2>
            <p>Lorem ipsum dolor sit amet, consectetur adipiscing elit. Doneca diam lectus.</p>
        </div>
    </RouterLink>
</template>

<style scoped>
.areal-item-container {
    display: flex;
    flex-direction: row;
    gap: 0.5rem;
    padding: 0rem;
    background-color: var(--color-background-mute);
    color: var(--color-text);
    border-radius: 1.0rem;
    overflow: hidden;
}
.areal-item-container > img {
    width: 8rem;
    height: 8rem;
    object-fit: cover;
    border-radius: 0.0rem;
}
.areal-item-text-container {
    display: inline-block;
    gap: 0.25rem;
    overflow: hidden;
    max-height: 8rem;
    padding: 0.75rem;
}
.areal-item-text-container > h2 {
    color: var(--color-heading);
}
.areal-item-text-container > .facility-type {
    color: var(--color-accent);
    text-transform: uppercase;
    font-weight: 700;
    font-size: x-small;
}
.areal-item-text-container > p {
    position: relative;
    font-size: 0.8rem;
    text-overflow: ellipsis;
    overflow: hidden;

}
</style>