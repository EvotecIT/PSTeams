# MessageX.Persistence.DbaClientX

Durable MessageX inbox and transactional outbox storage implemented through `DBAClientX.SQLite`.

```csharp
IMessageDurableStore store = new SqliteMessageDurableStore("messagex.db");
await store.InitializeAsync(cancellationToken);
```

The adapter owns MessageX schema and state transitions while DbaClientX owns SQLite connections, transactions, provider setup, cancellation, and diagnostics. It stores only bounded safe payload projections supplied by provider codecs; it must never receive signing material, raw HTTP bodies, response URLs, interaction tokens, SDK contexts, or credentials.
