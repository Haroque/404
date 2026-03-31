<script setup lang="ts">
import {secureFetch} from "@/auth.ts";
import {onMounted, ref} from "vue";
import {Form, required} from "@/form.ts";

const facilities = ref([])

onMounted(async () => {
  await reloadFacilities()
})

async function fetchAllFacilities() {
  return await secureFetch("/Facility?page_size=1000&page=1")
      .then(it => it.json())
      .then(it => it.items)
}

async function reloadFacilities() {
  facilities.value = await fetchAllFacilities()
}

class AddForm extends Form {
  name = ""
  type: undefined
  capacity = 0
  active = true
  knownTypes = []

  onClear(): void {
    this.name = ""
    this.type = ""
    this.capacity = 0
    this.active = true
    this.knownTypes = []
  }

  async onOpen(data: any): Promise<void> {
    this.knownTypes = await secureFetch("/Facility/Type").then(res => res.json())
  }

  async onReload(): Promise<void> {
    await reloadFacilities()
  }

  async onPost(): Promise<boolean> {
    const result = await secureFetch("/Facility", {
      method: "POST",
      body: JSON.stringify({
        name: this.name,
        typeId: this.type,
        capacity: this.capacity,
        isActive: this.active
      })
    })
    if (result.ok) {
      return true
    }
    switch (await result.text()) {
      default:
        this.fail("Neznáma chyba")
        break
    }
    return false
  }
}

class DelForm extends Form {

  facility = {}

  onClear(): void {
    this.facility = {}
  }

  async onOpen(data: any): Promise<void> {
    this.facility = data
  }

  async onReload(): Promise<void> {
    await reloadFacilities()
  }

  async onPost(): Promise<boolean> {
    const result = await secureFetch("/Facility/" + this.facility.id, {
      method: "DELETE"
    })
    if (result.ok) {
      return true
    }
    switch (await result.text()) {
      default:
        this.fail("Neznáma chyba")
        break
    }
    return false
  }
}

class EditForm extends Form {

  facility = {}
  type: undefined
  knownTypes = []

  onClear(): void {
    this.facility = {}
    this.type = ""
    this.knownTypes = []
  }

  async onOpen(data: any): Promise<void> {
    this.facility = data
    this.type = data.type.id
    this.knownTypes = await secureFetch("/Facility/Type").then(res => res.json())
  }

  async onReload(): Promise<void> {
    await reloadFacilities()
  }

  async onPost(): Promise<boolean> {
    const result = await secureFetch("/Facility", {
      method: "PATCH",
      body: JSON.stringify({
        id: this.facility.id,
        name: this.facility.name,
        typeId: this.type,
        capacity: this.facility.capacity,
        isActive: this.facility.active
      })
    })
    if (result.ok) {
      return true
    }
    switch (await result.text()) {
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

    <v-card title="Nové sportoviště">
      <v-form v-model="addForm.valid">
        <v-container>
          <v-row>
            <v-col>
              <v-text-field
                  label="Jméno"
                  :rules="[required]"
                  v-model="addForm.name"
              />
            </v-col>
            <v-col>
              <v-number-input
                  label="Kapacita"
                  :rules="[required]"
                  v-model="addForm.capacity"
                  :min="1"
              />
            </v-col>
          </v-row>
          <v-row>
            <v-col>
              <v-autocomplete
                  label="Typ"
                  :rules="[required]"
                  v-model="addForm.type"
                  :items="addForm.knownTypes"
                  item-title="name"
                  item-value="id"
              />
            </v-col>
            <v-col>
              <v-checkbox
                  label="Aktivní"
                  v-model="addForm.active"
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

    <v-card title="Úprava sportoviště">
      <v-form v-model="editForm.valid">
        <v-container>
          <v-row>
            <v-col>
              <v-text-field
                  label="Jméno"
                  :rules="[required]"
                  v-model="editForm.facility.name"
              />
            </v-col>
            <v-col>
              <v-number-input
                  label="Kapacita"
                  :rules="[required]"
                  v-model="editForm.facility.capacity"
                  :min="1"
              />
            </v-col>
          </v-row>
          <v-row>
            <v-col>
              <v-autocomplete
                  label="Type"
                  :rules="[required]"
                  v-model="editForm.type"
                  :items="editForm.knownTypes"
                  item-title="name"
                  item-value="id"
              />
            </v-col>
            <v-col>
              <v-checkbox
                  label="Aktivní"
                  v-model="editForm.facility.isActive"
              />
            </v-col>
          </v-row>
        </v-container>
      </v-form>
      <template v-slot:actions>
        <v-btn
            :disabled="!editForm.valid"
            :loading="editForm.loading"
            append-icon="mdi-plus"
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

    <v-card title="Smazat sportoviště?" v-bind:text="'Opravdu chceš ' + delForm.facility.name">
      <template v-slot:actions>
        <v-btn append-icon="mdi-close" variant="tonal" text="Zavřít" @click="delForm.close()"/>
        <v-btn append-icon="mdi-trash-can" variant="tonal" text="Smazat" @click="delForm.post()"/>
      </template>
    </v-card>
  </v-dialog>


  <div class="d-flex justify-space-between align-center mb-4">
    <h1 class="text-h4">Sportoviště</h1>
    <v-btn icon="mdi-plus" @click="addForm.open()"/>
  </div>

  <v-card>
    <v-table>
      <thead>
      <tr>
        <th>Jméno</th>
        <th>Typ</th>
        <th>Kapacita</th>
        <th>Aktivní</th>
        <th class="text-right">Akce</th>
      </tr>
      </thead>
      <tbody>
      <tr v-for="facility in facilities">
        <td>{{ facility.name }}</td>
        <td>{{ facility.type.name }}</td>
        <td>{{ facility.capacity }}</td>
        <td>{{ facility.isActive }}</td>
        <td class="text-right">
          <v-btn icon="mdi-pencil" @click="editForm.open(facility)"/>
          <v-btn icon="mdi-trash-can" @click="delForm.open(facility)"/>
        </td>
      </tr>
      </tbody>
    </v-table>
  </v-card>
</template>

<style scoped>

</style>