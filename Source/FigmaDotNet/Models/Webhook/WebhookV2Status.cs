namespace FigmaDotNet.Models.Webhook;

/// <summary>
/// An enum representing the possible statuses you can set a webhook to <see cref="ACTIVE"/> or <see cref="PAUSED"/>
/// </summary>
public struct WebHookV2Status
{
    /// <summary>
    /// The webhook is healthy and receive all events
    /// </summary>
    public const string ACTIVE = "ACTIVE";

    /// <summary>
    /// The webhook is paused and will not receive any events
    /// </summary>
    public const string PAUSED = "PAUSED";
}
