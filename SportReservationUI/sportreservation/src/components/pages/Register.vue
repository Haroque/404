<script setup lang="ts">
import { ref } from 'vue';
import Splash from '../../assets/login_splash.jpg'
import { secureFetch } from '@/auth';
import { useRouter } from '@/router';

const error = ref("")
const errorBar = ref(false)

const email = ref("")
const name = ref("")
const password = ref("")
const repeat = ref("")

async function validate() {
    if (email.value.length == 0) {
        error.value = "Musíte uvést email"
        errorBar.value = true
        return
    }
    if (name.value.length == 0) {
        error.value = "Musíte uvést jméno"
        errorBar.value = true
        return
    }
    if (password.value.length == 0) {
        error.value = "Musíte vyplnit heslo"
        errorBar.value = true
        return
    }
    if (repeat.value.length == 0) {
        error.value = "Musíte zopakovat heslo"
        errorBar.value = true
        return
    }
    if (password.value != repeat.value) {
        error.value = "Helsa se neshodují"
        errorBar.value = true
        return
    }
    const res = await secureFetch("/User", {
        method: 'POST',
        body: JSON.stringify({
            email: email.value,
            fullName: name.value,
            password: password.value
        })
    })
    if (res.status != 200) {
        switch (await res.text()) {
            case "email-invalid":
                error.value = "Neplatný email"
                break
            case "already-exists":
                error.value = "Tento email u nás již je zaregistrovaný"
                break
            default:
                error.value = "Nečekaná chyba, zkuste to později"
        }
        errorBar.value = true
        return;
    }
    await useRouter().push({ name: 'login' })
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
                <h3>
                    Zarezervujte si sportoviště, snadno a rychle.
                </h3>
            </div>
        </div>
    </header>

    <main>
        <form>
            <input v-model="email" class="input-textbox" type="email" placeholder="E-mail" />
            <input v-model="name" class="input-textbox" type="text" placeholder="Jméno" />
            <input v-model="password" class="input-textbox" type="password" placeholder="Heslo" />
            <input v-model="repeat" class="input-textbox" type="password" placeholder="Zopakuj Heslo" />
            <input  @click="validate()" class="input-submit" value="Zaregistrovat se"/>
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
    padding: 0.6rem 1rem;
    font-size: 1.0rem;
    background-color: var(--vt-c-yellow);
    border: none;
    border-radius: 0.5rem;
    font-weight: 600;
    margin-top: 1rem;
    transition-duration: 100ms;
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
