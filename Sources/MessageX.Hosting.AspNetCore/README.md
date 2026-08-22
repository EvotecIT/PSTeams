# MessageX.Hosting.AspNetCore

Provider-neutral ASP.NET Core hosting primitives for MessageX receivers.

The package reads exact bounded request bodies, writes provider acknowledgements, and hands verified typed envelopes to a bounded background-dispatch queue. It deliberately does not parse provider payloads or store raw requests.

```csharp
builder.Services.AddMessageXHostingAspNetCore(options => {
    options.QueueCapacity = 256;
    options.MaximumRequestBodyBytes = 1024 * 1024;
    options.ReplayCacheCapacity = 65536;
    options.ReplayRetention = TimeSpan.FromHours(1);
});
```

Use a provider endpoint package to verify Slack or Discord requests and enqueue only verified dispatch-ready envelopes. Microsoft Teams hosting continues to use the Microsoft Teams SDK adapter.

The default ingress is intentionally volatile but suppresses accepted provider deduplication coordinates for a bounded retention window. It fails closed when that replay cache is full. A host that needs provider success only after durable acceptance can register an `IMessageDurableStore`, call `AddMessageXDurableIngress`, and register one `IMessageDurableCodec<TProviderPayload>` for every accepted payload type. The codec is the security boundary that persists a bounded safe projection and reconstructs it after restart; transient tokens, response URLs, signing material, raw requests, and SDK contexts must not enter that projection.

Durable workers renew every claimed lease while a handler is running, cancel cooperative handlers when ownership is lost, commit handler-produced `MessageOutboxRecord` instances with inbox completion, and deliver them through registered `IMessageOutboxHandler` owners. Missing codecs or unavailable storage return HTTP 503 before a provider success acknowledgement is written.
