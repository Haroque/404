<script setup lang="ts">
import '@/assets/main.css';
import { ref } from 'vue';
import Splash from '../../assets/login_splash.jpg'
import { tryLogin } from '@/auth';
import { useRouter } from '@/router';

const error = ref("")
const errorBar = ref(false)

const email = ref("")
const password = ref("")

async function validate() {
    if (email.value.length == 0) {
        error.value = "Musíte uvést email"
        errorBar.value = true
        return
    }
    if (password.value.length == 0) {
        error.value = "Musíte vyplnit heslo"
        errorBar.value = true
        return
    }
    if (await tryLogin(email.value, password.value)) {
        return
    }
    error.value = "Neplatný email nebo heslo"
    errorBar.value = true
}

</script>

<template>
    <v-snackbar v-model="errorBar" :timeout="3000">
        {{ error }}
        <template v-slot:actions>
            <v-btn variant="text" @click="errorBar = false">Zavřít</v-btn>
        </template>
    </v-snackbar>

    <img class="splash-image" :src="Splash" />
    <header>
        <div class="wrapper">
            <div class="greetings">
                <h1><span class="yellow">Sport</span>Reservation</h1>
            </div>
        </div>
    </header>

    <main>
        <form>
            <input v-model="email" class="input-textbox" type="email" placeholder="E-mail" />
            <input v-model="password" class="input-textbox" type="password" placeholder="Heslo" />
            <input @click="validate()" class="input-submit" value="Přihlásit se"/>
            <RouterLink :to="{ name: 'register' }">Nemáte účet?</RouterLink>
        </form>
    </main>
</template>

<style scoped>
form {
    display: flex;
    flex-direction: column;
    align-items: center;
    gap: 0.5rem;
}
.input-textbox {
    padding: 0.6rem 1rem;
    font-size: 1.0rem;
    border-radius: 0.5rem;
    outline: none;
    border: none;
    transition-duration: 100ms;
}
.input-textbox:hover {
    background-color: var(--vt-c-white-mute);
}
.input-textbox:focus {
    background-color: var(--vt-c-white-mute);
}
.input-submit {
    padding: 0.6rem 0rem;
    font-size: 1.0rem;
    background-color: var(--vt-c-yellow);
    border: none;
    border-radius: 0.5rem;
    font-weight: 600;
    margin-top: 1rem;
    transition-duration: 100ms;
    text-align: center;
    cursor: pointer;
}
.input-submit:hover {
    background-color: var(--vt-c-yellow-light);
}
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
