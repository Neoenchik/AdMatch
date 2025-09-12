using AdMatch.DataAccessInterfaces.Models;
using AdMatch.DataAccessInterfaces.Repositories;
using AdMatch.MicroserviceInterfaces.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace AdMatch.Application.Services;

public class AdvertisingService : IAdvertisingService
{
    private readonly IAdvertisingRepository _repository;
    private readonly ILogger<AdvertisingService> _logger;

    /// <summary>
    /// ctor
    /// </summary>
    /// <param name="repository">Репозиторий рекламных площадок</param>
    /// <param name="logger">Логгер.</param>
    public AdvertisingService(IAdvertisingRepository repository, ILogger<AdvertisingService> logger)
    {
        _repository = repository;
        _logger = logger;
    }


    public async Task LoadPlatformsFromFileAsync(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            _logger.LogWarning("Попытка загрузить пустой или null файл.");
            throw new ArgumentException("Некорректный файл");
        }

        var platforms = new List<AdvertisingPlatform>();
        await using var stream = file.OpenReadStream();
        using var reader = new StreamReader(stream);

        while (await reader.ReadLineAsync() is { } line)
        {
            if(string.IsNullOrWhiteSpace(line))
                continue;

            try
            {
                var parts =  line.Split(':',2);
                if (parts.Length != 2)
                {
                    throw new FormatException($"Некорректный формат строки: {line}");
                }

                var name = parts[0].Trim();
                var locs = parts[1].Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => x.Trim())
                    .Where(x => x.StartsWith("/"))
                    .ToList();
                
                if (locs.Count == 0)
                {
                    throw new FormatException($"Отсутствуют допустимые локации для площадки «{name}»");
                }

                platforms.Add(new AdvertisingPlatform { Name = name, Locations = locs });
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Ошибка при обработке строки: '{Line}'", line);
            }
        }
        _repository.LoadPlatforms(platforms);
        if (platforms.Count == 0)
        {
            _logger.LogInformation("Не загружено ни одной платформы из файла.");
        }
        else
        {
            _logger.LogInformation("Загружено {Count} платформ из файла.", platforms.Count);
        }
    }

    public Task<List<string>> SearchPlatformsAsync(string location)
    {
        if (string.IsNullOrWhiteSpace(location) || !location.StartsWith("/"))
        {
            _logger.LogWarning("Некорректный параметр локации: {Location}", location);
            throw new ArgumentException("Некорректная локация");
        }

        try
        {
            return Task.FromResult(_repository.GetPlatformsForLocation(location));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при поиске площадок для локации: {Location}", location);
            throw;
        }
    }
}