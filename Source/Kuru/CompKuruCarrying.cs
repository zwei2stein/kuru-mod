using Verse;

namespace Kuru
{
    public abstract class CompKuruCarrying : ThingComp
    {
        public KuruCause cause = KuruCause.None;

        public override void PostIngested(Pawn ingester)
        {
            HediffKuruUtils.AddFoodKuruHediffByCause(ingester, this.parent, this.cause);
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look<KuruCause>(ref this.cause, "kuruCause", KuruCause.None);
        }
    }
}