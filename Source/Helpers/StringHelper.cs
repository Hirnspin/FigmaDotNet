using System.Globalization;

namespace FigmaDotNet;

public static class StringHelper
{
    public static string ConvertToPascalCase(string input)
    {
        TextInfo textInfo = CultureInfo.CurrentCulture.TextInfo;
        return textInfo.ToTitleCase(input).Replace(" ", string.Empty);
    }
}
