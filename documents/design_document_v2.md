# Design Document

## 1. Introduction

**Purpose**: This document provides a detailed design for the plant care mobile application. It includes the system architecture, data models, user interface design, API design, and security considerations.

**Scope**: The design covers both the frontend (mobile application) and backend (server-side) components of the application. It also addresses integration with external APIs for fetching plant care information.

**Audience**: This document is intended for developers, project managers, and stakeholders involved in the development and maintenance of the application.

## 2. Architecture

**Overview**: The plant care mobile application follows a client-server architecture. For the prototype stage, all data will be stored locally on the user's device to reduce costs and simplify development.

**Components**:

- **Mobile Application**: Built using .NET MAUI to support both iOS and Android devices with a single codebase.
- **Local Database**: SQLite for structured data storage (e.g., user information, plant details, reminders, logs).
- **Local File Storage**: Device storage for image files captured or uploaded by users.

**Data Flow**:

1. **User Interaction**: Users interact with the mobile application to manage plants, set reminders, and view care tips.
2. **Local Storage**: All data is stored locally on the device using SQLite for structured data and local file storage for images.
3. **External Data Fetching**: For additional plant care information, data may be fetched from external APIs as needed.

### Insights and Suggestions

- Using .NET MAUI will allow for a unified development experience and codebase for both iOS and Android.
- Local storage simplifies the architecture and reduces costs, making it suitable for the prototype stage.
- Ensure that the mobile application design adheres to platform-specific guidelines for better user experience.

## 3. Data Models

**Overview**: The following data models represent the main entities in the application. Data will be stored locally using SQLite for structured data and the device's file system for images.

### User Model

- **userId**: Unique identifier for the user (String, UUID).
- **username**: Username chosen by the user (String).
- **email**: User's email address (String).
- **passwordHash**: Hashed password (String).
- **createdAt**: Timestamp of when the user account was created (DateTime).

### Plant Model

- **plantId**: Unique identifier for the plant (String, UUID).
- **userId**: Identifier of the user who owns the plant (String, UUID).
- **species**: Species of the plant (String).
- **nickname**: User-defined nickname for the plant (String).
- **age**: Age of the plant in days (Integer).
- **lastWatered**: Timestamp of the last watering (DateTime).
- **photoPath**: Path to the plant's photo stored locally (String).

### Reminder Model

- **reminderId**: Unique identifier for the reminder (String, UUID).
- **userId**: Identifier of the user who set the reminder (String, UUID).
- **plantId**: Identifier of the plant associated with the reminder (String, UUID).
- **type**: Type of reminder (e.g., watering, fertilizing) (String).
- **scheduledAt**: Timestamp when the reminder is scheduled (DateTime).
- **isRecurring**: Flag indicating if the reminder is recurring (Boolean).

### Log Model

- **logId**: Unique identifier for the log entry (String, UUID).
- **userId**: Identifier of the user who created the log (String, UUID).
- **plantId**: Identifier of the plant associated with the log (String, UUID).
- **note**: User's note or observation (String).
- **photoPath**: Path to the photo associated with the log, stored locally (String).
- **createdAt**: Timestamp of when the log was created (DateTime).

### Insights and Suggestions

- SQLite provides a lightweight and efficient solution for local data storage.
- Implement data validation and constraints to ensure data integrity (e.g., email format, required fields).
- Efficiently manage local storage to handle potential storage constraints on mobile devices.

## 4. User Interface Design

**Overview**: The user interface design focuses on ease of use and intuitive navigation. The main screens and their functionalities are described below.

### Main Screens

1. **Login/Signup Screen**: Allows users to sign up or log in using their credentials or social media accounts.
2. **Dashboard**: Displays an overview of the user's plants, upcoming reminders, and quick access to care tips.
3. **Plant Management**:
   - **Plant List Screen**: Lists all the user's plants with options to add, edit, or delete entries.
   - **Plant Detail Screen**: Shows detailed information about a specific plant, including species, age, last watered date, and notes.
4. **Reminder Management**:
   - **Reminder List Screen**: Displays all reminders with options to add, edit, or delete.
   - **Reminder Detail Screen**: Shows details of a specific reminder and allows modifications.
5. **Tips and Information**:
   - **Tips List Screen**: Displays a list of care tips for different plant species.
   - **Tip Detail Screen**: Shows detailed information for a specific tip.
6. **Logging and Notes**:
   - **Log List Screen**: Lists all logs and notes for the user's plants.
   - **Log Detail Screen**: Shows detailed information about a specific log, including attached photos.
7. **Settings**:
   - **Settings Screen**: Allows users to customize notification preferences, switch to dark mode, and export data.

### Navigation Flow

- Use a tab-based navigation for major sections (Dashboard, Plants, Reminders, Tips, Settings).
- Implement a drawer or side menu for additional options and navigation links.
- Ensure consistent navigation and back navigation to enhance user experience.

### Insights and Suggestions

- Utilize .NET MAUI's built-in controls and layouts to create a responsive and adaptive UI.
- Follow MVVM (Model-View-ViewModel) pattern to separate UI logic from business logic, making the codebase more maintainable.
- Leverage .NET MAUI's support for platform-specific customization to enhance the user experience on both iOS and Android.

## 5. API Design

**Overview**: The backend server, implemented using ASP.NET Core, provides RESTful APIs for client-server communication. The APIs handle user authentication, plant management, reminders, and logs. For the prototype stage, focus on local data storage and operations.

### Endpoints

#### User Authentication

- **POST /api/auth/signup**: Create a new user account.
  - Request: `{ "username": "string", "email": "string", "password": "string" }`
  - Response: `{ "userId": "string", "username": "string", "email": "string", "token": "string" }`
- **POST /api/auth/login**: Authenticate a user and return a token.
  - Request: `{ "email": "string", "password": "string" }`
  - Response: `{ "userId": "string", "username": "string", "email": "string", "token": "string" }`

#### Plant Management

- **GET /api/plants**: Retrieve all plants for the authenticated user.
  - Response: `[ { "plantId": "string", "species": "string", "nickname": "string", "age": "integer", "lastWatered": "DateTime", "photoPath": "string" } ]`
- **POST /api/plants**: Add a new plant.
  - Request: `{ "species": "string", "nickname": "string", "age": "integer", "lastWatered": "DateTime", "photoPath": "string" }`
  - Response: `{ "plantId": "string", "species": "string", "nickname": "string", "age": "integer", "lastWatered": "DateTime", "photoPath": "string" }`
- **PUT /api/plants/{plantId}**: Update an existing plant.
  - Request: `{ "species": "string", "nickname": "string", "age": "integer", "lastWatered": "DateTime", "photoPath": "string" }`
  - Response: `{ "plantId": "string", "species": "string", "nickname": "string", "age": "integer", "lastWatered": "DateTime", "photoPath": "string" }`
- **DELETE /api/plants/{plantId}**: Delete a plant.
  - Response: `204 No Content`

#### Reminder Management

- **GET /api/reminders**: Retrieve all reminders for the authenticated user.
  - Response: `[ { "reminderId": "string", "plantId": "string", "type": "string", "scheduledAt": "DateTime", "isRecurring": "boolean" } ]`
- **POST /api/reminders**: Add a new reminder.
  - Request: `{ "plantId": "string", "type": "string", "scheduledAt": "DateTime", "isRecurring": "boolean" }`
  - Response: `{ "reminderId": "string", "plantId": "string", "type": "string", "scheduledAt": "DateTime", "isRecurring": "boolean" }`
- **PUT /api/reminders/{reminderId}**: Update an existing reminder.
  - Request: `{ "plantId": "string", "type": "string", "scheduledAt": "DateTime", "isRecurring": "boolean" }`
  - Response: `{ "reminderId": "string", "plantId": "string", "type": "string", "scheduledAt": "DateTime", "isRecurring": "boolean" }`
- **DELETE /api/reminders/{reminderId}**: Delete a reminder.
  - Response: `204 No Content`

#### Logging and Notes

- **GET /api/logs**: Retrieve all logs for the authenticated user.
  - Response: `[ { "logId": "string", "plantId": "string", "note": "string", "photoPath": "string", "createdAt": "DateTime" } ]`
- **POST /api/logs**: Add a new log entry.
  - Request: `{ "plantId": "string", "note": "string", "photoPath": "string" }`
  - Response: `{ "logId": "string", "plantId": "string", "note": "string", "photoPath": "string", "createdAt": "DateTime" }`
- **PUT /api/logs/{logId}**: Update an existing log entry.
  - Request: `{ "note": "string", "photoPath": "string" }`
  - Response: `{ "logId": "string", "plantId": "string", "note": "string", "photoPath": "string", "createdAt": "DateTime" }`
- **DELETE /api/logs/{logId}**: Delete a log entry.
  - Response: `204 No Content`

### Error Handling

- Return appropriate HTTP status codes for different scenarios (e.g., 400 for bad requests, 401 for unauthorized access, 404 for not found, 500 for server errors).
- Provide meaningful error messages in the response to help users and developers understand the issue.

### Insights and Suggestions

- Utilize ASP.NET Core's built-in support for creating RESTful APIs.
- Implement proper exception handling and logging to diagnose and fix issues efficiently.
- Consider using Swagger for API documentation to make it easier for developers to understand and use the APIs.

## 6. Security

**Overview**: Security measures are implemented to protect user data and ensure secure communication between the client and server.

### Authentication

- Use JWT (JSON Web Tokens) for secure user authentication.
- Implement token-based authentication for API requests.
- Ensure secure storage of tokens on the client-side using secure storage mechanisms provided by .NET MAUI.

### Data Encryption

- Use HTTPS for all client-server communications to encrypt data in transit.
- Encrypt sensitive data (e.g., passwords) using industry-standard encryption algorithms before storing them in the database.

### Vulnerability Management

- Regularly update dependencies and libraries to mitigate security vulnerabilities.
- Conduct regular security audits and penetration testing to identify and address potential security issues.
- Implement role-based access control (RBAC) to restrict access to certain functionalities based on user roles.

### Additional Security Measures

- Implement rate limiting to prevent abuse of API endpoints.
- Use CAPTCHA or other bot protection mechanisms during user signup and login.
- Monitor and log security-related events for audit and incident response purposes.

### Insights and Suggestions

- Leverage ASP.NET Core Identity for managing user authentication and authorization.
- Use .NET MAUI’s secure storage APIs to safely store sensitive information on the client side.
- Implement two-factor authentication (2FA) to enhance account security.

## 7. Local Data Storage

**Overview**: For the prototype stage, all data will be stored locally on the user's device to reduce costs and simplify development. This includes structured data stored in SQLite and image files stored in the device's local file system.

### SQLite Schema

**Users Table**:

```sql
CREATE TABLE Users (
    userId TEXT PRIMARY KEY,
    username TEXT NOT NULL,
    email TEXT NOT NULL,
    passwordHash TEXT NOT NULL,
    createdAt DATETIME NOT NULL
);
```

**Plants Table**:

```sql
CREATE TABLE Plants (
    plantId TEXT PRIMARY KEY,
    userId TEXT NOT NULL,
    species TEXT NOT NULL,
    nickname TEXT,
    age INTEGER,
    lastWatered DATETIME,
    photoPath TEXT,
    FOREIGN KEY (userId) REFERENCES Users(userId)
);
```

**Reminders Table**:

```sql
CREATE TABLE Reminders (
    reminderId TEXT PRIMARY KEY,
    userId TEXT NOT NULL,
    plantId TEXT NOT NULL,
    type TEXT NOT NULL,
    scheduledAt DATETIME NOT NULL,
    isRecurring BOOLEAN NOT NULL,
    FOREIGN KEY (userId) REFERENCES Users(userId),
    FOREIGN KEY (plantId) REFERENCES Plants(plantId)
);
```

**Logs Table**:

```sql
CREATE TABLE Logs (
    logId TEXT PRIMARY KEY,
    userId TEXT NOT NULL,
    plantId TEXT NOT NULL,
    note TEXT,
    photoPath TEXT,
    createdAt DATETIME NOT NULL,
    FOREIGN KEY (userId) REFERENCES Users(userId),
    FOREIGN KEY (plantId) REFERENCES Plants(plantId)
);
```
