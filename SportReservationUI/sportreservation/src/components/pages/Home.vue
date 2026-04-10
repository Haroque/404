<script setup lang="ts">
import '@/assets/main.css';
import NavBar from "../views/NavBar.vue";
import ArealList from "../views/ArealList.vue";
</script>

<template>
  <NavBar />
  
  <!-- Hero Section -->
  <section class="hero">
    <div class="hero-content">
      <h1>Pronajměte si sportoviště</h1>
      <p>Snadno, bezpečně a s nejlepšími cenami. Vyberte si z tisíců sportovišť v okolí.</p>
      <button class="btn btn-primary hero-btn">
        <i class="mdi mdi-magnify"></i>
        Prohledat sportoviště
      </button>
    </div>
    <div class="hero-background">
      <div class="gradient-orb orb-1"></div>
      <div class="gradient-orb orb-2"></div>
    </div>
  </section>


    <!-- Available Facilities -->
    <section class="facilities-section">
      <div class="section-header">
        <h2>Dostupná sportoviště</h2>
        <p>Procházejte a vyberte si z našeho rozsáhlého katalogu</p>
      </div>
      <ArealList />
    </section>
  </div>
</template>

<style scoped>
.hero {
  position: relative;
  margin-top: 4.5rem;
  padding: 4rem 1.5rem;
  min-height: 500px;
  display: flex;
  align-items: center;
  justify-content: center;
  overflow: hidden;
  background: linear-gradient(135deg, #f8f9fb 0%, #eef0f5 100%);
}

.hero-background {
  position: absolute;
  width: 100%;
  height: 100%;
  top: 0;
  left: 0;
  z-index: 0;
  opacity: 0.4;
}

.gradient-orb {
  position: absolute;
  border-radius: 50%;
  filter: blur(40px);
}

.orb-1 {
  width: 300px;
  height: 300px;
  background: linear-gradient(135deg, var(--vt-c-secondary), var(--vt-c-secondary-light));
  top: -50px;
  right: -50px;
  opacity: 0.3;
}

.orb-2 {
  width: 250px;
  height: 250px;
  background: linear-gradient(135deg, var(--vt-c-secondary-light), rgba(254, 232, 129, 0.3));
  bottom: -100px;
  left: -50px;
  opacity: 0.2;
}

.hero-content {
  position: relative;
  z-index: 1;
  max-width: 600px;
  text-align: center;
}

.hero-content h1 {
  font-size: 3rem;
  margin-bottom: 1rem;
  color: var(--color-heading);
  line-height: 1.2;
}

.hero-content p {
  font-size: 1.25rem;
  color: var(--color-text);
  margin-bottom: 2rem;
  line-height: 1.6;
}

.hero-btn {
  gap: 0.5rem;
  font-size: 1.1rem;
  padding: 1rem 2.5rem;
}

.main-content {
  max-width: 1200px;
  margin: 0 auto;
  padding: 0 1.5rem;
}

/* Features Section */
.features {
  padding: 4rem 0;
}

.section-header {
  text-align: center;
  margin-bottom: 3rem;
}

.section-header h2 {
  font-size: 2.5rem;
  margin-bottom: 0.5rem;
}

.section-header p {
  font-size: 1.1rem;
  color: var(--color-text);
}

.features-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(250px, 1fr));
  gap: 2rem;
  margin-bottom: 4rem;
}

.feature-card {
  background: var(--color-background);
  padding: 2rem;
  border-radius: var(--radius-lg);
  text-align: center;
  border: 1px solid var(--color-border);
  transition: all 0.3s ease;
}

.feature-card:hover {
  transform: translateY(-8px);
  box-shadow: var(--shadow-lg);
  border-color: var(--vt-c-primary);
}

.feature-icon {
  width: 80px;
  height: 80px;
  margin: 0 auto 1.5rem;
  display: flex;
  align-items: center;
  justify-content: center;
  background: linear-gradient(135deg, rgba(254, 232, 129, 0.1), rgba(23, 37, 99, 0.1));
  border-radius: var(--radius-lg);
  font-size: 2rem;
  color: var(--vt-c-secondary);
}

.feature-card h3 {
  margin-bottom: 0.75rem;
  font-size: 1.25rem;
}

.feature-card p {
  color: var(--color-text);
  font-size: 0.95rem;
  line-height: 1.6;
}

/* Facilities Section */
.facilities-section {
  padding: 4rem 0 6rem;
}

/* Responsive Design */
@media (max-width: 1024px) {
  .hero {
    padding: 3rem 1.5rem;
    min-height: 400px;
  }

  .hero-content h1 {
    font-size: 2.5rem;
  }

  .hero-content p {
    font-size: 1rem;
  }

  .features-grid {
    grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
    gap: 1.5rem;
  }

  .section-header h2 {
    font-size: 2rem;
  }
}

@media (max-width: 768px) {
  .hero {
    padding: 2rem 1rem;
    min-height: 300px;
    margin-top: 3.5rem;
  }

  .hero-content h1 {
    font-size: 2rem;
  }

  .hero-content p {
    font-size: 1rem;
    margin-bottom: 1.5rem;
  }

  .hero-btn {
    width: 100%;
    justify-content: center;
  }

  .orb-1, .orb-2 {
    display: none;
  }

  .features-grid {
    grid-template-columns: 1fr;
    gap: 1.5rem;
  }

  .section-header h2 {
    font-size: 1.75rem;
  }

  .facilities-section,
  .features {
    padding: 2rem 0;
  }

  .main-content {
    padding: 0 1rem;
  }
}

@media (max-width: 480px) {
  .hero {
    padding: 1.5rem 0.75rem;
    min-height: 240px;
    margin-top: 3rem;
  }

  .hero-content h1 {
    font-size: 1.4rem;
    margin-bottom: 0.5rem;
  }

  .hero-content p {
    font-size: 0.85rem;
    margin-bottom: 1.25rem;
  }

  .hero-btn {
    font-size: 0.9rem;
    padding: 0.75rem 1.5rem;
    width: 100%;
  }

  .section-header h2 {
    font-size: 1.25rem;
    margin-bottom: 1rem;
  }

  .section-header p {
    font-size: 0.85rem;
  }

  .feature-card {
    padding: 1.25rem;
    border-radius: var(--radius-md);
  }

  .feature-icon {
    width: 70px;
    height: 70px;
    font-size: 1.75rem;
    margin: 0 auto 1rem;
  }

  .feature-card h3 {
    font-size: 1.05rem;
    margin-bottom: 0.5rem;
  }

  .feature-card p {
    font-size: 0.8rem;
    line-height: 1.5;
  }

  .facilities-section,
  .features {
    padding: 1.5rem 0;
  }

  .main-content {
    padding: 0 0.75rem;
  }
}
</style>