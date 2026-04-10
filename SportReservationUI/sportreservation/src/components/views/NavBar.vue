<script setup lang="ts">
import { ref } from 'vue'

const isMenuOpen = ref(false)

const toggleMenu = () => {
  isMenuOpen.value = !isMenuOpen.value
}

const closeMenu = () => {
  isMenuOpen.value = false
}
</script>

<template>
  <nav class="navbar">
    <div class="navbar-container">
      <RouterLink to="/" class="navbar-brand" @click="closeMenu">
        <span class="brand-text"><span class="primary">Sport</span>Reservation</span>
      </RouterLink>

      <!-- Desktop Menu -->
      <div class="navbar-menu">
        <RouterLink class="nav-link" :to="{ name: 'home' }">
          <i class="mdi mdi-home"></i>
          Sportoviště
        </RouterLink>
        <RouterLink class="nav-link" :to="{ name: 'reservations' }">
          <i class="mdi mdi-calendar-check"></i>
          Rezervace
        </RouterLink>
        <RouterLink class="nav-link" :to="{ name: 'account' }">
          <i class="mdi mdi-account"></i>
          Účet
        </RouterLink>
      </div>

      <!-- Mobile Menu Button -->
      <button class="mobile-menu-btn" @click="toggleMenu" :aria-label="isMenuOpen ? 'Zavřít menu' : 'Otevřít menu'">
        <span :class="['hamburger', { active: isMenuOpen }]"></span>
      </button>
    </div>

    <!-- Mobile Menu -->
    <Transition name="slide">
      <div v-if="isMenuOpen" class="mobile-menu">
        <RouterLink class="mobile-nav-link" :to="{ name: 'home' }" @click="closeMenu">
          <i class="mdi mdi-home"></i>
          Sportoviště
        </RouterLink>
        <RouterLink class="mobile-nav-link" :to="{ name: 'reservations' }" @click="closeMenu">
          <i class="mdi mdi-calendar-check"></i>
          Rezervace
        </RouterLink>
        <RouterLink class="mobile-nav-link" :to="{ name: 'account' }" @click="closeMenu">
          <i class="mdi mdi-account"></i>
          Účet
        </RouterLink>
      </div>
    </Transition>
  </nav>
</template>

<style scoped>
.navbar {
  position: fixed;
  top: 0;
  left: 0;
  right: 0;
  z-index: 1000;
  background: linear-gradient(135deg, rgba(255, 255, 255, 0.98) 0%, rgba(250, 250, 250, 0.98) 100%);
  backdrop-filter: blur(8px);
  border-bottom: 1px solid var(--color-border);
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.05);
}

.navbar-container {
  max-width: 1200px;
  margin: 0 auto;
  padding: 1rem 1.5rem;
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.navbar-brand {
  display: flex;
  align-items: center;
  text-decoration: none;
  transition: transform 0.3s ease;
}

.navbar-brand:hover {
  transform: scale(1.05);
}

.brand-text {
  font-family: 'Baloo 2', sans-serif;
  font-size: 1.5rem;
  font-weight: 700;
  background: linear-gradient(135deg, #1E88E5 0%, #00BCD4 100%);
  -webkit-background-clip: text;
  -webkit-text-fill-color: transparent;
  background-clip: text;
}

.brand-text .primary {
  color: var(--vt-c-secondary);
  -webkit-text-fill-color: var(--vt-c-secondary);
}

/* Desktop Navigation */
.navbar-menu {
  display: flex;
  gap: 2rem;
  align-items: center;
}

.nav-link {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  color: var(--color-text);
  text-decoration: none;
  font-weight: 500;
  transition: all 0.3s ease;
  padding: 0.5rem 1rem;
  border-radius: var(--radius-md);
  position: relative;
}

.nav-link i {
  font-size: 1.25rem;
}

.nav-link::after {
  content: '';
  position: absolute;
  bottom: 0;
  left: 50%;
  width: 0;
  height: 2px;
  background: var(--vt-c-primary);
  transform: translateX(-50%);
  transition: width 0.3s ease;
}

.nav-link:hover {
  color: var(--vt-c-secondary);
}

.nav-link:hover::after {
  width: 100%;
}

.router-link-active {
  color: var(--vt-c-secondary);
  background-color: rgba(23, 37, 99, 0.08);
}

.router-link-active::after {
  width: 100%;
}

/* Mobile Menu Button */
.mobile-menu-btn {
  display: none;
  background: none;
  border: none;
  cursor: pointer;
  padding: 0.5rem;
  width: 2.5rem;
  height: 2.5rem;
  flex-direction: column;
  justify-content: center;
  align-items: center;
  gap: 0.4rem;
}

.hamburger {
  display: flex;
  flex-direction: column;
  gap: 0.4rem;
  width: 1.5rem;
}

.hamburger::before,
.hamburger::after,
.hamburger span {
  content: '';
  width: 100%;
  height: 2.5px;
  background: var(--vt-c-primary);
  border-radius: 2px;
  transition: all 0.3s ease;
  display: block;
}

.hamburger.active::before {
  transform: rotate(45deg) translate(0.5rem, 0.8rem);
}

.hamburger.active span {
  opacity: 0;
}

.hamburger.active::after {
  transform: rotate(-45deg) translate(0.5rem, -0.8rem);
}

/* Mobile Menu */
.mobile-menu {
  display: none;
  flex-direction: column;
  gap: 0;
  background: var(--color-background-soft);
  border-bottom: 1px solid var(--color-border);
  padding: 1rem 0;
}

.mobile-nav-link {
  display: flex;
  align-items: center;
  gap: 1rem;
  color: var(--color-text);
  text-decoration: none;
  font-weight: 500;
  padding: 1rem 1.5rem;
  transition: all 0.3s ease;
  border-left: 3px solid transparent;
}

.mobile-nav-link i {
  font-size: 1.5rem;
  color: var(--vt-c-primary);
}

.mobile-nav-link:hover {
  background-color: var(--color-background-mute);
  border-left-color: var(--vt-c-primary);
}

.mobile-nav-link.router-link-active {
  background-color: rgba(30, 136, 229, 0.08);
  border-left-color: var(--vt-c-primary);
}

/* Transitions */
.slide-enter-active,
.slide-leave-active {
  transition: all 0.3s ease;
}

.slide-enter-from {
  transform: translateY(-10px);
  opacity: 0;
}

.slide-leave-to {
  transform: translateY(-10px);
  opacity: 0;
}

/* Responsive Design */
@media (max-width: 768px) {
  .navbar-container {
    padding: 1rem 1rem;
  }

  .brand-text {
    font-size: 1.25rem;
  }

  .navbar-menu {
    display: none;
  }

  .mobile-menu-btn {
    display: flex;
  }

  .mobile-menu {
    display: flex;
  }

  .mobile-nav-link {
    padding: 1rem 1rem;
    min-height: 44px;
  }
}

@media (max-width: 480px) {
  .navbar-container {
    padding: 0.75rem 0.5rem;
  }

  .brand-text {
    font-size: 1.1rem;
  }

  .mobile-nav-link {
    padding: 0.875rem 1rem;
    font-size: 0.95rem;
  }

  .mobile-menu-btn {
    padding: 0.5rem;
    width: 2.25rem;
    height: 2.25rem;
  }
}
</style>