<script setup lang="ts">
import '@/assets/main.css';
import NavBar from "../views/NavBar.vue";
import { ref } from 'vue';
import { useRouter } from '@/router';

const router = useRouter();

interface UserProfile {
  firstName: string;
  lastName: string;
  email: string;
  phone: string;
  dateOfBirth: string;
  address: string;
  city: string;
  postalCode: string;
  memberSince: string;
  notifications: boolean;
  newsletter: boolean;
}

const profile = ref<UserProfile>({
  firstName: 'Jan',
  lastName: 'Novák',
  email: 'jan.novak@example.com',
  phone: '+420 123 456 789',
  dateOfBirth: '1990-05-15',
  address: 'Hlavní 123',
  city: 'Praha',
  postalCode: '110 00',
  memberSince: '2023-01-15',
  notifications: true,
  newsletter: false
});

const isEditing = ref(false);
const editProfile = ref<UserProfile>({ ...profile.value });
const activeTab = ref<'profile' | 'security' | 'preferences'>('profile');
const showPasswordChange = ref(false);

function startEditing() {
  isEditing.value = true;
  editProfile.value = { ...profile.value };
}

function cancelEditing() {
  isEditing.value = false;
  editProfile.value = { ...profile.value };
}

function saveProfile() {
  profile.value = { ...editProfile.value };
  isEditing.value = false;
}

function logout() {
  router.push({ name: 'login' });
}

const stats = ref([
  { label: 'Celkem rezervací', value: '24', icon: '📅' },
  { label: 'Aktivní rezervace', value: '3', icon: '⏰' },
  { label: 'Členství', value: `${Math.floor((Date.now() - new Date(profile.value.memberSince).getTime()) / (1000 * 60 * 60 * 24 * 30))} měsíců`, icon: '🏆' },
  { label: 'Utraceno', value: '4 250 Kč', icon: '💰' }
]);
</script>

<template>
  <NavBar></NavBar>
  <div class="account-container">
    <div class="account-header">
      <h1 class="page-title">Můj účet</h1>
      <p class="page-subtitle">Správa vašeho profilu a nastavení</p>
    </div>

    <div class="stats-grid">
      <div v-for="stat in stats" :key="stat.label" class="stat-card">
        <div class="stat-icon">{{ stat.icon }}</div>
        <div class="stat-content">
          <div class="stat-value">{{ stat.value }}</div>
          <div class="stat-label">{{ stat.label }}</div>
        </div>
      </div>
    </div>

    <div class="tabs">
      <button 
        @click="activeTab = 'profile'" 
        :class="['tab-button', { active: activeTab === 'profile' }]"
      >
        Profil
      </button>
      <button 
        @click="activeTab = 'security'" 
        :class="['tab-button', { active: activeTab === 'security' }]"
      >
        Zabezpečení
      </button>
      <button 
        @click="activeTab = 'preferences'" 
        :class="['tab-button', { active: activeTab === 'preferences' }]"
      >
        Předvolby
      </button>
    </div>

    <div class="tab-content">
      <!-- Profile Tab -->
      <div v-if="activeTab === 'profile'" class="profile-section">
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

        <div class="profile-form">
          <div class="form-row">
            <div class="form-group">
              <label>Jméno</label>
              <input 
                :value="isEditing ? editProfile.firstName : profile.firstName" 
                @input="isEditing ? editProfile.firstName = ($event.target as HTMLInputElement).value : null"
                :disabled="!isEditing"
                type="text" 
                class="form-input"
              />
            </div>
            <div class="form-group">
              <label>Příjmení</label>
              <input 
                :value="isEditing ? editProfile.lastName : profile.lastName" 
                @input="isEditing ? editProfile.lastName = ($event.target as HTMLInputElement).value : null"
                :disabled="!isEditing"
                type="text" 
                class="form-input"
              />
            </div>
          </div>

          <div class="form-group">
            <label>E-mail</label>
            <input 
              :value="isEditing ? editProfile.email : profile.email" 
              @input="isEditing ? editProfile.email = ($event.target as HTMLInputElement).value : null"
              :disabled="!isEditing"
              type="email" 
              class="form-input"
            />
          </div>

          <div class="form-group">
            <label>Telefon</label>
            <input 
              :value="isEditing ? editProfile.phone : profile.phone" 
              @input="isEditing ? editProfile.phone = ($event.target as HTMLInputElement).value : null"
              :disabled="!isEditing"
              type="tel" 
              class="form-input"
            />
          </div>

          <div class="form-group">
            <label>Datum narození</label>
            <input 
              :value="isEditing ? editProfile.dateOfBirth : profile.dateOfBirth" 
              @input="isEditing ? editProfile.dateOfBirth = ($event.target as HTMLInputElement).value : null"
              :disabled="!isEditing"
              type="date" 
              class="form-input"
            />
          </div>

          <div class="form-group">
            <label>Adresa</label>
            <input 
              :value="isEditing ? editProfile.address : profile.address" 
              @input="isEditing ? editProfile.address = ($event.target as HTMLInputElement).value : null"
              :disabled="!isEditing"
              type="text" 
              class="form-input"
            />
          </div>

          <div class="form-row">
            <div class="form-group">
              <label>Město</label>
              <input 
                :value="isEditing ? editProfile.city : profile.city" 
                @input="isEditing ? editProfile.city = ($event.target as HTMLInputElement).value : null"
                :disabled="!isEditing"
                type="text" 
                class="form-input"
              />
            </div>
            <div class="form-group">
              <label>PSČ</label>
              <input 
                :value="isEditing ? editProfile.postalCode : profile.postalCode" 
                @input="isEditing ? editProfile.postalCode = ($event.target as HTMLInputElement).value : null"
                :disabled="!isEditing"
                type="text" 
                class="form-input"
              />
            </div>
          </div>

          <div class="form-group">
            <label>Člen od</label>
            <input 
              :value="new Date(profile.memberSince).toLocaleDateString('cs-CZ')" 
              disabled
              type="text" 
              class="form-input"
            />
          </div>
        </div>
      </div>

      <!-- Security Tab -->
      <div v-if="activeTab === 'security'" class="security-section">
        <div class="section-header">
          <h2>Zabezpečení účtu</h2>
        </div>

        <div class="security-content">
          <div class="security-item">
            <div class="security-info">
              <h3>Změna hesla</h3>
              <p>Pravidelně měňte heslo pro zabezpečení účtu</p>
            </div>
            <button @click="showPasswordChange = !showPasswordChange" class="change-button">
              Změnit
            </button>
          </div>

          <div v-if="showPasswordChange" class="password-form">
            <div class="form-group">
              <label>Současné heslo</label>
              <input type="password" class="form-input" placeholder="Zadejte současné heslo" />
            </div>
            <div class="form-group">
              <label>Nové heslo</label>
              <input type="password" class="form-input" placeholder="Zadejte nové heslo" />
            </div>
            <div class="form-group">
              <label>Potvrzení nového hesla</label>
              <input type="password" class="form-input" placeholder="Zopakujte nové heslo" />
            </div>
            <div class="form-actions">
              <button @click="showPasswordChange = false" class="cancel-button">Zrušit</button>
              <button class="save-button">Uložit heslo</button>
            </div>
          </div>

          <div class="security-item">
            <div class="security-info">
              <h3>Dvoufaktorové ověření</h3>
              <p>Přidejte další vrstvu zabezpečení</p>
            </div>
            <button class="enable-button">Povolit</button>
          </div>

          <div class="security-item">
            <div class="security-info">
              <h3>Aktivní relace</h3>
              <p>Spravujte přihlášená zařízení</p>
            </div>
            <button class="manage-button">Spravovat</button>
          </div>
        </div>
      </div>

      <!-- Preferences Tab -->
      <div v-if="activeTab === 'preferences'" class="preferences-section">
        <div class="section-header">
          <h2>Předvolby</h2>
        </div>

        <div class="preferences-content">
          <div class="preference-item">
            <div class="preference-info">
              <h3>E-mailová oznámení</h3>
              <p>Dostávejte upozornění o rezervacích a důležité zprávy</p>
            </div>
            <label class="switch">
              <input v-model="profile.notifications" type="checkbox">
              <span class="slider"></span>
            </label>
          </div>

          <div class="preference-item">
            <div class="preference-info">
              <h3>Newsletter</h3>
              <p>Přihlaste se k odběru novinek a speciálních nabídek</p>
            </div>
            <label class="switch">
              <input v-model="profile.newsletter" type="checkbox">
              <span class="slider"></span>
            </label>
          </div>

          <div class="preference-item">
            <div class="preference-info">
              <h3>Jazyk</h3>
              <p>Vyberte preferovaný jazyk rozhraní</p>
            </div>
            <select class="form-input" style="width: 150px;">
              <option value="cs">Čeština</option>
              <option value="en">English</option>
            </select>
          </div>
        </div>
      </div>
    </div>

    <div class="danger-zone">
      <button @click="logout" class="logout-button">Odhlásit se</button>
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

.stats-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
  gap: 1rem;
  margin-bottom: 2rem;
}

.stat-card {
  background: linear-gradient(135deg, rgba(30, 136, 229, 0.05), rgba(0, 188, 212, 0.05));
  border: 2px solid rgba(30, 136, 229, 0.2);
  border-radius: var(--radius-lg);
  padding: 1.5rem;
  display: flex;
  align-items: center;
  gap: 1rem;
  transition: all 0.3s ease;
}

.stat-card:hover {
  transform: translateY(-4px);
  border-color: var(--vt-c-primary);
  box-shadow: var(--shadow-lg);
}

.stat-icon {
  font-size: 2rem;
  opacity: 0.8;
}

.stat-value {
  font-size: 1.5rem;
  font-weight: 600;
  color: var(--color-heading);
}

.stat-label {
  font-size: 0.875rem;
  color: var(--color-text);
  opacity: 0.8;
}

.tabs {
  display: flex;
  gap: 1rem;
  margin-bottom: 2rem;
  border-bottom: 2px solid var(--vt-c-divider);
  padding-bottom: 0;
}

.tab-button {
  padding: 1rem 1.5rem;
  background: none;
  border: none;
  border-bottom: 3px solid transparent;
  font-size: 1rem;
  font-weight: 500;
  color: var(--color-text);
  cursor: pointer;
  transition: all 0.3s ease;
  margin-bottom: -2px;
}

.tab-button:hover {
  color: var(--color-heading);
}

.tab-button.active {
  color: var(--vt-c-secondary);
  border-bottom-color: var(--vt-c-secondary);
}}

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

.edit-button, .save-button, .cancel-button, .change-button, .enable-button, .manage-button {
  padding: 0.5rem 1rem;
  border: none;
  border-radius: 0.5rem;
  font-weight: 500;
  cursor: pointer;
  transition: all 0.3s ease;
}

.edit-button, .change-button, .manage-button {
  background-color: var(--vt-c-primary);
  color: white;
  transition: all 0.3s ease;
}

.edit-button:hover, .change-button:hover, .manage-button:hover {
  background-color: var(--vt-c-primary-dark);
  transform: translateY(-2px);
  box-shadow: var(--shadow-lg);
}

.save-button {
  background-color: var(--vt-c-success);
  color: white;
  transition: all 0.3s ease;
}

.save-button:hover {
  background-color: var(--vt-c-success-light);
  transform: translateY(-2px);
  box-shadow: var(--shadow-lg);
}

.cancel-button {
  background: linear-gradient(135deg, rgba(248, 249, 251, 0.8), rgba(239, 240, 245, 0.8));
  color: var(--color-heading);
  border: 1px solid var(--vt-c-divider);
}

.cancel-button:hover {
  background: linear-gradient(135deg, rgba(248, 249, 251, 1), rgba(239, 240, 245, 1));
}

.enable-button {
  background-color: #2196F3;
  color: white;
}

.enable-button:hover {
  background-color: #1976D2;
}

.edit-actions {
  display: flex;
  gap: 0.5rem;
}

.profile-form {
  display: flex;
  flex-direction: column;
  gap: 1.5rem;
}

.form-row {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 1rem;
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
  background: linear-gradient(135deg, rgba(248, 249, 251, 0.8), rgba(239, 240, 245, 0.8));
  color: var(--color-text);
  transition: all 0.3s ease;
}

.form-input:focus {
  outline: none;
  border-color: var(--vt-c-primary);
  background-color: linear-gradient(135deg, rgba(248, 249, 251, 1), rgba(239, 240, 245, 1));
  box-shadow: 0 0 0 3px rgba(254, 232, 129, 0.2);
}

.form-input:disabled {
  opacity: 0.7;
  cursor: not-allowed;
}

.security-content, .preferences-content {
  display: flex;
  flex-direction: column;
  gap: 1.5rem;
}

.security-item, .preference-item {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 1.5rem;
  background: linear-gradient(135deg, var(--color-background), rgba(30, 136, 229, 0.02));
  border: 2px solid rgba(30, 136, 229, 0.1);
  border-radius: var(--radius-lg);
  transition: all 0.3s ease;
}

.security-item:hover, .preference-item:hover {
  border-color: var(--vt-c-primary);
  box-shadow: var(--shadow-md);
}

.security-info h3, .preference-info h3 {
  font-size: 1.1rem;
  font-weight: 600;
  color: var(--color-heading);
  margin-bottom: 0.25rem;
}

.security-info p, .preference-info p {
  color: var(--color-text);
  opacity: 0.8;
  font-size: 0.9rem;
}

.password-form {
  padding: 1.5rem;
  background: linear-gradient(135deg, rgba(30, 136, 229, 0.05), rgba(0, 188, 212, 0.05));
  border-radius: var(--radius-lg);
  border-left: 4px solid var(--vt-c-primary);
}

.form-actions {
  display: flex;
  gap: 1rem;
  margin-top: 1rem;
}

.switch {
  position: relative;
  display: inline-block;
  width: 60px;
  height: 34px;
}

.switch input {
  opacity: 0;
  width: 0;
  height: 0;
}

.slider {
  position: absolute;
  cursor: pointer;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background-color: #ccc;
  transition: .4s;
  border-radius: 34px;
}

.slider:before {
  position: absolute;
  content: "";
  height: 26px;
  width: 26px;
  left: 4px;
  bottom: 4px;
  background-color: white;
  transition: .4s;
  border-radius: 50%;
}

input:checked + .slider {
  background-color: var(--vt-c-primary);
}

input:checked + .slider:before {
  transform: translateX(26px);
}

.danger-zone {
  margin-top: 3rem;
  padding: 2rem;
  background: #FFF5F5;
  border: 1px solid #FED7D7;
  border-radius: 1rem;
  text-align: center;
}

.danger-zone h3 {
  color: #E53E3E;
  margin-bottom: 0.5rem;
}

.danger-zone p {
  color: var(--color-text);
  opacity: 0.8;
  margin-bottom: 1.5rem;
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
    border-bottom-color: var(--vt-c-primary);
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

@media (max-width: 480px) {
  .account-container {
    padding: 4rem 0.75rem 1rem;
  }

  .account-header {
    margin-bottom: 1.5rem;
  }

  .page-title {
    font-size: 1.5rem;
  }

  .page-subtitle {
    font-size: 0.9rem;
  }

  .stats-grid {
    grid-template-columns: 1fr;
    gap: 0.75rem;
    margin-bottom: 1.5rem;
  }

  .stat-card {
    padding: 1rem;
    gap: 0.75rem;
  }

  .stat-icon {
    font-size: 1.5rem;
  }

  .stat-value {
    font-size: 1.25rem;
  }

  .stat-label {
    font-size: 0.75rem;
  }

  .tabs {
    flex-direction: column;
    gap: 0;
    margin-bottom: 1.5rem;
    border-bottom: 1px solid var(--vt-c-divider);
    padding-bottom: 0;
  }

  .tab-button {
    padding: 0.75rem 1rem;
    font-size: 0.9rem;
    border-bottom: 1px solid var(--vt-c-divider);
    border-radius: 0;
    margin-bottom: 0;
    min-height: 44px;
  }

  .tab-button.active {
    border-bottom: 2px solid var(--vt-c-primary);
  }

  .section-header {
    flex-direction: column;
    align-items: stretch;
    gap: 1rem;
    margin-bottom: 1.5rem;
  }

  .section-header h2 {
    font-size: 1.2rem;
  }

  .section-header .edit-button {
    align-self: flex-start;
    padding: 0.5rem 1rem;
    font-size: 0.85rem;
  }

  .edit-button, .save-button, .cancel-button, .change-button, .enable-button, .manage-button {
    min-height: 44px;
    font-size: 0.85rem;
    padding: 0.75rem 1rem;
  }

  .edit-actions {
    flex-direction: column;
    gap: 0.75rem;
  }

  .profile-form {
    gap: 1rem;
  }

  .form-row {
    grid-template-columns: 1fr;
    gap: 0.75rem;
  }

  .form-group {
    gap: 0.375rem;
  }

  .form-group label {
    font-size: 0.85rem;
  }

  .form-input {
    padding: 0.75rem 0.75rem;
    font-size: 16px;
    min-height: 44px;
  }

  .security-content, .preferences-content {
    gap: 1rem;
  }

  .security-item, .preference-item {
    flex-direction: column;
    align-items: flex-start;
    padding: 1rem;
    gap: 1rem;
  }

  .security-info h3, .preference-info h3 {
    font-size: 1rem;
  }

  .security-info p, .preference-info p {
    font-size: 0.8rem;
  }

  .password-form {
    padding: 1rem;
    border-left: 3px solid var(--vt-c-primary);
  }

  .form-actions {
    flex-direction: column;
    gap: 0.5rem;
    margin-top: 0.75rem;
  }

  .form-actions button {
    width: 100%;
    min-height: 44px;
  }

  .switch {
    width: 50px;
    height: 28px;
  }

  .slider:before {
    height: 22px;
    width: 22px;
    left: 3px;
    bottom: 3px;
  }

  input:checked + .slider:before {
    transform: translateX(22px);
  }

  .danger-zone {
    margin-top: 2rem;
    padding: 1rem;
  }

  .danger-zone h3 {
    font-size: 1rem;
  }

  .logout-button {
    width: 100%;
    min-height: 44px;
    font-size: 0.9rem;
  }
}
</style>
