<script lang="ts" setup>
import '@/assets/main.css';
import NavBar from "../views/NavBar.vue";
import { ref } from 'vue';

const facility = ref({
  name: 'Kurt č. 1 - Tenis',
  type: 'Tenisový kurt',
  price: 300,
  rating: 4.8,
  reviews: 234,
  description: 'Profesionální tenisový kurt s moderním vybavením. Osvětlený kurt ideální pro zápasy i trénink.',
  capacity: 4,
  amenities: ['Osvětlení', 'Parkování', 'Šatny', 'Sprchy', 'WiFi', 'Bar']
});
</script>

<template>
  <NavBar />
  <div class="areal-detail-container">
    <!-- Hero Image -->
    <div class="hero-image-section">
      <div class="hero-placeholder">
        <i class="mdi mdi-tennis"></i>
      </div>
    </div>

    <!-- Content Section -->
    <div class="detail-content">
      <!-- Header -->
      <div class="header-section">
        <div class="header-top">
          <h1 class="facility-name">{{ facility.name }}</h1>
          <div class="rating-badge">
            <span class="stars">
              <i class="mdi mdi-star"></i>
              {{ facility.rating }}
            </span>
            <span class="reviews">({{ facility.reviews }} recenzí)</span>
          </div>
        </div>
        <p class="facility-type">{{ facility.type }}</p>
      </div>

      <!-- Quick Info Grid -->
      <div class="quick-info-grid">
        <div class="info-card">
          <i class="mdi mdi-cash"></i>
          <div>
            <span class="label">Cena za hodinu</span>
            <span class="value">{{ facility.price }} Kč</span>
          </div>
        </div>
        <div class="info-card">
          <i class="mdi mdi-account-multiple"></i>
          <div>
            <span class="label">Kapacita</span>
            <span class="value">{{ facility.capacity }} osob</span>
          </div>
        </div>
        <div class="info-card">
          <i class="mdi mdi-check-circle"></i>
          <div>
            <span class="label">Stav</span>
            <span class="value">Dostupné</span>
          </div>
        </div>
      </div>

      <!-- Description -->
      <section class="section">
        <h2>O sportoviště</h2>
        <p>{{ facility.description }}</p>
      </section>

      <!-- Amenities -->
      <section class="section">
        <h2>Vybavení a služby</h2>
        <div class="amenities-grid">
          <div v-for="amenity in facility.amenities" :key="amenity" class="amenity-tag">
            <i class="mdi mdi-check"></i>
            {{ amenity }}
          </div>
        </div>
      </section>

      <!-- CTA Section -->
      <section class="cta-section">
        <div class="cta-content">
          <h2>Připraveni rezervovat?</h2>
          <p>Vyberte si čas a začněte s rezervací nyní</p>
          <RouterLink :to="{ name: 'reservations' }" class="btn btn-primary">
            <i class="mdi mdi-calendar-check"></i>
            Přejít na rezervace
          </RouterLink>
        </div>
      </section>
    </div>
  </div>
</template>

<style scoped>
.areal-detail-container {
  display: flex;
  flex-direction: column;
  width: 100%;
  min-height: 100vh;
  background: var(--color-background);
  padding-top: 4rem;
}

.hero-image-section {
  width: 100%;
  height: 400px;
  background: linear-gradient(135deg, var(--vt-c-secondary) 0%, var(--vt-c-secondary-light) 100%);
  display: flex;
  align-items: center;
  justify-content: center;
  position: relative;
  overflow: hidden;
}

.hero-placeholder {
  font-size: 150px;
  color: white;
  opacity: 0.3;
}

.detail-content {
  max-width: 1000px;
  margin: 0 auto;
  width: 100%;
  padding: 3rem 1.5rem;
}

.header-section {
  margin-bottom: 3rem;
}

.header-top {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  gap: 2rem;
  margin-bottom: 1rem;
  flex-wrap: wrap;
}

.facility-name {
  font-size: 2.5rem;
  font-weight: 700;
  color: var(--color-heading);
  margin: 0;
  line-height: 1.2;
}

.facility-type {
  font-size: 1.1rem;
  color: var(--color-text);
  margin: 0.5rem 0 0;
}

.rating-badge {
  display: flex;
  align-items: center;
  gap: 0.75rem;
  background: linear-gradient(135deg, rgba(254, 232, 129, 0.1), rgba(255, 152, 0, 0.1));
  padding: 1rem 1.5rem;
  border-radius: var(--radius-lg);
  border: 1px solid rgba(254, 232, 129, 0.2);
}

.stars {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  font-size: 1.25rem;
  font-weight: 700;
  color: #FFA726;
}

.stars i {
  color: #FFA726;
  font-size: 1.5rem;
}

.reviews {
  font-size: 0.9rem;
  color: var(--color-text);
}

/* Quick Info Grid */
.quick-info-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(220px, 1fr));
  gap: 1.5rem;
  margin-bottom: 3rem;
}

.info-card {
  background: linear-gradient(135deg, rgba(254, 232, 129, 0.05), rgba(23, 37, 99, 0.05));
  border: 2px solid rgba(254, 232, 129, 0.15);
  border-radius: var(--radius-lg);
  padding: 1.5rem;
  display: flex;
  align-items: center;
  gap: 1rem;
  transition: all 0.3s ease;
}

.info-card:hover {
  border-color: var(--vt-c-primary);
  box-shadow: var(--shadow-lg);
  transform: translateY(-4px);
}

.info-card i {
  font-size: 2rem;
  color: var(--vt-c-primary);
}

.info-card div {
  display: flex;
  flex-direction: column;
}

.label {
  font-size: 0.9rem;
  color: var(--color-text);
  opacity: 0.8;
}

.value {
  font-size: 1.25rem;
  font-weight: 700;
  color: var(--color-heading);
}

/* Sections */
.section {
  margin-bottom: 3rem;
  padding-bottom: 3rem;
  border-bottom: 1px solid var(--color-border);
}

.section h2 {
  font-size: 1.75rem;
  margin-bottom: 1.5rem;
}

.section p {
  font-size: 1rem;
  line-height: 1.8;
  color: var(--color-text);
  margin: 0;
}

/* Amenities Grid */
.amenities-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(150px, 1fr));
  gap: 1rem;
}

.amenity-tag {
  background: linear-gradient(135deg, var(--color-background-soft), rgba(254, 232, 129, 0.03));
  border: 2px solid rgba(254, 232, 129, 0.1);
  border-radius: var(--radius-md);
  padding: 1rem;
  display: flex;
  align-items: center;
  gap: 0.75rem;
  font-weight: 500;
  transition: all 0.3s ease;
}

.amenity-tag:hover {
  border-color: var(--vt-c-primary);
  background: linear-gradient(135deg, rgba(254, 232, 129, 0.05), rgba(23, 37, 99, 0.05));
}

.amenity-tag i {
  color: var(--vt-c-success);
  font-size: 1.25rem;
}

/* CTA Section */
.cta-section {
  background: linear-gradient(135deg, var(--vt-c-secondary), var(--vt-c-secondary-light));
  border-radius: var(--radius-xl);
  padding: 3rem;
  text-align: center;
  margin-top: 2rem;
}

.cta-content h2 {
  color: white;
  font-size: 2rem;
  margin-bottom: 0.75rem;
}

.cta-content p {
  color: rgba(255, 255, 255, 0.9);
  font-size: 1.1rem;
  margin-bottom: 2rem;
}

.cta-section .btn {
  display: inline-flex;
  gap: 0.5rem;
}

/* Responsive Design */
@media (max-width: 768px) {
  .areal-detail-container {
    padding-top: 3.5rem;
  }

  .hero-image-section {
    height: 300px;
  }

  .hero-placeholder {
    font-size: 100px;
  }

  .detail-content {
    padding: 2rem 1rem;
  }

  .header-top {
    flex-direction: column;
    align-items: flex-start;
  }

  .facility-name {
    font-size: 2rem;
  }

  .quick-info-grid {
    grid-template-columns: 1fr;
    margin-bottom: 2rem;
  }

  .section {
    margin-bottom: 2rem;
    padding-bottom: 2rem;
  }

  .section h2 {
    font-size: 1.5rem;
    margin-bottom: 1rem;
  }

  .amenities-grid {
    gap: 0.75rem;
  }

  .cta-section {
    padding: 2rem;
  }

  .cta-content h2 {
    font-size: 1.5rem;
  }

  .cta-content p {
    font-size: 1rem;
  }
}

@media (max-width: 480px) {
  .areal-detail-container {
    padding-top: 3rem;
  }

  .hero-image-section {
    height: 200px;
  }

  .hero-placeholder {
    font-size: 70px;
  }

  .detail-content {
    padding: 1rem 0.75rem;
  }

  .header-section {
    margin-bottom: 1.5rem;
  }

  .header-top {
    flex-direction: column;
    gap: 1rem;
  }

  .facility-name {
    font-size: 1.4rem;
  }

  .facility-type {
    font-size: 0.95rem;
  }

  .rating-badge {
    width: 100%;
    justify-content: center;
    padding: 0.75rem 1rem;
  }

  .stars {
    font-size: 1rem;
  }

  .stars i {
    font-size: 1.25rem;
  }

  .reviews {
    font-size: 0.8rem;
  }

  .quick-info-grid {
    grid-template-columns: 1fr;
    gap: 0.75rem;
    margin-bottom: 1.5rem;
  }

  .info-card {
    padding: 1rem;
    gap: 0.75rem;
  }

  .info-card i {
    font-size: 1.5rem;
  }

  .label {
    font-size: 0.8rem;
  }

  .value {
    font-size: 1rem;
  }

  .section {
    margin-bottom: 1.5rem;
    padding-bottom: 1.5rem;
  }

  .section h2 {
    font-size: 1.25rem;
    margin-bottom: 0.75rem;
  }

  .section p {
    font-size: 0.95rem;
    line-height: 1.6;
  }

  .amenities-grid {
    grid-template-columns: repeat(2, 1fr);
    gap: 0.75rem;
  }

  .amenity-tag {
    padding: 0.75rem;
    font-size: 0.85rem;
    gap: 0.5rem;
  }

  .amenity-tag i {
    font-size: 1rem;
  }

  .cta-section {
    padding: 1.5rem;
    margin-top: 1rem;
    border-radius: var(--radius-lg);
  }

  .cta-content h2 {
    color: white;
    font-size: 1.3rem;
    margin-bottom: 0.5rem;
  }

  .cta-content p {
    color: rgba(255, 255, 255, 0.9);
    font-size: 0.95rem;
    margin-bottom: 1rem;
  }

  .cta-section .btn {
    display: flex;
    justify-content: center;
    gap: 0.5rem;
    min-height: 44px;
    padding: 0.75rem 1.5rem;
    font-size: 0.9rem;
  }
}
</style>