namespace FigmaDotNet;

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
using System.Xml.Linq;

using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Primitives;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using Polly;
using Polly.Retry;

using FigmaDotNet.Enums;
using FigmaDotNet.Models.Response;
using FigmaDotNet.Models.Webhook;

public sealed class FigmaHttpClient: IDisposable
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
    private readonly AsyncRetryPolicy<HttpResponseMessage> _retryPolicy;

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

    public FigmaHttpClient(ILoggerFactory loggerFactory, HttpClient httpClient, IConfiguration configuration)
    {
        _logger = loggerFactory.CreateLogger<FigmaHttpClient>();
        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri(_apiUrl);
        _retryPolicy = Policy.HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
            .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                (result, timeSpan, retryCount, context) =>
                {
                    Console.WriteLine($"Retry {retryCount} due to {result.Result.StatusCode}. Waiting {timeSpan} before next retry.");
                });

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
            AutoReplenishment = true
        });

        _imageCostRateLimiter = new TokenBucketRateLimiter(new TokenBucketRateLimiterOptions
        {
            TokenLimit = IMAGE_COST,
            TokensPerPeriod = IMAGE_COST,
            ReplenishmentPeriod = TimeSpan.FromMinutes(1),
            AutoReplenishment = true
        });

        _versionCostRateLimiter = new TokenBucketRateLimiter(new TokenBucketRateLimiterOptions
        {
            TokenLimit = VERSION_COST,
            TokensPerPeriod = VERSION_COST,
            ReplenishmentPeriod = TimeSpan.FromMinutes(1),
            AutoReplenishment = true
        });

        _commentCostRateLimiter = new TokenBucketRateLimiter(new TokenBucketRateLimiterOptions
        {
            TokenLimit = COMMENT_COST,
            TokensPerPeriod = COMMENT_COST,
            ReplenishmentPeriod = TimeSpan.FromMinutes(1),
            AutoReplenishment = true
        });

        _webhookCostRateLimiter = new TokenBucketRateLimiter(new TokenBucketRateLimiterOptions
        {
            TokenLimit = WEBHOOK_COST,
            TokensPerPeriod = WEBHOOK_COST,
            ReplenishmentPeriod = TimeSpan.FromMinutes(1),
            AutoReplenishment = true
        });

        _teamCostRateLimiter = new TokenBucketRateLimiter(new TokenBucketRateLimiterOptions
        {
            TokenLimit = TEAM_COST,
            TokensPerPeriod = TEAM_COST,
            ReplenishmentPeriod = TimeSpan.FromMinutes(1),
            AutoReplenishment = true
        });

        _projectCostRateLimiter = new TokenBucketRateLimiter(new TokenBucketRateLimiterOptions
        {
            TokenLimit = PROJECT_COST,
            TokensPerPeriod = PROJECT_COST,
            ReplenishmentPeriod = TimeSpan.FromMinutes(1),
            AutoReplenishment = true
        });

        _fileImageCostRateLimiter = new TokenBucketRateLimiter(new TokenBucketRateLimiterOptions
        {
            TokenLimit = FILE_IMAGE_COST,
            TokensPerPeriod = FILE_IMAGE_COST,
            ReplenishmentPeriod = TimeSpan.FromMinutes(1),
            AutoReplenishment = true
        });

        _selectionCostRateLimiter = new TokenBucketRateLimiter(new TokenBucketRateLimiterOptions
        {
            TokenLimit = SELECTION_COST,
            TokensPerPeriod = SELECTION_COST,
            ReplenishmentPeriod = TimeSpan.FromMinutes(1),
            AutoReplenishment = true
        });

        _recentFilesCostRateLimiter = new TokenBucketRateLimiter(new TokenBucketRateLimiterOptions
        {
            TokenLimit = RECENT_FILES_COST,
            TokensPerPeriod = RECENT_FILES_COST,
            ReplenishmentPeriod = TimeSpan.FromMinutes(1),
            AutoReplenishment = true
        });
    }

    private async Task<T> RateLimitedFigmaApiCallAsync<T>(string fetchUrl, TokenBucketRateLimiter _rateLimiter, HttpMethod httpMethod = null, HttpContent content = null, CancellationToken cancellationToken = default)
    {
        T result = default(T);

        if (httpMethod == null)
        {
            httpMethod = HttpMethod.Get;
            _logger.LogInformation($"No HTTP method was provided, proceed {httpMethod.Method} method.");
        }

        _logger.LogInformation($"Start rate limiter.");

        var lease = await _rateLimiter.AcquireAsync(1, cancellationToken);
        if (lease.IsAcquired)
        {
            try
            {
                _logger.LogInformation($"Send request.");
                HttpResponseMessage response = await _retryPolicy.ExecuteAsync(async () =>
                {
                    _logger.LogInformation($"Create {httpMethod.Method} request message for '{fetchUrl}'.");
                    using var request = new HttpRequestMessage(httpMethod, fetchUrl);
                    request.Headers.Add("X-FIGMA-TOKEN", _apiToken);

                    if (content != null)
                    {
                        _logger.LogInformation($"Set request content.");
                        request.Content = content;
                    }
                    return await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
                });

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("API call succeeded.");

                    if (typeof(T) == typeof(String))
                    {
                        var stringResult = await response.Content.ReadAsStringAsync(cancellationToken);
                        result = (T)Convert.ChangeType(stringResult, typeof(T));
                    }

                    result = await response.Content.ReadFromJsonAsync<T>(cancellationToken);
                }
                else
                {
                    _logger.LogError("API call failed after retries.");
                }
            }
            catch (OperationCanceledException ex)
            {
                _logger.LogWarning($"{httpMethod.Method} request to '{_apiUrl}/{fetchUrl}' failed!\n{ex.StackTrace}");
                _logger.LogError($"{ex.Message}");
                _logger.LogTrace($"{ex.StackTrace}");

                throw;
            }
            finally
            {
                lease.Dispose();
            }
        }
        else
        {
            _logger.LogTrace($"Wait 1 minute until next request.");
            await Task.Delay(TimeSpan.FromMinutes(1));
        }

        return result;
    }

    public async Task<CommentsResponse> GetCommentsAsync(string fileKey, CancellationToken cancellationToken)
    {
        string fetchUrl = $"/v1/files/{fileKey}/comments";
        var result = await RateLimitedFigmaApiCallAsync<CommentsResponse>(fetchUrl, _fileCostRateLimiter, cancellationToken: cancellationToken);

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
        var result = await RateLimitedFigmaApiCallAsync<ComponentResponse>(fetchUrl, _fileCostRateLimiter, cancellationToken: cancellationToken);

        return result;
    }

    public async Task<ComponentsResponse> GetFileComponentsAsync(string fileKey, CancellationToken cancellationToken)
    {
        string fetchUrl = $"/v1/files/{fileKey}/components";
        var result = await RateLimitedFigmaApiCallAsync<ComponentsResponse>(fetchUrl, _fileCostRateLimiter, cancellationToken: cancellationToken);

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
        var result = await RateLimitedFigmaApiCallAsync<ComponentSetsResponse>(fetchUrl, _fileCostRateLimiter, cancellationToken: cancellationToken);

        return result;
    }

    public async Task<FileResponse> GetFileAsync(string fileKey, CancellationToken cancellationToken, int depth = 2)
    {
        string fetchUrl = $"/v1/files/{fileKey}?depth={depth}";
        var result = await RateLimitedFigmaApiCallAsync<FileResponse>(fetchUrl, _fileCostRateLimiter, cancellationToken: cancellationToken);

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
        var result = await RateLimitedFigmaApiCallAsync<ImageResponse>(fetchUrl, _imageCostRateLimiter, cancellationToken: cancellationToken);

        return result;
    }

    public async Task<string> GetSvgUrlAsync(string fileKey, string ids, CancellationToken cancellationToken = default, float scale = 1, bool svgOutlineText = true, bool svgIncludeId = false,
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

        return imageUrl;
    }

    public async Task<string> GetSvgSourceAsync(string fileKey, string ids, CancellationToken cancellationToken = default, float scale = 1, bool svgOutlineText = true, bool svgIncludeId = false,
        bool svgIncludeNodeId = false, bool svgSimplifyStroke = true, bool contentsOnly = true, bool useAbsoluteBounds = false, string version = null)
    {
        string imageUrl = await GetSvgUrlAsync(fileKey, ids, cancellationToken, scale, svgOutlineText, svgIncludeId, svgIncludeNodeId, svgSimplifyStroke, contentsOnly, useAbsoluteBounds, version);
        string svgSource = await RateLimitedFigmaApiCallAsync<string>(imageUrl, _fileImageCostRateLimiter, cancellationToken: cancellationToken);

        if (!IsValidSvg(svgSource))
        {
            _logger.LogError("Invalid svg source from server!");
            return null;
        }

        return svgSource;
    }

    private bool IsValidSvg (string source)
    {
        try
        {
            XDocument.Parse(source);
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public async Task<bool> DeleteWebhookAsync(string id, CancellationToken cancellationToken = default)
    {
        string fetchUrl = $"/v2/webhooks/{id}";
        var result = await RateLimitedFigmaApiCallAsync<string>(fetchUrl, _webhookCostRateLimiter,  HttpMethod.Delete, cancellationToken: cancellationToken);

        _logger.LogInformation($"Done with result: '{result}'");
        return true;
    }

    public async Task<IEnumerable<WebHookV2>> GetTeamWebhooksAsync(string teamId, CancellationToken cancellationToken = default)
    {
        string fetchUrl = $"/v2/teams/{teamId}/webhooks";
        var webHookList = await RateLimitedFigmaApiCallAsync<WebHookListV2>(fetchUrl, _webhookCostRateLimiter, cancellationToken: cancellationToken);

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
        var result = await RateLimitedFigmaApiCallAsync<string>(fetchUrl, _webhookCostRateLimiter, HttpMethod.Post, content, cancellationToken);

        _logger.LogInformation($"Webhook was created: {result}");
    }

    public void Dispose()
    {
        _commentCostRateLimiter.Dispose();
        _fileCostRateLimiter.Dispose();
        _fileImageCostRateLimiter.Dispose();
        _imageCostRateLimiter.Dispose();
        _projectCostRateLimiter.Dispose();
        _recentFilesCostRateLimiter.Dispose();
        _selectionCostRateLimiter.Dispose();
        _teamCostRateLimiter.Dispose();
        _versionCostRateLimiter.Dispose();
        _webhookCostRateLimiter.Dispose();
    }
}
