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

        private float NextStdNormal()
        {
            var u1 = 1.0-Rand.NextDouble(); 
            var u2 = 1.0-Rand.NextDouble();
            return (float) (Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Sin(2.0 * Math.PI * u2));
        }
        
        public override void TickInterval(int delta)
        {
            base.TickInterval(delta);
            
            // we are storing only std dev for next event because storing ticks means that when user changes settings, 
            // we would have wrong time scheduled and it would tick too early or too late.
            var mean = KuruModSettings.progressionSpeed.ToTicks();
            var deviation = mean / 4.0f; // standard deviation from mean is 1/4 of mean.
            var nextBrainDamageInTicks = mean + (int)(deviation * this.nextBrainDamageIn);

            if (this.ageTicks > this.lastBrainDamageTick + nextBrainDamageInTicks)
            {
                // random number from normal distribution, we store it instead of final tick count.
                this.nextBrainDamageIn = NextStdNormal(); 
                this.lastBrainDamageTick = this.ageTicks;
                
                var crush = HediffMaker.MakeHediff(KuruDefOf.BrainDamage, pawn, pawn.health.hediffSet.GetBrain());
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
                    Messages.Message(
                        "MessageProgressedKuru".Translate((NamedArgument)pawn.LabelShort, pawn.Named("PAWN")),
                        MessageTypeDefOf.NegativeEvent);
                }

            }
            
        }
        
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<int>(ref this.lastBrainDamageTick, "lastBrainDamageTick", 0);
            Scribe_Values.Look<float>(ref this.nextBrainDamageIn, "nextBrainDamageIn", 1);
        }
        
    }
}