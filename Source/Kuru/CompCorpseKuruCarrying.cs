using Verse;
using System;

namespace Kuru
{
    public class CompCorpseKuruCarrying : ThingComp
    {
        public KuruCause cause = KuruCause.None;

        public void InitializeCorpse()
        {
            //Log.Message("[KuruMod] CompCorpseKuruCarrying.InitializeCorpse");

            var corpsePawn = ((Corpse)this.parent).InnerPawn;

            this.cause = KuruCauseUtils.CauseFromPawn(corpsePawn);

            //Log.Message("[KuruMod] Kuru lottery result: " + this.cause);
        }

        public override void PostIngested(Pawn ingester)
        {
            //Log.Message("[KuruMod] eaten CompCorpseKuruCarrying " + this.cause);
            KuruModStatic.AddFoodKuruHediffByCause(ingester, this.parent, this.cause);
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref this.cause, "kuruCause", KuruCause.None);
        }
    }

    public class CompCorpseKuruCarryingProperties : CompProperties
    {
        public CompCorpseKuruCarryingProperties()
        {
            this.compClass = typeof(CompCorpseKuruCarrying);
        }

        public CompCorpseKuruCarryingProperties(Type compClass) : base(compClass)
        {
            this.compClass = compClass;
        }
    }
}