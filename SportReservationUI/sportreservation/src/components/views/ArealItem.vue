<script setup lang="ts">
import Splash from '../../assets/login_splash.jpg'
const props = defineProps(['facility'])
</script>

<template>
  <RouterLink :to="{ name: 'register' }" class="facility-card">
    <div class="facility-image-wrapper">
      <img :src="Splash" alt="Facility image" class="facility-image" />
      <div class="facility-badge">{{ props.facility.message }}</div>
    </div>
    <div class="facility-content">
      <h3 class="facility-name">Název sportoviště</h3>
      <p class="facility-description">Lorem ipsum dolor sit amet, consectetur adipiscing elit. Donec diam lectus.</p>
      <div class="facility-footer">
        <span class="facility-price">300 Kč/hod</span>
        <span class="facility-rating">
          <i class="mdi mdi-star"></i>
          4.8 (234)
        </span>
      </div>
    </div>
  </RouterLink>
</template>

<style scoped>
.facility-card {
  display: flex;
  flex-direction: column;
  background: var(--color-background);
  border: 1px solid var(--color-border);
  border-radius: var(--radius-lg);
  overflow: hidden;
  text-decoration: none;
  color: inherit;
  transition: all 0.3s ease;
  height: 100%;
}

.facility-card:hover {
  transform: translateY(-8px);
  box-shadow: var(--shadow-lg);
  border-color: var(--vt-c-primary);
}

.facility-image-wrapper {
  position: relative;
  width: 100%;
  padding-bottom: 66.67%;
  overflow: hidden;
  background: var(--color-background-soft);
}

.facility-image {
  position: absolute;
  top: 0;
  left: 0;
  width: 100%;
  height: 100%;
  object-fit: cover;
  transition: transform 0.3s ease;
}

.facility-card:hover .facility-image {
  transform: scale(1.08);
}

.facility-badge {
  position: absolute;
  top: 1rem;
  left: 1rem;
  background: rgba(30, 136, 229, 0.9);
  color: white;
  padding: 0.5rem 1rem;
  border-radius: var(--radius-md);
  font-size: 0.75rem;
  font-weight: 700;
  text-transform: uppercase;
  letter-spacing: 0.05em;
  backdrop-filter: blur(4px);
}

.facility-content {
  display: flex;
  flex-direction: column;
  gap: 0.75rem;
  padding: 1.5rem;
  flex: 1;
  display: flex;
  flex-direction: column;
}

.facility-name {
  font-size: 1.25rem;
  margin: 0;
  color: var(--color-heading);
  font-weight: 700;
}

.facility-description {
  font-size: 0.9rem;
  color: var(--color-text);
  flex-grow: 1;
  line-height: 1.5;
  margin: 0;
  display: -webkit-box;
  -webkit-line-clamp: 2;
  -webkit-box-orient: vertical;
  overflow: hidden;
}

.facility-footer {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-top: auto;
  padding-top: 1rem;
  border-top: 1px solid var(--color-border);
}

.facility-price {
  font-size: 1.1rem;
  font-weight: 700;
  color: var(--vt-c-secondary);
}

.facility-rating {
  display: flex;
  align-items: center;
  gap: 0.25rem;
  font-size: 0.9rem;
  color: var(--vt-c-warning);
  font-weight: 600;
}

.facility-rating i {
  font-size: 1rem;
}

/* Responsive Design */
@media (max-width: 768px) {
  .facility-card {
    border-radius: var(--radius-md);
  }

  .facility-image-wrapper {
    padding-bottom: 60%;
  }

  .facility-content {
    padding: 1rem;
    gap: 0.5rem;
  }

  .facility-name {
    font-size: 1.05rem;
  }

  .facility-description {
    font-size: 0.8rem;
    -webkit-line-clamp: 2;
  }

  .facility-badge {
    top: 0.75rem;
    left: 0.75rem;
    font-size: 0.65rem;
    padding: 0.375rem 0.75rem;
  }

  .facility-footer {
    margin-top: 0.75rem;
    padding-top: 0.75rem;
    gap: 0.5rem;
  }

  .facility-price {
    font-size: 1rem;
  }

  .facility-rating {
    font-size: 0.8rem;
  }
}

@media (max-width: 480px) {
  .facility-image-wrapper {
    padding-bottom: 55%;
  }

  .facility-content {
    padding: 0.75rem;
    gap: 0.375rem;
  }

  .facility-name {
    font-size: 0.95rem;
    margin-bottom: 0.25rem;
  }

  .facility-description {
    font-size: 0.75rem;
    -webkit-line-clamp: 1;
    margin: 0.25rem 0;
  }

  .facility-badge {
    top: 0.5rem;
    left: 0.5rem;
    font-size: 0.6rem;
    padding: 0.25rem 0.5rem;
  }

  .facility-footer {
    display: flex;
    justify-content: space-between;
    align-items: center;
    flex-wrap: wrap;
    gap: 0.25rem;
    margin-top: 0.5rem;
    padding-top: 0.5rem;
    border-top: 1px solid var(--color-border);
  }

  .facility-price {
    font-size: 0.9rem;
  }

  .facility-rating {
    font-size: 0.75rem;
  }
}
</style>