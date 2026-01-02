using System;
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
        private static List<Pawn> candidates = new List<Pawn>();

        public override void PostStart(string reason)
        {
            base.PostStart(reason);
            this.ChooseNextTarget();
        }

        public override void MentalStateTick(int delta)
        {
            if (this.target != null && !InsultingSpreeMentalStateUtility.CanChaseAndInsult(this.pawn, this.target))
                this.ChooseNextTarget();
            if (this.pawn.IsHashIntervalTick(250, delta) && (this.target == null || this.laughedAtTargetAtLeastOnce))
                this.ChooseNextTarget();
            base.MentalStateTick(delta);
        }

        private void ChooseNextTarget()
        {
            InsultingSpreeMentalStateUtility.GetInsultCandidatesFor(this.pawn, MentalState_LaughingFitAll.candidates);
            if (!MentalState_LaughingFitAll.candidates.Any<Pawn>())
            {
                this.target = (Pawn)null;
                this.laughedAtTargetAtLeastOnce = false;
                this.targetFoundTicks = -1;
            }
            else
            {
                Pawn pawn =
                    this.target == null || Find.TickManager.TicksGame - this.targetFoundTicks <= 1250 ||
                    !MentalState_LaughingFitAll.candidates.Any<Pawn>((Predicate<Pawn>)(x => x != this.target))
                        ? MentalState_LaughingFitAll.candidates.RandomElementByWeight<Pawn>(
                            (Func<Pawn, float>)(x => this.GetCandidateWeight(x)))
                        : MentalState_LaughingFitAll.candidates.Where<Pawn>((Func<Pawn, bool>)(x => x != this.target))
                            .RandomElementByWeight<Pawn>((Func<Pawn, float>)(x => this.GetCandidateWeight(x)));
                if (pawn == this.target)
                    return;
                this.target = pawn;
                this.laughedAtTargetAtLeastOnce = false;
                this.targetFoundTicks = Find.TickManager.TicksGame;
            }
        }

        private float GetCandidateWeight(Pawn candidate)
        {
            return (float)(1.0 - (double)Mathf.Min(this.pawn.Position.DistanceTo(candidate.Position) / 40f, 1f) +
                           0.009999999776482582);
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
        private static List<Pawn> candidates = new List<Pawn>();

        public override bool StateCanOccur(Pawn pawn)
        {
            if (!base.StateCanOccur(pawn))
                return false;
            InsultingSpreeMentalStateUtility.GetInsultCandidatesFor(pawn, MentalStateWorker_LaughingFitAll.candidates);
            int num = MentalStateWorker_LaughingFitAll.candidates.Count >= 2 ? 1 : 0;
            MentalStateWorker_LaughingFitAll.candidates.Clear();
            return num != 0;
        }
    }

    public class JobGiver_LaughingFit : ThinkNode_JobGiver
    {
        protected override Job TryGiveJob(Pawn pawn)
        {
            if (!(pawn.MentalState is MentalState_LaughingFitAll mentalState) || mentalState.target == null ||
                !pawn.CanReach((LocalTargetInfo)(Thing)mentalState.target, PathEndMode.Touch, Danger.Deadly))
                return (Job)null;
            return !SocialInteractionUtility.BestInteractableCell(pawn, mentalState.target).IsValid
                ? (Job)null
                : JobMaker.MakeJob(KuruDefOf.KuruMod_LaughAt, (LocalTargetInfo)(Thing)mentalState.target);
        }
    }

    public class JobDriver_LaughAt : JobDriver
    {
        private const TargetIndex TargetInd = TargetIndex.A;

        private Pawn Target => (Pawn)(Thing)this.pawn.CurJob.GetTarget(TargetIndex.A);

        public override bool TryMakePreToilReservations(bool errorOnFailed) => true;

        protected override IEnumerable<Toil> MakeNewToils()
        {
            JobDriver_LaughAt f = this;
            f.FailOnDespawnedOrNull<JobDriver_LaughAt>(TargetIndex.A);
            yield return Toils_Interpersonal.GotoInteractablePosition(TargetIndex.A);
            yield return f.LaughingFitDelayToil();
            yield return Toils_Interpersonal.WaitToBeAbleToInteract(f.pawn);
            Toil toil = Toils_Interpersonal.GotoInteractablePosition(TargetIndex.A);
            toil.socialMode = RandomSocialMode.Off;
            yield return toil;
            yield return f.InteractToil();
        }

        private Toil InteractToil()
        {
            return Toils_General.Do((Action)(() =>
            {
                if (!this.pawn.interactions.TryInteractWith(this.Target, KuruDefOf.KuruMod_DisturbingLaugh) ||
                    !(this.pawn.MentalState is MentalState_LaughingFitAll mentalState2))
                    return;
                mentalState2.lastLaughTicks = Find.TickManager.TicksGame;
                if (mentalState2.target != this.Target)
                    return;
                mentalState2.laughedAtTargetAtLeastOnce = true;
            }));
        }

        private Toil LaughingFitDelayToil()
        {
            var toil = ToilMaker.MakeToil(nameof(LaughingFitDelayToil));
            toil.initAction = (Action)(WaitAction);
            toil.tickIntervalAction = (Action<int>)(delta => WaitAction());
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