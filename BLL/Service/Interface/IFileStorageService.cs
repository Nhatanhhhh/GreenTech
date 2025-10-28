using Microsoft.AspNetCore.Http;

namespace BLL.Service.Interface
{
    /// <summary>
    /// Interface for file storage operations (e.g., uploading, deleting).
    /// </summary>
    public interface IFileStorageService
    {
        /// <summary>
        /// Saves a file stream to the configured storage.
        /// </summary>
        /// <param name="fileStream">The stream containing the file data.</param>
        /// <param name="fileName">The original name of the file (used for extension and potential naming hints).</param>
        /// <param name="contentType">The MIME type of the file (optional, might be used by some storage providers).</param>
        /// <param name="subFolder">An optional subfolder path within the storage container (e.g., "products", "avatars").</param>
        /// <returns>A task representing the asynchronous operation, containing the publicly accessible URL of the saved file.</returns>
        /// <exception cref="ArgumentNullException">Thrown if fileStream or fileName is null.</exception>
        /// <exception cref="ArgumentException">Thrown if fileName is empty or invalid.</exception>
        /// <exception cref="Exception">Thrown if the upload operation fails.</exception>
        Task<string> SaveFileAsync(Stream fileStream, string fileName, string contentType, string subFolder = ""); // subFolder mặc định là "" (root)

        /// <summary>
        /// Saves an IFormFile to the configured storage. Convenience method.
        /// </summary>
        /// <param name="file">The IFormFile received from the HTTP request.</param>
        /// <param name="subFolder">An optional subfolder path within the storage container.</param>
        /// <returns>A task representing the asynchronous operation, containing the publicly accessible URL of the saved file.</returns>
        /// <exception cref="ArgumentNullException">Thrown if file is null.</exception>
        /// <exception cref="ArgumentException">Thrown if file is empty.</exception>
        /// <exception cref="Exception">Thrown if the upload operation fails.</exception>
        Task<string> SaveFileAsync(IFormFile file, string subFolder = ""); // subFolder mặc định là "" (root)


        /// <summary>
        /// Deletes a file from the configured storage based on its URL.
        /// </summary>
        /// <param name="fileUrl">The publicly accessible URL of the file to delete (obtained from SaveFileAsync).</param>
        /// <returns>A task representing the asynchronous delete operation.</returns>
        /// <remarks>
        /// Implementations should handle cases where the file doesn't exist gracefully (e.g., log a warning but don't throw an error).
        /// </remarks>
        Task DeleteFileAsync(string fileUrl);
    }
}
