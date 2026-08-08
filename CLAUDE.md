# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project

`Larcanum.Retention` is a .NET 10 (net10.0) class library implementing backup/artifact retention policy logic (GFS-style rotation, e.g. "keep 1 daily for 7 days, 1 weekly for 4 weeks"). It's a pure domain library — no web server, no CLI, no database, no I/O.

Solution layout (`src/Larcanum.Retention.slnx` — note the newer XML `.slnx` format, not a classic `.sln`):
- `src/Retention/` — the library (package `Larcanum.Retention`)
- `src/Retention.UnitTests/` — tests

## Build & test

```
dotnet restore src/Larcanum.Retention.slnx
dotnet build --no-restore src/Larcanum.Retention.slnx
dotnet test --no-build --verbosity normal src/Larcanum.Retention.slnx
```

The test project uses **TUnit** on **Microsoft.Testing.Platform** (set via `global.json`), not the classic xUnit/NUnit + VSTest combo — `dotnet test` runs through MTP, so IDE/tooling test discovery can differ from a standard VSTest setup. Assertions use **AwesomeAssertions** (`.Should()` syntax, a FluentAssertions-API-compatible package — not the `FluentAssertions` package itself).

Packaging only targets the library, not the test project:
```
dotnet pack --configuration Release --output ./artifacts src/Retention/Retention.csproj
```

## Code style

- File-scoped namespaces, `Nullable` enabled in both projects.
- This repo uses **C# 14 extension member blocks** (`extension(Type x) { public ... }` syntax, see `DateTimeExtensions.cs`) rather than legacy `this`-parameter extension methods — this is intentional given the net10.0/C# 14 target; don't "correct" it to the older syntax.
- Prefer `[GeneratedRegex]` source-generated regex over `new Regex(...)`.
- Primary constructors are not used for classes with logic — stick to explicit constructors to match existing style.

## Other notes

- `src/Larcanum.Retention.snk` (strong-name key) is intentionally committed to git — the `.gitignore` exclusion for `*.snk` is deliberately commented out. Don't flag or remove it as a stray secret.
- Work happens on feature branches merged via PR, not direct commits to `main`.
- **Never run `git commit` (or `git push`) unless explicitly instructed to do so in that conversation.** Reviewing and committing changes is the user's job — leave changes staged/unstaged for them to review and commit themselves.
