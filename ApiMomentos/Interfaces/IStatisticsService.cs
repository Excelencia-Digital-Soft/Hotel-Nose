using ApiObjetos.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ApiObjetos.Interfaces
{
    public interface IStatisticsService
    {
        Task<List<RoomRankingDto>> GetRoomRankingAsync(DateRangeDto dateRange);
        Task<List<RoomRevenueDto>> GetRoomRevenueAsync(DateRangeDto dateRange);
        Task<List<CategoryOccupancyDto>> GetCategoryOccupancyAsync(DateRangeDto dateRange);
        Task<List<RoomConsumptionDto>> GetRoomConsumptionAsync(DateRangeDto dateRange);
    }
}