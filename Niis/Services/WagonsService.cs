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
            // тут должен быть linq 
            var wagonsList = await (
                from epc in _dbContext.Epc
                join epcEvent in _dbContext.EpcEvent on epc.Id equals epcEvent.IdEpc
                where epc.Number != "00000000"
                      && epcEvent.Time >= startTime
                      && epcEvent.Time <= endTime
                let eventOrder = _dbContext.EpcEvent
                    .Where(e => e.IdEpc == epc.Id)
                    .OrderBy(e => e.Time)
                    .Select((e, index) => new { e, EventOrder = index + 1 })
                from e_arr in eventOrder
                from e_dep in eventOrder
                where e_arr.e.Type == 0
                      && e_dep.e.Type == 1
                      && e_arr.EventOrder == e_dep.EventOrder - 1
                orderby e_arr.e.IdEpc, e_arr.e.Time
                select new Wagon
                {
                    InventoryNumber = epc.Number,
                    ArrivalTime = e_arr.e.Time.ToString("yyyy-MM-dd HH:mm:ss"),
                    DepartureTime = e_dep.e.Time.ToString("yyyy-MM-dd HH:mm:ss")
                }).ToListAsync();

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
