using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace Kuru
{
    public class MentalState_LaughingFitAll : MentalState
    {
        public Pawn target;
        public bool laughedAtTargetAtLeastOnce;
        public int lastLaughTicks = -999999;

        private int targetFoundTicks;

        private const int CheckChooseNewTargetIntervalTicks = 250;
        private const int MaxSameTargetChaseTicks = 1250;
        private static readonly List<Pawn> candidates = new List<Pawn>();

        public override void PostStart(string reason)
        {
            base.PostStart(reason);
            this.ChooseNextTarget();
        }

        public override void MentalStateTick(int delta)
        {
            if (this.target != null && !InsultingSpreeMentalStateUtility.CanChaseAndInsult(this.pawn, this.target))
                this.ChooseNextTarget();
            if (this.pawn.IsHashIntervalTick(CheckChooseNewTargetIntervalTicks, delta) &&
                (this.target == null || this.laughedAtTargetAtLeastOnce))
                this.ChooseNextTarget();
            base.MentalStateTick(delta);
        }

        private void ChooseNextTarget()
        {
            InsultingSpreeMentalStateUtility.GetInsultCandidatesFor(this.pawn, candidates);

            if (!candidates.Any())
            {
                this.target = null;
                this.laughedAtTargetAtLeastOnce = false;
                this.targetFoundTicks = -1;
                return;
            }

            var keepCurrentTarget = this.target == null ||
                Find.TickManager.TicksGame - this.targetFoundTicks <= MaxSameTargetChaseTicks ||
                !candidates.Any(x => x != this.target);

            var candidatePool = keepCurrentTarget ? candidates : candidates.Where(x => x != this.target);
            var chosen = candidatePool.RandomElementByWeight(GetCandidateWeight);

            if (chosen == this.target)
                return;

            this.target = chosen;
            this.laughedAtTargetAtLeastOnce = false;
            this.targetFoundTicks = Find.TickManager.TicksGame;
        }

        private float GetCandidateWeight(Pawn candidate)
        {
            return 1f - Mathf.Min(this.pawn.Position.DistanceTo(candidate.Position) / 40f, 1f) + 0.01f;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look<Pawn>(ref this.target, "target");
            Scribe_Values.Look<bool>(ref this.laughedAtTargetAtLeastOnce, "laughedAtTargetAtLeastOnce");
            Scribe_Values.Look<int>(ref this.lastLaughTicks, "lastLaughTicks");
            Scribe_Values.Look<int>(ref this.targetFoundTicks, "targetFoundTicks");
        }

        public override RandomSocialMode SocialModeMax() => RandomSocialMode.Off;
    }

    public class MentalStateWorker_LaughingFitAll : MentalStateWorker
    {
        private static readonly List<Pawn> candidates = new List<Pawn>();

        public override bool StateCanOccur(Pawn pawn)
        {
            if (!base.StateCanOccur(pawn))
                return false;

            InsultingSpreeMentalStateUtility.GetInsultCandidatesFor(pawn, candidates);
            var canOccur = candidates.Count >= 2;
            candidates.Clear();
            return canOccur;
        }
    }

    public class JobGiver_LaughingFit : ThinkNode_JobGiver
    {
        protected override Job TryGiveJob(Pawn pawn)
        {
            if (!(pawn.MentalState is MentalState_LaughingFitAll mentalState) || mentalState.target == null ||
                !pawn.CanReach(mentalState.target, PathEndMode.Touch, Danger.Deadly))
                return null;

            return !SocialInteractionUtility.BestInteractableCell(pawn, mentalState.target).IsValid
                ? null
                : JobMaker.MakeJob(KuruDefOf.KuruMod_LaughAt, mentalState.target);
        }
    }

    public class JobDriver_LaughAt : JobDriver
    {
        private const TargetIndex TargetInd = TargetIndex.A;

        private Pawn Target => (Pawn)this.pawn.CurJob.GetTarget(TargetIndex.A).Thing;

        public override bool TryMakePreToilReservations(bool errorOnFailed) => true;

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDespawnedOrNull(TargetIndex.A);
            yield return Toils_Interpersonal.GotoInteractablePosition(TargetIndex.A);
            yield return this.LaughingFitDelayToil();
            yield return Toils_Interpersonal.WaitToBeAbleToInteract(this.pawn);

            var gotoToil = Toils_Interpersonal.GotoInteractablePosition(TargetIndex.A);
            gotoToil.socialMode = RandomSocialMode.Off;
            yield return gotoToil;

            yield return this.InteractToil();
        }

        private Toil InteractToil()
        {
            return Toils_General.Do(() =>
            {
                if (!this.pawn.interactions.TryInteractWith(this.Target, KuruDefOf.KuruMod_DisturbingLaugh) ||
                    !(this.pawn.MentalState is MentalState_LaughingFitAll mentalState))
                    return;

                mentalState.lastLaughTicks = Find.TickManager.TicksGame;
                if (mentalState.target != this.Target)
                    return;

                mentalState.laughedAtTargetAtLeastOnce = true;
            });
        }

        private Toil LaughingFitDelayToil()
        {
            var toil = ToilMaker.MakeToil(nameof(LaughingFitDelayToil));
            toil.initAction = WaitAction;
            toil.tickIntervalAction = delta => WaitAction();
            toil.socialMode = RandomSocialMode.Off;
            toil.defaultCompleteMode = ToilCompleteMode.Never;
            return toil;

            void WaitAction()
            {
                if (this.pawn.MentalState is MentalState_LaughingFitAll mentalState &&
                    Find.TickManager.TicksGame - mentalState.lastLaughTicks < 1200)
                    return;

                this.pawn.jobs.curDriver.ReadyForNextToil();
            }
        }
    }
}