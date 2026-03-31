<script setup lang="ts">
import {secureFetch} from "@/auth.ts";
import {onMounted, ref} from "vue";
import {Form, required} from "@/form.ts";

const downtimes = ref([])
const knownFacilities = ref([])
const facilityId = ref("")

onMounted(async () => {
  knownFacilities.value = await fetchAllFacilities()
  await reloadDowntimes()
})

async function fetchAllFacilities() {
  return await secureFetch("/Facility?page_size=1000&page=1")
      .then(it => it.json())
      .then(it => it.items)
}

async function reloadDowntimes() {
  downtimes.value = await secureFetch("/Downtime/").then(it => it.json())
}

class AddForm extends Form {
  period = []
  reason = ""

  onClear(): void {
    this.period = []
    this.reason = ""
  }

  async onReload(): Promise<void> {
    await reloadDowntimes()
  }

  async onPost(): Promise<boolean> {
    const result = await secureFetch("/Downtime", {
      method: "POST",
      body: JSON.stringify({
        facilityId: facilityId,
        startAt: this.period[0],
        endAt: this.period[this.period.length - 1],
        reason: this.reason
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

  downtime = {}

  onClear(): void {
    this.downtime = {}
  }

  async onOpen(data: any): Promise<void> {
    this.downtime = data
  }

  async onReload(): Promise<void> {
    await reloadDowntimes()
  }

  async onPost(): Promise<boolean> {
    const result = await secureFetch("/Downtime/" + this.downtime.id, {
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

  downtime = {}

  onClear(): void {
    this.downtime = {}
  }

  async onOpen(data: any): Promise<void> {
    this.downtime = data
  }

  async onReload(): Promise<void> {
    await reloadDowntimes()
  }

  async onPost(): Promise<boolean> {
    const result = await secureFetch("/Downtime/" + downtime.id, {
      method: "PATCH",
      body: JSON.stringify({
        facilityId: this.downtime.facilityId,
        startAt: this.downtime.startAt,
        endAt: this.downtime.endAt,
        reason: this.downtime.reason
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

    <v-card title="Nová údržba">
      <v-form v-model="addForm.valid">
        <v-container>
          <v-row>
            <v-col>

            </v-col>
            <v-col>
              <v-date-input
                  label="Doba"
                  :rules="[required]"
                  v-model="addForm.period"
                  multiple="range"
              />
            </v-col>
          </v-row>
          <v-row>
            <v-col>
              <v-text-field
                  label="Důvod"
                  :rules="[required]"
                  v-model="addForm.reason"
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

    <v-card title="Úprava uživatele">
      <v-form v-model="editForm.valid">
        <v-container>
          <v-row>
            <v-col>
              <v-text-field
                  label="Email"
                  v-model="editForm.user.email"
              />
            </v-col>
            <v-col>
              <v-text-field
                  label="Jméno"
                  v-model="editForm.user.fullName"
              />
            </v-col>
          </v-row>
          <v-row>
            <v-col>
              <v-text-field
                  label="Heslo"
                  type="password"
                  v-model="editForm.user.password"
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

    <v-card title="Smazat uživatele?" v-bind:text="'Opravdu chceš smazat túto údržbu?'">
      <template v-slot:actions>
        <v-btn append-icon="mdi-close" variant="tonal" text="Zavřít" @click="delForm.close()"/>
        <v-btn append-icon="mdi-trash-can" variant="tonal" text="Smazat" @click="delForm.post()"/>
      </template>
    </v-card>
  </v-dialog>


  <div class="d-flex justify-space-between align-center mb-4">
    <h1 class="text-h4">Údržby</h1>

    <v-combobox
        label="Sportoviště"
        v-model="facilityId"
        :items="knownFacilities"
        @update:modelValue="reloadDowntimes()"
        item-title="name"
        item-value="id"
        style="margin-left: 60%; margin-right: 2%"
    />
    <v-btn icon="mdi-plus" @click="addForm.open()"/>
  </div>

  <v-card>
    <v-table>
      <thead>
      <tr>
        <th>Od</th>
        <th>Do</th>
        <th>Důvod</th>
        <th class="text-right">Akce</th>
      </tr>
      </thead>
      <tbody>
      <tr v-for="downtime in downtimes">
        <td>{{ downtime.startAt }}</td>
        <td>{{ downtime.endAt }}</td>
        <td>{{ downtime.reason }}</td>
        <td class="text-right">
          <v-btn icon="mdi-pencil" @click="editForm.open(downtime)"/>
          <v-btn icon="mdi-trash-can" @click="delForm.open(downtime)"/>
        </td>
      </tr>
      </tbody>
    </v-table>
  </v-card>
</template>

<style scoped>

</style>