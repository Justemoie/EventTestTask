import React, { useState, useEffect } from 'react';
import { Event, EventCategory, PageParams } from '../types';
import { apiService } from '../services/api';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '../contexts/AuthContext';
import { ArrowLeft, Plus, Calendar, Users, MapPin, Edit, Trash2, XCircle, MoreVertical } from 'lucide-react';

const MyEventsPage: React.FC = () => {
    const { user, isAuthenticated } = useAuth();
    const navigate = useNavigate();
    const [activeTab, setActiveTab] = useState<'created' | 'registered'>('created');
    const [events, setEvents] = useState<Event[]>([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string>('');
    const [processingId, setProcessingId] = useState<string | null>(null);
    const [dropdownOpen, setDropdownOpen] = useState<string | null>(null);

    const fetchMyEvents = React.useCallback(async () => {
        if (!isAuthenticated || !user) return;

        try {
            setLoading(true);
            setError('');
            const pageParams: PageParams = { page: 1, pageSize: 50 };

            let result: any;
            if (activeTab === 'created') {
                result = await apiService.getMyCreatedEvents(user.id, pageParams);
            } else {
                result = await apiService.getMyRegisteredEvents(user.id, pageParams);
            }

            const eventList =
                result?.data?.$values ??
                result?.data ??
                result?.$values ??
                (Array.isArray(result) ? result : []);

            setEvents(Array.isArray(eventList) ? eventList : []);

        } catch (err: any) {
            console.error('Error fetching my events:', err);
            setError('Ошибка загрузки событий. Проверьте подключение к серверу.');
            setEvents([]);
        } finally {
            setLoading(false);
        }
    }, [isAuthenticated, user, activeTab]);

    useEffect(() => {
        fetchMyEvents();
    }, [fetchMyEvents]);

    const handleEditEvent = (event: Event) => {
        navigate(`/update-event/${event.id}`);
        setDropdownOpen(null);
    };

    const handleDeleteEvent = async (event: Event) => {
        if (!window.confirm('Вы уверены, что хотите удалить это событие? Это действие нельзя отменить.')) {
            setDropdownOpen(null);
            return;
        }

        setProcessingId(event.id);
        try {
            await apiService.deleteEvent(event.id);
            setEvents(prev => prev.filter(e => e.id !== event.id));
        } catch (error: any) {
            alert(error.response?.data?.message || 'Ошибка при удалении события');
            console.error('Delete error:', error);
        } finally {
            setProcessingId(null);
            setDropdownOpen(null);
        }
    };

    const handleUnregister = async (event: Event) => {
        if (!window.confirm(`Вы уверены, что хотите отменить запись на "${event.title}"?`)) return;

        setProcessingId(event.id);
        try {
            await apiService.unregisterFromEvent(event.id, user!.id);
            setEvents(prev => prev.filter(e => e.id !== event.id));
            alert('Вы успешно отписались от мероприятия');
        } catch (error: any) {
            alert(error.response?.data?.message || 'Ошибка при отмене регистрации');
            console.error('Unregister error:', error);
        } finally {
            setProcessingId(null);
        }
    };

    const getEventStatus = (event: Event): { text: string; color: string } => {
        const now = new Date();
        const start = new Date(event.startDate);
        const end = new Date(event.endDate);

        if (now < start) return { text: 'Ожидает', color: 'bg-gray-100 text-gray-800 border border-gray-300' };
        if (now >= start && now <= end) return { text: 'Идёт', color: 'bg-green-100 text-green-800' };
        return { text: 'Завершено', color: 'bg-gray-100 text-gray-800' };
    };

    const formatDate = (date: string) => {
        return new Date(date).toLocaleDateString('ru-RU', {
            day: 'numeric',
            month: 'long',
            year: 'numeric'
        });
    };

    const formatTime = (date: string) => {
        return new Date(date).toLocaleTimeString('ru-RU', {
            hour: '2-digit',
            minute: '2-digit',
            timeZone: 'UTC' // Добавляем для корректного отображения времени
        });
    };

    const formatDateTimeRange = (startDate: string, endDate: string) => {
        const start = new Date(startDate);
        const end = new Date(endDate);
        
        const isSameDay = start.toDateString() === end.toDateString();
        
        if (isSameDay) {
            return `${formatDate(startDate)}, ${formatTime(startDate)} – ${formatTime(endDate)}`;
        } else {
            return `${formatDate(startDate)} ${formatTime(startDate)} – ${formatDate(endDate)} ${formatTime(endDate)}`;
        }
    };

    if (!isAuthenticated) {
        return (
            <div className="min-h-screen bg-gray-50 flex items-center justify-center">
                <div className="text-center">
                    <h2 className="text-2xl font-bold text-gray-900 mb-4">Необходима авторизация</h2>
                    <p className="text-gray-600">Войдите в систему, чтобы просматривать свои события</p>
                </div>
            </div>
        );
    }

    if (loading) {
        return (
            <div className="min-h-screen bg-gray-50 flex items-center justify-center">
                <div className="text-center">
                    <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-gray-900 mx-auto"></div>
                    <p className="mt-4 text-gray-600">Загрузка...</p>
                </div>
            </div>
        );
    }

    const isProcessing = (id: string) => processingId === id;

    return (
        <div className="min-h-screen bg-gray-50">
            <div className="max-w-6xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
                {/* Header */}
                <div className="mb-8">
                    <div className="flex items-center justify-between mb-6">
                        <button 
                            onClick={() => navigate(-1)} 
                            className="flex items-center text-gray-600 hover:text-gray-900 transition-colors"
                        >
                            <ArrowLeft className="h-5 w-5 mr-2" />
                            Назад
                        </button>
                        
                        {activeTab === 'created' && (
                            <button
                                onClick={() => navigate('/create-event')}
                                className="flex items-center px-4 py-2 bg-gray-900 text-white rounded-lg hover:bg-gray-800 transition-colors"
                            >
                                <Plus className="h-5 w-5 mr-2" />
                                Создать мероприятие
                            </button>
                        )}
                    </div>

                    <h1 className="text-3xl font-bold text-gray-900 mb-2">Мои мероприятия</h1>
                    <p className="text-gray-600 mb-6">
                        {activeTab === 'created'}
                    </p>

                    {/* Tabs */}
                    <div className="border-b border-gray-200 mb-8">
                        <nav className="-mb-px flex space-x-8">
                            <button
                                onClick={() => setActiveTab('created')}
                                className={`py-4 px-1 border-b-2 font-medium text-sm transition-colors ${
                                    activeTab === 'created'
                                        ? 'border-gray-900 text-gray-900'
                                        : 'border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300'
                                }`}
                            >
                                Созданные мной
                            </button>
                            <button
                                onClick={() => setActiveTab('registered')}
                                className={`py-4 px-1 border-b-2 font-medium text-sm transition-colors ${
                                    activeTab === 'registered'
                                        ? 'border-gray-900 text-gray-900'
                                        : 'border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300'
                                }`}
                            >
                                Мои записи
                            </button>
                        </nav>
                    </div>
                </div>

                {/* Error */}
                {error && (
                    <div className="bg-red-50 border border-red-200 text-red-700 px-4 py-3 rounded-lg mb-6 flex items-center justify-between">
                        <span>{error}</span>
                        <button 
                            onClick={fetchMyEvents} 
                            className="text-red-700 hover:text-red-800 underline text-sm"
                        >
                            Повторить
                        </button>
                    </div>
                )}

                {/* Empty State */}
                {events.length === 0 && !loading && (
                    <div className="text-center py-16 bg-white rounded-xl border border-gray-200">
                        <div className="text-gray-400 mb-4">
                            <Calendar className="h-16 w-16 mx-auto" />
                        </div>
                        <h3 className="text-xl font-medium text-gray-900 mb-2">
                            {activeTab === 'created' ? 'Нет созданных мероприятий' : 'Вы ни на что не записаны'}
                        </h3>
                        <p className="text-gray-600 mb-6 max-w-md mx-auto">
                            {activeTab === 'created'
                                ? 'Создайте своё первое мероприятие и пригласите других участников'
                                : 'Найдите интересные мероприятия на главной странице и зарегистрируйтесь'
                            }
                        </p>
                        {activeTab === 'created' ? (
                            <button
                                onClick={() => navigate('/create-event')}
                                className="inline-flex items-center px-6 py-3 bg-gray-900 text-white rounded-lg hover:bg-gray-800 transition-colors"
                            >
                                <Plus className="h-5 w-5 mr-2" />
                                Создать мероприятие
                            </button>
                        ) : (
                            <button
                                onClick={() => navigate('/')}
                                className="inline-flex items-center px-6 py-3 bg-gray-900 text-white rounded-lg hover:bg-gray-800 transition-colors"
                            >
                                Найти мероприятия
                            </button>
                        )}
                    </div>
                )}

                {/* Events List */}
                {events.length > 0 && (
                    <div className="space-y-4">
                        {events.map((event) => {
                            const status = getEventStatus(event);
                            const isOwner = activeTab === 'created';
                            const isRegistered = activeTab === 'registered';

                            return (
                                <div 
                                    key={event.id} 
                                    className="bg-white rounded-lg border border-gray-200 hover:border-gray-300 transition-colors"
                                >
                                    <div className="p-6">
                                        <div className="flex items-start justify-between">
                                            <div className="flex-1 min-w-0">
                                                <div className="flex items-start justify-between mb-3">
                                                    <div className="flex-1">
                                                        <h3 className="text-xl font-semibold text-gray-900 mb-2">
                                                            {event.title}
                                                        </h3>
                                                        <p className="text-gray-600 line-clamp-2">
                                                            {event.description}
                                                        </p>
                                                    </div>
                                                    
                                                    {/* Actions Dropdown */}
                                                    {isOwner && (
                                                        <div className="relative ml-4">
                                                            <button
                                                                onClick={() => setDropdownOpen(
                                                                    dropdownOpen === event.id ? null : event.id
                                                                )}
                                                                className="p-2 text-gray-400 hover:text-gray-600 hover:bg-gray-100 rounded-lg transition-colors"
                                                            >
                                                                <MoreVertical className="h-5 w-5" />
                                                            </button>
                                                            
                                                            {dropdownOpen === event.id && (
                                                                <div className="absolute right-0 top-full mt-1 w-48 bg-white rounded-lg border border-gray-200 shadow-lg z-10">
                                                                    <button
                                                                        onClick={() => handleEditEvent(event)}
                                                                        disabled={isProcessing(event.id)}
                                                                        className="flex items-center w-full px-4 py-2 text-sm text-gray-700 hover:bg-gray-50 disabled:opacity-50 transition-colors"
                                                                    >
                                                                        <Edit className="h-4 w-4 mr-2" />
                                                                        Редактировать
                                                                    </button>
                                                                    <button
                                                                        onClick={() => handleDeleteEvent(event)}
                                                                        disabled={isProcessing(event.id)}
                                                                        className="flex items-center w-full px-4 py-2 text-sm text-red-600 hover:bg-red-50 disabled:opacity-50 transition-colors"
                                                                    >
                                                                        <Trash2 className="h-4 w-4 mr-2" />
                                                                        Удалить
                                                                        {isProcessing(event.id) && (
                                                                            <div className="ml-2 w-3 h-3 border-2 border-red-600 border-t-transparent rounded-full animate-spin"></div>
                                                                        )}
                                                                    </button>
                                                                </div>
                                                            )}
                                                        </div>
                                                    )}
                                                </div>

                                                {/* Event Details */}
                                                <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4 text-sm">
                                                    <div className="flex items-center text-gray-600">
                                                        <Calendar className="h-4 w-4 mr-2 text-gray-400" />
                                                        <span className="text-sm">
                                                            {formatDateTimeRange(event.startDate, event.endDate)}
                                                        </span>
                                                    </div>
                                                    
                                                    {event.location && (
                                                        <div className="flex items-center text-gray-600">
                                                            <MapPin className="h-4 w-4 mr-2 text-gray-400" />
                                                            <span className="text-sm">{event.location}</span>
                                                        </div>
                                                    )}
                                                    
                                                    <div className="flex items-center text-gray-600">
                                                        <Users className="h-4 w-4 mr-2 text-gray-400" />
                                                        <span className="text-sm">
                                                            {event.participants?.length || 0}
                                                            {event.maxParticipants > 0 ? ` / ${event.maxParticipants}` : ''} участников
                                                        </span>
                                                    </div>
                                                    
                                                    <div className="flex items-center justify-between">
                                                        <span className={`inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium ${status.color}`}>
                                                            {status.text}
                                                        </span>
                                                    </div>
                                                </div>
                                            </div>
                                        </div>

                                        {/* Unregister Button for registered events */}
                                        {isRegistered && (
                                            <div className="mt-4 pt-4 border-t border-gray-100">
                                                <button
                                                    onClick={() => handleUnregister(event)}
                                                    disabled={isProcessing(event.id)}
                                                    className="flex items-center justify-center w-full px-4 py-2 text-sm bg-gray-800 hover:bg-gray-900 text-white rounded-lg disabled:opacity-50 transition-colors"
                                                >
                                                    <XCircle className="h-4 w-4 mr-2" />
                                                    Отменить запись
                                                    {isProcessing(event.id) && (
                                                        <div className="ml-2 w-4 h-4 border-2 border-gray-600 border-t-transparent rounded-full animate-spin"></div>
                                                    )}
                                                </button>
                                            </div>
                                        )}
                                    </div>
                                </div>
                            );
                        })}
                    </div>
                )}
            </div>
        </div>
    );
};

export default MyEventsPage;