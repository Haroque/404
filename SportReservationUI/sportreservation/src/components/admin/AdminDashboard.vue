<script setup lang="ts">
import { ref, computed } from 'vue'
import { secureFetch } from "@/auth.ts"

// cele s AI btw, ale vypada to cool

import { Pie, Line } from 'vue-chartjs'
import {
  Chart as ChartJS,
  Title,
  Tooltip,
  Legend,
  ArcElement,
  CategoryScale,
  LinearScale,
  PointElement,
  LineElement
} from 'chart.js'

// Registrácia modulov pre grafy
ChartJS.register(Title, Tooltip, Legend, ArcElement, CategoryScale, LinearScale, PointElement, LineElement)

// Reaktívna premenná pre dáta z backendu
const stats = ref({
  recentUsers: [],
  popularFacilityTypes:[],
  reservationsLastWeek:[]
})

// Načítanie dát
stats.value = await secureFetch("/Other/Dashboard").then(it => it.json())

// Nastavenie dát pre Koláčový graf (Pie Chart)
const pieChartData = computed(() => {
  return {
    // Názvy typov športovísk (napr.["Tenis", "Bedminton"])
    labels: stats.value.popularFacilityTypes.map((item: any) => item.typeName),
    datasets: [
      {
        backgroundColor:['#41B883', '#E46651', '#00D8FF', '#DD1B16', '#8A2BE2'],
        // Počty rezervácií
        data: stats.value.popularFacilityTypes.map((item: any) => item.count)
      }
    ]
  }
})

// Nastavenie dát pre Krivkový graf (Line Chart)
const lineChartData = computed(() => {
  return {
    // Dátumy (napr. ["18.03.", "19.03.", ...])
    labels: stats.value.reservationsLastWeek.map((item: any) => item.date),
    datasets:[
      {
        label: 'Počet rezervací',
        backgroundColor: '#1976D2', // Modrá Vuetify farba
        borderColor: '#1976D2',
        data: stats.value.reservationsLastWeek.map((item: any) => item.count),
        tension: 0.3 // Mierne zaoblenie krivky
      }
    ]
  }
})

// Nastavenia grafov, aby boli responzívne
const chartOptions = {
  responsive: true,
  maintainAspectRatio: false
}
</script>

<template>
  <div>
    <h1 class="text-h4 mb-6">Dashboard</h1>

    <!-- Prvý riadok: Grafy -->
    <v-row>
      <!-- Krivka (Rezervácie za posledný týždeň) -->
      <v-col cols="12" md="8">
        <v-card class="fill-height" title="Rezervace (posledních 7 dní)">
          <v-card-text style="height: 300px;">
            <Line :data="lineChartData" :options="chartOptions" />
          </v-card-text>
        </v-card>
      </v-col>

      <!-- Koláčový graf (Najpopulárnejšie športoviská) -->
      <v-col cols="12" md="4">
        <v-card class="fill-height" title="Populárne typy športovišť">
          <v-card-text style="height: 300px;">
            <Pie :data="pieChartData" :options="chartOptions" />
          </v-card-text>
        </v-card>
      </v-col>
    </v-row>

    <!-- Druhý riadok: Tabuľka -->
    <v-row class="mt-4">
      <v-col cols="12">
        <v-card title="Poslední registrovaní uživatelé">
          <v-table>
            <thead>
              <tr>
                <th>Jméno</th>
                <th>Email</th>
                <th>Datum registrace</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="user in stats.recentUsers" :key="user.email">
                <td>{{ user.fullName }}</td>
                <td>{{ user.email }}</td>
                <td>{{ new Date(user.createdAt).toLocaleDateString('cs-CZ') }}</td>
              </tr>
              <tr v-if="stats.recentUsers.length === 0">
                <td colspan="3" class="text-center text-grey">Žádní noví uživatelé</td>
              </tr>
            </tbody>
          </v-table>
        </v-card>
      </v-col>
    </v-row>
  </div>
</template>