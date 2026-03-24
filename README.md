# UrlShortener

Небольшой сервис для сокращения URL-адресов, разработанный для практики и изучения современных возможностей ASP.NET Core.

## 📋 О проекте

Это пет-проект, целью которого является поэтапное освоение и применение различных технологий и практик из экосистемы .NET.
Развитие проекта разделено на этапы, начиная с базового функционала и постепенно усложняя его.

### Технологический стек:
- **Backend:** ASP.NET Core Web API
- **Frontend:** ASP.NET Core (статичные файлы), HTML5, CSS3, JavaScript (ES6+)
- **Язык:** C# (.NET 9.0)
- **База данных:** PostgreSQL
- **Кэширование:** Redis
- **ORM:** Entity Framework Core
- **Логирование:** Serilog
- **Архитектура:** Clean Architecture с разделением на слои, отдельное frontend приложение
- **Контейнеризация:** Docker + Docker Compose

## 📁 Структура проекта

```
UrlShortener/                 # Solution
├── Shortener/                # Main API project
│   ├── Common/               # Независимая от проекта бизнес логика
│   │   └── Utils             # Вспомогательные методы
│   ├── Controllers/          # Контроллеры API
│   ├── Data/                 # Контекст БД
│   ├── Entities/             # Сущности EF Core
│   ├── Extensions/           # Методы расширения
│   ├── Migrations/           # Миграции ef core
│   ├── Models/               # Модели данных
│   │   └── DTOs/             # DTO-классы для запросов/ответов API
│   ├── Options/              # Bind-классы для конфигураций
│   ├── Services/             # Сервисы (бизнес-логика)
│   └── Dockerfile            # Docker файл API приложения
├── Shortener.WebUI/          # Frontend приложение
│   ├── wwwroot/              # Статичные файлы (HTML, CSS, JS)
│   │   ├── css               # Css стили
│   │   └── js                # Javascript
│   └── Dockerfile            # Docker файл frontend приложения
├── docker-compose.yaml       # Docker Compose конфигурация
└── .env                      # Переменные окружения для Docker Compose
```

## 🛠 Установка и запуск

1. **Клонируйте репозиторий:**
    ```bash
    git clone https://github.com/EnderEzhik/UrlShortener.git
    cd ./UrlShortener
    ```

2. **Заполните .env файл по примеру из example.env**

3. **Создайте образ PostgreSQL:**
   ```bash
   docker-compose up -d database
   ```

3. **Примените миграции к базе данных:**
   ```bash
   cd ./Shortener
   dotnet ef database update  --connection Host=localhost;Port=6666;Database=url_shortener;Username=postgres;Password=postgres
   ```

4. **Запустите приложение с PostgreSQL и Redis:**
   ```bash
   docker-compose up -d --build
   ```

5. **Приложения будут доступны по адресам:**
    - **API:** http://localhost:5001
    - **Frontend (Web UI):** http://localhost:5000

## 🗺 Дорожная карта (Roadmap)

- [X] **Stage 1:** Базовый функционал (сокращение, редирект)
- [X] **Stage 2:** Постоянное хранилище (PostgreSQL), Docker
- [X] **Stage 3:** Логирование основных операций (Serilog)
- [X] **Stage 4:** Кэширование (Redis) для производительности
- [X] **Stage 5:** Пользовательский интерфейс (Frontend)
- [X] **Stage 6:** Аутентификация и личный кабинет
- [ ] **Stage 7:** Продвинутые функции (кастомные alias, аналитика, TTL)

---

## 🤝 Вклад в проект

Так как это учебный проект, любые предложения и советы по улучшению кода или архитектуры приветствуются! Не стесняйтесь создавать Issues или Pull Requests.