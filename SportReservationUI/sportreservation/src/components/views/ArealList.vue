<script setup lang="ts">
import { ref } from 'vue'
import ArealItem from "./ArealItem.vue"

const items = ref([
  { message: 'Tenisový kurt' }, 
  { message: 'Posilovna' }, 
  { message: 'Posilovna' }, 
  { message: 'Basketballový kurt' }, 
  { message: 'Tenisový kurt' }, 
  { message: 'Tenisový kurt' }
])

const sortBy = ref('popular')
const filterType = ref('all')
</script>

<template>
  <div class="facilities-container">
    <!-- Filters and Sort -->
    <div class="facilities-controls">
      <div class="filter-group">
        <label for="filter">Typ sportoviště:</label>
        <select id="filter" v-model="filterType" class="filter-select">
          <option value="all">Všechny typy</option>
          <option value="tennis">Tenis</option>
          <option value="gym">Posilovna</option>
          <option value="basketball">Basketbal</option>
        </select>
      </div>

      <div class="sort-group">
        <label for="sort">Řadit podle:</label>
        <select id="sort" v-model="sortBy" class="sort-select">
          <option value="popular">Nejpopulárnější</option>
          <option value="price-low">Cena: nejnižší</option>
          <option value="price-high">Cena: nejvyšší</option>
          <option value="rating">Hodnocení</option>
        </select>
      </div>
    </div>

    <!-- Facilities Grid -->
    <div class="facilities-grid">
      <ArealItem 
        v-for="(item, index) in items" 
        :key="index"
        :facility="item"
      />
    </div>
  </div>
</template>

<style scoped>
.facilities-container {
  width: 100%;
}

.facilities-controls {
  display: flex;
  gap: 2rem;
  margin-bottom: 2rem;
  flex-wrap: wrap;
}

.filter-group,
.sort-group {
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
}

.filter-group label,
.sort-group label {
  font-weight: 600;
  color: var(--color-heading);
  font-size: 0.95rem;
}

.filter-select,
.sort-select {
  padding: 0.75rem 1rem;
  border: 2px solid var(--color-border);
  border-radius: var(--radius-md);
  font-size: 0.95rem;
  background-color: var(--color-background);
  color: var(--color-heading);
  cursor: pointer;
  transition: all 0.3s ease;
}

.filter-select:hover,
.sort-select:hover {
  border-color: var(--vt-c-primary);
}

.filter-select:focus,
.sort-select:focus {
  outline: none;
  border-color: var(--vt-c-primary);
  box-shadow: 0 0 0 3px rgba(30, 136, 229, 0.1);
}

.facilities-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(280px, 1fr));
  gap: 2rem;
  width: 100%;
}

/* Responsive Design */
@media (max-width: 1024px) {
  .facilities-grid {
    grid-template-columns: repeat(auto-fill, minmax(250px, 1fr));
    gap: 1.5rem;
  }
}

@media (max-width: 768px) {
  .facilities-controls {
    flex-direction: column;
    gap: 1rem;
    margin-bottom: 1.5rem;
  }

  .filter-group,
  .sort-group {
    width: 100%;
  }

  .filter-select,
  .sort-select {
    width: 100%;
    min-height: 44px;
    padding: 0.75rem;
  }

  .facilities-grid {
    grid-template-columns: repeat(auto-fill, minmax(160px, 1fr));
    gap: 1rem;
  }
}

@media (max-width: 480px) {
  .facilities-container {
    width: 100%;
  }

  .facilities-controls {
    flex-direction: column;
    gap: 0.75rem;
    margin-bottom: 1rem;
  }

  .filter-group,
  .sort-group {
    width: 100%;
  }

  .filter-group label,
  .sort-group label {
    font-size: 0.85rem;
    margin-bottom: 0.375rem;
  }

  .filter-select,
  .sort-select {
    width: 100%;
    padding: 0.75rem 0.5rem;
    font-size: 0.9rem;
    min-height: 44px;
  }

  .facilities-grid {
    grid-template-columns: 1fr;
    gap: 0.75rem;
  }
}
</style>