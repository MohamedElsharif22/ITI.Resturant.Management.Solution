using ITI.Resturant.Management.Application.ExternalServices.Contracts;
using Microsoft.Extensions.Configuration;

namespace ITI.Resturant.Management.Infrastructure.Services
{
    public class ImageUrlResolver : IImageUrlResolver
    {
        private readonly string _baseUrl;

        public ImageUrlResolver(IConfiguration configuration)
        {
            // read BaseUrl from appsettings. If not configured, default to empty (relative paths remain relative)
            _baseUrl = configuration["BaseUrl"]?.TrimEnd('/') ?? string.Empty;
        }

        public string Resolve(string? imageUrl)
        {
            if (string.IsNullOrWhiteSpace(imageUrl))
                return string.Empty;

            // If already absolute URL, return as-is
            if (imageUrl.StartsWith("http://") || imageUrl.StartsWith("https://"))
                return imageUrl;

            // If imageUrl starts with '/', prefix base url
            if (imageUrl.StartsWith("/"))
            {
                if (string.IsNullOrEmpty(_baseUrl))
                    return imageUrl; // leave relative

                return _baseUrl + imageUrl;
            }

            // otherwise treat as relative path
            if (string.IsNullOrEmpty(_baseUrl))
                return "/" + imageUrl;

            return _baseUrl + "/" + imageUrl;
        }
    }
}
