namespace Forto.Application.Common;

/// <summary>إعدادات روابط الموافقة/الرفض في إيميل طلب حذف الفاتورة.</summary>
public class InvoiceDeletionLinkSettings
{
    public const string SectionName = "InvoiceDeletion";
    /// <summary>الرابط الأساسي للـ API (مثل https://localhost:7179) لبناء روابط الموافقة/الرفض.</summary>
    public string BaseUrl { get; set; } = "https://localhost:7179";
    /// <summary>سر لتوقيع الرابط (يُستخدم مع HMAC).</summary>
    public string Secret { get; set; } = "";
}
