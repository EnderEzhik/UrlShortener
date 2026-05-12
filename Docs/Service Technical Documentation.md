# High Level Design

## Функциональные требования
- Создать короткую ссылку
- Редирект с короткой ссылки на оригинальную
- Сохранение аналитики по каждому редиректу
- Регистрация
- Авторизация
- Просмотр списка созданных пользователем коротких ссылок
- Просмотр базовой информации о себе (логин, дата регистрации, количество активных коротких ссылок)
- Удалить короткую ссылку
- Получить аналитику по короткой ссылке

---

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
- Сохранение аналитики происходит асинхронно от редиректа
---

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

---

## Аналитика переходов
- Общее количество переходов
- Количество уникальных перешедших пользователей
- Количество переходов для каждого уникального пользователя
- Временное распределение переходов по дате и времени
- Список платформ (windows, macos, android, ios...)
- Список типов устройств (пк, телефон, планшет...)
- Список марок устройств (apple, samsung...)
- Список моделей устройств ((apple) iphone 15, (samsung) s21...)
- Список браузеров и их версий (chrome, yandex...)
- Список поддерживаемых языков пользователей (Accept-Language)
- Откуда был произведен переход (referer)
- Геораспределение переходов
- Авторизованные переходы (авторизованный пользователь перешел по короткой ссылке)
- Количество переходов по неактивной короткой ссылке
- Длительность использования короткой ссылки после истечения срока её жизни
- UTM-метки

---

## Сущности
### Короткая ссылка
- Короткий код
- Оригинальная ссылка
- Владелец
- Дата создания
- Срок жизни
- Общее количество переходов
- Alias
- Количество доступных переходов
- Пароль для перехода
- Доступ только для авторизованных пользователей
- UTM-метки

### Переход
- Короткий код
- Дата и время
- IP
- Тип платформы
- Тип устройства
- Марка устройства
- Модель устройства
- Браузер
- Версия браузера
- Поддерживаемый язык
- Исходный адрес
- Страна перехода
- Регион перехода
- Город перехода
- ID авторизованного пользователя
- UserAgent
- UTM-метки

### Пользователь
- Логин
- Пароль
- Дата регистрации

---

# Low Level Design
## Схема базы данных
### ER-диаграмма
https://dbdiagram.io
```
Table ShortUrls {
  ShortCode varchar [pk]
  OriginalUrl text [not null]
  OwnerId integer
  CreatedAt timestamp [default: `now()`]
  ExpiresAt timestamp
  TotalRedirects integer [default: 0]
  Alias varchar
  AvailableRedirects integer
  Password varchar
  OnlyAuthorized bool [default: false]
  UtmTags text
}

Table Users {
  Id integer [pk, increment]
  Login varchar(20) [not null, unique]
  Password varchar(64) [not null]
  RegisteredAt timestamp [default: `now()`]
}

Table Redirects {
  Id integer [pk, increment]
  ShortCode varchar [not null]
  "Timestamp" timestamp [not null, default: `now()`]
  IpAddress varchar
  PlatformType varchar
  DeviceType varchar
  DeviceBrand varchar
  DeviceModel varchar
  Browser varchar
  BrowserVersion varchar
  SupportedLanguage varchar
  RefererUrl text
  Country varchar
  Region varchar
  City varchar
  AuthorizedUserId integer
  UserAgent text
  UtmTags text
}

Ref: "Users"."Id" < "ShortUrls"."OwnerId"

Ref: "ShortUrls"."ShortCode" < "Redirects"."ShortCode"

Ref: "Users"."Id" < "Redirects"."AuthorizedUserId"
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