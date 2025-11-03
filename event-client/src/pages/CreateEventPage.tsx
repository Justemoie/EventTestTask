import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '../contexts/AuthContext';
import { EventRequest, EventCategory } from '../types';
import { apiService } from '../services/api';
import { ArrowLeft, Calendar, MapPin, Clock, Plus, X } from 'lucide-react';
import { fileToByteArray } from '../utils/FileToByteArray';

const CreateEventPage: React.FC = () => {
  const { isAuthenticated } = useAuth();
  const navigate = useNavigate();
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string>('');
  const [formData, setFormData] = useState<EventRequest & {
    startTime?: string;
    endTime?: string;
    imageFile?: File;
    imagePreview?: string;
  }>({
    title: '',
    description: '',
    startDate: '',
    endDate: '',
    location: '',
    category: EventCategory.Other,
    maxParticipants: 0,
    startTime: '',
    endTime: ''
  });

  const categoryOptions = [
    { value: EventCategory.Conference, label: 'Конференция' },
    { value: EventCategory.Workshop, label: 'Воркшоп' },
    { value: EventCategory.Webinar, label: 'Вебинар' },
    { value: EventCategory.Meetup, label: 'Митап' },
    { value: EventCategory.Party, label: 'Вечеринка' },
    { value: EventCategory.Sport, label: 'Спорт' },
    { value: EventCategory.Other, label: 'Другое' }
  ];

  const handleChange = (e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement | HTMLSelectElement>) => {
    const { name, value } = e.target;
    setFormData(prev => ({
      ...prev,
      [name]: name === 'maxParticipants' || name === 'category'
        ? parseInt(value) || 0
        : value
    }));
  };

  const handleFileChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    if (e.target.files?.[0]) {
      const file = e.target.files[0];
      setFormData(prev => ({
        ...prev,
        imageFile: file,
        imagePreview: URL.createObjectURL(file)
      }));
    }
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');

    if (!isAuthenticated) {
        setError('Необходима авторизация');
        return;
    }

    const startDateTime = `${formData.startDate}T${formData.startTime}:00`;
    const endDateTime = `${formData.endDate}T${formData.endTime}:00`;

    setLoading(true);

    try {
        const eventData: EventRequest = {
            title: formData.title,
            description: formData.description,
            startDate: startDateTime,
            endDate: endDateTime,
            location: formData.location,
            category: formData.category,
            maxParticipants: formData.maxParticipants
        };

        await apiService.createEvent(eventData);
        navigate('/my-events');
    } catch (err: any) {
        console.error('Error creating event:', err);
        setError(err.response?.data?.message || 'Ошибка создания события');
    } finally {
        setLoading(false);
    }
  };

  if (!isAuthenticated) {
    return (
      <div className="min-h-screen flex items-center justify-center bg-gray-50">
        <div className="text-center">
          <h2 className="text-2xl font-bold mb-4 text-gray-900">Необходима авторизация</h2>
          <p className="text-gray-600">Войдите в систему, чтобы создавать события</p>
        </div>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-gray-50 py-8">
      <div className="max-w-4xl mx-auto px-4 sm:px-6 lg:px-8">
        <button onClick={() => navigate(-1)} className="flex items-center mb-6 text-gray-600 hover:text-gray-900">
          <ArrowLeft className="h-5 w-5 mr-2" /> Назад
        </button>

        <h1 className="text-3xl font-bold mb-2 text-gray-900">Создать новое мероприятие</h1>
        <p className="text-gray-600 mb-6">Заполните форму ниже, чтобы создать и опубликовать мероприятие</p>

        <form onSubmit={handleSubmit} className="bg-white rounded-lg border border-gray-300 p-6 space-y-6">

          {/* Title */}
          <div>
            <label className="block text-sm font-medium mb-2 text-gray-700">Название мероприятия *</label>
            <input
              type="text"
              name="title"
              required
              value={formData.title}
              onChange={handleChange}
              className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-gray-500 focus:border-gray-500"
            />
          </div>

          {/* Date & Time */}
          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            <div>
              <label className="block mb-2 text-gray-700">Дата начала *</label>
              <input type="date" name="startDate" value={formData.startDate} onChange={handleChange} className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-gray-500 focus:border-gray-500" />
            </div>
            <div>
              <label className="block mb-2 text-gray-700">Время начала *</label>
              <input type="time" name="startTime" value={formData.startTime} onChange={handleChange} className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-gray-500 focus:border-gray-500" />
            </div>
            <div>
              <label className="block mb-2 text-gray-700">Дата окончания *</label>
              <input type="date" name="endDate" value={formData.endDate} onChange={handleChange} className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-gray-500 focus:border-gray-500" />
            </div>
            <div>
              <label className="block mb-2 text-gray-700">Время окончания *</label>
              <input type="time" name="endTime" value={formData.endTime} onChange={handleChange} className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-gray-500 focus:border-gray-500" />
            </div>
          </div>

          {/* Location */}
          <div>
            <label className="block mb-2 text-gray-700">Место проведения *</label>
            <input type="text" name="location" value={formData.location} onChange={handleChange} className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-gray-500 focus:border-gray-500" />
          </div>

          {/* Description */}
          <div>
            <label className="block mb-2 text-gray-700">Описание мероприятия *</label>
            <textarea name="description" value={formData.description} onChange={handleChange} rows={4} className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-gray-500 focus:border-gray-500" />
          </div>

          {/* Max Participants & Category */}
          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            <div>
              <label className="block mb-2 text-gray-700">Максимальное количество участников</label>
              <select name="maxParticipants" value={formData.maxParticipants} onChange={handleChange} className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-gray-500 focus:border-gray-500">
                <option value={0}>Без ограничений</option>
                <option value={10}>10</option>
                <option value={20}>20</option>
                <option value={50}>50</option>
                <option value={100}>100</option>
                <option value={200}>200</option>
              </select>
            </div>
            <div>
              <label className="block mb-2 text-gray-700">Категория мероприятия</label>
              <select name="category" value={formData.category} onChange={handleChange} className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-gray-500 focus:border-gray-500">
                {categoryOptions.map(opt => <option key={opt.value} value={opt.value}>{opt.label}</option>)}
              </select>
            </div>
          </div>

          {/* Event Image */}
          <div>
            <label className="block mb-2 text-gray-700">Изображение мероприятия</label>
            {formData.imagePreview && <img src={formData.imagePreview} alt="Preview" className="w-64 h-32 object-cover mb-2 rounded border border-gray-300" />}
            <input type="file" accept="image/*" onChange={handleFileChange} className="w-full" />
          </div>

          {error && <div className="bg-gray-50 border border-gray-300 text-gray-700 px-4 py-3 rounded-md">{error}</div>}

          {/* Buttons */}
          <div className="flex justify-end space-x-4 pt-6 border-t border-gray-300">
            <button type="button" onClick={() => navigate(-1)} className="px-4 py-2 border border-gray-400 rounded-md text-gray-700 bg-white hover:bg-gray-50">Отмена</button>
            <button type="submit" disabled={loading} className="px-4 py-2 rounded-md text-white bg-gray-900 hover:bg-gray-800 disabled:opacity-50">{loading ? 'Создание...' : 'Создать'}</button>
          </div>

        </form>
      </div>
    </div>
  );
};

export default CreateEventPage;