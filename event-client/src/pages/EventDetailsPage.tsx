import React, { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { Event, Registration, User } from '../types';
import { apiService } from '../services/api';
import { ArrowLeft, Calendar, Clock, MapPin, Users, User as UserIcon, Share } from 'lucide-react';
import { useAuth } from '../contexts/AuthContext';

const EventDetailsPage: React.FC = () => {
    const { eventId } = useParams<{ eventId: string }>();
    const navigate = useNavigate();
    const { user, isAuthenticated } = useAuth();
    const [event, setEvent] = useState<Event | null>(null);
    const [participants, setParticipants] = useState<User[]>([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string>('');
    const [joining, setJoining] = useState(false);

    useEffect(() => {
        if (eventId) {
            fetchEventData();
        }
    }, [eventId]);

    const fetchEventData = async () => {
        try {
            setLoading(true);
            setError('');
            
            // Получаем данные события
            const eventData = await apiService.getEventById(eventId!);
            setEvent(eventData);
            
            // Получаем участников события
            try {
                const participantsData = await apiService.getEventParticipants(eventId!);
                setParticipants(Array.isArray(participantsData) ? participantsData : []);
            } catch (participantsError) {
                console.error('Error fetching participants:', participantsError);
                setParticipants([]);
            }
        } catch (err: any) {
            console.error('Error fetching event:', err);
            setError('Ошибка загрузки события');
        } finally {
            setLoading(false);
        }
    };

    const handleJoinEvent = async () => {
        if (!isAuthenticated || !user) {
            navigate('/login');
            return;
        }

        try {
            setJoining(true);
            await apiService.registerForEvent(eventId!, user.id);
            // Обновляем данные после записи
            await fetchEventData();
        } catch (error) {
            console.error('Error joining event:', error);
        } finally {
            setJoining(false);
        }
    };

    const handleUnregister = async () => {
        if (!isAuthenticated || !user) return;

        try {
            setJoining(true);
            await apiService.unregisterFromEvent(eventId!, user.id);
            // Обновляем данные после отмены регистрации
            await fetchEventData();
        } catch (error) {
            console.error('Error unregistering from event:', error);
        } finally {
            setJoining(false);
        }
    };

    const formatDate = (dateString: string): string => {
        const date = new Date(dateString);
        return date.toLocaleDateString('ru-RU', {
            day: 'numeric',
            month: 'long',
            year: 'numeric'
        });
    };

    const formatTime = (dateString: string): string => {
        const date = new Date(dateString);
        return date.toLocaleTimeString('ru-RU', {
            hour: '2-digit',
            minute: '2-digit'
        });
    };

    const getEventStatus = (): { text: string; color: string } => {
        if (!event) return { text: '', color: '' };
        
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

    const isUserRegistered = (): boolean => {
        if (!user) return false;
        return participants.some(participant => participant.id === user.id);
    };

    const getImageSrc = (image: any): string | undefined => {
        if (!image) return undefined;
        
        try {
            if (typeof image === 'string') {
                if (image.startsWith('data:') || image.startsWith('http')) {
                    return image;
                }
                if (image.length > 100 && /^[A-Za-z0-9+/=]+$/.test(image)) {
                    return `data:image/jpeg;base64,${image}`;
                }
                return undefined;
            }
            
            let byteArray: number[] = [];
            
            if (image instanceof Uint8Array) {
                byteArray = Array.from(image);
            } else if (Array.isArray(image)) {
                byteArray = image;
            } else if (image.$values && Array.isArray(image.$values)) {
                byteArray = image.$values;
            } else {
                return undefined;
            }
            
            if (byteArray.length === 0) return undefined;
            
            const binary = byteArray.map(byte => String.fromCharCode(byte)).join('');
            return `data:image/jpeg;base64,${btoa(binary)}`;
        } catch (error) {
            console.error('Error processing image:', error);
            return undefined;
        }
    };

    if (loading) {
        return (
            <div className="min-h-screen bg-gray-50 flex items-center justify-center">
                <div className="text-center">
                    <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-gray-900 mx-auto"></div>
                    <p className="mt-4 text-gray-600">Загрузка события...</p>
                </div>
            </div>
        );
    }

    if (error || !event) {
        return (
            <div className="min-h-screen bg-gray-50 flex items-center justify-center">
                <div className="text-center">
                    <h2 className="text-2xl font-bold text-gray-900 mb-4">Ошибка</h2>
                    <p className="text-gray-600 mb-4">{error || 'Событие не найдено'}</p>
                    <button
                        onClick={() => navigate('/')}
                        className="inline-flex items-center px-4 py-2 border border-transparent text-sm font-medium rounded-md text-white bg-gray-900 hover:bg-gray-800 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-gray-500"
                    >
                        Вернуться к мероприятиям
                    </button>
                </div>
            </div>
        );
    }

    const status = getEventStatus();
    const userRegistered = isUserRegistered();

    return (
        <div className="min-h-screen bg-gray-50">
            <div className="max-w-4xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
                {/* Header */}
                <div className="flex items-center justify-between mb-6">
                    <button
                        onClick={() => navigate(-1)}
                        className="flex items-center text-gray-600 hover:text-gray-900"
                    >
                        <ArrowLeft className="h-5 w-5 mr-2" />
                        Назад
                    </button>
                    
                    <div className="flex items-center space-x-4">
                        <span className={`inline-flex items-center px-3 py-1 rounded-full text-sm font-medium ${status.color}`}>
                            {status.text}
                        </span>
                        <button className="flex items-center text-gray-600 hover:text-gray-900">
                            <Share className="h-5 w-5" />
                        </button>
                    </div>
                </div>

                {/* Event Image */}
                <div className="bg-white rounded-lg border border-gray-300 overflow-hidden mb-6">
                    <div className="h-96 bg-gray-200 flex items-center justify-center">
                        {event.image ? (
                            <img
                                src={getImageSrc(event.image)}
                                alt={event.title}
                                className="w-full h-full object-cover"
                            />
                        ) : (
                            <div className="text-gray-500 text-center">
                                <div className="text-6xl mb-4">📅</div>
                                <div className="text-lg">Изображение события</div>
                            </div>
                        )}
                    </div>
                </div>

                <div className="grid grid-cols-1 lg:grid-cols-3 gap-8">
                    {/* Main Content */}
                    <div className="lg:col-span-2">
                        <div className="bg-white rounded-lg border border-gray-300 p-6">
                            <h1 className="text-3xl font-bold text-gray-900 mb-4">{event.title}</h1>
                            
                            <div className="space-y-4 mb-6">
                                <div className="flex items-center text-gray-700">
                                    <Calendar className="h-5 w-5 mr-3 flex-shrink-0" />
                                    <div>
                                        <div className="font-medium">{formatDate(event.startDate)}</div>
                                        <div className="text-sm text-gray-500">
                                            {formatTime(event.startDate)} - {formatTime(event.endDate)}
                                        </div>
                                    </div>
                                </div>
                                
                                <div className="flex items-center text-gray-700">
                                    <MapPin className="h-5 w-5 mr-3 flex-shrink-0" />
                                    <span className="font-medium">{event.location}</span>
                                </div>
                                
                                <div className="flex items-center text-gray-700">
                                    <Users className="h-5 w-5 mr-3 flex-shrink-0" />
                                    <span>
                                        {participants.length} участников записано
                                        {event.maxParticipants > 0 && ` из ${event.maxParticipants}`}
                                    </span>
                                </div>
                            </div>

                            <div className="prose max-w-none">
                                <h3 className="text-lg font-semibold text-gray-900 mb-3">Описание</h3>
                                <p className="text-gray-700 leading-relaxed whitespace-pre-line">
                                    {event.description}
                                </p>
                            </div>
                        </div>
                    </div>

                    {/* Sidebar */}
                    <div className="space-y-6">
                        {/* Join/Unregister Button */}
                        <div className="bg-white rounded-lg border border-gray-300 p-6">
                            {userRegistered ? (
                                <div className="text-center">
                                    <div className="text-green-600 text-lg font-semibold mb-2">
                                        ✅ Вы записаны на это мероприятие
                                    </div>
                                    <p className="text-gray-600 text-sm mb-4">
                                        Мы напомним вам о мероприятии заранее
                                    </p>
                                    <button
                                        onClick={handleUnregister}
                                        disabled={joining || status.text === 'Завершено'}
                                        className="w-full bg-gray-300 hover:bg-gray-400 text-gray-700 font-medium py-2 px-4 rounded-md transition-colors duration-200 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-gray-500 disabled:opacity-50 disabled:cursor-not-allowed"
                                    >
                                        {joining ? 'Отмена...' : 'Отменить запись'}
                                    </button>
                                </div>
                            ) : (
                                <button
                                    onClick={handleJoinEvent}
                                    disabled={joining || status.text === 'Завершено'}
                                    className="w-full bg-gray-900 hover:bg-gray-800 text-white font-medium py-3 px-4 rounded-md transition-colors duration-200 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-gray-500 disabled:opacity-50 disabled:cursor-not-allowed"
                                >
                                    {joining ? 'Записываем...' : 'Записаться на мероприятие'}
                                </button>
                            )}
                            
                            {status.text === 'Завершено' && (
                                <p className="text-gray-500 text-sm mt-3 text-center">
                                    Это мероприятие уже завершено
                                </p>
                            )}
                        </div>

                        {/* Participants */}
                        <div className="bg-white rounded-lg border border-gray-300 p-6">
                            <h3 className="text-lg font-semibold text-gray-900 mb-4">
                                Участники ({participants.length})
                            </h3>
                            
                            {participants.length === 0 ? (
                                <p className="text-gray-500 text-center py-4">
                                    Пока никто не записался
                                </p>
                            ) : (
                                <div className="space-y-3">
                                    {participants.slice(0, 10).map((participant) => (
                                        <div key={participant.id} className="flex items-center">
                                            <div className="w-8 h-8 bg-gray-300 rounded-full flex items-center justify-center mr-3">
                                                <UserIcon className="h-4 w-4 text-gray-600" />
                                            </div>
                                            <div>
                                                <div className="text-sm font-medium text-gray-900">
                                                    {participant.firstName} {participant.lastName}
                                                </div>
                                                <div className="text-xs text-gray-500">
                                                    {participant.email}
                                                </div>
                                            </div>
                                        </div>
                                    ))}
                                    
                                    {participants.length > 10 && (
                                        <p className="text-gray-500 text-sm text-center pt-2">
                                            и еще {participants.length - 10} участников
                                        </p>
                                    )}
                                </div>
                            )}
                        </div>
                    </div>
                </div>
            </div>
        </div>
    );
};

export default EventDetailsPage;