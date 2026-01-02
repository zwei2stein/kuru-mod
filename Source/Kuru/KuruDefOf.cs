using RimWorld;
using Verse;

namespace Kuru
{
    [DefOf]
    public class KuruDefOf
    {
        public static HediffDef Kuru;

        public static HediffDef BrainDamage; //Caused by kuru

        public static TraitDef Cannibal; //Vanilla core cannibal trait def

        public static JobDef LaughAt;

        public static InteractionDef DisturbingLaugh;

        static KuruDefOf() => DefOfHelper.EnsureInitializedInCtor(typeof(KuruDefOf));
    }
}