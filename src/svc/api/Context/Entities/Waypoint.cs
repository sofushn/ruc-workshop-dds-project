using System.ComponentModel.DataAnnotations.Schema;

namespace Api;

[Table("waypoint")]
public class Waypoint
{
    [Column("id")]
    public int Id { get; set; }
    [Column("image_id")]
    public required string ImageId { get; set; }
    [Column("latitude")]
    public required decimal Latitude { get; set; }
    [Column("longitude")]
    public required decimal Longitude { get; set; }
    [Column("height")]
    public required decimal Height { get; set; }
    [Column("map_id")]
    public required int MapId { get; set; }
}
