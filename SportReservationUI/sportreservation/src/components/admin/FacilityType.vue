<script setup lang="ts">
import {ref, onMounted} from 'vue'
import {secureFetch} from "@/auth.ts";

interface FacilityType {
  id: string
  name: string
  description: string
}

const facilityTypes = ref<FacilityType[]>([])
const dialog = ref(false)
const editMode = ref(false)
const currentFacilityType = ref<FacilityType>({
  id: '',
  name: '',
  description: ''
})


const loadFacilityTypes = async () => {
  facilityTypes.value = await secureFetch("/Facility/Type").then(it => it.json())
}

const saveFacilityType = async () => {
  if (editMode.value) {
    await secureFetch('/Facility/Type', {
      method: 'PATCH',
      body: JSON.stringify({
        id: currentFacilityType.value.id,
        name: currentFacilityType.value.name,
        description: currentFacilityType.value.description
      })
    })
  } else {
    await secureFetch('/Facility/Type', {
      method: 'POST',
      body: JSON.stringify({
        name: currentFacilityType.value.name,
        description: currentFacilityType.value.description
      })
    })
  }
  dialog.value = false
  await loadFacilityTypes()
}

const deleteFacilityType = async (id: string) => {
  if (confirm('Opravdu chcete smazat tento typ sportoviště?')) {
    await secureFetch(`/Facility/Type/${id}`, {
      method: 'DELETE'
    })
    await loadFacilityTypes()
  }
}

const editFacilityType = (facilityType: FacilityType) => {
  currentFacilityType.value = {...facilityType}
  editMode.value = true
  dialog.value = true
}

const openAddDialog = () => {
  currentFacilityType.value = {
    id: '',
    name: '',
    description: ''
  }
  editMode.value = false
  dialog.value = true
}

onMounted(() => {
  loadFacilityTypes()
})
</script>

<template>
  <div class="d-flex justify-space-between align-center mb-4">
    <h1 class="text-h4">Typy sportovišť</h1>
    <v-btn icon="mdi-plus" @click="openAddDialog"/>
  </div>

  <v-card class="w-100">
    <v-table class="w-100">
      <thead>
      <tr>
        <th>Název</th>
        <th>Popis</th>
        <th>Akce</th>
      </tr>
      </thead>
      <tbody>
      <tr v-for="facilityType in facilityTypes" :key="facilityType.id">
        <td>{{ facilityType.name }}</td>
        <td>{{ facilityType.description }}</td>
        <td>
          <div class="d-flex gap-2">
            <v-btn
              icon="mdi-pencil"
              size="small"
              variant="text"
              @click="editFacilityType(facilityType)"
            />
            <v-btn
              icon="mdi-trash-can"
              size="small"
              variant="text"
              color="error"
              @click="deleteFacilityType(facilityType.id)"
            />
          </div>
        </td>
      </tr>
      </tbody>
    </v-table>
  </v-card>

  <v-dialog v-model="dialog" max-width="500px">
    <v-card>
      <v-card-title>
        <span class="text-h5">{{ editMode ? 'Upravit typ' : 'Přidat typ' }}</span>
      </v-card-title>
      
      <v-card-text>
        <v-form>
          <v-text-field
            v-model="currentFacilityType.name"
            label="Název"
            required
          />
          <v-textarea
            v-model="currentFacilityType.description"
            label="Popis"
            rows="3"
          />
        </v-form>
      </v-card-text>

      <v-card-actions>
        <v-spacer></v-spacer>
        <v-btn color="gray" variant="text" @click="dialog = false">
          Zrušit
        </v-btn>
        <v-btn color="primary" variant="text" @click="saveFacilityType">
          Uložit
        </v-btn>
      </v-card-actions>
    </v-card>
  </v-dialog>
</template>

<style scoped>
.gap-2 {
  gap: 8px;
}
</style>
