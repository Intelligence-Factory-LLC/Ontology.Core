# Ontology.Core
*A production-ready prototype graph runtime for healthcare and beyond*

> **Buffaly** is a neurosymbolic platform for building large, explainable knowledge graphs and running language-aware inference over them.
> It is powered by **Ontology** (a prototype graph runtime) and **ProtoScript** (an executable language for defining prototypes, rules, and functions).

This repository contains the ontology runtime and its parser/simulation projects.

## Scope
This repository is part of the open-source split of our platform.
It does **not** include several commercial components (partner-only datasets, agentic tooling, and medical extensions).

## ✨ Key capabilities (Ontology runtime)
| Feature | Why it matters |
|---|---|
| 🧬 **Prototype graph** | Typed nodes, property edges, child collections, and multi-parent `TypeOf` inheritance. |
| 🧭 **Graph navigation** | Structural comparison, categorization checks, and path/parameterization primitives. |
| 🧰 **Interop** | Native-value prototypes and reflection utilities for bridging CLR values into the graph. |
| 🧪 **Simulation helpers** | Runtime support for downstream parsers, interpreters, and workflows. |

## What This Repo Owns
1. `Ontology` - core runtime types (`Prototype`, properties, children, inheritance, conversions).
2. `Ontology.Parsers` - parsing utilities for ontology data.
3. `Ontology.Simulation` - simulation/runtime support utilities.

## Solution
- `Ontology.Core.sln`

Current solution membership:
- `Ontology`
- `Ontology.Parsers`
- `Ontology.Simulation`

## 🚀 Quick start (as source)
```csharp
using Ontology;

Initializer.Initialize();

Prototype animal = Prototypes.GetOrInsertPrototype("Animal");
Prototype dog = Prototypes.GetOrInsertPrototype("Dog", "Animal");
Prototype fido = dog.CreateInstance("Fido");

Console.WriteLine(fido.TypeOf(animal)); // True
```

## 🛠 Build
The split repos are designed to sit as **siblings** on disk so `Scripts\update_dlls.bat` can copy DLLs between `..\ontology-core`, `..\protoscript-core`, and `..\buffaly-nlu`.
If you clone into different folder names, update the paths inside `Scripts\update_dlls.bat`.

From repo root:

```powershell
Scripts\update_dlls.bat
dotnet build Ontology.Core.sln
```

## 🧪 Tests
There are currently no repo-local `*Tests*.csproj` projects in this repo.
ProtoScript unit/integration tests live in `ProtoScript.Core` (`ProtoScript.Tests` and `ProtoScript.Tests.Integration`).

## 📦 Dependency model
- Shared binaries are resolved from local `lib\` and `Deploy\`.
- `Scripts\update_dlls.bat` refreshes DLLs used by local builds.

## Related repositories
- Ontology core: https://github.com/Intelligence-Factory-LLC/Ontology.Core
- ProtoScript core: https://github.com/Intelligence-Factory-LLC/ProtoScript.Core
- Buffaly NLU: https://github.com/Intelligence-Factory-LLC/Buffaly.NLU

## 🏥 Need help?
We deploy explainable, neurosymbolic systems in regulated domains (healthcare and beyond).
If you'd like guidance or custom development, email **support@intelligencefactory.ai** or visit **https://intelligencefactory.ai**.

*© 2026 Intelligence Factory, LLC*
