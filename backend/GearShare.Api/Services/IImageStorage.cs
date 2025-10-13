public interface IImageStorage
{
    Task<(string fileName, string relativePath)> SaveItemImageAsync(Guid itemId, IFormFile file, CancellationToken ct);
}
