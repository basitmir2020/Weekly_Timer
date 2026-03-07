# Project Structure

## Top-Level Layout

```text
WeeklyTimetable/
  App.xaml(.cs)
  AppShell.xaml(.cs)
  MauiProgram.cs
  WeeklyTimetable.csproj
  Controls/
  Converters/
  Data/
  Models/
  Platforms/
  Resources/
  Services/
  ViewModels/
  Views/
  docs/
```

## Folder Responsibilities

- `Controls/`: reusable custom UI controls (ring progress, heatmap, energy slider, streak badge)
- `Converters/`: XAML value converters for formatting, color mapping, and display transformations
- `Data/`: default schedule seed/template logic
- `Models/`: entity models and message payloads
- `Platforms/`: platform-specific startup hooks and Android widget provider
- `Resources/`: app icons, splash, fonts, styles, and images
- `Services/`: business/infrastructure services (DB, persistence, notifications, sync, export)
- `ViewModels/`: MVVM orchestration and UI state management
- `Views/`: XAML pages/content views and code-behind
- `docs/`: project documentation

## Key Entry Points

- App bootstrapping: `MauiProgram.cs`, `App.xaml.cs`, `AppShell.xaml.cs`
- Main user flow: `Views/MainPage.xaml.cs`, `ViewModels/MainViewModel.cs`

## Important Service Contracts

- `IDatabaseService`
- `IPersistenceService`
- `INotificationService`
- `IStreakService`
- `IProfileService`
- `ISupabaseSyncService`
- `IExportService`

## Core ViewModels

- `MainViewModel`: schedule loading, filtering, completion, stats, reminders
- `PomodoroViewModel`: timer state machine
- `AnalyticsViewModel`: streak/heatmap/mood/goals analytics
- `SettingsViewModel`: preferences, template editing, cloud backup/restore
