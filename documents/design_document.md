# Design Document

## 1. System Architecture

- **Frontend**: Built using Flutter, providing a cross-platform UI.
- **Backend**: RESTful API for data management and integration with third-party services for plant information.
- **Database**: Use Firebase Firestore for real-time data storage and synchronization.

## 2. UI/UX Design

- **Main Screens**:
  - **Home Screen**: Overview of all plants and their statuses.
  - **Plant Detail Screen**: Detailed view of individual plant information.
  - **Reminder Screen**: List of upcoming care tasks.
  - **Settings Screen**: App settings and preferences.

## 3. Data Models

- **User**:
  - `userId`: String
  - `username`: String
  - `email`: String
  - `passwordHash`: String
- **Plant**:
  - `plantId`: String
  - `userId`: String
  - `name`: String
  - `species`: String
  - `age`: Int
  - `lastWatered`: DateTime
  - `notes`: List<String>
- **Reminder**:
  - `reminderId`: String
  - `plantId`: String
  - `type`: String (e.g., watering, fertilizing)
  - `date`: DateTime

## 4. Component Diagram

- **UI Components**:
  - `LoginComponent`
  - `SignUpComponent`
  - `PlantListComponent`
  - `PlantDetailComponent`
  - `ReminderComponent`
  - `SettingsComponent`
- **Services**:
  - `AuthService`
  - `PlantService`
  - `ReminderService`
  - `NotificationService`
- **External APIs**:
  - Plant information API for fetching care tips.

## 5. Sequence Diagrams

- **User Login**:
  1. User enters credentials.
  2. App sends credentials to AuthService.
  3. AuthService verifies credentials.
  4. AuthService returns success or failure response.
  5. App navigates to Home Screen on success.

- **Add Plant**:
  1. User navigates to Add Plant Screen.
  2. User enters plant details.
  3. App sends plant data to PlantService.
  4. PlantService saves data to Firestore.
  5. App updates PlantListComponent with new plant.

# Next Steps

1. **Prototyping**: Create UI mockups for the main screens.
2. **Development**: Start with the authentication module, followed by plant management and reminders.
3. **Testing**: Unit tests for services, integration tests for data flow.
4. **Deployment**: Prepare the app for deployment on Android and iOS platforms.

Feel free to ask for more details or modifications!
