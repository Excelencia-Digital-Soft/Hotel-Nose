namespace hotel.DTOs.Rooms;

public class RoomProgressDto
{
    public DateTime StartTime { get; set; }
    public DateTime CurrentTime { get; set; }
    public double ProgressPercentage { get; set; }
    public string TimeElapsed { get; set; } = string.Empty;
    public DateTime? EstimatedEndTime { get; set; }
    public int TotalMinutes { get; set; }
    public int ElapsedMinutes { get; set; }
}

public class OccupiedRoomDto
{
    public int RoomId { get; set; }
    public int? VisitaId { get; set; }
    public DateTime? ReservationStartTime { get; set; }
    public int TotalMinutes { get; set; }
    public int InstitutionId { get; set; }
}