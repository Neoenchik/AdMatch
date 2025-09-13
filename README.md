# BCS.EFS.AdvertisingPlatformsService

## Запуск
1. Клонируйте репозиторий.
2. Откройте в Rider (.NET 8).
3. Запустите AdMatch.Api (IIS Express/https/http или dotnet run).
4. По необходимости установить SSL сертификаты .NET, которые предложит Rider 
5. Swagger: https://localhost:<port>/swagger

## Endpoints
- POST /api/advertising/load — загрузка файла (multipart/form-data).
- GET /api/advertising/search?location=/ru/svrd — поиск.

## Тесты
dotnet test