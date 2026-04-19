# IntelligenceX Reviewer Rollout Proof

This file is a harmless PSTeams change used to verify that the newly installed
IntelligenceX reviewer workflow runs on normal pull requests after the workflow
has landed on `main`.

Expected signal:
- The `IntelligenceX Review` workflow starts on the proof pull request.
- The reviewer posts a sticky summary comment.
- The run uses the repository-local `.intelligencex/reviewer.json` configuration.
