using Verse;
using System;

namespace Kuru
{
    public class CompFoodKuruCarrying : CompKuruCarrying
    {
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