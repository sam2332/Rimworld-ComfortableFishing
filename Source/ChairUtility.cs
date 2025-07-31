using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace ComfortableFishing
{
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

        /// <summary>
        /// Calculates the quality multiplier for a chair based on its comfort stat
        /// </summary>
        public static float GetChairQualityMultiplier(Building chair)
        {
            if (!ComfortableFishingMod.Settings.enableChairQualityBonuses || chair == null)
                return 1.0f;

            try
            {
                // Get the chair's comfort stat
                float chairComfort = chair.GetStatValue(StatDefOf.Comfort, true);
                float baseComfort = ComfortableFishingMod.Settings.baseChairComfort;
                float qualityMultiplier = ComfortableFishingMod.Settings.chairQualityMultiplier;

                // Calculate quality multiplier based on how much the chair's comfort deviates from base
                // Formula: 1.0 + (chairComfort - baseComfort) * (qualityMultiplier - 1.0)
                // Examples with base=0.5, multiplier=1.5:
                // - Chair comfort 0.4: 1.0 + (0.4-0.5) * 0.5 = 0.95x (5% penalty)
                // - Chair comfort 0.5: 1.0 + (0.5-0.5) * 0.5 = 1.0x (baseline)
                // - Chair comfort 0.75: 1.0 + (0.75-0.5) * 0.5 = 1.125x (12.5% bonus)
                // - Chair comfort 0.9: 1.0 + (0.9-0.5) * 0.5 = 1.2x (20% bonus)
                
                float multiplier = 1.0f + (chairComfort - baseComfort) * (qualityMultiplier - 1.0f);
                
                // Clamp to reasonable bounds (0.5x to 2.0x)
                return Mathf.Clamp(multiplier, 0.5f, 2.0f);
            }
            catch (Exception ex)
            {
                Log.Error("[Comfortable Fishing] Error calculating chair quality multiplier: " + ex);
                return 1.0f;
            }
        }

       
    }
}
