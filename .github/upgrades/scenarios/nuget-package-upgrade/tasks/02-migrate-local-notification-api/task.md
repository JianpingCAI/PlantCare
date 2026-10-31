# 02-migrate-local-notification-api: Fix notification API changes

Adapt the PlantCare notification abstractions and callers to `Plugin.LocalNotification` 14.1.2. Use the generated API diff at `apidiff/Plugin.LocalNotification.apidiff.md` and compiler diagnostics to replace removed notification types and members, update changed model namespaces and signatures, and preserve the existing notification behavior.

## Research Findings

- The migration was completed while satisfying the post-package-update build gate in task `01-upgrade-package-references`.
- `INotificationService.cs` and `NotificationService.cs` now import `Plugin.LocalNotification.Core.Models`; `AreNotificationsEnabled` and parameterless permission requests construct an explicit `NotificationPermission` model required by v14.
- `PlantListOverviewViewModel.cs` now uses the v14 core model namespace and no longer references removed `AndroidNotificationPermission` or `AndroidOptions.DefaultGroupId` types.
- The targeted `PlantCare.App` build and full `PlantCare.sln` build both passed after these changes, with no remaining notification-related compiler diagnostics.

**Done when**: The app and test projects compile against `Plugin.LocalNotification` 14.1.2 without notification-related errors, and no obsolete v13 notification types or members remain in active code.
