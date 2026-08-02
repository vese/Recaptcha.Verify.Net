using System.Net.Http.Json;

namespace Recaptcha.Verify.Net.TokenVerification.Client;

/// <inheritdoc />
internal class RecaptchaClient(HttpClient httpClient) : IRecaptchaClient
{
    private const string verifyMethod = "siteverify";

    /// <summary>
    /// Maximum number of characters of the error response body captured in the thrown
    /// <see cref="HttpRequestException"/>. Keeps logs and exception messages bounded.
    /// </summary>
    private const int MaxErrorBodySnippetLength = 512;

    /// <inheritdoc />
    public async Task<VerifyResponse> VerifyAsync(VerifyRequest request, CancellationToken cancellationToken = default)
    {
        var formData = new Dictionary<string, string?>
        {
            { "secret", request.Secret },
            { "response", request.Response },
            { "remoteip", request.RemoteIp }
        };
        var content = new FormUrlEncodedContent(formData);

        using var response = await httpClient.PostAsync(verifyMethod, content, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var bodySnippet = await ReadErrorBodySnippetAsync(response.Content, cancellationToken);
            var message = $"reCAPTCHA verify request failed with status {(int)response.StatusCode} {response.StatusCode}.{bodySnippet}";
            throw new HttpRequestException(message, inner: null, response.StatusCode);
        }

        var verifyResponse = await response.Content.ReadFromJsonAsync<VerifyResponse>(cancellationToken);

        if (verifyResponse is null)
        {
            throw new EmptyResponseException();
        }

        return verifyResponse;
    }

    /// <summary>
    /// Reads a capped snippet of the error response body, best-effort. A body-read failure
    /// must not mask the original failure status, so this falls back to a placeholder.
    /// </summary>
    private static async Task<string> ReadErrorBodySnippetAsync(HttpContent content, CancellationToken cancellationToken)
    {
        string body;
        try
        {
            body = await content.ReadAsStringAsync(cancellationToken);
        }
        catch (Exception)
        {
            return " Body: <body unavailable>.";
        }

        if (body.Length > MaxErrorBodySnippetLength)
        {
            body = body[..MaxErrorBodySnippetLength];
        }

        return $" Body: {body}.";
    }
}
