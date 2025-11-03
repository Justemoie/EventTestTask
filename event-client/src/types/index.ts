// Enums
export enum EventCategory {
    Conference = 1,
    Workshop = 2,
    Webinar = 3,
    Meetup = 4,
    Party = 5,
    Sport = 6,
    Other = 7
}

export enum UserRole {
    User = 1,
    Admin = 2
}

// Event types
export interface Event {
    id: string;
    title: string;
    description: string;
    startDate: string;
    endDate: string;
    location: string;
    category: EventCategory;
    maxParticipants: number;
    image?: Uint8Array;        // <- теперь массив байтов
    participants: Registration[];
}


export interface EventRequest {
    title: string;
    description: string;
    startDate: string;
    endDate: string;
    location: string;
    category: EventCategory;
    maxParticipants: number;
    image?: Uint8Array; // <- исправлено
}

// User types
export interface User {
    id: string;
    firstName: string;
    lastName: string;
    birthDate: string;
    email: string;
    role: UserRole; // Добавьте поле role
    events: Registration[];
    passwordHash: string;
}

// Добавьте интерфейс для обновления пользователя
export interface UpdateUserRequest {
    firstName: string;
    lastName: string;
    email: string;
    birthDate: string;
}

export interface RegisterUser {
    firstName: string;
    lastName: string;
    email: string;
    password: string;
    birthDate: string;
}

export interface LoginUser {
    email: string;
    password: string;
}

// Registration types
export interface Registration {
    id: string;
    eventId: string;
    userId: string;
    registrationDate: string;
}

// Auth types
export interface TokenResponse {
    accessToken: string;
    refreshToken: string;
}

// Pagination types
export interface PageParams {
    page: number;
    pageSize: number;
}

export interface PageResult<T> {
    data: T[];
    totalCount: number;
    pageNumber?: number;
    pageSize?: number;
    totalPages?: number;
}

// Filter types
export interface EventFilter {
    category?: EventCategory;
    startDate?: string;
    endDate?: string;
    location?: string;
    searchTerm?: string;
}

// UI State types
export interface AuthState {
    user: User | null;
    token: string | null;
    isAuthenticated: boolean;
    loading: boolean;
}

export interface EventState {
    events: Event[];
    currentEvent: Event | null;
    loading: boolean;
    error: string | null;
}
