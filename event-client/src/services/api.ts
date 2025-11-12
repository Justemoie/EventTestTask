import axios, { AxiosInstance } from 'axios';
import {
    Event,
    EventRequest,
    User,
    UpdateUserRequest,
    RegisterUser,
    LoginUser,
    TokenResponse,
    PageParams,
    PageResult,
    EventFilter
} from '../types';

class ApiService {
    private api: AxiosInstance;

    constructor() {
        this.api = axios.create({
            baseURL: '/api',
            headers: {
                'Content-Type': 'application/json',
            },
            withCredentials: true
        });

        // Interceptor для добавления токена к запросам
        this.api.interceptors.request.use(
            (config) => {
                const token = localStorage.getItem('token');
                if (token) {
                    config.headers.Authorization = `Bearer ${token}`;
                }
                return config;
            },
            (error) => {
                return Promise.reject(error);
            }
        );

        // Interceptor для обработки ошибок
        this.api.interceptors.response.use(
            (response) => response,
            (error) => {
                if (error.response?.status === 401) {
                    localStorage.removeItem('token');
                    localStorage.removeItem('user');
                    window.location.href = '/login';
                } else if (error.code === 'NETWORK_ERROR' || error.message === 'Network Error') {
                    console.error('Network error - check if server is running');
                } else if (error.code === 'ERR_NETWORK') {
                    console.error('Network error - check server connection');
                }
                return Promise.reject(error);
            }
        );
    }

    // Auth endpoints
    async register(userData: RegisterUser): Promise<{ message: string }> {
        const response = await this.api.post('/auth/register', userData);
        return response.data;
    }

    async login(credentials: LoginUser): Promise<TokenResponse> {
        const response = await this.api.post('/auth/login', credentials);
        const tokenResponse = response.data;

        localStorage.setItem('token', tokenResponse.accessToken);

        return tokenResponse;
    }

    async logout(): Promise<{ message: string }> {
        const response = await this.api.post('/auth/logout');
        localStorage.removeItem('token');
        localStorage.removeItem('user');
        return response.data;
    }

    async updateTokens(): Promise<void> {
        await this.api.post('/auth/update');
    }

    // User endpoints
    async getUserById(userId: string): Promise<User> {
        const response = await this.api.get(`/users/${userId}`);
        return response.data;
    }

    async getUserByEmail(email: string): Promise<User> {
        const response = await this.api.get(`/users/email?email=${email}`);
        return response.data;
    }

    async updateUser(userId: string, userData: UpdateUserRequest): Promise<void> {
        await this.api.put(`/users/update/${userId}`, userData);
    }

    // Event endpoints
    async getEvents(pageParams: PageParams): Promise<PageResult<Event>> {
        const response = await this.api.get('/events', { params: pageParams });
        return response.data;
    }

    async getEventById(eventId: string): Promise<Event> {
        const response = await this.api.get(`/events/${eventId}`);
        return response.data;
    }

    async getEventByTitle(title: string): Promise<Event> {
        const response = await this.api.get(`/events/by-title?title=${title}`);
        return response.data;
    }

    // Создание события (без изображения)
    async createEvent(eventData: EventRequest): Promise<void> {
        const response = await this.api.post('/events/create', eventData, {
            headers: { 
                'Content-Type': 'application/json'
            }
        });
        return response.data;
    }

    // Создание события с изображением
    async createEventWithImage(event: EventRequest, imageFile?: File) {
        const formData = new FormData();
        
        formData.append("Title", event.title);
        formData.append("Description", event.description);
        formData.append("StartDate", event.startDate);
        formData.append("EndDate", event.endDate);
        formData.append("Location", event.location);
        formData.append("Category", event.category.toString());
        formData.append("MaxParticipants", event.maxParticipants.toString());
        
        if (imageFile) {
            formData.append("Image", imageFile);
        }
        
        return this.api.post('/events/create', formData, {
            headers: { 
                'Content-Type': 'multipart/form-data'
            }
        });
    }

    // Обновление события (без изображения)
    async updateEvent(eventId: string, eventData: EventRequest): Promise<void> {
        const response = await this.api.put(`/events/update/${eventId}`, eventData, {
            headers: { 
                'Content-Type': 'application/json'
            }
        });
        return response.data;
    }

    // Загрузка изображения события
    async uploadEventImage(eventId: string, imageFile: File) {
        const formData = new FormData();
        formData.append("image", imageFile);
        
        return this.api.post(`/events/image-update/${eventId}`, formData, {
            headers: { 
                'Content-Type': 'multipart/form-data'
            }
        });
    }

    // Обновление события с изображением
    async updateEventWithImage(eventId: string, eventData: EventRequest, imageFile?: File): Promise<void> {
        // Сначала обновляем данные события
        await this.updateEvent(eventId, eventData);
        
        // Затем загружаем изображение, если есть
        if (imageFile) {
            await this.uploadEventImage(eventId, imageFile);
        }
    }

    async deleteEvent(eventId: string): Promise<string> {
        const response = await this.api.delete(`/events/delete/${eventId}`);
        return response.data;
    }

    // В ApiService добавьте/обновите метод searchEvents:

async searchEvents(pageParams: PageParams, filter: EventFilter): Promise<PageResult<Event>> {
    // Создаем объект параметров, исключая undefined значения
    const params: any = {
        page: pageParams.page,
        pageSize: pageParams.pageSize
    };

    // Добавляем фильтры только если они определены
    if (filter.searchTerm) params.searchTerm = filter.searchTerm;
    if (filter.category) params.category = filter.category;
    if (filter.location) params.location = filter.location;
    if (filter.startDate) params.startDate = filter.startDate;
    if (filter.endDate) params.endDate = filter.endDate;

    const response = await this.api.get('/events/search', { params });
    return response.data;
}

    async getEventImage(eventId: string): Promise<string> {
        const response = await this.api.get(`/events/image/${eventId}`, {
            responseType: 'blob'
        });
        return URL.createObjectURL(response.data);
    }

    // Registration endpoints
    async getEventParticipants(eventId: string, pageParams?: PageParams): Promise<User[]> {
        const params = pageParams || { page: 1, pageSize: 50 };
        const response = await this.api.get(`/registrations/${eventId}`, { params });
        return response.data;
    }

    async getEventParticipantById(eventId: string, userId: string): Promise<User> {
        const response = await this.api.get(`/registrations/${eventId}/users/${userId}`);
        return response.data;
    }

    async registerForEvent(eventId: string, userId: string): Promise<void> {
        await this.api.post(`/registrations/${eventId}/users/${userId}/register`);
    }

    async unregisterFromEvent(eventId: string, userId: string): Promise<void> {
        await this.api.post(`/registrations/${eventId}/users/${userId}/unregister`);
    }

    // Получить созданные события (по создателю)
    async getMyCreatedEvents(userId: string, pageParams: PageParams): Promise<PageResult<Event>> {
        const response = await this.api.get(`/events/created-by/${userId}`, { params: pageParams });
        return response.data;
    }

    // Получить зарегистрированные события (по участнику)
    async getMyRegisteredEvents(userId: string, pageParams: PageParams): Promise<PageResult<Event>> {
        const response = await this.api.get(`/events/registered-by/${userId}`, { params: pageParams });
        return response.data;
    }

    // В apiService добавьте методы
async getEventParticipantsCount(eventId: string): Promise<number> {
    try {
        const participants = await this.getEventParticipants(eventId, { page: 1, pageSize: 1000 });
        return participants.length;
    } catch (error) {
        console.error('Error getting participants count:', error);
        return 0;
    }
}

async getEventsWithParticipants(userId: string): Promise<Event[]> {
    const pageParams = { page: 1, pageSize: 50 };
    const createdEvents = await this.getMyCreatedEvents(userId, pageParams);
    
    // Для каждого события получаем количество участников
    const eventsWithParticipants = await Promise.all(
        createdEvents.data.map(async (event: Event) => {
            const participantsCount = await this.getEventParticipantsCount(event.id);
            return {
                ...event,
                participantsCount: participantsCount
            };
        })
    );
    
    return eventsWithParticipants;
}
}

export const apiService = new ApiService();