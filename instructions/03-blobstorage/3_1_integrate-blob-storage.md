# Exercise 04 – Integrating with Azure Blob Storage (Image Service)

## Objective

In this exercise, you will implement an **Image Service** for managing image uploads to
**Azure Blob Storage**.

This expands your integration portfolio with a **cloud storage resource** and introduces Azure SDK usage in a local Aspire context.

---

## 1. Background

Your eShop solution now supports product and payment integrations.
Next, you’ll add support for **storing product images** using **Azure Blob Storage**.

In enterprise Azure solutions, Blob Storage is commonly used for:
- File uploads
- Documents and attachments
- Image repositories

Your implementation will allow other services to upload image data
as Base64‑encoded content and get back a storage URL.

---

## 2. Provided Interface

An interface definition has been added to your backend project:

```csharp
public interface IImageService
{
    Task<string> UploadImageAsync(string base64Image, string fileName);
}
```

You will provide the implementation and make it available via dependency injection.

---

## 3. Setting up the Azure Blob Client

Your Aspire `AppHost` project and backend startup already include a service connection for Azure Storage:

```csharp
builder.AddAzureBlobServiceClient("blobs");
```

This automatically configures an authenticated `BlobServiceClient` using configuration defined
in Aspire or in your secrets.

You will inject this client into your `ImageService` class.

> **Note:** When running locally with Aspire, a *local Azurite container* may be used.
> When deployed to Azure, the same setup uses a real Azure Storage account.

---

## 4. Your Task – Implement the Service

Implement the `IImageService` functionality using the Azure SDK:
- Convert Base64 image data into a byte array
- Determine appropriate file extensions and MIME types
- Upload the content to a Blob container (named `"blobs"`)
- Return the **public URI** of the uploaded image

You should also log meaningful events and handle errors safely.

You can reference the Azure SDK documentation for `BlobServiceClient` and `BlobContainerClient` usage:
https://learn.microsoft.com/azure/storage/blobs/storage-quickstart-blobs-dotnet

---

## 5. Suggested Registration Pattern

Ensure your DI registration includes both:
```csharp
builder.AddAzureBlobServiceClient("blobs");

builder.Services.AddScoped<IImageService, ImageService>();
```

When the backend starts, Aspire configures `BlobServiceClient` automatically.

---

## 6. Exposing Upload Functionality

Add a new endpoint to your backend (you can use **Minimal API** or **Controller** style).

Example (Minimal API pattern):

```csharp
app.MapPost("/api/images", async (IImageService imageService, ImageUploadRequest request) =>
{
    var imageUrl = await imageService.UploadImageAsync(request.Base64, request.FileName);
    return Results.Ok(new { Url = imageUrl });
});
```

`ImageUploadRequest` might look like:

```csharp
public record ImageUploadRequest(string Base64, string FileName);
```

This gives consumers a way to upload images leveraging your storage integration.

---

## 7. Validation & Testing

1. Start your Aspire environment (`aspire run` or Visual Studio).
2. Use cURL or Postman to call your new upload endpoint:

```bash
POST http://localhost:{port}/api/images
Content-Type: application/json

{
  "base64": "<base64-encoded-image-data>",
  "fileName": "product123"
}
```

3. You should receive a JSON response with a URL pointing to the uploaded blob.
4. Inspect Aspire Dashboard or the storage emulator (Azurite) to confirm the file exists.

---

## 8. Checkpoints

✅ Interface is implemented and registered in DI.
✅ Image uploads succeed locally.
✅ Blob container `"blobs"` contains your image content.
✅ Error handling and logging are in place.
✅ Endpoint returns blob URLs that can be accessed through Aspire or Azure.

---

## 9. Discussion

This exercise highlights:
- Integrating Azure SDKs (`Azure.Storage.Blobs`)
- Handling binary data and MIME types
- Applying proper secret and connection handling via Aspire configuration
- Maintaining clean service abstraction (`IImageService`)

It also sets you up for the next exercise, where you’ll publish event messages to **Azure Service Bus**
about successful uploads or product changes.

