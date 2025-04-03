# LIARSBAR_UTILS

A utility mod for the game Liar's Bar that adds gameplay enhancements and quality of life features.

## Features

### Card Tracker
- Track all players' cards in a convenient UI window
- Monitor which cards are currently active/inactive for each player
- Keep track of cards that have been played in chronological order
- Clear played cards history with a single key press

### Card Control
- Change all your cards to a specific type using hotkeys
  - F1: King
  - F2: Queen
  - F3: Ace
  - F4: Joker
- F5: Clear played cards history

### Player Control
- Toggle enhanced player movement controls with the semicolon (;) key
- WASD or Arrow Keys for horizontal movement
- Mouse wheel for vertical movement
- Sync player rotation to camera direction when holding right mouse button
- Adjustable movement speeds

## Installation

1. Make sure you have [MelonLoader](https://melonwiki.xyz/#/) installed
2. Download the latest release of LIARSBAR_UTILS from the releases page
3. Place the `LIARSBAR_UTILS.dll` file in your Liar's Bar `Mods` folder
4. Launch the game

## Usage

### Card Tracker UI
- A draggable window will appear showing the cards for all players
- The window updates in real-time as cards are played
- Your player will be indicated with "(You)" next to your name
- Active cards are shown first, followed by played card groups in brackets

### Movement Controls
- Press the semicolon (;) key to toggle enhanced movement controls
- When enabled:
  - Use WASD or arrow keys to move horizontally
  - Use mouse wheel to move up/down
  - Hold right mouse button to sync player rotation with camera

### Card Type Controls
- Press F1-F4 to change all your cards to a specific type
- Press F5 to clear your played card history from the UI

## Configuration

Currently, the mod does not have a configuration file, but you can modify the following variables in the code if you compile it yourself:

- `PlayerController.cs`: Adjust movement speed, vertical speed, and rotation speed
- `UIManager.cs`: Customize the UI window position and dimensions
- `Core.cs`: Change the position control toggle key

## Compatibility

- Developed for Liar's Bar version [game version]
- Requires MelonLoader v0.5.7 or higher

## Known Issues

- Position control may interfere with regular game teleportation
- The mod does not currently sync position changes with other players' games

## Credits

- Developer: thultz
- Built with MelonLoader and Il2CppInterop

## License

This project is licensed under the MIT License - see the LICENSE file for details.

## Disclaimer

This mod is not affiliated with or endorsed by Curve Animation, the developers of Liar's Bar. Use at your own risk.
