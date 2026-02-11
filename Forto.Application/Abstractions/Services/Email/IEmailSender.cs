namespace Forto.Application.Abstractions.Services.Email;

/// <summary>إرسال بريد إلكتروني (مثلاً تنبيهات للأدمن).</summary>
public interface IEmailSender
{
    Task SendAsync(string toEmail, string subject, string body, CancellationToken cancellationToken = default);
    /// <summary>إرسال للأدمن (الإيميل من الإعدادات).</summary>
    Task SendToAdminAsync(string subject, string body, CancellationToken cancellationToken = default);
    /// <summary>إرسال HTML للأدمن.</summary>
    Task SendHtmlToAdminAsync(string subject, string htmlBody, CancellationToken cancellationToken = default);
}
