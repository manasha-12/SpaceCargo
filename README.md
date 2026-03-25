# 🚀 Space Cargo - Lunar Lander Game

A physics-based 2D space lander game built with Unity, where players must carefully navigate and land their spacecraft on designated landing pads while managing fuel and avoiding crashes.

![Unity Version](https://img.shields.io/badge/Unity-6.0-blue)
![C#](https://img.shields.io/badge/C%23-Latest-green)
![Platform](https://img.shields.io/badge/Platform-Windows%20%7C%20Mac%20%7C%20Linux-lightgrey)

## 🎮 Game Overview

Space Cargo is a challenging lunar lander simulation game that tests your piloting skills. Navigate your spacecraft through treacherous terrain, manage your fuel consumption, and execute precision landings to score points.

### Key Features

- **Realistic Physics**: Smooth, physics-based movement with thrust and rotation mechanics
- **Precision Landing System**: Score-based landing evaluation considering speed and angle
- **Dynamic Particle Effects**: Visual feedback with thruster particle systems
- **Multiple Landing Zones**: Various landing pads with different score multipliers
- **Collision Detection**: Advanced collision system for realistic crash mechanics

## 🎯 Gameplay Mechanics

### Controls

| Key | Action |
|-----|--------|
| `↑` (Up Arrow) | Main Thruster - Upward thrust |
| `←` (Left Arrow) | Left Rotation - Rotate counterclockwise |
| `→` (Right Arrow) | Right Rotation - Rotate clockwise |

### Landing Criteria

To achieve a successful landing, you must meet the following conditions:

1. **Landing Speed**: Relative velocity must be below `4.0` units
2. **Landing Angle**: Spacecraft must be upright (within `10%` deviation from vertical)
3. **Landing Target**: Must land on a designated landing pad

### Scoring System

Your score is calculated based on:

- **Landing Angle Score** (0-100): Precision of your vertical alignment
- **Landing Speed Score** (0-100): How gently you touch down
- **Multiplier Bonus**: Each landing pad has a unique score multiplier (1x, 5x, etc.)

**Formula:**
```
Final Score = (Landing Angle Score + Landing Speed Score) × Landing Pad Multiplier
```

## 🛠️ Technical Details

### Built With

- **Engine**: Unity 6.0
- **Language**: C#
- **Physics**: Unity Physics2D
- **Input System**: Unity Input System (New Input System)

### Core Components

#### Lander.cs
The main spacecraft controller handling:
- Physics-based movement (thrust and rotation)
- Collision detection and landing validation
- Event system for visual feedback
- Score calculation

#### LanderVisuals.cs
Manages visual feedback:
- Thruster particle system control
- Event-driven particle activation
- Left, right, and middle thruster states

#### LandingPad.cs
Landing zone management:
- Score multiplier configuration
- Landing validation

### Physics Configuration

- **Linear Drag**: 0.5 (air resistance for movement)
- **Angular Drag**: 2.0 (rotation damping)
- **Thrust Force**: 8.0 units
- **Rotation Torque**: 3.0 units
- **Max Angular Velocity**: 200 units (prevents excessive spinning)

## 📦 Project Structure
```
SpaceCargo/
├── Assets/
│   ├── Scripts/
│   │   ├── Lander.cs              # Main spacecraft controller
│   │   ├── LanderVisuals.cs       # Visual effects manager
│   │   └── LandingPad.cs          # Landing zone controller
│   ├── Scenes/
│   │   └── SampleScene.unity      # Main game scene
│   ├── Sprites/                   # Game sprites and textures
│   ├── Prefabs/                   # Reusable game objects
│   └── Materials/                 # Physics materials
├── ProjectSettings/               # Unity project configuration
└── Packages/                      # Package dependencies
```

## 🚀 Getting Started

### Prerequisites

- Unity 6.0 or later
- Visual Studio 2022 or JetBrains Rider (recommended)
- Git

### Installation

1. **Clone the repository**
```bash
   git clone https://github.com/manasha-12/SpaceCargo.git
```

2. **Open in Unity**
   - Launch Unity Hub
   - Click "Add" → "Add project from disk"
   - Navigate to the cloned repository
   - Select the project folder

3. **Open the Scene**
   - Navigate to `Assets/Scenes/`
   - Double-click `SampleScene.unity`

4. **Play**
   - Press the Play button in Unity Editor
   - Use arrow keys to control the lander

### Building the Game

1. Go to `File` → `Build Settings`
2. Select your target platform (PC, Mac, Linux)
3. Click `Build` or `Build and Run`

## 🎮 How to Play

1. **Launch**: Start the game - your lander begins in space
2. **Navigate**: Use arrow keys to control thrust and rotation
3. **Approach**: Carefully descend toward a landing pad
4. **Land**: Reduce speed and align vertically before touchdown
5. **Score**: Check the console for your landing score and feedback

### Tips for Success

- 🔥 **Conserve Momentum**: Small, controlled bursts are better than constant thrust
- 📐 **Align Early**: Start rotating to vertical alignment while still high up
- 🎯 **Reduce Speed**: Begin braking well before reaching the landing pad
- ⚠️ **Watch the Terrain**: Avoid colliding with mountains and obstacles

## 🐛 Known Issues

- Collision detection may trigger slightly before visual contact (collider optimization in progress)

## 🔮 Future Enhancements

- [ ] Fuel management system
- [ ] Multiple levels with increasing difficulty
- [ ] Sound effects and background music
- [ ] Main menu and UI system
- [ ] High score leaderboard
- [ ] Mobile touch controls
- [ ] Additional spacecraft models
- [ ] Power-ups and collectibles

## 📝 Development Log

### Current Version: Alpha 0.1

**Latest Updates:**
- ✅ Implemented smooth physics-based controls
- ✅ Added collision detection system
- ✅ Created scoring algorithm
- ✅ Integrated particle thruster effects
- ✅ Fixed NullReferenceException on terrain collision

## 🤝 Contributing

Contributions are welcome! If you'd like to improve Space Cargo:

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 👤 Author

**Manasha**
- GitHub: [@manasha-12](https://github.com/manasha-12)

## 🙏 Acknowledgments

- Unity Technologies for the game engine
- Inspired by classic Lunar Lander arcade games
- Community feedback and testing

---

**⭐ If you enjoyed this project, please consider giving it a star!**

---

*Made with ❤️ and Unity*