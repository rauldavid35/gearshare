using Microsoft.AspNetCore.Hosting;

namespace GearShare.Api.Services
{
    public class LocalImageStorage : IImageStorage
    {
        private readonly IWebHostEnvironment _env;

        public LocalImageStorage(IWebHostEnvironment env)
        {
            _env = env;
        }

        public async Task<(string relativePath, string fileName)> SaveItemImageAsync(Guid itemId, IFormFile file)
        {
            var uploadsDir = Path.Combine(_env.WebRootPath, "uploads", "items", itemId.ToString());
            Directory.CreateDirectory(uploadsDir);

            var fileName = $"{Guid.NewGuid():N}{Path.GetExtension(file.FileName)}";
            var fullPath = Path.Combine(uploadsDir, fileName);

            using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // store with leading slash to work with ToAbsoluteContentUrl
            var relative = $"/uploads/items/{itemId}/{fileName}";
            return (relative, fileName);
        }

        public Task DeleteAsync(string relativePath)
        {
            if (string.IsNullOrWhiteSpace(relativePath))
                return Task.CompletedTask;

            // Accepts "/uploads/items/..." or "uploads/items/..."
            var rel = relativePath.TrimStart('/', '\\');
            var full = Path.Combine(_env.WebRootPath, rel);

            try
            {
                if (File.Exists(full))
                    File.Delete(full);
            }
            catch
            {
                // optional: log
            }

            return Task.CompletedTask;
        }
    }
}
