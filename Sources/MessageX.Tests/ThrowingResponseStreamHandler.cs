using System.Net;
using System.Net.Http;

namespace MessageX.Tests;

internal sealed class ThrowingResponseStreamHandler : HttpMessageHandler {
    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken) {
        return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) {
            Content = new StreamContent(new ThrowingStream())
        });
    }

    private sealed class ThrowingStream : Stream {
        public override bool CanRead => true;
        public override bool CanSeek => false;
        public override bool CanWrite => false;
        public override long Length => throw new NotSupportedException();
        public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }

        public override int Read(byte[] buffer, int offset, int count) {
            throw new IOException("Response stream failed after headers were received.");
        }

        public override Task<int> ReadAsync(
            byte[] buffer,
            int offset,
            int count,
            CancellationToken cancellationToken) {
            return Task.FromException<int>(new IOException("Response stream failed after headers were received."));
        }

        public override void Flush() { }
        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
        public override void SetLength(long value) => throw new NotSupportedException();
        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
    }
}
