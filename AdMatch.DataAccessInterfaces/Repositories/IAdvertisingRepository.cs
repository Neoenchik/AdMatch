using AdMatch.DataAccessInterfaces.Models;

namespace AdMatch.DataAccessInterfaces.Repositories;

/// <summary>
/// In-memory реализация репозитория рекламных площадок.
/// </summary>
public interface IAdvertisingRepository
{
    /// <summary>
    /// Загружает рекламные платформы и строит дерево по их локациям.
    /// Полностью очищает предыдущее состояние.
    /// </summary>
    /// <param name="platforms">Рекламные платформы</param>
    void LoadPlatforms(IEnumerable<AdvertisingPlatform> platforms);
    
    /// <summary>
    /// Возвращает список рекламных платформ, действующих в заданной локации
    /// </summary>
    /// <param name="location">Локация</param>
    /// <returns></returns>
    List<string> GetPlatformsForLocation(string location);
}