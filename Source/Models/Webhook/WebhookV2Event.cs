namespace FigmaDotNet.Models.Webhook;

public struct WebHookV2Event
{
    /// <summary>
    /// Triggers when a webhook is created. Used for debugging. Cannot be subscribed to, all webhooks will receive PING events.
    /// </summary>
    public const string PING = "PING";

    /// <summary>
    /// Triggers whenever a file saves. This occurs whenever a file is closed or within 30 seconds after changes have been made.
    /// </summary>
    public const string FILE_UPDATE = "FILE_UPDATE";

    /// <summary>
    /// Triggers whenever a named version is created in the version history of a file.
    /// </summary>
    public const string FILE_VERSION_UPDATE = "FILE_VERSION_UPDATE";

    /// <summary>
    /// Triggers whenever a file has been deleted. If you subscribe to FILE_UPDATE, you automatically get these notifications. Note that this does not trigger on all files within a folder, if the folder is deleted.
    /// </summary>
    public const string FILE_DELETE = "FILE_DELETE";

    /// <summary>
    /// Triggers whenever a library file is published.
    /// </summary>
    public const string LIBRARY_PUBLISH = "LIBRARY_PUBLISH";

    /// <summary>
    /// Triggers when someone comments on a file.
    /// </summary>
    public const string FILE_COMMENT = "FILE_COMMENT";
}
