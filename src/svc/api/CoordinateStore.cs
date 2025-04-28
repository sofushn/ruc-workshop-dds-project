using System.Collections.Concurrent;

namespace Api;

public class CoordinateStore
{
    private readonly ConcurrentDictionary<string, GPSCoordinate> _coordinates = new();

    public IEnumerable<GPSCoordinate> GetAll() => _coordinates.Values;

    public GPSCoordinate? Get(string id) =>
        _coordinates.TryGetValue(id, out var coord) ? coord : null;

    public void Add(GPSCoordinate coordinate) =>
        _coordinates[coordinate.ImageId] = coordinate;

    public bool Update(string id, GPSCoordinate updated)
    {
        return _coordinates.TryUpdate(id, updated, _coordinates[id]);
    }

    public bool Delete(string id) =>
        _coordinates.TryRemove(id, out _);
}
