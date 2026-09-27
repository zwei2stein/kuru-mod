using RimWorld;
using Verse;

namespace Kuru
{
    public class HediffKuruUtils
    {
        // pawn.Faction == Faction.OfPlayer -> so that ghouls and tamed wildmen can get messages too.
        public static bool ShouldNotifyAboutKuru(Pawn pawn)
        {
            if (PawnUtility.ShouldSendNotificationAbout(pawn))
                return true;
 
            return pawn.IsSubhuman && pawn.Faction == Faction.OfPlayer;
        }

        public static void AddFoodKuruHediffByCause(Pawn pawn, Thing ingestible, KuruCause cause)
        {
            if (cause == KuruCause.None || pawn == null)
                return;

            if (!Rand.Chance(KuruModSettings.baseKuruInfectionChance * cause.GetKuruCarrierChance()))
                return;

            if (pawn.health.hediffSet.GetFirstHediffOfDef(KuruDefOf.KuruMod_Kuru) == null)
            {
                pawn.health.AddHediff(HediffMaker.MakeHediff(KuruDefOf.KuruMod_Kuru, pawn,
                    pawn.health.hediffSet.GetBrain()));
                if (pawn.needs?.mood != null)
                    pawn.needs.mood.thoughts.memories.TryGainMemory(KuruDefOf.KuruMod_ContractedKuru);
                
                if (ingestible == null)
                    return; //pawn was just generated

                if (!ShouldNotifyAboutKuru(pawn) ||
                    !MessagesRepeatAvoider.MessageShowAllowed("MessageFoodKuru-" + pawn.thingIDNumber.ToString(), 0.1f))
                    return;

                var diseaseName = pawn.health.hediffSet.GetFirstHediffOfDef(KuruDefOf.KuruMod_Kuru).LabelBase;
                
                Messages.Message(
                    "MessageFoodKuru".Translate(pawn.Named("PAWN"), ingestible.Named("FOOD"), diseaseName.CapitalizeFirst().Named("DISEASE"))
                        .CapitalizeFirst(), (Thing)pawn, MessageTypeDefOf.NegativeEvent);

            }
        }
    }
}