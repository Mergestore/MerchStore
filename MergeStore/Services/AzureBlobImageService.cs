using Microsoft.Extensions.Options;
using MergeStore.Configurations;

namespace MergeStore.Storage
{
    public class AzureBlobImageService : IImageService
    {
        private readonly string _blobContainerUrl;

        public AzureBlobImageService(IOptions<AzureBlobOptions> options)
        {
            // Properly handle null values to avoid warnings
            if (options.Value.ContainerUrl == null)
            {
                throw new ArgumentNullException(nameof(options.Value.ContainerUrl), 
                    "Blob container URL is required");
            }
            
            _blobContainerUrl = options.Value.ContainerUrl;
        }

        public string GetImageUrl(string imageName)
        {
            if (string.IsNullOrEmpty(imageName))
            {
                throw new ArgumentException("Image name cannot be null or empty", nameof(imageName));
            }

            return $"{_blobContainerUrl}/{imageName}";
        }
    }
}