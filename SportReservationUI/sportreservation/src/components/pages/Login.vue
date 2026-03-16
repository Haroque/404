<script setup lang="ts">
import Splash from '../../assets/login_splash.jpg'
defineProps<{
}>()
</script>

<template>
    <img class="splash-image" :src="Splash" />
    <header>
        <div class="wrapper">
            <div class="greetings">
                <h1><span class="yellow">Sport</span>Reservation</h1>
                <h3>
                    Zarezervujte si sportoviště, snadno a rychle.
                </h3>
            </div>
        </div>
    </header>

    <main>
    </main>
</template>

<style scoped>
h1 {
    color: var(--vt-c-white-mute);
}
h1, span {
    font-weight: 500;
    font-size: 3.5rem;
    font-family: "Lexend", sans-serif;
    position: relative;
}

h3 {
    font-size: 1.0rem;
    padding: 0rem 0.25rem;
}

.splash-image {
    position: fixed;
    width: 100vw;
    height: 100vh;
    top: 0px;
    left: 0px;
    object-fit: cover;
    filter: brightness(50%);
    z-index: -1;
}

.greetings h1,
.greetings h3 {
    text-align: center;
}

@media (min-width: 1024px) {

    .greetings h1,
    .greetings h3 {
        text-align: left;
    }
}
</style>
