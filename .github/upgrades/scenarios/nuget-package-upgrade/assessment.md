# NuGet package upgrade assessment

_Mode: **quick assessment** — package API diffs only; no per-project source scan was run._

## Recommended versions

- **AutoMapper**: **16.2.0** (unified across 4 project(s)).
- **CommunityToolkit.Maui**: **15.0.1** (unified across 2 project(s)).
- **CommunityToolkit.Maui.Core**: **15.0.1** (unified across 1 project(s)).
- **CommunityToolkit.Mvvm**: **8.4.2** (unified across 2 project(s)).
- **coverlet.collector**: **10.1.0** (unified across 1 project(s)).
- **LiveChartsCore.SkiaSharpView.Maui**: **2.0.5** (unified across 1 project(s)).
- **Microsoft.EntityFrameworkCore.InMemory**: **10.0.14** (unified across 1 project(s)).
- **Microsoft.EntityFrameworkCore.Proxies**: **10.0.14** (unified across 1 project(s)).
- **Microsoft.EntityFrameworkCore.Sqlite**: **10.0.14** (unified across 1 project(s)).
- **Microsoft.EntityFrameworkCore.Tools**: **10.0.14** (unified across 1 project(s)).
- **Microsoft.Extensions.DependencyInjection**: **10.0.14** (unified across 1 project(s)).
- **Microsoft.Extensions.Hosting.Abstractions**: **10.0.14** (unified across 1 project(s)).
- **Microsoft.Extensions.Logging.Debug**: **10.0.14** (unified across 1 project(s)).
- **Microsoft.Maui.Controls**: **10.0.120** (unified across 1 project(s)).
- **Microsoft.Maui.Controls.Compatibility**: **10.0.120** (unified across 1 project(s)).
- **Microsoft.NET.Test.Sdk**: **18.11.0** (unified across 1 project(s)).
- **Moq**: **4.21.0** (unified across 1 project(s)).
- **Plugin.LocalNotification**: **14.1.2** (unified across 2 project(s)).
- **Plugin.Maui.Calendar**: **3.1.0** (unified across 1 project(s)).
- **Serilog**: **4.4.0** (unified across 1 project(s)).
- **Serilog.Extensions.Logging**: **10.0.0** (unified across 1 project(s)).
- **Serilog.Sinks.File**: **7.0.0** (unified across 1 project(s)).
- **SkiaSharp.Views.Maui.Controls**: **4.154.0** (unified across 1 project(s)).
- **System.Drawing.Common**: **10.0.14** (unified across 1 project(s)).
- **System.Security.Cryptography.ProtectedData**: **10.0.14** (unified across 1 project(s)).
- **xunit**: **2.9.3** (unified across 1 project(s)).
- **xunit.runner.visualstudio**: **4.0.1** (unified across 1 project(s)).

## Public API changes

> **Types moved (namespace changed) are not removals.** A moved type keeps its name and members;
> the fix is a `using`-directive change, not a rewrite. Do not treat a moved type as deleted.

- **Plugin.LocalNotification**: 39 type(s) removed, 17 member(s) removed, 6 signature(s) changed — see [`apidiff/Plugin.LocalNotification.apidiff.md`](apidiff/Plugin.LocalNotification.apidiff.md).

## Breaking-change findings

- Version divergence findings (Pkg.0003): 0
- Requested-version-unsupported findings (Pkg.0002): 0

- Quick mode does not scan source, so there are no per-line `PkgApi` usage findings. Review the
  per-package API diffs above and rely on build errors during execution to pinpoint affected code.
- A full code scan can locate the exact source location of every breaking-change usage across the repo.
  It is opt-in and slower — re-run the assessment with `fullScan=true` only if the user requests it.

## Next steps

1. Proceed to planning to triage the API changes above and plan the code fixes (if any).

