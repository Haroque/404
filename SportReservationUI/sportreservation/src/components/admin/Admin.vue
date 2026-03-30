<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { RouterView, RouterLink } from 'vue-router'
import { secureFetch } from "@/auth.ts";

interface Self {
  fullName: string
}

const isScrolled = ref(false)
const isMobileMenuOpen = ref(false)
const self = ref<Self>({ fullName: '' })

const handleScroll = () => {
  isScrolled.value = window.scrollY > 20
}

const toggleMobileMenu = () => {
  isMobileMenuOpen.value = !isMobileMenuOpen.value
}

const closeMobileMenu = () => {
  isMobileMenuOpen.value = false
}

onMounted(async () => {
  window.addEventListener('scroll', handleScroll)
  const response = await secureFetch("/User/Self")
  if (response.ok) {
    self.value = await response.json()
  }
})
</script>

<template>
  <div class="leyend-admin-layout">
    <!-- Sidebar Navigation -->
    <aside class="leyend-sidebar">
      <div class="leyend-sidebar-header">
        <div class="leyend-sidebar-brand">
          <div class="leyend-brand-icon">
            <v-icon size="32" color="#FEE881">mdi-shield-account</v-icon>
          </div>
          <div class="leyend-brand-text">
            <span class="leyend-brand-name">Admin</span>
            <span class="leyend-brand-suffix">Panel</span>
          </div>
        </div>
      </div>
      
      <nav class="leyend-sidebar-nav">
        <ul class="leyend-nav-list">
          <li class="leyend-nav-item">
            <RouterLink to="/admin" class="leyend-nav-link">
              <v-icon size="20" class="nav-icon">mdi-view-dashboard</v-icon>
              <span>Dashboard</span>
            </RouterLink>
          </li>
          <li class="leyend-nav-item">
            <RouterLink to="/admin/facilities" class="leyend-nav-link">
              <v-icon size="20" class="nav-icon">mdi-home-edit</v-icon>
              <span>Sportoviště</span>
            </RouterLink>
          </li>
          <li class="leyend-nav-item">
            <RouterLink to="/admin/facility-types" class="leyend-nav-link">
              <v-icon size="20" class="nav-icon">mdi-shape</v-icon>
              <span>Typy sportovišť</span>
            </RouterLink>
          </li>
          <li class="leyend-nav-item">
            <RouterLink to="/admin/users" class="leyend-nav-link">
              <v-icon size="20" class="nav-icon">mdi-account-group</v-icon>
              <span>Uživatelé</span>
            </RouterLink>
          </li>
          <li class="leyend-nav-item">
            <RouterLink to="/admin/reservations" class="leyend-nav-link">
              <v-icon size="20" class="nav-icon">mdi-calendar-check</v-icon>
              <span>Rezervace</span>
            </RouterLink>
          </li>
          <li class="leyend-nav-item">
            <RouterLink to="/admin/price-lists" class="leyend-nav-link">
              <v-icon size="20" class="nav-icon">mdi-cash</v-icon>
              <span>Cenníky</span>
            </RouterLink>
          </li>
          <li class="leyend-nav-item">
            <RouterLink to="/admin/downtimes" class="leyend-nav-link">
              <v-icon size="20" class="nav-icon">mdi-home-alert</v-icon>
              <span>Udržby</span>
            </RouterLink>
          </li>
        </ul>
      </nav>
      
      <div class="leyend-sidebar-footer">
        <div class="leyend-user-section">
          <div class="leyend-user-info">
            <v-icon size="24" color="#FEE881" class="user-icon">mdi-account-circle</v-icon>
            <span class="user-name">{{ self.fullName || 'Admin' }}</span>
          </div>
          <button class="leyend-logout-btn">
            <v-icon size="20" class="mr-2">mdi-logout</v-icon>
            Odhlásit
          </button>
        </div>
      </div>
    </aside>

    <!-- Mobile Menu Button -->
    <button class="leyend-mobile-menu-btn mobile" @click="toggleMobileMenu">
      <v-icon size="28" color="#172563">mdi-menu</v-icon>
    </button>
    
    <!-- Mobile Navigation -->
    <div class="leyend-mobile-menu mobile" :class="{ 'open': isMobileMenuOpen }">
      <div class="leyend-mobile-menu-header">
        <div class="leyend-mobile-brand">
          <div class="leyend-brand-icon">
            <v-icon size="32" color="#FEE881">mdi-shield-account</v-icon>
          </div>
          <span class="leyend-mobile-brand-name">Admin Panel</span>
        </div>
        <button class="leyend-mobile-close-btn" @click="closeMobileMenu">
          <v-icon size="28" color="#172563">mdi-close</v-icon>
        </button>
      </div>
      
      <div class="leyend-mobile-menu-content">
        <ul class="leyend-mobile-nav-list">
          <li class="leyend-mobile-nav-item">
            <RouterLink to="/admin" class="leyend-mobile-nav-link" @click="closeMobileMenu">
              <v-icon size="24" class="mr-3">mdi-view-dashboard</v-icon>
              Dashboard
            </RouterLink>
          </li>
          <li class="leyend-mobile-nav-item">
            <RouterLink to="/admin/facilities" class="leyend-mobile-nav-link" @click="closeMobileMenu">
              <v-icon size="24" class="mr-3">mdi-home-edit</v-icon>
              Sportoviště
            </RouterLink>
          </li>
          <li class="leyend-mobile-nav-item">
            <RouterLink to="/admin/facility-types" class="leyend-mobile-nav-link" @click="closeMobileMenu">
              <v-icon size="24" class="mr-3">mdi-shape</v-icon>
              Typy sportovišť
            </RouterLink>
          </li>
          <li class="leyend-mobile-nav-item">
            <RouterLink to="/admin/users" class="leyend-mobile-nav-link" @click="closeMobileMenu">
              <v-icon size="24" class="mr-3">mdi-account-group</v-icon>
              Uživatelé
            </RouterLink>
          </li>
          <li class="leyend-mobile-nav-item">
            <RouterLink to="/admin/reservations" class="leyend-mobile-nav-link" @click="closeMobileMenu">
              <v-icon size="24" class="mr-3">mdi-calendar-check</v-icon>
              Rezervace
            </RouterLink>
          </li>
          <li class="leyend-mobile-nav-item">
            <RouterLink to="/admin/price-lists" class="leyend-mobile-nav-link" @click="closeMobileMenu">
              <v-icon size="24" class="mr-3">mdi-cash</v-icon>
              Cenníky
            </RouterLink>
          </li>
          <li class="leyend-mobile-nav-item">
            <RouterLink to="/admin/downtimes" class="leyend-mobile-nav-link" @click="closeMobileMenu">
              <v-icon size="24" class="mr-3">mdi-home-alert</v-icon>
              Udržby
            </RouterLink>
          </li>
        </ul>
        
        <div class="leyend-mobile-actions">
          <button class="leyend-button-outline mobile-btn">
            <v-icon size="20" class="mr-2">mdi-account-circle</v-icon>
            {{ self.fullName || 'Admin' }}
          </button>
          <button class="leyend-button-logout mobile-btn">
            <v-icon size="20" class="mr-2">mdi-logout</v-icon>
            Odhlásit
          </button>
        </div>
      </div>
    </div>
    
    <!-- Mobile Menu Overlay -->
    <div 
      class="leyend-mobile-overlay mobile" 
      :class="{ 'open': isMobileMenuOpen }"
      @click="closeMobileMenu"
    ></div>
    
    <!-- Main Content -->
    <main class="leyend-admin-main">
      <div class="leyend-main-content">
        <Suspense>
          <RouterView />
          <template #fallback>
            <div class="leyend-loading-state">
              <div class="leyend-loading-content">
                <v-icon size="64" color="#FEE881" class="loading-icon">mdi-loading</v-icon>
                <h3 class="loading-title">Načítám...</h3>
              </div>
            </div>
          </template>
        </Suspense>
      </div>
    </main>
  </div>
</template>

<style scoped>
.leyend-admin-layout {
  min-height: 100vh;
  background: linear-gradient(135deg, #F8FAFC 0%, #FFFFFF 50%, #F1F5F9 100%);
  display: flex;
  flex-direction: row;
}

/* Sidebar */
.leyend-sidebar {
  width: 280px;
  background: #FFFFFF;
  border-right: 1px solid #E2E8F0;
  box-shadow: 4px 0 20px rgba(0, 0, 0, 0.08);
  display: flex;
  flex-direction: column;
  position: fixed;
  top: 0;
  left: 0;
  height: 100vh;
  z-index: 1000;
}

.leyend-sidebar-header {
  padding: 32px 24px 24px 24px;
  border-bottom: 1px solid #E2E8F0;
  background: linear-gradient(135deg, #172563 0%, #373F61 100%);
}

.leyend-sidebar-brand {
  display: flex;
  align-items: center;
  gap: 16px;
}

.leyend-brand-icon {
  background: linear-gradient(135deg, #FEE881 0%, #E6D06B 100%);
  border-radius: 12px;
  padding: 12px;
  display: flex;
  align-items: center;
  justify-content: center;
  box-shadow: 0 4px 12px rgba(254, 232, 129, 0.3);
}

.leyend-brand-text {
  display: flex;
  flex-direction: column;
  line-height: 1.1;
}

.leyend-brand-name {
  font-family: 'Leyend', serif;
  font-size: 24px;
  font-weight: 500;
  color: #FFFFFF;
  margin: 0;
}

.leyend-brand-suffix {
  font-family: 'Inter', sans-serif;
  font-size: 16px;
  font-weight: 400;
  color: rgba(255, 255, 255, 0.8);
  margin: 0;
}

/* Sidebar Navigation */
.leyend-sidebar-nav {
  flex: 1;
  padding: 24px 0;
  overflow-y: auto;
}

.leyend-nav-list {
  list-style: none;
  padding: 0;
  margin: 0;
}

.leyend-nav-item {
  margin-bottom: 4px;
}

.leyend-nav-link {
  display: flex;
  align-items: center;
  gap: 12px;
  padding: 16px 20px;
  border-radius: 12px;
  text-decoration: none;
  font-family: 'Inter', sans-serif;
  font-size: 15px;
  font-weight: 500;
  color: #373F61;
  transition: all 0.3s ease;
  position: relative;
  overflow: hidden;
  margin: 0 12px;
}

.leyend-nav-link::before {
  content: '';
  position: absolute;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background: linear-gradient(135deg, #FEE881 0%, #E6D06B 100%);
  opacity: 0;
  transition: opacity 0.3s ease;
  z-index: -1;
}

.leyend-nav-link:hover {
  color: #172563;
  transform: translateX(4px);
}

.leyend-nav-link:hover::before {
  opacity: 1;
}

.leyend-nav-link.router-link-active {
  color: #172563;
  background: linear-gradient(135deg, #FEE881 0%, #E6D06B 100%);
  box-shadow: 0 4px 12px rgba(254, 232, 129, 0.3);
}

.nav-icon {
  transition: transform 0.3s ease;
  flex-shrink: 0;
}

.leyend-nav-link:hover .nav-icon {
  transform: scale(1.1);
}

/* Sidebar Footer */
.leyend-sidebar-footer {
  padding: 24px;
  border-top: 1px solid #E2E8F0;
  background: #F8FAFC;
}

.leyend-user-section {
  display: flex;
  flex-direction: column;
  gap: 16px;
}

.leyend-user-info {
  display: flex;
  align-items: center;
  gap: 12px;
  padding: 16px;
  background: #FFFFFF;
  border-radius: 12px;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.06);
}

.user-icon {
  flex-shrink: 0;
}

.user-name {
  font-family: 'Inter', sans-serif;
  font-size: 14px;
  font-weight: 500;
  color: #172563;
  margin: 0;
}

.leyend-logout-btn {
  display: flex;
  align-items: center;
  gap: 8px;
  padding: 12px 16px;
  border: 2px solid #EF4444;
  border-radius: 12px;
  background: transparent;
  color: #EF4444;
  font-family: 'Inter', sans-serif;
  font-size: 14px;
  font-weight: 500;
  cursor: pointer;
  transition: all 0.3s ease;
  text-decoration: none;
  width: 100%;
  justify-content: center;
}

.leyend-logout-btn:hover {
  background: #EF4444;
  color: #FFFFFF;
  transform: translateY(-1px);
  box-shadow: 0 4px 12px rgba(239, 68, 68, 0.3);
}

/* Main Content */
.leyend-admin-main {
  flex: 1;
  margin-left: 280px;
  min-height: 100vh;
  background: linear-gradient(135deg, #F8FAFC 0%, #FFFFFF 50%, #F1F5F9 100%);
}

.leyend-main-content {
  padding: 32px;
  max-width: calc(100% - 64px);
}

/* Loading State */
.leyend-loading-state {
  display: flex;
  justify-content: center;
  align-items: center;
  min-height: 400px;
}

.leyend-loading-content {
  text-align: center;
  padding: 48px;
  background: #FFFFFF;
  border-radius: 20px;
  box-shadow: 0 8px 32px rgba(0, 0, 0, 0.08);
}

.loading-icon {
  animation: spin 2s linear infinite;
  margin-bottom: 24px;
}

@keyframes spin {
  from { transform: rotate(0deg); }
  to { transform: rotate(360deg); }
}

.loading-title {
  font-family: 'Inter', sans-serif;
  font-size: 28px;
  font-weight: 600;
  color: #172563;
  margin: 0;
}

/* Mobile Menu */
.leyend-mobile-menu-btn {
  display: none;
  background: transparent;
  border: none;
  cursor: pointer;
  padding: 12px;
  border-radius: 8px;
  transition: all 0.3s ease;
  position: fixed;
  top: 20px;
  left: 20px;
  z-index: 1001;
}

.leyend-mobile-menu-btn:hover {
  background: #F8FAFC;
}

.leyend-mobile-menu {
  position: fixed;
  top: 0;
  left: -100%;
  width: 320px;
  height: 100vh;
  background: #FFFFFF;
  box-shadow: 4px 0 20px rgba(0, 0, 0, 0.1);
  transition: left 0.3s ease;
  z-index: 1002;
  display: none;
}

.leyend-mobile-menu.open {
  left: 0;
}

.leyend-mobile-menu-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 20px 24px;
  border-bottom: 1px solid #E2E8F0;
  background: linear-gradient(135deg, #172563 0%, #373F61 100%);
  color: #FFFFFF;
}

.leyend-mobile-brand {
  display: flex;
  align-items: center;
  gap: 12px;
}

.leyend-mobile-brand-name {
  font-family: 'Inter', sans-serif;
  font-size: 20px;
  font-weight: 600;
  color: #FFFFFF;
}

.leyend-mobile-close-btn {
  background: transparent;
  border: none;
  cursor: pointer;
  padding: 8px;
  border-radius: 8px;
  transition: all 0.3s ease;
  color: #FFFFFF;
}

.leyend-mobile-close-btn:hover {
  background: rgba(255, 255, 255, 0.1);
}

.leyend-mobile-menu-content {
  padding: 24px;
  height: calc(100vh - 80px);
  overflow-y: auto;
}

.leyend-mobile-nav-list {
  list-style: none;
  padding: 0;
  margin: 0 0 32px 0;
}

.leyend-mobile-nav-item {
  margin-bottom: 8px;
}

.leyend-mobile-nav-link {
  display: flex;
  align-items: center;
  padding: 16px 20px;
  border-radius: 12px;
  text-decoration: none;
  font-family: 'Inter', sans-serif;
  font-size: 16px;
  font-weight: 500;
  color: #373F61;
  transition: all 0.3s ease;
}

.leyend-mobile-nav-link:hover {
  background: #F8FAFC;
  color: #172563;
}

.leyend-mobile-nav-link.router-link-active {
  background: linear-gradient(135deg, #FEE881 0%, #E6D06B 100%);
  color: #172563;
}

.leyend-mobile-actions {
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.mobile-btn {
  width: 100%;
  justify-content: center;
  padding: 16px 24px;
  font-size: 16px;
}

.leyend-button-outline {
  display: flex;
  align-items: center;
  gap: 8px;
  padding: 16px 24px;
  border: 2px solid #172563;
  border-radius: 12px;
  background: transparent;
  color: #172563;
  font-family: 'Inter', sans-serif;
  font-size: 16px;
  font-weight: 500;
  cursor: pointer;
  transition: all 0.3s ease;
  text-decoration: none;
}

.leyend-button-outline:hover {
  background: #172563;
  color: #FFFFFF;
  transform: translateY(-1px);
  box-shadow: 0 6px 20px rgba(23, 37, 99, 0.3);
}

.leyend-button-logout {
  display: flex;
  align-items: center;
  gap: 8px;
  padding: 16px 24px;
  border: 2px solid #EF4444;
  border-radius: 12px;
  background: transparent;
  color: #EF4444;
  font-family: 'Inter', sans-serif;
  font-size: 16px;
  font-weight: 500;
  cursor: pointer;
  transition: all 0.3s ease;
  text-decoration: none;
}

.leyend-button-logout:hover {
  background: #EF4444;
  color: #FFFFFF;
  transform: translateY(-1px);
  box-shadow: 0 6px 20px rgba(239, 68, 68, 0.3);
}

.leyend-mobile-overlay {
  position: fixed;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background: rgba(0, 0, 0, 0.5);
  opacity: 0;
  visibility: hidden;
  transition: all 0.3s ease;
  z-index: 1000;
  display: none;
}

.leyend-mobile-overlay.open {
  opacity: 1;
  visibility: visible;
}

/* Responsive Design */
@media (max-width: 1024px) {
  .leyend-sidebar {
    width: 260px;
  }
  
  .leyend-admin-main {
    margin-left: 260px;
  }
  
  .leyend-main-content {
    padding: 24px;
  }
  
  .leyend-brand-name {
    font-size: 22px;
  }
  
  .leyend-brand-suffix {
    font-size: 14px;
  }
  
  .leyend-nav-link {
    font-size: 14px;
    padding: 14px 16px;
  }
}

@media (max-width: 768px) {
  .leyend-sidebar {
    display: none;
  }
  
  .leyend-admin-main {
    margin-left: 0;
  }
  
  .leyend-main-content {
    padding: 16px;
  }
  
  .leyend-mobile-menu-btn,
  .leyend-mobile-menu,
  .leyend-mobile-overlay {
    display: block;
  }
}

@media (max-width: 480px) {
  .leyend-main-content {
    padding: 12px;
  }
  
  .leyend-mobile-menu {
    width: 280px;
  }
  
  .leyend-mobile-menu-header {
    padding: 16px 20px;
  }
  
  .leyend-mobile-menu-content {
    padding: 20px;
  }
  
  .leyend-mobile-nav-link {
    padding: 14px 16px;
    font-size: 14px;
  }
  
  .mobile-btn {
    padding: 14px 20px;
    font-size: 14px;
  }
}
</style>