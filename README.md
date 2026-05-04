# 🚀 SpaceCargo

> A physics-based 2D space lander game built with Unity 6. Pilot your rocket through hazardous terrain, manage fuel and health, collect coins, defeat enemies, and land precisely on designated pads — all with full PlayStation controller support and per-player profile management.

![Unity](https://img.shields.io/badge/Unity-6000.1.x-blue?logo=unity)
![C#](https://img.shields.io/badge/C%23-10.0-green?logo=csharp)
![Platform](https://img.shields.io/badge/Platform-Windows%20%7C%20macOS%20%7C%20Linux-lightgrey)
![Input](https://img.shields.io/badge/Input-Keyboard%20%7C%20Mouse%20%7C%20PS%20Controller-orange)

---

## 📋 Table of Contents

- [Overview](#-overview)
- [Features](#-features)
- [Scenes](#-scenes)
- [Gameplay](#-gameplay)
- [Controls](#-controls)
- [Achievement System](#-achievement-system)
- [Multi-Player Profiles](#-multi-player-profiles)
- [Technical Architecture](#-technical-architecture)
- [Project Structure](#-project-structure)
- [Getting Started](#-getting-started)
- [Building the Game](#-building-the-game)
- [Known Issues & Fixes](#-known-issues--fixes)
- [Future Enhancements](#-future-enhancements)
- [Author](#-author)

---

## 🎮 Overview

SpaceCargo is a complete production-ready 2D lander game developed in Unity 6. The game challenges players to pilot a cargo rocket through levels filled with asteroids, drones, coins, and fuel canisters — landing precisely on designated pads to score points and earn achievements.

The project features a full multi-player profile system where each player on a shared device maintains completely separate progress: their own level unlocks, achievement stars, best scores, and leaderboard entries. No account or internet connection is required.

---

## ✨ Features

| Feature | Description |
|---|---|
| Physics-based lander | Rigidbody2D with thrust, rotation, gravity, and a three-state freeze system |
| Multi-player profiles | Per-player level unlocks, stars, achievements — fully isolated per player name |
| Achievement system | 9 achievement types, 3 per level, evaluated after each successful landing |
| Procedural levels | Levels 1–3 hand-crafted; Level 4+ procedurally generated with scaled difficulty |
| Enemy AI | Asteroids (velocity-based) and drones (patrol AI) with pause/resume support |
| Leaderboard | Top-5 persistent leaderboard with player name, score, and date |
| Audio | Persistent background music + 7 SFX categories across all scenes |
| PS Controller | Full D-pad navigation on all menus; input bleed prevention in builds |
| Settings | Music and SFX toggles with per-button cooldown; preferences saved |
| Cross-platform | Builds for Windows, macOS, and Linux from a single project |

---

## 🎬 Scenes

The game uses six scenes managed by a central `SceneLoader`:

```
MainMenuScene → PlayerNameScene → LevelSelectionScene
                                         ↓
                              GameScene ←→ GameOverScene
```

| Scene | Purpose |
|---|---|
| `MainMenuScene` | Hub — Play, Shuttles, Leaderboard, Settings, Quit |
| `PlayerNameScene` | Register new pilot or select existing profile |
| `LanderSelectionScene` | Choose rocket skin (cosmetic only) |
| `LevelSelectionScene` | Browse and select unlocked levels |
| `GameScene` | Core gameplay — physics, enemies, achievements, HUD |
| `GameOverScene` | Final score, high score, leaderboard display |

---

## 🕹️ Gameplay

### HUD Elements
- **Score** — accumulated across levels in the session
- **Time** — seconds since first input
- **Speed X / Speed Y** — current lander velocity
- **Fuel bar** — depletes on thrust; collect fuel canisters to replenish
- **Hearts (×3)** — health points; one lost per non-fatal crash

### Landing Requirements
A successful landing requires **all three** conditions:

| Condition | Threshold |
|---|---|
| Contact speed | Below **4.0** units (watch Speed Y on HUD) |
| Lander angle | Within **10 degrees** of vertical |
| Contact surface | Designated **landing pad** only |

### Score Formula
```
Landing Angle Score  = 100 − |dotVector − 1| × 10 × 100
Landing Speed Score  = (4.0 − relativeVelocity) × 100
Final Score          = (Angle Score + Speed Score) × Pad Multiplier
```
Scores accumulate across all levels in a session.

### Collectibles

| Item | Effect |
|---|---|
| Coin | +500 score; counted for Coin Collector achievements |
| Fuel Canister | Replenishes fuel bar; counts for Fuel Saver achievement |

### Enemies

| Enemy | Behaviour |
|---|---|
| Asteroid | Spawns above play area; velocity-based drift (no gravity); destroyable |
| Drone | Patrols level; follows DroneAI script; destroyable with weapon |

> **Note:** All enemies freeze before the player starts moving and after any landing. They also freeze when the game is paused. No pre-start or post-landing damage is possible.

### Lander State Machine

```
WaitingToStart ──(first thrust)──► Normal ──(land/crash/fuel=0)──► GameOver
     │                                                                   │
  Kinematic                       Dynamic                           Kinematic
  gravityScale=0               gravityScale=0.7                  Colliders OFF
  Enemies frozen                Enemies active                    Enemies frozen
```

---

## 🎮 Controls

| Action | Keyboard | PS Controller |
|---|---|---|
| Thrust (up) | `↑` / `W` | `R2` |
| Rotate left | `←` / `A` | D-pad `←` |
| Rotate right | `→` / `D` | D-pad `→` |
| Shoot | `Space` | `Square` |
| Pause | `Escape` | `Options` |
| Navigate menus | Arrow keys | D-pad `↑↓←→` |
| Confirm / Select | `Enter` | `Cross` |

> **PS Controller:** Connect via USB or Bluetooth before launching. Detected automatically on all platforms.

---

## 🏆 Achievement System

Each level has **3 achievements**. Completing an achievement awards **1 star** to the current player only.

| Achievement | Condition |
|---|---|
| Coin Collector | Pick up the specified number of coins |
| Soft Touch | Land on the first landing pad in the level |
| Speed Runner | Complete a successful landing within the time limit |
| Feather Landing | Land with speed below the specified threshold |
| Fuel Saver | Pick up at least one fuel canister |
| Greedy Pilot | Collect every coin in the level |
| Untouchable | Land with all 3 hearts intact (no crashes) |
| High Flyer | Achieve a score above the specified threshold |
| Drone Slayer | Destroy the specified number of drones |

- **Levels 1–3:** Fixed hand-crafted achievement targets
- **Levels 4+:** Procedurally generated targets scaled to difficulty

Achievements are shown in the pre-level panel before the game starts, and evaluated in the post-landing panel after every successful landing.

---

## 👥 Multi-Player Profiles

SpaceCargo supports multiple local player profiles on a single device. Each player's data is completely isolated:

```
PlayerPrefs key format:

UnlockedLevels_Alice       ← level progress
Level1Stars_Alice          ← star rating per level
Level1BestScore_Alice      ← best score per level
Ach_Alice_1_coins_1        ← achievement completion per level
AchievementData            ← JSON blob with playerStars list
```

**Switching players:** Press Play → select your name from the existing player list. Stars, level unlocks, and achievements load immediately for the selected player.

**Resetting data:** Unity Editor: `Edit → Clear All PlayerPrefs`. In a build, delete the platform-specific PlayerPrefs storage file.

---

## 🏗️ Technical Architecture

### Persistent Singletons (DontDestroyOnLoad)

| Class | Responsibility |
|---|---|
| `GameManager` | Session score, level progression, pause/unpause events |
| `AudioManager` | Music + SFX playback, toggle persistence |
| `AchievementManager` | Per-player achievement evaluation and star tracking |
| `LeaderboardManager` | Player names, top-5 leaderboard data |
| `LevelSelectionManager` | Per-player level unlock and score data |

### Core Scripts

| Script | Description |
|---|---|
| `Lander.cs` | Rigidbody2D physics, three-state machine, collision, fuel, health, shooting |
| `EnemyPauser.cs` | Freezes/unfreezes enemies based on `GameStateManager.IsGameActive` |
| `GameStateManager.cs` | Static class; `IsGameActive` bool shared by all enemy scripts |
| `AsteroidSpawner.cs` | Spawns asteroids with velocity-based motion; respects game state |
| `LevelSelectionUI.cs` | Generates level cards with per-player stars and scores |
| `PlayerNameUI.cs` | Procedural registration and profile selection UI |
| `SettingsUI.cs` | Procedural settings panel; 0.4s per-button toggle cooldown |
| `AchievementUI.cs` | Pre-level panel (prefab) and post-landing panel (procedural) |
| `PausedUI.cs` | Pause menu; hides achievement panel on pause |
| `MainMenuUI.cs` | Main menu wiring; 0.5s submit delay on scene arrival |
| `GameOverUI.cs` | Score display, leaderboard, input bleed prevention |

### Physics Configuration

| Parameter | Value |
|---|---|
| Linear Drag | 0.5 |
| Angular Drag | 2.0 |
| Thrust Force | 8.0 units |
| Rotation Torque | 3.0 units |
| Gravity Scale (active) | 0.7 |
| Max Angular Velocity | 200 units |
| Collision Detection | Continuous |

### Input System

Uses Unity's **New Input System (1.7.x)** with a single `InputActions` asset shared by gameplay and the UI `EventSystem`. Action maps:
- `Player` — UpAction, LeftAction, RightAction, Shoot, Submit
- `UI` — Navigate, Submit, Cancel

Controller input bleed prevention: `DisableSubmitAction()` on every scene arrival + `0.5s WaitForSecondsRealtime` delay before re-enabling.

---

## 📁 Project Structure

```
SpaceCargo/
├── Assets/
│   ├── Input/
│   │   └── InputActions.inputactions   # Shared input asset
│   ├── Prefabs/
│   │   ├── Asteroid.prefab             # EnemyPauser + gravityScale=0
│   │   ├── Drone.prefab                # EnemyPauser + DroneAI script
│   │   ├── AchievementRowPrefab.prefab # Pre-level achievement row
│   │   └── Landers/                   # Rocket skin prefabs
│   ├── Resources/
│   │   └── LevelPreviews/             # Level card thumbnail sprites
│   ├── Scenes/
│   │   ├── MainMenuScene.unity
│   │   ├── PlayerNameScene.unity
│   │   ├── LanderSelectionScene.unity
│   │   ├── LevelSelectionScene.unity
│   │   ├── GameScene.unity
│   │   └── GameOverScene.unity
│   ├── Scripts/
│   │   ├── AchievementData.cs
│   │   ├── AchievementManager.cs
│   │   ├── AchievementUI.cs
│   │   ├── AsteroidSpawner.cs
│   │   ├── AudioManager.cs
│   │   ├── EnemyPauser.cs
│   │   ├── GameManager.cs
│   │   ├── GameOverUI.cs
│   │   ├── GameStateManager.cs
│   │   ├── Lander.cs
│   │   ├── LandingPad.cs
│   │   ├── LeaderboardManager.cs
│   │   ├── LevelSelectionManager.cs
│   │   ├── LevelSelectionUI.cs
│   │   ├── MainMenuUI.cs
│   │   ├── PausedUI.cs
│   │   ├── PlayerNameUI.cs
│   │   ├── SceneLoader.cs
│   │   ├── SettingsUI.cs
│   │   └── ...
│   ├── Audio/                         # Music and SFX clips
│   ├── Sprites/                       # Game sprites and textures
│   └── Materials/                     # Physics and visual materials
├── ProjectSettings/
└── Packages/
    └── manifest.json                  # Input System, TMP, Cinemachine
```

---

## 🚀 Getting Started

### Prerequisites

- [Unity Hub](https://unity.com/download) with **Unity 6000.1.x** installed
- Build modules: Windows, macOS, and Linux Build Support (IL2CPP)
- Visual Studio 2022 or JetBrains Rider
- Git 2.x
- PS DualSense or DualShock 4 controller (optional)

### Installation

**1. Clone the repository**
```bash
git clone https://github.com/manasha-12/SpaceCargo.git
cd SpaceCargo
```

**2. Open in Unity Hub**
- Launch Unity Hub
- Click **Add → Add project from disk**
- Navigate to the cloned folder and select it
- Unity 6000.1.x will open and import all assets (allow 2–5 minutes on first open)

**3. Configure Input System**
- Go to **Edit → Project Settings → Player**
- Under **Other Settings**, set **Active Input Handling** to **New Input System**
- Click **Apply and Restart** when prompted

**4. Import TMP Resources**
- If prompted, click **Import TMP Essentials**

**5. Verify Build Settings**
- Go to **File → Build Settings**
- Confirm all six scenes are listed in this order:

| Index | Scene |
|---|---|
| 0 | MainMenuScene |
| 1 | PlayerNameScene |
| 2 | LanderSelectionScene |
| 3 | LevelSelectionScene |
| 4 | GameScene |
| 5 | GameOverScene |

**6. Assign Audio Clips**
- In `MainMenuScene`, select the **AudioManagerObject** in the Hierarchy
- In the Inspector, assign audio clips to all `AudioManager` fields:
  - Music Clip, Sfx Crash, Sfx Fuel Pickup, Sfx Coin Pickup
  - Sfx Landing Success, Sfx Thruster, Sfx Button Click, Sfx Button Hover

**7. Play in Editor**
- Open `MainMenuScene`
- Press the **Play** button
- Use arrow keys or PS controller to navigate

---

## 🏗️ Building the Game

```
File → Build Settings → select platform → Switch Platform → Build
```

| Platform | Output |
|---|---|
| Windows | `SpaceCargo.exe` + `SpaceCargo_Data/` + `UnityPlayer.dll` |
| macOS | `SpaceCargo.app` bundle |
| Linux | `SpaceCargo.x86_64` + `SpaceCargo_Data/` |

> Distribute the **entire output folder**, not just the executable. All supporting files are required.

**Linux — first run:**
```bash
chmod +x SpaceCargo.x86_64
./SpaceCargo.x86_64
```

**macOS — first run (Gatekeeper bypass):**
Right-click `SpaceCargo.app` → **Open** → confirm in the dialog.
Or: `System Settings → Privacy & Security → Open Anyway`

---

## 🐛 Known Issues & Fixes

| Issue | Status | Fix Applied |
|---|---|---|
| Controller double-input on settings toggles | Fixed | 0.4s per-button cooldown in `SettingsUI.cs` |
| Input bleed from Game Over to Main Menu (builds) | Fixed | 0.5s `WaitForSecondsRealtime` + `DisableSubmitAction()` on scene load |
| Asteroids falling due to gravity instead of floating | Fixed | `gravityScale = 0` enforced on prefab and at each spawn in `AsteroidSpawner.cs` |
| Enemies damaging lander before game starts | Fixed | `EnemyPauser` component freezes all enemies until first input |
| Achievement panel visible behind Pause UI | Fixed | `PausedUI` calls `AchievementUI.SetPostLandingVisible(false)` on pause |
| Stars shared across all players (old global key) | Fixed | All PlayerPrefs keys now include player name as suffix |
| Lander taking damage after successful landing | Fixed | `SetState(GameOver)` disables all `Collider2D` components immediately |
| Music not starting (AudioSource timing) | Fixed | Music starts in `Start()` not `Awake()` in `AudioManager.cs` |

---

## 🔮 Future Enhancements

- [ ] Online leaderboard (REST API + cloud database)
- [ ] Mobile support (iOS / Android touch controls)
- [ ] New enemy types (homing missiles, shielded drones)
- [ ] In-game level editor
- [ ] Achievement leaderboard per level
- [ ] Cloud save (Unity Gaming Services / Firebase)
- [ ] Multiplayer co-op mode
- [ ] Procedural music system
- [ ] Additional lander skins and unlockable cosmetics
      
---

## 👤 Author

**Manasha**
- GitHub: [@manasha-12](https://github.com/manasha-12)

---

## 🙏 Acknowledgements

- [Unity Technologies](https://unity.com) — game engine
- [Box2D](https://box2d.org) — 2D physics foundation (via Unity PhysX2D)
- Classic Lunar Lander (Atari, 1979) — genre inspiration
- [UK Game Accessibility Guidelines](https://gameaccessibilityguidelines.com) — controller navigation standards

---

*Made with ❤️ and Unity 6*
