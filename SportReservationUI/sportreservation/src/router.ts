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
                path: '/admin',
                name: 'admin',
                component: () => import("./components/admin/Admin.vue"),
                children: [
                    {
                        path: '',
                        name: 'admin-dashboard',
                        component: () => import('./components/admin/AdminDashboard.vue')
                    },
                    {
                        path: 'users',
                        name: 'admin-users',
                        component: () => import('./components/admin/AdminUsers.vue')
                    },
                    {
                        path: 'facility-types',
                        name: 'admin-facility-types',
                        component: () => import('./components/admin/FacilityType.vue')
                    }
                ]
            }
        ]
    })
    return router
}

export function useRouter(): Router {
    return router
}
