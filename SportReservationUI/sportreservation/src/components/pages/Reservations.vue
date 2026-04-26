<script setup lang="ts">
import '@/assets/main.css';
import NavBar from "../views/NavBar.vue";
import { ref, computed, onMounted } from 'vue';
import { useRoute } from 'vue-router';
import { API_URL, secureFetch } from '../../auth';
import type { FacilityComplexDto, ReservationDto, DowntimeDto, CreateReservationDto } from '../../lib/sportApi';
import { formatDate, formatDateTime, buildDayRange, startOfDay, addHours, toIsoLocal, overlaps } from '../../lib/sportApi';

interface CalendarSlot {
  time: string;
  occupied: boolean;
  selected: boolean;
  isCurrentUser?: boolean;
  isOtherUser?: boolean;
  isDowntime?: boolean;
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
  id: string;
  userId: string;
  facilityId: string;
  facilityName: string;
  date: string;
  time: string;
  duration: number;
  price: number;
  status: 'active' | 'cancelled';
  isCurrentUser?: boolean;
  startAt: string;
  endAt: string;
}

const reservations = ref<Reservation[]>([]);
const downtimes = ref<DowntimeDto[]>([]);
const currentUserId = ref<string | null>(null);
const isLoadingReservations = ref(false);
const isLoadingDowntimes = ref(false);

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
const currentWeekStart = ref(startOfWeekMonday(new Date()));
const selectedSlots = ref<Array<{date: string, time: string}>>([]);
const hours = ref([6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19]);
const reservationFilterDate = ref('');
const highlightedSlot = ref<{date: string, time: string} | null>(null);
const showFacilityDetailModal = ref(false);

// Computed property for filtered reservations
const filteredReservations = computed(() => {
  let filtered = reservations.value.filter(r => r.status === 'active' && r.isCurrentUser);
  
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
      
      if (activePrice) {
        console.log(`   ✓ Selected price: ${activePrice.pricePerHour} Kč (valid from ${activePrice.validFrom} to ${activePrice.validTo})`);
        priceListCache.value.set(facilityTypeId, activePrice);
      }
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

const weekdayNames = ['Po', 'Út', 'St', 'Čt', 'Pá', 'So', 'Ne'];

function startOfWeekMonday(value: Date): Date {
  const date = new Date(value);
  const day = date.getDay();
  const diff = day === 0 ? -6 : 1 - day;
  date.setDate(date.getDate() + diff);
  date.setHours(0, 0, 0, 0);
  return date;
}

const route = useRoute();

// Load facilities on component mount
onMounted(async () => {
  await loadCurrentUserReservations();
  await loadFacilities();
  
  // Check if facility_id is in query params
  const facilityId = route.query.facility_id as string;
  if (facilityId) {
    const facility = facilities.value.find(f => f.id === facilityId);
    if (facility) {
      selectedFacility.value = facility;
      await loadReservationsForFacility(facilityId);
      await loadDowntimesForFacility(facilityId);
      await loadCurrentUserReservations();
    }
  }
});

async function loadReservationsForFacility(facilityId: string) {
  try {
    isLoadingReservations.value = true;
    const response = await secureFetch(`${API_URL}/Reservation?facility_id=${facilityId}`);
    if (response.ok) {
      const data: ReservationDto[] = await response.json();
      reservations.value = data.map(r => ({
        id: r.id,
        userId: r.userId,
        facilityId: r.facilityId,
        facilityName: facilities.value.find(f => f.id === r.facilityId)?.name || 'Unknown',
        date: formatDate(r.startAt),
        time: formatDateTime(r.startAt).split(' ')[1] || '',
        duration: Math.round((new Date(r.endAt).getTime() - new Date(r.startAt).getTime()) / 60000),
        price: r.finalPrice,
        status: r.status.toLowerCase() as 'active' | 'cancelled',
        isCurrentUser: r.userId === currentUserId.value,
        startAt: r.startAt,
        endAt: r.endAt
      }));
      // Výpis všech rezervací pro kontrolu
      console.log('RESERVATIONS FOR FACILITY', facilityId, JSON.stringify(reservations.value, null, 2));
      weekDays.value = getWeekDays();
    }
  } catch (error) {
    console.error('Error loading reservations:', error);
  } finally {
    isLoadingReservations.value = false;
  }
}

async function loadDowntimesForFacility(facilityId: string) {
  try {
    isLoadingDowntimes.value = true;
    const response = await secureFetch(`/Downtime/facility/${facilityId}`);
    if (response.ok) {
      downtimes.value = await response.json();
      weekDays.value = getWeekDays();
    }
  } catch (error) {
    console.error('Error loading downtimes:', error);
  } finally {
    isLoadingDowntimes.value = false;
  }
}

async function loadCurrentUserReservations() {
  try {
    const response = await secureFetch(`${API_URL}/User/Self`);
    if (response.ok) {
      const user = await response.json();
      currentUserId.value = user.id;
      
      // Update isCurrentUser for all reservations
      reservations.value.forEach(r => {
        r.isCurrentUser = r.userId === user.id;
      });
      weekDays.value = getWeekDays();
    }
  } catch (error) {
    console.error('Error loading current user:', error);
  }
}


async function cancelReservation(id: string) {
  const reservation = reservations.value.find(r => r.id === id);
  if (!reservation || !canCancelReservation(reservation)) {
    return;
  }

  try {
    const response = await secureFetch(`${API_URL}/Reservation/${id}`, {
      method: 'DELETE'
    });
    if (response.ok) {
      const index = reservations.value.findIndex(r => r.id === id);
      if (index !== -1 && reservations.value[index]) {
        reservations.value[index].status = 'cancelled';
        weekDays.value = getWeekDays();
      }
    }
  } catch (error) {
    console.error('Error cancelling reservation:', error);
  }
}

function createReservation() {
  if (!selectedFacility.value || !selectedDate.value || !selectedTime.value) {
    return;
  }

  const newReservation: Reservation = {
    id: String(reservations.value.length + 1),
    userId: currentUserId.value || '',
    facilityId: selectedFacility.value.id,
    facilityName: selectedFacility.value.name,
    date: selectedDate.value,
    time: selectedTime.value,
    duration: selectedDuration.value,
    price: (selectedFacility.value.pricePerHour * selectedDuration.value) / 60,
    status: 'active',
    isCurrentUser: true,
    startAt: '',
    endAt: ''
  };

  reservations.value.unshift(newReservation);
  
  // Reset form
  selectedFacility.value = null;
  selectedDate.value = '';
  selectedTime.value = '';
  selectedDuration.value = 60;
  showCreateForm.value = false;
}

async function selectFacility(facility: Facility) {
  selectedFacility.value = facility;
  selectedSlots.value = [];
  await loadReservationsForFacility(facility.id);
  await loadDowntimesForFacility(facility.id);
  weekDays.value = getWeekDays();
}

function goToFacilityDetail(facilityId: string | undefined) {
  if (!facilityId || !selectedFacility.value) return;
  showFacilityDetailModal.value = true;
}

function closeFacilityDetail() {
  showFacilityDetailModal.value = false;
}

function getMinDate() {
  const today = new Date();
  return today.toISOString().split('T')[0];
}

function startOfToday(): Date {
  const today = new Date();
  today.setHours(0, 0, 0, 0);
  return today;
}

function startOfTomorrow(): Date {
  const tomorrow = startOfToday();
  tomorrow.setDate(tomorrow.getDate() + 1);
  return tomorrow;
}

function canCancelReservation(reservation: Reservation): boolean {
  if (!reservation.isCurrentUser || reservation.status !== 'active') {
    return false;
  }

  const reservationStart = new Date(reservation.startAt);
  return reservationStart >= startOfTomorrow();
}

function calculatePrice() {
  if (!selectedFacility.value) return 0;
  return (selectedFacility.value.pricePerHour * selectedDuration.value) / 60;
}

function getWeekDays(): CalendarDay[] {
  const days = [];
  const weekRange = buildDayRange(currentWeekStart.value, 7);
  
  for (let i = 0; i < weekRange.length; i++) {
    const date = weekRange[i];
    if (!date) continue;
    
    const dateStr = formatDate(date);
    const slots: CalendarSlot[] = [];
    
    hours.value.forEach(hour => {
      const timeStr = `${hour.toString().padStart(2, '0')}:00`;
      const slotStart = addHours(startOfDay(date), hour);
      const slotEnd = addHours(slotStart, 1);
      
      // Check if this slot is occupied by existing reservations
      const occupiedReservation = reservations.value.find(reservation => {
        if (reservation.status === 'cancelled') return false;
        if (selectedFacility.value && reservation.facilityId !== selectedFacility.value.id) return false;
        const reservationStart = new Date(reservation.startAt);
        const reservationEnd = new Date(reservation.endAt);
        return overlaps(slotStart, slotEnd, reservationStart, reservationEnd);
      });

      // Check if this slot is in a downtime (closed)
      const isClosed = downtimes.value.some((downtime: DowntimeDto) => {
        if (selectedFacility.value && downtime.facilityId !== selectedFacility.value.id) return false;
        const downtimeStart = new Date(downtime.startAt);
        const downtimeEnd = new Date(downtime.endAt);
        return overlaps(slotStart, slotEnd, downtimeStart, downtimeEnd);
      });

      const isOccupied = !!occupiedReservation || isClosed;
      const isSelected = selectedSlots.value.some(slot => slot.date === dateStr && slot.time === timeStr);
      const isHighlighted = highlightedSlot.value?.date === dateStr && highlightedSlot.value?.time === timeStr;

      slots.push({
        time: timeStr,
        occupied: isOccupied,
        selected: isSelected,
        isCurrentUser: occupiedReservation?.isCurrentUser || false,
        isOtherUser: !!occupiedReservation && !occupiedReservation.isCurrentUser,
        isDowntime: isClosed,
        highlighted: isHighlighted
      });
    });
    
    days.push({
      date: dateStr,
      name: weekdayNames[(date.getDay() + 6) % 7] || '',
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
  const [hour, minute] = time.split(':').map(Number);
  const parts = date.split('.').map(part => parseInt(part, 10));
  const day = parts[0];
  const month = parts[1];
  const year = parts[2];

  if (day && month && year && hour !== undefined && minute !== undefined) {
    const slotDateTime = new Date(year, month - 1, day, hour, minute);
    if (slotDateTime < new Date()) {
      return;
    }
  }

  const slotIndex = selectedSlots.value.findIndex(slot => slot.date === date && slot.time === time);
  
  // Check if this slot is occupied by any user (current or other)
  const occupiedReservation = reservations.value.find(reservation => 
    reservation.status === 'active' && 
    reservation.date === date && 
    reservation.time === time
  );
  
  if (occupiedReservation) {
    if (occupiedReservation.isCurrentUser && canCancelReservation(occupiedReservation)) {
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

async function confirmReservation() {
  if (selectedSlots.value.length === 0 || !selectedFacility.value) return;
  
  try {
    // Create separate reservation for each selected slot
    for (const slot of selectedSlots.value) {
      const dateParts = slot.date.split('.').map(p => parseInt(p));
      const day = dateParts[0];
      const month = dateParts[1];
      const year = dateParts[2];
      const [hour, minute] = slot.time.split(':').map(Number);
      
      if (day === undefined || month === undefined || year === undefined || hour === undefined) {
        console.error('Invalid date or time format:', slot);
        continue;
      }
      
      const startAt = toIsoLocal(new Date(year, month - 1, day, hour, minute));
      const endAt = toIsoLocal(new Date(year, month - 1, day, hour + 1, minute));
      
      const createDto: CreateReservationDto = {
        facilityId: selectedFacility.value.id,
        startAt,
        endAt
      };
      
      const response = await secureFetch(`${API_URL}/Reservation`, {
        method: 'POST',
        body: JSON.stringify(createDto)
      });
      
      if (response.ok) {
        const newReservation: ReservationDto = await response.json();
        const facility = facilities.value.find(f => f.id === newReservation.facilityId);
        reservations.value.unshift({
          id: newReservation.id,
          userId: newReservation.userId,
          facilityId: newReservation.facilityId,
          facilityName: facility?.name || 'Unknown',
          date: formatDate(newReservation.startAt),
          time: formatDateTime(newReservation.startAt).split(' ')[1] || '',
          duration: Math.round((new Date(newReservation.endAt).getTime() - new Date(newReservation.startAt).getTime()) / 60000),
          price: newReservation.finalPrice,
          status: newReservation.status.toLowerCase() as 'active' | 'cancelled',
          isCurrentUser: true,
          startAt: newReservation.startAt,
          endAt: newReservation.endAt
        });
      }
    }
    
    // Reset form
    selectedSlots.value = [];
    
    // Refresh calendar to show new reservations
    weekDays.value = getWeekDays();
  } catch (error) {
    console.error('Error creating reservation:', error);
  }
}

function getStatusColor(status: string) {
  switch (status) {
    case 'active': return '#2196F3';
    case 'cancelled': return '#F44336';
    default: return '#757575';
  }
}

function getStatusText(status: string) {
  switch (status) {
    case 'active': return 'Aktivní';
    case 'cancelled': return 'Zrušená';
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

const canCancelById = computed(() => {
  const mapped = new Map<string, boolean>();
  reservations.value.forEach(r => {
    mapped.set(r.id, canCancelReservation(r));
  });
  return mapped;
});
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


          <div v-if="showFacilityDetailModal && selectedFacility" class="facility-detail-modal-backdrop" @click.self="closeFacilityDetail">
            <div class="facility-detail-modal facility-detail-modal--large colorful-modal">
              <div class="facility-modal-header">
                <span class="facility-modal-icon">{{ selectedFacility.image }}</span>
                <span class="facility-modal-title">{{ selectedFacility.name }}</span>
              </div>
              <div class="facility-modal-row">
                <span class="facility-modal-label"><span class="emoji">🏷️</span> Typ:</span>
                <span class="facility-modal-value">{{ selectedFacility.type?.name || 'Neuvedeno' }}</span>
              </div>
              <div class="facility-modal-row">
                <span class="facility-modal-label"><span class="emoji">💸</span> Cena:</span>
                <span class="facility-modal-value price">{{ selectedFacility.pricePerHour }} Kč / hod</span>
              </div>
              <div class="facility-modal-row">
                <span class="facility-modal-label"><span class="emoji">👥</span> Kapacita:</span>
                <span class="facility-modal-value">{{ selectedFacility.capacity }} osob</span>
              </div>
              <div class="facility-modal-row">
                <span class="facility-modal-label"><span class="emoji">📶</span> Dostupnost:</span>
                <span class="facility-modal-value" :class="{ open: selectedFacility.isActive, closed: !selectedFacility.isActive }">
                  {{ selectedFacility.isActive ? 'Otevřeno pro rezervace' : 'Momentálně nedostupné' }}
                </span>
              </div>
              <div class="facility-modal-description">
                <span class="facility-modal-label"><span class="emoji">📝</span> Popis:</span>
                <span class="facility-modal-value description">{{ selectedFacility.type?.description || 'Bez popisu' }}</span>
              </div>
              <div class="facility-detail-actions">
                <button @click="closeFacilityDetail" class="facility-close-button">Zavřít</button>
              </div>
            </div>
          </div>

          <div v-if="!isLoadingFacilities && facilities.length === 0" class="error-message-box" style="margin-bottom: 2rem;">
            💡 <strong>Nemám přístup k sportoviště.</strong> Zkontrolujte prosím:
            <ul style="margin: 0.5rem 0 0 1.5rem; padding: 0;">
              <li>Zda je backend spuštěn na http://localhost:5234</li>
              <li>Zda jsou v databázi vytvořena sportoviště a jejich typy</li>
              <li>Otevřete F12 konzolu (DevTools) pro podrobné diagnostické zprávy</li>
            </ul>
          </div>

          <div v-if="!selectedFacility" class="no-facility-warning">
            <div class="warning-box">
              <span class="warning-icon">⚠️</span>
              <p>Vyberte prosím sportoviště pro zobrazení kalendáře a možnost rezervace</p>
            </div>
          </div>

          <template v-else>
            <div class="facility-info-banner">
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
                  :class="[
                    'calendar-slot',
                    { 
                      'occupied': slot.occupied,
                      'free': !slot.occupied,
                      'selected': slot.selected,
                      'current-user': slot.isCurrentUser,
                      'other-user': slot.isOtherUser,
                      'downtime': slot.isDowntime,
                      'highlighted': slot.highlighted
                    }
                  ]"
                  @click="!slot.isOtherUser && !slot.isDowntime ? toggleSlot(day.date, slot.time) : null"
                ></div>
              </div>
              </div>
            </div>
            <div class="legend">
              <div class="legend-item"><span class="color-box occupied other-user"></span> Obsazeno ostatními</div>
              <div class="legend-item"><span class="color-box occupied current-user"></span> Vaše rezervace</div>
              <div class="legend-item"><span class="color-box downtime"></span> Údržba</div>
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
          </template>
        </div>
        
        <!-- Side panel for own reservations only -->
        <div class="completed-reservations-side">
          <h3>Moje rezervace</h3>
          
          <!-- Filter by date -->
          <div class="filter-section">
            <label class="filter-label">Filtrovat podle dne:</label>
            <select v-model="reservationFilterDate" class="date-filter">
              <option value="">Všechny dny</option>
              <option v-for="date in [...new Set(reservations.filter(r => r.status === 'active' && r.isCurrentUser).map(r => r.date))]" 
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
                <button
                  @click="cancelReservation(reservation.id)"
                  class="cancel-button"
                  :disabled="!canCancelById.get(reservation.id)"
                  :title="canCancelById.get(reservation.id) ? '' : 'Rezervaci lze zrušit nejpozději den předem.'"
                >
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


.calendar-slot.occupied.other-user {
  background-color: #FF5252;
  cursor: not-allowed;
  position: relative;
}
.calendar-slot.occupied.other-user::after {
  content: '×';
  position: absolute;
  top: 50%;
  left: 50%;
  transform: translate(-50%, -50%);
  color: white;
  font-size: 1.5rem;
  font-weight: bold;
}

.calendar-slot.downtime {
  background-color: #BDBDBD !important;
  cursor: not-allowed;
  position: relative;
}
.calendar-slot.downtime::after {
  content: '🛠';
  position: absolute;
  top: 50%;
  left: 50%;
  transform: translate(-50%, -50%);
  color: white;
  font-size: 1.3rem;
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


.color-box.occupied.other-user {
  background-color: #FF5252;
  position: relative;
}
.color-box.occupied.other-user::after {
  content: '×';
  position: absolute;
  top: 50%;
  left: 50%;
  transform: translate(-50%, -50%);
  color: white;
  font-size: 0.9rem;
  font-weight: bold;
}
.color-box.downtime {
  background-color: #BDBDBD;
  position: relative;
}
.color-box.downtime::after {
  content: '🛠';
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

.facility-detail-modal-backdrop {
  position: fixed;
  inset: 0;
  background: rgba(0, 0, 0, 0.45);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 300;
  padding: 1rem;
}


.facility-detail-modal {
  width: min(720px, 100%);
  background: white;
  border-radius: 1rem;
  padding: 1.5rem;
  border: 1px solid var(--vt-c-divider);
  box-shadow: 0 20px 60px rgba(0, 0, 0, 0.2);
}

.facility-detail-modal--large.colorful-modal {
  width: min(480px, 98vw);
  background: linear-gradient(135deg, #f8fafc 60%, #e0e7ff 100%);
  border-radius: 1.5rem;
  padding: 2.5rem 2.5rem 2rem 2.5rem;
  box-shadow: 0 24px 80px rgba(80, 80, 160, 0.18);
  border: none;
  display: flex;
  flex-direction: column;
  gap: 1.2rem;
  align-items: stretch;
}

.facility-modal-header {
  display: flex;
  align-items: center;
  gap: 1.2rem;
  margin-bottom: 0.5rem;
}
.facility-modal-icon {
  font-size: 3.5rem;
  background: white;
  border-radius: 1.2rem;
  box-shadow: 0 2px 12px rgba(80,80,160,0.10);
  padding: 0.5rem 1.2rem;
}
.facility-modal-title {
  font-size: 2.1rem;
  font-weight: 800;
  color: #2d2d4d;
  letter-spacing: -1px;
}
.facility-modal-row {
  display: flex;
  align-items: center;
  gap: 0.7rem;
  font-size: 1.13rem;
}
.facility-modal-label {
  font-weight: 600;
  color: #4b5563;
  min-width: 120px;
  display: flex;
  align-items: center;
  gap: 0.3rem;
}
.facility-modal-value {
  color: #22223b;
  font-weight: 500;
}
.facility-modal-value.price {
  color: #2563eb;
  font-weight: 700;
  font-size: 1.18rem;
}
.facility-modal-value.open {
  color: #059669;
  font-weight: 700;
}
.facility-modal-value.closed {
  color: #dc2626;
  font-weight: 700;
}
.facility-modal-description {
  margin-top: 1.2rem;
  background: rgba(255,255,255,0.7);
  border-radius: 0.8rem;
  padding: 1.1rem 1.2rem;
  font-size: 1.08rem;
  box-shadow: 0 1px 6px rgba(80,80,160,0.07);
  display: flex;
  flex-direction: column;
  gap: 0.3rem;
}
.facility-modal-value.description {
  color: #374151;
  font-size: 1.08rem;
  margin-top: 0.2rem;
}
.emoji {
  font-size: 1.1em;
  margin-right: 0.2em;
}

.facility-detail-modal h3 {
  margin-bottom: 1rem;
  font-size: 1.4rem;
  color: var(--color-heading);
}

.facility-detail-grid {
  display: grid;
  grid-template-columns: 160px 1fr;
  gap: 0.75rem 1rem;
}

.facility-detail-label {
  font-weight: 700;
  color: var(--color-heading);
}

.facility-detail-value {
  color: var(--color-text);
  word-break: break-word;
}

.facility-detail-actions {
  margin-top: 1.5rem;
  display: flex;
  justify-content: flex-end;
  gap: 0.75rem;
}

.facility-close-button {
  border: none;
  border-radius: 0.75rem;
  padding: 0.7rem 1.2rem;
  font-weight: 600;
  cursor: pointer;
}

.facility-close-button {
  background: var(--vt-c-white-soft);
  color: var(--color-text);
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

  .facility-detail-grid {
    grid-template-columns: 1fr;
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
