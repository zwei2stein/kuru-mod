using RimWorld;
using Verse;
using System;

namespace Kuru
{
    public class CompCorpseKuruCarrying : ThingComp
    {
        public CompCorpseKuruCarryingProperties Props => (CompCorpseKuruCarryingProperties)this.props;

        public void InitializeCorpse()
        {
            //Log.Message("[KuruMod] CompCorpseKuruCarrying.InitializeCorpse");

            var corpsePawn = ((Corpse)this.parent).InnerPawn;

            this.Props.Cause = KuruCauseUtils.CauseFromPawn(corpsePawn);

            //Log.Message("[KuruMod] Kuru lottery result: " + this.Props.Cause);
        }

        public override void PostIngested(Pawn ingester)
        {
            //Log.Message("[KuruMod] eaten CompCorpseKuruCarrying " + this.Props.Cause);
            KuruModStatic.AddFoodKuruHediffByCause(ingester, this.parent, this.Props.Cause);
        }
    }

    public class CompCorpseKuruCarryingProperties : CompProperties
    {
        public KuruCause Cause { get; set; } = KuruCause.None;

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