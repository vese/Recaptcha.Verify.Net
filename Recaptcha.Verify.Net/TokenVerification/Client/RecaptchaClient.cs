using System.Net.Http.Json;

namespace Recaptcha.Verify.Net.TokenVerification.Client;

/// <inheritdoc />
internal class RecaptchaClient(HttpClient httpClient) : IRecaptchaClient
{
    private const string verifyMethod = "siteverify";

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

        var response = await httpClient.PostAsync(verifyMethod, content, cancellationToken);

        response.EnsureSuccessStatusCode();

        var verifyResponse = await response.Content.ReadFromJsonAsync<VerifyResponse>(cancellationToken);

        if (verifyResponse is null)
        {
            throw new EmptyResponseException();
        }

        return verifyResponse;
    }
}
