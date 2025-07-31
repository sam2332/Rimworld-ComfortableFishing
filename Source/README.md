# Comfortable Fishing - Modular Structure

This mod has been refactored into a single-use module design for better maintainability and organization.

## Module Structure

### Core Files
- **ModCore.cs** - Main mod initialization and Harmony setup
- **ModSettings.cs** - All mod settings and configuration data
- **ModInterface.cs** - Settings UI and user interface components

### Utility Modules
- **ChairUtility.cs** - Chair detection, validation, and quality calculations

### Feature Patches
- **FishingPatches.cs** - Core fishing mechanics (speed, yield, spot selection)
- **WellBeingPatches.cs** - Recreation and comfort bonuses
- **StressPatches.cs** - Mental break reduction and stress management

## Design Principles

### Single Responsibility
Each module has a single, clear purpose:
- ModCore: Initialization only
- ModSettings: Configuration storage only  
- ModInterface: UI rendering only
- ChairUtility: Chair-related logic only
- Each patch file: One specific aspect of gameplay

### Minimal Dependencies
- Modules only depend on what they absolutely need
- Shared utilities are accessed through the ChairUtility class
- Settings are accessed through the static ModInterface.Settings property

### Clean Separation
- UI code is separate from game logic
- Patches are grouped by functionality, not by target class
- Utility functions are isolated and testable

## Benefits

1. **Easier Maintenance** - Changes to one feature don't affect others
2. **Better Testing** - Each module can be tested independently
3. **Clearer Code** - Each file has a specific, obvious purpose
4. **Reduced Conflicts** - Smaller modules reduce merge conflicts
5. **Better Performance** - Only needed modules are loaded/processed

## Usage

The mod works exactly the same as before - all functionality is preserved. The only change is internal organization. Build the project normally with:

```bash
dotnet build Source/Project.csproj --configuration Release
```

All modules will be compiled into a single `ComfortableFishing.dll` assembly.
