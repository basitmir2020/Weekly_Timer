# Architecture

## Overview

The app uses a pragmatic MVVM architecture on .NET MAUI:

- **Views**: XAML pages and controls (`Views/`, `Controls/`)
- **ViewModels**: state + command orchestration (`ViewModels/`)
- **Services**: persistence, notifications, sync, export (`Services/`)
- **Models**: data contracts/entities (`Models/`)
- **Data**: default schedule templates (`Data/`)

## Startup Path

1. `MauiProgram.CreateMauiApp()` configures DI and registers services/viewmodels/pages.
2. `App` resolves `AppShell` from DI.
3. `AppShell` registers routes and decides onboarding navigation in `OnAppearing()`.
4. `MainPage.OnAppearing()` triggers `MainViewModel.LoadDataAsync()`.

## Dependency Injection

Primary registrations happen in `MauiProgram.cs`:

- Core services: `IPersistenceService`, `IDatabaseService`, `INotificationService`, `IStreakService`, `IProfileService`, `IExportService`, `ISupabaseSyncService`
- ViewModels: `MainViewModel`, `PomodoroViewModel`, `AnalyticsViewModel`, `SettingsViewModel`, etc.
- Views/pages: main app pages and navigation targets

## Data & State Strategy

- **SQLite** (`DatabaseService`): streaks, check-ins, weekly goals
- **Preferences** (`PersistenceService` + direct settings): schedule snapshots and app preferences
- **Schedule versioning**:
  - Legacy key: `sched_v2`
  - Current key: `sched_v3`

## Messaging

- Uses `WeakReferenceMessenger` for cross-view-model update propagation (e.g., schedule changed events).

## Notification Flow

- `MainViewModel` requests permission and schedules rolling reminders through `INotificationService`.
- Existing notifications are canceled before rescheduling to keep state deterministic.

## External Integrations

- Supabase (`SupabaseSyncService`) for backup/restore of schedule payloads
- SkiaSharp for graphics-based controls and export rendering
- Local Notification plugin for reminders

## Architectural Strengths

- Clear layer boundaries and service interfaces
- DI-friendly composition
- Cross-platform abstraction for platform-sensitive concerns

## Current Technical Risks

- Some converter and MVVM warnings (nullability/AOT) appear at build time and should be addressed before production hardening.
- Event handler detachment in `HeatmapGridControl` currently uses lambda unsubscribe pattern that may not detach previous delegates.
