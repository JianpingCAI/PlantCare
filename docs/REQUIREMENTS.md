# PlantCare Requirements Document

## Document Information

**Version**: 1.0  
**Last Updated**: 2024-XX-XX  
**Author**: Jianping CAI  
**Status**: Living Document

---

## 1. Product Overview

### 1.1 Vision

PlantCare is a personal plant care management application that helps users track their plants, schedule care reminders, and maintain a history of plant care activities.

### 1.2 Goals

- Enable easy plant tracking and management
- Prevent plant neglect through timely reminders
- Provide visual care history and analytics
- Maintain user privacy with local-first data storage
- Support cross-platform use (Android, iOS, Windows)

### 1.3 Target Users

- **Primary**: Home plant enthusiasts who own 5-50 plants
- **Secondary**: Beginners learning plant care
- **Tertiary**: Small-scale plant collectors

### 1.4 Success Metrics

- User retention > 60% after 30 days
- Average session time > 2 minutes
- Zero critical bugs in production
- App rating > 4.0 stars

---

## 2. User Stories

### 2.1 Plant Management

**As a plant owner, I want to:**

- **US-001**: Add a new plant with photo and basic information
  - **Acceptance Criteria**:
    - Can capture photo from camera or select from gallery
    - Can enter name, species, and age
    - Plant is saved to local database
    - Can see plant in overview list immediately

- **US-002**: View all my plants in a list
  - **Acceptance Criteria**:
    - Plants displayed in alphabetical order
    - Each plant shows photo, name, and next care time
    - Visual indicator for plants needing care (color-coded)
    - Can search/filter plants

- **US-003**: Edit plant information
  - **Acceptance Criteria**:
    - Can update all plant fields
    - Changes are saved immediately
    - Care history is preserved
    - Can replace plant photo

- **US-004**: Delete a plant
  - **Acceptance Criteria**:
    - Requires confirmation before deletion
    - All related data is removed (cascade delete)
    - Photos are deleted from storage
    - Can undo within 5 seconds (nice-to-have)

### 2.2 Care Tracking

**As a plant owner, I want to:**

- **US-005**: Record when I water a plant
  - **Acceptance Criteria**:
    - Can mark plant as watered with timestamp
    - Next watering date is automatically calculated
    - Watering event is added to history
    - Visual status updates immediately

- **US-006**: Record when I fertilize a plant
  - **Acceptance Criteria**:
    - Can mark plant as fertilized with timestamp
    - Next fertilization date is calculated
    - Fertilization event is added to history
    - Visual status updates immediately

- **US-007**: Set custom care frequency for each plant
  - **Acceptance Criteria**:
    - Can set watering frequency (days + hours)
    - Can set fertilization frequency (days + hours)
    - Frequencies are independent per plant
    - Can update frequency anytime

- **US-008**: View complete care history for a plant
  - **Acceptance Criteria**:
    - See all watering events with dates
    - See all fertilization events with dates
    - Can delete individual history entries
    - Visual chart showing care patterns

### 2.3 Reminders and Notifications

**As a plant owner, I want to:**

- **US-009**: Receive notifications when plants need care
  - **Acceptance Criteria**:
    - Notifications scheduled based on care frequency
    - Separate notifications for watering and fertilization
    - Can enable/disable notifications globally
    - Can tap notification to view plant details

- **US-010**: See upcoming care tasks in a calendar
  - **Acceptance Criteria**:
    - Calendar view shows all care events
    - Different colors for watering vs fertilization
    - Can see multiple plants' schedules
    - Can navigate by month

### 2.4 Data Management

**As a plant owner, I want to:**

- **US-011**: Backup my plant data
  - **Acceptance Criteria**:
    - Can export all data to ZIP file
    - Export includes plants, history, and photos
    - Can choose export location
    - Export completes successfully

- **US-012**: Restore my plant data
  - **Acceptance Criteria**:
    - Can import from previously exported ZIP
    - Option to merge or replace existing data
    - Photos are restored correctly
    - Database integrity is maintained

- **US-013**: Keep my data private and local
  - **Acceptance Criteria**:
    - No data sent to external servers
    - No account required
    - Data stored in device private storage
    - Encryption for sensitive data

### 2.5 Localization

**As a user, I want to:**

- **US-014**: Use the app in my preferred language
  - **Acceptance Criteria**:
    - Support English (default)
    - Support Chinese (Simplified)
    - Can switch language in settings
    - All UI text is translated
    - Date/time formats follow locale

---

## 3. Functional Requirements

### 3.1 Plant Management

| ID | Requirement | Priority | Status |
|----|-------------|----------|--------|
| FR-001 | System shall allow users to add plants with name, species, age, photo, and notes | Must Have | ✅ Implemented |
| FR-002 | System shall validate plant name (1-50 characters, required) | Must Have | ✅ Implemented |
| FR-003 | System shall support photos from camera or gallery | Must Have | ✅ Implemented |
| FR-004 | System shall optimize and resize photos automatically | Should Have | ✅ Implemented |
| FR-005 | System shall generate thumbnails for list views | Should Have | ✅ Implemented |
| FR-006 | System shall provide default plant image if no photo | Must Have | ✅ Implemented |
| FR-007 | System shall allow editing all plant properties | Must Have | ✅ Implemented |
| FR-008 | System shall require confirmation for plant deletion | Must Have | ✅ Implemented |
| FR-009 | System shall cascade delete all related data when plant deleted | Must Have | ✅ Implemented |

### 3.2 Care Scheduling

| ID | Requirement | Priority | Status |
|----|-------------|----------|--------|
| FR-010 | System shall track last watered date/time for each plant | Must Have | ✅ Implemented |
| FR-011 | System shall track last fertilized date/time for each plant | Must Have | ✅ Implemented |
| FR-012 | System shall allow custom watering frequency per plant (1 hour - 1 year) | Must Have | ✅ Implemented |
| FR-013 | System shall allow custom fertilization frequency per plant (1 hour - 1 year) | Must Have | ✅ Implemented |
| FR-014 | System shall calculate next care dates automatically | Must Have | ✅ Implemented |
| FR-015 | System shall provide visual indicators for care status (due, overdue, ok) | Should Have | ✅ Implemented |
| FR-016 | System shall allow quick-marking plants as watered/fertilized | Must Have | ✅ Implemented |

### 3.3 History Tracking

| ID | Requirement | Priority | Status |
|----|-------------|----------|--------|
| FR-017 | System shall maintain complete watering history per plant | Must Have | ✅ Implemented |
| FR-018 | System shall maintain complete fertilization history per plant | Must Have | ✅ Implemented |
| FR-019 | System shall allow viewing care history with dates | Must Have | ✅ Implemented |
| FR-020 | System shall allow deleting individual history entries | Should Have | ✅ Implemented |
| FR-021 | System shall update plant's last care date when history deleted | Must Have | ✅ Implemented |
| FR-022 | System shall display care history in charts | Nice to Have | ✅ Implemented |
| FR-023 | System shall show care statistics and analytics | Nice to Have | ✅ Implemented |

### 3.4 Notifications

| ID | Requirement | Priority | Status |
|----|-------------|----------|--------|
| FR-024 | System shall schedule local notifications for care reminders | Must Have | ✅ Implemented |
| FR-025 | System shall support separate notification toggles for watering/fertilizing | Must Have | ✅ Implemented |
| FR-026 | System shall request notification permissions appropriately | Must Have | ✅ Implemented |
| FR-027 | System shall navigate to plant detail when notification tapped | Should Have | ✅ Implemented |
| FR-028 | System shall cancel notifications when plant deleted | Must Have | ✅ Implemented |
| FR-029 | System shall reschedule notifications when care performed | Must Have | ✅ Implemented |
| FR-030 | System shall limit notifications to platform-supported devices | Must Have | ✅ Implemented |

### 3.5 Calendar View

| ID | Requirement | Priority | Status |
|----|-------------|----------|--------|
| FR-031 | System shall display care schedule in calendar format | Should Have | ✅ Implemented |
| FR-032 | System shall differentiate watering and fertilization events visually | Should Have | ✅ Implemented |
| FR-033 | System shall allow navigation between months | Must Have | ✅ Implemented |
| FR-034 | System shall show multiple plants' events on same date | Must Have | ✅ Implemented |
| FR-035 | System shall allow tapping date to see event details | Should Have | ✅ Implemented |

### 3.6 Data Management

| ID | Requirement | Priority | Status |
|----|-------------|----------|--------|
| FR-036 | System shall export all data to ZIP format | Should Have | ✅ Implemented |
| FR-037 | System shall include photos in data export | Should Have | ✅ Implemented |
| FR-038 | System shall import data from ZIP format | Should Have | ✅ Implemented |
| FR-039 | System shall validate imported data integrity | Must Have | ✅ Implemented |
| FR-040 | System shall store all data locally on device | Must Have | ✅ Implemented |
| FR-041 | System shall use SQLite for structured data storage | Must Have | ✅ Implemented |
| FR-042 | System shall handle database migrations automatically | Must Have | ✅ Implemented |

### 3.7 Localization

| ID | Requirement | Priority | Status |
|----|-------------|----------|--------|
| FR-043 | System shall support English language | Must Have | ✅ Implemented |
| FR-044 | System shall support Simplified Chinese | Should Have | ✅ Implemented |
| FR-045 | System shall allow language selection in settings | Should Have | ✅ Implemented |
| FR-046 | System shall format dates according to locale | Should Have | ✅ Implemented |

---

## 4. Non-Functional Requirements

### 4.1 Performance

| ID | Requirement | Target | Priority |
|----|-------------|--------|----------|
| NFR-001 | App startup time | < 2 seconds | High |
| NFR-002 | Plant list loading time | < 1 second for 100 plants | High |
| NFR-003 | Image loading time | < 500ms per image | Medium |
| NFR-004 | Database query time | < 100ms for most queries | Medium |
| NFR-005 | Photo optimization time | < 3 seconds | Low |
| NFR-006 | Memory usage | < 200MB for 100 plants | Medium |
| NFR-007 | Storage usage | < 10MB per plant (with photo) | Low |

### 4.2 Reliability

| ID | Requirement | Target | Priority |
|----|-------------|--------|----------|
| NFR-008 | App crash-free rate | > 99.5% | Critical |
| NFR-009 | Data loss incidents | Zero | Critical |
| NFR-010 | Notification delivery rate | > 95% | High |
| NFR-011 | Database integrity | 100% | Critical |
| NFR-012 | Export/import success rate | > 99% | High |

### 4.3 Usability

| ID | Requirement | Target | Priority |
|----|-------------|--------|----------|
| NFR-013 | Intuitive UI (first-time user completion) | > 80% | High |
| NFR-014 | Touch target size | ≥ 44x44 dp | High |
| NFR-015 | Contrast ratio | ≥ 4.5:1 for text | Medium |
| NFR-016 | Loading indicators | All async operations | High |
| NFR-017 | Error messages | Clear and actionable | High |

### 4.4 Compatibility

| ID | Requirement | Details | Priority |
|----|-------------|---------|----------|
| NFR-018 | Android support | API 21+ (Android 5.0+) | Critical |
| NFR-019 | iOS support | iOS 11.0+ | High |
| NFR-020 | Windows support | Windows 10.0.19041.0+ | Medium |
| NFR-021 | Screen sizes | 4" to 13" displays | High |
| NFR-022 | Orientations | Portrait and landscape | Medium |

### 4.5 Security & Privacy

| ID | Requirement | Details | Priority |
|----|-------------|---------|----------|
| NFR-023 | Data encryption | Sensitive data encrypted at rest | Medium |
| NFR-024 | No telemetry | No user tracking or analytics | High |
| NFR-025 | No network calls | All features work offline | Critical |
| NFR-026 | Photo permissions | Only requested when needed | High |
| NFR-027 | Notification permissions | Only requested when needed | High |

### 4.6 Maintainability

| ID | Requirement | Details | Priority |
|----|-------------|---------|----------|
| NFR-028 | Code coverage | > 70% unit test coverage | Medium |
| NFR-029 | Documentation | All public APIs documented | High |
| NFR-030 | MVVM pattern | Strict separation of concerns | Critical |
| NFR-031 | Dependency injection | All services via DI | Critical |
| NFR-032 | Repository pattern | All data access abstracted | Critical |

### 4.7 Accessibility

| ID | Requirement | Details | Priority |
|----|-------------|---------|----------|
| NFR-033 | Screen reader support | All UI elements accessible | Medium |
| NFR-034 | Font scaling | Support system font sizes | Medium |
| NFR-035 | Color blindness | Not rely solely on color | Low |
| NFR-036 | Keyboard navigation | Full keyboard support (desktop) | Low |

---

## 5. Platform-Specific Requirements

### 5.1 Android

| ID | Requirement | Priority |
|----|-------------|----------|
| PSR-A001 | Support Android Material Design guidelines | Medium |
| PSR-A002 | Handle app lifecycle (pause, resume) correctly | Critical |
| PSR-A003 | Request runtime permissions (Camera, Storage, Notifications) | Critical |
| PSR-A004 | Support Android sharing (share plant data) | Nice to Have |
| PSR-A005 | Handle back button navigation correctly | High |

### 5.2 iOS

| ID | Requirement | Priority |
|----|-------------|----------|
| PSR-I001 | Follow iOS Human Interface Guidelines | Medium |
| PSR-I002 | Request iOS-specific permissions appropriately | Critical |
| PSR-I003 | Support iOS sharing (share plant data) | Nice to Have |
| PSR-I004 | Handle app lifecycle correctly | Critical |
| PSR-I005 | Support safe area insets | High |

### 5.3 Windows

| ID | Requirement | Priority |
|----|-------------|----------|
| PSR-W001 | Support mouse and keyboard input | High |
| PSR-W002 | Responsive layout for desktop | High |
| PSR-W003 | Support Windows notifications | Medium |
| PSR-W004 | Handle window resizing | High |

---

## 6. Data Requirements

### 6.1 Data Entities

**Plant**:
- ID (Guid)
- Name (string, 1-50 chars, required)
- Species (string, optional)
- Age (int, 0-1000)
- Photo path (string)
- Last watered (DateTime)
- Watering frequency (int hours, 1-8760)
- Last fertilized (DateTime)
- Fertilization frequency (int hours, 1-8760)
- Notes (string, optional)

**WateringHistory**:
- ID (Guid)
- Plant ID (Guid, FK)
- Care time (DateTime)

**FertilizationHistory**:
- ID (Guid)
- Plant ID (Guid, FK)
- Care time (DateTime)

### 6.2 Data Constraints

- Maximum 1000 plants per database
- Maximum 10,000 history records per plant
- Maximum photo size: 10MB (before optimization)
- Maximum photo dimensions: 4096x4096 pixels
- Thumbnail size: 200x200 pixels

### 6.3 Data Retention

- Plant data: Until user deletes
- Photos: Until plant deleted or photo replaced
- History: Until user deletes or plant deleted
- Logs: Last 30 days, rotating

---

## 7. Constraints and Assumptions

### 7.1 Technical Constraints

- **TC-001**: Must use .NET MAUI for cross-platform development
- **TC-002**: Must use SQLite for local database
- **TC-003**: Must support offline-first architecture
- **TC-004**: No backend server or cloud services
- **TC-005**: File storage limited to device capacity

### 7.2 Business Constraints

- **BC-001**: Free app with no monetization
- **BC-002**: Solo developer with limited time
- **BC-003**: Open-source project
- **BC-004**: No budget for third-party services

### 7.3 Assumptions

- **A-001**: Users have stable device storage (> 100MB free)
- **A-002**: Users grant necessary permissions when requested
- **A-003**: Users understand basic plant care concepts
- **A-004**: Devices have camera (or can skip photo features)
- **A-005**: Users primarily use one device (no sync needed yet)

---

## 8. Future Requirements (Roadmap)

### 8.1 Version 0.8.0

- Complete iOS testing and optimization
- Complete Windows testing and optimization
- Enhanced error handling
- Performance improvements

### 8.2 Version 0.9.0

- Plant categories
- Enhanced search
- Home screen widgets
- Dark mode
- Statistics dashboard

### 8.3 Version 1.0.0

- Cloud sync (optional)
- Social features
- Advanced notifications
- Care logs with notes and photos

### 8.4 Post-1.0

- Plant identification (AI/ML)
- Disease detection
- Weather integration
- IoT sensor support
- Community features

See [ROADMAP.md](ROADMAP.md) for detailed future plans.

---

## 9. Out of Scope

The following are explicitly NOT part of current requirements:

- **Multi-user support**: Single user per device only
- **Cloud backup**: Local storage only (export/import for backup)
- **Social network**: No sharing or social features (v1.0+)
- **E-commerce**: No buying/selling plants
- **Plant database**: No built-in species encyclopedia (v1.1+)
- **AI features**: No plant identification or disease detection (v1.1+)
- **Web version**: Mobile and desktop apps only
- **Real-time sync**: No multi-device synchronization (v1.0+)

---

## 10. Acceptance Criteria

### 10.1 Release Criteria

A version is ready for release when:

- ✅ All "Must Have" requirements implemented
- ✅ All critical and high-priority bugs fixed
- ✅ Test coverage ≥ 70%
- ✅ No known data loss scenarios
- ✅ Performance targets met
- ✅ Documentation updated
- ✅ User testing completed (if applicable)

### 10.2 Feature Acceptance

A feature is accepted when:

- ✅ Functional requirements met
- ✅ Non-functional requirements met
- ✅ Unit tests written and passing
- ✅ Integration tests passing (if applicable)
- ✅ Code reviewed
- ✅ Documentation updated
- ✅ User-facing: Tested on target platforms

---

## 11. Change Management

### 11.1 Requirement Changes

**Process**:
1. Identify change need
2. Document change in GitHub issue
3. Assess impact on existing features
4. Update requirements document
5. Update affected documentation (ARCHITECTURE, API, etc.)
6. Update CHANGELOG.md

### 11.2 Priority Changes

Requirements priority can change based on:
- User feedback
- Technical constraints
- Time availability
- Platform changes

**Priority Levels**:
- **Critical**: App cannot function without this
- **Must Have**: Core feature, required for release
- **Should Have**: Important but not blocking
- **Nice to Have**: Enhances experience
- **Future**: Planned for later versions

---

## Document Control

### Version History

| Version | Date | Changes | Author |
|---------|------|---------|--------|
| 1.0 | 2024-XX-XX | Initial requirements document | Jianping CAI |

### Review Schedule

- **Quarterly**: Full requirements review
- **Per Release**: Update status of implemented requirements
- **As Needed**: When new features or changes proposed

### Approvals

| Role | Name | Date | Signature |
|------|------|------|-----------|
| Product Owner | Jianping CAI | 2024-XX-XX | [Digital] |
| Developer | Jianping CAI | 2024-XX-XX | [Digital] |

---

**Note**: As a solo developer project, formal approvals are simplified. This document serves as the single source of truth for what PlantCare should do and how it should behave.
