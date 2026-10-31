# Progress Details

This task's API migration was completed during task `01-upgrade-package-references` because the required post-package-update build immediately surfaced the v14 breaking changes.

- Updated notification model imports to `Plugin.LocalNotification.Core.Models`.
- Updated notification permission checks and requests to pass `NotificationPermission` explicitly.
- Removed obsolete `AndroidNotificationPermission` and `AndroidOptions.DefaultGroupId` usage.
- Verified with a successful targeted `PlantCare.App` build and successful full `PlantCare.sln` build.
- No notification-related compiler errors remain.
