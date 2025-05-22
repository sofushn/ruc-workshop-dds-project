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

    public DbSet<Waypoint> Waypoints { get; set; }
    public DbSet<Map> Maps { get; set; }
}
