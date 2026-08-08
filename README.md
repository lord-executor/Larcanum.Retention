[![GitHub](https://img.shields.io/github/license/mashape/apistatus.svg)](https://github.com/lord-executor/Larcanum.Retention/blob/main/LICENSE)
[![Build](https://github.com/lord-executor/Larcanum.Retention/actions/workflows/build.yml/badge.svg)](https://github.com/lord-executor/Larcanum.Retention/actions/workflows/build.yml)
[![Nuget](https://img.shields.io/nuget/v/Larcanum.Retention.svg)](https://www.nuget.org/packages/Larcanum.Retention)

# Overview

`Larcanum.Retention` implements retention/rotation policies for timestamped things like backups or versioned
artifacts, following the classic "Grandfather-Father-Son" (GFS) rotation scheme — e.g. "keep the last 7 daily
backups, the last 4 weekly backups, and the last 12 monthly backups". Given a set of candidates and a set of
policies, it tells you which candidates to **retain** and which to **prune**.

The core concepts:

- **`RetentionCandidate<T>`** — any timestamped item you want to evaluate: your own item plus the `DateTimeOffset`
  it should be judged by.
- **`RetentionPolicy`** — a single rule of the form "within period P, keep one item per K", expressed as a
  *period interval*, a *keep interval*, and an *alignment* (keep the newest or the oldest item of each K). For
  example, "within the trailing 7 days, keep one per day" or "within the trailing 12 months, keep one per month".
- **`RetentionStrategy<T>`** — evaluates one or more policies against a set of candidates and splits them into a
  `RetentionResult<T>` with `Retain` and `Prune` lists. Multiple policies are typically combined to build a full
  GFS scheme (daily + weekly + monthly + yearly).

## Interval alignment: why the result doesn't drift

The most important thing to understand about this library is that intervals are **calendar-aligned, not
duration-aligned**. A "day" is not "the last 24 hours from right now" — it is a fixed slice of the calendar that
always starts at midnight. Likewise, a "week" always starts on Monday, a "month" always starts on the 1st, and a
year always starts on January 1st.

When a policy is evaluated, its period and keep boundaries are snapped to these fixed calendar edges relative to
a reference point (`RetentionStrategyOptions.StartPoint`, which defaults to "now"), rather than being measured as
exact durations back from that instant. This is what makes the result **stable**: evaluating a "keep 1 weekly
backup" policy on Monday morning versus evaluating it again on Wednesday evening of the *same* week produces the
same Monday-to-Monday segment, and therefore retains the same backup — the boundary doesn't shift just because you
happened to run the evaluation at a different moment.

That stability matters because retention is normally evaluated over and over as time passes and new candidates
appear. Because segments are pinned to fixed calendar points instead of "now minus X", repeated evaluations
converge on a consistent set of retained items instead of slowly drifting as the reference point moves forward one
run at a time.

A few other details that follow from this model:

- Any candidate newer than `StartPoint` is always retained unconditionally — it hasn't aged into any policy's
  window yet.
- `RetentionStrategyOptions.AllowOverlap` controls whether the same candidate can satisfy more than one policy at
  once. With `AllowOverlap = false`, a candidate already claimed by one policy is excluded when evaluating the
  next, so overlapping policies (e.g. a daily and a weekly rule covering the same days) don't count the same item
  twice.

# How to Use It

Policies can be built programmatically...

```csharp
using Larcanum.Retention;

var policy = new RetentionPolicy(
    periodInterval: new DailyInterval(7),
    keepInterval: new DailyInterval(1),
    alignment: RetentionAlignment.Newest);
```

...or parsed from a compact string syntax, which is often more convenient for configuration:

```csharp
// "keep 1 per day for 7 days, 1 per week for 4 weeks, 1 per month for 12 months"
var policies = RetentionPolicy.Parse("7D:1D:N,4W:1W:N,12M:1M:N");
```

Each policy is `{count}{unit}:{count}{unit}:{alignment}`, where `unit` is one of `D`/`W`/`M`/`Y` (day, week, month,
year) and `alignment` is `N` (keep the newest item of each segment) or `O` (keep the oldest). Multiple policies are
separated by commas.

Evaluate a policy set against your candidates to get back what to keep and what to discard:

```csharp
var candidates = backups.Select(b => new RetentionCandidate<Backup>(b, b.CreatedAt));

var strategy = new RetentionStrategy<Backup>(policies);
var result = strategy.Evaluate(candidates);

foreach (var candidate in result.Prune)
{
    DeleteBackup(candidate.Item);
}
```

`RetentionStrategy<T>` also accepts a `RetentionStrategyOptions` object to control the reference point the policies are
evaluated against (useful for deterministic, reproducible evaluation in tests) and whether policies are allowed to
overlap:

```csharp
var strategy = new RetentionStrategy<Backup>(policies, new RetentionStrategyOptions
{
    StartPoint = DateTimeOffset.UtcNow,
    AllowOverlap = false
});
```

# Development

Requires the .NET 10 SDK.

```
dotnet restore src/Larcanum.Retention.slnx
dotnet build --no-restore src/Larcanum.Retention.slnx
dotnet test --no-build --verbosity normal src/Larcanum.Retention.slnx
```

Tests use [TUnit](https://github.com/thomhurst/TUnit) on Microsoft.Testing.Platform (rather than the classic
xUnit/NUnit + VSTest combo) with [AwesomeAssertions](https://github.com/AwesomeAssertions/AwesomeAssertions) for
`.Should()`-style assertions.

## Release Process

The package version is derived from `git` tags via [`Larcanum.GitInfo`](https://www.nuget.org/packages/Larcanum.GitInfo) —
there is no version number to bump by hand in the `.csproj`.

To cut a release, push an annotated tag matching `vX.Y.Z` (semantic versioning) to `main`:

```
git tag -a vX.Y.Z -m "vX.Y.Z"
git push origin vX.Y.Z
```

Pushing the tag triggers `.github/workflows/publish.yml`, which packs `src/Retention/Retention.csproj` with the
version extracted from the tag and pushes the resulting package to nuget.org.

Publishing uses NuGet's [Trusted Publishing](https://learn.microsoft.com/en-us/nuget/nuget-org/trusted-publishing) —
the workflow exchanges a short-lived GitHub OIDC token for a temporary NuGet API key at push time, so there is no
long-lived `NUGET_API_KEY` secret to manage or rotate.
