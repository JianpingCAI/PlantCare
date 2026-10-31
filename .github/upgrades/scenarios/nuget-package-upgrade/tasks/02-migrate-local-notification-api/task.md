# 02-migrate-local-notification-api: Fix notification API changes

Adapt the PlantCare notification abstractions and callers to `Plugin.LocalNotification` 14.1.2. Use the generated API diff at `apidiff/Plugin.LocalNotification.apidiff.md` and compiler diagnostics to replace removed notification types and members, update changed model namespaces and signatures, and preserve the existing notification behavior.

**Done when**: The app and test projects compile against `Plugin.LocalNotification` 14.1.2 without notification-related errors, and no obsolete v13 notification types or members remain in active code.
