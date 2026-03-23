<script setup lang="ts">
import {onMounted, ref} from 'vue'
import {secureFetch} from "@/auth.ts";

const drawer = ref(true)
const rail = ref(true)

const self = ref({})

onMounted(async () => {
  self.value = await secureFetch("/User/Self").then(it => it.json())
})

</script>

<template>
  <v-card>
    <v-layout>
      <v-navigation-drawer
          v-model="drawer"
          :rail="rail"
          permanent
          @click="rail = false"
      >
        <v-list>
          <v-list-item prepend-icon="mdi-account" :title="self.fullName">
            <template v-slot:append>
              <v-btn
                  icon="mdi-chevron-left"
                  variant="text"
                  @click.stop="rail = !rail"
              />
            </template>
          </v-list-item>
        </v-list>

        <v-divider></v-divider>

        <v-list density="compact" nav>
          <v-list-item
              prepend-icon="mdi-view-dashboard"
              title="Dashboard"
              value="dashboard"
              to="/admin"
              exact
          />
          <v-list-item
              prepend-icon="mdi-account-group"
              title="Uživatele"
              value="users"
              to="/admin/users"
          />
          <v-list-item
              prepend-icon="mdi-home-edit"
              title="Typy sportovišť"
              value="facilities"
              to="/admin/facilities"
          />
          <v-list-item
              prepend-icon="mdi-home"
              title="Sportoviště"
              value="facility-types"
              to="/admin/facility-types"
          />
          <v-list-item
              prepend-icon="mdi-cash"
              title="Cenníky"
              value="price-lists"
              to="/admin/price-lists"
          />
          <v-list-item
              prepend-icon="mdi-home-alert"
              title="Udržby"
              value="downtimes"
              to="/admin/downtimes"
          />
          <v-list-item
              prepend-icon="mdi-texture-box"
              title="Rezervace"
              value="reservations"
              to="/admin/reservations"
          />
        </v-list>
      </v-navigation-drawer>
      <v-main>
        <v-container fluid>
          <Suspense>
            <router-view />
            <template #fallback>
              <h1>Načitávaní...</h1>
            </template>
          </Suspense>
        </v-container>
      </v-main>
    </v-layout>
  </v-card>
</template>