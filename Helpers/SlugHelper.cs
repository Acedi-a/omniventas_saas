using System.Text.RegularExpressions;

namespace SaaSEventos.Helpers;

public static class SlugHelper
{
    public static string Generate(string name)
    {
        var slug = name.ToLowerInvariant().Trim();
        slug = Regex.Replace(slug, "[^a-z0-9]+", "-");
        slug = slug.Trim('-');
        return string.IsNullOrWhiteSpace(slug) ? "tenant" : slug;
    }
}
