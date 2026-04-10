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
            },
            {
                path: '/reservations',
                name: 'reservations',
                component: () => import('./components/pages/Reservations.vue')
            },
            {
                path: '/account',
                name: 'account',
                component: () => import('./components/pages/Account.vue')
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
                        component: () => import('./components/admin/AdminFacilityType.vue')
                    },
                    {
                        path: 'facilities',
                        name: 'admin-facilities',
                        component: () => import('./components/admin/AdminFacilities.vue')
                    },
                    {
                        path: 'price-lists',
                        name: 'admin-price-lists',
                        component: () => import('./components/admin/AdminPrices.vue')
                    },
                    {
                        path: 'downtimes',
                        name: 'admin-downtimes',
                        component: () => import('./components/admin/AdminDowntimes.vue')
                    },
                    {
                        path: 'reservations',
                        name: 'admin-reservations',
                        component: () => import('./components/admin/AdminReservations.vue')
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
