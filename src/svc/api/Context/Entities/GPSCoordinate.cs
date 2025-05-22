namespace Api;

public class GPSCoordinate {
    public int Id {get; set;}
    public required string ImageId {get; set;}
    public required decimal Latitude { get; set; }
    public required decimal Longitude { get; set; }
    public required decimal Height { get; set; }

    public required int MapId { get; set; }


}
