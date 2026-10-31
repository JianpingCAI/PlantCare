# NuGet Package Upgrade Plan

## Overview

**Target**: Upgrade all direct NuGet package references in the PlantCare solution to the latest compatible stable versions while preserving the existing .NET 10 and .NET MAUI conventions.
**Scope**: Three projects in the full `PlantCare.sln` solution, including the MAUI app, data project, and test project.

## Tasks

### 01-upgrade-package-references: Apply compatible stable package versions

Update the direct package references across `PlantCare.App`, `PlantCare.Data`, and `PlantCare.App.Tests` using the assessment's resolved stable versions. Most packages are already current; the material version change is `Plugin.LocalNotification` 13.0.0 to 14.1.2 in the app and test projects. Preserve project-local package management because no central package management file is present.

**Done when**: Every scoped package reference matches the assessed compatible stable version, with no Xamarin.Forms packages introduced and project files retaining their existing conventions.

---

### 02-migrate-local-notification-api: Fix notification API changes

Adapt the PlantCare notification abstractions and callers to `Plugin.LocalNotification` 14.1.2. Use the generated API diff at `apidiff/Plugin.LocalNotification.apidiff.md` and compiler diagnostics to replace removed notification types and members, update changed model namespaces and signatures, and preserve the existing notification behavior.

**Done when**: The app and test projects compile against `Plugin.LocalNotification` 14.1.2 without notification-related errors, and no obsolete v13 notification types or members remain in active code.

---

### 03-validate-solution: Restore, build, and run tests

Restore and build the full solution, treating warnings as errors for the validation decision. Run all applicable tests discovered in `PlantCare.App.Tests`, investigate failures caused by the package upgrade, and confirm the final package graph and source-control diff.

**Done when**: The full solution restores and builds successfully, applicable tests pass, and the final report identifies changed packages and any packages that were already current.
