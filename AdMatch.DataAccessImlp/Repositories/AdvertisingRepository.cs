using System.Collections.Concurrent;
using AdMatch.DataAccessInterfaces.Models;
using AdMatch.DataAccessInterfaces.Repositories;

namespace AdMatch.Domain.Repositories;

public class AdvertisingRepository : IAdvertisingRepository
{
    private readonly LocationNode _root = new();
    private readonly ConcurrentDictionary<string, HashSet<string>> _cache = new();
    
    public void LoadPlatforms(IEnumerable<AdvertisingPlatform> platforms)
    {
        _root.Children.Clear();
        _cache.Clear();

        foreach (var platform in platforms)
        {
            foreach (var location in platform.Locations)
            {
                var normalizedLoc = NormalizeLocation(location);
                var node = GetOrCreateNode(normalizedLoc);
                node.Platforms.Add(platform.Name);
            }
        }
    }

    public List<string> GetPlatformsForLocation(string location)
    {
        var normalized = NormalizeLocation(location);
        
        if(_cache.TryGetValue(normalized, out var cached))
            return cached.ToList();
        
        var platforms = new HashSet<string>();
        var node = _root;
        var segments = location.Split('/',  StringSplitOptions.RemoveEmptyEntries);
        foreach (var segment in segments)
        {
            if (!node.Children.TryGetValue(segment, out var child))
                return [];
            
            node = child;
            platforms.UnionWith(node.Platforms);
        }
        
        _cache[normalized] = platforms;
        return platforms.ToList();
    }

    /// <summary>
    /// Получает или создает ноду
    /// </summary>
    private LocationNode GetOrCreateNode(string path)
    {
        var node = _root;
        var segments = path.Split('/',  StringSplitOptions.RemoveEmptyEntries);

        foreach (var segment in segments)
        {
            if (!node.Children.TryGetValue(segment, out var child))
            {
                child = new LocationNode();
                node.Children.Add(segment, child);
            }
            node = child;
        }
        return node;
    }

    /// <summary>
    /// Нормализует локацию
    /// </summary>
    private static string NormalizeLocation(string location)
        => location.Trim().ToLowerInvariant();
}