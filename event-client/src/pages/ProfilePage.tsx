import React, { useState, useEffect } from 'react';
import { useAuth } from '../contexts/AuthContext';
import { User, UpdateUserRequest } from '../types';
import { Calendar, User as UserIcon, LogOut, Edit, Music, Code, ChefHat } from 'lucide-react';

const ProfilePage: React.FC = () => {
    const { user, logout, updateUser, isAuthenticated } = useAuth();
    const [isEditing, setIsEditing] = useState(false);
    const [editData, setEditData] = useState<Partial<User>>({});
    const [loading, setLoading] = useState(false);
    const [error, setError] = useState<string>('');

    useEffect(() => {
        if (user) {
            setEditData({
                firstName: user.firstName,
                lastName: user.lastName,
                email: user.email,
                birthDate: user.birthDate
            });
        }
    }, [user]);

    const handleEdit = () => {
        setIsEditing(true);
    };

    const handleSave = async () => {
        if (!user) return;
      
        setLoading(true);
        setError('');
      
        try {
          // Преобразуем Partial в Required, подставляя значения по умолчанию
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

    // Mock data for demonstration
    const mockStats = {
        createdEvents: 12,
        attendedEvents: 28,
        totalParticipants: 156,
        averageRating: 4.8,
        successRate: 89
    };

    const mockRecentEvents = [
        {
            id: '1',
            title: 'Джазовый вечер',
            type: 'created',
            date: '18 января 2025',
            status: 'Завершено',
            icon: Music
        },
        {
            id: '2',
            title: 'ІТ-конференция 2025',
            type: 'attended',
            date: '15 января 2025',
            status: 'Активно',
            icon: Code
        },
        {
            id: '3',
            title: 'Кулинарный мастер-класс',
            type: 'created',
            date: '12 января 2025',
            status: 'Запланировано',
            icon: ChefHat
        }
    ];

    return (
        <div className="min-h-screen bg-gray-50">
            <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
                {/* Header */}
                <div className="mb-8">
                    <h1 className="text-3xl font-bold text-gray-900 mb-2">Профиль пользователя</h1>
                    <p className="text-gray-600">Управляйте своим аккаунтом и просматривайте активность</p>
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
                                    <span className="text-sm text-gray-600">Дата регистрации:</span>
                                    <span className="text-sm font-medium text-gray-900">
                                        {new Date(user.birthDate).toLocaleDateString('ru-RU', {
                                            day: 'numeric',
                                            month: 'long',
                                            year: 'numeric'
                                        })}
                                    </span>
                                </div>
                                <div className="flex justify-between">
                                    <span className="text-sm text-gray-600">Статус:</span>
                                    <span className="text-sm font-medium text-gray-900">Активный</span>
                                </div>
                                <div className="flex justify-between">
                                    <span className="text-sm text-gray-600">Уровень:</span>
                                    <span className="text-sm font-medium text-gray-900">Организатор</span>
                                </div>
                            </div>
                        </div>
                    </div>

                    {/* Right Column - Activity and Events */}
                    <div className="lg:col-span-2 space-y-6">
                        {/* My Activity */}
                        <div className="bg-white rounded-lg border border-gray-300 p-6">
                            <h3 className="text-lg font-medium text-gray-900 mb-4">Моя активность</h3>
                            <div className="grid grid-cols-2 lg:grid-cols-5 gap-4">
                                <div className="text-center">
                                    <div className="w-12 h-12 bg-gray-100 rounded-lg flex items-center justify-center mx-auto mb-2">
                                        <Calendar className="h-6 w-6 text-gray-600" />
                                    </div>
                                    <div className="text-2xl font-bold text-gray-900">{mockStats.createdEvents}</div>
                                    <div className="text-sm text-gray-600">Созданных событий</div>
                                </div>
                                <div className="text-center">
                                    <div className="w-12 h-12 bg-gray-100 rounded-lg flex items-center justify-center mx-auto mb-2">
                                        <Calendar className="h-6 w-6 text-gray-600" />
                                    </div>
                                    <div className="text-2xl font-bold text-gray-900">{mockStats.attendedEvents}</div>
                                    <div className="text-sm text-gray-600">Посещенных событий</div>
                                </div>
                                <div className="text-center">
                                    <div className="text-2xl font-bold text-gray-900">{mockStats.totalParticipants}</div>
                                    <div className="text-sm text-gray-600">Всего участников</div>
                                </div>
                                <div className="text-center">
                                    <div className="text-2xl font-bold text-gray-900">{mockStats.averageRating}</div>
                                    <div className="text-sm text-gray-600">Средний рейтинг</div>
                                </div>
                                <div className="text-center">
                                    <div className="text-2xl font-bold text-gray-900">{mockStats.successRate}%</div>
                                    <div className="text-sm text-gray-600">Успешность</div>
                                </div>
                            </div>
                        </div>

                        {/* Recent Events */}
                        <div className="bg-white rounded-lg border border-gray-300 p-6">
                            <h3 className="text-lg font-medium text-gray-900 mb-4">Недавние события</h3>
                            <div className="space-y-4">
                                {mockRecentEvents.map((event) => {
                                    const IconComponent = event.icon;
                                    return (
                                        <div key={event.id} className="flex items-center justify-between py-3 border-b border-gray-300 last:border-b-0">
                                            <div className="flex items-center">
                                                <div className="w-8 h-8 bg-gray-100 rounded-lg flex items-center justify-center mr-3">
                                                    <IconComponent className="h-4 w-4 text-gray-600" />
                                                </div>
                                                <div>
                                                    <div className="text-sm font-medium text-gray-900">{event.title}</div>
                                                    <div className="text-sm text-gray-500">
                                                        {event.type === 'created' ? 'Создано' : 'Участвовал'} • {event.date}
                                                    </div>
                                                </div>
                                            </div>
                                            <span className={`inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium bg-gray-100 text-gray-800 border border-gray-300`}>
                                                {event.status}
                                            </span>
                                        </div>
                                    );
                                })}
                            </div>
                            <div className="mt-4">
                                <button className="text-sm text-gray-900 hover:text-gray-700 font-medium">
                                    Показать все события
                                </button>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    );
};

export default ProfilePage;