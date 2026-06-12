using System;
using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace Kuru
{
    [StaticConstructorOnStartup]
    public static class KuruModStatic
    {
        static KuruModStatic()
        {
            //Log.Message("[KuruMod] loading!");

            //These defs are generated runtime, we much patch them in code.
            foreach (var def in DefDatabase<ThingDef>.AllDefs)
            {
                switch (def.defName)
                {
                    case "Corpse_Human":
                        //Log.Message("[KuruMod] Patching " + def.defName);
                        def.comps.Add(new CompCorpseKuruCarryingProperties());
                        break;
                    case "Meat_Human":
                        //Log.Message("[KuruMod] Patching " + def.defName);
                        def.comps.Add(new CompFoodKuruCarryingProperties());
                        break;
                }
            }

            //Run harmony patches
            var harmony = new HarmonyLib.Harmony("KuruMod");
            harmony.PatchAll();

            Log.Message("[KuruMod] loaded!");
        }

        public static void AddFoodKuruHediffByCause(Pawn pawn, Thing ingestible, KuruCause cause)
        {
            if (cause == KuruCause.None || pawn == null)
                return;

            if (!Rand.Chance(KuruModSettings.baseKuruInfectionChance * cause.GetKuruCarrierChance()))
                return;

            if (pawn.health.hediffSet.GetFirstHediffOfDef(KuruDefOf.KuruMod_Kuru) == null)
            {
                pawn.health.AddHediff(HediffMaker.MakeHediff(KuruDefOf.KuruMod_Kuru, pawn, pawn.health.hediffSet.GetBrain()));
            }

            if (ingestible == null) return; //pawn was just generated

            if (!PawnUtility.ShouldSendNotificationAbout(pawn) ||
                !MessagesRepeatAvoider.MessageShowAllowed("MessageFoodKuru-" + pawn.thingIDNumber.ToString(), 0.1f))
                return;
            Messages.Message(
                "MessageFoodKuru".Translate((NamedArgument)pawn.LabelShort,
                        (NamedArgument)ingestible.LabelCapNoCount, pawn.Named("PAWN"), ingestible.Named("FOOD"))
                    .CapitalizeFirst(), (LookTargets) (Thing) pawn, MessageTypeDefOf.NegativeEvent);
        }
    }

    public class KuruModMod : Mod
    {
        private KuruModSettings settings;

        public KuruModMod(ModContentPack content) : base(content)
        {
            this.settings = this.GetSettings<KuruModSettings>();
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            var listingStandard = new Listing_Standard();
            
            var gapWidth = 12f;
            
            listingStandard.Begin(inRect);
            
            listingStandard.CheckboxLabeled(
                "KuruOptions_worldgenPawnsCanBeInfected".Translate(),
                ref KuruModSettings.worldgenPawnsCanBeInfected,
                "KuruOptions_worldgenPawnsCanBeInfected_tooltip".Translate());

            KuruModSettings.baseKuruInfectionChance = listingStandard.SliderLabeled(
                "KuruOptions_baseKuruInfectionChance".Translate() +
                KuruModSettings.baseKuruInfectionChance.ToStringPercent()
                , KuruModSettings.baseKuruInfectionChance, 0f, 1f, 0.5f,
                "KuruOptions_baseKuruInfectionChance_tooltip".Translate());

            if (listingStandard.ButtonTextLabeledPct((string)"KuruOptions_progressionSpeed".Translate(),
                    KuruModSettings.progressionSpeed.ToStringHuman(), 0.6f, TextAnchor.MiddleLeft))
            {
                var options = new List<FloatMenuOption>();
                foreach (ProgressionSpeed progressionSpeed in Enum.GetValues(typeof(ProgressionSpeed)))
                {
                    var localProgressionSpeed = progressionSpeed;
                    options.Add(new FloatMenuOption(localProgressionSpeed.ToStringHuman(),
                        (Action)(() => KuruModSettings.progressionSpeed = localProgressionSpeed)));
                }

                Find.WindowStack.Add((Window)new FloatMenu(options));
            }
            
            listingStandard.CheckboxLabeled(
                "KuruOptions_butcherSkillMatters".Translate(),
                ref KuruModSettings.butcherSkillMatters,
                "KuruOptions_butcherSkillMatters_tooltip".Translate());

            listingStandard.GapLine();

            listingStandard.Label("KuruOptions_infectionSources_label".Translate());

            listingStandard.Indent(gapWidth);
            listingStandard.ColumnWidth -= gapWidth;

            listingStandard.CheckboxLabeled(
                "KuruOptions_infectFromIdeologion".Translate(),
                ref KuruModSettings.infectFromIdeologion,
                "KuruOptions_infectFromIdeologion_tooltip".Translate() + KuruCause.MeatOfPawnWithCannibalIdeology
                    .GetKuruCarrierChance().ToStringPercent());

            listingStandard.CheckboxLabeled(
                "KuruOptions_infectFromRecentIngestion".Translate(),
                ref KuruModSettings.infectFromRecentIngestion,
                "KuruOptions_infectFromRecentIngestion_tooltip".Translate() + KuruCause
                    .MeatOfPawnWhoIngestedHumanMeatRecently.GetKuruCarrierChance().ToStringPercent());

            listingStandard.CheckboxLabeled(
                "KuruOptions_infectFromTraits".Translate(),
                ref KuruModSettings.infectFromTraits,
                "KuruOptions_infectFromTraits_tooltip".Translate() +
                KuruCause.MeatOfPawnWithCannibalTrait.GetKuruCarrierChance().ToStringPercent());
            
            listingStandard.GapLine();
            
            listingStandard.Outdent(gapWidth);
            listingStandard.ColumnWidth += gapWidth;
            
            listingStandard.Label("KuruOptions_cureSources_label".Translate());

            listingStandard.Indent(gapWidth);
            listingStandard.ColumnWidth -= gapWidth;

            listingStandard.CheckboxLabeled(
                "KuruOptions_luciferiumCures".Translate(),
                ref KuruModSettings.luciferiumCures,
                "KuruOptions_luciferiumCures_tooltip".Translate());
            
            listingStandard.CheckboxLabeled(
                "KuruOptions_naturalCannibalCures".Translate(),
                ref KuruModSettings.naturalCannibalCures,
                "KuruOptions_naturalCannibalCures_tooltip".Translate());
            
            listingStandard.End();
            
            base.DoSettingsWindowContents(inRect);
        }

        public override string SettingsCategory()
        {
            return "KuruModName".Translate();
        }
    }

    public static class ProgressionSpeedEnumExtensions
    {
        public static string ToStringHuman(this ProgressionSpeed mode)
        {
            switch (mode)
            {
                case ProgressionSpeed.SECCOND:
                    return "ProgressionSpeed_SECOND".Translate();
                case ProgressionSpeed.DAY:
                    return "ProgressionSpeed_DAY".Translate();
                case ProgressionSpeed.QUADRUM:
                    return "ProgressionSpeed_QUADRUM".Translate();
                case ProgressionSpeed.YEAR:
                    return "ProgressionSpeed_YEAR".Translate();
                default:
                    throw new NotImplementedException();
            }
        }

        public static int ToTicks(this ProgressionSpeed mode)
        {
            switch (mode)
            {
                case ProgressionSpeed.SECCOND:
                    return 60;
                case ProgressionSpeed.DAY:
                    return 60000;
                case ProgressionSpeed.QUADRUM:
                    return 900000;
                case ProgressionSpeed.YEAR:
                    return 3600000;
                default:
                    throw new NotImplementedException();
            }
        }
    }

    public enum ProgressionSpeed : byte
    {
        SECCOND,
        DAY,
        QUADRUM,
        YEAR
    }

}