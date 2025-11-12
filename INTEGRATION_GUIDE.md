# Интеграция с реальной базой данных

## Запуск полного стека

### 1. Запуск API сервера с базой данных

```bash
# Перейдите в корневую папку проекта
cd ../EventTestTask

# Запустите Docker Compose
docker-compose -f docker-compose.yml up -d --build
```

Это запустит:
- **PostgreSQL** базу данных на порту 5433
- **ASP.NET Core API** на порту 5001 (HTTP, маппинг с 8080)

### 2. Проверка работы API

После запуска API будет доступен по адресам:
- HTTP: `http://localhost:5001`
- Swagger UI: `http://localhost:5001/swagger`

### 3. Запуск React клиента

```bash
# Перейдите в папку клиента
cd event-client

# Запустите React приложение
npm start
```

React приложение будет доступно по адресу: `http://localhost:3000`

## Интеграция с реальными данными

### Что изменилось:

1. **EventsPage** - теперь загружает данные из API `/api/events/search`
2. **MyEventsPage** - интегрирован с API для получения событий пользователя
3. **CreateEventPage** - создает события через API `/api/events/create`
4. **API Service** - настроен для работы с HTTPS API

### Обработка ошибок:

При недоступности API приложение показывает соответствующие сообщения об ошибках и пустые списки.

## Тестирование интеграции

### 1. Проверьте подключение к API:
- Откройте браузер и перейдите на `http://localhost:5001/swagger`
- Убедитесь, что API отвечает

### 2. Проверьте работу клиента:
- Откройте `http://localhost:3000`
- Попробуйте загрузить список событий
- Если API недоступен, вы увидите уведомление об ошибке

### 3. Тестирование функций:
- **Регистрация/Вход** - создайте тестового пользователя
- **Просмотр событий** - проверьте загрузку из базы данных
- **Создание события** - создайте новое событие через форму
- **Поиск и фильтрация** - протестируйте функциональность поиска

## Возможные проблемы и решения

### 1. CORS ошибки
CORS настроен в `Program.cs` для работы с `http://localhost:3000`:
```csharp
app.UseCors(policy => policy
    .WithOrigins("http://localhost:3000", "https://localhost:3000")
    .AllowAnyMethod()
    .AllowAnyHeader()
    .AllowCredentials());
```

### 2. Ошибка SSL сертификата
Если возникают проблемы с HTTPS, измените в `src/services/api.ts`:
```typescript
baseURL: 'http://localhost:5001/api', // Используйте HTTP вместо HTTPS
```

## Структура данных

### Event (Событие)
- `id` - уникальный идентификатор
- `title` - название события
- `description` - описание
- `startDate` - дата начала
- `endDate` - дата окончания
- `location` - место проведения
- `category` - категория события
- `maxParticipants` - максимальное количество участников
- `participants` - список участников

### Registration (Регистрация)
- `id` - уникальный идентификатор
- `eventId` - ID события
- `userId` - ID пользователя
- `registrationDate` - дата регистрации

## API Endpoints

- `GET /api/events` - список всех событий
- `GET /api/events/search` - поиск событий с фильтрами
- `POST /api/events/create` - создание события
- `PUT /api/events/update/{id}` - обновление события
- `DELETE /api/events/delete/{id}` - удаление события
- `GET /api/registrations/{eventId}` - участники события
- `POST /api/registrations/{eventId}/users/{userId}/register` - регистрация на событие
