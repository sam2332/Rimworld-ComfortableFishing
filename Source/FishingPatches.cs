using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace ComfortableFishing
{
    /// <summary>
    /// Harmony patch to modify fishing speed and provide skill bonuses when sitting on chairs
    /// </summary>
    [HarmonyPatch(typeof(JobDriver_Fish), "MakeNewToils")]
    public static class JobDriver_Fish_MakeNewToils_Patch
    {
        static void Postfix(JobDriver_Fish __instance, ref IEnumerable<Toil> __result)
        {
            try
            {
                // Early safety checks to prevent null reference exceptions
                if (__instance?.pawn?.Map == null || __result == null)
                    return;

                // Check if settings are available and feature is enabled
                if (ComfortableFishingMod.Settings?.enableChairFishingBonus != true)
                    return;

                var toils = __result.ToList();
                if (toils.Count < 2)
                    return;

                // Find the fishing wait toil - it should have a tickAction and be a WaitWith toil
                Toil waitToil = null;
                for (int i = 0; i < toils.Count; i++)
                {
                    var toil = toils[i];
                    // Look for the wait toil that has a tickAction (skill learning)
                    if (toil?.tickAction != null && i > 0) // Skip the goto toil
                    {
                        waitToil = toil;
                        break;
                    }
                }

                if (waitToil == null)
                    return;

                // Check if we can safely access job data
                Job currentJob = __instance.job;
                if (currentJob?.def == null || currentJob.targetA.Cell == IntVec3.Invalid || currentJob.targetB.Cell == IntVec3.Invalid)
                    return;

                Pawn pawn = __instance.pawn;
                IntVec3 fishingSpot = currentJob.GetTarget(TargetIndex.A).Cell;
                IntVec3 standingSpot = currentJob.GetTarget(TargetIndex.B).Cell;
                
                // Check if standing spot has a chair
                Building chair = standingSpot.GetEdifice(pawn.Map);
                if (chair?.def?.building?.isSittable != true)
                    return;

                // Validate chair for fishing
                if (!FishingChairUtility.IsChairValidForFishing(standingSpot, fishingSpot, pawn.Map))
                    return;

                // Apply speed bonus by modifying the wait toil (if enabled)
                if (ComfortableFishingMod.Settings.enableFishBonus)
                {
                    float speedBonus = ComfortableFishingMod.Settings.speedMultiplier;
                    
                    // Apply chair quality multiplier if enabled
                    float qualityMultiplier = FishingChairUtility.GetChairQualityMultiplier(chair);
                    speedBonus = 1.0f + ((speedBonus - 1.0f) * qualityMultiplier);
                    
                    if (speedBonus > 1.0f)
                    {
                        // Try to modify the toil's duration using reflection
                        var ticksField = typeof(Toil).GetField("ticksLeftToRun", BindingFlags.NonPublic | BindingFlags.Instance);
                        if (ticksField != null)
                        {
                            object ticksValue = ticksField.GetValue(waitToil);
                            if (ticksValue is int originalTicks && originalTicks > 0)
                            {
                                int newTicks = Mathf.RoundToInt(originalTicks / speedBonus);
                                ticksField.SetValue(waitToil, newTicks);
                                
                                // Only show message if in game (not during loading)
                                if (Current.ProgramState == ProgramState.Playing && PawnUtility.ShouldSendNotificationAbout(pawn))
                                {
                                    string message = pawn.LabelShort + " is fishing comfortably from a chair (+" + 
                                                   ((speedBonus - 1f) * 100f).ToString("F0") + "% speed)" ;
                                    Messages.Message(message, pawn, MessageTypeDefOf.PositiveEvent, false);
                                }
                            }
                        }
                    }
                }

                // Add skill bonuses to the tick action (if enabled)
                if (ComfortableFishingMod.Settings.enableSkillBonuses)
                {
                    // Store the original tick action
                    var originalTickAction = waitToil.tickAction;

                    // Create new tick action that includes skill bonuses
                    waitToil.tickAction = () =>
                    {
                        // Call original tick action first (includes normal Animals skill gain)
                        originalTickAction?.Invoke();

                        // Add enhanced skill bonuses if still sitting on chair
                        if (FishingChairUtility.IsPawnSittingOnChair(pawn))
                        {
                            Building currentChair = FishingChairUtility.GetChairPawnIsSittingOn(pawn);
                            if (currentChair != null && 
                                FishingChairUtility.IsChairValidForFishing(currentChair.Position, fishingSpot, pawn.Map))
                            {
                                // Apply chair quality multiplier if enabled
                                float qualityMultiplier = FishingChairUtility.GetChairQualityMultiplier(currentChair);
                                
                                // Enhanced fishing (Animals) skill - multiply the normal gain
                                float fishingMultiplier = ComfortableFishingMod.Settings.fishingSkillMultiplier;
                                fishingMultiplier = 1.0f + ((fishingMultiplier - 1.0f) * qualityMultiplier);
                                float extraFishingXP = 0.025f * (fishingMultiplier - 1.0f);
                                if (extraFishingXP > 0f)
                                {
                                    pawn.skills.Learn(SkillDefOf.Animals, extraFishingXP);
                                }

                                // Intellectual skill - contemplation and observation
                                float intellectualRate = ComfortableFishingMod.Settings.intellectualSkillRate * qualityMultiplier;
                                if (intellectualRate > 0f)
                                {
                                    pawn.skills.Learn(SkillDefOf.Intellectual, intellectualRate);
                                }

                                // Artistic skill - inspiration from nature
                                float artisticRate = ComfortableFishingMod.Settings.artisticSkillRate * qualityMultiplier;
                                if (artisticRate > 0f)
                                {
                                    pawn.skills.Learn(SkillDefOf.Artistic, artisticRate);
                                }
                            }
                        }
                    };
                }
                
                __result = toils;
            }
            catch (Exception ex)
            {
                Log.Error("[Comfortable Fishing] Error in MakeNewToils patch: " + ex);
            }
        }
    }

    /// <summary>
    /// Harmony patch to modify fishing yield when sitting on chairs
    /// </summary>
    [HarmonyPatch(typeof(FishingUtility), "GetCatchesFor")]
    public static class FishingUtility_GetCatchesFor_Patch
    {
        static void Postfix(Pawn pawn, IntVec3 cell, bool animalFishing, ref List<Thing> __result, ref bool rare)
        {
            try
            {
                if (!ComfortableFishingMod.Settings.enableChairFishingBonus || 
                    !ComfortableFishingMod.Settings.enableFishBonus || 
                    __result == null || __result.Count == 0)
                    return;

                if (animalFishing || pawn?.Map == null)
                    return;

                // Check if pawn is sitting on a valid chair for fishing
                if (FishingChairUtility.IsPawnSittingOnChair(pawn))
                {
                    Building chair = FishingChairUtility.GetChairPawnIsSittingOn(pawn);
                    if (chair != null && FishingChairUtility.IsChairValidForFishing(chair.Position, cell, pawn.Map))
                    {
                        // Apply yield bonus
                        float yieldBonus = ComfortableFishingMod.Settings.yieldMultiplier;
                        
                        // Apply chair quality multiplier if enabled
                        float qualityMultiplier = FishingChairUtility.GetChairQualityMultiplier(chair);
                        yieldBonus = 1.0f + ((yieldBonus - 1.0f) * qualityMultiplier);
                        
                        foreach (Thing thing in __result.ToList())
                        {
                            int bonusAmount = Mathf.RoundToInt(thing.stackCount * (yieldBonus - 1f));
                            if (bonusAmount > 0)
                            {
                                thing.stackCount += bonusAmount;
                            }
                        }
                        
                        // Show bonus alert if enabled
                        if (ComfortableFishingMod.Settings.showBonusAlert && PawnUtility.ShouldSendNotificationAbout(pawn))
                        {
                                Messages.Message("Seated Fishing: " + pawn.LabelShort + " caught extra fish from chair comfort!",
                                pawn, MessageTypeDefOf.PositiveEvent, false);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error($"[Comfortable Fishing] Error in GetCatchesFor patch: {ex}");
            }
        }
    }

    /// <summary>
    /// Harmony patch to suggest better standing spots near chairs when possible
    /// </summary>
    [HarmonyPatch(typeof(WorkGiver_Fish), "BestStandSpotFor")]
    public static class WorkGiver_Fish_BestStandSpotFor_Patch
    {
        static void Postfix(Pawn pawn, IntVec3 fishSpot, bool avoidStandingInWater, ref IntVec3 __result)
        {
            try
            {
                if (!ComfortableFishingMod.Settings.enableChairFishingBonus)
                    return;

                // Only try to find chairs if we have a valid result and the original spot isn't already a chair
                if (__result.IsValid)
                {
                    // Check if current result is already a chair
                    Building currentBuilding = __result.GetEdifice(pawn.Map);
                    if (currentBuilding?.def?.building?.isSittable == true)
                        return; // Already found a chair, don't change it

                    // Only suggest chairs if the original spot is valid and reservable
                    if (!pawn.CanReserveSittableOrSpot(__result) || !pawn.CanReserve(fishSpot, 1, -1, ReservationLayerDefOf.Floor))
                        return; // Don't suggest chairs if we can't even reserve the basics

                    Building nearbyChair = FishingChairUtility.FindNearestValidChairForFishing(__result, pawn, fishSpot);
                    if (nearbyChair != null)
                    {
                        IntVec3 chairSittingSpot;
                        if (Toils_Ingest.TryFindFreeSittingSpotOnThing(nearbyChair, pawn, out chairSittingSpot))
                        {
                            // Check if chair position allows fishing to the fish spot AND can be reserved
                            if (GenSight.LineOfSight(chairSittingSpot, fishSpot, pawn.Map, true) &&
                                pawn.CanReserveSittableOrSpot(chairSittingSpot) &&
                                pawn.CanReserve(fishSpot, 1, -1, ReservationLayerDefOf.Floor) &&
                                pawn.CanReserve(chairSittingSpot, 1, -1, ReservationLayerDefOf.Floor))
                            {
                                __result = chairSittingSpot;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error("[Comfortable Fishing] Error in BestStandSpotFor patch: " + ex);
            }
        }
    }
}
