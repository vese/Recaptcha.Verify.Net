using Microsoft.Extensions.Logging;
using Recaptcha.Verify.Net.Logging;
using Recaptcha.Verify.Net.TokenVerification.Client.Models.Request;
using Xunit;

namespace Recaptcha.Verify.Net.Test.Logging;

/// <summary>
/// Tests that the shared secret is never emitted into logs, whether via the formatted
/// message text or via the structured <c>Data</c> log property (which now holds a redacted
/// string projection rather than the <see cref="VerifyRequest" /> object).
/// </summary>
public class LoggerExtensionsTest
{
    [Fact]
    public void SendingRequest_NeverEmitsSensitiveData()
    {
        const string secret = "super-secret-key";
        var request = new VerifyRequest
        {
            Secret = secret,
            Response = "response-token",
            RemoteIp = "1.2.3.4",
        };

        var capture = new CapturingLogger();
        capture.SendingRequest(request);

        var entry = Assert.Single(capture.Entries);
        Assert.Equal(CoreEventId.SendingRequest, entry.EventId);

        // The secret must never appear in the formatted message text.
        Assert.DoesNotContain(secret, entry.FormattedMessage);
        Assert.Contains("Sending verify request", entry.FormattedMessage);

        // The structured "Data" property is a redacted string (not the VerifyRequest object),
        // so the secret, the response token, and the remote IP are all kept out of it.
        var data = entry.GetProperty("Data")!.ToString()!;
        Assert.DoesNotContain(secret, data);
        Assert.DoesNotContain("response-token", data);
        Assert.DoesNotContain("1.2.3.4", data);
        Assert.Contains("Secret=***", data);
        Assert.Contains("Response=<length=", data);
        Assert.Contains("RemoteIp=***", data);
    }

    private sealed class CapturingLogger : ILogger
    {
        public List<CapturedEntry> Entries { get; } = new();

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => NullScope.Instance;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            Entries.Add(new CapturedEntry(
                eventId,
                formatter(state, exception),
                state));
        }
    }

    private sealed class NullScope : IDisposable
    {
        public static readonly NullScope Instance = new();
        public void Dispose() { }
    }

    private sealed class CapturedEntry
    {
        public EventId EventId { get; }
        public string FormattedMessage { get; }
        public object? State { get; }

        public CapturedEntry(EventId eventId, string formattedMessage, object? state)
        {
            EventId = eventId;
            FormattedMessage = formattedMessage;
            State = state;
        }

        /// <summary>
        /// Reads a structured log property value from the captured state (which is an
        /// <c>IReadOnlyList&lt;KeyValuePair&lt;string, object?&gt;&gt;</c> as produced by the
        /// logging infrastructure).
        /// </summary>
        public object? GetProperty(string name)
        {
            if (State is System.Collections.IEnumerable enumerable)
            {
                foreach (var item in enumerable)
                {
                    if (item is System.Collections.Generic.KeyValuePair<string, object?> kvp && kvp.Key == name)
                    {
                        return kvp.Value;
                    }
                }
            }

            return null;
        }
    }
}
