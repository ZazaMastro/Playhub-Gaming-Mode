# Playhub Gaming Mode

Playhub Gaming Mode is a console mode for a Windows living-room
PC. It keeps the current Windows user account and switches between two shells:

- Desktop Mode: standard Windows desktop with Explorer.
- Gaming Mode: a controller-first Steam Big Picture session with Sunshine and
  Decky Loader prepared before Steam starts (so you can also turn on your PC and stream it wherever you want with Moonlight).

The goal is a cleaner couch gaming experience without creating a second Windows
user, replacing the operating system, or disabling the services required by
games, controllers, streaming, audio, and drivers.

## Features

- Graphical installer and companion app.
- Clean mode switching through sign-out/sign-in.
- Per-user shell switching: Explorer for Desktop Mode, `GamingMode.exe shell`
  for Gaming Mode.
- Steam Big Picture startup with Decky Loader launched first.
- Sunshine startup and service preparation for Moonlight streaming.
- Input compatibility preparation for HID, Bluetooth, GameInput, Xbox GIP,
  Steam Input, DirectInput, and `dinput8.dll` wrappers.
- Idle mouse cursor hiding in Gaming Mode.
- Experimental focus borderless fullscreen handling for foreground game windows.
- Automatic cleanup of orphaned Decky Loader PyInstaller child processes.
- Localhost API for the Decky companion plugin.
- Shift-at-sign-in safety bypass to force Desktop Mode.

## Requirements

- Windows 11.
- Steam.
- Sunshine, if Moonlight streaming is used.
- Decky Loader for Windows, if the Decky plugin is used.
- PowerShell 5 or newer.

For development:

- .NET SDK 10.0.x or compatible.
- Node.js 24 or compatible.

## Install

Download `gaming-mode-win-x64.zip` from the latest release, extract it, then run:

```text
Setup.exe
```

The installer configures the companion app, local agent, shortcuts, startup
mode, and required service preparation. It does not install Decky Loader itself.

If you use the Decky companion plugin, install `gaming-mode-decky.zip` manually
through Decky Loader before using the plugin controls.

## Usage

Open `Gaming Mode` from the desktop shortcut or Start Menu.

Main actions:

- Switch to Gaming Mode.
- Switch to Desktop Mode.
- Choose the default startup mode.

Switching modes saves the destination shell and signs out. On the next sign-in,
Windows starts the selected shell naturally.

Gaming Mode startup order:

1. Sunshine compatibility services.
2. Sunshine.
3. Decky Loader from `%USERPROFILE%\homebrew\services\PluginLoader_noconsole.exe`.
4. Input compatibility services.
5. Steam Big Picture / gamepad mode.

## Safety

Gaming Mode is designed to be reversible:

- Hold `Shift` during sign-in to force Desktop Mode.
- Desktop Mode restores Explorer as the normal shell before sign-out.
- Explorer is restored automatically if Steam cannot start.
- A watchdog restores Desktop Mode if Steam disappears while Explorer is hidden.
- The local API binds to `127.0.0.1` by default.

Core Windows display, audio, networking, GPU, input, anti-cheat, and driver
stacks are not disabled.

## Configuration

Configuration file:

```text
%APPDATA%\GamingMode\config.json
```

Useful fields:

```json
{
  "defaultMode": "Desktop",
  "nextBootMode": null,
  "gaming": {
    "steamPath": null,
    "steamArguments": "-gamepadui",
    "deckyPath": null,
    "sunshinePath": null,
    "deckyRequired": true,
    "sunshineRequired": true,
    "delaySteamAfterDeckyMs": 1500,
    "closeExplorerInGamingMode": true,
    "allowExplorerCloseInGamingMode": true,
    "restoreExplorerOnDesktop": true,
    "restoreStartupAppsOnDesktop": false,
    "openSteamDesktopOnInteractiveDesktopMode": false,
    "ensureInputCompatibilityInGamingMode": true,
    "ensureSunshineCompatibilityInGamingMode": true,
    "autoHideMouseCursorInGamingMode": true,
    "autoHideMouseCursorAfterMs": 2200,
    "borderlessFullscreenWindowsInGamingMode": true,
    "manageAudio": false
  },
  "safety": {
    "apiPort": 47991,
    "allowRemoteApi": false,
    "restartWithoutPrompt": true
  }
}
```

Leave paths as `null` to use automatic discovery. Set explicit paths only when
Steam, Decky Loader, or Sunshine are installed in custom locations.

## Decky Plugin

The Decky plugin talks to the local Windows agent:

```text
http://127.0.0.1:47991
```

Release asset:

```text
gaming-mode-decky.zip
```

Manual development install helper:

```powershell
.\scripts\install-decky-plugin.ps1 -DeckyPluginsDir "C:\path\to\decky\plugins"
```

## Build From Source

```powershell
.\scripts\build.ps1
```

Generated packages:

```text
artifacts\gaming-mode-win-x64.zip
artifacts\gaming-mode-decky.zip
```

Local install from source:

```powershell
.\scripts\install.ps1
```

## Uninstall

Run:

```text
Uninstall Gaming Mode.cmd
```

Or:

```powershell
.\scripts\uninstall.ps1
```

Uninstall restores the standard Explorer shell, removes shortcuts, and removes
installed files under `%LOCALAPPDATA%\GamingMode`. It does not remove Steam,
Sunshine, Decky Loader, or their settings.

## Local API

- `GET /health`
- `GET /status`
- `POST /mode/gaming`
- `POST /mode/desktop`
- `POST /mode/gaming/switch`
- `POST /mode/desktop/switch`
- `POST /mode/gaming/restart`
- `POST /mode/desktop/restart`
- `POST /default/gaming`
- `POST /default/desktop`
- `POST /restart/steam`
- `POST /restart/decky`
- `POST /cursor/autohide/start`
- `POST /cursor/autohide/stop`

## Status

This is an early public version. It is intended for advanced Windows handheld,
HTPC, and living-room PC setups where Steam Big Picture is the primary gaming
interface.
