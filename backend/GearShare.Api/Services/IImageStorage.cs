using Microsoft.AspNetCore.Http;

namespace GearShare.Api.Services
{
    public interface IImageStorage
    {
        Task<(string relativePath, string fileName)> SaveItemImageAsync(Guid itemId, IFormFile file);

        // NEW: delete a file from wwwroot by relative path like "/uploads/items/{itemId}/img.jpg"
        Task DeleteAsync(string relativePath);
    }
}
