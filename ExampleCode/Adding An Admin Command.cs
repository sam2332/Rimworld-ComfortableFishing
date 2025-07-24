using System.Collections.Generic;
using System.Linq;
using LudeonTK;
using RimWorld;
using UnityEngine;
using Verse;

namespace Verse
{
    public class ModInitializer : Mod
    {
        public ModInitializer(ModContentPack content) : base(content)
        {
            Log.Warning("BrushTool mod: ModInitializer constructor called - mod is loading!");
        }
    }

    [StaticConstructorOnStartup]
    public static class DebugActionsTerrainBrush
    {
        static DebugActionsTerrainBrush()
        {
            Log.Warning("BrushTool mod: DebugActionsTerrainBrush static constructor called - mod is loading!");
        }

        [DebugAction("Map", "TEST - Terrain brush", false, false, false, false, false, 0, false, allowedGameStates = AllowedGameStates.PlayingOnMap, displayPriority = 99)]
        private static List<DebugActionNode> TestTerrainBrush()
        {
            List<DebugActionNode> list = new List<DebugActionNode>();
            list.Add(new DebugActionNode("Test Action")
            {
                action = delegate
                {
                    Log.Message("TEST: Terrain brush debug action is working!");
                }
            });
            return list;
        }
    }
}