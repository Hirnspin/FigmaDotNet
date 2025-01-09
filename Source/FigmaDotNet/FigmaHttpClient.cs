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

namespace FigmaDotNet;

/// <summary>
/// FigmaHttpClient is a class that handles all the HTTP requests to the Figma API.
/// </summary>
public sealed class FigmaHttpClient : IDisposable
{
    private readonly ILogger _logger;
    private readonly HttpClient _httpClient;
    private readonly string _apiToken;
    private readonly int _retryAmount;
    // Using rate limiter for each Figma endpoint type, because Figma API is a bit odd. See https://forum.figma.com/t/rest-api-rate-limit/11687/6
    private readonly TokenBucketRateLimiter _fileCostRateLimiter = new (
        new ()
        {
            TokenLimit = 24000,
            TokensPerPeriod = 120,
            ReplenishmentPeriod = TimeSpan.FromMinutes(1),
            AutoReplenishment = true
        });
    private readonly TokenBucketRateLimiter _imageCostRateLimiter = new(
        new()
        {
            TokenLimit = 6000,
            TokensPerPeriod = 30,
            ReplenishmentPeriod = TimeSpan.FromMinutes(1),
            AutoReplenishment = true
        }); 
    private readonly TokenBucketRateLimiter _fileImageCostRateLimiter = new(
        new()
        {
            TokenLimit = 60000,
            TokensPerPeriod = 300,
            ReplenishmentPeriod = TimeSpan.FromMinutes(1),
            AutoReplenishment = true
        });
    private readonly TokenBucketRateLimiter _webhookCostRateLimiter = new(
        new()
        {
            TokenLimit = 60000,
            TokensPerPeriod = 300,
            ReplenishmentPeriod = TimeSpan.FromMinutes(1),
            AutoReplenishment = true
        });
    private readonly TokenBucketRateLimiter _versionCostRateLimiter = new(
        new()
        {
            TokenLimit = 12000,
            TokensPerPeriod = 60,
            ReplenishmentPeriod = TimeSpan.FromMinutes(1),
            AutoReplenishment = true
        });
    private readonly TokenBucketRateLimiter _commentCostRateLimiter = new(
        new()
        {
            TokenLimit = 60000,
            TokensPerPeriod = 300,
            ReplenishmentPeriod = TimeSpan.FromMinutes(1),
            AutoReplenishment = true
        });
    private readonly TokenBucketRateLimiter _teamCostRateLimiter = new(
        new()
        {
            TokenLimit = 60000,
            TokensPerPeriod = 300,
            ReplenishmentPeriod = TimeSpan.FromMinutes(1),
            AutoReplenishment = true
        });
    private readonly TokenBucketRateLimiter _projectCostRateLimiter = new(
        new()
        {
            TokenLimit = 60000,
            TokensPerPeriod = 300,
            ReplenishmentPeriod = TimeSpan.FromMinutes(1),
            AutoReplenishment = true
        });
    private readonly TokenBucketRateLimiter _selectionCostRateLimiter = new(
        new()
        {
            TokenLimit = 60000,
            TokensPerPeriod = 300,
            ReplenishmentPeriod = TimeSpan.FromMinutes(1),
            AutoReplenishment = true
        });
    private readonly TokenBucketRateLimiter _recentFilesCostRateLimiter = new(
        new()
        {
            TokenLimit = 12000,
            TokensPerPeriod = 600,
            ReplenishmentPeriod = TimeSpan.FromMinutes(1),
            AutoReplenishment = true
        });
    private readonly AsyncRetryPolicy<HttpResponseMessage> _retryPolicy;

    /// <summary>
    /// Constructor for the FigmaHttpClient.
    /// </summary>
    /// <param name="httpClient"></param>
    /// <param name="logger"></param>
    /// <param name="configuration"></param>
    /// <param name="apiToken"></param>
    /// <param name="retryAmount"></param>
    /// <param name="timeOut"></param>
    /// <exception cref="ConfigurationErrorsException"></exception>
    public FigmaHttpClient(HttpClient httpClient, ILogger<FigmaHttpClient> logger, IConfiguration configuration = null, string apiToken = null, int? retryAmount = null, int? timeOut = null)
    {
        _logger = logger;
        _httpClient = httpClient;

        if (!string.IsNullOrEmpty(apiToken))
        {
            _logger.LogInformation($"Using api token from parameter.");
            _apiToken = apiToken;
        }
        else if (configuration.GetSection(Constants.CONFIG_NAME_API_TOKEN).Exists())
        {
            _logger.LogInformation($"Using api token from configuration.");
            _apiToken = configuration.GetValue<string>(Constants.CONFIG_NAME_API_TOKEN);
        }
        else
        {
            throw new ConfigurationErrorsException($"No ${Constants.CONFIG_NAME_API_TOKEN} was provided!");
        }

        if (retryAmount != null)
        {
            _logger.LogInformation($"Using retry amount '{retryAmount}' from parameter.");
            _retryAmount = (int)retryAmount;
        }
        else if (configuration.GetSection(Constants.CONFIG_NAME_RETRY_AMOUNT).Exists())
        {
            _retryAmount = configuration.GetValue<int>(Constants.CONFIG_NAME_RETRY_AMOUNT, Constants.FALLBACK_VALUE_RETRY_AMOUNT);
            _logger.LogInformation($"Using retry amount '{_retryAmount}' from configuration.");
        }

        _retryPolicy = Policy.HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode || r.Content == null)
            .WaitAndRetryAsync(_retryAmount, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                (result, timeSpan, retryCount, context) =>
                {
                    _logger.LogInformation($"Retry {retryCount} due to {result.Result.StatusCode.ToString() ?? "null content"}. Waiting {timeSpan} before next retry.");
                });
    }

    private async Task<T> RateLimitedFigmaApiCallAsync<T>(string fetchUrl, TokenBucketRateLimiter rateLimiter, HttpMethod httpMethod = null, HttpContent content = null, CancellationToken cancellationToken = default)
    {
        T result = default;

        httpMethod ??= HttpMethod.Get;
        _logger.LogInformation($"Proceeding with {httpMethod.Method} method.");

        _logger.LogInformation("Starting rate limiter.");

        var lease = await rateLimiter.AcquireAsync(1, cancellationToken);
        if (lease.IsAcquired)
        {
            try
            {
                _logger.LogInformation("Sending request.");
                HttpResponseMessage response = await _retryPolicy.ExecuteAsync(async () =>
                {
                    _logger.LogInformation($"Creating {httpMethod.Method} request message for '{fetchUrl}'.");
                    using HttpRequestMessage request = new (httpMethod, fetchUrl);
                    request.Headers.Add("X-FIGMA-TOKEN", _apiToken);

                    if (content != null)
                    {
                        _logger.LogInformation("Setting request content.");
                        request.Content = content;
                    }

                    return await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
                });

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("API call succeeded.");

                    if (typeof(T) == typeof(string))
                    {
                        var stringResult = await response.Content.ReadAsStringAsync(cancellationToken);
                        result = (T)Convert.ChangeType(stringResult, typeof(T));
                    }
                    else
                    {
                        result = await response.Content.ReadFromJsonAsync<T>(cancellationToken);
                    }
                }
                else
                {
                    _logger.LogError("API call failed after retries.");
                }
            }
            catch (OperationCanceledException ex)
            {
                _logger.LogWarning($"{httpMethod.Method} request to '{Constants.FIGMA_API_BASE_URL}/{fetchUrl}' was canceled.");
                _logger.LogError(ex, "Operation was canceled.");
                throw;
            }
            finally
            {
                lease.Dispose();
            }
        }
        else
        {
            _logger.LogTrace("Rate limit exceeded. Waiting 1 minute until next request.");
            await Task.Delay(TimeSpan.FromMinutes(1), cancellationToken);
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
        var result = await RateLimitedFigmaApiCallAsync<ImageResponse>(fetchUrl, _fileImageCostRateLimiter, cancellationToken: cancellationToken);

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

        imgResponse.Images.TryGetValue(ids, out string imageUrl);

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
