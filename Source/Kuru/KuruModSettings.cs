using Verse;

namespace Kuru
{
    public class KuruModSettings : ModSettings
    {
        public static ProgressionSpeed progressionSpeed = ProgressionSpeed.QUADRUM;
        public static float baseKuruInfectionChance = 1f;
        public static bool worldgenPawnsCanBeInfected = true;
        public static bool butcherSkillMatters = true;
        
        public static bool infectFromIdeologion = true;
        public static bool infectFromRecentIngestion = true;
        public static bool infectFromTraits = true;

        public static bool luciferiumCures = true;
        public static bool naturalCannibalCures  = true;

        public override void ExposeData()
        {
            Scribe_Values.Look<ProgressionSpeed>(ref progressionSpeed, "progressionSpeed", ProgressionSpeed.QUADRUM);
            Scribe_Values.Look<float>(ref baseKuruInfectionChance, "baseKuruInfectionChance", 1f);
            Scribe_Values.Look<bool>(ref worldgenPawnsCanBeInfected, "worldgenPawnsCanBeInfected", true);
            Scribe_Values.Look<bool>(ref butcherSkillMatters, "butcherSkillMatters", true);

            Scribe_Values.Look<bool>(ref infectFromIdeologion, "infectFromIdeologion", true);
            Scribe_Values.Look<bool>(ref infectFromRecentIngestion, "infectFromRecentIngestion", true);
            Scribe_Values.Look<bool>(ref infectFromTraits, "infectFromTraits", true);
            
            Scribe_Values.Look<bool>(ref luciferiumCures, "luciferiumCures", true);
            Scribe_Values.Look<bool>(ref naturalCannibalCures, "naturalCannibalCures", true);

            base.ExposeData();
        }
    }
}