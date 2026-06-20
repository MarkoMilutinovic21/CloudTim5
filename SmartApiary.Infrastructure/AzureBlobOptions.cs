namespace SmartApiary.Infrastructure;

public class AzureBlobOptions
{
    public string ConnectionString { get; set; } = "UseDevelopmentStorage=true";
    public string ApiaryImagesContainer { get; set; } = "apiary-images";
}
