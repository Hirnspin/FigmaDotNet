namespace FigmaDotNet.Test;

using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;

using FigmaDotNet;
using FigmaDotNet.Models.Response;
using FigmaDotNet.Models.Webhook;

public class FigmaHttpClientTests
{
    private readonly FigmaHttpClient _client;
    private readonly Mock<ILogger<FigmaHttpClient>> _logger;
    private readonly Mock<IConfiguration> _configurationMock;

    public FigmaHttpClientTests()
    {
        _logger = new Mock<ILogger<FigmaHttpClient>>();
        _configurationMock = new Mock<IConfiguration>();
        _configurationMock.Setup(c => c["FIGMA_API_TOKEN"]).Returns("test_token");

        _client = new FigmaHttpClient(_logger.Object, _configurationMock.Object);
    }

    [Fact]
    public async Task GetCommentsAsync_ShouldReturnCommentsResponse()
    {
        // Arrange
        string fileKey = "test_file_key";
        var cancellationToken = CancellationToken.None;

        // Act
        var result = await _client.GetCommentsAsync(fileKey, cancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.IsType<CommentsResponse>(result);
    }

    [Fact]
    public async Task GetComponentAsync_ShouldReturnComponentResponse()
    {
        // Arrange
        string key = "test_key";
        var cancellationToken = CancellationToken.None;

        // Act
        var result = await _client.GetComponentAsync(key, cancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.IsType<ComponentResponse>(result);
    }

    [Fact]
    public async Task GetFileComponentsAsync_ShouldReturnComponentsResponse()
    {
        // Arrange
        string fileKey = "test_file_key";
        var cancellationToken = CancellationToken.None;

        // Act
        var result = await _client.GetFileComponentsAsync(fileKey, cancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.IsType<ComponentsResponse>(result);
    }

    [Fact]
    public async Task GetFileComponentSetsAsync_ShouldReturnComponentSetsResponse()
    {
        // Arrange
        string fileKey = "test_file_key";
        var cancellationToken = CancellationToken.None;

        // Act
        var result = await _client.GetFileComponentSetsAsync(fileKey, cancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.IsType<ComponentSetsResponse>(result);
    }

    [Fact]
    public async Task GetFileAsync_ShouldReturnFileResponse()
    {
        // Arrange
        string fileKey = "test_file_key";
        var cancellationToken = CancellationToken.None;

        // Act
        var result = await _client.GetFileAsync(fileKey, cancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.IsType<FileResponse>(result);
    }

    [Fact]
    public async Task GetImageAsync_ShouldReturnImageResponse()
    {
        // Arrange
        string fileKey = "test_file_key";
        string ids = "test_ids";
        var cancellationToken = CancellationToken.None;

        // Act
        var result = await _client.GetImageAsync(fileKey, ids, cancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.IsType<ImageResponse>(result);
    }

    [Fact]
    public async Task GetSvgUrlAsync_ShouldReturnString()
    {
        // Arrange
        string fileKey = "test_file_key";
        string ids = "test_ids";
        var cancellationToken = CancellationToken.None;

        // Act
        var result = await _client.GetSvgUrlAsync(fileKey, ids, cancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.IsType<string>(result);
    }

    [Fact]
    public async Task GetSvgSourceAsync_ShouldReturnString()
    {
        // Arrange
        string fileKey = "test_file_key";
        string ids = "test_ids";
        var cancellationToken = CancellationToken.None;

        // Act
        var result = await _client.GetSvgSourceAsync(fileKey, ids, cancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.IsType<string>(result);
    }

    [Fact]
    public async Task DeleteWebhookAsync_ShouldReturnTrue()
    {
        // Arrange
        string id = "test_id";
        var cancellationToken = CancellationToken.None;

        // Act
        var result = await _client.DeleteWebhookAsync(id, cancellationToken);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task GetTeamWebhooksAsync_ShouldReturnWebHookV2Enumerable()
    {
        // Arrange
        string teamId = "test_team_id";
        var cancellationToken = CancellationToken.None;

        // Act
        var result = await _client.GetTeamWebhooksAsync(teamId, cancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.IsAssignableFrom<IEnumerable<WebHookV2>>(result);
    }

    [Fact]
    public async Task PostWebhookAsync_ShouldCompleteSuccessfully()
    {
        // Arrange
        var requestPayload = new WebHook
        {
            TeamId = "test_team_id",
            EventType = "test_event",
            Endpoint = "https://example.com",
            Passcode = "test_passcode",
            Status = "ACTIVE",
            Description = "Test webhook"
        };
        var cancellationToken = CancellationToken.None;

        // Act
        await _client.PostWebhookAsync(requestPayload, cancellationToken);

        // Assert
        // No exception means success
    }
}
