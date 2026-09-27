using LudeonTK;
using RimWorld;
using Verse;

namespace Kuru
{
    public static class KuruDebugActions
    {
        [DebugAction("Kuru", "Inspect Kuru cause (click a thing)", actionType = DebugActionType.ToolMap,
            allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static void InspectKuruCause()
        {
            var cell = UI.MouseCell();
            var things = cell.GetThingList(Find.CurrentMap);

            if (things.Count == 0)
            {
                Log.Message("[KuruMod] No things at " + cell);
                return;
            }

            foreach (var thing in things)
            {
                var comp = thing.TryGetComp<CompKuruCarrying>();
                if (comp == null)
                {
                    var message = "[KuruMod] " + thing.LabelCap + " (" + thing.def.defName + ") - no CompKuruCarrying";
                    Log.Message(message);
                    Messages.Message(message, MessageTypeDefOf.NegativeEvent);
                }
                else
                {
                    var message = "[KuruMod] " + thing.LabelCap + " (" + thing.def.defName + ") - cause: " +
                                  comp.cause + ", carrier chance: " +
                                  comp.cause.GetKuruCarrierChance().ToStringPercent();
                    Log.Message(message);
                    Messages.Message(message, MessageTypeDefOf.NegativeEvent);
                }
            }
        }
    }
}