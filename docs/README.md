# WeeklyTimetable Docs

This folder contains project-level documentation for the `WeeklyTimetable` .NET MAUI application.

## Document Index

- [INSTALL.md](./INSTALL.md): Environment setup and build/run instructions
- [ARCHITECTURE.md](./ARCHITECTURE.md): High-level architecture, layers, and runtime flow
- [PROJECT_STRUCTURE.md](./PROJECT_STRUCTURE.md): Directory layout and responsibility map
- [MINDMAP_CLASS_DIAGRAM_FLOW.md](./MINDMAP_CLASS_DIAGRAM_FLOW.md): Mermaid mindmap, class diagram, and flowchart

## Project Summary

`WeeklyTimetable` is a cross-platform MAUI app for daily schedule tracking, productivity blocks, streaks, check-ins, goals, and reminders.

### Core Functional Areas

- Weekly schedule planning and block completion
- Pomodoro timer flow (focus/break cycles)
- Streak tracking and analytics heatmap
- Daily mood/energy check-ins
- Weekly goals tracking
- Local notifications
- Local persistence (SQLite + Preferences) and Supabase backup/restore

## Target Platforms

- Android (`net10.0-android`)
- iOS (`net10.0-ios`)
- Mac Catalyst (`net10.0-maccatalyst`)
- Windows (`net10.0-windows10.0.19041.0`)

## Notes

- This documentation is intentionally implementation-aware and aligned with the current codebase.
- Keep docs updated when adding services, view models, pages, routes, or persistence keys.
