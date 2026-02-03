using System.Collections.Generic;

namespace Forto.Application.DTOs.Billings.Gifts
{
    /// <summary>الهدايا المتاحة بناءً على قائمة خدمات (serviceIds) — مع مخزون الفرع لو branchId مُمرّر.</summary>
    public class GiftOptionsByServicesResponse
    {
        public List<int> ServiceIds { get; set; } = new();
        public int? BranchId { get; set; }
        public List<GiftOptionDto> Options { get; set; } = new();
    }
}
