# MessageX.Hosting.AspNetCore

Provider-neutral ASP.NET Core hosting primitives for MessageX receivers.

The package reads exact bounded request bodies, writes provider acknowledgements, and hands verified typed envelopes to a bounded background-dispatch queue. It deliberately does not parse provider payloads, store raw requests, or provide durable delivery guarantees.

```csharp
builder.Services.AddMessageXHostingAspNetCore(options => {
    options.QueueCapacity = 256;
    options.MaximumRequestBodyBytes = 1024 * 1024;
});
```

Use a provider endpoint package to verify Slack or Discord requests and enqueue only verified dispatch-ready envelopes. Microsoft Teams hosting continues to use the Microsoft Teams SDK adapter.
