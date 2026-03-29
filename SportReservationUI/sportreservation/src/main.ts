import { createApp } from 'vue'
import App from './App.vue'
import { initRouter } from './router'
import { createVuetify } from 'vuetify'
import colors from 'vuetify/util/colors'
import * as components from 'vuetify/components'
import * as directives from 'vuetify/directives'

import './assets/main.css'

createApp(App)
    .use(initRouter())
    .use(createVuetify({
        theme: {
            defaultTheme: 'light', // aby nerozbilo current styles
            themes: {
                light: {
                    dark: false,
                    colors: {
                        primary: colors.yellow.lighten2, // #E53935
                        secondary: colors.indigo.darken2, // #FFCDD2
                    }
                },
            },
        },
        components,
        directives
    }))
    .mount('#app')
