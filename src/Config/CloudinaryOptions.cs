namespace backend.src.Config;

public sealed class CloudinaryOptions
{
    public string CloudName { get; init; } = string.Empty;
    public string ApiKey { get; init; } = string.Empty;
    public string ApiSecret { get; init; } = string.Empty;
    public string UploadFolder { get; init; } = "labstore/products";
    public string LocalUploadRoot { get; init; } = "wwwroot/uploads";
}
