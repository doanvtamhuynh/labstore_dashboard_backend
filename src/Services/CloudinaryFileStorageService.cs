using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using backend.src.Config;
using Microsoft.Extensions.Options;

namespace backend.src.Services;

public sealed class CloudinaryFileStorageService : IFileStorageService
{
    private static readonly HashSet<string> AllowedContentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "image/jpeg",
        "image/png",
        "image/webp",
        "image/gif"
    };

    private readonly CloudinaryOptions _options;
    private readonly IWebHostEnvironment _environment;
    private readonly IHttpClientFactory _httpClientFactory;

    public CloudinaryFileStorageService(IOptions<CloudinaryOptions> options, IWebHostEnvironment environment, IHttpClientFactory httpClientFactory)
    {
        _options = options.Value;
        _environment = environment;
        _httpClientFactory = httpClientFactory;
    }

    public async Task<string> UploadImageAsync(IFormFile file, string folder, CancellationToken cancellationToken)
    {
        ValidateFile(file);
        if (HasCloudinaryCredentials())
        {
            return await UploadToCloudinaryAsync(file, folder, cancellationToken);
        }

        return await SaveLocalAsync(file, folder, cancellationToken);
    }

    private bool HasCloudinaryCredentials()
    {
        return !string.IsNullOrWhiteSpace(_options.CloudName)
            && !string.IsNullOrWhiteSpace(_options.ApiKey)
            && !string.IsNullOrWhiteSpace(_options.ApiSecret);
    }

    private async Task<string> UploadToCloudinaryAsync(IFormFile file, string folder, CancellationToken cancellationToken)
    {
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();
        var uploadFolder = string.IsNullOrWhiteSpace(folder) ? _options.UploadFolder : folder;
        var signature = SignCloudinaryParameters(new SortedDictionary<string, string>
        {
            ["folder"] = uploadFolder,
            ["timestamp"] = timestamp
        });

        using var content = new MultipartFormDataContent();
        content.Add(new StringContent(_options.ApiKey), "api_key");
        content.Add(new StringContent(timestamp), "timestamp");
        content.Add(new StringContent(uploadFolder), "folder");
        content.Add(new StringContent(signature), "signature");

        await using var stream = file.OpenReadStream();
        using var fileContent = new StreamContent(stream);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);
        content.Add(fileContent, "file", file.FileName);

        var client = _httpClientFactory.CreateClient();
        var response = await client.PostAsync($"https://api.cloudinary.com/v1_1/{_options.CloudName}/image/upload", content, cancellationToken);
        var json = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException($"Cloudinary upload failed: {json}");
        }

        using var document = JsonDocument.Parse(json);
        return document.RootElement.GetProperty("secure_url").GetString()
            ?? throw new InvalidOperationException("Cloudinary response did not include secure_url");
    }

    private async Task<string> SaveLocalAsync(IFormFile file, string folder, CancellationToken cancellationToken)
    {
        var safeFolder = string.Join('/', (string.IsNullOrWhiteSpace(folder) ? "products" : folder).Split('/', '\\').Where(part => !string.IsNullOrWhiteSpace(part)).Select(SanitizeSegment));
        var root = Path.Combine(_environment.ContentRootPath, _options.LocalUploadRoot);
        var directory = Path.Combine(root, safeFolder.Replace('/', Path.DirectorySeparatorChar));
        Directory.CreateDirectory(directory);

        var extension = Path.GetExtension(file.FileName);
        var fileName = $"{Guid.NewGuid():N}{extension}";
        var path = Path.Combine(directory, fileName);
        await using (var output = File.Create(path))
        {
            await file.CopyToAsync(output, cancellationToken);
        }

        return $"/uploads/{safeFolder}/{fileName}";
    }

    private string SignCloudinaryParameters(SortedDictionary<string, string> parameters)
    {
        var payload = string.Join('&', parameters.Select(pair => $"{pair.Key}={pair.Value}")) + _options.ApiSecret;
        var hash = SHA1.HashData(Encoding.UTF8.GetBytes(payload));
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    private static void ValidateFile(IFormFile file)
    {
        if (file.Length <= 0)
        {
            throw new InvalidOperationException("Image file is empty");
        }

        if (file.Length > 10 * 1024 * 1024)
        {
            throw new InvalidOperationException("Image file must be 10MB or smaller");
        }

        if (!AllowedContentTypes.Contains(file.ContentType))
        {
            throw new InvalidOperationException("Only JPG, PNG, WEBP, and GIF images are supported");
        }
    }

    private static string SanitizeSegment(string value)
    {
        var chars = value.Trim().Select(ch => char.IsLetterOrDigit(ch) || ch is '-' or '_' ? ch : '-').ToArray();
        return new string(chars);
    }
}
