namespace backend.src.Services;

public interface IFileStorageService
{
    Task<string> UploadImageAsync(IFormFile file, string folder, CancellationToken cancellationToken);
}
