namespace AdMatch.DataAccessInterfaces.Models;

/// <summary>
/// Узел префиксного дерева, представляющий сегмент локации.
/// Хранит дочерние узлы и рекламные площадки, действующие для на этом уровне.
/// </summary>
public class LocationNode
{
    public Dictionary<string, LocationNode> Children { get; } = new();
    public HashSet<string> Platforms { get; } = [];
}