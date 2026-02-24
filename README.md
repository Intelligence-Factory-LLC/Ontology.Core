# Ontology.Core

Core ontology runtime and parser projects used by the split Buffaly stack.

## What This Repo Owns
- `Ontology`: Core prototype/ontology model runtime.
- `Ontology.Parsers`: Parsing layer for ontology data.
- `Ontology.Simulation`: Simulation/runtime support utilities.

## Solution
- `Ontology.Core.sln`

Current solution membership:
- `Ontology`
- `Ontology.Parsers`
- `Ontology.Simulation`

## Build
From repo root:

```powershell
Scripts\update_dlls.bat
dotnet build Ontology.Core.sln
```

## Tests
There are currently no repo-local `*Tests*.csproj` projects in this repo.

Note: ProtoScript unit/integration tests were moved to `ProtoScript.Core` (`ProtoScript.Tests` and `ProtoScript.Tests.Integration`).

## Dependency Model
- Shared binaries are resolved from local `lib\` and `Deploy\`.
- `Scripts\update_dlls.bat` refreshes dependencies used by local builds.

## Related Repositories
- Ontology core: https://github.com/Intelligence-Factory-LLC/Ontology.Core
- ProtoScript core: https://github.com/Intelligence-Factory-LLC/ProtoScript.Core
- Buffaly NLU: https://github.com/Intelligence-Factory-LLC/Buffaly.NLU
