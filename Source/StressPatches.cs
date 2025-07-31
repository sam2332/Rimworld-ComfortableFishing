using System;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;

namespace ComfortableFishing
{
    /// <summary>
    /// Harmony patch to reduce stress/mental break chance while fishing from chairs
    /// Patches MentalBreaker to reduce break chance during chair fishing
    /// </summary>
    [HarmonyPatch(typeof(MentalBreaker), "BreakThresholdExtreme", MethodType.Getter)]
    public static class MentalBreaker_StressReduction_Patch
    {
        static void Postfix(MentalBreaker __instance, ref float __result)
        {
            try
            {
                if (!ComfortableFishingMod.Settings.enableChairFishingBonus || 
                    !ComfortableFishingMod.Settings.enableStressReduction)
                    return;

                // Use reflection to access the private pawn field
                var pawnField = typeof(MentalBreaker).GetField("pawn", BindingFlags.NonPublic | BindingFlags.Instance);
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
                        float adjustedStressReduction = ComfortableFishingMod.Settings.stressReductionFactor;
                        adjustedStressReduction = 1.0f + ((adjustedStressReduction - 1.0f) * qualityMultiplier);
                        
                        // Reduce break threshold (higher threshold = less likely to break)
                        __result *= (1f / adjustedStressReduction);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error("[Comfortable Fishing] Error in Stress Reduction patch: " + ex);
            }
        }
    }

    /// <summary>
    /// Harmony patch to also reduce major mental break chance
    /// </summary>
    [HarmonyPatch(typeof(MentalBreaker), "BreakThresholdMajor", MethodType.Getter)]
    public static class MentalBreaker_StressReductionMajor_Patch
    {
        static void Postfix(MentalBreaker __instance, ref float __result)
        {
            try
            {
                if (!ComfortableFishingMod.Settings.enableChairFishingBonus || 
                    !ComfortableFishingMod.Settings.enableStressReduction)
                    return;

                // Use reflection to access the private pawn field
                var pawnField = typeof(MentalBreaker).GetField("pawn", BindingFlags.NonPublic | BindingFlags.Instance);
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
                        float adjustedStressReduction = ComfortableFishingMod.Settings.stressReductionFactor;
                        adjustedStressReduction = 1.0f + ((adjustedStressReduction - 1.0f) * qualityMultiplier);
                        
                        // Reduce break threshold (higher threshold = less likely to break)
                        __result *= (1f / adjustedStressReduction);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error("[Comfortable Fishing] Error in Major Stress Reduction patch: " + ex);
            }
        }
    }

    /// <summary>
    /// Harmony patch to also reduce minor mental break chance
    /// </summary>
    [HarmonyPatch(typeof(MentalBreaker), "BreakThresholdMinor", MethodType.Getter)]
    public static class MentalBreaker_StressReductionMinor_Patch
    {
        static void Postfix(MentalBreaker __instance, ref float __result)
        {
            try
            {
                if (!ComfortableFishingMod.Settings.enableChairFishingBonus || 
                    !ComfortableFishingMod.Settings.enableStressReduction)
                    return;

                // Use reflection to access the private pawn field
                var pawnField = typeof(MentalBreaker).GetField("pawn", BindingFlags.NonPublic | BindingFlags.Instance);
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
                        float adjustedStressReduction = ComfortableFishingMod.Settings.stressReductionFactor;
                        adjustedStressReduction = 1.0f + ((adjustedStressReduction - 1.0f) * qualityMultiplier);
                        
                        // Reduce break threshold (higher threshold = less likely to break)
                        __result *= (1f / adjustedStressReduction);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error("[Comfortable Fishing] Error in Minor Stress Reduction patch: " + ex);
            }
        }
    }
}
