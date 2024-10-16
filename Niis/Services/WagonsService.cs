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
// Находим все события для вагонов в указанном диапазоне по времени
            var events = await (
                from epc in _dbContext.Epc
                join epcEvent in _dbContext.EpcEvent on epc.Id equals epcEvent.IdEpc
                where epc.Number != "00000000"
                orderby epc.Number, epcEvent.Time
                select new
                {
                    epc.Number,
                    epcEvent.Time,
                    epcEvent.Type
                }).ToListAsync();

// Пронумеровываем события для каждой группы вагонов
            var orderedEvents = events
                .GroupBy(e => e.Number)
                .SelectMany(g => g
                    .OrderBy(e => e.Time)
                    .Select((e, index) => new
                    {
                        e.Number,
                        e.Time,
                        e.Type,
                        EventOrder = index + 1 // аналог ROW_NUMBER()
                    })
                ).ToList();

// Находим отправления в указанном диапазоне, и ищем к ним соответствующие прибытия
            var wagonsList = (
                from eDep in orderedEvents
                join eArr in orderedEvents on eDep.Number equals eArr.Number
                where eDep.Type == 1 // Departure type
                      && eArr.Type == 0 // Arrival type
                      && eDep.Time >= startTime // Проверяем только по времени отправления
                      && eDep.Time <= endTime
                      && eArr.EventOrder == eDep.EventOrder - 1 // Учитываем только прибытие перед отправлением
                orderby eDep.Number, eDep.Time
                select new Wagon
                {
                    InventoryNumber = eArr.Number, // Используем номер вагона
                    ArrivalTime = eArr.Time.ToString("yyyy-MM-dd HH:mm:ss"), // Время прибытия (до отправления)
                    DepartureTime = eDep.Time.ToString("yyyy-MM-dd HH:mm:ss") // Время отправления
                }).ToList();

// Возвращаем результат
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
