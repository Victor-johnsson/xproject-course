using Azure.Security.KeyVault.Secrets;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace XProjectIntegrationsBackend.Services;

public interface IImageService
{
    Task<string> UploadImageAsync(string base64Image, string fileName);
}

public class ImageService : IImageService
{
    private readonly ILogger<ImageService> _logger;

    private readonly string _containerName = "blobs";

    private readonly BlobServiceClient _blobServiceClient;

    public ImageService(ILogger<ImageService> logger, BlobServiceClient blobServiceClient)
    {
        _logger = logger;
        _blobServiceClient = blobServiceClient;
    }

    public async Task<string> UploadImageAsync(string base64Image, string fileName)
    {
        try
        {
            string base64Data = base64Image;
            if (base64Image.Contains(','))
            {
                base64Data = base64Image[(base64Image.IndexOf(",") + 1)..];
            }

            byte[] imageBytes = Convert.FromBase64String(base64Data);

            string extension = DetermineFileExtension(base64Image);

            string blobName = $"{fileName}_{DateTime.Now:yyyyMMddHHmmssffff}{extension}";

            return await UploadToBlobStorageAsync(imageBytes, blobName, extension);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading base64 image to storage");
            throw;
        }
    }

    private static string DetermineFileExtension(string base64Image)
    {
        string extension = ".jpg";

        if (base64Image.Contains("data:image/"))
        {
            string mimeType = base64Image.Split(',')[0].Split(':')[1].Split(';')[0];
            extension = mimeType switch
            {
                "image/png" => ".png",
                "image/jpeg" => ".jpg",
                "image/gif" => ".gif",
                _ => ".jpg",
            };
        }

        return extension;
    }

    private async Task<string> UploadToBlobStorageAsync(
        byte[] imageBytes,
        string blobName,
        string extension
    )
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
        var blobClient = containerClient.GetBlobClient(blobName);

        using (var stream = new MemoryStream(imageBytes))
        {
            await blobClient.UploadAsync(
                stream,
                new BlobHttpHeaders
                {
                    ContentType = extension switch
                    {
                        ".png" => "image/png",
                        ".jpg" or ".jpeg" => "image/jpeg",
                        ".gif" => "image/gif",
                        _ => "application/octet-stream",
                    },
                }
            );
        }

        return blobClient.Uri.ToString();
    }
}
