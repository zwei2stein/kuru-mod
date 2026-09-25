using Verse;
using System;

namespace Kuru
{
    public class CompFoodKuruCarrying : ThingComp
    {
        public CompFoodKuruCarryingProperties Props => (CompFoodKuruCarryingProperties)this.props;

        public KuruCause cause = KuruCause.None;

        public override void Initialize(CompProperties props)
        {
            base.Initialize(props);
            this.cause = this.Props.defaultCause;
        }

        public override void PostSplitOff(Thing piece)
        {
            base.PostSplitOff(piece);
            var comp = piece.TryGetComp<CompFoodKuruCarrying>();
            comp.cause = this.cause;
        }

        public override void PreAbsorbStack(Thing otherStack, int count)
        {
            base.PreAbsorbStack(otherStack, count);
            var comp = otherStack.TryGetComp<CompFoodKuruCarrying>();
            if (comp.cause.GetKuruCarrierChance() > this.cause.GetKuruCarrierChance())
                this.cause = comp.cause;
        }

        public override void PostIngested(Pawn ingester)
        {
            //Log.Message("[KuruMod] eaten CompFoodKuruCarrying " + this.cause);
            KuruModStatic.AddFoodKuruHediffByCause(ingester, this.parent, this.cause);
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref this.cause, "kuruCause", KuruCause.None);
        }
    }

    public class CompFoodKuruCarryingProperties : CompProperties
    {
        public KuruCause defaultCause = KuruCause.None;

        public CompFoodKuruCarryingProperties()
        {
            this.compClass = typeof(CompFoodKuruCarrying);
        }

        public CompFoodKuruCarryingProperties(Type compClass) : base(compClass)
        {
            this.compClass = compClass;
        }
    }
}