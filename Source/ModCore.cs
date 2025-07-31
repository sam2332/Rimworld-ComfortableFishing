using HarmonyLib;
using Verse;

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
}
