# PLAN

## Main Branch Direction

This branch keeps `PSTeams` as the product surface while migrating implementation to `TeamsX`.

The public shape on `main` is:

- `TeamsX` as the reusable .NET library
- `TeamsX.PowerShell` as thin binary cmdlets
- `Module\PSTeams` as the shipping PowerShell module layout

The old `PSTeams` function surface is the migration contract on `main`: commands remain script-based until equivalent C# cmdlets exist.

## Current Goals

1. keep webhook and workflow webhook sending solid
2. expand adaptive-card composition through typed C# models and cmdlets
3. add payload preview and validation tooling for development workflows
4. use the standard `Build\Build-Project.ps1` flow for the library/repository build
5. use `Module\Build\Build-Module.ps1` for the PowerShell module packaging path
6. introduce Graph channel/chat senders as the next delivery backends
7. keep the public module as a hybrid shell until each legacy command is fully converted and tested
