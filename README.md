# Comfortable Fishing

A RimWorld mod that enhances fishing by allowing pawns to fish from chairs for bonus yield and speed. Works with any chair from any mod!

## Features

### 🪑 Universal Chair Compatibility
- **Works with ANY chair from ANY mod** - automatically detects chairs using the universal `isSittable` property
- **Future-proof design** - no hardcoded chair types, works with modded furniture automatically
- **Smart detection** - recognizes chairs from vanilla game and all furniture mods

### 🎣 Fishing Bonuses
- **Yield Bonus**: Get more fish when fishing from chairs (default +25%)
- **Speed Bonus**: Fish faster when sitting comfortably (default +15%)
- **Configurable multipliers**: Adjust bonuses from 1.0x to 2.0x via mod settings
- **Balanced gameplay**: Modest bonuses that reward setup without being overpowered

### 🎯 Smart Pawn Behavior
- **Chair preference**: Pawns automatically prefer chairs when available for fishing
- **Zone proximity**: Chairs must be adjacent to fishing zones (since they can't be in water)
- **Intelligent validation**: System checks line of sight, reservations, and accessibility
- **Fallback logic**: Won't break existing fishing jobs if chairs aren't available

### ⚙️ Customizable Settings
Access via **Options → Mod Settings → Comfortable Fishing**:
- **Enable/Disable**: Toggle chair fishing bonuses on/off
- **Yield Multiplier**: 1.0x - 2.0x (default 1.25x = +25% more fish)
- **Speed Multiplier**: 1.0x - 2.0x (default 1.15x = +15% faster)
- **Zone Requirements**: Require chairs to be near fishing zones
- **Max Distance**: How close chairs must be to fishing zones (0-5 tiles)
- **Alert Messages**: Show "Seated Fishing Bonus Granted" notifications

## Installation

1. Subscribe on Steam Workshop OR download from GitHub releases
2. Add to your mod list in RimWorld
3. Load after Core and Odyssey (handled automatically)
4. Start fishing from chairs!

## How It Works

### Chair Detection
The mod automatically detects when pawns are:
- Sitting on a chair (any building with `isSittable = true`)
- Fishing from a valid position
- Within configured distance of fishing zones

### Bonus Application
- **Yield bonuses** are applied when fish are caught
- **Speed bonuses** reduce fishing time
- **Alert messages** notify you when bonuses are active (optional)

### Chair Placement
Since chairs cannot be placed in water:
- Place chairs **adjacent** to your fishing zones
- Default maximum distance is 2 tiles from fishing zones
- Pawns will automatically choose chair positions when available

## Compatibility

### ✅ Compatible With
- **All furniture mods** - any mod that adds chairs will work automatically
- **All fishing mods** - works with base game fishing system
- **Save games** - can be added to existing saves safely
- **Multiplayer** - should work in multiplayer (not extensively tested)

### 📋 Requirements
- **RimWorld 1.6+**
- **Odyssey DLC** (for fishing system)

### 🔧 Load Order
- Core
- Odyssey
- **Comfortable Fishing** (loads automatically after dependencies)
- Other mods

## Configuration Examples

### Relaxed Fishing (Default)
```
Yield Multiplier: 1.25x (+25% fish)
Speed Multiplier: 1.15x (+15% faster)
Require chairs near zones: Yes
Max distance: 2 tiles
```

### High Reward Setup
```
Yield Multiplier: 1.5x (+50% fish)
Speed Multiplier: 1.3x (+30% faster)
Require chairs near zones: Yes
Max distance: 1 tile
```

### Casual Mode
```
Yield Multiplier: 2.0x (+100% fish)
Speed Multiplier: 2.0x (+100% faster)
Require chairs near zones: No
Max distance: 5 tiles
```

## Technical Details

### Implementation
- **Harmony patches** modify fishing behavior without conflicts
- **Reflection-based** for compatibility with game updates
- **Robust error handling** prevents crashes from edge cases
- **Performance optimized** with efficient chair detection

### Files Structure
```
ComfortableFishing/
├── About/
│   ├── About.xml          # Mod metadata
│   └── Preview.png        # Workshop preview
├── Assemblies/
│   └── ComfortableFishing.dll  # Compiled mod
└── Source/
    └── ComfortableFishing.cs   # Source code
```

## Troubleshooting

### Common Issues

**"Pawns won't use chairs for fishing"**
- Check if chairs are within max distance of fishing zones
- Ensure chairs aren't forbidden or reserved
- Verify line of sight between chair and fishing spot

**"No bonuses being applied"**
- Check mod settings - ensure bonuses are enabled
- Verify pawns are actually sitting (not standing next to chairs)
- Make sure chairs are valid for fishing zones

**"Mod settings not appearing"**
- Restart RimWorld after enabling the mod
- Check mod load order (should load after Core/Odyssey)

### Debug Information
Enable "Show Bonus Alert Messages" in settings to see when bonuses are being applied.

## Development

### Building from Source
```bash
dotnet build Source/Project.csproj --configuration Release
```

### Contributing
1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Test thoroughly
5. Submit a pull request

## Credits

- **Author**: ProgrammerLily
- **Version**: 1.0.1
- **RimWorld Version**: 1.6+
- **License**: [License info if applicable]

## Links

- **GitHub**: https://github.com/sam2332/Rimworld-ComfortableFishing
- **Steam Workshop**: [Workshop link when published]
- **Bug Reports**: Use GitHub Issues

---

*Enjoy your comfortable fishing experience! 🎣*