# Gaming Mode Blueprint

## Product Decision

Gaming Mode runs inside the current Windows user account. It does not create
or switch to a separate user. It behaves like a trigger/state change, similar in
spirit to SteamOS/Bazzite Gaming Mode, but implemented on Windows 11.

The reference behavior is Microsoft's Windows 11 Xbox Mode / Full Screen
Experience:

- one controller-first full-screen home app
- reduced background startup activity while in gaming mode
- return to the normal Windows desktop on demand
- Game Bar / Task View style entry and exit points where useful

Our version keeps the same operating-system shape but replaces the Xbox home app
with Steam Big Picture.

## Non-Goals

- Do not launch the Xbox app as the home interface.
- Do not create a separate Windows user.
- Do not manage Dolby Atmos directly. Windows and the selected audio endpoint
  remain responsible for spatial audio.
- Do not aggressively disable core Windows services, drivers, audio, networking,
  GPU, input, anti-cheat, or Sunshine dependencies.

## Core Modes

### Desktop Mode

Normal Windows behavior.

- Explorer is running.
- User startup apps run normally.
- Steam, Decky PluginLoader, and Sunshine may still run if the user configured
  them that way, but the mode manager does not force a console shell.

### Gaming Mode

Console-like Windows behavior on the same user account.

- Steam starts as the home app using Big Picture / Deck UI.
- Decky PluginLoader starts before Steam when possible.
- Sunshine is required and must be running.
- Desktop startup noise is reduced.
- Explorer may be hidden or stopped after the mode agent is alive.
- Return to Desktop Mode restarts/restores Explorer and normal startup behavior.

## Required Gaming Stack

These are the processes/services we should preserve or explicitly start because
they are part of the Windows gaming and streaming baseline.

- Steam home app
  - `steam.exe -gamepadui`
- Decky Loader for Windows
  - `PluginLoader_noconsole.exe` preferred when available
- Sunshine
  - `sunshine.exe` or the installed Sunshine service
- Windows input/controller stack
  - GameInput Service
  - Xbox controller transport services if present and needed by the controller
  - HID, Bluetooth, USB, and related device services
- Windows gaming shell helpers
  - Game Bar, if used for controller button / quick overlay / Task View entry
  - Task View support for switching back to desktop-like surfaces
- Graphics and display stack
  - GPU driver services
  - DWM
  - HDR/display configuration as currently configured by Windows
- Audio stack
  - Windows Audio
  - selected HDMI/TV/AVR/soundbar endpoint
  - spatial audio is left untouched
- Network stack
  - required for Sunshine/Moonlight LAN streaming

## Explicitly Not Home App

The Xbox app is not launched as the dashboard.

We may keep underlying Windows gaming services if they are part of the OS gaming
stack or controller stack, but the user-facing home app is Steam.

## Startup Policy

The mode manager owns a small config file:

```json
{
  "defaultMode": "Desktop",
  "nextBootMode": null,
  "gaming": {
    "homeApp": "Steam",
    "steamArgs": "-gamepadui",
    "deckyRequired": true,
    "sunshineRequired": true,
    "delaySteamAfterDeckyMs": 1500,
    "manageAudio": false
  }
}
```

Rules:

- `nextBootMode` wins once, then clears itself.
- `defaultMode` is used when `nextBootMode` is empty.
- Desktop app has controls for:
  - restart once into Gaming Mode
  - restart once into Desktop Mode
  - set default startup mode to Gaming
  - set default startup mode to Desktop

## Windows App

Small desktop utility:

- title: Gaming Mode
- primary action: Restart in Gaming Mode
- secondary action: Restart in Desktop Mode
- segmented default: Start in Desktop / Start in Gaming
- status indicators:
  - Steam
  - Decky
  - Sunshine
  - Current mode

## Decky Plugin

Decky plugin calls the local Windows mode agent.

Minimum actions:

- Return to Desktop
- Restart in Desktop Mode
- Restart Steam
- Restart Decky Loader
- Show Sunshine status

The plugin should not directly edit registry keys, services, startup items, or
system settings. It talks to the local agent over localhost or a named pipe.

## Local Agent

The local agent is the privileged Windows-side coordinator.

Responsibilities:

- read and write mode config
- run at user login
- start Steam Big Picture in Gaming Mode
- start Decky PluginLoader
- ensure Sunshine is running
- reduce user startup activity in Gaming Mode
- restore Explorer and normal desktop state on Desktop Mode
- expose a small local API for the Decky plugin and desktop app

Possible transport:

- localhost HTTP bound to `127.0.0.1`
- named pipe

The API must reject remote network access.

## Open Technical Questions

- Whether Windows 11 exposes the same Xbox Mode startup-app categories to
  third-party home apps on this machine.
- Whether Steam can be registered as a supported gaming home app for native Xbox
  Mode entry points, or whether we need to emulate the behavior entirely.
- Whether Decky Loader for Windows is stable enough for always-on startup, or
  should be supervised and restarted by the agent.
- Whether Explorer should be stopped in Gaming Mode or only hidden/minimized for
  safer recovery.
