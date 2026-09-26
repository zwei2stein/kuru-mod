using Verse;
using System;

namespace Kuru
{
    public class CompCorpseKuruCarrying : CompKuruCarrying
    {

        public void InitializeCorpse()
        { 
            var corpsePawn = ((Corpse)this.parent).InnerPawn;
            this.cause = KuruCauseUtils.CauseFromPawn(corpsePawn);
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