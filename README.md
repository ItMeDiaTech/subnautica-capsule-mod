# Unlimited Time Capsules

A [BepInEx](https://github.com/BepInEx/BepInEx) mod for Subnautica that removes the time capsule spawn limit and lets you configure spawn rates.

By default, Subnautica spawns a small, fixed number of time capsules per save. This mod injects time capsules into biome loot tables across the map so they keep appearing throughout your playthrough.

## Requirements

- [Subnautica](https://store.steampowered.com/app/264710/Subnautica/) (Steam)
- [BepInEx 5.x](https://github.com/BepInEx/BepInEx/releases) for Unity (Mono)

## Installation

1. Download `SubnauticaCapsule.Installer.exe` from the [latest release](https://github.com/ItMeDiaTech/subnautica-capsule-mod/releases/latest)
2. Run the installer — it auto-detects your Steam installation
3. Click **Install** (installs BepInEx if needed, then copies the mod DLL)

To uninstall, run the installer again and click **Uninstall**.

## Updating

The installer includes a built-in update checker. Click **Check for updates** in the bottom-left corner of the installer window to check for new versions. If an update is available, the installer will download it and restart automatically.

## Configuration

After first launch with the mod installed, a config file is created at:
```
Subnautica\BepInEx\config\com.diatech.unlimitedtimecapsules.cfg
```

| Option | Default | Description |
|--------|---------|-------------|
| `MaxCapsules` | `0` | Total capsule limit per save. `0` = unlimited. |
| `SpawnProbability` | `2.0` | Probability weight in each biome's loot table. Higher = more capsules. Range: 0.01–10.0. |
| `EnableExtraSpawns` | `true` | Enable injecting TimeCapsule into biome loot tables for additional spawns. |
| `MaxQueueSize` | `15` | Max capsules waiting for API content at once. Prevents request bursts. |
| `DebugGlow` | `false` | Adds a bright point light to spawned capsules for testing visibility. |

## Building from Source

The mod project references Subnautica game assemblies. Set the `SUBNAUTICA_DIR` environment variable to your install path, or edit `Directory.Build.props` directly:

```bash
set SUBNAUTICA_DIR=D:\Games\Steam\steamapps\common\Subnautica
dotnet build -c Release
```

If Subnautica is installed at the default Steam location (`C:\Program Files (x86)\Steam\steamapps\common\Subnautica`), no configuration is needed.

To build the installer as a self-contained exe:

```bash
dotnet build -c Release
dotnet publish src/SubnauticaCapsule.Installer -c Release -r win-x64 --self-contained -p:PublishSingleFile=true -o ./publish
```

## License

MIT
