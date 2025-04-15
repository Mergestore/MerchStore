namespace MergeStore.Configurations
{
    public class AzureBlobOptions
    {
        public const string SectionName = "AzureBlob";
        
        public string? ConnectionString { get; set; }
        public string? ContainerName { get; set; }
        public string? ContainerUrl { get; set; }
    }
}