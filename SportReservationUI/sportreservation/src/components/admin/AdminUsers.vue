<script setup lang="ts">
import type {User} from "@/interfaces.ts";
import {secureFetch} from "@/auth.ts";

const users: User[] = await secureFetch("/User").then(it => it.json())

</script>

<template>
  <div class="d-flex justify-space-between align-center mb-4">
    <h1 class="text-h4">Uživatelé</h1>
    <v-btn icon="mdi-plus"/>
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