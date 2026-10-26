# SmartTroopUpgrade

**SmartTroopUpgrade** is an intelligent, automated army composition and troop upgrade mod for **Mount & Blade II: Bannerlord** (v1.4.8+).

Instead of manually clicking through dozens of troop upgrade trees across different cultures, **SmartTroopUpgrade** automatically elevates ready-to-upgrade troops based on customizable tactical role percentages and culture-specific strengths.

---

## Features

- **5 Tactical Role Quotas (%)**:
  Configure target ratios via **Mod Configuration Menu (MCM v5)**:
  - **Shield Infantry** (Default: 40%)
  - **Shock / 2-Handed / Polearm Infantry** (Default: 10%)
  - **Ranged Foot Archers & Crossbowmen** (Default: 25%)
  - **Melee Shock Cavalry** (Default: 15%)
  - **Horse Archers** (Default: 10%)
  Target quotas dynamically scale as your party size grows.

- **Smart Culture Prioritization**:
  When multiple upgrade paths are available for recruits and infantry, troops are routed into classes where their culture excels:
  - **Shield Infantry**: Sturgia > Empire > Vlandia > Battania > Aserai > Khuzait
  - **Ranged Foot**: Battania (Fian line) > Empire > Aserai > Vlandia > Khuzait > Sturgia
  - **Heavy Cavalry**: Vlandia (Banner Knight) > Empire (Cataphract) > Aserai > Khuzait > Sturgia > Battania
  - **Horse Archers**: Khuzait (Khan's Guard) > Aserai > Empire
  - **Shock Infantry**: Sturgia (Line Breaker) > Battania (Falxman) > Empire > Aserai > Vlandia > Khuzait

- **Mount & Gold Reserves Safeguards**:
  - Requires configurable minimum gold reserves before upgrading.
  - Checks party inventory for required horses/war horses and respects a configurable mount reserve buffer. Troops are held ready for cavalry rather than forced into infantry if horses are temporarily missing.

- **Noble Troop & Lock Protection**:
  - Noble lines are prioritized towards their peak unit branches.
  - Troops locked in the party screen (`Alt` + Click / padlock icon) are strictly ignored by the auto-upgrader.

- **Automatic Campaign Triggers & Hotkey**:
  - Automatically evaluates upgrades upon **exiting battles**, on **hourly campaign ticks**, and upon **loading saves**.
  - Manual on-demand hotkey: `Ctrl + Shift + U`.

- **MCM v5 Integration with Fail-Safe Fallback**:
  - Full in-game configuration via MCM v5.
  - If MCM is not installed, the mod gracefully defaults to built-in presets without crashing or throwing errors.
  - 100% save-game safe (does not store custom data in save files).

---

## Requirements

- Mount & Blade II: Bannerlord (v1.4.8+)
- [Harmony](https://github.com/BUTR/Bannerlord.Harmony)
- [MCM v5](https://github.com/Aragas/Bannerlord.MBOptionScreen) *(Optional, recommended for settings GUI)*

---

## Installation

1. Download the latest release or clone the repository.
2. Place the `SmartTroopUpgrade` folder into your Bannerlord `Modules` directory:
   `...\Mount & Blade II Bannerlord\Modules\SmartTroopUpgrade\`
3. Enable **Smart Troop Upgrade** in the Bannerlord Launcher below the official modules (`StoryMode`, `CustomBattle`, `NavalDLC`).

---

## Building from Source

```bash
dotnet build -c Release
```

---

## License

MIT License
