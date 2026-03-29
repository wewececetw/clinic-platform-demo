import { createRouter, createWebHistory } from 'vue-router'

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    {
      path: '/',
      name: 'PatientCheckIn',
      component: () => import('@/views/patient/CheckIn.vue'),
    },
    {
      path: '/queue',
      name: 'PatientQueue',
      component: () => import('@/views/patient/Queue.vue'),
    },
    {
      path: '/nurse',
      name: 'NurseDashboard',
      component: () => import('@/views/nurse/Dashboard.vue'),
    },
    {
      path: '/doctor',
      name: 'DoctorConsult',
      component: () => import('@/views/doctor/Consult.vue'),
    },
    {
      path: '/pharmacy',
      name: 'PharmacyQueue',
      component: () => import('@/views/pharmacy/Queue.vue'),
    },
    {
      path: '/admin',
      name: 'AdminDashboard',
      component: () => import('@/views/admin/Dashboard.vue'),
    },
  ],
})

export default router
