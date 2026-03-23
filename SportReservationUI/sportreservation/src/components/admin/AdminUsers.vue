<script setup lang="ts">
import {secureFetch} from "@/auth.ts";

const users = await secureFetch("/User").then(it => it.json())

</script>

<template>
  <div class="d-flex justify-lg-space-between">
    <h1>Uživatelé</h1>
    <v-btn icon="mdi-plus"/>
  </div>

  <v-card>
    <v-table>
      <thead>
      <tr>
        <th>Jméno</th>
        <th>Email</th>
        <th>Admin</th>
        <th>Akce</th>
      </tr>
      </thead>
      <tbody v-for="user in users">
      <tr>
        <td>{{ user.fullName }}</td>
        <td>{{ user.email }}</td>
        <td>{{ user.role == 'Admin' }}</td>
        <td class="d-flex justify-lg-space-between">
          <v-btn icon="mdi-pencil"/>
          <v-btn icon="mdi-trash-can"/>
        </td>
      </tr>
      </tbody>
    </v-table>
  </v-card>
</template>

<style scoped>

</style>