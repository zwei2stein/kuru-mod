using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Verse;

namespace Kuru
{
    public class KuruFoodCensusUtil
    {
        public static void FoodCensus()
        {
            var csv = new StringBuilder();

            csv.Append("def_name;def_label;mod_package_id;mod_name;kuru_default_cause\n");
            
            var carrierDefs = new List<ThingDef>();
            foreach (var thingDef in DefDatabase<ThingDef>.AllDefsListForReading)
            {
                if (thingDef.GetCompProperties<CompFoodKuruCarryingProperties>() != null)
                    carrierDefs.Add(thingDef);
            }

            foreach (var recipeDef in DefDatabase<RecipeDef>.AllDefsListForReading)
            {
                if (recipeDef.products.NullOrEmpty()) continue;
                if (!RecipeAllowsAnyCarrier(recipeDef, carrierDefs)) continue;

                foreach (var product in recipeDef.products)
                {
                    var thingDef = product.thingDef;
                    if (thingDef == null) continue;

                    csv.Append(thingDef.defName).Append(";");
                    csv.Append(thingDef.label?.CapitalizeFirst()).Append(";");
                    csv.Append(thingDef.modContentPack?.PackageId).Append(";");
                    csv.Append(thingDef.modContentPack?.Name).Append(";");

                    var compProps = thingDef.GetCompProperties<CompFoodKuruCarryingProperties>();
                    csv.Append(compProps != null ? compProps.defaultCause.ToString() : "NOT_PATCHED");

                    csv.Append("\n");
                }
            }

            GUIUtility.systemCopyBuffer = csv.ToString();
        }

        private static bool RecipeAllowsAnyCarrier(RecipeDef recipeDef, List<ThingDef> carrierDefs)
        {
            foreach (var carrierDef in carrierDefs)
            {
                if (recipeDef.fixedIngredientFilter != null && recipeDef.fixedIngredientFilter.Allows(carrierDef))
                    return true;

                if (recipeDef.ingredients != null)
                {
                    foreach (var ingredient in recipeDef.ingredients)
                    {
                        if (ingredient.filter != null && ingredient.filter.Allows(carrierDef))
                            return true;
                    }
                }
            }

            return false;
        }
    }
}