using UnityEngine;
using Verse;

namespace ComfortableFishing
{
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
        
        // Chair Quality Bonus Settings
        public bool enableChairQualityBonuses = false;
        public float chairQualityMultiplier = 1.5f; // How much chair comfort affects bonuses (1.0 = no effect, 2.0 = double effect)
        public float baseChairComfort = 0.5f; // Baseline comfort value for scaling (chairs below this get penalties)
        
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
            
            // Chair Quality Bonuses
            Scribe_Values.Look(ref enableChairQualityBonuses, "enableChairQualityBonuses", false);
            Scribe_Values.Look(ref chairQualityMultiplier, "chairQualityMultiplier", 1.5f);
            Scribe_Values.Look(ref baseChairComfort, "baseChairComfort", 0.5f);
            
            // General
            Scribe_Values.Look(ref requireChairInZone, "requireChairInZone", true);
            Scribe_Values.Look(ref maxChairDistance, "maxChairDistance", 2);
            Scribe_Values.Look(ref showBonusAlert, "showBonusAlert", true);
            base.ExposeData();
        }
    }
}
