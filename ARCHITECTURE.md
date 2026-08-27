# MessageX architecture

## Ownership

MessageX is a set of provider-native libraries connected by a small shared operational core.

| Owner | Responsibility | Does not own |
| --- | --- | --- |
| `MessageX.Core` | Results, references, capabilities, errors, bounded data, HTTP policy | Provider payloads or routing |
| `MessageX.Teams` | Webhook messages and Teams card models | Microsoft Graph administration |
| `MessageX.Slack` | Webhooks, Web API, Block Kit, files, interaction continuation | OAuth installation hosting or Socket Mode |
| `MessageX.Discord` | Webhooks, bot REST, embeds, attachments, components, interaction continuation | Gateway sessions |
| `MessageX.Hosting` | Routes, acknowledgements, dispatch, replay, retries, durable records | Provider authentication details |
| Provider hosting packages | Request verification and native-to-shared projection | General queue or persistence engines |
| `MessageX.Persistence.DbaClientX` | MessageX schema and DbaClientX adapter | SQL/SQLite provider infrastructure |
| `MessageX.PowerShell` | Parameters, pipeline behavior, `ShouldProcess`, typed output | Provider protocol behavior |

GraphEssentialsX owns Microsoft Graph authentication, paging, Teams collaboration administration, and governed writes. PowerForge/PSPublishModule owns build, packaging, signing, and publication behavior.

## Data boundaries

Durable records may contain routing coordinates, provider event identifiers, bounded provider data, and non-secret installation identifiers. They must not contain:

- Slack response URLs or trigger IDs;
- Discord interaction tokens;
- Teams, Slack, or Discord webhook URLs;
- bot tokens, signing secrets, private keys, authorization headers, or refresh tokens;
- unbounded raw request or response bodies.

Verified receive adapters create transient contexts for operations that must happen during the current process lifetime. Durable codecs replace those contexts with an explicitly unavailable value.

## Delivery flow

```text
PowerShell / C# consumer
        |
        v
provider-native request and target
        |
        v
provider sender or lifecycle client
        |
        v
bounded HTTP transport -> typed result + durable reference
```

Inbound hosting uses a separate flow:

```text
raw provider request
        |
        v
verify signature/token before dispatch
        |
        v
provider-native event + shared route/envelope
        |
        +--> synchronous response when the provider contract requires it
        |
        `--> bounded queue / durable store / retry / dead letter
```

## Rich-content rule

Adaptive Cards, Slack Block Kit, and Discord components evolve independently. Shared abstractions cover operational semantics only; they do not pretend that provider documents are interchangeable. Product adapters map domain notifications into the selected provider model.

## Runtime rule

The reusable provider and PowerShell libraries retain .NET Framework 4.7.2 compatibility for Windows PowerShell 5.1 while also targeting .NET 8 and .NET 10. ASP.NET Core and persistence packages target modern .NET only. New APIs must compile warning-free on every target declared by their owning project.

The `0.1.0` provider packages are not marked trim-safe or Native AOT compatible. Their provider JSON renderers currently use reflection-based `System.Text.Json` metadata. A source-generated serialization context and a runnable trimmed/AOT consumer are required before either capability can be advertised.

## Dependency rule

Consumers reference the smallest required package. Provider packages do not reference each other. Hosting is split so notification-only consumers do not inherit ASP.NET Core or persistence dependencies. Database-provider behavior routes through DbaClientX.

## Release-state rule

Local source, staged packages, public packages, and installed modules are separate states. A source build does not prove package contents; a staged package does not prove public availability; a public package does not prove a consumer has upgraded.
