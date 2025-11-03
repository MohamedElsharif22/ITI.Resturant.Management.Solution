namespace ITI.Resturant.Management.Application.ExternalServices.Contracts
{
    public interface IImageUrlResolver
    {
        /// <summary>
        /// Resolve an image url stored in the database into a fully-qualified URL for use in views and client code.
        /// If the given value is absolute (http/https) it's returned as-is. If it's relative (starts with '/') the configured BaseUrl is prefixed.
        /// </summary>
        string Resolve(string? imageUrl);
    }
}
