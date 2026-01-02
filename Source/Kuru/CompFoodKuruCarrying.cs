using RimWorld;
using Verse;
using System;

namespace Kuru
{
    public class CompFoodKuruCarrying : ThingComp
    {
        public CompFoodKuruCarryingProperties Props => (CompFoodKuruCarryingProperties)this.props;

        public override void PostSplitOff(Thing piece)
        {
            base.PostSplitOff(piece);
            var comp = piece.TryGetComp<CompFoodKuruCarrying>();
            comp.Props.Cause = this.Props.Cause;
        }

        public override void PreAbsorbStack(Thing otherStack, int count)
        {
            base.PreAbsorbStack(otherStack, count);
            var comp = otherStack.TryGetComp<CompFoodKuruCarrying>();
            if (this.Props.Cause == KuruCause.Unknown && comp.Props.Cause != KuruCause.Unknown)
                this.Props.Cause = comp.Props.Cause;
        }

        public override void PostIngested(Pawn ingester)
        {
            //Log.Message("[KuruMod] eaten CompFoodKuruCarrying " + this.Props.Cause);
            KuruModStatic.AddFoodKuruHediffByCause(ingester, this.parent, this.Props.Cause);
        }
    }

    public class CompFoodKuruCarryingProperties : CompProperties
    {
        public KuruCause Cause { get; set; } = KuruCause.None;

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