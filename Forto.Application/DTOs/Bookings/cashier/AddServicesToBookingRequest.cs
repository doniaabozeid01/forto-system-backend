namespace Forto.Application.DTOs.Bookings.cashier
{
    /// <summary>
    /// Add multiple services to an existing booking in one call.
    /// Avoids race conditions when adding several services at once.
    /// </summary>
    public class AddServicesToBookingRequest
    {
        public int CashierId { get; set; }
        public List<int> ServiceIds { get; set; } = new();
        /// <summary>
        /// Required when booking is InProgress. Applies to all added services if set.
        /// </summary>
        public int? AssignedEmployeeId { get; set; }
    }
}
