using hotel.DTOs.Rooms;

namespace hotel.Interfaces;

public interface IRoomNotificationService
{
    Task NotifyRoomStatusChanged(int roomId, string status, int? visitaId, int institutionId, string? userId = null);
    Task NotifyRoomProgressUpdated(int roomId, int visitaId, RoomProgressDto progress, int institutionId);
    Task NotifyRoomReservationChanged(int roomId, int? reservaId, int? visitaId, string action, int institutionId);
    Task NotifyRoomMaintenanceChanged(int roomId, string maintenanceType, string status, string? description, int institutionId);
}