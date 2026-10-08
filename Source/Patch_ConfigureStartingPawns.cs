using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace NameColonyAtStart
{
    // Draws a rename-style icon button just after the starting-pawn page's title.
    //
    // Hooked on DrawApparelOptions because it's called at the very top of DoWindowContents
    // with the untouched window rect, and runs (as a postfix) even when Ideology is off and
    // the method itself draws nothing. The top-right corner is left alone: vanilla puts the
    // headgear/apparel checkboxes there and pawn-editor mods add their own controls, while
    // the space right of the title is empty. The icon is tinted once names are set.
    [HarmonyPatch(typeof(Page_ConfigureStartingPawns), "DrawApparelOptions")]
    public static class Patch_ConfigureStartingPawns
    {
        private const float IconSize = 30f;
        private const float TitleHeight = 45f;

        private static readonly Color SetColor = new Color(0.55f, 1f, 0.55f);

        public static void Postfix(Page_ConfigureStartingPawns __instance, Rect rect)
        {
            float titleWidth;
            using (new TextBlock(GameFont.Medium))
            {
                titleWidth = Text.CalcSize(__instance.PageTitle ?? "").x;
            }
            // Medium text sits at the top of the 45px title band; centre the icon on its line.
            float y = rect.y + (Text.LineHeightOf(GameFont.Medium) - IconSize) / 2f;
            Rect iconRect = new Rect(rect.x + titleWidth + 12f, Mathf.Max(rect.y, y), IconSize, IconSize);

            PendingColonyNames pending = PendingColonyNames.Peek();
            bool named = pending != null && pending.Any;

            TooltipHandler.TipRegion(iconRect, () => TooltipText(pending), 0x4E43_4153);
            Color baseColor = named ? SetColor : Color.white;
            if (Widgets.ButtonImage(iconRect, TexButton.Rename, baseColor, GenUI.MouseoverColor))
            {
                Find.WindowStack.Add(new Dialog_NameColonyAtStart());
            }
        }

        private static string TooltipText(PendingColonyNames pending)
        {
            string tip = "NCAS_NameColonyTip".Translate();
            if (pending != null && pending.Any)
            {
                tip += "\n";
                if (!pending.factionName.NullOrEmpty())
                {
                    tip += "\n" + "NCAS_FactionLabel".Translate() + " " + pending.factionName;
                }
                if (!pending.settlementName.NullOrEmpty())
                {
                    tip += "\n" + "NCAS_SettlementLabel".Translate() + " " + pending.settlementName;
                }
            }
            return tip;
        }
    }
}
