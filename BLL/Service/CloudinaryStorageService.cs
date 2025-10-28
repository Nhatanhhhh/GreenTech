using BLL.Config;
using BLL.Service.Interface;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BLL.Service
{
    /// <summary>
    /// Service implementation for interacting with the Cloudinary file storage.
    /// </summary>
    public class CloudinaryStorageService : IFileStorageService
    {
        private readonly Cloudinary _cloudinary;
        private readonly ILogger<CloudinaryStorageService> _logger;

        /// <summary>
        /// Initializes a new instance of the<see cref = "CloudinaryStorageService" /> class.
        /// </summary>
        /// <param name = "cloudinarySettings" > The Cloudinary configuration settings, injected via IOptions.</param>
        /// <param name = "logger" > The logger for logging information and errors.</param>
        public CloudinaryStorageService(IOptions<CloudinarySettings> cloudinarySettings, ILogger<CloudinaryStorageService> logger)
        {
            _logger = logger;
            var settings = cloudinarySettings.Value;

            // Validate settings
            if (string.IsNullOrEmpty(settings.CloudName) ||
                string.IsNullOrEmpty(settings.ApiKey) ||
                string.IsNullOrEmpty(settings.ApiSecret))
            {
                var errorMessage = "Cloudinary settings are invalid or missing. Please check your configuration.";
                _logger.LogError(errorMessage);
                throw new ArgumentException(errorMessage);
            }

            // Initialize Cloudinary account
            var account = new Account(
                settings.CloudName,
                settings.ApiKey,
                settings.ApiSecret
            );

            _cloudinary = new Cloudinary(account);
        }

        /// <inheritdoc />
        public async Task<string> SaveFileAsync(IFormFile file, string subFolder = "")
        {
            if (file == null || file.Length == 0)
            {
                throw new ArgumentException("File cannot be null or empty.", nameof(file));
            }

            // Using statement ensures the stream is properly disposed
            await using var stream = file.OpenReadStream();
            return await SaveFileAsync(stream, file.FileName, file.ContentType, subFolder);
        }

        /// <inheritdoc />
        public async Task<string> SaveFileAsync(Stream fileStream, string fileName, string contentType, string subFolder = "")
        {
            if (fileStream == null)
            {
                throw new ArgumentNullException(nameof(fileStream), "File stream cannot be null.");
            }
            if (string.IsNullOrWhiteSpace(fileName))
            {
                throw new ArgumentException("File name cannot be empty.", nameof(fileName));
            }

            // Generate a unique file name to avoid conflicts, but keep the original extension
            var uniqueFileName = $"{Guid.NewGuid()}{Path.GetExtension(fileName)}";

            // Combine subfolder with the unique file name to create the PublicId
            var publicId = string.IsNullOrWhiteSpace(subFolder)
                ? uniqueFileName
                : $"{subFolder.Trim('/')}/{uniqueFileName}";

            // Use RawUploadParams for flexibility with any file type (images, videos, pdfs, etc.)
            var uploadParams = new RawUploadParams()
            {
                File = new FileDescription(fileName, fileStream),
                PublicId = publicId,
                Overwrite = true, // Overwrite if a file with the same PublicId exists (unlikely with GUID)
            };

            try
            {
                _logger.LogInformation("Uploading file to Cloudinary with PublicId: {PublicId}", publicId);
                var uploadResult = await _cloudinary.UploadAsync(uploadParams);

                if (uploadResult.Error != null)
                {
                    _logger.LogError("Cloudinary upload failed: {Error}", uploadResult.Error.Message);
                    throw new Exception($"Failed to upload file. Cloudinary error: {uploadResult.Error.Message}");
                }

                _logger.LogInformation("File uploaded successfully. URL: {Url}", uploadResult.SecureUrl.ToString());
                return uploadResult.SecureUrl.ToString();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred during file upload.");
                throw; // Re-throw the exception to be handled by higher-level code
            }
        }

        /// <inheritdoc />
        public async Task DeleteFileAsync(string fileUrl)
        {
            if (string.IsNullOrWhiteSpace(fileUrl))
            {
                _logger.LogWarning("DeleteFileAsync called with an empty or null URL.");
                return; // Nothing to do
            }

            try
            {
                // Extract the Public ID from the URL.
                // Example URL: https://res.cloudinary.com/cloud_name/resource_type/upload/v12345/sub_folder/guid.jpg
                // Public ID needed: sub_folder/guid
                var publicId = GetPublicIdFromUrl(fileUrl);
                if (string.IsNullOrEmpty(publicId))
                {
                    _logger.LogWarning("Could not extract Public ID from URL: {FileUrl}", fileUrl);
                    return;
                }

                var deletionParams = new DeletionParams(publicId)
                {
                    // By default, Cloudinary determines resource type. 
                    // If you store everything as 'raw', you can specify it:
                    // ResourceType = ResourceType.Raw 
                };

                _logger.LogInformation("Deleting file from Cloudinary with PublicId: {PublicId}", publicId);
                var deletionResult = await _cloudinary.DestroyAsync(deletionParams);

                // "ok" means success, "not found" is also acceptable (already deleted)
                if (deletionResult.Result.ToLower() == "ok")
                {
                    _logger.LogInformation("File deleted successfully.");
                }
                else
                {
                    _logger.LogWarning("Cloudinary deletion result for PublicId '{PublicId}': {Result}", publicId, deletionResult.Result);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while trying to delete file: {FileUrl}", fileUrl);
                // Do not re-throw, as per interface documentation (handle gracefully)
            }
        }

        private string GetPublicIdFromUrl(string url)
        {
            try
            {
                var uri = new Uri(url);
                // The Public ID is the part of the path after the version number and before the file extension.
                // e.g. /image/upload/v1678886344/avatars/c3a0b1.png -> avatars/c3a0b1
                // The segments are ["/", "image", "upload", "v123...", "avatars", "c3a0b1.png"]
                // We need to find the segment starting with 'v' and take everything after it.

                var pathSegments = uri.AbsolutePath.Split('/');
                int versionSegmentIndex = -1;
                for (int i = 0; i < pathSegments.Length; i++)
                {
                    if (pathSegments[i].StartsWith("v") && int.TryParse(pathSegments[i].Substring(1), out _))
                    {
                        versionSegmentIndex = i;
                        break;
                    }
                }

                if (versionSegmentIndex == -1 || versionSegmentIndex + 1 >= pathSegments.Length)
                {
                    return null;
                }

                // Join the segments after the version segment
                var fullPath = string.Join("/", pathSegments, versionSegmentIndex + 1, pathSegments.Length - (versionSegmentIndex + 1));

                // Remove the extension
                var publicId = Path.ChangeExtension(fullPath, null);

                return publicId;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to parse Public ID from URL: {Url}", url);
                return null;
            }
        }
    }
}
