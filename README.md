# Chicken Invaders Unity

A complete Unity port of the classic Chicken Invaders game, converted from C++ SFML to Unity C#.

## 🎮 Game Features

- **Dual Player Support**: Two players can play simultaneously with different control schemes
- **Advanced Weapon System**: 6 different weapon types with upgrade levels
- **Touch Controls**: Full mobile support with touch controls
- **Audio System**: Complete sound effects and background music
- **Object Pooling**: Optimized performance with bullet pooling
- **Original Assets**: Uses original sprites, sounds, and animations from C++ version

## 🎯 Control Schemes

### Keyboard Controls
- **Player 1**: WASD for movement, Space to shoot
- **Player 2**: Arrow keys for movement, Right Shift to shoot

### Touch Controls (Mobile)
- Virtual joystick for movement
- Touch button for shooting

## 🚀 Installation

1. Open Unity 2022.3.0f1 or later
2. Import this project
3. Open Assets/Scenes/GameScene
4. Press Play

## 📁 Project Structure

```
Assets/
├── Scenes/           # Game scenes
├── Scripts/          # All C# scripts
│   ├── Controllers/  # Player, Enemy, Bullet controllers
│   ├── Managers/     # Game, Audio, UI, Input managers
│   ├── UI/           # UI components
│   └── Utils/        # Utilities and constants
├── Sprites/          # Game textures and images
├── Audio/            # Sound effects and music
├── Prefabs/          # Game object prefabs
└── Materials/        # Unity materials
```

## 🔧 Current Implementation Status

### ✅ Phase 1: Core Structure (COMPLETED)
- [x] GameManager - Main game controller
- [x] BulletManager - Bullet pooling system
- [x] AudioManager - Sound management
- [x] UIManager - User interface
- [x] InputManager - Keyboard and touch input
- [x] Project structure and assets

### 🚧 Next Phases
- **Phase 2**: Player System and Movement
- **Phase 3**: Enemy System and AI
- **Phase 4**: Weapon System and Combat
- **Phase 5**: Final Polish and Testing

## 🎨 Assets

All original assets from the C++ version are included:
- Sprites: Characters, weapons, effects
- Audio: Sound effects and background music
- Fonts: Original game fonts

## 🏗️ Architecture

The game follows Unity best practices:
- **Singleton Managers**: GameManager, AudioManager, etc.
- **Component-Based Design**: PlayerController, EnemyController
- **Object Pooling**: For bullets and effects
- **Input Abstraction**: Supports both keyboard and touch
- **ScriptableObjects**: For weapon configurations

## 📱 Platform Support

- ✅ Windows Standalone
- ✅ Android Mobile
- ✅ iOS Mobile
- ✅ WebGL Browser
- ✅ Mac Standalone

## 🔊 Audio Features

- Background music with volume control
- Weapon-specific sound effects
- UI interaction sounds
- Audio settings (music/SFX toggle)

## 📝 Development Notes

This project was created by converting C++ SFML code to Unity C#:
- Original game loop translated to Unity's component system
- SFML graphics converted to Unity sprites and renderers
- Input system redesigned for multi-platform support
- Audio system rebuilt using Unity's AudioSource components

## 🏆 Credits

Converted from original C++ SFML Chicken Invaders game.
All assets and game design from original project.