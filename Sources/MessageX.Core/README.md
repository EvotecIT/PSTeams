# MessageX.Core

MessageX.Core contains the small provider-neutral contracts shared by MessageX provider packages. It does not install Teams, Slack, Discord, hosting, or persistence clients.

Use it for:

- safe-to-persist message and conversation references;
- provider capability flags;
- shared delivery-result state;
- classified provider and transport errors;
- focused typed sender contracts.

Provider-native message bodies, targets, authentication, and protocol behavior stay in provider packages such as `MessageX.Teams`.
