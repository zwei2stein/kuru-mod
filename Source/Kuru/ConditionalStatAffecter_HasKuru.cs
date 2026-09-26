using RimWorld;
using Verse;

namespace Kuru
{
    public class ConditionalStatAffecter_HasKuru : ConditionalStatAffecter
    {
        public override string Label => "Kuru_StatsReport_HasKuru".Translate();

        public override bool Applies(StatRequest req)
        {
            if (!ModsConfig.BiotechActive || !req.HasThing || !(req.Thing is Pawn thing1))
                return false;

            return thing1.health.hediffSet.GetFirstHediffOfDef(KuruDefOf.KuruMod_Kuru) != null;
        }
    }
}