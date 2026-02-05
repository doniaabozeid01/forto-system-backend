namespace Forto.Application.DTOs.Billings
{
    /// <summary>تعيين المجموع قبل الضريبة بعد تعديل الكاشير (قبل إنهاء الخدمة أو قبل الدفع). الـ Total يُحسب: AdjustedTotal + (AdjustedTotal × 14%) - الخصم.</summary>
    public class SetAdjustedTotalRequest
    {
        public decimal AdjustedTotal { get; set; }
    }
}
