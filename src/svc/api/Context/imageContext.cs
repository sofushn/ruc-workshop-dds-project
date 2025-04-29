using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using DataAcessLayer.Entities;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;



namespace DataAcessLayer.Context;
public class imageContext : DbContext
{
    private readonly IConfiguration _configuration;

    public imageContext()
    {
    }

    public imageContext(DbContextOptions<imageContext> options, IConfiguration configuration)
    : base(options)
    {
        _configuration = configuration;
    }

    public DbSet<Image> Images { get; set; }


    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.LogTo(Console.WriteLine, Microsoft.Extensions.Logging.LogLevel.Information);
            string connectionString = _configuration.GetConnectionString("host=localhost:5433; db=database; uid=postgres; pwd=postgres;");
            optionsBuilder.UseNpgsql(connectionString);
        }

    }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        //Tables
        modelBuilder.Entity<Image>().ToTable("waypoint");
        modelBuilder.Entity<Image>().Property(x => x.Id).HasColumnName("id");
        modelBuilder.Entity<Image>().Property(x => x.Latitude).HasColumnName("latitude");
        modelBuilder.Entity<Image>().Property(x => x.Longitude).HasColumnName("longitude");
        modelBuilder.Entity<Image>().Property(x => x.ImageId).HasColumnName("image_id");
        modelBuilder.Entity<Image>().Property(x => x.MapId).HasColumnName("map_id");

        modelBuilder.Entity<Image>().ToTable("map");
        modelBuilder.Entity<Image>().Property(x => x.Id).HasColumnName("id");
        modelBuilder.Entity<Image>().Property(x => x.NELatitude).HasColumnName("NE_latitude");
        modelBuilder.Entity<Image>().Property(x => x.NELongitude).HasColumnName("NE_longitude");
        modelBuilder.Entity<Image>().Property(x => x.SWLatitude).HasColumnName("SW_latitude");
        modelBuilder.Entity<Image>().Property(x => x.SWLongitude).HasColumnName("SW_longitude");
        modelBuilder.Entity<Image>().Property(x => x.ImageId).HasColumnName("image_id");



    }
}
