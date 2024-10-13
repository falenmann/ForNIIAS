using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Niis;

public class PostgresDbContext : DbContext
{
    public PostgresDbContext(DbContextOptions<PostgresDbContext> options) : base(options) {}

    /*protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseNpgsql()
    }*/

    public DbSet<Parks> Park { get; set; }
    public DbSet<Paths> Path { get; set; }
    public DbSet<Epcs> Epc { get; set; }
    public DbSet<EpcEvents> EpcEvent { get; set; }
    public DbSet<EventArrivals> EventArrival { get; set; }
    public DbSet<EventDepartures> EventDeparture { get; set; }
    public DbSet<EventAdds> EventAdd { get; set; }
    public DbSet<EventSubs> EventSub { get; set; }
    

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Define a DateTime converter for UTC DateTimes
        var dateTimeConverter = new ValueConverter<DateTime, DateTime>(
            v => v.Kind == DateTimeKind.Unspecified ? DateTime.SpecifyKind(v, DateTimeKind.Utc) : v.ToUniversalTime(),
            v => DateTime.SpecifyKind(v, DateTimeKind.Utc)
        );

        var nullableDateTimeConverter = new ValueConverter<DateTime?, DateTime?>(
            v => v.HasValue && v.Value.Kind == DateTimeKind.Unspecified
                ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc)
                : v.Value.ToUniversalTime(),
            v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : v
        );
        
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            var dateTimeProperties = entityType.ClrType.GetProperties()
                .Where(p => p.PropertyType == typeof(DateTime) || p.PropertyType == typeof(DateTime?));

            foreach (var property in dateTimeProperties)
            {
                if (property.PropertyType == typeof(DateTime))
                {
                    modelBuilder.Entity(entityType.Name).Property(property.Name)
                        .HasConversion(dateTimeConverter);
                }
                else if (property.PropertyType == typeof(DateTime?))
                {
                    modelBuilder.Entity(entityType.Name).Property(property.Name)
                        .HasConversion(nullableDateTimeConverter);
                }
            }
        }

        // Configure keyless entities
        modelBuilder.Entity<EpcEvents>().HasNoKey();
        modelBuilder.Entity<EventAdds>().HasNoKey();
        modelBuilder.Entity<EventArrivals>().HasNoKey();
        modelBuilder.Entity<EventDepartures>().HasNoKey();
        modelBuilder.Entity<EventSubs>().HasNoKey();

        base.OnModelCreating(modelBuilder);
    }

    public class Parks
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string AsuNumber { get; set; }
        public int Type { get; set; }
        public int Direction { get; set; }
    }

    public class Paths
    {
        public int Id { get; set; }
        public string AsuNumber { get; set; }
        public int IdPark { get; set; }
    }

    public class Epcs
    {
        public int Id { get; set; }
        public string Number { get; set; }
        public int Type { get; set; }
    }

    public class EpcEvents
    {
        public DateTime Time { get; set; } = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc);
        public int IdPath { get; set; }
        public int Type { get; set; }
        public int NumberInOrder { get; set; }
        public int IdEpc { get; set; }
    }

    public class EventArrivals
    {
        public DateTime Time { get; set; }
        public int IdPath { get; set; }
        public string TrainNumber { get; set; }
        public string TrainIndex { get; set; }
    }

    public class EventDepartures
    {
        public DateTime Time { get; set; }
        public int IdPath { get; set; }
        public string TrainNumber { get; set; }
        public string TrainIndex { get; set; }
    }

    public class EventAdds
    {
        public DateTime Time { get; set; }
        public int IdPath { get; set; }
        public int Direction { get; set; }
    }

    public class EventSubs
    {
        public DateTime Time { get; set; }
        public int IdPath { get; set; }
        public int Direction { get; set; }
    }
}
