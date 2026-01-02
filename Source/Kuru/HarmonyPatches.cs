using System;
using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using Verse;

namespace Kuru
{
    [HarmonyPatch(typeof(PawnGenerator), "GenerateInitialHediffs")]
    public class GenerateInitialHediffsPatch
    {
        static void Prefix(ref Pawn pawn)
        {
            //Log.Message("[KuruMod] Generating initialHediffs for pawn");

            if (!KuruModSettings.worldgenPawnsCanBeInfected) return;

            if (!pawn.RaceProps.Humanlike) return;

            var cause = KuruCauseUtils.CauseFromPawn(pawn);
            KuruModStatic.AddFoodKuruHediffByCause(pawn, null, cause);
        }
    }

    [HarmonyPatch(typeof(Corpse), nameof(Corpse.ButcherProducts))] // if possible use nameof() here
    public class ButcherPatch
    {
        static void Postfix(ref IEnumerable<Thing> __result, ref Corpse __instance, Pawn butcher)
        {
            // propagate kuru causes to butcher results if we have comp (= human corpse, human meat)

            var compCorpseKuruCarrying = __instance.TryGetComp<CompCorpseKuruCarrying>();

            if (compCorpseKuruCarrying == null) return;


            if (KuruModSettings.butcherSkillMatters)
            {
                // 0 - 20 -> 20 skill gives 50% chance of avoiding infections
                var butcherSkill = 0.5f * butcher.skills.AverageOfRelevantSkillsFor(KuruDefOf.Cooking) / 20;

                if (Rand.Chance(butcherSkill))
                {
                    //Log.Message("[KuruMod] butcherSkill " + butcherSkill + " prevented infection");
                    return;
                }
            }

            foreach (var thing in __result)
            {
                if (!thing.TryGetComp(out CompFoodKuruCarrying compFoodKuruCarrying)) continue;
                //Log.Message("[KuruMod] butcher result - applying " + compCorpseKuruCarrying.Props.Cause);
                compFoodKuruCarrying.Props.Cause = compCorpseKuruCarrying.Props.Cause;
            }
        }
    }

    [HarmonyPatch(typeof(Pawn), nameof(Pawn.MakeCorpse),
        new Type[] { typeof(Building_Grave), typeof(bool), typeof(float) })]
    public class PawnPatch
    {
        static void Postfix(ref Corpse __result, ref Pawn __instance)
        {
            var compCorpseKuruCarrying = __result.TryGetComp<CompCorpseKuruCarrying>();

            // Pawn is set to corpse after initializing comps, we must init after corpse is done generating
            compCorpseKuruCarrying?.InitializeCorpse();
        }
    }

    [HarmonyPatch(typeof(GenRecipe), nameof(GenRecipe.MakeRecipeProducts))]
    public class GenRecipePatch
    {
        static void Postfix(ref IEnumerable<Thing> __result, List<Thing> ingredients)
        {
            var bestCause = KuruCause.None;

            //Log.Message("[KuruMod] propagating kuru causes from ingredients to result");

            foreach (var ingredient in ingredients)
            {
                var comp = ingredient.TryGetComp<CompFoodKuruCarrying>();
                if (comp == null) continue;

                if (comp.Props.Cause.GetKuruCarrierChance() >
                    bestCause.GetKuruCarrierChance())
                {
                    bestCause = comp.Props.Cause;
                }
            }

            if (bestCause == KuruCause.None) return;

            //Log.Message("[KuruMod] propagating kuru causes from ingredients to result, best cause: " + bestCause);

            foreach (var result in __result)
            {
                var compFoodKuruCarrying = result.TryGetComp<CompFoodKuruCarrying>();
                if (compFoodKuruCarrying == null) continue;
                //Log.Message("[KuruMod] propagating recipe, setting cause: " + bestCause);
                compFoodKuruCarrying.Props.Cause = bestCause;
            }
        }
    }
}