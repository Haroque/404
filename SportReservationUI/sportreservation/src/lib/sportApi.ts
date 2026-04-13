export interface FacilityTypeDto {
    id: string
    name: string
    description?: string | null
}

export interface FacilityComplexDto {
    id: string
    name: string
    capacity: number
    isActive: boolean
    createdAt: string
    type: FacilityTypeDto
}

export interface FacilityPaginatedDto {
    totalPages: number
    items: FacilityComplexDto[]
}

export interface ReservationDto {
    id: string
    userId: string
    facilityId: string
    startAt: string
    endAt: string
    status: string
    basePrice: number
    discountPercent: number
    finalPrice: number
    createdAt: string
    cancelledAt?: string | null
}

export interface DowntimeDto {
    id: string
    facilityId: string
    startAt: string
    endAt: string
    reason: string
}

export interface UserDto {
    id: string
    email: string
    role: string
    fullName: string
    createdAt: string
}

export interface UserPatchPasswordDto {
    current?: string | null
    new: string
}

export interface UserPatchDto {
    id?: string | null
    email?: string | null
    fullName?: string | null
    password?: UserPatchPasswordDto | null
}

export interface CreateReservationDto {
    userId?: string | null
    facilityId: string
    startAt: string
    endAt: string
}

export interface ScheduleSlot {
    date: Date
    hour: number
}

const dateFormatter = new Intl.DateTimeFormat('cs-CZ', {
    day: '2-digit',
    month: '2-digit',
    year: 'numeric'
})

const dateTimeFormatter = new Intl.DateTimeFormat('cs-CZ', {
    day: '2-digit',
    month: '2-digit',
    hour: '2-digit',
    minute: '2-digit'
})

const weekdayFormatter = new Intl.DateTimeFormat('cs-CZ', {
    weekday: 'short',
    day: '2-digit',
    month: '2-digit'
})

export function uniqueFacilityTypes(facilities: FacilityComplexDto[]): FacilityTypeDto[] {
    const mapped = new Map<string, FacilityTypeDto>()

    for (const facility of facilities) {
        if (!mapped.has(facility.type.id)) {
            mapped.set(facility.type.id, facility.type)
        }
    }

    return [...mapped.values()].sort((left, right) => left.name.localeCompare(right.name))
}

export function formatDate(value: string | Date): string {
    return dateFormatter.format(typeof value === 'string' ? new Date(value) : value)
}

export function formatDateTime(value: string | Date): string {
    return dateTimeFormatter.format(typeof value === 'string' ? new Date(value) : value)
}

export function formatWeekday(value: Date): string {
    return weekdayFormatter.format(value)
}

export function formatHourLabel(hour: number): string {
    return `${String(hour).padStart(2, '0')}:00`
}

export function buildDayRange(start: Date, days = 7): Date[] {
    const result: Date[] = []

    for (let index = 0; index < days; index += 1) {
        const day = new Date(start)
        day.setHours(0, 0, 0, 0)
        day.setDate(day.getDate() + index)
        result.push(day)
    }

    return result
}

export function startOfDay(value: Date): Date {
    const day = new Date(value)
    day.setHours(0, 0, 0, 0)
    return day
}

export function addHours(value: Date, hours: number): Date {
    return new Date(value.getTime() + hours * 60 * 60 * 1000)
}

export function toIsoLocal(value: Date): string {
    const year = value.getFullYear()
    const month = String(value.getMonth() + 1).padStart(2, '0')
    const day = String(value.getDate()).padStart(2, '0')
    const hours = String(value.getHours()).padStart(2, '0')
    const minutes = String(value.getMinutes()).padStart(2, '0')
    const seconds = String(value.getSeconds()).padStart(2, '0')

    return `${year}-${month}-${day}T${hours}:${minutes}:${seconds}`
}

export function overlaps(leftStart: Date, leftEnd: Date, rightStart: Date, rightEnd: Date): boolean {
    return leftStart < rightEnd && leftEnd > rightStart
}

export function isReservationActive(status: string): boolean {
    return status.toLowerCase() === 'active'
}

export function normalizeStatus(status: string): string {
    return status.charAt(0).toUpperCase() + status.slice(1).toLowerCase()
}
