<script setup lang="ts">
import {secureFetch} from "@/auth.ts";
import {onMounted, ref} from "vue";
import {Form, isNotNullOrEmpty, required} from "@/form.ts";

interface FacilityType {
  id: string
  name: string
  description: string
}

const facilityTypes = ref<FacilityType[]>([])

onMounted(async () => {
  await reloadFacilityTypes()
})

async function reloadFacilityTypes() {
  facilityTypes.value = await secureFetch("/Facility/Type").then(it => it.json())
}

class AddForm extends Form {
  name = ""
  description = ""

  onClear(): void {
    this.name = ""
    this.description = ""
  }

  async onReload(): Promise<void> {
    await reloadFacilityTypes()
  }

  async onPost(): Promise<boolean> {
    const result = await secureFetch("/Facility/Type", {
      method: "POST",
      body: JSON.stringify({
        name: this.name,
        description: this.description
      })
    })
    if (result.ok) {
      return true
    }
    switch (await result.text()) {
      case "name-empty":
        this.fail("Název nemůže být prázdný")
        break
      case "already-exists":
        this.fail("Tento typ sportoviště již existuje")
        break
      default:
        this.fail("Neznáma chyba")
        break
    }
    return false
  }
}

class DelForm extends Form {

  facilityType: FacilityType = {} as FacilityType

  onClear(): void {
    this.facilityType = {} as FacilityType
  }

  onOpen(data: FacilityType) {
    this.facilityType = data
  }

  async onReload(): Promise<void> {
    await reloadFacilityTypes()
  }

  async onPost(): Promise<boolean> {
    const result = await secureFetch("/Facility/Type/" + this.facilityType.id, {
      method: "DELETE"
    })
    if (result.ok) {
      return true
    }
    switch (await result.text()) {
      case "cant-delete":
        this.fail("Tento typ sportoviště nelze smazat.")
        break
      default:
        this.fail("Neznáma chyba")
        break
    }
    return false
  }
}

class EditForm extends Form {

  facilityType: FacilityType = {} as FacilityType

  onClear(): void {
    this.facilityType = {} as FacilityType
  }

  onOpen(data: FacilityType) {
    this.facilityType = data
  }

  async onReload(): Promise<void> {
    await reloadFacilityTypes()
  }

  async onPost(): Promise<boolean> {
    const result = await secureFetch("/Facility/Type", {
      method: "PATCH",
      body: JSON.stringify({
        id: this.facilityType.id,
        name: this.facilityType.name,
        description: this.facilityType.description
      })
    })
    if (result.ok) {
      return true
    }
    switch (await result.text()) {
      case "name-empty":
        this.fail("Název nemůže být prázdný")
        break
      case "already-exists":
        this.fail("Tento typ sportoviště již existuje")
        break
      default:
        this.fail("Neznáma chyba")
        break
    }
    return false
  }
}

const addForm = ref(new AddForm())
const delForm = ref(new DelForm())
const editForm = ref(new EditForm())

</script>

<template>
  <v-dialog v-model="addForm.opened" max-width="450">

    <v-dialog v-model="addForm.error" max-width="300">
      <v-card title="Chyba" v-bind:text="addForm.errorMessage"/>
    </v-dialog>

    <v-card title="Nový typ sportoviště">
      <v-form v-model="addForm.valid">
        <v-container>
          <v-row>
            <v-col>
              <v-text-field
                  label="Název"
                  :rules="[required]"
                  v-model="addForm.name"
              />
            </v-col>
          </v-row>
          <v-row>
            <v-col>
              <v-textarea
                  label="Popis"
                  rows="3"
                  v-model="addForm.description"
              />
            </v-col>
          </v-row>
        </v-container>
      </v-form>
      <template v-slot:actions>
        <v-btn
            :disabled="!addForm.valid"
            :loading="addForm.loading"
            append-icon="mdi-plus"
            variant="tonal"
            text="Vytvořit"
            @click="addForm.post()"
        />
        <v-btn append-icon="mdi-close" variant="tonal" text="Zavřít" @click="addForm.close()"/>
      </template>
    </v-card>
  </v-dialog>


  <v-dialog v-model="editForm.opened" max-width="450">

    <v-dialog v-model="editForm.error" max-width="300">
      <v-card title="Chyba" v-bind:text="editForm.errorMessage"/>
    </v-dialog>

    <v-card title="Úprava typu sportoviště">
      <v-form v-model="editForm.valid">
        <v-container>
          <v-row>
            <v-col>
              <v-text-field
                  label="Název"
                  :rules="[required]"
                  v-model="editForm.facilityType.name"
              />
            </v-col>
          </v-row>
          <v-row>
            <v-col>
              <v-textarea
                  label="Popis"
                  rows="3"
                  v-model="editForm.facilityType.description"
              />
            </v-col>
          </v-row>
        </v-container>
      </v-form>
      <template v-slot:actions>
        <v-btn
            :disabled="!editForm.valid"
            :loading="editForm.loading"
            append-icon="mdi-content-save"
            variant="tonal"
            text="Uložit"
            @click="editForm.post()"
        />
        <v-btn append-icon="mdi-close" variant="tonal" text="Zavřít" @click="editForm.close()"/>
      </template>
    </v-card>
  </v-dialog>


  <v-dialog v-model="delForm.opened" max-width="450">

    <v-dialog v-model="delForm.error" max-width="300">
      <v-card title="Chyba" v-bind:text="delForm.errorMessage"/>
    </v-dialog>

    <v-card title="Smazat typ sportoviště?" v-bind:text="'Opravdu chceš smazat typ sportoviště ' + delForm.facilityType.name">
      <template v-slot:actions>
        <v-btn append-icon="mdi-close" variant="tonal" text="Zavřít" @click="delForm.close()"/>
        <v-btn append-icon="mdi-trash-can" variant="tonal" text="Smazat" @click="delForm.post()"/>
      </template>
    </v-card>
  </v-dialog>


  <div class="d-flex justify-space-between align-center mb-4">
    <h1 class="text-h4">Typy sportovišť</h1>
    <v-btn icon="mdi-plus" @click="addForm.open()"/>
  </div>

  <v-card>
    <v-table>
      <thead>
      <tr>
        <th>Název</th>
        <th>Popis</th>
        <th class="text-right">Akce</th>
      </tr>
      </thead>
      <tbody>
      <tr v-for="facilityType in facilityTypes">
        <td>{{ facilityType.name }}</td>
        <td>{{ facilityType.description }}</td>
        <td class="text-right">
          <v-btn icon="mdi-pencil" @click="editForm.open(facilityType)"/>
          <v-btn icon="mdi-trash-can" @click="delForm.open(facilityType)"/>
        </td>
      </tr>
      </tbody>
    </v-table>
  </v-card>
</template>

<style scoped>
.gap-2 {
  gap: 8px;
}
</style>
