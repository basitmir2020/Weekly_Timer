# Install Guide

## 1. Prerequisites

- .NET SDK 10.x (matching project target frameworks)
- .NET MAUI workloads
- Visual Studio 2022/2025 (or Rider with MAUI support)

Platform-specific:

- Android: Android SDK + emulator/device
- iOS/MacCatalyst: Xcode + Apple toolchain (macOS)
- Windows: Windows 10/11 with WinApp SDK support

## 2. Verify MAUI Workloads

```bash
dotnet workload list
```

Install if missing:

```bash
dotnet workload install maui
```

## 3. Restore Dependencies

```bash
dotnet restore WeeklyTimetable.csproj
```

## 4. Build

Windows target:

```bash
dotnet build WeeklyTimetable.csproj -f net10.0-windows10.0.19041.0
```

Android target:

```bash
dotnet build WeeklyTimetable.csproj -f net10.0-android
```

## 5. Run (Examples)

Windows:

```bash
dotnet build WeeklyTimetable.csproj -t:Run -f net10.0-windows10.0.19041.0
```

Android (device/emulator must be available):

```bash
dotnet build WeeklyTimetable.csproj -t:Run -f net10.0-android
```

## 6. Troubleshooting

- If Android dex files are locked (`classes.dex`/`classes2.dex`), stop running emulator/build processes and retry.
- If workloads are out of date, run:

```bash
dotnet workload update
```

- If restore/build warnings mention package vulnerabilities, review package versions in `WeeklyTimetable.csproj` before release.
