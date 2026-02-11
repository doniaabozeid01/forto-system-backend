using System.Net;
using System.Net.Mail;
using Forto.Application.Abstractions.Services.Email;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Forto.Infrastructure.Services;

public class EmailSettings
{
    public const string SectionName = "Email";
    /// <summary>إيميل الأدمن اللي بيوصله طلبات حذف الفواتير.</summary>
    public string AdminEmail { get; set; } = "doniaabozeid16@gmail.com";
    public string? SmtpHost { get; set; }
    public int SmtpPort { get; set; } = 587;
    public bool SmtpEnableSsl { get; set; } = true;
    public string? SmtpUser { get; set; }
    public string? SmtpPassword { get; set; }
    /// <summary>عنوان المرسل (From).</summary>
    public string? FromEmail { get; set; }
    /// <summary>اسم المرسل الظاهر (مثل FORTO CAR CLEAN CENTER).</summary>
    public string? FromDisplayName { get; set; }
}

public class EmailSender : IEmailSender
{
    private readonly EmailSettings _settings;
    private readonly ILogger<EmailSender> _logger;

    public EmailSender(IOptions<EmailSettings> options, ILogger<EmailSender> logger)
    {
        _settings = options?.Value ?? new EmailSettings();
        _logger = logger;
    }

    public Task SendToAdminAsync(string subject, string body, CancellationToken cancellationToken = default)
        => SendAsync(_settings.AdminEmail, subject, body, false, cancellationToken);

    public Task SendHtmlToAdminAsync(string subject, string htmlBody, CancellationToken cancellationToken = default)
        => SendAsync(_settings.AdminEmail, subject, htmlBody, true, cancellationToken);

    public async Task SendAsync(string toEmail, string subject, string body, CancellationToken cancellationToken = default)
        => await SendAsync(toEmail, subject, body, false, cancellationToken);

    private async Task SendAsync(string toEmail, string subject, string body, bool isHtml, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_settings.SmtpHost))
        {
            _logger.LogWarning("Email not sent (SmtpHost not configured). To: {To}, Subject: {Subject}", toEmail, subject);
            return;
        }

        try
        {
            using var client = new SmtpClient(_settings.SmtpHost, _settings.SmtpPort)
            {
                EnableSsl = _settings.SmtpEnableSsl,
                Credentials = string.IsNullOrEmpty(_settings.SmtpUser)
                    ? null
                    : new NetworkCredential(_settings.SmtpUser, _settings.SmtpPassword)
            };
            var fromEmail = string.IsNullOrWhiteSpace(_settings.FromEmail) ? "noreply@forto.com" : _settings.FromEmail;
            var from = string.IsNullOrWhiteSpace(_settings.FromDisplayName)
                ? new MailAddress(fromEmail)
                : new MailAddress(fromEmail, _settings.FromDisplayName);
            var to = new MailAddress(toEmail);
            var mail = new MailMessage(from, to)
            {
                Subject = subject,
                Body = body,
                IsBodyHtml = isHtml
            };
            await client.SendMailAsync(mail, cancellationToken);
            _logger.LogInformation("Email sent to {To}, Subject: {Subject}", toEmail, subject);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {To}, Subject: {Subject}", toEmail, subject);
            throw;
        }
    }
}
