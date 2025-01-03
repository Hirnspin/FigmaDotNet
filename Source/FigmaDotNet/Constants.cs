namespace FigmaDotNet;

/// <summary>
/// Configuration keys and fallback values for the Figma API.
/// </summary>
public class Constants
{
    /// <summary>
    /// The namespace for the configuration values.
    /// </summary>
    private const string CONFIG_NAMESPACE = "FigmaHttpClient";

    /// <summary>
    /// The configuration key for the Figma API token.
    /// </summary>
    public const string CONFIG_NAME_API_TOKEN = $"{CONFIG_NAMESPACE}:ApiToken";

    /// <summary>
    /// The configuration key for the timeout value in minutes for the HttpClient.
    /// </summary>
    public const string CONFIG_NAME_TIMEOUT_MINUTES = $"{CONFIG_NAMESPACE}:TimeoutMinutes";

    /// <summary>
    /// The configuration key for the retry amount for the HttpClient.
    /// </summary>
    public const string CONFIG_NAME_RETRY_AMOUNT = $"{CONFIG_NAMESPACE}:RetryAmount";

    /// <summary>
    /// The default timeout value in minutes for the HttpClient.
    /// </summary>
    public const int FALLBACK_VALUE_TIMEOUT_MINUTES = 5;

    /// <summary>
    /// The configuration key for the retry amount for the HttpClient.
    /// </summary>
    public const int FALLBACK_VALUE_RETRY_AMOUNT = 10;

    /// <summary>
    /// The base URL for the Figma API.
    /// </summary>
    public const string FIGMA_API_BASE_URL = "https://api.figma.com";
}
