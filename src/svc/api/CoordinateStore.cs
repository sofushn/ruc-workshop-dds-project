namespace Api;

public class CoordinateStore
{
    private readonly Dictionary<string, GPSCoordinate> _coordinates = new();

    public IEnumerable<GPSCoordinate> GetAll() => _coordinates.Values;

    public GPSCoordinate? Get(string id) =>
        _coordinates.TryGetValue(id, out var coord) ? coord : null;

    public void Add(GPSCoordinate coordinate) =>
        _coordinates[coordinate.ImageId] = coordinate;

    public bool Update(string id, GPSCoordinate updated)
    {
        if (!_coordinates.ContainsKey(id)) return false;
        _coordinates[id] = updated;
        return true;
    }

    public bool Delete(string id) =>
        _coordinates.Remove(id);
}
