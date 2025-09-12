namespace AdMatch.DataAccessInterfaces.Models;

/// <summary>
/// Модель рекламной площадки с именем и списком локаций и списком локаций, в которых она действует.
/// </summary>
public class AdvertisingPlatform
{
    /// <summary>
    /// Имя рекламной площадки (обязательное).
    /// </summary>
    public required string Name { get; init; }
    
    /// <summary>
    /// Список локаций, в которых действует площадка (обязательный).
    /// </summary>
    public required List<string> Locations { get; init; }
}