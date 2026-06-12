using RimWorld;
using Verse;

namespace Kuru
{
    public enum KuruCause: byte
    {
        None,
        Unknown,
        MeatOfKuruCarrier,
        MeatOfPawnWithCannibalTrait,
        MeatOfPawnWithCannibalIdeology,
        MeatOfPawnWhoIngestedHumanMeatRecently
    }

    public static class KuruCauseUtils
    {
        
        public static KuruCause CauseFromPawn(Pawn pawn)
        {
            if (pawn == null)
            {
                return KuruCause.None;
            }

            if ((KuruModSettings.luciferiumCures && pawn.health.hediffSet.GetFirstHediffOfDef(KuruDefOf.LuciferiumAddiction) != null)
                || (KuruModSettings.naturalCannibalCures && pawn.genes.HasActiveGene(KuruDefOf.KuruMod_NaturalCannibal)))
            {
                //Luciferium cures kuru, pawn can't spawn with kuru
                //Natural Cannibal gene also heals kuru, so pawn will not spawn with it
                return KuruCause.None;
            }

            if (pawn.health.hediffSet.GetFirstHediffOfDef(KuruDefOf.KuruMod_Kuru) != null)
            {
                return KuruCause.MeatOfKuruCarrier;
            }

            if (KuruModSettings.infectFromIdeologion && pawn.Ideo != null && pawn.Ideo.IdeoCausesHumanMeatCravings())
            {
                return KuruCause.MeatOfPawnWithCannibalIdeology;
            }

            if (KuruModSettings.infectFromRecentIngestion && pawn.mindState.lastHumanMeatIngestedTick != -99999)
            {
                return KuruCause.MeatOfPawnWhoIngestedHumanMeatRecently;
            }

            if (KuruModSettings.infectFromTraits && pawn.story.traits.HasTrait(KuruDefOf.Cannibal))
            {
                return KuruCause.MeatOfPawnWithCannibalTrait;
            }

            return KuruCause.None;
        }

        public static float GetKuruCarrierChance(this KuruCause cause)
        {
            switch (cause)
            {
                case KuruCause.MeatOfKuruCarrier:
                    // 100% is carrier, will infect.
                    return 1.0f;
                case KuruCause.MeatOfPawnWithCannibalIdeology:
                    // 25% is carrier
                    // pawn it from human meat eating culture, kuru is one of defining things about that
                    return 0.25f;
                case KuruCause.MeatOfPawnWhoIngestedHumanMeatRecently:
                    // 5% is carrier
                    // did eat human meat, so, it is possible
                    return 0.05f;
                case KuruCause.MeatOfPawnWithCannibalTrait:
                    // 2.5% is carrier
                    // trait claims pawn did eat meat, so unlikely, but still..
                    return 0.025f;
                case KuruCause.Unknown:
                    // 0.1% is carrier
                    // okay, it us human meat, we have no history for that, so ... unlikely, but possible kuru?
                    return 0.001f;
                case KuruCause.None:
                default:
                    return 0;
            }
        }
    }
}