# FigmaDotNet
## .NET SDK for Figma REST API

This project provides a http client for dotnet to interact with the [Figma REST API](https://www.figma.com/developers/api).

### Getting started

Install the Nuget package from [nuget.org](https://www.nuget.org/packages/FigmaDotNet/) or [github.com](https://github.com/Hirnspin/FigmaDotNet/pkgs/nuget/FigmaDotNet)

```bash
dotnet add package FigmaDotNet
```

You have to add a config to your application:

```json
{
  "Values": {
    "FigmaHttpClient": {
      "ApiToken": "###",
      "RetryAmount": 5, // optional
      "TimeoutMinutes": 5 // optional
    }
  }
}
```

Check https://help.figma.com/hc/en-us/articles/8085703771159-Manage-personal-access-tokens how to get a personal access token in Figma.

The API token can also be applied via the constructor of the `FigmaHttpClient`, as well as the `retryAmount`:
```csharp
var figmaHttpClient = new FigmaHttpClient(logger, configuration, apiKey: "###", retryAmount: 5);;
```

### Further development topics & missing features

- Not all endpoints are implemented yet.
- Only .Net 9 is supported.
- Missing documentation.
- Rate limit customization in config.

### Change log

#### v1.4.0:
- Added endpoints for [Dev Resources](https://www.figma.com/developers/api#dev-resources).

#### v1.3.0:
- Added `ServiceCollectionExtensions` to extract http client and made it configurable.
- The configuration key `FIGMA_API_TOKEN` has changed to `FigmaHttpClient:ApiToken`.
- New configuration key `FigmaHttpClient:TimeoutMinutes` was added to configure http service timeout. Default is 5 minutes.

#### v1.1.1:
- Extend `FileResponse` with `Branches` and `Document` properties.

#### v1.1.0:
- Removed property `InternalName` from `FigmaComponent`.
- Removed property `InternalName` from `WebhookLibraryUpdatePayload`.
