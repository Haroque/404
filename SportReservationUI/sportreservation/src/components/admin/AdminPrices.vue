<script setup lang="ts">
import {secureFetch} from "@/auth.ts";
import {onMounted, ref} from "vue";
import {Form, required} from "@/form.ts";

const prices = ref([])
const facilityTypes = ref([])

onMounted(async () => {
  await loadFacilityTypes()
  await loadPrices()
})

async function loadFacilityTypes() {
  facilityTypes.value = await secureFetch("/Facility/Type").then(it => it.json())
}

async function loadPrices() {
  const allPrices = []
  
  // Načteme ceny pro každý typ sportoviště
  for (const facilityType of facilityTypes.value) {
    try {
      const typePrices = await secureFetch(`/PriceList/${facilityType.id}`).then(it => it.json())
      const pricesWithType = typePrices.map((price) => ({
        ...price,
        facilityTypeName: facilityType.name,
        validFrom: new Date(price.validFrom).toLocaleString('cs-CZ'),
        validTo: price.validTo ? new Date(price.validTo).toLocaleString('cs-CZ') : null
      }))
      allPrices.push(...pricesWithType)
    } catch (error) {
      console.warn(`No prices found for facility type: ${facilityType.name}`)
    }
  }
  
  prices.value = allPrices.sort((a, b) => new Date(a.validFrom).getTime() - new Date(b.validFrom).getTime())
}

class AddForm extends Form {
  facilityTypeId = ""
  validFrom = ""
  validTo = ""
  pricePerHour = 0

  onClear(): void {
    this.facilityTypeId = ""
    this.validFrom = ""
    this.validTo = ""
    this.pricePerHour = 0
  }

  async onReload(): Promise<void> {
    await loadPrices()
  }

  async onPost(): Promise<boolean> {
    // Vytvoříme DateTime v lokálním čase bez UTC konverze
    const validFrom = new Date(this.validFrom);
    const validTo = this.validTo ? new Date(this.validTo) : null;
    
    const result = await secureFetch("/PriceList", {
      method: "POST",
      body: JSON.stringify({
        facilityTypeId: this.facilityTypeId,
        validFrom: validFrom.toISOString(),
        validTo: validTo ? validTo.toISOString() : null,
        pricePerHour: this.pricePerHour
      })
    })
    if (result.ok) {
      return true
    }
    switch (await result.text()) {
      case "facility-type-not-found":
        this.fail("Typ sportoviště nebyl nalezen")
        break
      case "invalid-date-range":
        this.fail("Datum 'do' musí být později než datum 'od'")
        break
      case "pricing-overlap":
        this.fail("Cena se překrývá s existující cenou pro toto období")
        break
      case "price-must-be-positive":
        this.fail("Cena musí být kladná")
        break
      default:
        this.fail("Neznáma chyba")
        break
    }
    return false
  }
}

class DelForm extends Form {

  price = {} as any

  onClear(): void {
    this.price = {} as any
  }

  onOpen(data: any) {
    this.price = data
  }

  async onReload(): Promise<void> {
    await loadPrices()
  }

  async onPost(): Promise<boolean> {
    const result = await secureFetch("/PriceList/" + this.price.id, {
      method: "DELETE"
    })
    if (result.ok) {
      return true
    }
    switch (await result.text()) {
      case "cant-delete":
        this.fail("Tuto cenu nelze smazat.")
        break
      default:
        this.fail("Neznáma chyba")
        break
    }
    return false
  }
}

class EditForm extends Form {

  price = {} as any

  onClear(): void {
    this.price = {} as any
  }

  onOpen(data: any) {
    this.price = data
    this.validFrom = new Date(data.validFrom).toISOString().slice(0, 16)
    this.validTo = data.validTo ? new Date(data.validTo).toISOString().slice(0, 16) : ""
  }

  async onReload(): Promise<void> {
    await loadPrices()
  }

  async onPost(): Promise<boolean> {
    // Vytvoříme DateTime v lokálním čase bez UTC konverze
    const validFrom = this.validFrom ? new Date(this.validFrom) : null;
    const validTo = this.validTo ? new Date(this.validTo) : null;
    
    const result = await secureFetch("/PriceList/" + this.price.id, {
      method: "PATCH",
      body: JSON.stringify({
        validFrom: validFrom ? validFrom.toISOString() : null,
        validTo: validTo ? validTo.toISOString() : null,
        pricePerHour: this.pricePerHour
      })
    })
    if (result.ok) {
      return true
    }
    switch (await result.text()) {
      case "invalid-date-range":
        this.fail("Datum 'do' musí být později než datum 'od'")
        break
      case "pricing-overlap":
        this.fail("Cena se překrývá s existující cenou pro toto období")
        break
      case "price-must-be-positive":
        this.fail("Cena musí být kladná")
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
  <v-dialog v-model="addForm.opened" max-width="600">

    <v-dialog v-model="addForm.error" max-width="300">
      <v-card title="Chyba" v-bind:text="addForm.errorMessage"/>
    </v-dialog>

    <v-card title="Nová cena">
      <v-form v-model="addForm.valid">
        <v-container>
          <v-row>
            <v-col>
              <v-select
                  label="Typ sportoviště"
                  :rules="[required]"
                  v-model="addForm.facilityTypeId"
                  :items="facilityTypes"
                  item-title="name"
                  item-value="id"
              />
            </v-col>
            <v-col>
              <v-text-field
                  label="Cena za hodinu (Kč)"
                  type="number"
                  :rules="[required]"
                  v-model="addForm.pricePerHour"
                  min="0"
                  step="1"
              />
            </v-col>
          </v-row>
          <v-row>
            <v-col>
              <v-text-field
                  label="Platnost od"
                  type="datetime-local"
                  :rules="[required]"
                  v-model="addForm.validFrom"
              />
            </v-col>
            <v-col>
              <v-text-field
                  label="Platnost do"
                  type="datetime-local"
                  v-model="addForm.validTo"
                  hint="Nechte prázdné pro neomezenou platnost"
                  persistent-hint
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


  <v-dialog v-model="editForm.opened" max-width="600">

    <v-dialog v-model="editForm.error" max-width="300">
      <v-card title="Chyba" v-bind:text="editForm.errorMessage"/>
    </v-dialog>

    <v-card title="Úprava ceny">
      <v-form v-model="editForm.valid">
        <v-container>
          <v-row>
            <v-col>
              <v-text-field
                  label="Typ sportoviště"
                  v-model="editForm.price.facilityTypeName"
                  readonly
                  disabled
              />
            </v-col>
            <v-col>
              <v-text-field
                  label="Cena za hodinu (Kč)"
                  type="number"
                  :rules="[required]"
                  v-model="editForm.pricePerHour"
                  min="0"
                  step="1"
              />
            </v-col>
          </v-row>
          <v-row>
            <v-col>
              <v-text-field
                  label="Platnost od"
                  type="datetime-local"
                  :rules="[required]"
                  v-model="editForm.validFrom"
              />
            </v-col>
            <v-col>
              <v-text-field
                  label="Platnost do"
                  type="datetime-local"
                  v-model="editForm.validTo"
                  hint="Nechte prázdné pro neomezenou platnost"
                  persistent-hint
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

    <v-card title="Smazat cenu?" v-bind:text="'Opravdu chceš smazat cenu ' + delForm.price.pricePerHour + ' Kč pro ' + delForm.price.facilityTypeName + '?'">
      <template v-slot:actions>
        <v-btn append-icon="mdi-close" variant="tonal" text="Zavřít" @click="delForm.close()"/>
        <v-btn append-icon="mdi-trash-can" variant="tonal" text="Smazat" @click="delForm.post()"/>
      </template>
    </v-card>
  </v-dialog>


  <div class="d-flex justify-space-between align-center mb-4">
    <h1 class="text-h4">Cenník</h1>
    <v-btn icon="mdi-plus" @click="addForm.open()"/>
  </div>

  <v-card>
    <v-table>
      <thead>
      <tr>
        <th>Typ sportoviště</th>
        <th>Cena za hodinu</th>
        <th>Platnost od</th>
        <th>Platnost do</th>
        <th class="text-right">Akce</th>
      </tr>
      </thead>
      <tbody>
      <tr v-for="price in prices">
        <td>{{ price.facilityTypeName }}</td>
        <td>{{ price.pricePerHour }} Kč</td>
        <td>{{ price.validFrom }}</td>
        <td>{{ price.validTo || 'Neomezeně' }}</td>
        <td class="text-right">
          <v-btn icon="mdi-pencil" @click="editForm.open(price)"/>
          <v-btn icon="mdi-trash-can" @click="delForm.open(price)"/>
        </td>
      </tr>
      </tbody>
    </v-table>
    <v-card-text v-if="prices.length === 0" class="text-center py-8">
      <v-icon size="64" color="grey-lighten-1" class="mb-4">mdi-currency-usd-off</v-icon>
      <div class="text-h6 mb-2">Žádné ceny</div>
      <div class="text-body-2 text-grey-darken-1">Zatím nebyly vytvořeny žádné ceny</div>
    </v-card-text>
  </v-card>
</template>

<style scoped>

</style>
