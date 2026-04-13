<script setup lang="ts">
import {secureFetch} from "@/auth.ts";
import {onMounted, ref, computed, watch} from "vue";
import {Form, required} from "@/form.ts";
import {useRoute, useRouter} from "vue-router";

const reservations = ref([])
const users = ref([])
const facilities = ref([])
const loading = ref(false)

const route = useRoute()
const router = useRouter()

onMounted(async () => {
  await loadData()
})

async function loadData() {
  loading.value = true
  try {
    await Promise.all([
      loadUsers(),
      loadFacilities(),
      loadReservations()
    ])
  } finally {
    loading.value = false
  }
}

//omezení
async function loadUsers() {
  const usersData = await secureFetch("/User").then(it => it.json())
  users.value = usersData.map((user: any) => ({
    id: user.id,
    fullName: user.fullName,
    email: user.email
  }))
}

async function loadFacilities() {
  try {
    const facilitiesData = await secureFetch("/Facility?page_size=1000&page=1").then(it => it.json()).then(it => it.items)
    facilities.value = facilitiesData.map((facility: any) => ({
      id: facility.id,
      name: facility.name
    }))
  } catch (error) {
    console.warn("Could not load facilities")
    facilities.value = []
  }
}

async function loadReservations() {
  // Sestavení query parametrů
  const params = new URLSearchParams()
  
  if (statusFilter.value && statusFilter.value !== 'all') {
    params.append('status', statusFilter.value)
  }
  
  if (userFilter.value) {
    params.append('user_id', userFilter.value)
  }
  
  if (facilityFilter.value) {
    params.append('facility_id', facilityFilter.value)
  }
  
  const queryString = params.toString()
  const url = queryString ? `/Reservation?${queryString}` : '/Reservation'
  
  const reservationsData = await secureFetch(url).then(it => it.json())
  
  reservations.value = await Promise.all(
    reservationsData.map(async (reservation: any) => {
      const user = users.value.find((u: any) => u.id === reservation.userId)
      const facility = facilities.value.find((f: any) => f.id === reservation.facilityId)
      
      return {
        ...reservation,
        userName: user?.fullName || 'Neznámý uživatel',
        userEmail: user?.email || '',
        facilityName: facility?.name || 'Neznámé sportoviště',
        startAt: new Date(reservation.startAt).toLocaleString('cs-CZ'),
        endAt: new Date(reservation.endAt).toLocaleString('cs-CZ'),
        createdAt: new Date(reservation.createdAt).toLocaleString('cs-CZ'),
        cancelledAt: reservation.cancelledAt ? new Date(reservation.cancelledAt).toLocaleString('cs-CZ') : undefined
      }
    })
  )
  
  // Seřadit podle data vytvoření (nejnovější první)
  reservations.value.sort((a: any, b: any) => new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime())
}

class CancelForm extends Form {

  reservation = {} as any

  onClear(): void {
    this.reservation = {} as any
  }

  async onOpen(data: any): Promise<void> {
    this.reservation = data
  }

  async onReload(): Promise<void> {
    await loadReservations()
  }

  async onPost(): Promise<boolean> {
    const result = await secureFetch("/Reservation/" + this.reservation.id, {
      method: "DELETE"
    })
    if (result.ok) {
      return true
    }
    switch (await result.text()) {
      case "not-found":
        this.fail("Rezervace nebyla nalezena")
        break
      case "already-cancelled":
        this.fail("Rezervace je již zrušena")
        break
      case "cannot-cancel":
        this.fail("Tuto rezervaci nelze zrušit")
        break
      default:
        this.fail("Neznáma chyba")
        break
    }
    return false
  }
}

const cancelForm = ref(new CancelForm())

// Filtry
const statusFilter = ref(route.query.status as string || 'all')
const userFilter = ref(route.query.userSearch as string || '')
const facilityFilter = ref(route.query.facilitySearch as string || '')

// Sledování změn filtrů a aktualizace URL
watch([statusFilter, userFilter, facilityFilter], () => {
  const query: any = {}
  
  if (statusFilter.value && statusFilter.value !== 'all') {
    query.status = statusFilter.value
  }
  
  if (userFilter.value) {
    query.userSearch = userFilter.value
  }
  
  if (facilityFilter.value) {
    query.facilitySearch = facilityFilter.value
  }
  
  router.replace({ query })
  loadReservations()
})

const filteredReservations = computed(() => {
  // Filtry jsou nyní řešeny na backendu přes query parametry
  return reservations.value
})

</script>

<template>
  <v-dialog v-model="cancelForm.opened" max-width="450">

    <v-dialog v-model="cancelForm.error" max-width="300">
      <v-card title="Chyba" v-bind:text="cancelForm.errorMessage"/>
    </v-dialog>

    <v-card title="Zrušit rezervaci?" v-bind:text="'Opravdu chceš zrušit rezervaci uživatele ' + cancelForm.reservation.userName + ' na ' + cancelForm.reservation.facilityName + '?'">
      <template v-slot:actions>
        <v-btn append-icon="mdi-close" variant="tonal" text="Zavřít" @click="cancelForm.close()"/>
        <v-btn append-icon="mdi-cancel" variant="tonal" text="Zrušit" color="error" @click="cancelForm.post()"/>
      </template>
    </v-card>
  </v-dialog>

  <div class="d-flex justify-space-between align-center mb-4">
    <h1 class="text-h4">Rezervace</h1>
    <v-btn @click="loadData" :loading="loading" prepend-icon="mdi-refresh">
      Obnovit
    </v-btn>
  </div>

  <!-- Filtry -->
  <v-card class="mb-4">
    <v-card-title>Filtry</v-card-title>
    <v-card-text>
      <v-row>
        <v-col cols="12" md="3">
          <v-select
              label="Status"
              v-model="statusFilter"
              :items="[
                { title: 'Všechny', value: 'all' },
                { title: 'Aktivní', value: 'active' },
                { title: 'Zrušené', value: 'cancelled' }
              ]"
          />
        </v-col>
        <v-col cols="12" md="4">
          <v-autocomplete
            label="Uživatel"
            v-model="userFilter"
            :items="users"
            item-title="fullName"
            item-value="id"
            clearable
            prepend-inner-icon="mdi-account-search"
          />
        </v-col>
        <v-col cols="12" md="4">
          <v-autocomplete
            label="Sportoviště"
            v-model="facilityFilter"
            :items="facilities"
            item-title="name"
            item-value="id"
            clearable
            prepend-inner-icon="mdi-map-marker"
          />
        </v-col>
        <v-col cols="12" md="1" class="d-flex align-center">
          <v-btn @click="statusFilter = 'all'; userFilter = ''; facilityFilter = ''" variant="outlined" size="small">
            Vymazat
          </v-btn>
        </v-col>
      </v-row>
    </v-card-text>
  </v-card>

  <v-card>
    <v-table>
      <thead>
      <tr>
        <th>Uživatel</th>
        <th>Sportoviště</th>
        <th>Začátek</th>
        <th>Konec</th>
        <th>Cena</th>
        <th>Status</th>
        <th>Vytvořeno</th>
        <th class="text-right">Akce</th>
      </tr>
      </thead>
      <tbody>
      <tr v-for="reservation in filteredReservations">
        <td>
          <div>
            <div class="font-weight-medium">{{ reservation.userName }}</div>
            <div class="text-caption text-grey-darken-1">{{ reservation.userEmail }}</div>
          </div>
        </td>
        <td>{{ reservation.facilityName }}</td>
        <td>{{ reservation.startAt }}</td>
        <td>{{ reservation.endAt }}</td>
        <td>
          <div>
            <div v-if="reservation.discountPercent > 0" class="text-decoration-line-through text-grey">
              {{ reservation.basePrice }} Kč
            </div>
            <div class="font-weight-medium">
              {{ reservation.finalPrice }} Kč
              <v-chip v-if="reservation.discountPercent > 0" size="x-small" color="success">
                -{{ reservation.discountPercent }}%
              </v-chip>
            </div>
          </div>
        </td>
        <td>
          <v-chip
              :color="reservation.status === 'Active' ? 'success' : 'error'"
              size="small"
          >
            {{ reservation.status === 'Active' ? 'Aktivní' : 'Zrušená' }}
          </v-chip>
        </td>
        <td>{{ reservation.createdAt }}</td>
        <td class="text-right">
          <v-btn 
            icon="mdi-cancel" 
            size="small"
            variant="text"
            color="error"
            @click="cancelForm.open(reservation)"
            :disabled="reservation.status === 'Cancelled'"
            title="Zrušit rezervaci"
          />
        </td>
      </tr>
      </tbody>
    </v-table>
    
    <v-card-text v-if="filteredReservations.length === 0 && !loading" class="text-center py-8">
      <v-icon size="64" color="grey-lighten-1" class="mb-4">mdi-calendar-blank</v-icon>
      <div class="text-h6 mb-2">Žádné rezervace</div>
      <div class="text-body-2 text-grey-darken-1">
        {{ reservations.length === 0 ? 'Zatím nebyly vytvořeny žádné rezervace' : 'Žádné rezervace neodpovídají filtrům' }}
      </div>
    </v-card-text>
    
    <v-card-text v-if="loading" class="text-center py-8">
      <v-progress-circular indeterminate size="64" color="primary" class="mb-4" />
      <div class="text-h6">Načítám rezervace...</div>
    </v-card-text>
  </v-card>
</template>

<style scoped>

</style>
