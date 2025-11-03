import React from 'react';
import { Event, EventCategory } from '../types';
import { Calendar, Clock, MapPin, Users, Edit, Trash2 } from 'lucide-react';

interface EventCardProps {
    event: Event;
    onEdit?: (event: Event) => void;
    onDelete?: (event: Event) => void;
    onJoin?: (event: Event) => void;
    showActions?: boolean;
}

const EventCard: React.FC<EventCardProps> = ({
    event,
    onEdit,
    onDelete,
    onJoin,
    showActions = false
}) => {
    const getCategoryName = (category: EventCategory): string => {
        const categoryNames = {
            [EventCategory.Conference]: 'Конференция',
            [EventCategory.Workshop]: 'Воркшоп',
            [EventCategory.Webinar]: 'Вебинар',
            [EventCategory.Meetup]: 'Митап',
            [EventCategory.Party]: 'Вечеринка',
            [EventCategory.Sport]: 'Спорт',
            [EventCategory.Other]: 'Другое'
        };
        return categoryNames[category] || 'Другое';
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

    const getImageSrc = (image: any): string | undefined => {
        if (!image) return undefined;
        
        try {
            if (typeof image === 'string' && image.startsWith('data:')) {
                return image;
            }
            
            if (typeof image === 'string') {
                if (image.length > 100 && /^[A-Za-z0-9+/=]+$/.test(image)) {
                    return `data:image/jpeg;base64,${image}`;
                }
                return undefined;
            }
            
            let byteArray: number[];
            if (image instanceof Uint8Array) {
                byteArray = Array.from(image);
            } else if (Array.isArray(image)) {
                byteArray = image;
            } else {
                console.warn('Unknown image format:', typeof image, image);
                return undefined;
            }
            
            if (byteArray.length === 0) return undefined;
            
            let binary = '';
            const chunkSize = 8192;
            
            for (let i = 0; i < byteArray.length; i += chunkSize) {
                const chunk = byteArray.slice(i, i + chunkSize);
                binary += String.fromCharCode.apply(null, chunk);
            }
            
            return `data:image/jpeg;base64,${btoa(binary)}`;
        } catch (error) {
            console.error('Error converting image:', error);
            return undefined;
        }
    };

    const status = getEventStatus();

    return (
        <div className="bg-white rounded-lg border border-gray-300 hover:border-gray-400 transition-colors duration-200 overflow-hidden">
            <div className="h-48 bg-gray-100 flex items-center justify-center">
                {event.image ? (
                    <img
                        src={getImageSrc(event.image)}
                        alt={event.title}
                        className="w-full h-full object-cover"
                    />
                ) : (
                    <div className="text-gray-500 text-center">
                        <div className="text-4xl mb-2">📅</div>
                        <div className="text-sm">Изображение события</div>
                    </div>
                )}
            </div>

            <div className="p-6">
                <h3 className="text-xl font-semibold text-gray-900 mb-3 line-clamp-2">
                    {event.title}
                </h3>

                <div className="space-y-2 mb-4">
                    <div className="flex items-center text-gray-700">
                        <Calendar className="h-4 w-4 mr-2 flex-shrink-0" />
                        <span className="text-sm">{formatDate(event.startDate)}</span>
                    </div>
                    <div className="flex items-center text-gray-700">
                        <Clock className="h-4 w-4 mr-2 flex-shrink-0" />
                        <span className="text-sm">
                            {formatTime(event.startDate)} - {formatTime(event.endDate)}
                        </span>
                    </div>
                    <div className="flex items-center text-gray-700">
                        <MapPin className="h-4 w-4 mr-2 flex-shrink-0" />
                        <span className="text-sm truncate">{event.location}</span>
                    </div>
                    <div className="flex items-center text-gray-700">
                        <Users className="h-4 w-4 mr-2 flex-shrink-0" />
                        <span className="text-sm">
                            {event.participants.length} участников записано
                            {event.maxParticipants > 0 && ` из ${event.maxParticipants}`}
                        </span>
                    </div>
                </div>

                <p className="text-gray-700 text-sm mb-4 line-clamp-3">
                    {event.description}
                </p>

                <div className="flex items-center justify-between mb-4">
                    <span className="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium bg-gray-100 text-gray-800 border border-gray-300">
                        {getCategoryName(event.category)}
                    </span>
                    <span className={`inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium ${status.color}`}>
                        {status.text}
                    </span>
                </div>

                <div className="flex items-center justify-between">
                    {showActions ? (
                        <div className="flex space-x-2">
                            <button
                                onClick={() => onEdit?.(event)}
                                className="inline-flex items-center px-3 py-2 border border-gray-400 shadow-sm text-sm leading-4 font-medium rounded-md text-gray-700 bg-white hover:bg-gray-50 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-gray-500"
                            >
                                <Edit className="h-4 w-4 mr-1" />
                                Редактировать
                            </button>
                            <button
                                onClick={() => onDelete?.(event)}
                                className="inline-flex items-center px-3 py-2 border border-gray-400 shadow-sm text-sm leading-4 font-medium rounded-md text-gray-700 bg-white hover:bg-gray-50 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-gray-500"
                            >
                                <Trash2 className="h-4 w-4 mr-1" />
                                Удалить
                            </button>
                        </div>
                    ) : (
                        <button
                            onClick={() => onJoin?.(event)}
                            className="w-full bg-gray-800 hover:bg-gray-900 text-white font-medium py-2 px-4 rounded-md transition-colors duration-200 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-gray-500"
                        >
                            Присоединиться
                        </button>
                    )}
                </div>
            </div>
        </div>
    );
};

export default EventCard;