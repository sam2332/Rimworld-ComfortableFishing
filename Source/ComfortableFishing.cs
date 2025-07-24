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
    [StaticConstructorOnStartup]
    public static class ComfortableFishing
    {
        static ComfortableFishing()
        {
            var harmony = new Harmony("programmerLily.comfortableFishing");
            harmony.PatchAll();
            Log.Message("[Comfortable Fishing] Harmony patches applied successfully.");
        }
    }

    public class ComfortableFishingSettings : ModSettings
    {
        public bool enableChairFishingBonus = true;
        public float yieldMultiplier = 1.25f;
        public float speedMultiplier = 1.15f;
        public bool requireChairInZone = true;
        public int maxChairDistance = 2;
        public bool showBonusAlert = true;

        public override void ExposeData()
        {
            Scribe_Values.Look(ref enableChairFishingBonus, "enableChairFishingBonus", true);
            Scribe_Values.Look(ref yieldMultiplier, "yieldMultiplier", 1.25f);
            Scribe_Values.Look(ref speedMultiplier, "speedMultiplier", 1.15f);
            Scribe_Values.Look(ref requireChairInZone, "requireChairInZone", true);
            Scribe_Values.Look(ref maxChairDistance, "maxChairDistance", 2);
            Scribe_Values.Look(ref showBonusAlert, "showBonusAlert", true);
            base.ExposeData();
        }
    }

    public class ComfortableFishingMod : Mod
    {
        public static ComfortableFishingSettings Settings;

        public ComfortableFishingMod(ModContentPack content) : base(content)
        {
            Settings = GetSettings<ComfortableFishingSettings>();
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            Listing_Standard listingStandard = new Listing_Standard();
            listingStandard.Begin(inRect);

            listingStandard.CheckboxLabeled("Enable Chair Fishing Bonus", ref Settings.enableChairFishingBonus, 
                "When enabled, pawns fishing from chairs get bonuses to speed and yield.");

            if (Settings.enableChairFishingBonus)
            {
                listingStandard.Gap();
                listingStandard.Label("Yield Multiplier: " + Settings.yieldMultiplier.ToString("F2") + "x");
                Settings.yieldMultiplier = listingStandard.Slider(Settings.yieldMultiplier, 1.0f, 2.0f);

                listingStandard.Label("Speed Multiplier: " + Settings.speedMultiplier.ToString("F2") + "x");
                Settings.speedMultiplier = listingStandard.Slider(Settings.speedMultiplier, 1.0f, 2.0f);

                listingStandard.Gap();
                listingStandard.CheckboxLabeled("Require Chair in Fishing Zone", ref Settings.requireChairInZone,
                    "If checked, chairs must be adjacent to fishing zones to provide bonuses (since chairs can't be placed in water).");

                listingStandard.Label("Max Chair Distance from Fishing Zone: " + Settings.maxChairDistance);
                Settings.maxChairDistance = (int)listingStandard.Slider(Settings.maxChairDistance, 0, 5);
                
                listingStandard.Gap();
                listingStandard.CheckboxLabeled("Show Bonus Alert Messages", ref Settings.showBonusAlert,
                    "If checked, shows 'Seated Fishing Bonus Granted' messages when fishing with chair bonuses.");
            }

            listingStandard.End();
            base.DoSettingsWindowContents(inRect);
        }

        public override string SettingsCategory()
        {
            return "Comfortable Fishing";
        }
    }

    public static class FishingChairUtility
    {
        /// <summary>
        /// Checks if a pawn is currently sitting on a chair
        /// </summary>
        public static bool IsPawnSittingOnChair(Pawn pawn)
        {
            if (pawn?.Map == null || pawn.pather.MovingNow || pawn.GetPosture() != PawnPosture.Standing)
                return false;

            Building edifice = pawn.Position.GetEdifice(pawn.Map);
            return edifice?.def?.building?.isSittable == true;
        }

        /// <summary>
        /// Gets the chair the pawn is sitting on, if any
        /// </summary>
        public static Building GetChairPawnIsSittingOn(Pawn pawn)
        {
            if (!IsPawnSittingOnChair(pawn))
                return null;

            return pawn.Position.GetEdifice(pawn.Map);
        }

        /// <summary>
        /// Checks if a chair position is valid for fishing bonuses based on fishing zones
        /// Since chairs cannot be placed in water, they must be adjacent to fishing zones
        /// </summary>
        public static bool IsChairValidForFishing(IntVec3 chairPos, IntVec3 fishingSpot, Map map)
        {
            if (!ComfortableFishingMod.Settings.enableChairFishingBonus)
                return false;

            // Get the fishing zone at the fishing spot
            Zone_Fishing fishZone = fishingSpot.GetZone(map) as Zone_Fishing;
            if (fishZone == null)
                return false; // No fishing zone, no bonus

            if (ComfortableFishingMod.Settings.requireChairInZone)
            {
                // Check if chair is within the fishing zone itself (unlikely since chairs can't be in water)
                Zone_Fishing chairZone = chairPos.GetZone(map) as Zone_Fishing;
                if (chairZone == fishZone)
                    return true; // Chair is somehow in the same fishing zone
                
                // Since chairs can't be in water, check if chair is adjacent/close to the fishing zone
                float distance = chairPos.DistanceTo(fishingSpot);
                if (distance <= ComfortableFishingMod.Settings.maxChairDistance)
                {
                    // Additional check: is the chair close to any cell in the fishing zone?
                    foreach (IntVec3 zoneCell in fishZone.Cells)
                    {
                        if (chairPos.DistanceTo(zoneCell) <= ComfortableFishingMod.Settings.maxChairDistance)
                            return true;
                    }
                }
                
                return false;
            }

            return true; // No zone requirement, any chair is valid
        }

        /// <summary>
        /// Finds the nearest valid chair for fishing from a given position
        /// </summary>
        public static Building FindNearestValidChairForFishing(IntVec3 position, Pawn pawn, IntVec3 fishingSpot)
        {
            if (!ComfortableFishingMod.Settings.enableChairFishingBonus)
                return null;

            Map map = pawn.Map;
            int searchRadius = Math.Max(ComfortableFishingMod.Settings.maxChairDistance + 2, 5);

            // Use a temporary list to collect and sort chairs to reduce conflicts
            List<Building> potentialChairs = new List<Building>();

            // Search in expanding radius
            for (int radius = 1; radius <= searchRadius; radius++)
            {
                foreach (IntVec3 cell in GenRadial.RadialCellsAround(position, radius, true))
                {
                    if (!cell.InBounds(map))
                        continue;

                    Building edifice = cell.GetEdifice(map);
                    if (edifice?.def?.building?.isSittable != true)
                        continue;

                    // Check if chair is valid for fishing
                    if (!IsChairValidForFishing(cell, fishingSpot, map))
                        continue;

                    // ENHANCED: Check if pawn can use this chair with thorough validation
                    if (!pawn.CanReserveSittableOrSpot(cell) || edifice.IsForbidden(pawn))
                        continue;

                    // Check if there's a free sitting spot
                    IntVec3 sittingSpot;
                    if (!Toils_Ingest.TryFindFreeSittingSpotOnThing(edifice, pawn, out sittingSpot))
                        continue;

                    // Additional validation: ensure pawn can actually reserve both spots
                    if (!pawn.CanReserve(fishingSpot, 1, -1, ReservationLayerDefOf.Floor))
                        continue;

                    // CRITICAL: Check if the sitting spot is already reserved by someone else
                    if (!pawn.CanReserve(sittingSpot, 1, -1, ReservationLayerDefOf.Floor))
                        continue;

                    // Check line of sight from chair to fishing spot
                    if (!GenSight.LineOfSight(sittingSpot, fishingSpot, map, true))
                        continue;

                    potentialChairs.Add(edifice);
                }
            }

            // Sort by distance and return the closest one that's actually available
            if (potentialChairs.Count > 0)
            {
                potentialChairs = potentialChairs
                    .OrderBy(chair => chair.Position.DistanceTo(position))
                    .ToList();
                
                // Double-check the closest chair is still available
                foreach (Building chair in potentialChairs)
                {
                    IntVec3 chairSpot;
                    if (Toils_Ingest.TryFindFreeSittingSpotOnThing(chair, pawn, out chairSpot)
                        && pawn.CanReserve(chairSpot, 1, -1, ReservationLayerDefOf.Floor))
                    {
                        return chair;
                    }
                }
            }

            return null;
        }
    }

    /// <summary>
    /// Harmony patch to modify fishing speed when sitting on chairs
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

                // Apply speed bonus by modifying the wait toil
                float speedBonus = ComfortableFishingMod.Settings.speedMultiplier;
                if (speedBonus <= 1.0f)
                    return;

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
                                           ((speedBonus - 1f) * 100f).ToString("F0") + "% speed)";
                            Messages.Message(message, pawn, MessageTypeDefOf.PositiveEvent, false);
                        }
                    }
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
                if (!ComfortableFishingMod.Settings.enableChairFishingBonus || __result == null || __result.Count == 0)
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
                            Messages.Message("Seated Fishing Bonus Granted: " + pawn.LabelShort + " caught extra fish from chair comfort!",
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
