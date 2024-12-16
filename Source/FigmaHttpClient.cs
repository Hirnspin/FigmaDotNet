namespace FigmaDotNet;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.RateLimiting;
using System.Threading.Tasks;
using FigmaDotNet.Enums;
using FigmaDotNet.Models.Response;
using FigmaDotNet.Models.Webhook;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Primitives;

public sealed class FigmaHttpClient
{
    private readonly ILogger<FigmaHttpClient> _logger;
    private readonly HttpClient _httpClient;
    private readonly string _apiToken;
    private readonly string _apiUrl = "https://api.figma.com";
    // Using rate limiter for each Figma endpoint type, because Figma API is a bit odd. See https://forum.figma.com/t/rest-api-rate-limit/11687/6
    private readonly TokenBucketRateLimiter _fileCostRateLimiter;
    private readonly TokenBucketRateLimiter _imageCostRateLimiter;
    private readonly TokenBucketRateLimiter _webhookCostRateLimiter;
    private readonly TokenBucketRateLimiter _versionCostRateLimiter;
    private readonly TokenBucketRateLimiter _commentCostRateLimiter;
    private readonly TokenBucketRateLimiter _teamCostRateLimiter;
    private readonly TokenBucketRateLimiter _projectCostRateLimiter;
    private readonly TokenBucketRateLimiter _fileImageCostRateLimiter;
    private readonly TokenBucketRateLimiter _selectionCostRateLimiter;
    private readonly TokenBucketRateLimiter _recentFilesCostRateLimiter;

    private const string CONFIG_NAME_FIGMA_API_TOKEN = "FIGMA_API_TOKEN";
    private const int FILE_COST = 50; // Equates to 120 req/min and 24000 req/day per user
    private const int FILE_IMAGE_COST = 20; // Equates to 300 req/min and 60000 req/day per user
    private const int COMMENT_COST = 20; // Equates to 300 req/min and 60000 req/day per user
    private const int IMAGE_COST = 200; // Equates to 30 req/min and 6000 req/day per user
    private const int VERSION_COST = 100; // Equates to 60 req/min and 12000 req/day per user
    private const int PROJECT_COST = 20; // Equates to 300 req/min and 60000 req/day per user
    private const int WEBHOOK_COST = 20; // Equates to 300 req/min and 60000 req/day per user
    private const int TEAM_COST = 20; // Equates to 300 req/min and 60000 req/day per user
    private const int SELECTION_COST = 20; // Equates to 300 req/min and 60000 req/day per user
    private const int RECENT_FILES_COST = 10; // Equates to 600 req/min and 120000 req/day per user
    private const int QUEUE_LIMIT = 400; // Equates to 600 req/min and 120000 req/day per user

    public FigmaHttpClient(ILoggerFactory loggerFactory, HttpClient httpClient, IConfiguration configuration)
    {
        _logger = loggerFactory.CreateLogger<FigmaHttpClient>();
        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri(_apiUrl);

        if (configuration.GetSection(CONFIG_NAME_FIGMA_API_TOKEN).Exists())
        {
            _apiToken = configuration[CONFIG_NAME_FIGMA_API_TOKEN];
        }
        else
        {
            var errorMessage = $"No FIGMA_API_TOKEN was provided! '{CONFIG_NAME_FIGMA_API_TOKEN}'";
            _logger.LogError(errorMessage);
            throw new ConfigurationErrorsException(errorMessage);
        }

        _fileCostRateLimiter = new TokenBucketRateLimiter(new TokenBucketRateLimiterOptions
        {
            TokenLimit = FILE_COST,
            TokensPerPeriod = FILE_COST,
            ReplenishmentPeriod = TimeSpan.FromMinutes(1),
            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
            QueueLimit = QUEUE_LIMIT
        });

        _imageCostRateLimiter = new TokenBucketRateLimiter(new TokenBucketRateLimiterOptions
        {
            TokenLimit = IMAGE_COST,
            TokensPerPeriod = IMAGE_COST,
            ReplenishmentPeriod = TimeSpan.FromMinutes(1),
            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
            QueueLimit = QUEUE_LIMIT
        });

        _versionCostRateLimiter = new TokenBucketRateLimiter(new TokenBucketRateLimiterOptions
        {
            TokenLimit = VERSION_COST,
            TokensPerPeriod = VERSION_COST,
            ReplenishmentPeriod = TimeSpan.FromMinutes(1),
            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
            QueueLimit = QUEUE_LIMIT
        });

        _commentCostRateLimiter = new TokenBucketRateLimiter(new TokenBucketRateLimiterOptions
        {
            TokenLimit = COMMENT_COST,
            TokensPerPeriod = COMMENT_COST,
            ReplenishmentPeriod = TimeSpan.FromMinutes(1),
            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
            QueueLimit = QUEUE_LIMIT
        });

        _webhookCostRateLimiter = new TokenBucketRateLimiter(new TokenBucketRateLimiterOptions
        {
            TokenLimit = WEBHOOK_COST,
            TokensPerPeriod = WEBHOOK_COST,
            ReplenishmentPeriod = TimeSpan.FromMinutes(1),
            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
            QueueLimit = QUEUE_LIMIT
        });

        _teamCostRateLimiter = new TokenBucketRateLimiter(new TokenBucketRateLimiterOptions
        {
            TokenLimit = TEAM_COST,
            TokensPerPeriod = TEAM_COST,
            ReplenishmentPeriod = TimeSpan.FromMinutes(1),
            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
            QueueLimit = QUEUE_LIMIT
        });

        _projectCostRateLimiter = new TokenBucketRateLimiter(new TokenBucketRateLimiterOptions
        {
            TokenLimit = PROJECT_COST,
            TokensPerPeriod = PROJECT_COST,
            ReplenishmentPeriod = TimeSpan.FromMinutes(1),
            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
            QueueLimit = QUEUE_LIMIT
        });

        _fileImageCostRateLimiter = new TokenBucketRateLimiter(new TokenBucketRateLimiterOptions
        {
            TokenLimit = FILE_IMAGE_COST,
            TokensPerPeriod = FILE_IMAGE_COST,
            ReplenishmentPeriod = TimeSpan.FromMinutes(1),
            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
            QueueLimit = QUEUE_LIMIT
        });

        _selectionCostRateLimiter = new TokenBucketRateLimiter(new TokenBucketRateLimiterOptions
        {
            TokenLimit = SELECTION_COST,
            TokensPerPeriod = SELECTION_COST,
            ReplenishmentPeriod = TimeSpan.FromMinutes(1),
            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
            QueueLimit = QUEUE_LIMIT
        });

        _recentFilesCostRateLimiter = new TokenBucketRateLimiter(new TokenBucketRateLimiterOptions
        {
            TokenLimit = RECENT_FILES_COST,
            TokensPerPeriod = RECENT_FILES_COST,
            ReplenishmentPeriod = TimeSpan.FromMinutes(1),
            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
            QueueLimit = QUEUE_LIMIT
        });
    }

    private async Task<T> UseFigmaApiAsync<T>(string fetchUrl, TokenBucketRateLimiter _rateLimiter, CancellationToken cancellationToken = default, 
        HttpMethod httpMethod = null, HttpContent content = null)
    {
        if (httpMethod == null)
        {
            httpMethod = HttpMethod.Get;
        }

        using var request = new HttpRequestMessage(httpMethod, fetchUrl);
        request.Headers.Add("X-FIGMA-TOKEN", _apiToken);

        if (content != null)
        {
            request.Content = content;
        }

        while (true)
        {
            using var lease = await _rateLimiter.AcquireAsync(1);
            if (lease.IsAcquired)
            {
                var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
                response.EnsureSuccessStatusCode();

                if (typeof(T) == typeof(String))
                {
                    var stringResult = await response.Content.ReadAsStringAsync();
                    return (T)Convert.ChangeType(stringResult, typeof(T));
                }

                var result = await response.Content.ReadFromJsonAsync<T>();
                return result;
            }
            else
            {
                await Task.Delay(TimeSpan.FromMinutes(1));
            }
        }
    }

    public async Task<CommentsResponse> GetCommentsAsync(string fileKey, CancellationToken cancellationToken)
    {
        string fetchUrl = $"/v1/files/{fileKey}/comments";
        var result = await UseFigmaApiAsync<CommentsResponse>(fetchUrl, _fileCostRateLimiter, cancellationToken);

        return result;
    }

    /// <summary>
    /// Get metadata on a component by key.
    /// </summary>
    /// <see cref="https://www.figma.com/developers/api#get-component-endpoint"/>
    /// <param name="fileKey"></param>
    /// <param name="cancellationToken"></param>
    public async Task<ComponentResponse> GetComponentAsync(string key, CancellationToken cancellationToken)
    {
        string fetchUrl = $"/v1/components/{key}";
        var result = await UseFigmaApiAsync<ComponentResponse>(fetchUrl, _fileCostRateLimiter, cancellationToken);

        return result;
    }

    public async Task<ComponentsResponse> GetFileComponentsAsync(string fileKey, CancellationToken cancellationToken)
    {
        string fetchUrl = $"/v1/files/{fileKey}/components";
        var result = await UseFigmaApiAsync<ComponentsResponse>(fetchUrl, _fileCostRateLimiter, cancellationToken);

        return result;
    }

    /// <summary>
    /// Get a list of published component sets within a file library.
    /// </summary>
    /// <see cref="https://www.figma.com/developers/api#get-file-component-sets-endpoint"/>
    /// <param name="fileKey"></param>
    /// <param name="cancellationToken"></param>
    public async Task<ComponentSetsResponse> GetFileComponentSetsAsync(string fileKey, CancellationToken cancellationToken)
    {
        string fetchUrl = $"/v1/files/{fileKey}/component_sets";
        var result = await UseFigmaApiAsync<ComponentSetsResponse>(fetchUrl, _fileCostRateLimiter, cancellationToken);

        return result;
    }

    public async Task<FileResponse> GetFileAsync(string fileKey, CancellationToken cancellationToken, int depth = 2)
    {
        string fetchUrl = $"/v1/files/{fileKey}?depth={depth}";
        var result = await UseFigmaApiAsync<FileResponse>(fetchUrl, _fileCostRateLimiter, cancellationToken);

        return result;
    }

    /// <summary>
    /// Get a list of published component sets within a file library.
    /// </summary>
    /// <see cref="https://www.figma.com/developers/api#get-file-component-sets-endpoint"/>
    /// <param name="fileKey">File to export images from. This can be a file key or branch key. Use GET /v1/files/:key with the branch_data query param to get the branch key.</param>
    /// <param name="ids">A comma separated list of node IDs to render</param>
    /// <param name="cancellationToken"></param>
    /// <param name="scale">A number between 0.01 and 4, the image scaling factor</param>
    /// <param name="format">A string enum for the image output format, can be jpg, png, svg, or pdf</param>
    /// <param name="svgOutlineText">
    /// Whether text elements are rendered as outlines (vector paths) or as <text> elements in SVGs. Default: true.
    /// Rendering text elements as outlines guarantees that the text looks exactly the same in the SVG as it does in the browser/inside Figma.
    /// Exporting as <text> allows text to be selectable inside SVGs and generally makes the SVG easier to read.However, this relies on the browser's rendering engine which can vary between browsers and/or operating systems. As such, visual accuracy is not guaranteed as the result could look different than in Figma.
    /// </param>
    /// <param name="svgIncludeId">
    /// Whether to include id attributes for all SVG elements. Adds the layer name to the id attribute of an svg element. Default: false.
    /// </param>
    /// <param name="svgIncludeNodeId">Whether to include node id attributes for all SVG elements. Adds the node id to a data-node-id attribute of an svg element. Default: false.</param>
    /// <param name="svgSimplifyStroke">Whether to simplify inside/outside strokes and use stroke attribute if possible instead of <mask>. Default: true.</param>
    /// <param name="contentsOnly">
    /// Whether content that overlaps the node should be excluded from rendering. Passing false (i.e., rendering overlaps) may increase processing time, since more of the document must be included in rendering. Default: true.
    /// </param>
    /// <param name="useAbsoluteBounds">
    /// Use the full dimensions of the node regardless of whether or not it is cropped or the space around it is empty. Use this to export text nodes without cropping. Default: false.
    /// </param>
    /// <param name="version">A specific version ID to use. Omitting this will use the current version of the file</param>
    /// <returns></returns>
    public async Task<ImageResponse> GetImageAsync(string fileKey, string ids, CancellationToken cancellationToken = default, float scale = 1,
    FileFormatType format = FileFormatType.svg, bool svgOutlineText = true, bool svgIncludeId = false,
        bool svgIncludeNodeId = false, bool svgSimplifyStroke = true, bool contentsOnly = true, bool useAbsoluteBounds = false, string version = null)
    {
        var qb = new Dictionary<string, StringValues> {
            { "ids", ids },
            { "scale", scale.ToString() },
            { "format", format.ToString() },
            { "svg_outline_text", svgOutlineText.ToString().ToLower() },
            { "svg_include_id", svgIncludeId.ToString().ToLower() },
            { "svg_include_node_id", svgIncludeNodeId.ToString().ToLower() },
            { "svg_simplify_stroke", svgSimplifyStroke.ToString().ToLower() },
            { "contents_only", contentsOnly.ToString().ToLower() }
        };

        if (version != null)
        {
            qb.Add("version", version);
        }

        string fetchUrl = QueryHelpers.AddQueryString($"/v1/images/{fileKey}", qb);
        var result = await UseFigmaApiAsync<ImageResponse>(fetchUrl, _imageCostRateLimiter, cancellationToken);

        return result;
    }

    public async Task<string> GetSvgSourceAsync(string fileKey, string ids, CancellationToken cancellationToken = default, float scale = 1, bool svgOutlineText = true, bool svgIncludeId = false,
        bool svgIncludeNodeId = false, bool svgSimplifyStroke = true, bool contentsOnly = true, bool useAbsoluteBounds = false, string version = null)
    {
        ImageResponse imgResponse = await GetImageAsync(fileKey, ids, cancellationToken, scale, FileFormatType.svg, svgOutlineText, svgIncludeId, svgIncludeNodeId, svgSimplifyStroke, contentsOnly, useAbsoluteBounds, version);

        if (imgResponse == null)
        {
            _logger.LogWarning($"Image response is null for file '{fileKey}' and ids '{ids}'.");
            return null;
        }

        if (!imgResponse.Images.Any(i => i.Key == ids))
        {
            _logger.LogWarning($"Svg source with the ids '{ids}' was not found in file '{fileKey}'!");
            return null;
        }

        string imageUrl = imgResponse.Images.First(i => i.Key == ids).Value;

        var response = await _httpClient.GetAsync(imageUrl);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
    }

    public async Task<bool> DeleteWebhookAsync(string id, CancellationToken cancellationToken = default)
    {
        string fetchUrl = $"/v2/webhooks/{id}";
        var result = await UseFigmaApiAsync<string>(fetchUrl, _webhookCostRateLimiter, cancellationToken, HttpMethod.Delete);

        _logger.LogInformation($"Done with result: '{result}'");
        return true;
    }

    public async Task<IEnumerable<WebHookV2>> GetTeamWebhooksAsync(string teamId, CancellationToken cancellationToken = default)
    {
        string fetchUrl = $"/v2/teams/{teamId}/webhooks";
        var webHookList = await UseFigmaApiAsync<WebHookListV2>(fetchUrl, _webhookCostRateLimiter, cancellationToken);

        if (webHookList?.WebHooks.Count() > 0)
        {
            _logger.LogInformation($"Found {webHookList.WebHooks.Count()} WebHooks at Figma API.");
            _logger.LogInformation($"{webHookList.WebHooks.Count(wh => Equals(wh.Status, WebHookV2Status.ACTIVE))} of them is/are active.");
            return webHookList.WebHooks;
        }

        _logger.LogInformation("No webhook found.");
        return Enumerable.Empty<WebHookV2>();
    }

    /// <summary>
    /// Create a new webhook which will call the specified endpoint when the event triggers. By default, this webhook will automatically send a PING event to the endpoint when it is created. If this behavior is not desired, you can create the webhook and set the status to PAUSED and reactivate it later.
    /// </summary>
    /// <param name="url"></param>
    /// <param name="description"></param>
    /// <param name="passcode"></param>
    /// <param name="teamId"></param>
    /// <see cref="https://www.figma.com/developers/api#webhooks-v2-post-endpoint"/>
    public async Task PostWebhookAsync(WebHook requestPayload, CancellationToken cancellationToken = default)
    {
        string fetchUrl = $"/v2/webhooks";
        var content = new StringContent(JsonSerializer.Serialize(requestPayload), Encoding.UTF8, "application/json");
        var result = await UseFigmaApiAsync<string>(fetchUrl, _webhookCostRateLimiter, cancellationToken, HttpMethod.Post, content);

        _logger.LogInformation($"Webhook was created: {result}");
    }
}
