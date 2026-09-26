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
            var patchedCorpseDefs = new HashSet<ThingDef>();
            var patchedMeatDefs = new HashSet<ThingDef>();

            foreach (var raceDef in DefDatabase<ThingDef>.AllDefs)
            {
                if (raceDef.race == null || !raceDef.race.Humanlike)
                    continue;

                var corpseDef = raceDef.race.corpseDef;
                if (corpseDef != null && patchedCorpseDefs.Add(corpseDef))
                {
                    corpseDef.comps.Add(new CompCorpseKuruCarryingProperties());
                }

                var meatDef = raceDef.race.meatDef;
                if (meatDef != null && patchedMeatDefs.Add(meatDef))
                {
                    meatDef.comps.Add(new CompFoodKuruCarryingProperties { defaultCause = KuruCause.Unknown });
                }
            }

            //Run harmony patches
            var harmony = new HarmonyLib.Harmony("KuruMod");
            harmony.PatchAll();

            Log.Message("[KuruMod] loaded!");
        }
        
    }

    public class KuruModMod : Mod
    {
        private KuruModSettings settings;

        public KuruModMod(ModContentPack content) : base(content)
        {
            settings = GetSettings<KuruModSettings>();
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
                "KuruOptions_baseKuruInfectionChance".Translate(KuruModSettings.baseKuruInfectionChance.ToStringPercent().Named("CHANCE")),
                KuruModSettings.baseKuruInfectionChance, 0f, 1f, 0.5f,
                "KuruOptions_baseKuruInfectionChance_tooltip".Translate());

            if (listingStandard.ButtonTextLabeledPct("KuruOptions_progressionSpeed".Translate(),
                    KuruModSettings.progressionSpeed.ToStringHuman(), 0.6f, TextAnchor.MiddleLeft))
            {
                var options = new List<FloatMenuOption>();
                foreach (ProgressionSpeed progressionSpeed in Enum.GetValues(typeof(ProgressionSpeed)))
                {
                    var localProgressionSpeed = progressionSpeed;
                    options.Add(new FloatMenuOption(localProgressionSpeed.ToStringHuman(),
                        () => KuruModSettings.progressionSpeed = localProgressionSpeed));
                }

                Find.WindowStack.Add(new FloatMenu(options));
            }
            
            listingStandard.CheckboxLabeled(
                "KuruOptions_butcherSkillMatters".Translate(),
                ref KuruModSettings.butcherSkillMatters,
                "KuruOptions_butcherSkillMatters_tooltip".Translate());

            listingStandard.GapLine();

            listingStandard.Label("KuruOptions_infectionSources_label".Translate());

            listingStandard.Indent(gapWidth);
            listingStandard.ColumnWidth -= gapWidth;

            if (ModsConfig.IdeologyActive)
            {
                listingStandard.CheckboxLabeled(
                    "KuruOptions_infectFromIdeologion".Translate(),
                    ref KuruModSettings.infectFromIdeologion,
                    "KuruOptions_infectFromIdeologion_tooltip".Translate(KuruCause.MeatOfPawnWithCannibalIdeology
                        .GetKuruCarrierChance().ToStringPercent().Named("CHANCE")));
            }

            listingStandard.CheckboxLabeled(
                "KuruOptions_infectFromRecentIngestion".Translate(),
                ref KuruModSettings.infectFromRecentIngestion,
                "KuruOptions_infectFromRecentIngestion_tooltip".Translate(KuruCause.MeatOfPawnWhoIngestedHumanMeatRecently.GetKuruCarrierChance().ToStringPercent().Named("CHANCE")));

            listingStandard.CheckboxLabeled(
                "KuruOptions_infectFromTraits".Translate(),
                ref KuruModSettings.infectFromTraits,
                "KuruOptions_infectFromTraits_tooltip".Translate(KuruCause.MeatOfPawnWithCannibalTrait.GetKuruCarrierChance().ToStringPercent().Named("CHANCE")));
            
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

            if (ModsConfig.BiotechActive)
            {
                listingStandard.CheckboxLabeled(
                    "KuruOptions_naturalCannibalCures".Translate(),
                    ref KuruModSettings.naturalCannibalCures,
                    "KuruOptions_naturalCannibalCures_tooltip".Translate());
            }
            
            listingStandard.GapLine();
            
            listingStandard.Outdent(gapWidth);
            listingStandard.ColumnWidth += gapWidth;
            
            if (Prefs.DevMode)
            {
                listingStandard.Label("KuruOptions_devtools_section_label".Translate());

                listingStandard.Indent(gapWidth);
                listingStandard.ColumnWidth -= gapWidth;

                if (listingStandard.ButtonText("KuruOptions_devtools_FoodDefCensus_button_label".Translate()))
                {
                    KuruFoodCensusUtil.FoodCensus();
                    Find.WindowStack.Add(
                        new Dialog_MessageBox(
                            "KuruOptions_devtools_FoodDefCensus_TraifDefCensus_CSVCopied".Translate()
                        )
                    );
                }

                listingStandard.Outdent(gapWidth);
                listingStandard.ColumnWidth += gapWidth;
                
                listingStandard.GapLine();
                
            }
            

            listingStandard.End();
            
            base.DoSettingsWindowContents(inRect);
        }

        public override string SettingsCategory()
        {
            return "KuruModName".Translate();
        }
    }

}