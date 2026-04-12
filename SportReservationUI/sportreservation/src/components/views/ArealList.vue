<script setup lang="ts">
import { ref, onMounted } from 'vue'
import ArealItem from "./ArealItem.vue"
import { API_URL, secureFetch } from '../../auth'
import type { FacilityComplexDto, FacilityTypeDto } from '../../lib/sportApi'

const facilities = ref<FacilityComplexDto[]>([])
const facilityTypes = ref<FacilityTypeDto[]>([])
const selectedTypeId = ref<string>('')
const isLoading = ref(false)

async function loadFacilities() {
  try {
    isLoading.value = true
    let url = `${API_URL}/Facility?page=1&per_page=100`
    if (selectedTypeId.value) {
      url += `&type_id=${selectedTypeId.value}`
    }
    console.log('Loading facilities from:', url)
    const response = await secureFetch(url)
    console.log('Response status:', response.status)
    if (response.ok) {
      const data = await response.json()
      console.log('Facilities data:', data)
      facilities.value = data.items || data
      console.log('Facilities loaded:', facilities.value.length)
    } else {
      console.error('Failed to load facilities:', response.status)
    }
  } catch (error) {
    console.error('Error loading facilities:', error)
  } finally {
    isLoading.value = false
  }
}

async function loadFacilityTypes() {
  try {
    console.log('Loading facility types from:', `${API_URL}/FacilityType`)
    const response = await secureFetch(`${API_URL}/FacilityType`)
    console.log('Facility types response status:', response.status)
    if (response.ok) {
      const data = await response.json()
      console.log('Facility types data:', data)
      facilityTypes.value = data
      console.log('Facility types loaded:', facilityTypes.value.length)
    } else {
      console.error('Failed to load facility types:', response.status)
    }
  } catch (error) {
    console.error('Error loading facility types:', error)
  }
}

onMounted(() => {
  loadFacilityTypes()
  loadFacilities()
})

function handleTypeChange() {
  loadFacilities()
}
</script>

<template>
    <div class="areal-list-container">
        <div class="search-bar">
            <label for="type-filter">Filtrovat podle typu:</label>
            <select id="type-filter" v-model="selectedTypeId" @change="handleTypeChange">
                <option value="">Všechny typy</option>
                <option v-for="type in facilityTypes" :key="type.id" :value="type.id">
                    {{ type.name }}
                </option>
            </select>
        </div>
        <div v-if="isLoading" class="loading">Načítám...</div>
        <ArealItem 
            v-for="facility in facilities" 
            :key="facility.id" 
            :facility="facility"
            class="subheader-item"
        >
        </ArealItem>
        <div v-if="!isLoading && facilities.length === 0" class="no-results">
            Žádná sportoviště nenalezena
        </div>
    </div>
</template>

<style scoped>
.areal-list-container {
    height: fit-content;
    overflow-y: hidden;
    width: 100%;
    display: flex;
    flex-direction: column;
    gap: 1rem;
}

.search-bar {
    display: flex;
    align-items: center;
    gap: 1rem;
    padding: 1rem;
    background: var(--vt-c-white-soft);
    border-radius: 0.5rem;
}

.search-bar label {
    font-weight: 500;
    color: var(--color-heading);
}

.search-bar select {
    padding: 0.5rem 1rem;
    border: 1px solid var(--vt-c-divider);
    border-radius: 0.5rem;
    background: var(--vt-c-white);
    color: var(--color-text);
    min-width: 200px;
}

.loading, .no-results {
    text-align: center;
    padding: 2rem;
    color: var(--color-text);
    opacity: 0.8;
}

@media (min-width: 1024px) {
    .areal-list-container {
        display: grid;
        grid: auto / auto auto;
    }
}
</style>