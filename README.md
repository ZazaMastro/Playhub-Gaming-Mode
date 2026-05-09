# Playhub Gaming Mode

Playhub Gaming Mode is a console mode for a Windows living-room
PC. It keeps the current Windows user account and switches between two shells:

- Desktop Mode: your normal Windows PC installation with no changes.
- Gaming Mode: a controller-first Steam Big Picture session with Sunshine and
  Decky Loader prepared before Steam starts (so you can also turn on your PC and stream it wherever you want with Moonlight).

The goal is a cleaner couch gaming experience without Windows bullshit services (except for the gaming ones obviously).

## Features

- a GUI installer and a Companion app.
- Steam Big Picture startup with Decky Loader launched first to avoid loading issues.
- Sunshine startup and service preparation for streaming.
- Input compatibility preparation for HID, Bluetooth, GameInput, Xbox GIP,
  Steam Input, DirectInput, and `dinput8.dll` wrappers.
- Idle mouse cursor hiding in Gaming Mode (this is a cool one if you don't want that damn thing around when using a controller).
- Focus borderless fullscreen handling for foreground game windows (similar to SteamOS / Bazzite Gaming Mode). Important system windows are out of this for obvious reasons.
- Automatic cleanup of orphaned Decky Loader PyInstaller child processes.
- A Decky companion plugin to do everything with your gamepad.
- Keep pressed the Shift key during sign-in to force Desktop Mode (just a security thing).

## Requirements

- Windows 11.
- Steam.
- Sunshine, if Moonlight streaming is used.
- Decky Loader for Windows, if the Decky plugin is used.
- PowerShell 5 or newer.

## Install

Download `gaming-mode-win-x64.zip` from the latest release, extract it, then run:

```text
Setup.exe
```

The installer configures the companion app, local agent, shortcuts, startup modes, and required service preparation.

If you use the Decky companion plugin, install `gaming-mode-decky.zip` manually
through Decky Loader before using the plugin controls. 

## How to install the plugin in DeckyLoader: 
- Open Steam Big Picture 
- Press CTRL + 2 / Home + B / PS + Circle
- Select the electric plug icon on the bottom
- Select the Setting icon on the top right of the menu
- In General, scroll down and enable "Developer Mode"
- Now in "Developer" select "Install from a ZIP file"
- Search for the file `gaming-mode-decky.zip` in your PC and click Install 

## Usage

Open `Gaming Mode` from the desktop shortcut or Start Menu.

Here you can:
- Switch to Gaming Mode.
- Switch to Desktop Mode.
- Choose the default startup mode.

Switching modes saves the destination shell and signs out. On the next sign-in,
Windows will start the selected shell, that's it.

Gaming Mode startup order:

1. Sunshine compatibility services.
2. Sunshine.
3. Decky Loader from `%USERPROFILE%\homebrew\services\PluginLoader_noconsole.exe`.
4. Input compatibility services.
5. Steam Big Picture.

## Safety

Gaming Mode is designed to be reversible:

- Hold `Shift` during sign-in to force Desktop Mode.
- Desktop Mode restores Explorer as the normal shell before sign-out.
- Explorer is restored automatically if Steam cannot start.
- A watchdog restores Desktop Mode if Steam disappears while Explorer is hidden.

Core Windows display, audio, networking, GPU, input, anti-cheat, and driver
stacks are not disabled for obvious reason.

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

Open Setup.exe, select the language, skip the warning and select Uninstall (if it gives you an error just select Uninstall again and it will work)

Run:

```text
Uninstall Gaming Mode.cmd
```

Or:

```powershell
.\scripts\uninstall.ps1
```

Uninstall restores the standard Explorer shell, removes shortcuts, and removes
installed files under `%LOCALAPPDATA%\GamingMode`.

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
