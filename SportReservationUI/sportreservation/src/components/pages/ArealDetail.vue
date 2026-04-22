<script lang="ts" setup>
import '@/assets/main.css';
import NavBar from "../views/NavBar.vue";
import { ref, onMounted } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import { secureFetch } from '../../auth';

interface ArealDetail {
    id: string;
    name: string;
    type: string;
    description: string;
    capacity: number;
    pricePerHour: number;
    image: string;
}

interface FacilityListItem {
    id: string;
    name: string;
    capacity: number;
    type: {
        id: string;
        name: string;
        description?: string;
    };
}

const route = useRoute();
const router = useRouter();
const isLoading = ref(true);
const error = ref('');
const detail = ref<ArealDetail | null>(null);

function applyQueryData(): boolean {
    const id = String(route.params.id || '');
    const name = String(route.query.name || '');
    if (!id || !name) {
        return false;
    }

    detail.value = {
        id,
        name,
        type: String(route.query.type || 'Neznámý typ'),
        description: String(route.query.description || 'Popis není k dispozici.'),
        capacity: Number(route.query.capacity || 0),
        pricePerHour: Number(route.query.pricePerHour || 0),
        image: String(route.query.image || '🏟️')
    };

    return true;
}

async function loadDetailFallback() {
    const id = String(route.params.id || '');
    if (!id) {
        error.value = 'Chybí identifikátor sportoviště.';
        return;
    }

    const response = await secureFetch(`/Facility?page=1&per_page=100`);
    if (!response.ok) {
        error.value = 'Nepodařilo se načíst detail sportoviště.';
        return;
    }

    const raw = await response.json();
    const facilities: FacilityListItem[] = Array.isArray(raw) ? raw : (raw.items || []);
    const facility = facilities.find(item => item.id === id);

    if (!facility) {
        error.value = 'Sportoviště nebylo nalezeno.';
        return;
    }

    detail.value = {
        id: facility.id,
        name: facility.name,
        type: facility.type?.name || 'Neznámý typ',
        description: facility.type?.description || 'Popis není k dispozici.',
        capacity: facility.capacity,
        pricePerHour: 0,
        image: '🏟️'
    };
}

function goToReservation() {
    if (!detail.value) return;
    router.push({ name: 'reservations', query: { facility_id: detail.value.id } });
}

onMounted(async () => {
    try {
        if (!applyQueryData()) {
            await loadDetailFallback();
        }
    } catch {
        error.value = 'Nepodařilo se načíst detail sportoviště.';
    } finally {
        isLoading.value = false;
    }
});

</script>
<template>
    <NavBar></NavBar>
    <div class="areal-detail-container">
        <div v-if="isLoading" class="state-box">Načítám detail sportoviště...</div>

        <div v-else-if="error" class="state-box error">{{ error }}</div>

        <div v-else-if="detail" class="detail-card">
            <div class="detail-head">
                <div class="detail-icon">{{ detail.image }}</div>
                <div>
                    <h1>{{ detail.name }}</h1>
                    <p class="subtitle">{{ detail.type }}</p>
                </div>
            </div>

            <p class="description">{{ detail.description }}</p>

            <div class="detail-grid">
                <div class="item">
                    <span class="label">Kapacita</span>
                    <span class="value">{{ detail.capacity }} osob</span>
                </div>
                <div class="item">
                    <span class="label">Cena</span>
                    <span class="value">{{ detail.pricePerHour }} Kč / hod</span>
                </div>
            </div>

            <div class="actions">
                <button class="primary-btn" @click="goToReservation">Rezervovat termín</button>
                <button class="secondary-btn" @click="$router.back()">Zpět</button>
            </div>
        </div>
    </div>
</template>
<style scoped>
h1 {
    color: var(--color-heading);
    font-size: 2.4rem;
    font-weight: 600;
    margin: 0;
}
.areal-detail-container {
    max-width: 1200px;
    margin: 0 auto;
    padding: 6rem 1.5rem 2rem;
}

.detail-card {
    background: var(--color-background-soft);
    border-radius: 16px;
    padding: 1.5rem;
    border: 1px solid var(--vt-c-divider);
}

.detail-head {
    display: flex;
    align-items: center;
    gap: 1rem;
}

.detail-icon {
    font-size: 2.5rem;
}

.subtitle {
    margin: 0.2rem 0 0;
    color: var(--color-text);
    opacity: 0.85;
}

.description {
    margin: 1rem 0 1.5rem;
    color: var(--color-text);
}

.detail-grid {
    display: grid;
    grid-template-columns: repeat(auto-fit, minmax(180px, 1fr));
    gap: 1rem;
}

.item {
    background: var(--color-background);
    border-radius: 10px;
    padding: 0.8rem 1rem;
    border: 1px solid var(--vt-c-divider);
}

.label {
    display: block;
    font-size: 0.85rem;
    opacity: 0.7;
}

.value {
    display: block;
    margin-top: 0.25rem;
    font-weight: 600;
}

.actions {
    display: flex;
    gap: 0.75rem;
    margin-top: 1.5rem;
}

.primary-btn,
.secondary-btn {
    border: none;
    border-radius: 10px;
    padding: 0.7rem 1rem;
    cursor: pointer;
    font-weight: 600;
}

.primary-btn {
    background: var(--vt-c-yellow);
    color: #222;
}

.secondary-btn {
    background: var(--vt-c-divider-light-1);
    color: var(--color-text);
}

.state-box {
    padding: 1rem;
    border-radius: 10px;
    background: var(--color-background-soft);
    border: 1px solid var(--vt-c-divider);
}

.state-box.error {
    color: #b42318;
}
</style>