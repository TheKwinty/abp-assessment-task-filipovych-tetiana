**[Українська](README.md)** | [English](README.en.md)

# Conference Rooms API

## Огляд

Conference Rooms API — це ASP.NET Core Web API для управління конференц-залами, пошуку доступності, бронювання, розрахунку вартості оренди та аналітики бронювань. Рішення реалізує правила тестового завдання, зберігаючи роботу з даними, HTTP-рівень і бізнес-логіку в окремих сфокусованих проєктах.

## Реалізовані можливості

- CRUD для залів із місткістю, погодинною ставкою та доступними послугами.
- Пошук доступних залів за майбутнім часовим інтервалом і необхідною місткістю.
- Підтвердження бронювання з незмінними знімками даних залу, вартості послуг і загальної ціни.
- Початок бронювання о `:00` або `:30`, тривалість у цілих годинах, перевірка робочого часу та запобігання перетинам.
- Розрахунок ціни на межах тарифів та одноразова оплата вибраних послуг.
- Аналітика бронювань, згрупована за поточними ідентифікатором і назвою залу.
- Swagger/OpenAPI, доступний лише в середовищі Development.
- Безпечні відповіді ProblemDetails з ідентифікаторами трасування.
- Налаштовувані CORS, rate limiting, обмеження тіла запиту, HTTPS redirection і HSTS.
- Переносимі набори unit- та API-тестів.

## Технології

- .NET 10
- ASP.NET Core Web API with controllers
- Entity Framework Core 10
- Microsoft SQL Server
- xUnit
- Swashbuckle / Swagger

## Структура рішення

| Проєкт | Відповідальність |
| --- | --- |
| `ConferenceRooms.Api` | HTTP-контракти, контролери, прикладні сервіси, конфігурація та API pipeline. |
| `ConferenceRooms.Core` | Доменні сутності, правила часу бронювання та розрахунок орендної вартості. |
| `ConferenceRooms.Infrastructure` | EF Core SQL Server context, mappings, migrations і початкові дані. |
| `ConferenceRooms.UnitTests` | Переносимі тести Core і бізнес-правил. |
| `ConferenceRooms.ApiTests` | Переносимі тести HTTP pipeline, валідації та Swagger. |

Залежності спрямовані всередину: API та Infrastructure залежать від Core, а Core не залежить від ASP.NET Core або EF Core. API збирає застосунок і посилається на Infrastructure для роботи зі сховищем даних.

## Передумови

- .NET 10 SDK
- Microsoft SQL Server, доступний як `localhost`
- За потреби: `dotnet-ef` 10.x для ручного виконання команд EF CLI

## Налаштування бази даних

Збережене в репозиторії локальне Development-підключення використовує:

- Server: `localhost`
- Database: `ConferenceRoomsDb`
- Windows Integrated Authentication

Рядок підключення не містить пароля і не призначений для production. У цільовому середовищі перевизначте його через `ConnectionStrings__DefaultConnection`.

Застосуйте наявну migration з PowerShell:

```powershell
dotnet ef database update `
  --project src/ConferenceRooms.Infrastructure `
  --startup-project src/ConferenceRooms.Api
```

Однорядковий варіант для Windows:

```powershell
dotnet ef database update --project src/ConferenceRooms.Infrastructure --startup-project src/ConferenceRooms.Api
```

## Запуск API

```powershell
dotnet run --project src/ConferenceRooms.Api --launch-profile https
```

Development URLs:

- `https://localhost:7284`
- `http://localhost:5228`

## Swagger

Після запуску HTTPS Development profile відкрийте `https://localhost:7284/swagger`. Swagger UI та OpenAPI JSON document увімкнені лише в середовищі Development.

## Початкові дані

| Зал | Місткість | Базова погодинна ставка |
| --- | ---: | ---: |
| Hall A | 50 | 2000 UAH |
| Hall B | 100 | 3500 UAH |
| Hall C | 30 | 1500 UAH |

Кожен початковий зал пропонує:

- Projector: 500 UAH
- Wi-Fi: 300 UAH
- Sound system (`Sound`): 700 UAH

Файл [`src/ConferenceRooms.Api/ConferenceRooms.Api.http`](src/ConferenceRooms.Api/ConferenceRooms.Api.http) містить практичні приклади запитів і стабільні ID початкових даних.

## API Endpoints

| Метод | Route | Призначення |
| --- | --- | --- |
| GET | `/api/halls` | Отримати всі зали та їхні послуги. |
| GET | `/api/halls/{id}` | Отримати один зал. |
| GET | `/api/halls/available` | Знайти доступні зали за початком, тривалістю та місткістю. |
| POST | `/api/halls` | Створити зал. |
| PUT | `/api/halls/{id}` | Замінити дані залу та перелік послуг. |
| DELETE | `/api/halls/{id}` | Видалити зал без історичних бронювань. |
| POST | `/api/bookings` | Перевірити, розрахувати та підтвердити бронювання. |
| GET | `/api/bookings/{id}` | Отримати підтвердження бронювання. |
| GET | `/api/reports/bookings-summary` | Агрегувати кількість бронювань і дохід за період запланованого початку. |

## Правила ціноутворення

| Часовий сегмент | Множник ставки залу |
| --- | ---: |
| 06:00–09:00 | 0.90 (-10%) |
| 09:00–12:00 | 1.00 (базова ціна) |
| 12:00–14:00 | 1.15 (+15%) |
| 14:00–18:00 | 1.00 (базова ціна) |
| 18:00–23:00 | 0.80 (-20%) |

Тарифні сегменти є напіввідкритими та не накладаються один на одного. Для бронювання, що перетинає межу тарифів, ціна пропорційно розраховується за тривалістю в кожному сегменті. Вибрані послуги оплачуються один раз за бронювання.

Приклад для Hall A з 10:30 до 12:30 без послуг:

- 10:30–12:00: `1.5 × 2000 × 1.00 = 3000`
- 12:00–12:30: `0.5 × 2000 × 1.15 = 1150`
- Разом: `4150 UAH`

## Правила бронювання

- Бронювання має починатися в майбутньому.
- Час початку має бути точно о `:00` або `:30`, включно з нульовими секундами та sub-second ticks.
- `durationHours` — додатна ціла кількість годин.
- Усе бронювання має залишатися в межах 06:00–23:00 одного календарного дня.
- Кількість учасників не може перевищувати місткість залу.
- ID вибраних послуг мають належати вибраному залу та не можуть дублюватися.
- Перетин визначається за напіввідкритими інтервалами `[start, end)`.
- Суміжні інтервали дозволені: одне бронювання може починатися точно в момент завершення іншого.
- Підтверджений Booking є незмінним і зберігає знімки загальної ціни, назви залу та вибраних послуг.
- Зал, на який посилається історичний Booking, не можна видалити.

## Правила пошуку доступності

Пошук доступності виконується лише для майбутнього та використовує ті самі правила початку, тривалості, робочого часу й календарного дня, що й бронювання. Результати фільтруються за мінімальною місткістю та виключають зали з перетином напіввідкритих інтервалів. Наступний `POST /api/bookings` залишається авторитетною операцією, оскільки доступність може змінитися конкурентно.

## Звіти та аналітика

`GET /api/reports/bookings-summary?from=...&to=...` приймає обов'язкові межі `DateTimeOffset`, для яких `from < to`.

Booking входить до звіту, коли `Booking.StartAt` належить `[from, to)`: початок точно в `from` включається, а початок точно в `to` виключається. Дохід підсумовується з незмінних знімків `Booking.TotalPrice`, а не перераховується за поточними тарифами. Рядки залів групуються за стабільним Hall ID та поточною Hall name і впорядковуються спочатку за назвою, потім за ID. Для періоду без бронювань повертаються нульові підсумки та порожній список залів.

## Конкурентність

Створення бронювання використовує SQL Server Serializable transaction, що охоплює валідацію залу й послуг, перевірку перетинів і вставлення запису. Тому два конкурентні запити не можуть одночасно успішно зарезервувати той самий зал і часовий інтервал. Для SQL Server deadlock victim error 1205 виконується повтор із обмеженою затримкою, максимум три повні transaction attempts. Реалізація не заявляє і не потребує distributed locking.

## Безпека

- Безпечні відповіді ProblemDetails містять `traceId` і не розкривають stack traces або деталі бази даних.
- HTTPS redirection увімкнено; HSTS працює поза Development.
- CORS використовує точний allow-list origin-ів і за замовчуванням не довіряє жодному origin.
- Fixed-window rate limiter за замовчуванням дозволяє 120 запитів за 60 секунд для кожного remote IP без черги.
- Kestrel відхиляє тіла запитів розміром понад 64 KiB за замовчуванням.
- Build warnings обробляються як errors.
- Репозиторій не містить паролів, API tokens, private keys або certificates.

Authentication та authorization навмисно не вигадувалися, оскільки тестове завдання не визначає користувачів, ролі, володіння ресурсами або identity model.

## Тести

```powershell
dotnet build ConferenceRooms.sln
dotnet test ConferenceRooms.sln
```

Перевірений склад тестів:

- `ConferenceRooms.UnitTests`: 108 переносимих тестів Core і бізнес-правил.
- `ConferenceRooms.ApiTests`: 20 переносимих тестів HTTP pipeline, валідації та Swagger, які не потребують і не змінюють локальний SQL Server.
- Разом: 128 тестів (0 failed, 0 skipped).

Окремий real SQL smoke test використовується для перевірки persistence, конкурентності бронювань та агрегації звітів.

## Технічні рішення та компроміси

- Збережено сфокусовану архітектуру з трьох production-проєктів без додавання спекулятивних шарів.
- Generic repository не додано, оскільки EF Core `DbContext` надає достатню persistence abstraction у межах цього завдання.
- `DateTimeOffset` зберігає переданий offset для меж бронювання та звіту.
- Знімки Booking зберігають коректні історичні ціни після зміни поточних цін залу або послуг.
- Звіти агрегуються в SQL без завантаження відповідних Booking entities у пам'ять.
- Cancellation і status lifecycle бронювання не додані, оскільки їх не вимагали.
- Authentication model не вигадувалася без бізнес-вимог.
- Swagger доступний лише в Development.

## Конфігурація

| Environment variable | Призначення | Збережене значення за замовчуванням |
| --- | --- | --- |
| `ConnectionStrings__DefaultConnection` | SQL Server connection string | Локальний SQL Server із Windows Integrated Authentication |
| `Cors__AllowedOrigins__0` | Перший точний trusted browser origin | Немає trusted origins |
| `RateLimiting__PermitLimit` | Кількість запитів у fixed window | `120` |
| `RateLimiting__WindowSeconds` | Тривалість fixed window | `60` |
| `RequestLimits__MaxRequestBodySizeBytes` | Глобальне обмеження тіла запиту Kestrel | `65536` |

## Приклади HTTP

Приклади для Visual Studio та JetBrains HTTP Client доступні у [`src/ConferenceRooms.Api/ConferenceRooms.Api.http`](src/ConferenceRooms.Api/ConferenceRooms.Api.http). Вони охоплюють Hall CRUD, availability, booking, validation, overlap, pricing і межі звітів.
