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
            var wagons = await _dbContext.EventDepartures
                .Where(e => e.Time >= startTime && e.Time <= endTime)
                .Join(_dbContext.EpcEvents,
                    dep => dep.IdPath,
                    epcEvent => epcEvent.IdPath,
                    (dep, epcEvent) => new { dep, epcEvent })
                .Join(_dbContext.Epics,
                    result => result.epcEvent.IdEpc,
                    epc => epc.Id,
                    (result, epc) => new { result.dep, epc })
                .Where(res => res.epc.Number != "00000000")
                .Select(res => new WagonService.Wagon
                {
                    InventoryNumber = res.epc.Number,
                    ArrivalTime = res.dep.Time.ToString("yyyy-MM-dd HH:mm:ss"),
                    DepartureTime = res.dep.Time.ToString("yyyy-MM-dd HH:mm:ss")
                })
                .ToListAsync();
            
            var response = new WagonResponse
            {
                Wagons = { wagons } 
            };

            return response; 
        }
        catch (Exception ex)
        {
            throw new RpcException(new Status(StatusCode.Internal, $"An error occurred while retrieving wagons: {ex.Message}"));
        }
    }
}
