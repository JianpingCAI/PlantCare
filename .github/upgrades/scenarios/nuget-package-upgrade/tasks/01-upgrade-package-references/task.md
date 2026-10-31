# 01-upgrade-package-references: Apply compatible stable package versions

Update the direct package references across `PlantCare.App`, `PlantCare.Data`, and `PlantCare.App.Tests` using the assessment's resolved stable versions. Most packages are already current; the material version change is `Plugin.LocalNotification` 13.0.0 to 14.1.2 in the app and test projects. Preserve project-local package management because no central package management file is present.

## Research Findings

- **Projects in scope**: `PlantCare.App/PlantCare.App.csproj`, `PlantCare.Data/PlantCare.Data.csproj`, and `PlantCare.App.Tests/PlantCare.App.Tests.csproj` from the full `PlantCare.sln`.
- **Package management**: Every package is defined directly in its project file; no `Directory.Packages.props` or central package file is present. Existing `PackageReference Include=... Version=...` formatting was preserved.
- **Resolved versions**: The quick assessment reports unified latest stable versions for every package. All current references already matched the target except `Plugin.LocalNotification`, updated from `13.0.0` to `14.1.2` in the app and test projects.
- **Framework constraints**: `PlantCare.App` targets .NET 10 MAUI TFMs; `PlantCare.Data` and tests target `net10.0`. No Xamarin.Forms references were present or introduced.
- **API migration**: The v14 build diagnostics required `Plugin.LocalNotification.Core.Models` for `NotificationRequest` and `NotificationPermission`, permission calls with an explicit permission model, and removal of the deleted `AndroidOptions.DefaultGroupId` API. These changes are applied in `INotificationService.cs`, `NotificationService.cs`, and `PlantListOverviewViewModel.cs`.
- **Initial validation**: The first post-package-update build reported CS0246 for the removed v13 model types and CS0103 for `AndroidOptions`; those errors were addressed. The initial Android XARDF7024 directory-cleanup error was environmental and separate from the C# API failures.

## Execution Note

The package update immediately exposed the planned Plugin.LocalNotification API migration. To satisfy the required post-task full-solution build invariant, this task includes that directly related migration before completion; the next planned API task will be treated as already completed if no further API work remains.

**Done when**: Every scoped package reference matches the assessed compatible stable version, the directly exposed Plugin.LocalNotification API usages compile against 14.1.2, no Xamarin.Forms packages are introduced, and project files retain their existing conventions.
