import { createRouter, createWebHistory, type Router } from 'vue-router'

let router: Router

export function initRouter() {
    router = createRouter({
        history: createWebHistory(import.meta.env.BASE_URL),
        routes: [
            {
                path: '/',
                name: 'home',
                component: () => import('./components/pages/Home.vue')
            },
            {
                path: '/login',
                name: 'login',
                component: () => import('./components/pages/Login.vue')
            },
            {
                path: '/register',
                name: 'register',
                component: () => import('./components/pages/Register.vue')
            },
            {
                path: '/areal/:id',
                name: 'areal-detail',
                component: () => import('./components/pages/ArealDetail.vue')
            }
        ]
    })
    return router
}

export function useRouter(): Router {
    return router
}
