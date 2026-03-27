# Awesome Files

**Тестовое задание (C# Intern / Junior)**

Асинхронный сервис архивирования файлов. REST API + CLI-клиент.  
.NET 9, Clean Architecture, Docker, PostgreSQL для логов.

---

## 📋 Оглавление

- [Архитектура](#-архитектура)
- [Технологии](#-технологии)
- [Запуск](#-запуск)
- [API](#-api)
- [CLI-клиент](#-cli-клиент)
- [Тестирование](#-тестирование)
- [Docker](#-docker)
- [Безопасность](#-безопасность)

---

## 🏗 Архитектура

Clean Architecture с чётким разделением ответственности:
Domain → сущности, статусы, бизнес-правила
Application → use cases, DTO, порты (интерфейсы)
Infrastructure → реализация портов: файловая система, очередь, архивация, кэш
API → контроллеры, middleware, Swagger
Client → CLI-клиент (System.CommandLine)
Tests → unit-тесты (xUnit, Moq)

text

**Ключевые решения:**

- `BackgroundService` + `ConcurrentQueue` + `SemaphoreSlim` — асинхронная очередь задач
- `ConcurrentDictionary` — in-memory хранилище задач и кэш архивов
- `lock` в `ArchiveTask` — потокобезопасное изменение статуса
- `Path.GetFullPath` + проверка `StartsWith` — защита от path traversal
- `Serilog` — структурированное логирование в консоль и PostgreSQL
- `System.CommandLine` — POSIX-совместимый CLI

---

## 🛠 Технологии

| Компонент | Стек |
|-----------|------|
| Backend | .NET 9, ASP.NET Core, Serilog |
| CLI | System.CommandLine, HttpClient |
| База | PostgreSQL 15 (логи) |
| Контейнеризация | Docker, Docker Compose |
| Тесты | xUnit, Moq, FluentAssertions, ReportGenerator |
| Архитектура | Clean Architecture, DI, async/await |

---

## 🚀 Запуск

### Docker (рекомендуется)

```bash
git clone https://github.com/fairwix/Awesome-Files.git
cd AwesomeFiles
docker compose up --build
API: http://localhost:5001
Swagger: http://localhost:5001/swagger
Локально

bash
cd AwesomeFiles.Api
dotnet run
📡 API

Метод	URL	Описание
GET	/api/files	Список файлов
POST	/api/archives	Создать архив → { "id": "..." }
GET	/api/archives/{id}	Статус
GET	/api/archives/{id}/download	Скачать архив
Статусы: Pending → InProgress → Completed / Failed
Коды ответов: 200, 202, 400, 404, 500

💻 CLI-клиент

POSIX-совместимая утилита с авто-режимом.

bash
dotnet run --project AwesomeFiles.Client
Команда	Пример	Описание
list	list	Список файлов
create-archive	create-archive file1.txt file2.txt	Создать архив → ID
status	status <id>	Статус задачи
download	download <id> ./downloads	Скачать архив
auto	auto file1.txt file2.txt ./downloads	Создать → ждать → скачать
Поддержка ENV: AWESOME_FILES_API_URL (по умолчанию http://localhost:5083)

🧪 Тестирование

bash
dotnet test
Покрытие

Line: 85%
Branch: 77%
Отчёт о покрытии

bash
dotnet test --collect:"XPlat Code Coverage" --results-directory ./coverage
reportgenerator -reports:./coverage/**/coverage.cobertura.xml -targetdir:./coverage/report -reporttypes:Html
open ./coverage/report/index.html
Что покрыто: Use Cases, сервисы, контроллеры, middleware, клиент (ApiClient, ArchiveClientService)

📦 Docker

Multi-stage сборка, non-root пользователь.

bash
docker compose up --build
Сервис	Назначение
api	ASP.NET Core
logsdb	PostgreSQL 15 (логи)
Volumes:

./Files → /app/Files
./Archives → /app/Archives
🔒 Безопасность

Path traversal защита (Path.GetFullPath + StartsWith)
Валидация входных данных
Лимит на количество файлов (50)
CancellationToken во всех асинхронных операциях
Non-root пользователь в Docker
Обработка ошибок через middleware с понятными HTTP-статусами
