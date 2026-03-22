import { createApp } from 'vue'
import App from './App.vue'
import { initRouter } from './router'
import { createVuetify } from 'vuetify'
import * as components from 'vuetify/components'
import * as directives from 'vuetify/directives'

import './assets/main.css'

createApp(App)
    .use(initRouter())
    .use(createVuetify({
        theme: {
            defaultTheme: 'light' // aby nerozbilo current styles
        },
        components,
        directives
    }))
    .mount('#app')
