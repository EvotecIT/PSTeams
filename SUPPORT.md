# MessageX support policy

## Candidate support matrix

| Surface | Supported in the current source candidate |
| --- | --- |
| Windows PowerShell | 5.1 through .NET Framework 4.7.2 binary cmdlets |
| PowerShell | Supported PowerShell 7 runtimes on Windows, Linux, and macOS through modern binaries |
| .NET | .NET Framework 4.7.2, .NET 8, and .NET 10 where declared by the package |
| Trimming / Native AOT | Not supported in the `0.1.0` candidate; use the standard JIT runtime |
| Teams notification | Official incoming-webhook and Workflow URLs |
| Teams application receive | Verified ASP.NET Core activity/card-action endpoint |
| Slack notification | Official incoming webhook and bot Web API |
| Slack receive | Signed HTTP Events API and Interactivity requests |
| Discord notification | Official incoming webhook and bot REST API v10 |
| Discord receive | Signed HTTP interactions |

The source candidate is not a promise that the same features exist in the currently published PSTeams module.

Native AOT toolchain smoke tests pass, but a provider-serialization consumer emits trim/AOT warnings and fails when reflection-based JSON serialization is disabled. MessageX will not set `IsTrimmable` or `IsAotCompatible` until source-generated JSON metadata and an executable consumer test pass.

## Required credentials and permissions

- Treat every webhook URL as a credential even when it looks like an ordinary URI.
- Slack bot file upload requires `files:write`; message and interaction permissions depend on the operations enabled for the installation.
- Discord bot permissions and intents are installation-specific. HTTP interactions do not require a Gateway connection.
- Teams Workflow capabilities depend on how the flow was configured. The URL itself remains send-only.

Use the least privilege that satisfies the selected feature. Keep credentials in a secret store or process environment and rotate any value that was disclosed in logs, scripts, issues, or commits.

## Supported receive transports

Verified HTTP receive is the supported preview transport. Slack Socket Mode and the Discord Gateway are deferred until reconnect, heartbeat, resume, shutdown, and health behavior have dedicated tests and operational proof.

## Security reports

Do not open a public issue containing tokens, webhook URLs, private keys, raw authorization headers, or exploitable tenant data. Use the repository's private security-reporting channel when available.

## Bug reports

Useful reports include:

- the MessageX package or PSTeams module version and its source;
- runtime and operating system;
- provider operation and safe target type;
- classified error, HTTP status, provider code, correlation ID, and retry delay;
- a minimal redacted request model or reproduction;
- whether the behavior occurs in source, staged artifact, public package, or installed module.

Do not include raw response bodies unless they have been reviewed and redacted.

## Compatibility

Provider identifiers and durable message references are opaque. Do not parse them in consumers. Public breaking changes may occur during `0.x` previews and will be documented. Existing PSTeams names are retained when their behavior remains supported; obsolete protocol behavior is not preserved merely to keep dead code callable.
