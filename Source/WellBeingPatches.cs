using System;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;

namespace ComfortableFishing
{
    /// <summary>
    /// Harmony patch to provide recreation bonus while fishing from chairs
    /// Patches Pawn tick to give recreation/joy when fishing from chairs
    /// </summary>
    [HarmonyPatch(typeof(Pawn), "TickRare")]
    public static class Pawn_Recreation_Patch
    {
        static void Postfix(Pawn __instance)
        {
            try
            {
                if (!ComfortableFishingMod.Settings.enableChairFishingBonus || 
                    !ComfortableFishingMod.Settings.enableRecreationBonus)
                    return;

                Pawn pawn = __instance;
                if (pawn?.needs?.joy == null || pawn?.CurJob?.def?.defName != "Fish")
                    return;

                if (!FishingChairUtility.IsPawnSittingOnChair(pawn))
                    return;

                Building chair = FishingChairUtility.GetChairPawnIsSittingOn(pawn);
                if (chair != null)
                {
                    Job currentJob = pawn.CurJob;
                    if (currentJob?.targetA.Cell != null && 
                        FishingChairUtility.IsChairValidForFishing(chair.Position, currentJob.targetA.Cell, pawn.Map))
                    {
                        // Apply chair quality multiplier if enabled
                        float qualityMultiplier = FishingChairUtility.GetChairQualityMultiplier(chair);
                        
                        // Give recreation (TickRare is called every 250 ticks, so multiply accordingly)
                        float recreationGain = ComfortableFishingMod.Settings.recreationGainRate * 250f * qualityMultiplier;
                        pawn.needs.joy.GainJoy(recreationGain, null); // null JoyKindDef means general joy
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error("[Comfortable Fishing] Error in Recreation patch: " + ex);
            }
        }
    }

    /// <summary>
    /// Harmony patch to provide comfort bonus while fishing from chairs
    /// Patches the comfort system to provide high comfort when fishing from chairs
    /// </summary>
    [HarmonyPatch(typeof(Need_Comfort), "CurInstantLevel", MethodType.Getter)]
    public static class Need_Comfort_ChairFishing_Patch
    {
        static void Postfix(Need_Comfort __instance, ref float __result)
        {
            try
            {
                if (!ComfortableFishingMod.Settings.enableChairFishingBonus || 
                    !ComfortableFishingMod.Settings.enableComfortBonus)
                    return;

                // Use reflection to access the private pawn field
                var pawnField = typeof(Need).GetField("pawn", BindingFlags.NonPublic | BindingFlags.Instance);
                if (pawnField == null) return;
                
                Pawn pawn = pawnField.GetValue(__instance) as Pawn;
                if (pawn?.CurJob?.def?.defName != "Fish" || !FishingChairUtility.IsPawnSittingOnChair(pawn))
                    return;

                Building chair = FishingChairUtility.GetChairPawnIsSittingOn(pawn);
                if (chair != null)
                {
                    Job currentJob = pawn.CurJob;
                    if (currentJob?.targetA.Cell != null && 
                        FishingChairUtility.IsChairValidForFishing(chair.Position, currentJob.targetA.Cell, pawn.Map))
                    {
                        // Apply chair quality multiplier if enabled
                        float qualityMultiplier = FishingChairUtility.GetChairQualityMultiplier(chair);
                        float adjustedComfortLevel = ComfortableFishingMod.Settings.comfortLevel * qualityMultiplier;
                        
                        // Override comfort level while fishing from chair
                        __result = Math.Max(__result, adjustedComfortLevel);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error("[Comfortable Fishing] Error in Comfort patch: " + ex);
            }
        }
    }
}
