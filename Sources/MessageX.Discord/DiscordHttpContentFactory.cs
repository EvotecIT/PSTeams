using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace MessageX.Discord;

internal static class DiscordHttpContentFactory {
    public static HttpContent Create(DiscordMessageRequest message, DiscordMessageTarget target) {
        var json = DiscordMessageRenderer.Render(message, target);
        if (message.Attachments.Count == 0) {
            return new StringContent(json, Encoding.UTF8, "application/json");
        }

        var multipart = new MultipartFormDataContent();
        multipart.Add(new StringContent(json, Encoding.UTF8, "application/json"), "payload_json");
        for (var index = 0; index < message.Attachments.Count; index++) {
            var attachment = message.Attachments[index];
            var content = new ByteArrayContent(attachment.Content);
            if (!string.IsNullOrWhiteSpace(attachment.ContentType)) {
                content.Headers.ContentType = MediaTypeHeaderValue.Parse(attachment.ContentType);
            }
            multipart.Add(content, $"files[{index}]", attachment.FileName);
        }
        return multipart;
    }
}
