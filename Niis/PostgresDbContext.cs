using Microsoft.EntityFrameworkCore;


namespace Niis;


public class PostgresDbContext : DbContext
{
    public PostgresDbContext(DbContextOptions<PostgresDbContext> options) : base(options) { }

    public DbSet<Park> Parks { get; set; }
    public DbSet<Path> Paths { get; set; }
    public DbSet<Epc> Epics { get; set; }
    public DbSet<EpcEvent> EpcEvents { get; set; }
    public DbSet<EventArrival> EventArrivals { get; set; }
    public DbSet<EventDeparture> EventDepartures { get; set; }
    public DbSet<EventAdd> EventAdds { get; set; }
    public DbSet<EventSub> EventSubs { get; set; }
}
public class Park
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string AsuNumber { get; set; }
    public int Type { get; set; }
    public int Direction { get; set; }
}
public class Path
{
    public int Id { get; set; }
    public string AsuNumber { get; set; }
    public int IdPark { get; set; }
}

public class Epc
{
    public int Id { get; set; }
    public string Number { get; set; }
    public int Type { get; set; }
}

public class EpcEvent
{
    public DateTime Time { get; set; }
    public int IdPath { get; set; }
    public int Type { get; set; }
    public int NumberInOrder { get; set; }
    public int IdEpc { get; set; }
}

public class EventArrival
{
    public DateTime Time { get; set; }
    public int IdPath { get; set; }
    public string TrainNumber { get; set; }
    public string TrainIndex { get; set; }
}

public class EventDeparture
{
    public DateTime Time { get; set; }
    public int IdPath { get; set; }
    public string TrainNumber { get; set; }
    public string TrainIndex { get; set; }
}

public class EventAdd
{
    public DateTime Time { get; set; }
    public int IdPath { get; set; }
    public int Direction { get; set; }
}

public class EventSub
{
    public DateTime Time { get; set; }
    public int IdPath { get; set; }
    public int Direction { get; set; }
}