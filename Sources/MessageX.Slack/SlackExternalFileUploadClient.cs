using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MessageX.Slack;

/// <summary>Uploads files through Slack's supported external upload workflow.</summary>
public sealed class SlackExternalFileUploadClient : IDisposable {
    private static readonly JsonSerializerOptions JsonOptions = new() {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };
    private readonly SlackConnection _connection;
    private readonly HttpClient _httpClient;
    private readonly bool _disposeHttpClient;

    /// <summary>Creates a client with default MessageX transport behavior.</summary>
    public SlackExternalFileUploadClient(SlackConnection connection)
        : this(connection, SlackHttpClientPool.Shared) {
    }

    /// <summary>Creates a client with configured MessageX transport behavior.</summary>
    public SlackExternalFileUploadClient(SlackConnection connection, MessageHttpTransportOptions options)
        : this(connection, MessageHttpClientFactory.CreateClient(options), disposeHttpClient: true) {
    }

    /// <summary>Creates a client over a caller-supplied HTTP client.</summary>
    public SlackExternalFileUploadClient(
        SlackConnection connection,
        HttpClient httpClient,
        bool disposeHttpClient = false) {
        _connection = connection ?? throw new ArgumentNullException(nameof(connection));
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _disposeHttpClient = disposeHttpClient;
    }

    /// <summary>Uploads and finalizes one file, optionally sharing it to a conversation.</summary>
    public async Task<SlackFileUploadResult> UploadAsync(
        SlackFileUploadRequest upload,
        CancellationToken cancellationToken = default) {
        var normalized = Validate(upload);
        using var operationCancellation = MessageHttpClientFactory.CreateOperationCancellation(
            _httpClient,
            cancellationToken);
        try {
            var ticket = await RequestUploadTicketAsync(normalized, operationCancellation.Token).ConfigureAwait(false);
            if (!ticket.IsSuccess) {
                return ticket.Result;
            }

            var uploaded = await UploadContentAsync(
                ticket.UploadUri!,
                normalized.Content,
                normalized.Length,
                normalized.FileName,
                operationCancellation.Token).ConfigureAwait(false);
            if (!uploaded.IsSuccess) {
                return uploaded;
            }

            return await CompleteUploadAsync(
                normalized,
                ticket.FileId!,
                operationCancellation.Token).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested) {
            throw new MessageDeliveryException("Slack file upload timed out.", MessageErrorKind.Transient);
        }
        catch (Exception exception) when (exception is HttpRequestException or IOException) {
            cancellationToken.ThrowIfCancellationRequested();
            throw new MessageDeliveryException("Slack file upload request failed.", MessageErrorKind.Transient);
        }
    }

    private async Task<UploadTicketResult> RequestUploadTicketAsync(
        SlackFileUploadRequest upload,
        CancellationToken cancellationToken) {
        var payload = new Dictionary<string, object?> {
            ["filename"] = upload.FileName,
            ["length"] = upload.Length,
            ["alt_txt"] = upload.AlternativeText,
            ["snippet_type"] = upload.SnippetType
        };
        using var request = CreateApiRequest("files.getUploadURLExternal", payload);
        using var response = await _httpClient
            .SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken)
            .ConfigureAwait(false);
        var responseBody = await MessageHttpResponseReader.ReadUtf8BodyAsync(response, cancellationToken).ConfigureAwait(false);
        var parsed = SlackExternalUploadResponse.ParseTicket(responseBody);
        var accepted = response.IsSuccessStatusCode && parsed.Ok &&
            IsSafeUploadUri(parsed.UploadUri) && IsProviderIdentifier(parsed.FileId, 'F');
        if (accepted) {
            return UploadTicketResult.Success(parsed.UploadUri!, parsed.FileId!);
        }

        return UploadTicketResult.Failure(CreateFailure(
            upload,
            response,
            parsed.Error ?? (parsed.Ok ? "invalid_response" : null),
            "files.getUploadURLExternal"));
    }

    private async Task<SlackFileUploadResult> UploadContentAsync(
        Uri uploadUri,
        Stream content,
        long length,
        string fileName,
        CancellationToken cancellationToken) {
        using var request = new HttpRequestMessage(HttpMethod.Post, uploadUri);
        request.Content = new StreamContent(new NonDisposingStream(content));
        request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
        request.Content.Headers.ContentLength = length;
        using var response = await _httpClient
            .SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken)
            .ConfigureAwait(false);
        await MessageHttpResponseReader.ReadUtf8BodyAsync(response, cancellationToken).ConfigureAwait(false);
        if (response.IsSuccessStatusCode) {
            return new SlackFileUploadResult {
                IsSuccess = true,
                FileName = fileName,
                StatusCode = (int)response.StatusCode,
                CorrelationId = SlackHttpResponseSupport.ReadCorrelationId(response)
            };
        }

        return new SlackFileUploadResult {
            IsSuccess = false,
            FileName = fileName,
            StatusCode = (int)response.StatusCode,
            ProviderCode = "upload_failed",
            ErrorKind = SlackHttpResponseSupport.Classify((int)response.StatusCode, null),
            ErrorMessage = $"Slack upload service returned HTTP status {(int)response.StatusCode}.",
            CorrelationId = SlackHttpResponseSupport.ReadCorrelationId(response),
            RetryAfter = SlackHttpResponseSupport.ReadRetryAfter(response)
        };
    }

    private async Task<SlackFileUploadResult> CompleteUploadAsync(
        SlackFileUploadRequest upload,
        string fileId,
        CancellationToken cancellationToken) {
        var payload = new Dictionary<string, object?> {
            ["files"] = new[] {
                new Dictionary<string, string?> {
                    ["id"] = fileId,
                    ["title"] = upload.Title
                }
            },
            ["channel_id"] = upload.ConversationId,
            ["thread_ts"] = upload.ThreadTimestamp,
            ["initial_comment"] = upload.InitialComment
        };
        using var request = CreateApiRequest("files.completeUploadExternal", payload);
        using var response = await _httpClient
            .SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken)
            .ConfigureAwait(false);
        var responseBody = await MessageHttpResponseReader.ReadUtf8BodyAsync(response, cancellationToken).ConfigureAwait(false);
        var parsed = SlackExternalUploadResponse.ParseCompletion(responseBody);
        var accepted = response.IsSuccessStatusCode && parsed.Ok &&
            string.Equals(parsed.FileId, fileId, StringComparison.Ordinal);
        if (!accepted) {
            return CreateFailure(
                upload,
                response,
                parsed.Error ?? (parsed.Ok ? "invalid_response" : null),
                "files.completeUploadExternal");
        }

        return new SlackFileUploadResult {
            IsSuccess = true,
            FileId = fileId,
            FileName = upload.FileName,
            ConversationId = upload.ConversationId,
            StatusCode = (int)response.StatusCode,
            CorrelationId = SlackHttpResponseSupport.ReadCorrelationId(response)
        };
    }

    private HttpRequestMessage CreateApiRequest(string method, object payload) {
        var request = new HttpRequestMessage(HttpMethod.Post, new Uri(_connection.ApiBaseUri, method)) {
            Content = new StringContent(
                JsonSerializer.Serialize(payload, JsonOptions),
                Encoding.UTF8,
                "application/json")
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _connection.BotToken);
        return request;
    }

    private static SlackFileUploadRequest Validate(SlackFileUploadRequest upload) {
        if (upload is null) {
            throw new ArgumentNullException(nameof(upload));
        }
        if (upload.Content is null || !upload.Content.CanRead) {
            throw new ArgumentException("Slack file content must be a readable stream.", nameof(upload));
        }
        if (upload.Length <= 0) {
            throw new ArgumentException("Slack file length must be greater than zero.", nameof(upload));
        }
        if (upload.Content.CanSeek && upload.Content.Length - upload.Content.Position != upload.Length) {
            throw new ArgumentException("Slack file length must match the bytes remaining in the content stream.", nameof(upload));
        }

        upload.FileName = ValidateText(upload.FileName, 255, "file name", required: true)!;
        if (upload.FileName.IndexOfAny(new[] { '/', '\\' }) >= 0) {
            throw new ArgumentException("Slack file name cannot contain path separators.", nameof(upload));
        }
        upload.Title = ValidateText(upload.Title, 255, "title");
        upload.AlternativeText = ValidateText(upload.AlternativeText, 1000, "alternative text");
        upload.SnippetType = ValidateText(upload.SnippetType, 50, "snippet type");
        upload.InitialComment = ValidateText(upload.InitialComment, 4000, "initial comment");
        if (!string.IsNullOrWhiteSpace(upload.ConversationId)) {
            upload.ConversationId = SlackMessageTarget.ValidateConversationId(upload.ConversationId);
        }
        if (!string.IsNullOrWhiteSpace(upload.ThreadTimestamp)) {
            if (upload.ConversationId is null || SlackMessageValidator.ParseTimestamp(upload.ThreadTimestamp) is null) {
                throw new ArgumentException(
                    "Slack thread uploads require a conversation and valid parent timestamp.",
                    nameof(upload));
            }
            upload.ThreadTimestamp = upload.ThreadTimestamp!.Trim();
        }
        return upload;
    }

    private static string? ValidateText(string? value, int maximumLength, string label, bool required = false) {
        var normalized = value?.Trim();
        if ((required && string.IsNullOrWhiteSpace(normalized)) ||
            normalized?.Length > maximumLength || normalized?.Any(char.IsControl) == true) {
            throw new ArgumentException($"Slack {label} must be bounded non-control text.", label);
        }
        return string.IsNullOrWhiteSpace(normalized) ? null : normalized;
    }

    private static bool IsSafeUploadUri(Uri? uri) => uri is not null && uri.IsAbsoluteUri &&
        string.Equals(uri.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase) &&
        uri.IsDefaultPort && string.IsNullOrEmpty(uri.UserInfo) && string.IsNullOrEmpty(uri.Fragment) &&
        (string.Equals(uri.Host, "files.slack.com", StringComparison.OrdinalIgnoreCase) ||
         string.Equals(uri.Host, "files.slack-gov.com", StringComparison.OrdinalIgnoreCase));

    private static bool IsProviderIdentifier(string? value, char prefix) =>
        !string.IsNullOrWhiteSpace(value) && value!.Length <= 255 && value[0] == prefix &&
        value.All(static character => char.IsLetterOrDigit(character));

    private static SlackFileUploadResult CreateFailure(
        SlackFileUploadRequest upload,
        HttpResponseMessage response,
        string? providerCode,
        string method) {
        var statusCode = (int)response.StatusCode;
        var code = providerCode ?? "invalid_response";
        var errorKind = response.IsSuccessStatusCode
            ? MessageErrorKind.Transient
            : SlackHttpResponseSupport.Classify(statusCode, code);
        return new SlackFileUploadResult {
            IsSuccess = false,
            FileName = upload.FileName,
            ConversationId = upload.ConversationId,
            StatusCode = statusCode,
            ProviderCode = code,
            ErrorKind = errorKind,
            ErrorMessage = $"Slack Web API rejected {method} with '{code}'.",
            CorrelationId = SlackHttpResponseSupport.ReadCorrelationId(response),
            RetryAfter = SlackHttpResponseSupport.ReadRetryAfter(response)
        };
    }

    /// <inheritdoc />
    public void Dispose() {
        if (_disposeHttpClient) {
            _httpClient.Dispose();
        }
    }

    private sealed class UploadTicketResult {
        private UploadTicketResult(bool isSuccess, Uri? uploadUri, string? fileId, SlackFileUploadResult result) {
            IsSuccess = isSuccess;
            UploadUri = uploadUri;
            FileId = fileId;
            Result = result;
        }

        public bool IsSuccess { get; }
        public Uri? UploadUri { get; }
        public string? FileId { get; }
        public SlackFileUploadResult Result { get; }

        public static UploadTicketResult Success(Uri uploadUri, string fileId) =>
            new(true, uploadUri, fileId, new SlackFileUploadResult());

        public static UploadTicketResult Failure(SlackFileUploadResult result) =>
            new(false, null, null, result);
    }

    private sealed class NonDisposingStream : Stream {
        private readonly Stream _inner;

        public NonDisposingStream(Stream inner) {
            _inner = inner;
        }

        public override bool CanRead => _inner.CanRead;
        public override bool CanSeek => _inner.CanSeek;
        public override bool CanWrite => false;
        public override long Length => _inner.Length;
        public override long Position {
            get => _inner.Position;
            set => _inner.Position = value;
        }
        public override void Flush() => _inner.Flush();
        public override int Read(byte[] buffer, int offset, int count) => _inner.Read(buffer, offset, count);
        public override long Seek(long offset, SeekOrigin origin) => _inner.Seek(offset, origin);
        public override void SetLength(long value) => throw new NotSupportedException();
        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
    }
}

internal sealed class SlackExternalUploadResponse {
    public bool Ok { get; private set; }
    public string? Error { get; private set; }
    public Uri? UploadUri { get; private set; }
    public string? FileId { get; private set; }

    public static SlackExternalUploadResponse ParseTicket(string responseBody) => Parse(responseBody, completion: false);

    public static SlackExternalUploadResponse ParseCompletion(string responseBody) => Parse(responseBody, completion: true);

    private static SlackExternalUploadResponse Parse(string responseBody, bool completion) {
        try {
            using var document = JsonDocument.Parse(responseBody);
            var root = document.RootElement;
            if (root.ValueKind != JsonValueKind.Object ||
                !root.TryGetProperty("ok", out var okElement) ||
                okElement.ValueKind is not JsonValueKind.True and not JsonValueKind.False) {
                return new SlackExternalUploadResponse();
            }

            var response = new SlackExternalUploadResponse {
                Ok = okElement.GetBoolean(),
                Error = ReadString(root, "error")
            };
            if (completion) {
                if (root.TryGetProperty("files", out var files) && files.ValueKind == JsonValueKind.Array &&
                    files.GetArrayLength() == 1) {
                    response.FileId = ReadString(files[0], "id");
                }
            } else {
                response.FileId = ReadString(root, "file_id");
                var uploadUrl = ReadString(root, "upload_url");
                if (Uri.TryCreate(uploadUrl, UriKind.Absolute, out var uri)) {
                    response.UploadUri = uri;
                }
            }
            return response;
        }
        catch (JsonException) {
            return new SlackExternalUploadResponse();
        }
    }

    private static string? ReadString(JsonElement element, string name) =>
        element.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.String
            ? value.GetString()
            : null;
}
