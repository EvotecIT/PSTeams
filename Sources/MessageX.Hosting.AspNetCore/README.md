# MessageX.Hosting.AspNetCore

Provider-neutral ASP.NET Core hosting primitives for MessageX receivers.

The package reads exact bounded request bodies, writes provider acknowledgements, and hands verified typed envelopes to a bounded background-dispatch queue. It deliberately does not parse provider payloads or store raw requests.

```csharp
builder.Services.AddMessageXHostingAspNetCore(options => {
    options.QueueCapacity = 256;
    options.MaximumRequestBodyBytes = 1024 * 1024;
    options.ReplayCapacity = 65536;
    options.ReplayRetention = TimeSpan.FromHours(1);
});
```

Use a provider endpoint package to verify Slack or Discord requests and enqueue only verified dispatch-ready envelopes. Microsoft Teams hosting continues to use the Microsoft Teams SDK adapter.

The default ingress is intentionally volatile but suppresses accepted provider deduplication coordinates for a bounded retention window. It fails closed when that replay cache is full. A host that needs provider success only after durable acceptance for asynchronously dispatched work can register an `IMessageDurableStore`, call `AddMessageXDurableIngress`, and register one `IMessageDurableCodec<TProviderPayload>` for every accepted payload type. The codec is the security boundary that persists a bounded safe projection and reconstructs it after restart; transient tokens, response URLs, signing material, raw requests, and SDK contexts must not enter that projection.

Provider operations whose acknowledgement is produced by the handler are an explicit exception: they must run inline on the original request, use bounded process-local replay protection, and are not persisted or replayed after a host restart. Discord autocomplete, Slack modal view submissions, and Teams Adaptive Card invoke responses select this automatically. Register a command or action with `MessageDispatchMode.Synchronous` when its handler must open a Slack or Discord modal or otherwise consume a short-lived capability before acknowledgement. Keep ordinary routes deferred, and use provider retry behavior plus idempotent handlers for synchronous operations.

Completed synchronous responses share the host-wide `ReplayAcknowledgementBodyBytes` memory budget and are released when `ReplayRetention` expires, even when no later request arrives. If the body budget is full, the original response still succeeds while duplicates receive a retryable response and a later provider attempt may dispatch again.

```csharp
builder.Services.AddMessageXDurableIngress(options => {
    options.TerminalRetention = TimeSpan.FromDays(7);
    options.CleanupInterval = TimeSpan.FromHours(1);
    options.CleanupBatchSize = 1000;
});
```

Terminal cleanup removes completed and dead-lettered records only after `TerminalRetention`; pending, leased, and inbox records with retained outbound work are preserved. The retention period is also the durable deduplication window, so size it for provider retry behavior and the operator-inspection period required for dead letters.

Durable workers renew every claimed lease while a handler is running, release route-unmatched records for another capable worker, cancel cooperative handlers when ownership is lost, commit handler-produced `MessageOutboxRecord` instances with inbox completion, and deliver them through registered `IMessageOutboxHandler` owners. Missing codecs or unavailable storage return HTTP 503 before a provider success acknowledgement is written.
