namespace Forto.Application.DTOs.Billings;

/// <summary>طلب من الكاشير حذف فاتورة — لازم يكتب السبب.</summary>
public class RequestInvoiceDeletionRequest
{
    /// <summary>سبب طلب الحذف (إجباري).</summary>
    public string Reason { get; set; } = "";
    /// <summary>معرف موظف الكاشير اللي بيطلب الحذف.</summary>
    public int CashierEmployeeId { get; set; }
}
