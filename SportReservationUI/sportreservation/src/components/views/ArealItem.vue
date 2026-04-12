<script setup lang="ts">
import Splash from '../../assets/login_splash.jpg'
import type { FacilityComplexDto } from '../../lib/sportApi'

const props = defineProps<{ facility: FacilityComplexDto }>()

function getImageForFacilityType(typeName?: string): string {
    if (!typeName) return Splash
    const typeMap: { [key: string]: string } = {
        'Tenis': '🎾',
        'Badminton': '🏸',
        'Volejbal': '🏐',
        'Fotbal': '⚽',
        'Hokej': '🏒'
    }
    return typeMap[typeName] || Splash
}
</script>

<template>
    <RouterLink 
        :to="{ name: 'reservations', query: { facility_id: props.facility.id } }" 
        class="areal-item-container"
        :class="{ disabled: !props.facility.isActive }"
    >
        <div class="facility-icon">{{ getImageForFacilityType(props.facility.type?.name) }}</div>
        <div class="areal-item-text-container">
            <span class="facility-type">{{ props.facility.type?.name }}</span>
            <h2>{{ props.facility.name }}</h2>
            <p>Kapacita: {{ props.facility.capacity }} osob</p>
            <p v-if="!props.facility.isActive" class="status-closed">Uzavřeno</p>
        </div>
    </RouterLink>
</template>

<style scoped>
.areal-item-container {
    display: flex;
    flex-direction: row;
    gap: 0.5rem;
    padding: 0rem;
    background-color: var(--color-background-mute);
    color: var(--color-text);
    border-radius: 1.0rem;
    overflow: hidden;
    text-decoration: none;
    transition: all 0.3s ease;
}

.areal-item-container:hover {
    transform: translateY(-2px);
    box-shadow: 0 4px 12px rgba(0, 0, 0, 0.1);
}

.areal-item-container.disabled {
    opacity: 0.6;
    pointer-events: none;
}

.facility-icon {
    width: 8rem;
    height: 8rem;
    display: flex;
    align-items: center;
    justify-content: center;
    font-size: 3rem;
    background: var(--vt-c-white-soft);
}

.areal-item-text-container {
    display: inline-block;
    gap: 0.25rem;
    overflow: hidden;
    max-height: 8rem;
    padding: 0.75rem;
    flex: 1;
}

.areal-item-text-container > h2 {
    color: var(--color-heading);
    margin: 0.25rem 0;
}

.areal-item-text-container > .facility-type {
    color: var(--color-accent);
    text-transform: uppercase;
    font-weight: 700;
    font-size: x-small;
}

.areal-item-text-container > p {
    position: relative;
    font-size: 0.8rem;
    text-overflow: ellipsis;
    overflow: hidden;
    margin: 0.25rem 0;
}

.status-closed {
    color: #F44336;
    font-weight: 600;
}
</style>