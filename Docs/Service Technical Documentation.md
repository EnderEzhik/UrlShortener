# High Level Design

## Функциональные требования
- Создать короткую ссылку
- Редирект с короткой ссылки на оригинальную
- Регистрация
- Авторизация
- Просмотр списка созданных пользователем коротких ссылок
- Просмотр базовой информации о себе (логин, дата регистрации, количество активных коротких ссылок)
- Удалить короткую ссылку
- Получить аналитику по короткой ссылке
- Аналитика переходов
  - Общее количество переходов
  - Количество уникальных перешедших пользователей
  - Временное распределение переходов по дням и часам

## Нефункциональные требования
- Время ответа API не должно превышать 100мс для нетребовательных действий
    - Создание короткой ссылки
    - Редирект
    - Регистрация
    - Авторизация
    - Получение данных пользователя о себе
    - Получение короткой ссылки по короткому коду
    - Удаление короткой ссылки по короткому коду
- Время ответа API не должно превышать 200мс для запросов передающих больший объем данных
    - Получение списка ссылок пользователя (как с исключением коротких ссылок с истекшим сроком жизни, так и с ними)

## Ограничения
### Короткая ссылка
- Длина короткого кода - 8 символов
- Время истечения срока жизни короткой ссылки должно быть в будущем времени в момент её создания
- Длина оригинальной ссылки не должна превышать 1000 символов
- Если ссылка создана неавторизованным пользователем, то у неё нет владельца
- Просмотр аналитики доступен только владельцу ссылки, если владельца нет, то аналитика недоступна

### Пользователь
- Длина логина должна быть от 4 до 20 символов
- Длина пароля должна быть от 8 до 64 символов

## Сущности
- Короткая ссылка
  - Короткий код
  - Оригинальная ссылка
  - Владелец
  - Дата создания
  - Срок жизни
- Переход
  - Дата и время
  - IP
  - ID авторизованного пользователя при наличии
  - Исходный адрес
- Пользователь
  - Логин
  - Пароль
  - Дата регистрации

# Low Level Design
## Схема базы данных
```postgresql
CREATE TABLE ShortUrls(
    ShortCode VARCHAR(8) PRIMARY KEY,
    OriginalUrl TEXT NOT NULL,
    OwnerId INTEGER NOT NULL REFERENCES Users(Id),
    CreatedAt TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    ExpiresAt TIMESTAMP WITH TIME ZONE
);
CREATE INDEX idx_short_code ON ShortUrls(ShortCode);
CREATE TABLE Users(
    Id INTEGER PRIMARY KEY,
    Login VARCHAR(20) NOT NULL UNIQUE,
    Password VARCHAR NOT NULL,
    RegistrationAt TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
);
CREATE TABLE Clicks(
    Id INTEGER PRIMARY KEY,
    ShortCode VARCHAR(8) NOT NULL REFERENCES ShortUrls(ShortCode),
    "RedirectAt" TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    IpAddress VARCHAR,
    UserId INTEGER REFERENCES Users(Id),
    Referer TEXT
);
CREATE INDEX idx_short_code ON Clicks(ShortCode);
```

## Endpoints
### Создание короткой ссылки
POST /api/links

Request:
```json
{
    "url": "https://example.com",
    "expiresAt": "2026-01-00T00:00:00Z"
}
```

Response:
```json
{
    "shortCode": "1234abcd",
    "expiresAt": "2026-01-00T00:00:00Z"
}
```

Status Codes: 200, 400

### Редирект по короткому коду
GET /{shortCode}

Request: path param

Response: редирект 302

Status Codes: 302, 404

### Регистрация
POST /api/auth/register

Request:
```json
{
    "login": "test_login",
    "password": "secure_password_123"
}
```

Response:
```json
{
    "token": "jwt.token.test",
    "expiresAt": "2026-01-00T00:00:00Z"
}
```

Status Codes: 200, 400, 409

### Авторизация
POST /api/auth/login

Request:
```json
{
    "login": "test_login",
    "password": "secure_password_123"
}
```

Response:
```json
{
    "token": "jwt.token.test",
    "expiresAt": "2026-01-00T00:00:00Z"
}
```

Status Codes: 200, 401

### Получить список ссылок
GET /api/links?ExcludeExpiredUrls=true

Request: none

Response:
```json
[
    {
        "OriginalUrl": "https://example.com",
        "ShortCode": "1234abcd",
        "CreatedAt": "2026-01-00T00:00:00Z",
        "ExpiresAt": "2026-02-00T00:00:00Z"
    }
]
```

Status Codes: 200

### Получить короткую ссылку по короткому коду
GET /api/links/{shortCode}

Request: none

Response:
```json
{
    "OriginalUrl": "https://example.com",
    "ShortCode": "1234abcd",
    "CreatedAt": "2026-01-00T00:00:00Z",
    "ExpiresAt": "2026-02-00T00:00:00Z"
}
```

Status Codes: 200, 404

### Удалить короткую ссылку по короткому коду
DELETE /api/links/{shortCode}

Request: none

Response: none

Status Codes: 204, 404

### Получить информацию о себе
GET /api/users/me

Request: none

Response:
```json
{
    "Login": "username",
    "RegistrationAt": "2026-01-00T00:00:00Z"
}
```

Status Codes: 200