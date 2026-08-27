# MessageX.Hosting

Host-neutral routing and handler contracts for verified inbound Teams, Slack, and Discord events. Provider receivers and ASP.NET Core adapters remain separate layers so applications can reuse routing without taking a web-host dependency.

Handlers are deferred by default. Register `MessageDispatchMode.Synchronous` only when a route must produce the initial provider acknowledgement or consume a short-lived capability such as a Slack modal trigger.
