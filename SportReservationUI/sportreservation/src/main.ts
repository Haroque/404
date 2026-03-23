import { createApp } from 'vue'
import App from './App.vue'
import { initRouter } from './router'
import { createVuetify } from 'vuetify'
import * as components from 'vuetify/components'
import * as directives from 'vuetify/directives'
import * as labs from 'vuetify/labs/components'

import 'vuetify/styles'
import '@mdi/font/css/materialdesignicons.css'
import './assets/main.css'

createApp(App)
    .use(initRouter())
    .use(createVuetify({
        theme: {
            defaultTheme: 'light' // aby nerozbilo current styles
        },
        components: {
            ...components,
            ...directives,
            ...labs
        },
    }))
    .mount('#app')
