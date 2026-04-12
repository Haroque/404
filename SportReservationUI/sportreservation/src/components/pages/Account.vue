<script setup lang="ts">
import '@/assets/main.css';
import NavBar from "../views/NavBar.vue";
import { ref, onMounted } from 'vue';
import { useRouter } from '@/router';
import { API_URL, secureFetch, logout } from '../../auth';
import type { UserDto, ReservationDto, UserPatchDto, UserPatchPasswordDto } from '../../lib/sportApi';
import { formatDate, formatDateTime } from '../../lib/sportApi';

interface UserProfile {
  id: string;
  fullName: string;
  email: string;
  createdAt: string;
}

interface UserReservation {
  id: string;
  facilityName: string;
  date: string;
  time: string;
  duration: number;
  price: number;
  status: 'active' | 'cancelled';
}

const router = useRouter();
const profile = ref<UserProfile | null>(null);
const reservations = ref<UserReservation[]>([]);
const isLoading = ref(false);
const isEditing = ref(false);
const editProfile = ref<UserProfile | null>(null);
const showPasswordChange = ref(false);
const currentPassword = ref('');
const newPassword = ref('');
const confirmPassword = ref('');
const passwordError = ref('');
const saveError = ref('');
const saveSuccess = ref(false);

async function loadUserData() {
  try {
    isLoading.value = true;
    const response = await secureFetch(`${API_URL}/User/Self`);
    if (response.ok) {
      const data: UserDto = await response.json();
      profile.value = {
        id: data.id,
        fullName: data.fullName,
        email: data.email,
        createdAt: data.createdAt
      };
      editProfile.value = { ...profile.value };
      await loadUserReservations(data.id);
    }
  } catch (error) {
    console.error('Error loading user data:', error);
  } finally {
    isLoading.value = false;
  }
}

async function loadUserReservations(userId: string) {
  try {
    const response = await secureFetch(`${API_URL}/Reservation?user_id=${userId}`);
    if (response.ok) {
      const data: ReservationDto[] = await response.json();
      reservations.value = data.map(r => ({
        id: r.id,
        facilityName: '', // Will be filled by fetching facility details if needed
        date: formatDate(r.startAt),
        time: formatDateTime(r.startAt).split(' ')[1] || '',
        duration: Math.round((new Date(r.endAt).getTime() - new Date(r.startAt).getTime()) / 60000),
        price: r.finalPrice,
        status: r.status.toLowerCase() as 'active' | 'cancelled'
      }));
    }
  } catch (error) {
    console.error('Error loading user reservations:', error);
  }
}

function startEditing() {
  isEditing.value = true;
  if (profile.value) {
    editProfile.value = { ...profile.value };
  }
  passwordError.value = '';
  saveError.value = '';
  saveSuccess.value = false;
}

function cancelEditing() {
  isEditing.value = false;
  if (profile.value) {
    editProfile.value = { ...profile.value };
  }
  currentPassword.value = '';
  newPassword.value = '';
  confirmPassword.value = '';
  passwordError.value = '';
  saveError.value = '';
}

async function saveProfile() {
  if (!editProfile.value) return;
  
  try {
    saveError.value = '';
    saveSuccess.value = false;
    
    const patchDto: UserPatchDto = {
      fullName: editProfile.value.fullName !== profile.value?.fullName ? editProfile.value.fullName : undefined,
      email: editProfile.value.email !== profile.value?.email ? editProfile.value.email : undefined
    };
    
    // Add password change if provided
    if (newPassword.value) {
      if (newPassword.value !== confirmPassword.value) {
        passwordError.value = 'Hesla se neshodují';
        return;
      }
      if (newPassword.value.length < 6) {
        passwordError.value = 'Heslo musí mít alespoň 6 znaků';
        return;
      }
      patchDto.password = {
        current: currentPassword.value || null,
        new: newPassword.value
      };
    }
    
    const response = await secureFetch(`${API_URL}/User/${profile.value?.id}`, {
      method: 'PATCH',
      body: JSON.stringify(patchDto)
    });
    
    if (response.ok) {
      profile.value = { ...editProfile.value };
      isEditing.value = false;
      saveSuccess.value = true;
      currentPassword.value = '';
      newPassword.value = '';
      confirmPassword.value = '';
      showPasswordChange.value = false;
      setTimeout(() => saveSuccess.value = false, 3000);
    } else {
      const error = await response.text();
      saveError.value = error || 'Chyba při ukládání údajů';
    }
  } catch (error) {
    console.error('Error saving profile:', error);
    saveError.value = 'Chyba při ukládání údajů';
  }
}

onMounted(() => {
  loadUserData();
});
</script>

<template>
  <NavBar></NavBar>
  <div class="account-container">
    <div class="account-header">
      <h1 class="page-title">Můj profil</h1>
      <p class="page-subtitle">Správa vašeho profilu a rezervací</p>
    </div>

    <div v-if="isLoading" class="loading">Načítám údaje...</div>
    <div v-else-if="profile">
      <!-- Profile Section -->
      <div class="profile-section">
        <div class="section-header">
          <h2>Osobní údaje</h2>
          <button v-if="!isEditing" @click="startEditing" class="edit-button">
            Upravit
          </button>
          <div v-else class="edit-actions">
            <button @click="cancelEditing" class="cancel-button">Zrušit</button>
            <button @click="saveProfile" class="save-button">Uložit</button>
          </div>
        </div>

        <div class="success-message" v-if="saveSuccess">
          Údaje byly úspěšně uloženy!
        </div>
        <div class="error-message" v-if="saveError">
          {{ saveError }}
        </div>

        <div class="profile-form">
          <div class="form-group">
            <label>Jméno</label>
            <input 
              :value="isEditing && editProfile ? editProfile.fullName : profile.fullName" 
              @input="isEditing && editProfile ? editProfile.fullName = ($event.target as HTMLInputElement).value : null"
              :disabled="!isEditing"
              type="text" 
              class="form-input"
            />
          </div>

          <div class="form-group">
            <label>E-mail</label>
            <input 
              :value="isEditing && editProfile ? editProfile.email : profile.email" 
              @input="isEditing && editProfile ? editProfile.email = ($event.target as HTMLInputElement).value : null"
              :disabled="!isEditing"
              type="email" 
              class="form-input"
            />
          </div>

          <div class="form-group">
            <label>Člen od</label>
            <input 
              :value="new Date(profile.createdAt).toLocaleDateString('cs-CZ')" 
              disabled
              type="text" 
              class="form-input"
            />
          </div>

          <div v-if="isEditing" class="password-section">
            <h3>Změna hesla (volitelné)</h3>
            <div class="form-group">
              <label>Současné heslo</label>
              <input v-model="currentPassword" type="password" class="form-input" placeholder="Zadejte současné heslo" />
            </div>
            <div class="form-group">
              <label>Nové heslo</label>
              <input v-model="newPassword" type="password" class="form-input" placeholder="Zadejte nové heslo" />
            </div>
            <div class="form-group">
              <label>Potvrzení nového hesla</label>
              <input v-model="confirmPassword" type="password" class="form-input" placeholder="Zopakujte nové heslo" />
            </div>
            <div class="error-message" v-if="passwordError">
              {{ passwordError }}
            </div>
          </div>
        </div>
      </div>

      <!-- Reservations Section -->
      <div class="reservations-section">
        <div class="section-header">
          <h2>Moje rezervace</h2>
        </div>

        <div v-if="reservations.length === 0" class="empty-state">
          Nemáte žádné rezervace
        </div>
        <div v-else class="reservation-cards">
          <div v-for="reservation in reservations" :key="reservation.id" class="reservation-card">
            <div class="card-header">
              <h4 class="facility-name">Rezervace #{{ reservation.id }}</h4>
              <span class="status-badge" :class="reservation.status">
                {{ reservation.status === 'active' ? 'Aktivní' : 'Zrušená' }}
              </span>
            </div>
            
            <div class="card-details">
              <div class="detail-row">
                <span class="detail-icon">📅</span>
                <span class="detail-text">{{ reservation.date }}</span>
              </div>
              <div class="detail-row">
                <span class="detail-icon">⏰</span>
                <span class="detail-text">{{ reservation.time }} ({{ reservation.duration }} min)</span>
              </div>
              <div class="detail-row">
                <span class="detail-icon">💰</span>
                <span class="detail-text">{{ reservation.price }} Kč</span>
              </div>
            </div>
          </div>
        </div>
      </div>

      <div class="danger-zone">
        <button @click="logout" class="logout-button">Odhlásit se</button>
      </div>
    </div>
  </div>
</template>

<style scoped>
.account-container {
  max-width: 1200px;
  margin: 0 auto;
  padding: 6rem 1.5rem 2rem;
}

.account-header {
  text-align: center;
  margin-bottom: 2rem;
}

.page-title {
  font-size: 2.5rem;
  font-weight: 600;
  color: var(--color-heading);
  margin-bottom: 0.5rem;
}

.page-subtitle {
  font-size: 1.1rem;
  color: var(--color-text);
  opacity: 0.8;
}

.loading {
  text-align: center;
  padding: 2rem;
  color: var(--color-text);
  opacity: 0.8;
}

.profile-section, .reservations-section {
  background: var(--vt-c-white-soft);
  border: 1px solid var(--vt-c-divider);
  border-radius: 1rem;
  padding: 2rem;
  margin-bottom: 2rem;
}

.section-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 2rem;
}

.section-header h2 {
  font-size: 1.5rem;
  font-weight: 600;
  color: var(--color-heading);
}

.edit-button, .save-button, .cancel-button {
  padding: 0.5rem 1rem;
  border: none;
  border-radius: 0.5rem;
  font-weight: 500;
  cursor: pointer;
  transition: all 0.3s ease;
}

.edit-button {
  background-color: var(--vt-c-yellow);
  color: var(--color-heading);
}

.edit-button:hover {
  background-color: var(--vt-c-yellow-light);
}

.save-button {
  background-color: #4CAF50;
  color: white;
}

.save-button:hover {
  background-color: #45a049;
}

.cancel-button {
  background-color: var(--vt-c-white-mute);
  color: var(--color-heading);
  border: 1px solid var(--vt-c-divider);
}

.cancel-button:hover {
  background-color: var(--vt-c-white-soft);
}

.edit-actions {
  display: flex;
  gap: 0.5rem;
}

.success-message {
  background-color: #d4edda;
  color: #155724;
  padding: 1rem;
  border-radius: 0.5rem;
  margin-bottom: 1rem;
  border: 1px solid #c3e6cb;
}

.error-message {
  background-color: #f8d7da;
  color: #721c24;
  padding: 1rem;
  border-radius: 0.5rem;
  margin-bottom: 1rem;
  border: 1px solid #f5c6cb;
}

.profile-form {
  display: flex;
  flex-direction: column;
  gap: 1.5rem;
}

.form-group {
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
}

.form-group label {
  font-weight: 500;
  color: var(--color-heading);
}

.form-input {
  padding: 0.75rem 1rem;
  border: 1px solid var(--vt-c-divider);
  border-radius: 0.5rem;
  background-color: var(--vt-c-white-soft);
  color: var(--color-text);
  transition: all 0.3s ease;
}

.form-input:focus {
  outline: none;
  border-color: var(--vt-c-yellow);
  background-color: white;
}

.form-input:disabled {
  opacity: 0.7;
  cursor: not-allowed;
}

.password-section {
  margin-top: 2rem;
  padding-top: 2rem;
  border-top: 1px solid var(--vt-c-divider);
}

.password-section h3 {
  font-size: 1.1rem;
  font-weight: 600;
  color: var(--color-heading);
  margin-bottom: 1rem;
}

.reservation-cards {
  display: grid;
  gap: 1rem;
}

.reservation-card {
  background: var(--vt-c-white);
  border: 1px solid var(--vt-c-divider);
  border-radius: 0.5rem;
  padding: 1.5rem;
  transition: all 0.3s ease;
}

.reservation-card:hover {
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
}

.card-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 1rem;
}

.facility-name {
  font-size: 1.1rem;
  font-weight: 600;
  color: var(--color-heading);
  margin: 0;
}

.status-badge {
  color: white;
  padding: 0.25rem 0.75rem;
  border-radius: 1rem;
  font-size: 0.875rem;
  font-weight: 500;
}

.status-badge.active {
  background-color: #2196F3;
}

.status-badge.cancelled {
  background-color: #F44336;
}

.card-details {
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
}

.detail-row {
  display: flex;
  align-items: center;
  gap: 0.75rem;
}

.detail-icon {
  font-size: 1.1rem;
  opacity: 0.7;
}

.detail-text {
  color: var(--color-text);
  font-weight: 500;
}

.empty-state {
  text-align: center;
  padding: 2rem;
  color: var(--color-text);
  opacity: 0.8;
}
.danger-zone {
  margin-top: 3rem;
  padding: 2rem;
  background: #FFF5F5;
  border: 1px solid #FED7D7;
  border-radius: 1rem;
  text-align: center;
}

.logout-button {
  background-color: #E53E3E;
  color: white;
  padding: 0.75rem 2rem;
  border: none;
  border-radius: 0.5rem;
  font-weight: 600;
  cursor: pointer;
  transition: background-color 0.3s ease;
}

.logout-button:hover {
  background-color: #C53030;
}

@media (max-width: 768px) {
  .account-container {
    padding: 6rem 1rem 2rem;
  }
  
  .stats-grid {
    grid-template-columns: 1fr;
  }
  
  .tabs {
    flex-direction: column;
    gap: 0;
  }
  
  .tab-button {
    border-bottom: 1px solid var(--vt-c-divider);
    border-radius: 0;
    margin-bottom: 0;
  }
  
  .tab-button.active {
    border-bottom-color: var(--vt-c-yellow);
  }
  
  .form-row {
    grid-template-columns: 1fr;
  }
  
  .security-item, .preference-item {
    flex-direction: column;
    align-items: flex-start;
    gap: 1rem;
  }
  
  .section-header {
    flex-direction: column;
    align-items: flex-start;
    gap: 1rem;
  }
}
</style>
