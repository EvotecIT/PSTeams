using System.Net;
using System.Net.Http;

namespace MessageX.Tests;

internal sealed class ThrowingResponseStreamHandler : HttpMessageHandler {
    private readonly bool _throwAfterCancellation;

    public ThrowingResponseStreamHandler(bool throwAfterCancellation = false) {
        _throwAfterCancellation = throwAfterCancellation;
    }

    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken) {
        return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) {
            Content = new StreamContent(new ThrowingStream(_throwAfterCancellation))
        });
    }

    private sealed class ThrowingStream : Stream {
        private readonly bool _throwAfterCancellation;

        public ThrowingStream(bool throwAfterCancellation) {
            _throwAfterCancellation = throwAfterCancellation;
        }

        public override bool CanRead => true;
        public override bool CanSeek => false;
        public override bool CanWrite => false;
        public override long Length => throw new NotSupportedException();
        public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }

        public override int Read(byte[] buffer, int offset, int count) {
            throw new IOException("Response stream failed after headers were received.");
        }

        public override async Task<int> ReadAsync(
            byte[] buffer,
            int offset,
            int count,
            CancellationToken cancellationToken) {
            if (_throwAfterCancellation) {
                try {
                    await Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken);
                } catch (OperationCanceledException) {
                    throw new IOException("Response stream failed for https://example.test/workflows/secret-token.");
                }
            }

            throw new IOException("Response stream failed after headers were received.");
        }

        public override void Flush() { }
        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
        public override void SetLength(long value) => throw new NotSupportedException();
        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
    }
}
