using Azure.Storage.Files.Shares;
using Azure.Storage.Files.Shares.Models;
using RetailappPOE.Models;

namespace RetailappPOE.Services
{
    
    public class FilesService
    {
        private readonly ShareClient _shareClient;

        public FilesService(string connectionString, string shareName = "fileshare")
        {
            _shareClient = new ShareClient(connectionString, shareName);
            _shareClient.CreateIfNotExists();
        }
        // List files in the root directory
        public async Task<List<FilesModel>> GetFilesAsync()
        {
            var files = new List<FilesModel>();
            var rootDir = _shareClient.GetRootDirectoryClient();

            await foreach (ShareFileItem item in rootDir.GetFilesAndDirectoriesAsync())
            {
                if (!item.IsDirectory)
                {
                    var fileClient = rootDir.GetFileClient(item.Name);
                    var properties = await fileClient.GetPropertiesAsync();

                    files.Add(new FilesModel
                    {
                        Name = item.Name,
                        Size = properties.Value.ContentLength,
                        LastModified = properties.Value.LastModified
                    });
                }
            }

            return files;
        }

        // Upload file
        public async Task UploadFileAsync(IFormFile file)
        {
            var rootDir = _shareClient.GetRootDirectoryClient();
            var fileClient = rootDir.GetFileClient(file.FileName);

            await using var stream = file.OpenReadStream();
            await fileClient.CreateAsync(file.Length);
            await fileClient.UploadRangeAsync(
                new Azure.HttpRange(0, file.Length),
                stream
            );
        }

        // Delete file
        public async Task DeleteFileAsync(string fileName)
        {
            var rootDir = _shareClient.GetRootDirectoryClient();
            var fileClient = rootDir.GetFileClient(fileName);
            await fileClient.DeleteIfExistsAsync();
        }

        // Download file
        public async Task<Stream?> DownloadFileAsync(string fileName)
        {
            var rootDir = _shareClient.GetRootDirectoryClient();
            var fileClient = rootDir.GetFileClient(fileName);

            if (await fileClient.ExistsAsync())
            {
                var download = await fileClient.DownloadAsync();
                return download.Value.Content;
            }

            return null;
        }
    }
}
