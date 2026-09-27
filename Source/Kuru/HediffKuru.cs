using System;
using RimWorld;
using Verse;

namespace Kuru
{
    public class HediffKuru : Hediff
    {
        private static readonly Random Rand = new Random(); 
        
        private int lastBrainDamageTick = 0;
        private float nextBrainDamageIn = 1;
        private bool removalScheduled = false;
        
        public override bool ShouldRemove => this.removalScheduled || base.ShouldRemove;
        
        public override string LabelBase =>
            (pawn?.RaceProps != null && !pawn.RaceProps.Humanlike)
                ? (string)"Kuru_MadAnimalDisease".Translate(pawn.def.label.Named("SPECIES"))
                : base.LabelBase;
        
        public override void TickInterval(int delta)
        {
            base.TickInterval(delta);
            
            // we are storing only std dev for next event because storing ticks means that when user changes settings, 
            // we would have wrong time scheduled, and it would tick too early or too late.
            var mean = KuruModSettings.progressionSpeed.ToTicks();
            var deviation = mean / 4.0f; // standard deviation from mean is 1/4 of mean.
            var nextBrainDamageInTicks = mean + (int)(deviation * this.nextBrainDamageIn);

            if (this.ageTicks > this.lastBrainDamageTick + nextBrainDamageInTicks)
            {
                if (pawn.health.hediffSet.GetBrain() == null)
                {
                    //Kuru makes no sense without brain.
                    this.removalScheduled = true;
                    return;
                }
                if (KuruModSettings.luciferiumCures && pawn.health.hediffSet.GetFirstHediffOfDef(KuruDefOf.LuciferiumAddiction) != null)
                {
                    this.removalScheduled = true;
                    if (HediffKuruUtils.ShouldNotifyAboutKuru(pawn))
                        Messages.Message(
                            "MessageHealedKuruLuciferium".Translate(pawn.Named("PAWN"), LabelBase.CapitalizeFirst().Named("DISEASE")), 
                            (LookTargets) (Thing) pawn,
                            MessageTypeDefOf.PositiveEvent);
                    if (pawn.needs?.mood != null)
                        pawn.needs.mood.thoughts.memories.TryGainMemory(KuruDefOf.KuruMod_KuruCured);
                    return;
                }
                if (KuruModSettings.naturalCannibalCures && ModsConfig.BiotechActive && pawn.genes != null && pawn.genes.HasActiveGene(KuruDefOf.KuruMod_NaturalCannibal))
                {
                    this.removalScheduled = true;
                    if (HediffKuruUtils.ShouldNotifyAboutKuru(pawn))
                        Messages.Message(
                            "MessageHealedKuruNaturalCannibal".Translate(pawn.Named("PAWN")), 
                            (LookTargets) (Thing) pawn,
                            MessageTypeDefOf.PositiveEvent);
                    if (pawn.needs?.mood != null)
                        pawn.needs.mood.thoughts.memories.TryGainMemory(KuruDefOf.KuruMod_KuruCured);
                    return;
                }
                
                // random number from normal distribution, we store it instead of final tick count.
                this.nextBrainDamageIn = Verse.Rand.Gaussian(0f, 1f);
                this.lastBrainDamageTick = this.ageTicks;
                
                var crush = HediffMaker.MakeHediff(KuruDefOf.KuruMod_BrainDamage, pawn, pawn.health.hediffSet.GetBrain());
                var comp = crush.TryGetComp<HediffComp_GetsPermanent>();
                comp.IsPermanent = true;
                crush.Severity = 1.0f;
                
                // if we add final fatal brain damage, we might destroy brain and remove kuru infection.
                // so we just kill pawn instead.
                if (pawn.health.WouldDieAfterAddingHediff(crush))
                {
                    pawn.Kill(null, this);
                }
                else
                {
                    pawn.health.AddHediff(crush);
                    if (HediffKuruUtils.ShouldNotifyAboutKuru(pawn))
                        Messages.Message(
                            "MessageProgressedKuru".Translate(pawn.Named("PAWN"), LabelBase.CapitalizeFirst().Named("DISEASE")), 
                            (Thing) pawn,
                            MessageTypeDefOf.NegativeEvent);
                    if (pawn.needs?.mood != null)
                        pawn.needs.mood.thoughts.memories.TryGainMemory(KuruDefOf.KuruMod_KuruAttack);
                }

            }
            
        }
        
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<int>(ref this.lastBrainDamageTick, "lastBrainDamageTick", 0);
            Scribe_Values.Look<float>(ref this.nextBrainDamageIn, "nextBrainDamageIn", 1);
            Scribe_Values.Look<bool>(ref this.removalScheduled, "removalScheduled", false);
        }
        
    }
}