# ImageGalleryApp

FullStack проект .NET C# ASP

Web-приложение для загрузки и просмотра изображений.

[Техническое задание к проекту](https://docs.google.com/document/d/1d7kElNHmTkOwaSfvxIYySmMDocQ-E0YfctY693HJoCo/edit?usp=sharing)

# Инструкция к запуску
## Требования

- [Docker](https://www.docker.com/products/docker-desktop/) (версия 20.10+)
- [Docker Compose](https://docs.docker.com/compose/) (версия 2.0+)
- Git

### 1. Клонирование репозитория
```bash
git clone https://github.com/Stanislaw-P/ImageGalleryApp.git
cd <папка-проекта>
```

### 2. Настройка переменных окружения
Создайте файл .env в корне проекта:
```env
# База данных
DB_USER=postgres
DB_PASSWORD=your_strong_password
DB_NAME=imagegallery

# pgAdmin
PGADMIN_PASSWORD=admin123

# ASP.NET Core (опционально)
ASPNETCORE_ENVIRONMENT=Development
```

### 3. Запуск проекта
```bash
docker-compose up -d
```
### 4. Проверка работы сервисов
Сервис	Адрес	Описание
ASP.NET API	http://localhost:8080	Основное приложение

pgAdmin	http://localhost:5050	Управление БД

PostgreSQL	localhost:5432	База данных

### 5. Остановка проекта
```bash
docker-compose down
```
