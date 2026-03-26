# 🚀 Awesome Files

**.NET • PostgreSQL • Docker • CLI • Tests**

Сервис для асинхронного архивирования файлов с REST API и CLI клиентом.
**Backend** — REST API для управления файлами и создания ZIP-архивов.  
**Консольный клиент** — POSIX-совместимая утилита для взаимодействия с бэкендом.

---

## 📋 Оглавление

* [✨ Возможности](#-возможности)
* [🛠 Технологический стек](#-технологический-стек)
* [🏗 Архитектура](#-архитектура)
* [⚙️ Как это работает](#️-как-это-работает)
* [🚀 Быстрый запуск](#-быстрый-запуск)
* [📡 API Endpoints](#-api-endpoints)
* [💻 CLI клиент](#-cli-клиент)
* [🧪 Тестирование](#-тестирование)
* [📦 Docker](#-docker)
* [🔒 Безопасность](#-безопасность)
* [📈 Соответствие ТЗ](#-соответствие-тз)

---

## ✨ Возможности

* 📂 Получение списка файлов
* 📦 Асинхронное создание ZIP архивов
* ⏳ Отслеживание статуса задач
* ⬇️ Скачивание архивов
* ⚡ Кэширование архивов (повторные запросы ускоряются)
* 🧵 Очередь фоновых задач
* 📊 Логирование в PostgreSQL + консоль
* 💻 CLI клиент (POSIX стиль)
* 🤖 Auto режим (одной командой всё сделать)
* 🧪 Покрытие тестами ~85%

---

## 🛠 Технологический стек

### Backend

* .NET 9 + ASP.NET Core Web API
* Clean Architecture
* Serilog — логирование
* BackgroundService — фоновые задачи
* ConcurrentDictionary — хранение состояния
* CancellationToken — во всех async операциях

### CLI

* System.CommandLine
* HttpClient (typed)

### Инфраструктура

* Docker + Docker Compose
* PostgreSQL 15 — хранение логов

### Тестирование

* xUnit
* Moq
* FluentAssertions
* ReportGenerator

---

## 🏗 Архитектура

```bash
AwesomeFiles/
├── AwesomeFiles.Domain
├── AwesomeFiles.Application
├── AwesomeFiles.Infrastructure
├── AwesomeFiles.Api
├── AwesomeFiles.Client
└── AwesomeFiles.Tests
```

### Слои:

**Domain**

* ArchiveTask
* ArchiveStatus
* Бизнес-правила

**Application**

* UseCases
* DTO
* Валидация

**Infrastructure**

* FileService
* ArchiveService (ядро)
* BackgroundTaskQueue
* ArchiveWorker

**API**

* Controllers
* Middleware
* Swagger

**Client**

* CLI команды
* Auto режим

---

## ⚙️ Как это работает

```text
1. Пользователь → POST /archives
2. Создаётся ArchiveTask
3. Задача кладётся в очередь
4. BackgroundWorker обрабатывает
5. Архив создаётся или берётся из кэша
6. Пользователь проверяет статус
7. Скачивает архив
```

---

## 🚀 Быстрый запуск

### 🐳 Через Docker (РЕКОМЕНДУЕТСЯ)

```bash
git clone <your-repo>
cd AwesomeFiles

docker-compose up --build
```

После запуска:

* API → [http://localhost:5001](http://localhost:5001)
* Swagger → [http://localhost:5001/swagger](http://localhost:5001/swagger)
* PostgreSQL → localhost:5433

---

### 🖥 Локально (без Docker)

#### Требования:

* .NET 9 SDK
* PostgreSQL (опционально)

```bash
cd AwesomeFiles.Api
dotnet run
```

---

## 📡 API Endpoints

### 📂 Получить файлы

```http
GET /api/files
```

---

### 📦 Создать архив

```http
POST /api/archives
```

```json
{
  "fileNames": ["file1.txt", "file2.txt"]
}
```

---

### ⏳ Статус

```http
GET /api/archives/{id}
```

---

### ⬇️ Скачать

```http
GET /api/archives/{id}/download
```

---

## 💻 CLI клиент

### 🚀 Запуск

```bash
dotnet run --project AwesomeFiles.Client
```

---

### 🔧 ENV

```bash
export AWESOME_FILES_API_URL=http://localhost:5001
```

---

### 📟 Команды

#### 📂 Список файлов

```bash
list
```

---

#### 📦 Создать архив

```bash
create-archive file1.txt file2.txt
```

---

#### ⏳ Статус

```bash
status <id>
```

---

#### ⬇️ Скачать

```bash
download <id> ./downloads
```

---

#### 🤖 AUTO режим (⭐)

```bash
auto file1.txt file2.txt ./downloads
```

👉 Полный цикл:

* создать
* дождаться
* скачать

---

## 🧪 Тестирование

```bash
dotnet test
```

### 📊 Покрытие

* Line coverage: **85%**
* Branch coverage: **77%**

---

## 📦 Docker

```bash
docker-compose up --build
```

### Сервисы:

* `api` — ASP.NET Core
* `logsdb` — PostgreSQL

---

### Volumes

```bash
./Files → /app/Files
./Archives → /app/Archives
```

---

## 🔒 Безопасность

* Проверка path traversal
* Валидация входных данных
* Обработка ошибок через middleware
* Контроль статусов задач

---
