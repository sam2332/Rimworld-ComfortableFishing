MOD NAME: "Comfort Fishing: Chair Bonus System"

GOAL:
Enhance the utility of fishing zones by allowing chairs to be placed within them. When a pawn uses a chair while fishing, they receive a yield bonus (higher catch rates, faster fishing speed, or better quality). This adds immersion, rewards proper setup, and promotes realistic fishing behavior.

KEY FEATURES:

Pawns will prefer to fish from a chair if one is adjacent to or within the fishing zone.

While seated, pawns gain a configurable bonus to fishing yield (e.g., increased speed, success chance, or quality).

Optional: UI feedback (e.g., an icon or buff text) when the pawn is benefiting from chair fishing.

Compatibility with modded chairs and modded fishing zones.

TO IMPLEMENT:

1. Detect Chair Use During Fishing

Identify when a pawn is performing a fishing job (verify which JobDef and Toil sequence is used).

Check whether they are Sitting on a valid Building_Chair or similar during the job.

Only apply the bonus if the chair is correctly positioned adjacent to or inside the fishing zone.

2. Apply Fishing Yield Bonus

Patch the method that determines fishing result/yield (e.g., fish quantity, speed).

Use a Harmony postfix or transpiler to multiply yield values or reduce toil duration if the pawn is seated.

Yield bonus should be tunable via mod settings (e.g., +15% yield, -20% toil time).

4. Validation of Chair Position

Must be adjacent to or within the FishingZone (verify via zone or cell proximity check).

Chairs not meeting the criteria don’t grant bonuses, even if a pawn is sitting in them.

5. Settings Menu (Mod Settings)

Enable/disable chair fishing bonuses

Adjust yield/speed multiplier

ASSUMPTIONS TO VERIFY IN GAME FILES:

The specific JobDriver and Toil used during fishing (varies by fishing mod, e.g., Vanilla Fishing Expanded or others).

The method that calculates the output of a fishing job (likely a TryMakeFish or ProduceFish method).

Whether pawns are naturally inclined to sit while fishing or if job driver needs modification to support this.

How to identify a pawn as “seated” (likely via pather.MovingNow = false and CurrentJobDef + IsSitting or IsIngesting logic).

PSEUDOCODE STRUCTURE:

csharp
Copy
Edit
// Harmony Patch on Fishing Toil
[HarmonyPatch(typeof(FishingJobDriver), "FinalizeFishing")]
class Patch_FishingYieldBonus
{
    static void Postfix(Pawn pawn, ref float yield)
    {
        if (IsSittingOnChair(pawn) && IsChairInFishingZone(pawn.Position))
        {
            yield *= ModSettings.FishingYieldMultiplier;
            // Optional: Add visual feedback
        }
    }

    static bool IsSittingOnChair(Pawn pawn)
    {
        // Check current posture or check for chair building underneath
    }

    static bool IsChairInFishingZone(IntVec3 pos)
    {
        // Query map zone manager to validate if chair is inside or adjacent to a fishing zone
    }
}