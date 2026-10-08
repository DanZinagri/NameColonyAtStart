using RimWorld;
using UnityEngine;
using Verse;

namespace NameColonyAtStart
{
    // Two-field naming window for the setup screens.
    //
    // Vanilla's Dialog_GiveName can't be reused here: its constructor picks a map colonist
    // as the "suggesting pawn" and its labels dereference it, and there are no map pawns
    // before the game starts. Validation reuses vanilla's own rules, so anything accepted
    // here is something the in-game prompt would have accepted too.
    public class Dialog_NameColonyAtStart : Window
    {
        private const float RowHeight = 35f;
        private const float RandomizeWidth = 150f;
        private const int CharLimit = 64;

        private string factionName;
        private string settlementName;

        public override Vector2 InitialSize => new Vector2(560f, 300f);

        public Dialog_NameColonyAtStart()
        {
            doCloseX = true;
            forcePause = true;
            absorbInputAroundWindow = true;
            closeOnClickedOutside = false;
            closeOnAccept = false;

            PendingColonyNames pending = PendingColonyNames.Peek();
            factionName = pending?.factionName ?? GenerateFactionName();
            settlementName = pending?.settlementName ?? GenerateSettlementName();
        }

        public override void DoWindowContents(Rect inRect)
        {
            bool enterPressed = false;
            if (Event.current.type == EventType.KeyDown
                && (Event.current.keyCode == KeyCode.Return || Event.current.keyCode == KeyCode.KeypadEnter))
            {
                enterPressed = true;
                Event.current.Use();
            }

            float y = 0f;
            using (new TextBlock(GameFont.Medium))
            {
                Widgets.Label(new Rect(0f, y, inRect.width, 35f), "NCAS_DialogTitle".Translate());
            }
            y += 45f;

            factionName = DrawNameRow(inRect, ref y, "NCAS_FactionLabel".Translate(), factionName, GenerateFactionName);
            settlementName = DrawNameRow(inRect, ref y, "NCAS_SettlementLabel".Translate(), settlementName, GenerateSettlementName);

            using (new TextBlock(GameFont.Tiny, ColoredText.SubtleGrayColor))
            {
                Widgets.Label(new Rect(0f, y, inRect.width, 22f), "NCAS_BlankHint".Translate());
            }

            float buttonWidth = 150f;
            Rect okRect = new Rect(inRect.width - buttonWidth, inRect.height - RowHeight, buttonWidth, RowHeight);
            if (Widgets.ButtonText(okRect, "OK".Translate()) || enterPressed)
            {
                TryAccept();
            }
        }

        private static string DrawNameRow(Rect inRect, ref float y, string label, string value, System.Func<string> generator)
        {
            Widgets.Label(new Rect(0f, y, inRect.width, 24f), label);
            y += 24f;
            float fieldWidth = inRect.width - RandomizeWidth - 10f;
            value = Widgets.TextField(new Rect(0f, y, fieldWidth, RowHeight), value, CharLimit);
            if (Widgets.ButtonText(new Rect(fieldWidth + 10f, y, RandomizeWidth, RowHeight), "Randomize".Translate()))
            {
                value = generator();
            }
            y += RowHeight + 12f;
            return value;
        }

        private void TryAccept()
        {
            string faction = factionName?.Trim();
            string settlement = settlementName?.Trim();

            // Blank is allowed: that name is left to the vanilla in-game prompt.
            if (!faction.NullOrEmpty() && !NamePlayerFactionDialogUtility.IsValidName(faction))
            {
                Messages.Message("PlayerFactionNameIsInvalid".Translate(), MessageTypeDefOf.RejectInput, historical: false);
                return;
            }
            if (!settlement.NullOrEmpty() && !NamePlayerSettlementDialogUtility.IsValidName(settlement))
            {
                Messages.Message("PlayerFactionBaseNameIsInvalid".Translate(), MessageTypeDefOf.RejectInput, historical: false);
                return;
            }

            PendingColonyNames pending = PendingColonyNames.Current;
            if (pending != null)
            {
                pending.factionName = faction.NullOrEmpty() ? null : faction;
                pending.settlementName = settlement.NullOrEmpty() ? null : settlement;
            }
            Close();
        }

        private static string GenerateFactionName()
        {
            RulePackDef maker = PlayerFactionDef?.factionNameMaker;
            return maker == null ? "" : NameGenerator.GenerateName(maker, NamePlayerFactionDialogUtility.IsValidName);
        }

        private static string GenerateSettlementName()
        {
            RulePackDef maker = PlayerFactionDef?.settlementNameMaker;
            return maker == null ? "" : NameGenerator.GenerateName(maker, NamePlayerSettlementDialogUtility.IsValidName);
        }

        // The world (and so the player faction) exists by the time the starting-pawn page
        // is open, but don't assume it.
        private static FactionDef PlayerFactionDef => Find.World?.factionManager?.OfPlayer?.def;
    }
}
