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
        // Master toggle
        public bool enableChairFishingBonus = true;
        
        // Bonus Fish Settings
        public bool enableFishBonus = true;
        public float yieldMultiplier = 1.25f;
        public float speedMultiplier = 1.15f;
        
        // Recreation Settings  
        public bool enableRecreationBonus = false;
        public float recreationGainRate = 0.01f; // Recreation gained per tick
        
        // Comfort Settings
        public bool enableComfortBonus = false;
        public float comfortLevel = 0.8f; // Comfort level while fishing from chair
        
        // Stress Reduction Settings
        public bool enableStressReduction = false;
        public float stressReductionFactor = 0.5f; // Multiplier for mental break chance
        
        // Skill Bonus Settings
        public bool enableSkillBonuses = false;
        public float fishingSkillMultiplier = 2.0f; // Multiplier for Animals (fishing) skill XP
        public float intellectualSkillRate = 0.02f; // XP per tick for Intellectual 
        public float artisticSkillRate = 0.015f; // XP per tick for Artistic
        
        // General Settings
        public bool requireChairInZone = true;
        public int maxChairDistance = 2;
        public bool showBonusAlert = true;

        public override void ExposeData()
        {
            Scribe_Values.Look(ref enableChairFishingBonus, "enableChairFishingBonus", true);
            
            // Bonus Fish
            Scribe_Values.Look(ref enableFishBonus, "enableFishBonus", true);
            Scribe_Values.Look(ref yieldMultiplier, "yieldMultiplier", 1.25f);
            Scribe_Values.Look(ref speedMultiplier, "speedMultiplier", 1.15f);
            
            // Recreation
            Scribe_Values.Look(ref enableRecreationBonus, "enableRecreationBonus", false);
            Scribe_Values.Look(ref recreationGainRate, "recreationGainRate", 0.01f);
            
            // Comfort
            Scribe_Values.Look(ref enableComfortBonus, "enableComfortBonus", false);
            Scribe_Values.Look(ref comfortLevel, "comfortLevel", 0.8f);
            
            // Stress Reduction
            Scribe_Values.Look(ref enableStressReduction, "enableStressReduction", false);
            Scribe_Values.Look(ref stressReductionFactor, "stressReductionFactor", 0.5f);
            
            // Skill Bonuses
            Scribe_Values.Look(ref enableSkillBonuses, "enableSkillBonuses", false);
            Scribe_Values.Look(ref fishingSkillMultiplier, "fishingSkillMultiplier", 2.0f);
            Scribe_Values.Look(ref intellectualSkillRate, "intellectualSkillRate", 0.02f);
            Scribe_Values.Look(ref artisticSkillRate, "artisticSkillRate", 0.015f);
            
            // General
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

            listingStandard.CheckboxLabeled("Enable Chair Fishing Bonuses", ref Settings.enableChairFishingBonus, 
                "Master toggle for all chair fishing bonuses.");

            if (Settings.enableChairFishingBonus)
            {
                listingStandard.Gap();
                
                // Bonus Fish Section
                listingStandard.Label("=== BONUS FISH ===".Colorize(Color.cyan));
                listingStandard.CheckboxLabeled("Enable Fish Bonuses", ref Settings.enableFishBonus,
                    "Get extra fish and faster fishing when using chairs.");
                
                if (Settings.enableFishBonus)
                {
                    listingStandard.Label("Yield Multiplier: " + Settings.yieldMultiplier.ToString("F2") + "x");
                    Settings.yieldMultiplier = listingStandard.Slider(Settings.yieldMultiplier, 1.0f, 2.0f);

                    listingStandard.Label("Speed Multiplier: " + Settings.speedMultiplier.ToString("F2") + "x");
                    Settings.speedMultiplier = listingStandard.Slider(Settings.speedMultiplier, 1.0f, 2.0f);
                }
                
                listingStandard.Gap();
                
                // Recreation Section
                listingStandard.Label("=== RECREATION ===".Colorize(Color.green));
                listingStandard.CheckboxLabeled("Enable Recreation Bonus", ref Settings.enableRecreationBonus,
                    "Pawns gain recreation/joy while fishing from chairs.");
                
                if (Settings.enableRecreationBonus)
                {
                    listingStandard.Label("Recreation Gain Rate: " + Settings.recreationGainRate.ToString("F3") + " per tick");
                    Settings.recreationGainRate = listingStandard.Slider(Settings.recreationGainRate, 0.001f, 0.05f);
                }
                
                listingStandard.Gap();
                
                // Comfort Section
                listingStandard.Label("=== COMFORT ===".Colorize(Color.yellow));
                listingStandard.CheckboxLabeled("Enable Comfort Bonus", ref Settings.enableComfortBonus,
                    "Pawns get comfort while fishing from chairs.");
                
                if (Settings.enableComfortBonus)
                {
                    listingStandard.Label("Comfort Level: " + Settings.comfortLevel.ToString("F2"));
                    Settings.comfortLevel = listingStandard.Slider(Settings.comfortLevel, 0.1f, 1.0f);
                }
                
                listingStandard.Gap();
                
                // Stress Reduction Section
                listingStandard.Label("=== STRESS REDUCTION ===".Colorize(Color.magenta));
                listingStandard.CheckboxLabeled("Enable Stress Reduction", ref Settings.enableStressReduction,
                    "Reduce mental break chance while fishing from chairs.");
                
                if (Settings.enableStressReduction)
                {
                    listingStandard.Label("Stress Reduction: " + ((1f - Settings.stressReductionFactor) * 100f).ToString("F0") + "% less mental breaks");
                    Settings.stressReductionFactor = listingStandard.Slider(Settings.stressReductionFactor, 0.1f, 1.0f);
                }
                
                listingStandard.Gap();
                
                // Skill Bonuses Section
                listingStandard.Label("=== SKILL BONUSES ===".Colorize(Color.blue));
                listingStandard.CheckboxLabeled("Enable Skill Bonuses", ref Settings.enableSkillBonuses,
                    "Gain extra skill XP while fishing from chairs.");
                
                if (Settings.enableSkillBonuses)
                {
                    listingStandard.Label("Fishing Skill Multiplier: " + Settings.fishingSkillMultiplier.ToString("F1") + "x");
                    Settings.fishingSkillMultiplier = listingStandard.Slider(Settings.fishingSkillMultiplier, 1.0f, 3.0f);
                    
                    listingStandard.Label("Intellectual XP Rate: " + Settings.intellectualSkillRate.ToString("F3") + " per tick");
                    Settings.intellectualSkillRate = listingStandard.Slider(Settings.intellectualSkillRate, 0.001f, 0.05f);
                    
                    listingStandard.Label("Artistic XP Rate: " + Settings.artisticSkillRate.ToString("F3") + " per tick");
                    Settings.artisticSkillRate = listingStandard.Slider(Settings.artisticSkillRate, 0.001f, 0.05f);
                }
                
                listingStandard.Gap();
                
                // General Settings Section
                listingStandard.Label("=== GENERAL SETTINGS ===".Colorize(Color.white));
                listingStandard.CheckboxLabeled("Require Chair in Fishing Zone", ref Settings.requireChairInZone,
                    "If checked, chairs must be adjacent to fishing zones to provide bonuses (since chairs can't be placed in water).");

                listingStandard.Label("Max Chair Distance from Fishing Zone: " + Settings.maxChairDistance);
                Settings.maxChairDistance = (int)listingStandard.Slider(Settings.maxChairDistance, 0, 5);
                
                listingStandard.CheckboxLabeled("Show Bonus Alert Messages", ref Settings.showBonusAlert,
                    "If checked, shows alert messages when fishing bonuses are active.");
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
                                                   ((speedBonus - 1f) * 100f).ToString("F0") + "% speed)";
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
                                // Enhanced fishing (Animals) skill - multiply the normal gain
                                float extraFishingXP = 0.025f * (ComfortableFishingMod.Settings.fishingSkillMultiplier - 1.0f);
                                if (extraFishingXP > 0f)
                                {
                                    pawn.skills.Learn(SkillDefOf.Animals, extraFishingXP);
                                }

                                // Intellectual skill - contemplation and observation
                                if (ComfortableFishingMod.Settings.intellectualSkillRate > 0f)
                                {
                                    pawn.skills.Learn(SkillDefOf.Intellectual, ComfortableFishingMod.Settings.intellectualSkillRate);
                                }

                                // Artistic skill - inspiration from nature
                                if (ComfortableFishingMod.Settings.artisticSkillRate > 0f)
                                {
                                    pawn.skills.Learn(SkillDefOf.Artistic, ComfortableFishingMod.Settings.artisticSkillRate);
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
                        // Give recreation (TickRare is called every 250 ticks, so multiply accordingly)
                        float recreationGain = ComfortableFishingMod.Settings.recreationGainRate * 250f;
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
                        // Override comfort level while fishing from chair
                        __result = Mathf.Max(__result, ComfortableFishingMod.Settings.comfortLevel);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error("[Comfortable Fishing] Error in Comfort patch: " + ex);
            }
        }
    }

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
                        // Reduce break threshold (higher threshold = less likely to break)
                        __result *= (1f / ComfortableFishingMod.Settings.stressReductionFactor);
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
                        // Reduce break threshold (higher threshold = less likely to break)
                        __result *= (1f / ComfortableFishingMod.Settings.stressReductionFactor);
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
                        // Reduce break threshold (higher threshold = less likely to break)
                        __result *= (1f / ComfortableFishingMod.Settings.stressReductionFactor);
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
