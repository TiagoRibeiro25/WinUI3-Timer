# WinUI3 Timer

A simple Windows desktop timer app built with WinUI 3 and .NET 10.

## Features

- Create, rename, and delete timers (right-click a timer in the sidebar)
- Start, stop, and reset elapsed time
- Timers persist automatically to `%LOCALAPPDATA%\WinUI3-Timer\timers.json`
- Running timers recover their elapsed time after an app restart

## Requirements

- Windows 10 1809 or later
- .NET 10 SDK (to build)

## Run in development

```powershell
dotnet run
```

## Build a standalone executable

```powershell
# Folder publish (publish profiles output)
dotnet publish -c Release -p:Platform=x64

# Single-file executable
dotnet publish -c Release -p:Platform=x64 -p:PublishSingleFile=true -p:PublishProfile= -p:PublishDir=bin\publish-single\
```

The standalone executable is self-contained (includes the .NET runtime and
Windows App SDK), so the target machine needs no extra installs.

## Notes

- Release builds must not be trimmed (`PublishTrimmed` stays `false`).
  IL trimming breaks WinUI 3 interop and causes silent startup/AUI failures.