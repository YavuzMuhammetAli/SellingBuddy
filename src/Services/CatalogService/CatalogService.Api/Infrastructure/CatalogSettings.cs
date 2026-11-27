namespace CatalogService.Api.Infrastructure
{
    public class CatalogSettings
    {
        //[0] is replaced with the item id
        public string PicBaseUrl { get; set; }

        //category, seller and campaign images, file name is appended
        public string FileBaseUrl { get; set; }
    }
}
