<script setup lang="ts">
import {secureFetch} from "@/auth.ts";
import {onMounted, ref} from "vue";
import {Form, isNotNullOrEmpty, required} from "@/form.ts";

const users = ref([])

onMounted(async () => {
  await reloadUsers()
})

async function reloadUsers() {
  users.value = await secureFetch("/User").then(it => it.json())
}

class AddForm extends Form {
  email = ""
  fullName = ""
  password = ""

  onClear(): void {
    this.email = ""
    this.fullName = ""
    this.password = ""
  }

  async onReload(): Promise<void> {
    await reloadUsers()
  }

  async onPost(): Promise<boolean> {
    const result = await secureFetch("/User", {
      method: "POST",
      body: JSON.stringify({
        email: this.email,
        fullName: this.fullName,
        password: this.password
      })
    })
    if (result.ok) {
      return true
    }
    switch (await result.text()) {
      case "email-invalid":
        this.fail("Neplatný email")
        break
      case "already-exists":
        this.fail("Tento email již je zaregistrovaný")
        break
      default:
        this.fail("Neznáma chyba")
        break
    }
    return false
  }
}

class DelForm extends Form {

  user = {}

  onClear(): void {
    this.user = {}
  }

  async onOpen(data: any): Promise<void> {
    this.user = data
  }

  async onReload(): Promise<void> {
    await reloadUsers()
  }

  async onPost(): Promise<boolean> {
    const result = await secureFetch("/User/" + this.user.id, {
      method: "DELETE"
    })
    if (result.ok) {
      return true
    }
    switch (await result.text()) {
      case "cant-delete":
        this.fail("Tohoto uživatele nelze smazat.")
        break
      default:
        this.fail("Neznáma chyba")
        break
    }
    return false
  }
}

class EditForm extends Form {

  user = {}

  onClear(): void {
    this.user = {}
  }

  async onOpen(data: any): Promise<void> {
    this.user = data
  }

  async onReload(): Promise<void> {
    await reloadUsers()
  }

  async onPost(): Promise<boolean> {
    const body = {
      id: this.user.id,
      email: this.user.email,
      fullName: this.user.fullName
    }
    if (isNotNullOrEmpty(this.user.password)) {
      body.password = {
        new: this.user.password
      }
    }
    const result = await secureFetch("/User", {
      method: "PATCH",
      body: JSON.stringify(body)
    })
    if (result.ok) {
      return true
    }
    switch (await result.text()) {
      case "email-already-exists":
        this.fail("Tento email již někdo má nastavený.")
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

    <v-card title="Nový uživatel">
      <v-form v-model="addForm.valid">
        <v-container>
          <v-row>
            <v-col>
              <v-text-field
                  label="Email"
                  :rules="[required]"
                  v-model="addForm.email"
              />
            </v-col>
            <v-col>
              <v-text-field
                  label="Jméno"
                  :rules="[required]"
                  v-model="addForm.fullName"
              />
            </v-col>
          </v-row>
          <v-row>
            <v-col>
              <v-text-field
                  label="Heslo"
                  type="password"
                  :rules="[required]"
                  v-model="addForm.password"
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

    <v-card title="Smazat uživatele?" v-bind:text="'Opravdu chceš smazat uživatele ' + delForm.user.fullName">
      <template v-slot:actions>
        <v-btn append-icon="mdi-close" variant="tonal" text="Zavřít" @click="delForm.close()"/>
        <v-btn append-icon="mdi-trash-can" variant="tonal" text="Smazat" @click="delForm.post()"/>
      </template>
    </v-card>
  </v-dialog>


  <div class="d-flex justify-space-between align-center mb-4">
    <h1 class="text-h4">Uživatelé</h1>
    <v-btn icon="mdi-plus" @click="addForm.open()"/>
  </div>

  <v-card>
    <v-table>
      <thead>
      <tr>
        <th>Jméno</th>
        <th>Email</th>
        <th>Admin</th>
        <th class="text-right">Akce</th>
      </tr>
      </thead>
      <tbody>
      <tr v-for="user in users">
        <td>{{ user.fullName }}</td>
        <td>{{ user.email }}</td>
        <td>{{ user.role == 'Admin' }}</td>
        <td class="text-right">
          <v-btn icon="mdi-pencil" @click="editForm.open(user)"/>
          <v-btn icon="mdi-trash-can" @click="delForm.open(user)"/>
        </td>
      </tr>
      </tbody>
    </v-table>
  </v-card>
</template>

<style scoped>

</style>