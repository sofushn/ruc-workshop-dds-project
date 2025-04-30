namespace Api;

public class GPSCoordinate {
    public required int Id {get; set;}
    public required int ImageId {get; set;}
    public required decimal Latitude { get; set; }
    public required decimal Longitude { get; set; }

    public required int MapId { get; set; }


}
