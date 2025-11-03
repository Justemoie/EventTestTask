import React, { useState, useEffect } from 'react';
import { Event, EventCategory, PageParams } from '../types';
import { apiService } from '../services/api';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '../contexts/AuthContext';
import { ArrowLeft, Plus, Calendar, Users } from 'lucide-react';

const MyEventsPage: React.FC = () => {
    const { user, isAuthenticated } = useAuth();
    const navigate = useNavigate();
    const [activeTab, setActiveTab] = useState<'created' | 'registered'>('created');
    const [events, setEvents] = useState<Event[]>([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string>('');

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
    };

    const handleDeleteEvent = async (event: Event) => {
        if (window.confirm('Вы уверены, что хотите удалить это событие?')) {
            try {
                await apiService.deleteEvent(event.id);
                setEvents(prev => prev.filter(e => e.id !== event.id));
            } catch (error) {
                console.error('Error deleting event:', error);
            }
        }
    };

    const getEventStatus = (event: Event): { text: string; color: string } => {
        const now = new Date();
        const startDate = new Date(event.startDate);
        const endDate = new Date(event.endDate);

        if (now < startDate) {
            return { text: 'Ожидает', color: 'bg-gray-100 text-gray-800 border border-gray-300' };
        } else if (now >= startDate && now <= endDate) {
            return { text: 'Активно', color: 'bg-gray-100 text-gray-800 border border-gray-300' };
        } else {
            return { text: 'Завершено', color: 'bg-gray-100 text-gray-800 border border-gray-300' };
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
                    <p className="mt-4 text-gray-600">Загрузка событий...</p>
                </div>
            </div>
        );
    }

    return (
        <div className="min-h-screen bg-gray-50">
            <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
                {/* Header */}
                <div className="mb-8">
                    <div className="flex items-center mb-6">
                        <button 
                            onClick={() => navigate(-1)}
                            className="flex items-center text-gray-600 hover:text-gray-900 mr-4"
                        >
                            <ArrowLeft className="h-5 w-5 mr-2" />
                            Вернуться
                        </button>
                    </div>

                    <h1 className="text-3xl font-bold text-gray-900 mb-2">Мои мероприятия</h1>

                    {/* Tabs */}
                    <div className="border-b border-gray-300">
                        <nav className="-mb-px flex space-x-8">
                            <button
                                onClick={() => setActiveTab('created')}
                                className={`py-2 px-1 border-b-2 font-medium text-sm ${activeTab === 'created'
                                    ? 'border-gray-900 text-gray-900'
                                    : 'border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300'
                                    }`}
                            >
                                Созданные мероприятия
                            </button>
                            <button
                                onClick={() => setActiveTab('registered')}
                                className={`py-2 px-1 border-b-2 font-medium text-sm ${activeTab === 'registered'
                                    ? 'border-gray-900 text-gray-900'
                                    : 'border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300'
                                    }`}
                            >
                                Записанные мероприятия
                            </button>
                        </nav>
                    </div>
                </div>

                {/* Content */}
                {error ? (
                    <div className="text-center py-12">
                        <div className="text-gray-900 text-lg font-medium mb-2">Ошибка загрузки</div>
                        <p className="text-gray-600 mb-4">{error}</p>
                        <button
                            onClick={fetchMyEvents}
                            className="inline-flex items-center px-4 py-2 border border-transparent text-sm font-medium rounded-md text-white bg-gray-900 hover:bg-gray-800 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-gray-500"
                        >
                            Попробовать снова
                        </button>
                    </div>
                ) : !events || events.length === 0 ? (
                    <div className="text-center py-12">
                        <div className="text-gray-900 text-lg font-medium mb-2">
                            {activeTab === 'created' ? 'У вас нет созданных мероприятий' : 'Вы не записаны ни на одно мероприятие'}
                        </div>
                        <p className="text-gray-600 mb-6">
                            {activeTab === 'created'
                                ? 'Создайте свое первое мероприятие'
                                : 'Найдите интересные мероприятия и запишитесь на них'
                            }
                        </p>
                        {activeTab === 'created' && (
                            <button 
                                onClick={() => navigate('/create-event')}
                                className="inline-flex items-center px-4 py-2 border border-transparent text-sm font-medium rounded-md text-white bg-gray-900 hover:bg-gray-800 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-gray-500"
                            >
                                <Plus className="h-4 w-4 mr-2" />
                                Создать мероприятие
                            </button>
                        )}
                    </div>
                ) : (
                    <div className="space-y-6">
                        {events.map((event) => {
                            const status = getEventStatus(event);
                            return (
                                <div key={event.id} className="bg-white rounded-lg border border-gray-300 p-6">
                                    <div className="flex items-start justify-between">
                                        <div className="flex-1">
                                            <div className="flex items-center justify-between mb-4">
                                                <h3 className="text-xl font-semibold text-gray-900">{event.title}</h3>
                                                <div className="flex items-center space-x-2">
                                                    <button
                                                        onClick={() => handleEditEvent(event)}
                                                        className="inline-flex items-center px-3 py-2 border border-gray-400 shadow-sm text-sm leading-4 font-medium rounded-md text-gray-700 bg-white hover:bg-gray-50 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-gray-500"
                                                    >
                                                        Редактировать
                                                    </button>
                                                    <button
                                                        onClick={() => handleDeleteEvent(event)}
                                                        className="inline-flex items-center px-3 py-2 border border-gray-400 shadow-sm text-sm leading-4 font-medium rounded-md text-gray-700 bg-white hover:bg-gray-50 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-gray-500"
                                                    >
                                                        Удалить
                                                    </button>
                                                </div>
                                            </div>

                                            <div className="grid grid-cols-1 md:grid-cols-4 gap-4 mb-4">
                                                <div className="flex items-center text-gray-700">
                                                    <Calendar className="h-4 w-4 mr-2 flex-shrink-0" />
                                                    <span className="text-sm">
                                                        {new Date(event.startDate).toLocaleDateString('ru-RU', {
                                                            day: 'numeric',
                                                            month: 'long',
                                                            year: 'numeric'
                                                        })}
                                                    </span>
                                                </div>

                                                <div className="flex items-center text-gray-700">
                                                    <Calendar className="h-4 w-4 mr-2 flex-shrink-0" />
                                                    <span className="text-sm">
                                                        {new Date(event.startDate).toLocaleTimeString('ru-RU', {
                                                            hour: '2-digit',
                                                            minute: '2-digit'
                                                        })} - {new Date(event.endDate).toLocaleTimeString('ru-RU', {
                                                            hour: '2-digit',
                                                            minute: '2-digit'
                                                        })}
                                                    </span>
                                                </div>

                                                <div className="flex items-center text-gray-700">
                                                    <Calendar className="h-4 w-4 mr-2 flex-shrink-0" />
                                                    <span className="text-sm truncate">{event.location}</span>
                                                </div>

                                                <div className="flex items-center text-gray-700">
                                                    <Users className="h-4 w-4 mr-2 flex-shrink-0" />
                                                    <span className="text-sm">
                                                        {event.participants.length} участников записано
                                                    </span>
                                                </div>
                                            </div>

                                            <div className="flex items-center justify-between">
                                                <span className={`inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium ${status.color}`}>
                                                    {status.text}
                                                </span>
                                            </div>
                                        </div>
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