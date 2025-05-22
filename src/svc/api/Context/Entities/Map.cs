using System.ComponentModel.DataAnnotations.Schema;

namespace Api;

[Table("map")]
public class Map
{
    [Column("id")]
    public required int Id { get; set; }
    [Column("ne_latitude")]
    public required decimal NELatitude { get; set; }
    [Column("ne_longitude")]
    public required decimal NELongitude { get; set; }
    [Column("sw_latitude")]
    public required decimal SWLatitude { get; set; }
    [Column("sw_longitude")]
    public required decimal SWLongitude { get; set; }
    [Column("image_id")]
    public required string ImageId { get; set; }
}
