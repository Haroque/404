<script setup lang="ts">
import '@/assets/main.css';
import NavBar from "../views/NavBar.vue";
import { ref, computed, onMounted } from 'vue';
import { useRouter } from '@/router';

interface CalendarSlot {
  time: string;
  occupied: boolean;
  selected: boolean;
  isCurrentUser?: boolean;
  highlighted?: boolean;
}

interface CalendarDay {
  date: string;
  name: string;
  slots: CalendarSlot[];
}

interface FacilityType {
  id: string;
  name: string;
  description?: string;
}

interface Facility {
  id: string;
  name: string;
  typeId: string;
  type: FacilityType;
  pricePerHour: number;
  capacity: number;
  isActive: boolean;
  createdAt: string;
  description?: string;
  image?: string;
}

interface PriceList {
  id: string;
  facilityTypeId: string;
  validFrom: string;
  validTo?: string;
  pricePerHour: number;
}

interface TimeSlot {
  id: string;
  time: string;
  available: boolean;
  price?: number;
}

interface Reservation {
  id: number;
  facilityName: string;
  date: string;
  time: string;
  duration: number;
  price: number;
  status: 'completed' | 'cancelled';
  isCurrentUser?: boolean;
}

const reservations = ref<Reservation[]>([
  {
    id: 1,
    facilityName: 'Hala A - Badminton',
    date: '10.04.2024',
    time: '18:00',
    duration: 60,
    price: 300,
    status: 'completed',
    isCurrentUser: true
  },
  {
    id: 2,
    facilityName: 'Kurt č. 3 - Tenis',
    date: '20.04.2024',
    time: '10:00',
    duration: 120,
    price: 600,
    status: 'completed',
    isCurrentUser: false
  },
  {
    id: 3,
    facilityName: 'Kurt č. 1 - Tenis',
    date: '08.04.2026',
    time: '14:00',
    duration: 60,
    price: 300,
    status: 'completed',
    isCurrentUser: false
  },
  {
    id: 4,
    facilityName: 'Kurt č. 1 - Tenis',
    date: '08.04.2026',
    time: '16:00',
    duration: 60,
    price: 300,
    status: 'completed',
    isCurrentUser: true
  },
  {
    id: 5,
    facilityName: 'Hala A - Badminton',
    date: '09.04.2026',
    time: '10:00',
    duration: 60,
    price: 250,
    status: 'completed',
    isCurrentUser: false
  }
]);

const facilities = ref<Facility[]>([]);
const priceListCache = ref<Map<string, PriceList>>(new Map());
const isLoadingFacilities = ref(false);

const timeSlots = ref<TimeSlot[]>([
  { id: '1', time: '06:00', available: true },
  { id: '2', time: '07:00', available: true },
  { id: '3', time: '08:00', available: false },
  { id: '4', time: '09:00', available: true },
  { id: '5', time: '10:00', available: true },
  { id: '6', time: '11:00', available: false },
  { id: '7', time: '12:00', available: true },
  { id: '8', time: '13:00', available: true },
  { id: '9', time: '14:00', available: false },
  { id: '10', time: '15:00', available: true },
  { id: '11', time: '16:00', available: true },
  { id: '12', time: '17:00', available: true },
  { id: '13', time: '18:00', available: false },
  { id: '14', time: '19:00', available: true },
  { id: '15', time: '20:00', available: true },
  { id: '16', time: '21:00', available: true },
  { id: '17', time: '22:00', available: false }
]);

const activeTab = ref<'create'>('create');
const selectedFacility = ref<Facility | null>(facilities.value[0] || null);
const selectedDate = ref('');
const selectedTime = ref('');
const selectedDuration = ref(60);
const showCreateForm = ref(true);
const currentWeekStart = ref(new Date());
const selectedSlots = ref<Array<{date: string, time: string}>>([]);
const hours = ref([6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19]);
const reservationFilterDate = ref('');
const highlightedSlot = ref<{date: string, time: string} | null>(null);

// Computed property for filtered reservations
const filteredReservations = computed(() => {
  let filtered = reservations.value.filter(r => r.status === 'completed' && r.isCurrentUser);
  
  if (reservationFilterDate.value) {
    filtered = filtered.filter(r => r.date === reservationFilterDate.value);
  }
  
  return filtered;
});

// API Functions
const API_BASE_URL = 'http://localhost:5234/api';

async function loadFacilities() {
  try {
    isLoadingFacilities.value = true;
    const fetchUrl = `${API_BASE_URL}/Facility?page=1&per_page=100`;
    console.log('=== FACILITIES LOADING ===');
    console.log('Loading facilities from:', fetchUrl);
    console.log('⏱️ Starting fetch...');
    
    // Create abort controller with 10 second timeout
    const controller = new AbortController();
    const timeoutId = setTimeout(() => controller.abort(), 10000);
    
    const response = await fetch(fetchUrl, { signal: controller.signal });
    clearTimeout(timeoutId);
    
    console.log('✓ Response received, status:', response.status);
    
    if (!response.ok) {
      console.error('❌ API response status:', response.status);
      const errorText = await response.text();
      console.error('API error body:', errorText);
      throw new Error(`Failed to load facilities: HTTP ${response.status}`);
    }
    
    const data = await response.json();
    console.log('Raw API Response:', JSON.stringify(data, null, 2));
    
    // Handle different response formats
    let facilitiesData: any[] = [];
    
    // Check if it's a FacilityPaginatedDto with 'items' property
    if (data.items && Array.isArray(data.items)) {
      facilitiesData = data.items;
      console.log('✓ Response has items property, found:', facilitiesData.length, 'facilities');
    } 
    // Check if it's a direct array
    else if (Array.isArray(data)) {
      facilitiesData = data;
      console.log('✓ Response is direct array, found:', facilitiesData.length, 'facilities');
    }
    
    if (facilitiesData.length === 0) {
      console.warn('⚠️ No facilities found in API response!');
      console.warn('Make sure you have:');
      console.warn('1. Created FacilityTypes in the database');
      console.warn('2. Created Facilities with those types');
      console.warn('3. Created PriceLists for each FacilityType');
      console.warn('Current API response structure:', Object.keys(data));
      facilities.value = [];
      return;
    }
    
    console.log('Facilities fetched successfully:', facilitiesData);
    console.log('\n=== LOADING PRICES ===');
    
    // Fetch prices for each facility type
    const priceMap = new Map<string, any>();
    
    for (const facility of facilitiesData) {
      if (facility.type?.id && !priceMap.has(facility.type.id)) {
        await loadPriceForFacilityType(facility.type.id);
        const cachedPrice = priceListCache.value.get(facility.type.id);
        if (cachedPrice) {
          priceMap.set(facility.type.id, cachedPrice);
          console.log(`💰 Price for type "${facility.type.name}" (${facility.type.id}): ${cachedPrice.pricePerHour} Kč`);
        }
      }
    }
    
    console.log('\n=== MAPPING FACILITIES WITH PRICES ===');
    
    // Map facilities and add current prices
    facilities.value = facilitiesData.map((facility: any) => {
      const typeId = facility.typeId || facility.type?.id;
      const currentPrice = getCurrentPrice(typeId);
      const mappedFacility = {
        id: facility.id,
        name: facility.name,
        typeId: typeId,
        type: facility.type || { id: '', name: '', description: '' },
        pricePerHour: currentPrice || 0,
        capacity: facility.capacity,
        isActive: facility.isActive,
        createdAt: facility.createdAt,
        image: getImageForFacilityType(facility.type?.name)
      };
      console.log(`  ✓ ${facility.name} (${facility.type?.name}): ${currentPrice} Kč/hod`);
      return mappedFacility;
    });
    
    console.log('\n✅ Final facilities array:', facilities.value);
  } catch (error: any) {
    if (error.name === 'AbortError') {
      console.error('❌ REQUEST TIMEOUT: Backend took longer than 10 seconds to respond');
      console.error('   Check if backend is running on http://localhost:5234');
    } else {
      console.error('❌ Error loading facilities:', error.message);
      console.error('   Full error:', error);
    }
    facilities.value = [];
  } finally {
    isLoadingFacilities.value = false;
  }
}

async function loadPriceForFacilityType(facilityTypeId: string) {
  try {
    const priceUrl = `${API_BASE_URL}/pricelist/${facilityTypeId}?onlyActive=true`;
    console.log(`  🔄 Fetching prices for facility type: ${facilityTypeId}`);
    
    const response = await fetch(priceUrl);
    
    if (!response.ok) {
      console.warn(`⚠️ Price API response status ${response.status} for type ${facilityTypeId}`);
      console.warn(`   URL: ${priceUrl}`);
      return;
    }
    
    const prices: PriceList[] = await response.json();
    console.log(`   Raw prices response:`, prices);
    
    if (prices.length > 0) {
      // Get the first active price (or most recent one)
      const activePrice = prices.find(p => {
        const today = new Date();
        const validFrom = new Date(p.validFrom);
        const validTo = p.validTo ? new Date(p.validTo) : null;
        const isActive = validFrom <= today && (!validTo || validTo >= today);
        console.log(`     - Price ${p.pricePerHour} Kč: validFrom=${p.validFrom}, validTo=${p.validTo}, isActive=${isActive}`);
        return isActive;
      }) || prices[0];
      
      console.log(`   ✓ Selected price: ${activePrice.pricePerHour} Kč (valid from ${activePrice.validFrom} to ${activePrice.validTo})`);
      priceListCache.value.set(facilityTypeId, activePrice);
    } else {
      console.warn(`⚠️ No prices found for facility type ${facilityTypeId}`);
    }
  } catch (error) {
    console.error(`❌ Error loading prices for facility type ${facilityTypeId}:`, error);
  }
}

function getCurrentPrice(facilityTypeId?: string): number {
  if (!facilityTypeId) {
    console.warn('   ⚠️ No facilityTypeId provided');
    return 0;
  }
  const priceEntry = priceListCache.value.get(facilityTypeId);
  if (!priceEntry) {
    console.warn(`   ⚠️ No price found in cache for type ${facilityTypeId}`);
    return 0;
  }
  console.log(`   ✓ Price from cache: ${priceEntry.pricePerHour} Kč`);
  return priceEntry.pricePerHour;
}

function getImageForFacilityType(typeName?: string): string {
  if (!typeName) return '🏟️';
  const typeMap: { [key: string]: string } = {
    'Tenis': '🎾',
    'Badminton': '🏸',
    'Volejbal': '🏐',
    'Fotbal': '⚽',
    'Hokej': '🏒'
  };
  return typeMap[typeName] || '🏟️';
}

// Load facilities on component mount
onMounted(() => {
  loadFacilities();
});


function cancelReservation(id: number) {
  const index = reservations.value.findIndex(r => r.id === id);
  if (index !== -1 && reservations.value[index]) {
    reservations.value[index].status = 'cancelled';
    // Refresh calendar to show the slot as available
    weekDays.value = getWeekDays();
  }
}

function createReservation() {
  if (!selectedFacility.value || !selectedDate.value || !selectedTime.value) {
    return;
  }

  const newReservation: Reservation = {
    id: reservations.value.length + 1,
    facilityName: selectedFacility.value.name,
    date: selectedDate.value,
    time: selectedTime.value,
    duration: selectedDuration.value,
    price: (selectedFacility.value.pricePerHour * selectedDuration.value) / 60,
    status: 'completed',
    isCurrentUser: true
  };

  reservations.value.unshift(newReservation);
  
  // Reset form
  selectedFacility.value = null;
  selectedDate.value = '';
  selectedTime.value = '';
  selectedDuration.value = 60;
  showCreateForm.value = false;
}

function selectFacility(facility: Facility) {
  selectedFacility.value = facility;
  selectedSlots.value = [];
  weekDays.value = getWeekDays();
}

function goToFacilityDetail(facilityId: string | undefined) {
  // Navigate to facility detail page using router
  if (!facilityId) return;
  const router = useRouter();
  if (router) {
    router.push({ name: 'areal-detail', params: { id: facilityId } });
  }
}

function getMinDate() {
  const today = new Date();
  return today.toISOString().split('T')[0];
}

function calculatePrice() {
  if (!selectedFacility.value) return 0;
  return (selectedFacility.value.pricePerHour * selectedDuration.value) / 60;
}

function getWeekDays(): CalendarDay[] {
  const days = [];
  const dayNames = ['Po', 'Út', 'St', 'Čt', 'Pá', 'So', 'Ne'];
  
  console.log('getWeekDays called, selectedFacility:', selectedFacility.value?.name);
  console.log('Current week start:', currentWeekStart.value);
  
  for (let i = 0; i < 7; i++) {
    const date = new Date(currentWeekStart.value);
    date.setDate(currentWeekStart.value.getDate() + i);
    
    const dateStr = date.toLocaleDateString('cs-CZ', { day: '2-digit', month: '2-digit', year: 'numeric' });
    const slots: CalendarSlot[] = [];
    
    hours.value.forEach(hour => {
      const timeStr = `${hour.toString().padStart(2, '0')}:00`;
      
      // Check if this slot is occupied by existing reservations
      const occupiedReservation = reservations.value.find(reservation => {
        if (reservation.status === 'cancelled') return false;
        
        // Don't filter by facility when no facility is selected - show all reservations
        if (selectedFacility.value && reservation.facilityName !== selectedFacility.value.name) return false;
        
        // Check if this time slot falls within a reservation
        const reservationStart = new Date(`${reservation.date} ${reservation.time}`);
        const reservationEnd = new Date(reservationStart.getTime() + reservation.duration * 60000);
        const slotTime = new Date(`${dateStr} ${timeStr}`);
        const slotEndTime = new Date(slotTime.getTime() + 60 * 60000); // 1 hour slot
        
        const isMatch = reservation.date === dateStr && 
                       slotTime < reservationEnd && 
                       slotEndTime > reservationStart;
        
        // Debug logging for specific dates
        if (dateStr === '08.04.2026' && timeStr === '14:00') {
          console.log('Checking slot 08.04.2026 14:00:', {
            reservation,
            isMatch,
            facilityMatch: !selectedFacility.value || reservation.facilityName === selectedFacility.value.name,
            dateMatch: reservation.date === dateStr,
            timeOverlap: slotTime < reservationEnd && slotEndTime > reservationStart,
            selectedFacility: selectedFacility.value?.name
          });
        }
        
        return isMatch;
      });
      
      const isOccupied = !!occupiedReservation;
      const isSelected = selectedSlots.value.some(slot => slot.date === dateStr && slot.time === timeStr);
      const isHighlighted = highlightedSlot.value?.date === dateStr && highlightedSlot.value?.time === timeStr;
      
      slots.push({
        time: timeStr,
        occupied: isOccupied,
        selected: isSelected,
        isCurrentUser: occupiedReservation?.isCurrentUser || false,
        highlighted: isHighlighted
      });
    });
    
    days.push({
      date: dateStr,
      name: dayNames[i] || '',
      slots
    });
  }
  
  return days;
}

const weekDays = ref(getWeekDays());

function formattedDateRange() {
  const start = new Date(currentWeekStart.value);
  const end = new Date(currentWeekStart.value);
  end.setDate(start.getDate() + 6);
  
  const formatDate = (date: Date) => {
    return date.toLocaleDateString('cs-CZ', { day: '2-digit', month: '2-digit', year: 'numeric' });
  };
  
  return `${formatDate(start)} - ${formatDate(end)}`;
}

function prevWeek() {
  const newDate = new Date(currentWeekStart.value);
  newDate.setDate(newDate.getDate() - 7);
  currentWeekStart.value = newDate;
  weekDays.value = getWeekDays();
}

function nextWeek() {
  const newDate = new Date(currentWeekStart.value);
  newDate.setDate(newDate.getDate() + 7);
  currentWeekStart.value = newDate;
  weekDays.value = getWeekDays();
}

function toggleSlot(date: string, time: string) {
  const slotIndex = selectedSlots.value.findIndex(slot => slot.date === date && slot.time === time);
  
  // Check if this slot is occupied by any user (current or other)
  const occupiedReservation = reservations.value.find(reservation => 
    reservation.status === 'completed' && 
    reservation.date === date && 
    reservation.time === time
  );
  
  if (occupiedReservation) {
    if (occupiedReservation.isCurrentUser) {
      // Cancel the current user's reservation
      cancelReservation(occupiedReservation.id);
    }
    // If occupied by another user, do nothing (can't select)
    return;
  }
  
  if (slotIndex !== -1) {
    selectedSlots.value.splice(slotIndex, 1);
  } else {
    selectedSlots.value.push({ date, time });
  }
  
  // Update the weekDays to reflect selection
  weekDays.value = getWeekDays();
}

function confirmReservation() {
  if (selectedSlots.value.length === 0 || !selectedFacility.value) return;
  
  // Create separate reservation for each selected slot
  selectedSlots.value.forEach(slot => {
    const newReservation: Reservation = {
      id: reservations.value.length + Math.floor(Math.random() * 1000), // Generate unique ID
      facilityName: selectedFacility.value!.name,
      date: slot.date,
      time: slot.time,
      duration: 60, // Each slot is 1 hour
      price: selectedFacility.value!.pricePerHour,
      status: 'completed',
      isCurrentUser: true
    };
    
    reservations.value.unshift(newReservation);
  });
  
  // Reset form
  selectedSlots.value = [];
  // Don't reset selectedFacility to keep same facility selected
  // showCreateForm.value = false; // Keep calendar visible
  // activeTab.value = 'create'; // Stay on create tab
  
  // Refresh calendar to show new reservations
  weekDays.value = getWeekDays();
}

function getStatusColor(status: string) {
  switch (status) {
    case 'completed': return '#2196F3';
    case 'cancelled': return '#F44336';
    default: return '#757575';
  }
}

function getStatusText(status: string) {
  switch (status) {
    case 'completed': return 'Dokončeno';
    case 'cancelled': return 'Zrušeno';
    default: return 'Neznámý';
  }
}

function navigateToReservation(reservation: Reservation) {
  console.log('navigateToReservation called with:', reservation);
  
  // Find the facility for this reservation
  const facility = facilities.value.find(f => f.name === reservation.facilityName);
  if (!facility) {
    console.log('Facility not found:', reservation.facilityName);
    return;
  }
  
  console.log('Found facility:', facility);
  
  // Select the facility
  selectedFacility.value = facility;
  
  // Parse the reservation date to navigate to the correct week
  const dateParts = reservation.date.split('.').map(part => parseInt(part));
  const day = dateParts[0];
  const month = dateParts[1];
  const year = dateParts[2];
  
  if (!day || !month || !year) {
    console.log('Invalid date format:', reservation.date);
    return;
  }
  
  const reservationDate = new Date(year, month - 1, day);
  
  console.log('Parsed date:', { day, month, year, reservationDate });
  
  // Calculate the start of the week containing this reservation
  const dayOfWeek = reservationDate.getDay();
  const weekStart = new Date(reservationDate);
  // Adjust to Monday (or Sunday if you prefer)
  const diff = dayOfWeek === 0 ? -6 : 1 - dayOfWeek;
  weekStart.setDate(reservationDate.getDate() + diff);
  
  console.log('Week start calculated:', { dayOfWeek, diff, weekStart });
  
  // Set the current week to show the reservation
  currentWeekStart.value = weekStart;
  
  // Refresh the calendar
  weekDays.value = getWeekDays();
  
  // Highlight the specific slot with brown color
  highlightedSlot.value = { date: reservation.date, time: reservation.time };
  
  // Clear highlight after 5 seconds
  setTimeout(() => {
    highlightedSlot.value = null;
  }, 5000);
}
</script>

<template>
  <NavBar></NavBar>
  <div class="reservations-container">

    <div class="reservations-list">
      <!-- Create Reservation Tab -->
      <div class="create-reservation-layout">
        <div class="calendar-view">
          <div class="facility-selector">
            <label class="selector-label">Vyberte sportoviště:</label>
            <select v-model="selectedFacility" @change="selectedFacility && selectFacility(selectedFacility)" class="facility-dropdown" :disabled="isLoadingFacilities">
              <option 
                v-if="isLoadingFacilities" 
                disabled 
                selected
              >
                Načítám sportoviště...
              </option>
              <option 
                v-else-if="facilities.length === 0" 
                disabled 
                selected
              >
                Žádná sportoviště nejsou k dispozici
              </option>
              <option v-for="facility in facilities" :key="facility.id" :value="facility">
                {{ facility.name }} - {{ facility.pricePerHour }} Kč/hod
              </option>
            </select>
            <button @click="goToFacilityDetail(selectedFacility?.id)" class="detail-button" :disabled="!selectedFacility">
              Detail sportoviště →
            </button>
          </div>
          <div v-if="!isLoadingFacilities && facilities.length === 0" class="error-message-box" style="margin-bottom: 2rem;">
            💡 <strong>Nemám přístup k sportoviště.</strong> Zkontrolujte prosím:
            <ul style="margin: 0.5rem 0 0 1.5rem; padding: 0;">
              <li>Zda je backend spuštěn na http://localhost:5234</li>
              <li>Zda jsou v databázi vytvořena sportoviště a jejich typy</li>
              <li>Otevřete F12 konzolu (DevTools) pro podrobné diagnostické zprávy</li>
            </ul>
          </div>

          <div class="facility-info-banner" v-if="selectedFacility">
            <div class="facility-banner-content">
              <div class="facility-icon">{{ selectedFacility.image }}</div>
              <div class="facility-details">
                <h3>{{ selectedFacility.name }}</h3>
                <p>{{ selectedFacility.type?.name }} • {{ selectedFacility.pricePerHour }} Kč/hod</p>
                <p class="description">{{ selectedFacility.type?.description }}</p>
              </div>
            </div>
          </div>

          <div class="calendar-header">
            <button @click="prevWeek" class="nav-button">←</button>
            <h3>{{ formattedDateRange() }}</h3>
            <button @click="nextWeek" class="nav-button">→</button>
          </div>
          <div class="calendar-grid">
            <div class="time-column">
              <div class="time-slot-header"></div>
              <div v-for="hour in hours" :key="hour" class="time-slot">{{ hour }}:00</div>
            </div>
            <div v-for="day in weekDays" :key="day.date" class="day-column">
              <div class="day-header">
                <span class="day-name">{{ day.name }}</span>
                <span class="day-date">{{ day.date }}</span>
              </div>
              <div class="slots-container">
                <div 
                  v-for="slot in day.slots"
                  :key="slot.time"
                  :class="['calendar-slot', { occupied: slot.occupied, free: !slot.occupied, selected: slot.selected, 'current-user': slot.isCurrentUser, 'confirmed-reservation': slot.occupied && slot.isCurrentUser, highlighted: slot.highlighted }]"
                  @click="toggleSlot(day.date, slot.time)"
                >
                </div>
              </div>
            </div>
          </div>
          <div class="legend">
            <div class="legend-item"><span class="color-box occupied"></span> Obsazeno ostatními</div>
            <div class="legend-item"><span class="color-box occupied current-user"></span> Vaše rezervace</div>
            <div class="legend-item"><span class="color-box free"></span> Volné</div>
            <div class="legend-item"><span class="color-box selected"></span> Vámi vybrané</div>
            <div class="legend-item"><span class="color-box highlighted"></span> Zvýrazněná rezervace</div>
          </div>
          <div class="selected-summary" v-if="selectedSlots.length > 0">
            <h4>Vybrané sloty: {{ selectedSlots.length }}</h4>
            <p>Celková cena: {{ selectedSlots.length * (selectedFacility?.pricePerHour || 0) }} Kč</p>
          </div>
          <div class="form-actions">
            <button @click="confirmReservation" class="create-button" :disabled="selectedSlots.length === 0">
              Potvrdit vybranou rezervaci
            </button>
          </div>
        </div>
        
        <!-- Side panel for own reservations only -->
        <div class="completed-reservations-side">
          <h3>Moje rezervace</h3>
          
          <!-- Filter by date -->
          <div class="filter-section">
            <label class="filter-label">Filtrovat podle dne:</label>
            <select v-model="reservationFilterDate" class="date-filter">
              <option value="">Všechny dny</option>
              <option v-for="date in [...new Set(reservations.filter(r => r.status === 'completed' && r.isCurrentUser).map(r => r.date))]" 
                      :key="date" :value="date">
                {{ date }}
              </option>
            </select>
          </div>
          
          <div class="reservation-cards-side">
            <div v-for="reservation in filteredReservations" :key="reservation.id" 
                 class="reservation-card-side">
              <div class="card-header">
                <h4 class="facility-name">{{ reservation.facilityName }}</h4>
                <span class="status-badge" :style="{ backgroundColor: getStatusColor(reservation.status) }">
                  {{ getStatusText(reservation.status) }}
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

              <div class="card-actions">
                <button @click="navigateToReservation(reservation)" class="navigate-button">
                  Zobrazit v kalendáři
                </button>
                <button @click="cancelReservation(reservation.id)" class="cancel-button">
                  Zrušit
                </button>
              </div>
            </div>
          </div>
        </div>
      </div>

    </div>
  </div>
</template>

<style scoped>
.reservations-container {
  max-width: 1600px;
  margin: 0 auto;
  padding: 6rem 1.5rem 2rem;
}

.reservations-header {
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
  color: var(--vt-c-yellow);
  border-bottom-color: var(--vt-c-yellow);
}

.reservations-list {
  min-height: 400px;
}

.empty-state {
  text-align: center;
  padding: 4rem 2rem;
  color: var(--color-text);
}

.empty-icon {
  font-size: 4rem;
  margin-bottom: 1rem;
  opacity: 0.5;
}

.empty-state h3 {
  font-size: 1.5rem;
  margin-bottom: 0.5rem;
  color: var(--color-heading);
}

.empty-state p {
  margin-bottom: 2rem;
  opacity: 0.8;
}

.cta-button {
  display: inline-block;
  padding: 0.75rem 2rem;
  background-color: var(--vt-c-yellow);
  color: var(--color-heading);
  text-decoration: none;
  border-radius: 0.5rem;
  font-weight: 600;
  transition: background-color 0.3s ease;
}

.cta-button:hover {
  background-color: var(--vt-c-yellow-light);
}

.reservation-cards {
  display: grid;
  gap: 1.5rem;
  grid-template-columns: repeat(auto-fill, minmax(350px, 1fr));
}

.reservation-card {
  background: var(--vt-c-white-soft);
  border: 1px solid var(--vt-c-divider);
  border-radius: 1rem;
  padding: 1.5rem;
  transition: all 0.3s ease;
}

.reservation-card:hover {
  transform: translateY(-2px);
  box-shadow: 0 8px 25px rgba(0, 0, 0, 0.1);
}

.card-header {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  margin-bottom: 1rem;
}

.facility-name {
  font-size: 1.25rem;
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

.card-details {
  display: flex;
  flex-direction: column;
  gap: 0.75rem;
  margin-bottom: 1.5rem;
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

.card-actions {
  display: flex;
  gap: 1rem;
  padding-top: 1rem;
  border-top: 1px solid var(--vt-c-divider);
}

.cancel-button, .modify-button {
  flex: 1;
  padding: 0.75rem 1rem;
  border: none;
  border-radius: 0.5rem;
  font-weight: 500;
  text-align: center;
  text-decoration: none;
  cursor: pointer;
  transition: all 0.3s ease;
}

.cancel-button {
  background-color: #F44336;
  color: white;
}

.cancel-button:hover {
  background-color: #D32F2F;
}

.modify-button {
  background-color: var(--vt-c-white-mute);
  color: var(--color-heading);
  border: 1px solid var(--vt-c-divider);
}

.modify-button:hover {
  background-color: var(--vt-c-white-soft);
}

/* Calendar Styles */
.create-reservation-layout {
  display: grid;
  grid-template-columns: 1fr 350px;
  gap: 2rem;
  align-items: start;
}

.calendar-view {
  width: 100%;
}

.calendar-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 2rem;
  padding: 1rem;
  background-color: var(--vt-c-white-soft);
  border-radius: 1rem;
  border: 1px solid var(--vt-c-divider);
}

.calendar-header h3 {
  font-size: 1.5rem;
  font-weight: 600;
  color: var(--color-heading);
}

.nav-button {
  background: var(--vt-c-white-mute);
  border: 2px solid var(--vt-c-divider);
  border-radius: 0.75rem;
  padding: 0.75rem 1.5rem;
  cursor: pointer;
  transition: all 0.3s ease;
  font-weight: 600;
  font-size: 1rem;
}

.nav-button:hover {
  background-color: var(--vt-c-yellow);
  border-color: var(--vt-c-yellow);
  transform: translateY(-2px);
}

.calendar-grid {
  display: grid;
  grid-template-columns: 120px repeat(7, 1fr);
  gap: 2px;
  background-color: var(--vt-c-divider);
  border: 2px solid var(--vt-c-divider);
  border-radius: 1rem;
  overflow: hidden;
  box-shadow: 0 4px 20px rgba(0, 0, 0, 0.1);
  width: 100%;
}

.time-column {
  background-color: var(--vt-c-white-soft);
}

.time-slot-header {
  height: 80px;
  border-bottom: 2px solid var(--vt-c-divider);
}

.time-slot {
  height: 60px;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 1.1rem;
  font-weight: 600;
  color: var(--color-heading);
  border-bottom: 1px solid var(--vt-c-divider);
  background-color: var(--vt-c-white-mute);
}

.day-column {
  background-color: var(--vt-c-white-soft);
}

.day-header {
  height: 90px;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  border-bottom: 2px solid var(--vt-c-divider);
  background-color: var(--vt-c-white-mute);
}

.day-name {
  font-weight: 700;
  color: var(--color-heading);
  font-size: 1.2rem;
  margin-bottom: 0.25rem;
}

.day-date {
  font-size: 0.95rem;
  color: var(--color-text);
  opacity: 0.8;
  font-weight: 500;
}

.slots-container {
  display: flex;
  flex-direction: column;
}

.calendar-slot {
  height: 60px;
  border-bottom: 1px solid var(--vt-c-divider);
  cursor: pointer;
  transition: all 0.3s ease;
  position: relative;
}

.calendar-slot.free {
  background-color: white;
}

.calendar-slot.free:hover {
  background-color: var(--vt-c-yellow-light);
  transform: scale(1.02);
  box-shadow: inset 0 0 0 2px var(--vt-c-yellow);
}

.calendar-slot.occupied:not(.current-user) {
  background-color: #FF5252;
  cursor: not-allowed;
  position: relative;
}

.calendar-slot.occupied:not(.current-user)::after {
  content: '×';
  position: absolute;
  top: 50%;
  left: 50%;
  transform: translate(-50%, -50%);
  color: white;
  font-size: 1.5rem;
  font-weight: bold;
}

.calendar-slot.occupied.current-user {
  background-color: #2196F3;
  cursor: pointer;
  position: relative;
}

.calendar-slot.occupied.current-user::after {
  content: '✓';
  position: absolute;
  top: 50%;
  left: 50%;
  transform: translate(-50%, -50%);
  color: white;
  font-size: 1.5rem;
  font-weight: bold;
}

.calendar-slot.occupied.current-user:hover {
  background-color: #1976D2;
  transform: scale(1.02);
  box-shadow: inset 0 0 0 2px #F44336;
}

.completed-reservations-side {
  background: var(--vt-c-white-soft);
  border: 1px solid var(--vt-c-divider);
  border-radius: 1rem;
  padding: 1.5rem;
  max-height: 80vh;
  overflow-y: auto;
}

.completed-reservations-side h3 {
  font-size: 1.25rem;
  font-weight: 600;
  color: var(--color-heading);
  margin-bottom: 1rem;
  text-align: center;
}

.filter-section {
  margin-bottom: 1rem;
  padding: 0.75rem;
  background-color: var(--vt-c-white-mute);
  border-radius: 0.5rem;
  border: 1px solid var(--vt-c-divider);
}

.filter-label {
  display: block;
  font-size: 0.9rem;
  font-weight: 500;
  color: var(--color-heading);
  margin-bottom: 0.5rem;
}

.date-filter {
  width: 100%;
  padding: 0.5rem;
  border: 1px solid var(--vt-c-divider);
  border-radius: 0.25rem;
  background-color: white;
  font-size: 0.9rem;
  color: var(--color-text);
}

.reservation-cards-side {
  display: flex;
  flex-direction: column;
  gap: 1rem;
}

.reservation-card-side {
  background: white;
  border: 1px solid var(--vt-c-divider);
  border-radius: 0.75rem;
  padding: 1rem;
  transition: all 0.3s ease;
}

.reservation-card-side:hover {
  transform: translateY(-1px);
  box-shadow: 0 4px 15px rgba(0, 0, 0, 0.1);
}

.reservation-card-side .card-header {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  margin-bottom: 0.75rem;
}

.reservation-card-side .facility-name {
  font-size: 1rem;
  font-weight: 600;
  color: var(--color-heading);
  margin: 0;
}

.reservation-card-side .status-badge {
  color: white;
  padding: 0.25rem 0.5rem;
  border-radius: 0.5rem;
  font-size: 0.75rem;
  font-weight: 500;
}

.reservation-card-side .card-details {
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
  margin-bottom: 1rem;
}

.reservation-card-side .detail-row {
  display: flex;
  align-items: center;
  gap: 0.5rem;
}

.reservation-card-side .detail-icon {
  font-size: 0.9rem;
  opacity: 0.7;
}

.reservation-card-side .detail-text {
  color: var(--color-text);
  font-weight: 500;
  font-size: 0.9rem;
}

.navigate-button {
  flex: 1;
  padding: 0.5rem 0.75rem;
  background-color: #2196F3;
  color: white;
  border: none;
  border-radius: 0.5rem;
  font-weight: 500;
  font-size: 0.875rem;
  cursor: pointer;
  transition: all 0.3s ease;
}

.navigate-button:hover {
  background-color: #1976D2;
}

.reservation-card-side .card-actions {
  display: flex;
  gap: 0.5rem;
}

.reservation-card-side .cancel-button {
  flex: 1;
  padding: 0.5rem 0.75rem;
  background-color: #F44336;
  color: white;
  border: none;
  border-radius: 0.5rem;
  font-weight: 500;
  font-size: 0.875rem;
  cursor: pointer;
  transition: all 0.3s ease;
}

.reservation-card-side .cancel-button:hover {
  background-color: #D32F2F;
}

.color-box.occupied.current-user {
  background-color: #2196F3;
}

.color-box.occupied.current-user::after {
  content: '✓';
  position: absolute;
  top: 50%;
  left: 50%;
  transform: translate(-50%, -50%);
  color: white;
  font-size: 0.8rem;
  font-weight: bold;
}

.calendar-slot.selected {
  background-color: #4CAF50;
  position: relative;
}

.calendar-slot.selected::after {
  content: '✓';
  position: absolute;
  top: 50%;
  left: 50%;
  transform: translate(-50%, -50%);
  color: white;
  font-size: 1.2rem;
  font-weight: bold;
}

.calendar-slot.highlighted {
  background-color: #8D6E63 !important; /* Brown color */
  position: relative;
  border: 3px solid #5D4037 !important; /* Darker brown border */
  box-shadow: 0 0 10px rgba(141, 110, 99, 0.5);
}

.calendar-slot.highlighted::after {
  content: '📍';
  position: absolute;
  top: 50%;
  left: 50%;
  transform: translate(-50%, -50%);
  color: white;
  font-size: 1.2rem;
  font-weight: bold;
}

.calendar-slot.selected:hover {
  background-color: #45a049;
  transform: scale(1.02);
  box-shadow: inset 0 0 0 2px #2e7d32;
}

.legend {
  display: flex;
  justify-content: center;
  gap: 3rem;
  margin-top: 2rem;
  padding: 1.5rem;
  background-color: var(--vt-c-white-soft);
  border-radius: 1rem;
  border: 1px solid var(--vt-c-divider);
}

.legend-item {
  display: flex;
  align-items: center;
  gap: 0.75rem;
  font-size: 1rem;
  color: var(--color-text);
  font-weight: 500;
}

.color-box {
  width: 24px;
  height: 24px;
  border-radius: 0.5rem;
  border: 2px solid var(--vt-c-divider);
}

.color-box.occupied {
  background-color: #FF5252;
  position: relative;
}

.color-box.occupied::after {
  content: '×';
  position: absolute;
  top: 50%;
  left: 50%;
  transform: translate(-50%, -50%);
  color: white;
  font-size: 0.9rem;
  font-weight: bold;
}

.color-box.free {
  background-color: white;
}

.color-box.selected {
  background-color: #4CAF50;
  position: relative;
}

.color-box.selected::after {
  content: '✓';
  position: absolute;
  top: 50%;
  left: 50%;
  transform: translate(-50%, -50%);
  color: white;
  font-size: 0.8rem;
  font-weight: bold;
}

.color-box.highlighted {
  background-color: #8D6E63;
  border: 2px solid #5D4037;
  position: relative;
}

.color-box.highlighted::after {
  content: '📍';
  position: absolute;
  top: 50%;
  left: 50%;
  transform: translate(-50%, -50%);
  color: white;
  font-size: 0.8rem;
  font-weight: bold;
}

.facility-info-banner {
  background: linear-gradient(135deg, var(--vt-c-yellow-light), var(--vt-c-yellow));
  border-radius: 1rem;
  padding: 1.5rem;
  margin-bottom: 2rem;
  border: 2px solid var(--vt-c-yellow);
}

.facility-banner-content {
  display: flex;
  align-items: center;
  gap: 1.5rem;
}

.facility-icon {
  font-size: 3rem;
  background: white;
  width: 80px;
  height: 80px;
  border-radius: 1rem;
  display: flex;
  align-items: center;
  justify-content: center;
  box-shadow: 0 4px 15px rgba(0, 0, 0, 0.1);
}

.facility-details h3 {
  font-size: 1.5rem;
  font-weight: 700;
  color: var(--color-heading);
  margin-bottom: 0.5rem;
}

.facility-details p.description {
  font-size: 1rem;
  color: var(--color-text);
  opacity: 0.8;
  margin-top: 0.5rem;
  font-weight: 400;
}

.facility-selector {
  display: flex;
  align-items: center;
  gap: 1rem;
  margin-bottom: 2rem;
  padding: 1.5rem;
  background: var(--vt-c-white-soft);
  border-radius: 1rem;
  border: 1px solid var(--vt-c-divider);
}

.selector-label {
  font-weight: 600;
  color: var(--color-heading);
  font-size: 1.1rem;
  white-space: nowrap;
}

.facility-dropdown {
  flex: 1;
  padding: 0.75rem 1rem;
  border: 2px solid var(--vt-c-divider);
  border-radius: 0.75rem;
  background-color: white;
  color: var(--color-heading);
  font-size: 1rem;
  font-weight: 500;
  cursor: pointer;
  transition: all 0.3s ease;
}

.facility-dropdown:focus {
  outline: none;
  border-color: var(--vt-c-yellow);
  box-shadow: 0 0 0 3px rgba(255, 193, 7, 0.2);
}

.facility-dropdown:disabled {
  background-color: var(--vt-c-white-mute);
  color: var(--color-text);
  opacity: 0.6;
  cursor: not-allowed;
  border-color: var(--vt-c-divider);
}

.detail-button {
  padding: 0.75rem 1.5rem;
  background: linear-gradient(135deg, var(--vt-c-yellow), var(--vt-c-yellow-light));
  color: var(--color-heading);
  border: none;
  border-radius: 0.75rem;
  font-weight: 600;
  font-size: 1rem;
  cursor: pointer;
  transition: all 0.3s ease;
  white-space: nowrap;
}

.detail-button:hover {
  transform: translateY(-2px);
  box-shadow: 0 4px 15px rgba(0, 0, 0, 0.1);
}

.detail-button:disabled {
  background: var(--vt-c-white-mute);
  color: var(--color-text);
  opacity: 0.6;
  cursor: not-allowed;
  border: none;
}

.error-message-box {
  color: #F44336;
  font-size: 0.95rem;
  padding: 1rem;
  background-color: rgba(244, 67, 54, 0.1);
  border-radius: 0.75rem;
  border-left: 4px solid #F44336;
}

.selected-summary {
  background: var(--vt-c-white-soft);
  border: 2px solid var(--vt-c-yellow);
  border-radius: 1rem;
  padding: 1.5rem;
  margin: 2rem 0;
  text-align: center;
}

.selected-summary h4 {
  font-size: 1.25rem;
  font-weight: 600;
  color: var(--color-heading);
  margin-bottom: 0.5rem;
}

.selected-summary p {
  font-size: 1.1rem;
  color: var(--color-text);
  font-weight: 500;
}

.create-button {
  padding: 1rem 3rem;
  background: linear-gradient(135deg, var(--vt-c-yellow), var(--vt-c-yellow-light));
  color: var(--color-heading);
  border: none;
  border-radius: 1rem;
  font-size: 1.2rem;
  font-weight: 700;
  cursor: pointer;
  transition: all 0.3s ease;
  box-shadow: 0 4px 15px rgba(0, 0, 0, 0.1);
}

.create-button:hover:not(:disabled) {
  transform: translateY(-3px);
  box-shadow: 0 8px 25px rgba(0, 0, 0, 0.15);
}

.create-button:disabled {
  opacity: 0.5;
  cursor: not-allowed;
  transform: none;
}

@media (max-width: 768px) {
  .reservations-container {
    padding: 6rem 1rem 2rem;
  }
  
  .create-reservation-layout {
    grid-template-columns: 1fr;
    gap: 1rem;
  }
  
  .completed-reservations-side {
    max-height: 400px;
    order: 2;
  }
  
  .calendar-view {
    order: 1;
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
  
  .reservation-cards {
    grid-template-columns: 1fr;
  }
  
  .card-actions {
    flex-direction: column;
  }
  
  .facilities-grid {
    grid-template-columns: 1fr;
  }
  
  .calendar-grid {
    grid-template-columns: 80px repeat(7, 1fr);
    gap: 1px;
  }
  
  .time-slot {
    height: 45px;
    font-size: 0.9rem;
  }
  
  .calendar-slot {
    height: 45px;
  }
  
  .day-header {
    height: 70px;
  }
  
  .day-name {
    font-size: 1rem;
  }
  
  .day-date {
    font-size: 0.8rem;
  }
  
  .calendar-header h3 {
    font-size: 1.25rem;
  }
  
  .nav-button {
    padding: 0.5rem 1rem;
    font-size: 0.875rem;
  }
  
  .facility-selector {
    flex-direction: column;
    align-items: stretch;
    gap: 1rem;
  }
  
  .detail-button {
    align-self: flex-end;
  }
}

/* Create Reservation Styles */
.create-reservation {
  min-height: 500px;
}

.facilities-selection h2 {
  font-size: 1.5rem;
  font-weight: 600;
  color: var(--color-heading);
  margin-bottom: 2rem;
  text-align: center;
}

.facilities-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(300px, 1fr));
  gap: 1.5rem;
}

.facility-card {
  background: var(--vt-c-white-soft);
  border: 2px solid var(--vt-c-divider);
  border-radius: 1rem;
  padding: 1.5rem;
  cursor: pointer;
  transition: all 0.3s ease;
  position: relative;
}

.facility-card:hover:not(.unavailable) {
  transform: translateY(-2px);
  border-color: var(--vt-c-yellow);
  box-shadow: 0 8px 25px rgba(0, 0, 0, 0.1);
}

.facility-card.unavailable {
  opacity: 0.6;
  cursor: not-allowed;
}

.facility-icon {
  font-size: 3rem;
  text-align: center;
  margin-bottom: 1rem;
}

.facility-info h3 {
  font-size: 1.25rem;
  font-weight: 600;
  color: var(--color-heading);
  margin-bottom: 0.5rem;
}

.facility-info p {
  color: var(--color-text);
  margin-bottom: 0.5rem;
}

.description {
  font-size: 0.9rem;
  opacity: 0.8;
  margin-bottom: 1rem;
}

.facility-price {
  font-size: 1.25rem;
  font-weight: 600;
  color: var(--vt-c-yellow);
}

.unavailable-badge {
  position: absolute;
  top: 1rem;
  right: 1rem;
  background-color: #F44336;
  color: white;
  padding: 0.25rem 0.75rem;
  border-radius: 1rem;
  font-size: 0.875rem;
  font-weight: 500;
}

.reservation-form {
  max-width: 800px;
  margin: 0 auto;
}

.form-header {
  display: flex;
  align-items: center;
  gap: 1rem;
  margin-bottom: 2rem;
}

.back-button {
  background: none;
  border: none;
  color: var(--color-text);
  cursor: pointer;
  font-size: 1rem;
  padding: 0.5rem;
  border-radius: 0.5rem;
  transition: background-color 0.3s ease;
}

.back-button:hover {
  background-color: var(--vt-c-white-mute);
}

.form-header h2 {
  font-size: 1.5rem;
  font-weight: 600;
  color: var(--color-heading);
}

.form-content {
  display: flex;
  flex-direction: column;
  gap: 2rem;
}

.form-section h3 {
  font-size: 1.25rem;
  font-weight: 600;
  color: var(--color-heading);
  margin-bottom: 1rem;
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
  background-color: var(--vt-c-white-soft);
  color: var(--color-text);
  transition: all 0.3s ease;
}

.form-input:focus {
  outline: none;
  border-color: var(--vt-c-yellow);
  background-color: white;
}

.duration-options {
  display: grid;
  grid-template-columns: repeat(4, 1fr);
  gap: 1rem;
}

.duration-button {
  padding: 1rem;
  border: 2px solid var(--vt-c-divider);
  border-radius: 0.5rem;
  background-color: var(--vt-c-white-soft);
  cursor: pointer;
  transition: all 0.3s ease;
  font-weight: 500;
}

.duration-button:hover {
  border-color: var(--vt-c-yellow);
}

.duration-button.active {
  background-color: var(--vt-c-yellow);
  border-color: var(--vt-c-yellow);
  color: var(--color-heading);
}

.reservation-summary {
  background-color: var(--vt-c-white-mute);
  border-radius: 1rem;
  padding: 1.5rem;
  border-left: 4px solid var(--vt-c-yellow);
}

.summary-row {
  display: flex;
  justify-content: space-between;
  padding: 0.5rem 0;
  border-bottom: 1px solid var(--vt-c-divider);
}

.summary-row:last-child {
  border-bottom: none;
}

.summary-row.total {
  font-weight: 600;
  font-size: 1.1rem;
  color: var(--color-heading);
  padding-top: 1rem;
  border-top: 2px solid var(--vt-c-divider);
}

.form-actions {
  display: flex;
  justify-content: center;
  margin-top: 2rem;
}

.create-button {
  padding: 1rem 3rem;
  background-color: var(--vt-c-yellow);
  color: var(--color-heading);
  border: none;
  border-radius: 0.5rem;
  font-size: 1.1rem;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.3s ease;
}

.create-button:hover:not(:disabled) {
  background-color: var(--vt-c-yellow-light);
  transform: translateY(-2px);
}

.create-button:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}
</style>
