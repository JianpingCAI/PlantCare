# NuGet Package Upgrade

## Strategy
Apply the latest compatible stable versions resolved by the full-solution assessment, retaining project-local package references and migrating the Plugin.LocalNotification API where required.

## Preferences
- **Flow Mode**: Automatic
- **Scope**: Full PlantCare solution (`PlantCare.sln`)
- **Package Scope**: Upgrade all direct NuGet package references in the solution
- **Version Policy**: Latest compatible stable versions
- **Framework Constraint**: Preserve .NET 10 and .NET MAUI conventions; do not introduce Xamarin.Forms packages or guidance
- **Validation**: Restore, build the solution, and run applicable tests
- **Commit Strategy**: After Each Task

## Decisions
- **Version reconciliation**: Use the unified assessment version for every package; no divergence or incompatibility was reported.
- **Package management**: Keep existing project-local PackageReference conventions; no Directory.Packages.props file is present.
- **Breaking changes**: Migrate Plugin.LocalNotification 13.0.0 to 14.1.2 APIs using the generated API diff and compiler diagnostics.
- **Packages already current**: Leave package references unchanged when the assessment's latest compatible stable version equals the current version.

## Source Control
- **Source Branch**: master
- **Working Branch**: upgrade-nuget-packages
- **Pending Changes**: Committed before initialization (`Save work before starting nuget-package-upgrade`)
- **Commit Strategy**: After Each Task
- **Branch Sync**: Auto (Merge)
