using UnityEngine;
using Verse;

namespace ComfortableFishing
{
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
                
                // Chair Quality Bonuses Section
                listingStandard.Label("=== CHAIR QUALITY BONUSES ===".Colorize(Color.green));
                listingStandard.CheckboxLabeled("Enable Chair Quality Bonuses", ref Settings.enableChairQualityBonuses,
                    "Higher quality/comfort chairs provide better bonuses.");
                
                if (Settings.enableChairQualityBonuses)
                {
                    listingStandard.Label("Quality Multiplier: " + Settings.chairQualityMultiplier.ToString("F1") + "x");
                    Settings.chairQualityMultiplier = listingStandard.Slider(Settings.chairQualityMultiplier, 1.0f, 3.0f);
                    
                    listingStandard.Label("Base Comfort Level: " + Settings.baseChairComfort.ToString("F2"));
                    Settings.baseChairComfort = listingStandard.Slider(Settings.baseChairComfort, 0.3f, 0.8f);
                    
                    Text.Font = GameFont.Tiny;
                    listingStandard.Label("(Chairs above base comfort get bonus multipliers, chairs below get penalties)");
                    Text.Font = GameFont.Small;
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
}
