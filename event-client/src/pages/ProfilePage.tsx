import React, { useState, useEffect } from 'react';
import { useAuth } from '../contexts/AuthContext';
import { User, UpdateUserRequest, Event, PageParams } from '../types';
import { apiService } from '../services/api';
import { Calendar, User as UserIcon, LogOut, Edit, Users, CheckCircle, TrendingUp } from 'lucide-react';

const ProfilePage: React.FC = () => {
    const { user, logout, updateUser, isAuthenticated } = useAuth();
    const [isEditing, setIsEditing] = useState(false);
    const [editData, setEditData] = useState<Partial<User>>({});
    const [loading, setLoading] = useState(false);
    const [error, setError] = useState<string>('');
    const [registeredEvents, setRegisteredEvents] = useState<Event[]>([]);
    const [createdEvents, setCreatedEvents] = useState<Event[]>([]);
    const [statsLoading, setStatsLoading] = useState(true);

    useEffect(() => {
        if (user) {
            setEditData({
                firstName: user.firstName,
                lastName: user.lastName,
                email: user.email,
                birthDate: user.birthDate
            });
            loadUserStats();
        }
    }, [user]);

    const loadUserStats = async () => {
        if (!user) return;

        try {
            setStatsLoading(true);
            const pageParams: PageParams = { page: 1, pageSize: 50 };

            // Загружаем созданные события
            const createdResult = await apiService.getMyCreatedEvents(user.id, pageParams);
            const createdEventsList = getEventsFromResponse(createdResult);
            setCreatedEvents(createdEventsList);

            // Загружаем зарегистрированные события
            const registeredResult = await apiService.getMyRegisteredEvents(user.id, pageParams);
            const registeredEventsList = getEventsFromResponse(registeredResult);
            setRegisteredEvents(registeredEventsList);

        } catch (err) {
            console.error('Error loading user stats:', err);
        } finally {
            setStatsLoading(false);
        }
    };

    const getEventsFromResponse = (result: any): Event[] => {
        if (result && typeof result === 'object') {
            if (Array.isArray((result as any).$values)) {
                return (result as any).$values;
            } else if (Array.isArray((result as any).data)) {
                return (result as any).data;
            } else if (Array.isArray(result)) {
                return result;
            } else if ((result as any).data?.$values) {
                return (result as any).data.$values;
            }
        }
        return [];
    };

    // Статистика
    const calculateStats = () => {
        const now = new Date();
        
        // Количество созданных событий
        const createdCount = createdEvents.length;
        
        // Количество зарегистрированных событий
        const registeredCount = registeredEvents.length;
        
        // Общее количество участников в созданных событиях
        const totalParticipants = createdEvents.reduce((total, event) => 
            total + (event.participants?.length || 0), 0
        );
        
        // Количество завершенных созданных событий
        const completedCreatedEvents = createdEvents.filter(event => 
            new Date(event.endDate) < now
        ).length;
        
        // Количество завершенных зарегистрированных событий
        const completedRegisteredEvents = registeredEvents.filter(event => 
            new Date(event.endDate) < now
        ).length;

        return {
            createdEvents: createdCount,
            registeredEvents: registeredCount,
            totalParticipants,
            completedCreatedEvents,
            completedRegisteredEvents,
            completionRate: createdCount > 0 ? Math.round((completedCreatedEvents / createdCount) * 100) : 0
        };
    };

    const stats = calculateStats();

    const handleEdit = () => {
        setIsEditing(true);
    };

    const handleSave = async () => {
        if (!user) return;
      
        setLoading(true);
        setError('');
      
        try {
            const updateData: UpdateUserRequest = {
                firstName: editData.firstName || user.firstName || '',
                lastName: editData.lastName || user.lastName || '',
                email: editData.email || user.email || '',
                birthDate: editData.birthDate || user.birthDate || ''
            };
            
            await updateUser(updateData);
            setIsEditing(false);
        } catch (err: any) {
            setError(err.response?.data?.message || 'Ошибка обновления профиля');
        } finally {
            setLoading(false);
        }
    };

    const handleCancel = () => {
        setEditData({
            firstName: user?.firstName || '',
            lastName: user?.lastName || '',
            email: user?.email || '',
            birthDate: user?.birthDate || ''
        });
        setIsEditing(false);
        setError('');
    };

    const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
        const { name, value } = e.target;
        setEditData(prev => ({
            ...prev,
            [name]: value
        }));
    };

    const handleLogout = async () => {
        try {
            await logout();
        } catch (error) {
            console.error('Logout error:', error);
        }
    };

    const formatDate = (dateString: string) => {
        return new Date(dateString).toLocaleDateString('ru-RU', {
            day: 'numeric',
            month: 'long',
            year: 'numeric'
        });
    };

    const getEventStatus = (event: Event) => {
        const now = new Date();
        const start = new Date(event.startDate);
        const end = new Date(event.endDate);

        if (now < start) return { text: 'Ожидает', color: 'bg-gray-200 text-gray-900' };
        if (now >= start && now <= end) return { text: 'Идёт', color: 'bg-gray-100 text-gray-800' };
        return { text: 'Завершено', color: 'bg-gray-100 text-gray-800' };
    };

    if (!isAuthenticated || !user) {
        return (
            <div className="min-h-screen bg-gray-50 flex items-center justify-center">
                <div className="text-center">
                    <h2 className="text-2xl font-bold text-gray-900 mb-4">Необходима авторизация</h2>
                    <p className="text-gray-600">Войдите в систему, чтобы просматривать профиль</p>
                </div>
            </div>
        );
    }

    return (
        <div className="min-h-screen bg-gray-50">
            <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
                {/* Header */}
                <div className="mb-8">
                    <h1 className="text-3xl font-bold text-gray-900 mb-2">Профиль пользователя</h1>
                </div>

                <div className="grid grid-cols-1 lg:grid-cols-3 gap-8">
                    {/* Left Column - Profile Info */}
                    <div className="lg:col-span-1 space-y-6">
                        {/* User Profile Card */}
                        <div className="bg-white rounded-lg border border-gray-300 p-6">
                            <div className="text-center">
                                <div className="w-20 h-20 bg-gray-300 rounded-full flex items-center justify-center mx-auto mb-4">
                                    <UserIcon className="h-10 w-10 text-gray-600" />
                                </div>
                                {isEditing ? (
                                    <div className="space-y-4">
                                        <div>
                                            <input
                                                type="text"
                                                name="firstName"
                                                value={editData.firstName || ''}
                                                onChange={handleChange}
                                                className="w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm focus:outline-none focus:ring-gray-500 focus:border-gray-500 sm:text-sm"
                                                placeholder="Имя"
                                            />
                                        </div>
                                        <div>
                                            <input
                                                type="text"
                                                name="lastName"
                                                value={editData.lastName || ''}
                                                onChange={handleChange}
                                                className="w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm focus:outline-none focus:ring-gray-500 focus:border-gray-500 sm:text-sm"
                                                placeholder="Фамилия"
                                            />
                                        </div>
                                        <div>
                                            <input
                                                type="email"
                                                name="email"
                                                value={editData.email || ''}
                                                onChange={handleChange}
                                                className="w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm focus:outline-none focus:ring-gray-500 focus:border-gray-500 sm:text-sm"
                                                placeholder="Email"
                                            />
                                        </div>
                                        <div>
                                            <input
                                                type="date"
                                                name="birthDate"
                                                value={editData.birthDate || ''}
                                                onChange={handleChange}
                                                className="w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm focus:outline-none focus:ring-gray-500 focus:border-gray-500 sm:text-sm"
                                            />
                                        </div>
                                        {error && (
                                            <div className="text-gray-700 text-sm">{error}</div>
                                        )}
                                        <div className="flex space-x-2">
                                            <button
                                                onClick={handleSave}
                                                disabled={loading}
                                                className="flex-1 bg-gray-900 text-white px-4 py-2 rounded-md text-sm font-medium hover:bg-gray-800 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-gray-500 disabled:opacity-50"
                                            >
                                                {loading ? 'Сохранение...' : 'Сохранить'}
                                            </button>
                                            <button
                                                onClick={handleCancel}
                                                className="flex-1 bg-gray-300 text-gray-700 px-4 py-2 rounded-md text-sm font-medium hover:bg-gray-400 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-gray-500"
                                            >
                                                Отмена
                                            </button>
                                        </div>
                                    </div>
                                ) : (
                                    <>
                                        <h2 className="text-xl font-semibold text-gray-900 mb-1">
                                            {user.firstName} {user.lastName}
                                        </h2>
                                        <p className="text-gray-600 mb-4">{user.email}</p>
                                        <div className="space-y-2">
                                            <button
                                                onClick={handleEdit}
                                                className="w-full inline-flex items-center justify-center px-4 py-2 border border-gray-400 shadow-sm text-sm font-medium rounded-md text-gray-700 bg-white hover:bg-gray-50 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-gray-500"
                                            >
                                                <Edit className="h-4 w-4 mr-2" />
                                                Редактировать профиль
                                            </button>
                                            <button
                                                onClick={handleLogout}
                                                className="w-full inline-flex items-center justify-center px-4 py-2 border border-gray-400 shadow-sm text-sm font-medium rounded-md text-gray-700 bg-white hover:bg-gray-50 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-gray-500"
                                            >
                                                <LogOut className="h-4 w-4 mr-2" />
                                                Выйти
                                            </button>
                                        </div>
                                    </>
                                )}
                            </div>
                        </div>

                        {/* Account Information */}
                        <div className="bg-white rounded-lg border border-gray-300 p-6">
                            <h3 className="text-lg font-medium text-gray-900 mb-4">Информация аккаунта</h3>
                            <div className="space-y-3">
                                <div className="flex justify-between">
                                    <span className="text-sm text-gray-600">Дата рождения:</span>
                                    <span className="text-sm font-medium text-gray-900">
                                        {formatDate(user.birthDate)}
                                    </span>
                                </div>
                                <div className="flex justify-between">
                                    <span className="text-sm text-gray-600">Статус:</span>
                                    <span className="text-sm font-medium text-gray-900">Активный</span>
                                </div>
                                <div className="flex justify-between">
                                    <span className="text-sm text-gray-600">Роль:</span>
                                    <span className="text-sm font-medium text-gray-900 capitalize">{user.role || 'User'}</span>
                                </div>
                            </div>
                        </div>
                    </div>

                    {/* Right Column - Activity and Events */}
                    <div className="lg:col-span-2 space-y-6">
                        {/* My Activity */}
                        <div className="bg-white rounded-lg border border-gray-300 p-6">
                            <h3 className="text-lg font-medium text-gray-900 mb-4">Моя активность</h3>
                            {statsLoading ? (
                                <div className="text-center py-8">
                                    <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-gray-900 mx-auto"></div>
                                    <p className="mt-2 text-gray-600">Загрузка статистики...</p>
                                </div>
                            ) : (
                                <div className="grid grid-cols-2 lg:grid-cols-5 gap-4">
                                    <div className="text-center">
                                        <div className="w-12 h-12 bg-blue-100 rounded-lg flex items-center justify-center mx-auto mb-2">
                                            <Calendar className="h-6 w-6 text-blue-600" />
                                        </div>
                                        <div className="text-2xl font-bold text-gray-900">{stats.createdEvents}</div>
                                        <div className="text-sm text-gray-600">Созданных событий</div>
                                    </div>
                                    <div className="text-center">
                                        <div className="w-12 h-12 bg-green-100 rounded-lg flex items-center justify-center mx-auto mb-2">
                                            <Users className="h-6 w-6 text-green-600" />
                                        </div>
                                        <div className="text-2xl font-bold text-gray-900">{stats.registeredEvents}</div>
                                        <div className="text-sm text-gray-600">Участий в событиях</div>
                                    </div>
                                    <div className="text-center">
                                        <div className="w-12 h-12 bg-purple-100 rounded-lg flex items-center justify-center mx-auto mb-2">
                                            <Users className="h-6 w-6 text-purple-600" />
                                        </div>
                                        <div className="text-2xl font-bold text-gray-900">{stats.totalParticipants}</div>
                                        <div className="text-sm text-gray-600">Участников в моих событиях</div>
                                    </div>
                                    <div className="text-center">
                                        <div className="w-12 h-12 bg-orange-100 rounded-lg flex items-center justify-center mx-auto mb-2">
                                            <CheckCircle className="h-6 w-6 text-orange-600" />
                                        </div>
                                        <div className="text-2xl font-bold text-gray-900">{stats.completedCreatedEvents}</div>
                                        <div className="text-sm text-gray-600">Завершенных событий</div>
                                    </div>
                                    <div className="text-center">
                                        <div className="w-12 h-12 bg-teal-100 rounded-lg flex items-center justify-center mx-auto mb-2">
                                            <TrendingUp className="h-6 w-6 text-teal-600" />
                                        </div>
                                        <div className="text-2xl font-bold text-gray-900">{stats.completionRate}%</div>
                                        <div className="text-sm text-gray-600">Завершено событий</div>
                                    </div>
                                </div>
                            )}
                        </div>

                        {/* Registered Events */}
                        <div className="bg-white rounded-lg border border-gray-300 p-6">
                            <h3 className="text-lg font-medium text-gray-900 mb-4">
                                Мои записи на мероприятия ({registeredEvents.length})
                            </h3>
                            {statsLoading ? (
                                <div className="text-center py-8">
                                    <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-gray-900 mx-auto"></div>
                                    <p className="mt-2 text-gray-600">Загрузка мероприятий...</p>
                                </div>
                            ) : registeredEvents.length === 0 ? (
                                <div className="text-center py-8 text-gray-500">
                                    <Calendar className="h-12 w-12 mx-auto mb-3 text-gray-400" />
                                    <p>Вы еще не записались ни на одно мероприятие</p>
                                </div>
                            ) : (
                                <div className="space-y-4">
                                    {registeredEvents.slice(0, 5).map((event) => {
                                        const status = getEventStatus(event);
                                        return (
                                            <div key={event.id} className="flex items-center justify-between py-3 border-b border-gray-200 last:border-b-0">
                                                <div className="flex items-center flex-1 min-w-0">
                                                    <div className="w-10 h-10 bg-gray-100 rounded-lg flex items-center justify-center mr-3 flex-shrink-0">
                                                        <Calendar className="h-5 w-5 text-gray-600" />
                                                    </div>
                                                    <div className="flex-1 min-w-0">
                                                        <div className="text-sm font-medium text-gray-900 truncate">
                                                            {event.title}
                                                        </div>
                                                        <div className="text-sm text-gray-500">
                                                            {formatDate(event.startDate)} • {event.location}
                                                        </div>
                                                    </div>
                                                </div>
                                                <span className={`inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium ${status.color} ml-3 flex-shrink-0`}>
                                                    {status.text}
                                                </span>
                                            </div>
                                        );
                                    })}
                                    {registeredEvents.length > 5 && (
                                        <div className="mt-4 text-center">
                                            <button className="text-sm text-gray-900 hover:text-gray-700 font-medium">
                                                Показать все {registeredEvents.length} записей
                                            </button>
                                        </div>
                                    )}
                                </div>
                            )}
                        </div>

                        {/* Created Events */}
                        <div className="bg-white rounded-lg border border-gray-300 p-6">
                            <h3 className="text-lg font-medium text-gray-900 mb-4">
                                Мои созданные мероприятия ({createdEvents.length})
                            </h3>
                            {statsLoading ? (
                                <div className="text-center py-8">
                                    <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-gray-900 mx-auto"></div>
                                    <p className="mt-2 text-gray-600">Загрузка мероприятий...</p>
                                </div>
                            ) : createdEvents.length === 0 ? (
                                <div className="text-center py-8 text-gray-500">
                                    <Calendar className="h-12 w-12 mx-auto mb-3 text-gray-400" />
                                    <p>Вы еще не создали ни одного мероприятия</p>
                                </div>
                            ) : (
                                <div className="space-y-4">
                                    {createdEvents.slice(0, 5).map((event) => {
                                        const status = getEventStatus(event);
                                        const participantsCount = event.participants?.length || 0;
                                        return (
                                            <div key={event.id} className="flex items-center justify-between py-3 border-b border-gray-200 last:border-b-0">
                                                <div className="flex items-center flex-1 min-w-0">
                                                    <div className="w-10 h-10 bg-gray-100 rounded-lg flex items-center justify-center mr-3 flex-shrink-0">
                                                        <Calendar className="h-5 w-5 text-gray-600" />
                                                    </div>
                                                    <div className="flex-1 min-w-0">
                                                        <div className="text-sm font-medium text-gray-900 truncate">
                                                            {event.title}
                                                        </div>
                                                        <div className="text-sm text-gray-500">
                                                            {formatDate(event.startDate)} • {participantsCount} участников
                                                        </div>
                                                    </div>
                                                </div>
                                                <div className="flex items-center gap-2 ml-3 flex-shrink-0">
                                                    <span className="text-xs text-gray-500">
                                                        {participantsCount}/{event.maxParticipants > 0 ? event.maxParticipants : '∞'}
                                                    </span>
                                                    <span className={`inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium ${status.color}`}>
                                                        {status.text}
                                                    </span>
                                                </div>
                                            </div>
                                        );
                                    })}
                                    {createdEvents.length > 5 && (
                                        <div className="mt-4 text-center">
                                            <button className="text-sm text-gray-900 hover:text-gray-700 font-medium">
                                                Показать все {createdEvents.length} мероприятий
                                            </button>
                                        </div>
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

export default ProfilePage;