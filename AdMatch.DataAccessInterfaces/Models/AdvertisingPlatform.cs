namespace AdMatch.DataAccessInterfaces.Models;

/// <summary>
/// Модель рекламной площадки с именем и списком локаций и списком локаций, в которых она действует.
/// </summary>
public class AdvertisingPlatform
{
    public string Name { get; set; }
    public List<string> Locations { get; set; }
}