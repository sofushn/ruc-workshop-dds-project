namespace Api.Context;

public class Map
{
    public required int Id { get; set; }
    public required decimal NELatitude { get; set; }
    public required decimal NELongitude { get; set; }
    public required decimal SWLatitude { get; set; }
    public required decimal SWLongitude { get; set; }

    public required string ImageId  { get; set; }
}
