using ITI.Resturant.Management.Application.ExternalServices.Contracts;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace ITI.Resturant.Management.MVC.TagHelpers
{
    [HtmlTargetElement("image-resolver")]
    public class ImageResolverTagHelper : TagHelper
    {
        private readonly IImageUrlResolver _resolver;

        public ImageResolverTagHelper(IImageUrlResolver resolver)
        {
            _resolver = resolver;
        }

        [HtmlAttributeName("src")]
        public string? Src { get; set; }

        [HtmlAttributeName("alt")]
        public string? Alt { get; set; }

        [HtmlAttributeName("class")]
        public string? Class { get; set; }

        [HtmlAttributeName("width")]
        public string? Width { get; set; }

        [HtmlAttributeName("height")]
        public string? Height { get; set; }

        [HtmlAttributeName("style")]
        public string? Style { get; set; }

        [HtmlAttributeName("loading")]
        public string? Loading { get; set; }

        public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            output.TagName = "img";
            output.TagMode = TagMode.SelfClosing;

            var resolved = string.IsNullOrWhiteSpace(Src) ? string.Empty : _resolver.Resolve(Src);

            // If no src resolved, fallback to placeholder
            if (string.IsNullOrWhiteSpace(resolved))
            {
                resolved = "/images/placeholder-food.png";
            }

            output.Attributes.SetAttribute("src", resolved);

            if (!string.IsNullOrEmpty(Alt))
                output.Attributes.SetAttribute("alt", Alt);

            if (!string.IsNullOrEmpty(Class))
                output.Attributes.SetAttribute("class", Class);

            if (!string.IsNullOrEmpty(Width))
                output.Attributes.SetAttribute("width", Width);

            if (!string.IsNullOrEmpty(Height))
                output.Attributes.SetAttribute("height", Height);

            if (!string.IsNullOrEmpty(Style))
                output.Attributes.SetAttribute("style", Style);

            if (!string.IsNullOrEmpty(Loading))
                output.Attributes.SetAttribute("loading", Loading);

            return Task.CompletedTask;
        }
    }
}
