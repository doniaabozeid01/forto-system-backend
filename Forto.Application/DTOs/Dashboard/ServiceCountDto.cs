namespace Forto.Application.DTOs.Dashboard
{
    /// <summary>خدمة مع عدد مرات تنفيذها (مثلاً ضمن تقرير موظف).</summary>
    public class ServiceCountDto
    {
        public int ServiceId { get; set; }
        public string ServiceName { get; set; } = "";
        public int Count { get; set; }
    }
}
