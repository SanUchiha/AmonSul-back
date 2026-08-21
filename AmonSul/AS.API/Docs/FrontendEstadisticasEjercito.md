# Frontend — Estadísticas de Ejércitos en Torneos

Vista completa en Vue 3 + Vuetify 3 + TypeScript.  
Consume los dos endpoints creados en `EstadisticasController`.

---

## 1. Interfaces TypeScript

**Crear `src/interfaces/Estadisticas.ts`**

```typescript
// ── EP1: Top bandos ───────────────────────────────────────────────────────────

export interface EjercitoStatsDTO {
  ejercito: string;
  bando: string | null;
  totalPartidas: number;
  victorias: number;
  derrotas: number;
  empates: number;
  winRate: number;
  mediaPuntosFavor: number;
  mediaPuntosContra: number;
}

export interface BandoTopDTO {
  mejores: EjercitoStatsDTO[];
  peores: EjercitoStatsDTO[];
}

export interface TopBandosResponseDTO {
  bien: BandoTopDTO;
  oscuridad: BandoTopDTO;
}

// ── EP2: Rating de un ejército ────────────────────────────────────────────────

export interface RatingEjercitoRequestDTO {
  ejercito: string;
  fechaDesde?: string | null;   // formato ISO: "2024-01-15"
  fechaHasta?: string | null;
  ejercitosRivales?: string[];  // mutuamente excluyente con bandoRival
  bandoRival?: string | null;   // "good" | "evil"
}

export interface RatingEjercitoResponseDTO extends EjercitoStatsDTO {
  esFiltrado: boolean;
  descripcionFiltro: string | null;
}
```

---

## 2. Servicio API

**Crear `src/services/EstadisticasService.ts`**

```typescript
import { http } from "./index";
import type {
  RatingEjercitoRequestDTO,
  RatingEjercitoResponseDTO,
  TopBandosResponseDTO,
} from "@/interfaces/Estadisticas";

export const getTopEjercitosPorBando = async (
  top = 3
): Promise<TopBandosResponseDTO> => {
  const response = await http.get<TopBandosResponseDTO>(
    `Estadisticas/ejercito/top-bandos`,
    { params: { top } }
  );
  return response.data;
};

export const getRatingEjercito = async (
  request: RatingEjercitoRequestDTO
): Promise<RatingEjercitoResponseDTO> => {
  const response = await http.post<RatingEjercitoResponseDTO>(
    `Estadisticas/ejercito/rating`,
    request
  );
  return response.data;
};
```

---

## 3. Vista principal

**Crear `src/views/EstadisticasEjercitoView.vue`**

```vue
<template>
  <v-container class="text-center">
    <div v-if="isLoading">
      <LoadingGandalf />
    </div>

    <div v-else>
      <h2 class="text-h5 font-weight-bold mb-4">
        <v-icon color="primary" class="mr-2">mdi-sword-cross</v-icon>
        Estadísticas de Ejércitos en Torneos
      </h2>

      <v-tabs v-model="tab" color="primary" grow class="mb-4">
        <v-tab value="ranking">
          <v-icon start>mdi-trophy</v-icon>
          Ranking por bando
        </v-tab>
        <v-tab value="filtro">
          <v-icon start>mdi-filter-variant</v-icon>
          Consultar ejército
        </v-tab>
      </v-tabs>

      <v-tabs-window v-model="tab">
        <v-tabs-window-item value="ranking">
          <TopBandosCard :data="topBandos" />
        </v-tabs-window-item>

        <v-tabs-window-item value="filtro">
          <FiltroRatingEjercito />
        </v-tabs-window-item>
      </v-tabs-window>
    </div>
  </v-container>
</template>

<script setup lang="ts">
import { onMounted, ref } from "vue";
import { getTopEjercitosPorBando } from "@/services/EstadisticasService";
import type { TopBandosResponseDTO } from "@/interfaces/Estadisticas";
import LoadingGandalf from "@/components/Commons/LoadingGandalf.vue";
import TopBandosCard from "@/components/Estadisticas/TopBandosCard.vue";
import FiltroRatingEjercito from "@/components/Estadisticas/FiltroRatingEjercito.vue";

const tab = ref<string>("ranking");
const isLoading = ref(true);
const topBandos = ref<TopBandosResponseDTO | null>(null);

onMounted(async () => {
  try {
    topBandos.value = await getTopEjercitosPorBando(3);
  } catch (e) {
    console.error("Error al cargar estadísticas", e);
  } finally {
    isLoading.value = false;
  }
});
</script>
```

---

## 4. Componente TopBandosCard

**Crear `src/components/Estadisticas/TopBandosCard.vue`**

```vue
<template>
  <v-row v-if="data">
    <!-- BIEN (good) -->
    <v-col cols="12" md="6">
      <v-card class="pa-3" elevation="2">
        <v-card-title class="d-flex align-center gap-2">
          <v-icon color="blue-darken-2">mdi-shield-star</v-icon>
          Bando del Bien
        </v-card-title>

        <v-divider class="mb-3" />

        <p class="text-subtitle-2 text-green-darken-3 font-weight-bold mb-1">
          <v-icon size="18" color="green-darken-3">mdi-arrow-up-bold</v-icon>
          Top {{ data.bien.mejores.length }} mejores
        </p>
        <EjercitoStatsRow
          v-for="(item, i) in data.bien.mejores"
          :key="'bien-mejor-' + i"
          :stats="item"
          :posicion="i + 1"
          color="green-darken-2"
        />

        <v-divider class="my-3" />

        <p class="text-subtitle-2 text-red-darken-3 font-weight-bold mb-1">
          <v-icon size="18" color="red-darken-3">mdi-arrow-down-bold</v-icon>
          Bottom {{ data.bien.peores.length }} peores
        </p>
        <EjercitoStatsRow
          v-for="(item, i) in data.bien.peores"
          :key="'bien-peor-' + i"
          :stats="item"
          :posicion="i + 1"
          color="red-darken-2"
        />
      </v-card>
    </v-col>

    <!-- OSCURIDAD (evil) -->
    <v-col cols="12" md="6">
      <v-card class="pa-3" elevation="2">
        <v-card-title class="d-flex align-center gap-2">
          <v-icon color="red-darken-4">mdi-eye</v-icon>
          Bando de la Oscuridad
        </v-card-title>

        <v-divider class="mb-3" />

        <p class="text-subtitle-2 text-green-darken-3 font-weight-bold mb-1">
          <v-icon size="18" color="green-darken-3">mdi-arrow-up-bold</v-icon>
          Top {{ data.oscuridad.mejores.length }} mejores
        </p>
        <EjercitoStatsRow
          v-for="(item, i) in data.oscuridad.mejores"
          :key="'osc-mejor-' + i"
          :stats="item"
          :posicion="i + 1"
          color="green-darken-2"
        />

        <v-divider class="my-3" />

        <p class="text-subtitle-2 text-red-darken-3 font-weight-bold mb-1">
          <v-icon size="18" color="red-darken-3">mdi-arrow-down-bold</v-icon>
          Bottom {{ data.oscuridad.peores.length }} peores
        </p>
        <EjercitoStatsRow
          v-for="(item, i) in data.oscuridad.peores"
          :key="'osc-peor-' + i"
          :stats="item"
          :posicion="i + 1"
          color="red-darken-2"
        />
      </v-card>
    </v-col>
  </v-row>

  <v-alert v-else type="info" class="mt-4">
    No hay datos de estadísticas disponibles todavía.
  </v-alert>
</template>

<script setup lang="ts">
import type { TopBandosResponseDTO } from "@/interfaces/Estadisticas";
import EjercitoStatsRow from "./EjercitoStatsRow.vue";

defineProps<{ data: TopBandosResponseDTO | null }>();
</script>
```

---

## 5. Componente EjercitoStatsRow (fila de stats reutilizable)

**Crear `src/components/Estadisticas/EjercitoStatsRow.vue`**

```vue
<template>
  <v-row class="text-left align-center py-1" dense>
    <v-col cols="1" class="text-center">
      <span class="font-weight-bold text-caption">{{ posicion }}</span>
    </v-col>
    <v-col cols="5">
      <span class="font-weight-medium">{{ stats.ejercito }}</span>
    </v-col>
    <v-col cols="3" class="text-center">
      <!-- Barra de win rate -->
      <v-progress-linear
        :model-value="stats.winRate"
        :color="color"
        bg-color="grey-lighten-3"
        height="18"
        rounded
      >
        <template #default>
          <span class="text-caption font-weight-bold" style="color: white">
            {{ stats.winRate.toFixed(1) }}%
          </span>
        </template>
      </v-progress-linear>
    </v-col>
    <v-col cols="3" class="text-right text-caption text-medium-emphasis">
      {{ stats.victorias }}V / {{ stats.empates }}E / {{ stats.derrotas }}D
      <br />
      <span class="text-caption">
        ({{ stats.totalPartidas }} partidas)
      </span>
    </v-col>
  </v-row>
</template>

<script setup lang="ts">
import type { EjercitoStatsDTO } from "@/interfaces/Estadisticas";

defineProps<{
  stats: EjercitoStatsDTO;
  posicion: number;
  color: string;
}>();
</script>
```

---

## 6. Componente FiltroRatingEjercito

**Crear `src/components/Estadisticas/FiltroRatingEjercito.vue`**

```vue
<template>
  <v-card class="pa-4" elevation="2">
    <v-card-title class="mb-3">
      <v-icon class="mr-2">mdi-magnify</v-icon>
      Consultar rating de un ejército
    </v-card-title>

    <v-form @submit.prevent="buscar">
      <v-row dense>
        <!-- Ejército -->
        <v-col cols="12" md="6">
          <v-text-field
            v-model="form.ejercito"
            label="Ejército *"
            placeholder="Ej: Gondor"
            prepend-inner-icon="mdi-shield"
            clearable
            :rules="[(v) => !!v || 'Campo obligatorio']"
          />
        </v-col>

        <!-- Bando rival -->
        <v-col cols="12" md="6">
          <v-select
            v-model="form.bandoRival"
            label="Bando rival"
            :items="bandosRivales"
            item-title="label"
            item-value="value"
            clearable
            prepend-inner-icon="mdi-sword"
            :disabled="form.ejercitosRivales.length > 0"
            hint="Se ignora si hay ejércitos rivales concretos"
            persistent-hint
          />
        </v-col>

        <!-- Fecha desde -->
        <v-col cols="12" md="4">
          <v-text-field
            v-model="form.fechaDesde"
            label="Fecha desde"
            type="date"
            prepend-inner-icon="mdi-calendar-start"
            clearable
          />
        </v-col>

        <!-- Fecha hasta -->
        <v-col cols="12" md="4">
          <v-text-field
            v-model="form.fechaHasta"
            label="Fecha hasta"
            type="date"
            prepend-inner-icon="mdi-calendar-end"
            clearable
          />
        </v-col>

        <!-- Ejércitos rivales (multi-input) -->
        <v-col cols="12" md="4">
          <v-combobox
            v-model="form.ejercitosRivales"
            label="Ejércitos rivales concretos"
            multiple
            chips
            closable-chips
            prepend-inner-icon="mdi-shield-half-full"
            hint="Escribe y pulsa Enter para añadir"
            persistent-hint
            :disabled="!!form.bandoRival"
          />
        </v-col>

        <!-- Botón buscar -->
        <v-col cols="12" class="text-center mt-2">
          <v-btn
            type="submit"
            color="primary"
            :loading="isLoading"
            prepend-icon="mdi-magnify"
            :disabled="!form.ejercito"
          >
            Consultar
          </v-btn>
          <v-btn
            class="ml-2"
            variant="tonal"
            prepend-icon="mdi-refresh"
            @click="resetForm"
          >
            Limpiar
          </v-btn>
        </v-col>
      </v-row>
    </v-form>

    <!-- Resultado -->
    <v-divider class="my-4" v-if="resultado" />

    <div v-if="resultado">
      <v-row class="align-center mb-2">
        <v-col>
          <span class="text-h6 font-weight-bold">{{ resultado.ejercito }}</span>
          <v-chip
            :color="resultado.bando === 'good' ? 'blue-darken-2' : 'red-darken-4'"
            size="small"
            class="ml-2"
          >
            {{ resultado.bando === "good" ? "Bien" : "Oscuridad" }}
          </v-chip>
        </v-col>
        <v-col cols="auto" v-if="resultado.esFiltrado && resultado.descripcionFiltro">
          <v-chip size="small" color="orange-darken-2" prepend-icon="mdi-filter">
            {{ resultado.descripcionFiltro }}
          </v-chip>
        </v-col>
      </v-row>

      <!-- Barra de resultado -->
      <div class="resultado-barra mb-3">
        <div
          class="victorias"
          :style="{ flex: resultado.victorias || 0 }"
          v-if="resultado.victorias > 0"
        >
          {{ resultado.victorias }}V
        </div>
        <div
          class="empates"
          :style="{ flex: resultado.empates || 0 }"
          v-if="resultado.empates > 0"
        >
          {{ resultado.empates }}E
        </div>
        <div
          class="derrotas"
          :style="{ flex: resultado.derrotas || 0 }"
          v-if="resultado.derrotas > 0"
        >
          {{ resultado.derrotas }}D
        </div>
      </div>

      <!-- Stats en chips/cards -->
      <v-row dense class="text-center">
        <v-col cols="6" md="3">
          <v-card variant="tonal" color="primary" class="pa-2">
            <div class="text-h5 font-weight-bold">{{ resultado.winRate.toFixed(1) }}%</div>
            <div class="text-caption">Win Rate</div>
          </v-card>
        </v-col>
        <v-col cols="6" md="3">
          <v-card variant="tonal" color="grey" class="pa-2">
            <div class="text-h5 font-weight-bold">{{ resultado.totalPartidas }}</div>
            <div class="text-caption">Total partidas</div>
          </v-card>
        </v-col>
        <v-col cols="6" md="3">
          <v-card variant="tonal" color="green-darken-2" class="pa-2">
            <div class="text-h5 font-weight-bold">{{ resultado.mediaPuntosFavor }}</div>
            <div class="text-caption">Media puntos a favor</div>
          </v-card>
        </v-col>
        <v-col cols="6" md="3">
          <v-card variant="tonal" color="red-darken-2" class="pa-2">
            <div class="text-h5 font-weight-bold">{{ resultado.mediaPuntosContra }}</div>
            <div class="text-caption">Media puntos en contra</div>
          </v-card>
        </v-col>
      </v-row>

      <!-- Sin partidas -->
      <v-alert
        v-if="resultado.totalPartidas === 0"
        type="warning"
        class="mt-3"
      >
        No se encontraron partidas para este ejército con los filtros aplicados.
      </v-alert>
    </div>
  </v-card>
</template>

<script setup lang="ts">
import { ref } from "vue";
import { getRatingEjercito } from "@/services/EstadisticasService";
import type {
  RatingEjercitoRequestDTO,
  RatingEjercitoResponseDTO,
} from "@/interfaces/Estadisticas";

const isLoading = ref(false);
const resultado = ref<RatingEjercitoResponseDTO | null>(null);

const bandosRivales = [
  { label: "Bien", value: "good" },
  { label: "Oscuridad", value: "evil" },
];

const form = ref<RatingEjercitoRequestDTO & { ejercitosRivales: string[] }>({
  ejercito: "",
  fechaDesde: null,
  fechaHasta: null,
  ejercitosRivales: [],
  bandoRival: null,
});

async function buscar() {
  if (!form.value.ejercito) return;
  isLoading.value = true;
  resultado.value = null;
  try {
    const request: RatingEjercitoRequestDTO = {
      ejercito: form.value.ejercito,
      fechaDesde: form.value.fechaDesde || null,
      fechaHasta: form.value.fechaHasta || null,
      ejercitosRivales:
        form.value.ejercitosRivales.length > 0
          ? form.value.ejercitosRivales
          : undefined,
      bandoRival:
        form.value.ejercitosRivales.length === 0
          ? form.value.bandoRival || null
          : null,
    };
    resultado.value = await getRatingEjercito(request);
  } catch (e) {
    console.error("Error al consultar rating", e);
  } finally {
    isLoading.value = false;
  }
}

function resetForm() {
  form.value = {
    ejercito: "",
    fechaDesde: null,
    fechaHasta: null,
    ejercitosRivales: [],
    bandoRival: null,
  };
  resultado.value = null;
}
</script>

<style scoped>
.resultado-barra {
  display: flex;
  height: 30px;
  border-radius: 6px;
  box-shadow: 0 0 2px #ccc;
  width: 100%;
}

.resultado-barra > div {
  display: flex;
  justify-content: center;
  align-items: center;
  color: white;
  font-weight: bold;
  font-size: 14px;
}

.victorias {
  border-radius: 6px 0 0 6px;
  background-color: #145c17;
}

.empates {
  background-color: #dbba00;
}

.derrotas {
  border-radius: 0 6px 6px 0;
  background-color: #751710;
}
</style>
```

---

## 7. Router — añadir la ruta

En `src/router/index.ts`, añadir el import y la ruta:

```typescript
// Import
import EstadisticasEjercitoView from "@/views/EstadisticasEjercitoView.vue";

// Dentro del array routes[]
{
  path: "/estadisticas-ejercito",
  name: "estadisticas-ejercito",
  component: EstadisticasEjercitoView,
  meta: { requiresAuth: true },
},
```

---

## 8. NavBar — añadir el botón

En `src/components/Commons/NavBar.vue`:

```html
<!-- En el template, añadir el botón -->
<v-btn @click="goToEstadisticas"> Estadísticas </v-btn>
```

```typescript
// En el script, añadir la función
const goToEstadisticas = () => router.push("estadisticas-ejercito");
```

---

## Estructura de archivos a crear

```
src/
├── interfaces/
│   └── Estadisticas.ts                        (nuevo)
├── services/
│   └── EstadisticasService.ts                 (nuevo)
├── views/
│   └── EstadisticasEjercitoView.vue           (nuevo)
└── components/
    └── Estadisticas/
        ├── TopBandosCard.vue                  (nuevo)
        ├── EjercitoStatsRow.vue               (nuevo)
        └── FiltroRatingEjercito.vue           (nuevo)
```

Modificaciones en archivos existentes:
- `src/router/index.ts` — añadir import + ruta
- `src/components/Commons/NavBar.vue` — añadir botón

---

## Notas de los endpoints

| Endpoint | Método | Ruta | Auth |
|---|---|---|---|
| Top/bottom ejércitos por bando | GET | `/api/Estadisticas/ejercito/top-bandos?top=3` | Bearer token |
| Rating de un ejército filtrado | POST | `/api/Estadisticas/ejercito/rating` | Bearer token |

**`bando`** en las respuestas: `"good"` = Bien · `"evil"` = Oscuridad

**`winRate`** = `victorias / totalPartidas * 100` (redondeado a 2 decimales)
