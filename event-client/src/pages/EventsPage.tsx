import React, { useState, useEffect, useCallback, useRef } from 'react';
import { Event, PageParams, EventFilter, EventCategory } from '../types';
import { apiService } from '../services/api';
import EventCard from '../components/EventCard';
import { Search, Filter, ChevronLeft, ChevronRight, Calendar, MapPin, X } from 'lucide-react';
import { useAuth } from '../contexts/AuthContext';
import { useNavigate } from 'react-router-dom';

const EventsPage: React.FC = () => {
    const [events, setEvents] = useState<Event[]>([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string>('');
    const [joiningId, setJoiningId] = useState<string | null>(null);
    const { isAuthenticated, user } = useAuth();
    const navigate = useNavigate();

    // Фильтры
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

    // Для дебаунса
    const searchTimeoutRef = useRef<NodeJS.Timeout | null>(null);

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

    // Один fetchEvents — всегда актуальные фильтры
    const fetchEvents = useCallback(async () => {
        if (searchTimeoutRef.current) {
            clearTimeout(searchTimeoutRef.current);
            searchTimeoutRef.current = null;
        }

        try {
            setLoading(true);
            setError('');

            const pageParams: PageParams = {
                page: pagination.page,
                pageSize: pagination.pageSize
            };

            const filter: EventFilter = {
                searchTerm: searchTerm.trim() || undefined,
                category: selectedCategory || undefined,
                location: location.trim() || undefined,
                startDate: startDate || undefined,
                endDate: endDate || undefined
            };

            console.log('🔍 Fetching with filter:', filter, 'page:', pagination.page);

            const result = await apiService.searchEvents(pageParams, filter);

            let eventList: Event[] = [];
            if (result && typeof result === 'object') {
                if (Array.isArray((result as any).$values)) {
                    eventList = (result as any).$values;
                } else if (Array.isArray((result as any).data)) {
                    eventList = (result as any).data;
                } else if (Array.isArray(result)) {
                    eventList = result;
                } else if ((result as any).data?.$values) {
                    eventList = (result as any).data.$values;
                }
            }

            setEvents(eventList || []);
            setPagination(prev => ({
                ...prev,
                totalPages: Math.ceil((result?.totalCount || 0) / prev.pageSize),
                totalCount: result?.totalCount || 0
            }));
        } catch (err: any) {
            console.error('Error:', err);
            setError('Ошибка загрузки. Проверьте подключение.');
            setEvents([]);
        } finally {
            setLoading(false);
        }
    }, [pagination.page, pagination.pageSize, searchTerm, selectedCategory, location, startDate, endDate]);

    // Дебаунс только для поискового поля
    const debouncedSearch = useCallback(() => {
        if (searchTimeoutRef.current) {
            clearTimeout(searchTimeoutRef.current);
        }
        searchTimeoutRef.current = setTimeout(() => {
            setPagination(p => ({ ...p, page: 1 }));
            fetchEvents();
        }, 600);
    }, [fetchEvents]);

    // Ручной поиск (кнопка или Enter)
    const triggerSearch = () => {
        if (searchTimeoutRef.current) {
            clearTimeout(searchTimeoutRef.current);
        }
        setPagination(p => ({ ...p, page: 1 }));
        fetchEvents();
    };

    // Обработчик ввода в поиск
    const handleSearchChange = (e: React.ChangeEvent<HTMLInputElement>) => {
        const value = e.target.value;
        setSearchTerm(value);
        setPagination(p => ({ ...p, page: 1 }));

        if (value.length === 0 || value.length >= 2) {
            debouncedSearch();
        }
    };

    // Кнопка "Очистить поиск"
    const clearSearch = () => {
        setSearchTerm('');
        setPagination(p => ({ ...p, page: 1 }));
        fetchEvents();
    };

    // Фильтры — мгновенно
    const handleFilterChange = () => {
        setPagination(p => ({ ...p, page: 1 }));
        fetchEvents();
    };

    // Сброс всех фильтров
    const handleResetFilters = () => {
        setSearchTerm('');
        setSelectedCategory('');
        setLocation('');
        setStartDate('');
        setEndDate('');
        setPagination(p => ({ ...p, page: 1 }));
        fetchEvents();
    };

    // useEffect: реагируем на ВСЕ изменения фильтров + пагинацию
    useEffect(() => {
        fetchEvents();
    }, [fetchEvents]);

    // Очистка таймаута
    useEffect(() => {
        return () => {
            if (searchTimeoutRef.current) {
                clearTimeout(searchTimeoutRef.current);
            }
        };
    }, []);

    const handlePageChange = (newPage: number) => {
        setPagination(prev => ({ ...prev, page: newPage }));
        window.scrollTo(0, 0);
    };

    const handleJoinEvent = async (event: Event) => {
        if (!isAuthenticated || !user) {
            navigate('/login');
            return;
        }
        try {
            setJoiningId(event.id);
            await apiService.registerForEvent(event.id, user.id);
            await fetchEvents();
        } catch (err) {
            console.error('Join error:', err);
        } finally {
            setJoiningId(null);
        }
    };

    if (loading && events.length === 0) {
        return (
            <div className="min-h-screen bg-gray-50 flex items-center justify-center">
                <div className="text-center">
                    <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-gray-900 mx-auto"></div>
                    <p className="mt-4 text-gray-600">Загрузка...</p>
                </div>
            </div>
        );
    }

    const hasActiveFilters = searchTerm || selectedCategory || location || startDate || endDate;

    return (
        <div className="min-h-screen bg-gray-50">
            <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
                <div className="mb-8">
                    <div className="flex flex-col sm:flex-row gap-4 items-start sm:items-center justify-between">
                        {/* Поиск с кнопкой - исправленное выравнивание */}
                        <div className="flex-1 w-full sm:max-w-md">
                            <div className="flex rounded-md shadow-sm h-10">
                                <div className="relative flex-1">
                                    <div className="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none">
                                        <Search className="h-4 w-4 text-gray-400" />
                                    </div>
                                    <input
                                        type="text"
                                        placeholder="Поиск мероприятий..."
                                        value={searchTerm}
                                        onChange={handleSearchChange}
                                        onKeyDown={(e) => e.key === 'Enter' && triggerSearch()}
                                        className="block w-full h-full pl-10 pr-10 border border-gray-300 rounded-l-md focus:outline-none focus:ring-1 focus:ring-gray-500 focus:border-gray-500 sm:text-sm"
                                    />
                                    {searchTerm && (
                                        <button
                                            onClick={clearSearch}
                                            className="absolute inset-y-0 right-0 pr-3 flex items-center"
                                        >
                                            <X className="h-4 w-4 text-gray-400 hover:text-gray-600" />
                                        </button>
                                    )}
                                </div>
                                <button
                                    onClick={triggerSearch}
                                    className="inline-flex items-center px-4 bg-gray-900 text-white rounded-r-md hover:bg-gray-800 transition-colors whitespace-nowrap"
                                >
                                    <Search className="h-4 w-4" />
                                    <span className="hidden sm:inline ml-2">Поиск</span>
                                </button>
                            </div>
                        </div>

                        <div className="flex gap-2 w-full sm:w-auto">
                            <button
                                onClick={() => setShowFilters(!showFilters)}
                                className="inline-flex items-center px-4 py-2 border border-gray-300 rounded-md text-sm font-medium text-gray-700 bg-white hover:bg-gray-50 w-full sm:w-auto justify-center"
                            >
                                <Filter className="h-4 w-4 mr-2" />
                                Фильтры {showFilters ? '▲' : '▼'}
                            </button>

                            {hasActiveFilters && (
                                <button
                                    onClick={handleResetFilters}
                                    className="inline-flex items-center px-4 py-2 border border-gray-300 rounded-md text-sm font-medium text-gray-700 bg-white hover:bg-gray-50 w-full sm:w-auto justify-center"
                                >
                                    Сбросить
                                </button>
                            )}
                        </div>
                    </div>

                    {/* Фильтры */}
                    {showFilters && (
                        <div className="mt-6 p-5 bg-white rounded-lg border border-gray-300 shadow-sm">
                            <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-5">
                                <div>
                                    <label className="block text-sm font-medium text-gray-700 mb-2">Категория</label>
                                    <select
                                        value={selectedCategory}
                                        onChange={(e) => {
                                            setSelectedCategory(e.target.value as EventCategory | '');
                                            handleFilterChange();
                                        }}
                                        className="w-full px-3 py-2 border border-gray-300 rounded-md focus:ring-gray-500 focus:border-gray-500"
                                    >
                                        {categoryOptions.map(o => (
                                            <option key={o.value} value={o.value}>{o.label}</option>
                                        ))}
                                    </select>
                                </div>

                                <div>
                                    <label className="block text-sm font-medium text-gray-700 mb-2">
                                        <MapPin className="h-4 w-4 inline mr-1" /> Местоположение
                                    </label>
                                    <input
                                        type="text"
                                        placeholder="Город или адрес"
                                        value={location}
                                        onChange={(e) => {
                                            setLocation(e.target.value);
                                            handleFilterChange();
                                        }}
                                        className="w-full px-3 py-2 border border-gray-300 rounded-md"
                                    />
                                </div>

                                <div>
                                    <label className="block text-sm font-medium text-gray-700 mb-2">
                                        <Calendar className="h-4 w-4 inline mr-1" /> Дата от
                                    </label>
                                    <input
                                        type="date"
                                        value={startDate}
                                        onChange={(e) => {
                                            setStartDate(e.target.value);
                                            handleFilterChange();
                                        }}
                                        className="w-full px-3 py-2 border border-gray-300 rounded-md"
                                    />
                                </div>

                                <div>
                                    <label className="block text-sm font-medium text-gray-700 mb-2">
                                        <Calendar className="h-4 w-4 inline mr-1" /> Дата до
                                    </label>
                                    <input
                                        type="date"
                                        value={endDate}
                                        onChange={(e) => {
                                            setEndDate(e.target.value);
                                            handleFilterChange();
                                        }}
                                        className="w-full px-3 py-2 border border-gray-300 rounded-md"
                                    />
                                </div>
                            </div>
                        </div>
                    )}
                </div>

                {/* Результаты */}
                {error ? (
                    <div className="text-center py-12">
                        <p className="text-red-600 text-lg">{error}</p>
                        <button onClick={fetchEvents} className="mt-4 px-6 py-2 bg-gray-900 text-white rounded-md hover:bg-gray-800">
                            Повторить
                        </button>
                    </div>
                ) : events.length === 0 ? (
                    <div className="text-center py-12 bg-white rounded-lg border">
                        <p className="text-gray-700 text-lg">
                            {hasActiveFilters ? 'Ничего не найдено по вашему запросу' : 'Нет событий'}
                        </p>
                        {hasActiveFilters && (
                            <button onClick={handleResetFilters} className="mt-4 text-sm text-gray-600 hover:text-gray-900">
                                Очистить фильтры
                            </button>
                        )}
                    </div>
                ) : (
                    <>
                        <div className="mb-4 flex justify-between items-center">
                            <p className="text-gray-600">Найдено: {pagination.totalCount}</p>
                            {hasActiveFilters && (
                                <button onClick={handleResetFilters} className="text-sm text-gray-600 hover:text-gray-900">
                                    Сбросить фильтры
                                </button>
                            )}
                        </div>

                        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6 mb-8">
                            {events.map(event => (
                                <EventCard key={event.id} event={event} onJoin={handleJoinEvent} />
                            ))}
                        </div>

                        {/* Пагинация */}
                        {pagination.totalPages > 1 && (
                            <div className="flex justify-center items-center gap-2 mt-8">
                                <button
                                    onClick={() => handlePageChange(pagination.page - 1)}
                                    disabled={pagination.page === 1}
                                    className="p-2 rounded border disabled:opacity-50"
                                >
                                    <ChevronLeft className="h-5 w-5" />
                                </button>

                                {[...Array(pagination.totalPages)].map((_, i) => (
                                    <button
                                        key={i + 1}
                                        onClick={() => handlePageChange(i + 1)}
                                        className={`px-3 py-2 rounded text-sm font-medium ${
                                            i + 1 === pagination.page
                                                ? 'bg-gray-900 text-white'
                                                : 'bg-white border border-gray-300 hover:bg-gray-50'
                                        }`}
                                    >
                                        {i + 1}
                                    </button>
                                ))}

                                <button
                                    onClick={() => handlePageChange(pagination.page + 1)}
                                    disabled={pagination.page === pagination.totalPages}
                                    className="p-2 rounded border disabled:opacity-50"
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