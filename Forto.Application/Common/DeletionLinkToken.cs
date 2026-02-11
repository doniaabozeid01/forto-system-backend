using System.Security.Cryptography;
using System.Text;

namespace Forto.Application.Common;

/// <summary>توقيع وتحقق من روابط الموافقة/الرفض على حذف الفاتورة.</summary>
public static class DeletionLinkToken
{
    private const int ExpiryHours = 24;

    private static string Base64UrlEncode(byte[] bytes)
    {
        return Convert.ToBase64String(bytes).Replace("+", "-").Replace("/", "_").TrimEnd('=');
    }

    public static string Generate(int invoiceId, string action, string secret)
    {
        if (string.IsNullOrEmpty(secret)) return "";
        var expiry = DateTimeOffset.UtcNow.AddHours(ExpiryHours).ToUnixTimeSeconds();
        var payload = $"{invoiceId}:{action}:{expiry}";
        var key = Encoding.UTF8.GetBytes(secret);
        var data = Encoding.UTF8.GetBytes(payload);
        using var hmac = new HMACSHA256(key);
        var hash = hmac.ComputeHash(data);
        return expiry + "." + Base64UrlEncode(hash);
    }

    public static bool Validate(int invoiceId, string action, string token, string secret)
    {
        if (string.IsNullOrEmpty(secret) || string.IsNullOrEmpty(token)) return false;
        var parts = token.Split('.');
        if (parts.Length != 2) return false;
        try
        {
            if (!long.TryParse(parts[0], out var expirySec)) return false;
            if (DateTimeOffset.UtcNow.ToUnixTimeSeconds() > expirySec) return false;
            var payload = $"{invoiceId}:{action}:{expirySec}";
            var key = Encoding.UTF8.GetBytes(secret);
            var data = Encoding.UTF8.GetBytes(payload);
            using var hmac = new HMACSHA256(key);
            var expectedHash = Base64UrlEncode(hmac.ComputeHash(data));
            return string.Equals(expectedHash, parts[1], StringComparison.Ordinal);
        }
        catch { return false; }
    }
}
