namespace Forto.Application.DTOs.Billings
{
    public class TipResponse
    {
        public int Id { get; set; }
        public decimal Amount { get; set; }
        public DateOnly TipsDate { get; set; }
        public int? CashierId { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
