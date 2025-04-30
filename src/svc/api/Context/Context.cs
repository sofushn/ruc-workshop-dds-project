using Microsoft.EntityFrameworkCore;



namespace Api;
public class MetadataContext : DbContext
{
    private readonly IConfiguration _configuration;

    public MetadataContext(DbContextOptions<MetadataContext> options, IConfiguration configuration)
    : base(options)
    {
        _configuration = configuration;
    }

    public DbSet<GPSCoordinate> GPSCoordinates { get; set; }
    public DbSet<Map> Maps { get; set; }



    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<GPSCoordinate>().ToTable("waypoint");
        modelBuilder.Entity<GPSCoordinate>().Property(x => x.Id).HasColumnName("id");
        modelBuilder.Entity<GPSCoordinate>().Property(x => x.Latitude).HasColumnName("latitude");
        modelBuilder.Entity<GPSCoordinate>().Property(x => x.Longitude).HasColumnName("longitude");
        modelBuilder.Entity<GPSCoordinate>().Property(x => x.ImageId).HasColumnName("image_id");
        modelBuilder.Entity<GPSCoordinate>().Property(x => x.MapId).HasColumnName("map_id");

        modelBuilder.Entity<Map>().ToTable("map");
        modelBuilder.Entity<Map>().Property(x => x.Id).HasColumnName("id");
        modelBuilder.Entity<Map>().Property(x => x.NELatitude).HasColumnName("NE_latitude");
        modelBuilder.Entity<Map>().Property(x => x.NELongitude).HasColumnName("NE_longitude");
        modelBuilder.Entity<Map>().Property(x => x.SWLatitude).HasColumnName("SW_latitude");
        modelBuilder.Entity<Map>().Property(x => x.SWLongitude).HasColumnName("SW_longitude");
        modelBuilder.Entity<Map>().Property(x => x.ImageId).HasColumnName("image_id");

    }
}
