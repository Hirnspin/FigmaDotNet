using System.Globalization;
using System.Text.RegularExpressions;

namespace FigmaDotNet;

public static class StringHelper
{
    public static string ToPascalCase(this string input)
    {
        TextInfo textInfo = CultureInfo.CurrentCulture.TextInfo;
        return textInfo.ToTitleCase(input).Replace(" ", string.Empty);
    }

    public static string ToKebabCase(this string value)
    {
        // Replace all non-alphanumeric characters with a dash
        value = Regex.Replace(value, @"[^0-9a-zA-Z]", "-");

        // Replace all subsequent dashes with a single dash
        value = Regex.Replace(value, @"[-]{2,}", "-");

        // Remove any trailing dashes
        value = Regex.Replace(value, @"-+$", string.Empty);

        // Remove any dashes in position zero
        if (value.StartsWith("-")) value = value.Substring(1);

        // Lowercase and return
        return value.ToLower();
    }
}
