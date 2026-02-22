# 快乐俄罗斯方块 - Happy Tetris

A classic Tetris game built with C# and WPF for Windows, featuring a beautiful Chinese-themed UI. **Designed with elderly players in mind** - featuring larger UI elements, high contrast colors, and slower gameplay speed.

![Version](https://img.shields.io/badge/version-1.0.0-blue)
![Platform](https://img.shields.io/badge/platform-Windows-green)
![.NET](https://img.shields.io/badge/.NET-8.0-purple)

## 🎮 Features

- **Classic Tetris Gameplay**: All 7 standard tetromino pieces (I, L, J, O, Z, S, T)
- **Chinese Theme**: Beautiful red and gold color scheme inspired by Chinese aesthetics
- **Elderly-Friendly Design**:
  - Large, clear UI elements (1400x900 window)
  - Extra-large game board (400x800 pixels, 33px cells)
  - Big, easy-to-read text (24-48pt fonts)
  - High contrast colors for better visibility
  - Thicker borders and grid lines
  - Large clickable buttons (200x60px minimum)
  - Bilingual labels (Chinese + English)
- **Slower Gameplay Speed**:
  - Starting speed: 2000ms per drop (gentler for reaction time)
  - Minimum speed: 800ms (more time to respond)
  - Gentle difficulty progression
- **Tetris Theme Music**: Looping background melody during gameplay
- **Sound Effects**: Built-in audio feedback for game actions
- **Scoring System**: 
  - 1 line: 100 points
  - 2 lines: 300 points
  - 3 lines: 500 points
  - 4 lines: 800 points
- **Level Progression**: Speed increases every 10 lines cleared
- **Next Piece Preview**: See what's coming next
- **Pause/Resume**: Press P to pause anytime
- **Fullscreen Mode**: Immersive gameplay

## 🎹 Controls

| Key | Action |
|-----|--------|
| ← | Move Left |
| → | Move Right |
| ↑ | Rotate |
| ↓ | Soft Drop |
| Space | Hard Drop |
| P | Pause/Resume |
| G | Guide |
| H or F1 | Help |

## 📋 Requirements

### To Run the Game
- Windows 10/11 (64-bit)
- No additional software needed - self-contained executable!

### To Build the Game
- Windows 10/11 (64-bit)
- .NET 8.0 SDK
- Visual Studio 2022 (recommended) or VS Code

## 🚀 Quick Start

### Option 1: Run the Published Game
```bash
# Navigate to the publish folder
cd HappyTetris\publish

# Run the game
HappyTetris.exe
```

### Option 2: Run Directly (Development)
```bash
cd HappyTetris
dotnet run
```

### Option 3: Build and Run
```bash
# Build the project
dotnet build --configuration Release

# Run the executable
.\bin\Release\net8.0-windows\HappyTetris.exe
```

### Option 4: Publish Self-Contained
```bash
# Run the build script
build.bat

# Or use PowerShell
.\build.ps1

# Run the published game
.\publish\HappyTetris.exe
```

## 📦 Creating Windows Installer

### Method 1: WiX Toolset (Recommended)

1. **Install WiX Toolset v4**
   ```bash
   dotnet tool install --global wix
   ```

2. **Build the application**
   ```bash
   build.bat
   ```

3. **Create MSI installer**
   ```bash
   wix build -arch x64 -out HappyTetris.msi .\HappyTetris\Installer\Product.wxs
   ```

### Method 2: Inno Setup (Simple)

1. **Download and install** [Inno Setup](https://jrsoftware.org/isdl.php)

2. **Open the setup script**
   - Open `Installer\Setup.iss` in Inno Setup Compiler
   - Click "Build" → "Compile"

3. **Find your installer** in the `Installer\output` folder

### Method 3: Simple Distribution

Just share the contents of the `publish` folder after building! The `HappyTetris.exe` is a self-contained executable that runs on any Windows 10/11 machine.

## 📁 Project Structure

```
HappyTetris/
├── HappyTetris.csproj      # Project file
├── HappyTetris.sln         # Solution file
├── App.xaml                # Application definition
├── App.xaml.cs             # Application code-behind
├── MainWindow.xaml         # Main game window UI
├── MainWindow.xaml.cs      # Main window logic
├── app.manifest            # Application manifest
│
├── Models/                 # Data models
│   ├── Piece.cs            # Tetromino piece definitions
│   ├── Board.cs            # Game board logic
│   └── Player.cs           # Player state management
│
├── Engine/                 # Game engine
│   └── GameEngine.cs       # Core game logic
│
├── Audio/                  # Audio system
│   └── AudioController.cs  # Sound effect controller
│
├── Installer/              # Installer configurations
│   ├── Product.wxs         # WiX Toolset config
│   └── Setup.iss           # Inno Setup script
│
├── build.bat               # Windows build script
├── build.ps1               # PowerShell build script
├── run.bat                 # Quick run script
└── README.md               # This file
```

## 🎨 Color Scheme

The game features a traditional Chinese color palette with high contrast for elderly visibility:

| Color | Hex Code | Usage |
|-------|----------|-------|
| Chinese Red | #c41e3a | Primary accent |
| Chinese Gold | #ffd700 | Highlights, borders |
| Chinese Yellow | #ffcc00 | Secondary accent |
| Chinese Orange | #ff6b35 | Piece color |
| Cream | #fff8e7 | Text - high contrast |
| Jade | #00a86b | Success states, tips |

## 🎵 Audio Features

The game includes synthesized audio for:
- Full-length Type-A Tetris theme loop (note name + pitch + duration score)
- Slower background music tempo tuned for older adult players
- Separate toggles for `Game Music` and `Sound Effects` from one 🔊 options button
- Piece movement
- Piece rotation
- Piece drop (low bass "boom" impact)
- Line clear
- Level up
- Game over
- Game start

*Note: Click the 🔊 button to open audio options and toggle music/effects independently*

## 🏆 Scoring Details

| Lines Cleared | Base Points |
|---------------|-------------|
| 1 | 100 |
| 2 | 300 |
| 3 | 500 |
| 4 | 800 |

**Total Score = Base Points × Current Level**

## 🔧 Development

### Build Commands

```bash
# Restore dependencies
dotnet restore

# Build Debug
dotnet build

# Build Release
dotnet build --configuration Release

# Run
dotnet run

# Publish
dotnet publish --configuration Release --runtime win-x64 --self-contained true -p:PublishSingleFile=true -o publish
```

### Debugging

1. Open `HappyTetris.sln` in Visual Studio 2022
2. Set breakpoints in code
3. Press F5 to start debugging

## 👴 Elderly-Friendly Features

This game is specifically designed for seniors who grew up playing classic Nintendo games:

1. **Vision Considerations**:
   - Extra-large game board and pieces
   - High contrast color scheme
   - Large, readable fonts
   - Thicker borders and grid lines

2. **Motor Control Considerations**:
   - Large button click targets
   - Simple keyboard controls (arrow keys + space)
   - Slower game speed for more reaction time

3. **Cognitive Considerations**:
   - Clear, simple instructions
   - Bilingual labels for clarity
   - Familiar gameplay from their youth
   - Helpful tips displayed on screen

4. **Accessibility**:
   - Fullscreen mode for better focus
   - Audio cues for feedback
   - Pause function for breaks
   - No time pressure at lower levels

## 📝 License

This project is for educational and personal entertainment purposes.

## 🙏 Acknowledgments

- Original Tetris concept by Alexey Pajitnov
- Built with .NET 8.0 and WPF
- Audio powered by NAudio library
- Designed with love for the elderly community

## 📧 Contact

For questions or feedback, please open an issue on the repository.

---

**Enjoy the game! 祝您游戏愉快！** 🎮🀄
