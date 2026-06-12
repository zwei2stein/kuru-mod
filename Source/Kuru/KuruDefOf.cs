using RimWorld;
using Verse;

namespace Kuru
{
    [DefOf]
    public class KuruDefOf
    {
        public static HediffDef KuruMod_Kuru;
        public static HediffDef KuruMod_BrainDamage; //Caused by kuru
        public static HediffDef LuciferiumAddiction; //Vanilla luciferium
        
        public static TraitDef Cannibal; //Vanilla core cannibal trait def
        
        public static GeneDef KuruMod_NaturalCannibal;

        public static WorkTypeDef Cooking;

        public static JobDef KuruMod_LaughAt;

        public static InteractionDef KuruMod_DisturbingLaugh;

        static KuruDefOf() => DefOfHelper.EnsureInitializedInCtor(typeof(KuruDefOf));
    }
}