# BCS.EFS.AdvertisingPlatformsService

## Запуск
1. Клонируй репозиторий.
2. Открой в Visual Studio 2022+ (.NET 8).
3. Сгенерируй контроллеры: запусти .sh скрипты в MicroserviceInterfaces (нужен NSwag).
4. Установи пакеты: NuGet restore.
5. Запусти BCS.EFS.AdvertisingPlatformsService (IIS Express или dotnet run).
6. Swagger: https://localhost:<port>/swagger

## Endpoints
- POST /api/advertising/load — загрузка файла (multipart/form-data).
- GET /api/advertising/search?location=/ru/svrd — поиск.

## Тесты
dotnet test