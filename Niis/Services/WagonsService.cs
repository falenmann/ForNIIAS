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
            var wagonsRaw = await (
                    from departure in _dbContext.EventDeparture
                    join epcEvent in _dbContext.EpcEvent on 
                        new { departure.IdPath, Time = departure.Time } 
                        equals 
                        new { epcEvent.IdPath, epcEvent.Time } 
                        into departureGroup
                    from d in departureGroup
                    join epc in _dbContext.Epc on d.IdEpc equals epc.Id
                    where departure.Time >= startTime && departure.Time <= endTime
                                                      && epc.Number != "00000000"
                    select new
                    {
                        InventoryNumber = epc.Number,
                        DepartureTime = departure.Time,
                        DeparturePath = departure.IdPath
                    }
                ).SelectMany(departure => (
                    from arrival in _dbContext.EventArrival
                    where arrival.Time <= departure.DepartureTime
                          && arrival.IdPath != departure.DeparturePath
                          && arrival.Time >= startTime
                    orderby arrival.Time descending
                    select new
                    {
                        departure.InventoryNumber,
                        ArrivalTime = arrival.Time,
                        departure.DepartureTime
                    })).ToListAsync();
           

            
            var wagonsList = wagonsRaw.Select(w => new WagonService.Wagon
            {
                InventoryNumber = w.InventoryNumber,
                ArrivalTime = w.ArrivalTime.ToString("yyyy-MM-dd HH:mm:ss"), 
                DepartureTime = w.DepartureTime.ToString("yyyy-MM-dd HH:mm:ss") 
            }).ToList();
            
            var response = new WagonResponse
            {
                Wagons = { wagonsList }
            };

            return response;

            return response; 
        }
        catch (Exception ex)
        {
            throw new RpcException(new Status(StatusCode.Internal, $"An error occurred while retrieving wagons: {ex.Message}"));
        }
    }
}
