using Microsoft.AspNetCore.Http;

namespace GearShare.Api.Utils
{
    public static class UrlExtensions
    {
        public static string? ToAbsoluteContentUrl(this HttpRequest req, string? path)
        {
            if (string.IsNullOrWhiteSpace(path)) return path;

            var p = path.Replace("\\", "/").Trim();
            if (p.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                p.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                return p;

            if (!p.StartsWith("/")) p = "/" + p;
            var origin = $"{req.Scheme}://{req.Host.Value}".TrimEnd('/');
            return origin + p;
        }
    }
}
