using Grpc.Core;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Linq;
using WagonService;

namespace Niis.Services;

public class WagonsService : WagonService.WagonsService.WagonsServiceBase
{
    private readonly PostgresDbContext _dbContext;

    public WagonsService(PostgresDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public override async Task<WagonResponse> GetWagons(WagonRequest request, ServerCallContext context)
    {
        DateTime startTime;
        DateTime endTime;
        if (!DateTime.TryParseExact(request.StartTime, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out startTime) ||
            !DateTime.TryParseExact(request.EndTime, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out endTime))
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid date format. Expected format: yyyy-MM-dd HH:mm:ss"));
        }
        if (startTime >= endTime)
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Start time must be before end time."));
        }

        try
        {
            var events = await (
                from epc in _dbContext.Epc
                join epcEvent in _dbContext.EpcEvent on epc.Id equals epcEvent.IdEpc
                where epc.Number != "00000000"
                      && epcEvent.Time >= startTime
                      && epcEvent.Time <= endTime
                orderby epcEvent.Time
                select new
                {
                    epc.Number,
                    epcEvent.Time,
                    epcEvent.Type
                }).ToListAsync();

            var orderedEvents = events
                .GroupBy(e => e.Number)
                .SelectMany(g => g
                    .OrderBy(e => e.Time)
                    .Select((e, index) => new
                    {
                        e.Number,
                        e.Time,
                        e.Type,
                        EventOrder = index + 1
                    })
                ).ToList();

            var wagonsList = (
                from eArr in orderedEvents
                join eDep in orderedEvents on eArr.Number equals eDep.Number
                where eArr.Type == 0 // Arrival type
                      && eDep.Type == 1 // Departure type
                      && eArr.EventOrder == eDep.EventOrder - 1
                orderby eArr.Number, eArr.Time
                select new Wagon
                {
                    InventoryNumber = eArr.Number,
                    ArrivalTime = eArr.Time.ToString("yyyy-MM-dd HH:mm:ss"),
                    DepartureTime = eDep.Time.ToString("yyyy-MM-dd HH:mm:ss")
                }).ToList();

            return new WagonResponse
            {
                Wagons = { wagonsList }
            };

            
        }
        catch (Exception ex)
        {
            throw new RpcException(new Status(StatusCode.Internal, $"An error occurred while retrieving wagons: {ex.Message}"));
        }
    }
}
