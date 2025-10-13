using Microsoft.AspNetCore.Hosting;

namespace GearShare.Api.Services
{
    public class LocalImageStorage : IImageStorage
    {
        private readonly IWebHostEnvironment _env;
        public LocalImageStorage(IWebHostEnvironment env) => _env = env;

        public async Task<(string fileName, string relativePath)> SaveItemImageAsync(
            Guid itemId,
            IFormFile file,
            CancellationToken ct)
        {
            // Resolve wwwroot even if WebRootPath is null
            var webRoot = _env.WebRootPath ?? Path.Combine(_env.ContentRootPath, "wwwroot");

            // Keep the same folder convention consistently (hyphenated GUID)
            var itemFolder = Path.Combine(webRoot, "uploads", "items", itemId.ToString()); // e.g. aaaaa-bbbb-...
            Directory.CreateDirectory(itemFolder);

            var ext = Path.GetExtension(file.FileName);
            var fileName = $"{Guid.NewGuid():N}{ext}";
            var fullPath = Path.Combine(itemFolder, fileName);

            await using (var fs = new FileStream(fullPath, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                await file.CopyToAsync(fs, ct);
            }

            // Return a URL-like relative path the browser can use via StaticFiles
            // Leading "/" + forward slashes
            var rel = $"/uploads/items/{itemId}/{fileName}".Replace("\\", "/");
            return (fileName, rel);
        }
    }
}
