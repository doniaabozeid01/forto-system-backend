namespace Forto.Application.DTOs.Billings
{
    /// <summary>استجابة قائمة الإكراميات مع المجموع في الفترة</summary>
    public class TipsListResponse
    {
        public IReadOnlyList<TipResponse> Items { get; set; } = new List<TipResponse>();
        /// <summary>مجموع مبالغ الإكراميات في الفترة (fromDate - toDate)</summary>
        public decimal Total { get; set; }
    }
}
