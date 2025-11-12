# Software Requirements Specification (SRS) for EventTestTask

Table of Contents  
1 [Введение](#introduction)  
1.1 [Назначение](#purpose)  
1.2 [Бизнес-требования](#business-requirements)  
1.2.1 [Контекст](#background)  
1.2.2 [Возможности](#business-opportunities)  
1.2.3 [Границы проекта](#project-scope)  
1.3 [Аналоги](#analogs)  
2 [Пользовательские требования](#user-requirements)  
2.1 [Программные интерфейсы](#software-interfaces)  
2.2 [Пользовательский интерфейс](#user-interface)  
2.3 [Характеристики пользователей](#user-characteristics)  
2.3.1 [Классы пользователей](#user-classes)  
2.3.2 [Аудитория приложения](#application-audience)  
2.4 [Предположения и зависимости](#assumptions-and-dependencies)  
3 [Системные требования](#system-requirements)  
3.1 [Функциональные требования](#functional-requirements)  
3.1.1 [Базовые функции](#core-functions)  
3.1.1.1 [Регистрация пользователя](#user-registration)  
3.1.1.2 [Аутентификация и выход](#user-login-logout)  
3.1.1.3 [CRUD событий и поиск](#events-crud-search)  
3.1.1.4 [Изображение события](#event-image)  
3.1.1.5 [Регистрация на событие](#event-registration)  
3.1.1.6 [Профиль пользователя](#user-profile)  
3.1.2 [Ограничения и исключения](#constraints-and-exclusions)  
3.2 [Нефункциональные требования](#non-functional-requirements)  
3.2.1 [Атрибуты качества](#quality-attributes)  
3.2.1.1 [Юзабилити](#usability-requirements)  
3.2.1.2 [Безопасность](#security-requirements)  
3.2.1.3 [Производительность](#performance-requirements)  
3.2.2 [Внешние интерфейсы](#external-interfaces)  
3.2.3 [Ограничения](#constraints)  
3.2.4 [Требования к архитектуре](#architectural-requirements)

<a name="introduction"/>

# 1 Введение

<a name="purpose"/>

## 1.1 Назначение
Документ описывает функциональные и нефункциональные требования к веб‑приложению “Events System” (EventTestTask) — системе управления событиями с регистрацией участников. Документ ориентирован на команду разработки и проверяющих.

<a name="business-requirements"/>

## 1.2 Бизнес-требования

<a name="background"/>

### 1.2.1 Контекст
Людям и организациям требуется простой способ создавать публичные или приватные события, управлять списком участников и получать актуальную информацию. Существующие платформы нередко избыточны или требуют сложной интеграции. Нужен лёгкий в развертывании и использовании сервис.

<a name="business-opportunities"/>

### 1.2.2 Возможности
EventTestTask предоставляет API для:
- Создания и поиска событий с фильтрами и пагинацией.
- Регистрации/отмены регистрации участников.
- Управления профилем пользователя.
- Хранения изображений события.
- Безопасной аутентификации на основе JWT.

<a name="project-scope"/>

### 1.2.3 Границы проекта
MVP включает бэкенд API (ASP.NET Core) и клиент (React) для базового управления событиями и регистрациями. Монетизация, биллинг, уведомления и интеграции c внешними календарями вне рамок MVP.

<a name="analogs"/>

## 1.3 Аналоги
- **Eventbrite / Meetup:** Полнофункциональные платформы. Отличие — наш сервис проще, фокус на MVP и локальном развертывании.
- **Google Calendar (Events):** Календарь с событиями, но без моделей участников и доменной логики регистрации.
- **Внутренние ERP/CRM-модули событий:** Сложнее по внедрению и интеграциям.

<a name="user-requirements"/>

# 2 Пользовательские требования

<a name="software-interfaces"/>

## 2.1 Программные интерфейсы
- **Бэкенд:** ASP.NET Core Web API (.NET 9), C# 13, Swagger.
- **База данных:** PostgreSQL, EF Core (Fluent API, миграции в проде).
- **Аутентификация:** JWT (cookies `_at`, `_rt`), ASP.NET Authorization (роли: `User`, `Admin`).
- **Клиент:** React (локально `http://localhost:3000/3001`), CORS политика `AllowReactApp`.
- **Контейнеризация:** Docker, Docker Compose.
- **Тесты:** xUnit, AutoFixture, FluentAssertions, Moq.
- **Прочее:** AutoMapper, FluentValidation, MemoryCache.

<a name="user-interface"/>

## 2.2 Пользовательский интерфейс
- Клиент на React (репозиторий `event-client`).  
- Swagger UI для тестирования API: `http://localhost:8080/swagger/index.html`.  
- Макеты интерфейсов в репозитории отсутствуют; фронтенд предполагает:
  - Список событий с пагинацией и фильтрами.
  - Просмотр карточки события.
  - Регистрация/отмена регистрации.
  - Профиль и аутентификация.

<a name="user-characteristics"/>

## 2.3 Характеристики пользователей

<a name="user-classes"/>

### 2.3.1 Классы пользователей
- **Гость:** Доступ к публичным GET (часть эндпоинтов без [Authorize]).
- **Авторизованный пользователь (User):** Полный доступ к пользовательским операциям, создание/редактирование своих событий, регистрация.
- **Администратор (Admin):** Расширенные права (например, обновление событий не только своих).

<a name="application-audience"/>

### 2.3.2 Аудитория приложения
- **Основная:** Организаторы небольших мероприятий, учебные проекты, внутренние отделы.
- **Вторичная:** Разработчики, изучающие стек ASP.NET Core + EF Core + PostgreSQL + React.

<a name="assumptions-and-dependencies"/>

## 2.4 Предположения и зависимости
1. Пользователь предоставляет валидные данные и соблюдает правила сервиса.
2. Подключение к БД PostgreSQL доступно из API.
3. Клиентский домен включён в CORS (http://localhost:3000, 3001).
4. JWT секреты и строки подключения заданы через переменные окружения/конфигурацию.
5. Docker и Docker Compose доступны для локального запуска.

<a name="system-requirements"/>

# 3 Системные требования

<a name="functional-requirements"/>

## 3.1 Функциональные требования

<a name="core-functions"/>

### 3.1.1 Базовые функции

<a name="user-registration"/>

#### 3.1.1.1 Регистрация пользователя
- **Описание.** Создание учётной записи.
- **Ввод данных.** `firstName`, `lastName`, `birthDate`, `email`, `password`.
- **API.** `POST /api/auth/register`.
- **Валидация.** FluentValidation. Email — уникальный. Пароль хэшируется (кастомный `IPasswordHasher`).
- **Результат.** Создание записи `User` в PostgreSQL, роль по умолчанию `User`.

<a name="user-login-logout"/>

#### 3.1.1.2 Аутентификация и выход
- **Описание.** Вход, отпуск токенов, выход.
- **API.**
  - `POST /api/auth/login` → `TokenResponse` (добавляет cookies `_at` и `_rt`).
  - `POST /api/auth/logout` [Authorize] → инвалидация refresh-токена, удаление cookies.
  - `POST /api/auth/update` [Authorize] → обновление токенов.
- **Безопасность.** JWT хранится в HttpOnly/Secure cookies. Авторизация по ролям.

<a name="events-crud-search"/>

#### 3.1.1.3 CRUD событий и поиск
- **Сущность Event.** `Id`, `CreatorId`, `Title`, `Description`, `StartDate`, `EndDate`, `Location`, `Category`, `MaxParticipants`, `Image (byte[])`.
- **API.**
  - `GET /api/events` → `PageResult<EventResponse>` (пагинация).
  - `GET /api/events/{eventId}` → `EventResponse`.
  - `GET /api/events/by-title?title=...` → `EventResponse`.
  - `POST /api/events/create` [Authorize(Roles="User")] → создание события (CreatorId берётся из JWT `NameIdentifier`).
  - `PUT /api/events/update/{eventId}` [Authorize] → обновление события:
    - Разрешено автору события или `Admin`.
    - Проверка через разбор JWT из cookie `_at` на сервере.
  - `DELETE /api/events/delete/{eventId}` [Authorize] → удаление.
  - `GET /api/events/search` → поиск по фильтрам `EventFilter` с пагинацией.
  - `GET /api/events/created-by/{userId}` [Authorize] → события, созданные пользователем.
  - `GET /api/events/registered-by/{userId}` [Authorize] → события, где пользователь зарегистрирован.
- **Валидация.** FluentValidation для `Event`.
- **Кэш.** MemoryCache для изображений (инвалидация при обновлении).

<a name="event-image"/>

#### 3.1.1.4 Изображение события
- **Описание.** Хранение обложки события в БД (byte[]).
- **API.**
  - `GET /api/events/image/{eventId}` → `byte[]`.
  - `POST /api/events/image-update/{eventId}` [Authorize] + `multipart/form-data` → загрузка изображения.
- **Требования.** Проверка, что файл присутствует и не пустой. Инвалидация кеша по ключу `image_{eventId}`.

<a name="event-registration"/>

#### 3.1.1.5 Регистрация на событие
- **Описание.** Запись/отмена записи пользователя на событие.
- **API.**
  - `POST /api/registrations/{eventId}/users/{userId}/register` [Authorize].
  - `POST /api/registrations/{eventId}/users/{userId}/unregister` [Authorize].
  - `GET /api/registrations/{eventId}` [Authorize] → `PageResult<User>`.
  - `GET /api/registrations/{eventId}/users/{userId}` [Authorize] → `User`.
- **Правила.** Проверка существования события и пользователя. Дата регистрации — `UtcNow`. Ограничение по `MaxParticipants` — расширение для будущих итераций.

<a name="user-profile"/>

#### 3.1.1.6 Профиль пользователя
- **API.**
  - `GET /api/users/{userId}` [Authorize] → профиль.
  - `GET /api/users/email?email=...` [Authorize].
  - `PUT /api/users/update/{userId}` [Authorize] → обновление профиля (валидируется).
- **Требования.** Даты нормализуются в UTC. Email уникальный.

<a name="constraints-and-exclusions"/>

### 3.1.2 Ограничения и исключения
1. Нет платёжных операций, билетов, QR‑кодов.
2. Нет уведомлений (email/SMS/push).
3. Нет редактирования изображений/метаданных сверх описанного.
4. Нет ролей/прав тоньше `User`/`Admin` в MVP.
5. Нет импорта/экспорта событий в календари (ics).

<a name="non-functional-requirements"/>

## 3.2 Нефункциональные требования

<a name="quality-attributes"/>

### 3.2.1 Атрибуты качества

<a name="usability-requirements"/>

#### 3.2.1.1 Юзабилити
- **Простота API.** Предсказуемые REST маршруты, описаны в Swagger.
- **Пагинация и фильтры.** Удобны для фронтенда (модель `PageParams`).
- **CORS.** Разрешены `http://localhost:3000` и `3001`.

<a name="security-requirements"/>

#### 3.2.1.2 Безопасность
- **JWT.** HttpOnly/Secure cookies, проверка ролей.
- **Авторизация.** [Authorize] на изменяющих операциях; проверка автора события или роли Admin при обновлении.
- **Хранение секретов.** Через переменные окружения/конфиги.
- **Валидация.** FluentValidation; глобальный обработчик исключений `GlobalExceptionHandlerMiddleware`.

<a name="performance-requirements"/>

#### 3.2.1.3 Производительность
- **API задержка.** Большинство запросов <100 мс при локальном развёртывании.
- **БД.** Использование EF Core, индексы на ключевых полях (Id, Email, CreatorId, EventId).
- **Кэш.** MemoryCache для изображений событий.
- **Пагинация.** Все списки — с серверной пагинацией.

<a name="external-interfaces"/>

### 3.2.2 Внешние интерфейсы
- **Браузеры.** Современные версии Chrome, Firefox, Safari, Edge.
- **Swagger UI.** Документация и тестирование.
- **Docker Compose.** Быстрый старт окружения: `docker-compose -f ./EventTestTask.Api/docker-compose.yml up -d`.

<a name="constraints"/>

### 3.2.3 Ограничения
- Бэкенд — ASP.NET Core Web API (.NET 9).
- БД — PostgreSQL.
- Клиент — React (локально 3000/3001).
- JWT‑аутентификация.
- EF Core с Fluent API.
- Ограничения CORS по `AllowReactApp`.

<a name="architectural-requirements"/>

### 3.2.4 Требования к архитектуре
- **Слои.** Проект разделён на слои:
  - Api (Controllers, Middlewares, Extensions).
  - Application (Services, бизнес-логика, кэш, валидация).
  - Core (Entities, Enums, Interfaces, Models).
  - Infrastructure (EF Core Context, Repositories).
- **Шаблон.** Контроллеры → Сервисы → Репозитории → БД.
- **Модели.** DTO ↔ Entities через AutoMapper.
- **Авторизация.** JWT с ролями. Доступ к Claims из cookies в сервисах при необходимости (например, в `EventsService.UpdateEvent`).
- **Миграции.** Авто‑миграция в производственной среде (Program.cs).
- **Обработка ошибок.** Глобальный middleware, единообразные ответы об ошибках.

# Recommended Actions
- **[уточнить]** Нужны ли дополнительные фильтры поиска (по диапазону дат, категориям, гео)?
- **[уточнить]** Нужны ли лимиты на регистрации (учёт `MaxParticipants`) уже в MVP?
- **[уточнить]** Нужна ли модерация/админ‑панель и расширенные роли?
- **[уточнить]** Нужны ли клиентские макеты/скрины для раздела UI в SRS?

# Итог
Подготовлен SRS, основанный на вашем коде (ASP.NET Core Web API, PostgreSQL, EF Core, JWT, React‑клиент, Docker). Описаны ключевые контроллеры, сервисы и сценарии: регистрация/логин, CRUD событий, загрузка изображений, поиск, регистрация участников, профиль пользователя, права доступа и требования к архитектуре. Готов дополнить по вашим уточнениям.
