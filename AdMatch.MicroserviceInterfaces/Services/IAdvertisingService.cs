using Microsoft.AspNetCore.Http;
namespace AdMatch.MicroserviceInterfaces.Services;

/// <summary>
/// Сервис для работы с площадкой
/// </summary>
public interface IAdvertisingService
{
    /// <summary>
    /// Загружает рекламные площадки из файла 
    /// </summary>
    Task LoadPlatformsFromFileAsync(IFormFile file);
    
    /// <summary>
    /// Возвращает список рекламных площадок, действующих в заданной локации
    /// </summary>
    Task<List<string>> SearchPlatformsAsync(string location);
}