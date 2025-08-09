using System.ComponentModel.DataAnnotations;

namespace hotel.DTOs.Caracteristicas
{
    /// <summary>
    /// DTO for assigning caracteristicas to a room
    /// </summary>
    public class RoomCaracteristicaAssignmentDto
    {
        /// <summary>
        /// List of caracteristica IDs to assign to the room
        /// </summary>
        [Required(ErrorMessage = "La lista de características es obligatoria")]
        public IEnumerable<int> CaracteristicaIds { get; set; } = new List<int>();
    }
}