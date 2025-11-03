import React, { useState, useEffect, useCallback } from 'react';
import { Event, PageParams, EventFilter, EventCategory } from '../types';
import { apiService } from '../services/api';
import EventCard from '../components/EventCard';
import { Search, Filter, ChevronLeft, ChevronRight, Calendar, MapPin } from 'lucide-react';
import { useAuth } from '../contexts/AuthContext';
import { useNavigate } from 'react-router-dom';

const EventsPage: React.FC = () => {
    const [events, setEvents] = useState<Event[]>([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string>('');
    const [joiningId, setJoiningId] = useState<string | null>(null);
    const { isAuthenticated, user } = useAuth();
    const navigate = useNavigate();
    
    // Состояния для фильтров
    const [searchTerm, setSearchTerm] = useState('');
    const [selectedCategory, setSelectedCategory] = useState<EventCategory | ''>('');
    const [location, setLocation] = useState('');
    const [startDate, setStartDate] = useState('');
    const [endDate, setEndDate] = useState('');
    const [showFilters, setShowFilters] = useState(false);
    
    const [pagination, setPagination] = useState({
        page: 1,
        pageSize: 6,
        totalPages: 1,
        totalCount: 0
    });

    // Дебаунс для поиска
    const [searchTimeout, setSearchTimeout] = useState<NodeJS.Timeout | null>(null);

    const categoryOptions = [
        { value: '', label: 'Все категории' },
        { value: EventCategory.Conference, label: 'Конференция' },
        { value: EventCategory.Workshop, label: 'Воркшоп' },
        { value: EventCategory.Webinar, label: 'Вебинар' },
        { value: EventCategory.Meetup, label: 'Митап' },
        { value: EventCategory.Party, label: 'Вечеринка' },
        { value: EventCategory.Sport, label: 'Спорт' },
        { value: EventCategory.Other, label: 'Другое' }
    ];

    const fetchEvents = useCallback(async (searchQuery: string = searchTerm) => {
        try {
            setLoading(true);
            setError('');

            const pageParams: PageParams = {
                page: pagination.page,
                pageSize: pagination.pageSize
            };

            const filter: EventFilter = {
                searchTerm: searchQuery || undefined,
                category: selectedCategory || undefined,
                location: location || undefined,
                startDate: startDate || undefined,
                endDate: endDate || undefined
            };

            console.log('Searching with filter:', filter);

            const result = await apiService.searchEvents(pageParams, filter);
            console.log('API result:', result);

            // Универсальное извлечение массива событий
            let eventList: Event[] = [];
            if (result && typeof result === 'object') {
                if (Array.isArray((result as any).$values)) {
                    eventList = (result as any).$values;
                } else if (Array.isArray((result as any).data)) {
                    eventList = (result as any).data;
                } else if (Array.isArray(result)) {
                    eventList = result;
                } else if ((result as any).data && Array.isArray((result as any).data.$values)) {
                    eventList = (result as any).data.$values;
                }
            }

            setEvents(eventList || []);

            setPagination(prev => ({
                ...prev,
                totalPages: Math.ceil((result?.totalCount || 0) / pagination.pageSize),
                totalCount: result?.totalCount || 0
            }));
        } catch (err: any) {
            console.error('Error fetching events:', err);
            setError('Ошибка загрузки событий. Проверьте подключение к серверу.');
            setEvents([]);
        } finally {
            setLoading(false);
        }
    }, [pagination.page, pagination.pageSize, searchTerm, selectedCategory, location, startDate, endDate]);

    // Дебаунсинг поиска
    const handleSearchChange = (e: React.ChangeEvent<HTMLInputElement>) => {
        const value = e.target.value;
        setSearchTerm(value);
        
        setPagination(prev => ({ ...prev, page: 1 }));
    
        if (searchTimeout) {
            clearTimeout(searchTimeout);
        }
    
        // Если введен пробел в конце - ищем сразу
        if (value.endsWith(' ')) {
            fetchEvents(value.trim());
        } else if (value.length === 0 || value.length >= 2) {
            // Обычный дебаунс для остальных случаев
            const timeout = setTimeout(() => {
                fetchEvents(value);
            }, 500);
            setSearchTimeout(timeout);
        }
    };

    // Поиск по форме (при нажатии Enter или кнопке)
    const handleSearchSubmit = (e: React.FormEvent) => {
        e.preventDefault();
        if (searchTimeout) {
            clearTimeout(searchTimeout);
        }
        setPagination(prev => ({ ...prev, page: 1 }));
        fetchEvents();
    };

    // Обработчики для фильтров
    const handleCategoryChange = (category: EventCategory | '') => {
        setSelectedCategory(category);
        setPagination(prev => ({ ...prev, page: 1 }));
    };

    const handleLocationChange = (value: string) => {
        setLocation(value);
        setPagination(prev => ({ ...prev, page: 1 }));
    };

    const handleStartDateChange = (value: string) => {
        setStartDate(value);
        setPagination(prev => ({ ...prev, page: 1 }));
    };

    const handleEndDateChange = (value: string) => {
        setEndDate(value);
        setPagination(prev => ({ ...prev, page: 1 }));
    };

    // Сброс всех фильтров
    const handleResetFilters = () => {
        setSearchTerm('');
        setSelectedCategory('');
        setLocation('');
        setStartDate('');
        setEndDate('');
        setPagination(prev => ({ ...prev, page: 1 }));
    };

    // Запуск поиска при изменении фильтров (кроме поискового запроса)
    useEffect(() => {
        if (searchTimeout) {
            clearTimeout(searchTimeout);
        }
        fetchEvents();
    }, [selectedCategory, location, startDate, endDate, pagination.page]);

    // Очистка таймаута при размонтировании
    useEffect(() => {
        return () => {
            if (searchTimeout) {
                clearTimeout(searchTimeout);
            }
        };
    }, [searchTimeout]);

    const handlePageChange = (newPage: number) => {
        setPagination(prev => ({ ...prev, page: newPage }));
    };

    const handleJoinEvent = async (event: Event) => {
        try {
            if (!isAuthenticated || !user) {
                navigate('/login');
                return;
            }
            setJoiningId(event.id);
            await apiService.registerForEvent(event.id, user.id);
            await fetchEvents();
        } catch (error) {
            console.error('Error joining event:', error);
        } finally {
            setJoiningId(null);
        }
    };

    if (loading && events.length === 0) {
        return (
            <div className="min-h-screen bg-gray-50 flex items-center justify-center">
                <div className="text-center">
                    <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-gray-900 mx-auto"></div>
                    <p className="mt-4 text-gray-600">Загрузка событий...</p>
                </div>
            </div>
        );
    }

    return (
        <div className="min-h-screen bg-gray-50">
            <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
                {/* Search and Filter Section */}
                <div className="mb-8">
                    <div className="flex flex-col sm:flex-row gap-4 items-center justify-between">
                        <div className="flex-1 max-w-md">
                            <form onSubmit={handleSearchSubmit} className="relative">
                                <div className="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none">
                                    <Search className="h-5 w-5 text-gray-400" />
                                </div>
                                <input
                                    type="text"
                                    placeholder="Поиск мероприятий..."
                                    value={searchTerm}
                                    onChange={handleSearchChange}
                                    className="block w-full pl-10 pr-3 py-2 border border-gray-300 rounded-md leading-5 bg-white placeholder-gray-500 focus:outline-none focus:ring-1 focus:ring-gray-500 focus:border-gray-500 sm:text-sm"
                                />
                            </form>
                        </div>

                        <div className="flex gap-2">
                            <button
                                onClick={() => setShowFilters(!showFilters)}
                                className="inline-flex items-center px-4 py-2 border border-gray-300 rounded-md shadow-sm text-sm font-medium text-gray-700 bg-white hover:bg-gray-50 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-gray-500"
                            >
                                <Filter className="h-4 w-4 mr-2" />
                                Фильтры {showFilters ? '▲' : '▼'}
                            </button>
                            
                            {(selectedCategory || location || startDate || endDate || searchTerm) && (
                                <button
                                    onClick={handleResetFilters}
                                    className="inline-flex items-center px-4 py-2 border border-gray-300 rounded-md shadow-sm text-sm font-medium text-gray-700 bg-white hover:bg-gray-50 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-gray-500"
                                >
                                    Сбросить
                                </button>
                            )}
                        </div>
                    </div>

                    {/* Filter Options */}
                    {showFilters && (
                        <div className="mt-4 p-4 bg-white rounded-lg border border-gray-300">
                            <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4">
                                <div>
                                    <label className="block text-sm font-medium text-gray-700 mb-2">
                                        Категория
                                    </label>
                                    <select
                                        value={selectedCategory}
                                        onChange={(e) => handleCategoryChange(e.target.value as EventCategory | '')}
                                        className="block w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm focus:outline-none focus:ring-gray-500 focus:border-gray-500 sm:text-sm"
                                    >
                                        {categoryOptions.map(option => (
                                            <option key={option.value} value={option.value}>
                                                {option.label}
                                            </option>
                                        ))}
                                    </select>
                                </div>

                                <div>
                                    <label className="block text-sm font-medium text-gray-700 mb-2">
                                        <MapPin className="h-4 w-4 inline mr-1" />
                                        Местоположение
                                    </label>
                                    <input
                                        type="text"
                                        placeholder="Город или адрес"
                                        value={location}
                                        onChange={(e) => handleLocationChange(e.target.value)}
                                        className="block w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm focus:outline-none focus:ring-gray-500 focus:border-gray-500 sm:text-sm"
                                    />
                                </div>

                                <div>
                                    <label className="block text-sm font-medium text-gray-700 mb-2">
                                        <Calendar className="h-4 w-4 inline mr-1" />
                                        Дата от
                                    </label>
                                    <input
                                        type="date"
                                        value={startDate}
                                        onChange={(e) => handleStartDateChange(e.target.value)}
                                        className="block w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm focus:outline-none focus:ring-gray-500 focus:border-gray-500 sm:text-sm"
                                    />
                                </div>

                                <div>
                                    <label className="block text-sm font-medium text-gray-700 mb-2">
                                        <Calendar className="h-4 w-4 inline mr-1" />
                                        Дата до
                                    </label>
                                    <input
                                        type="date"
                                        value={endDate}
                                        onChange={(e) => handleEndDateChange(e.target.value)}
                                        className="block w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm focus:outline-none focus:ring-gray-500 focus:border-gray-500 sm:text-sm"
                                    />
                                </div>
                            </div>
                        </div>
                    )}
                </div>

                {/* Events Grid */}
                {error ? (
                    <div className="text-center py-12">
                        <div className="text-gray-900 text-lg font-medium mb-2">Ошибка загрузки</div>
                        <p className="text-gray-600 mb-4">{error}</p>
                        <button
                            onClick={() => fetchEvents()}
                            className="inline-flex items-center px-4 py-2 border border-transparent text-sm font-medium rounded-md text-white bg-gray-900 hover:bg-gray-800 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-gray-500"
                        >
                            Попробовать снова
                        </button>
                    </div>
                ) : events.length === 0 ? (
                    <div className="text-center py-12">
                        <div className="text-gray-900 text-lg font-medium mb-2">
                            {searchTerm || selectedCategory || location || startDate || endDate 
                                ? 'События по вашему запросу не найдены' 
                                : 'События не найдены'
                            }
                        </div>
                        <p className="text-gray-600 mb-4">
                            {searchTerm || selectedCategory || location || startDate || endDate 
                                ? 'Попробуйте изменить параметры поиска' 
                                : 'Попробуйте загрузить позже'
                            }
                        </p>
                        {(searchTerm || selectedCategory || location || startDate || endDate) && (
                            <button
                                onClick={handleResetFilters}
                                className="inline-flex items-center px-4 py-2 border border-transparent text-sm font-medium rounded-md text-white bg-gray-900 hover:bg-gray-800 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-gray-500"
                            >
                                Сбросить фильтры
                            </button>
                        )}
                    </div>
                ) : (
                    <>
                        <div className="mb-4 flex justify-between items-center">
                            <p className="text-gray-600">
                                Найдено событий: {pagination.totalCount}
                            </p>
                            {(searchTerm || selectedCategory || location || startDate || endDate) && (
                                <button
                                    onClick={handleResetFilters}
                                    className="text-sm text-gray-600 hover:text-gray-900"
                                >
                                    Сбросить фильтры
                                </button>
                            )}
                        </div>

                        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6 mb-8">
                            {events.map((event) => (
                                <EventCard
                                    key={event.id}
                                    event={event}
                                    onJoin={handleJoinEvent}
                                />
                            ))}
                        </div>

                        {/* Pagination */}
                        {pagination.totalPages > 1 && (
                            <div className="flex items-center justify-center space-x-2">
                                <button
                                    onClick={() => handlePageChange(pagination.page - 1)}
                                    disabled={pagination.page === 1}
                                    className="p-2 rounded-md border border-gray-300 bg-white text-gray-500 hover:text-gray-700 disabled:opacity-50 disabled:cursor-not-allowed"
                                >
                                    <ChevronLeft className="h-5 w-5" />
                                </button>

                                {Array.from({ length: pagination.totalPages }, (_, i) => i + 1).map((page) => (
                                    <button
                                        key={page}
                                        onClick={() => handlePageChange(page)}
                                        className={`px-3 py-2 rounded-md text-sm font-medium ${page === pagination.page
                                            ? 'bg-gray-900 text-white'
                                            : 'bg-white text-gray-700 border border-gray-300 hover:bg-gray-50'
                                            }`}
                                    >
                                        {page}
                                    </button>
                                ))}

                                <button
                                    onClick={() => handlePageChange(pagination.page + 1)}
                                    disabled={pagination.page === pagination.totalPages}
                                    className="p-2 rounded-md border border-gray-300 bg-white text-gray-500 hover:text-gray-700 disabled:opacity-50 disabled:cursor-not-allowed"
                                >
                                    <ChevronRight className="h-5 w-5" />
                                </button>
                            </div>
                        )}
                    </>
                )}
            </div>
        </div>
    );
};

export default EventsPage;