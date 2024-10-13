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
            // 1. Выполняем запрос и загружаем данные
            var wagonsRaw = await _dbContext.EventDeparture
                .GroupJoin(_dbContext.EventSub,
                    ed => ed.IdPath,
                    es => es.IdPath,
                    (ed, esGroup) => new { ed, esGroup })
                .SelectMany(
                    x => x.esGroup.DefaultIfEmpty(),
                    (x, es) => new { x.ed, es })
                .GroupJoin(_dbContext.EpcEvent,
                    temp => temp.ed.IdPath,
                    ee => ee.IdPath,
                    (temp, eeGroup) => new { temp.ed, temp.es, eeGroup })
                .SelectMany(
                    x => x.eeGroup.DefaultIfEmpty(),
                    (x, ee) => new { x.ed, x.es, ee })
                .GroupJoin(_dbContext.Epc,
                    temp => temp.ee.IdEpc,
                    e => e.Id,
                    (temp, eGroup) => new { temp.ed, temp.es, temp.ee, eGroup })
                .SelectMany(
                    x => x.eGroup.DefaultIfEmpty(),
                    (x, e) => new { x.ed, x.es, x.ee, e })
                .GroupJoin(_dbContext.EventAdd,
                    temp => temp.ed.IdPath,
                    ea => ea.IdPath,
                    (temp, eaGroup) => new { temp.ed, temp.es, temp.ee, temp.e, eaGroup })
                .SelectMany(
                    x => x.eaGroup.DefaultIfEmpty(),
                    (x, ea) => new { x.ed, x.es, x.ee, x.e, ea })
                .GroupJoin(_dbContext.EventArrival,
                    temp => temp.ea.IdPath,
                    a => a.IdPath,
                    (temp, aGroup) => new { temp.ed, temp.es, temp.ee, temp.e, temp.ea, aGroup })
                .SelectMany(
                    x => x.aGroup.DefaultIfEmpty(),
                    (x, a) => new { x.ed, x.e, x.ea })
                .Where(x => x.ed.Time >= startTime
                            && x.ed.Time <= endTime && x.e.Number != "00000000")
                .Select(x => new Wagon
                {
                    InventoryNumber = x.e.Number,
                    ArrivalTime = x.ea.Time.ToLongTimeString(),  // ISO 8601 формат
                    DepartureTime = x.ed.Time.ToLongTimeString()  // ISO 8601 формат
                })
                .Distinct()
                .ToListAsync();
            
           




            
            var response = new WagonResponse
            {
                Wagons = { wagonsRaw } 
            };

            return response; 
        }
        catch (Exception ex)
        {
            throw new RpcException(new Status(StatusCode.Internal, $"An error occurred while retrieving wagons: {ex.Message}"));
        }
    }
}
