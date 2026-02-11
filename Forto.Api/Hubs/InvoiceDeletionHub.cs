using Microsoft.AspNetCore.SignalR;

namespace Forto.Api.Hubs;

/// <summary>الفرونت يتصل هنا ويستمع لـ DeletionProcessed عشان يحدّث القائمة بدون refresh.</summary>
/// <remarks>
/// الاتصال: baseUrl + "/hubs/invoice-deletion" (مثل https://localhost:7179/hubs/invoice-deletion).
/// الاستماع: connection.on("DeletionProcessed", (invoiceId: number, action: string) => { ... }) حيث action = "approved" أو "rejected".
/// </remarks>
public class InvoiceDeletionHub : Hub
{
    /// <summary>اسم الحدث اللي الباك يبعته: (invoiceId, action) حيث action = "approved" أو "rejected".</summary>
    public const string EventName = "DeletionProcessed";
}
