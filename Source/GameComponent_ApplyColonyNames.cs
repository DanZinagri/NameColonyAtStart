using RimWorld;
using RimWorld.Planet;
using Verse;

namespace NameColonyAtStart
{
    // Applies the pre-chosen names once, when a new game starts.
    //
    // StartedNewGame runs inside Game.InitNewGame after the starting map exists and before
    // InitData is cleared, so the pending names are still reachable. Vanilla's naming check
    // (Faction.FactionTick) only prompts while the faction has no name or the settlement isn't
    // namedByPlayer, so setting both here is all it takes to stop the prompt.
    //
    // Sets the fields directly rather than calling NamePlayerFactionDialogUtility.Named: in
    // permadeath that renames the save by autosaving and deleting the old file, which is
    // wrong before the first save exists. The permadeath name is set directly below instead.
    public class GameComponent_ApplyColonyNames : GameComponent
    {
        public GameComponent_ApplyColonyNames(Game game)
        {
        }

        public override void StartedNewGame()
        {
            PendingColonyNames pending = PendingColonyNames.Peek();
            if (pending == null || !pending.Any)
            {
                return;
            }

            if (!pending.factionName.NullOrEmpty())
            {
                Faction.OfPlayer.Name = pending.factionName;
                GameInfo info = Find.GameInfo;
                if (info.permadeathMode)
                {
                    info.permadeathModeUniqueName = PermadeathModeUtility.GeneratePermadeathSaveNameBasedOnPlayerInput(pending.factionName);
                }
            }

            if (!pending.settlementName.NullOrEmpty())
            {
                Settlement settlement = StartingSettlement();
                if (settlement != null)
                {
                    settlement.Name = pending.settlementName;
                    settlement.namedByPlayer = true;
                }
            }

            pending.factionName = null;
            pending.settlementName = null;
        }

        private static Settlement StartingSettlement()
        {
            if (Find.CurrentMap?.Parent is Settlement current && current.Faction == Faction.OfPlayer)
            {
                return current;
            }
            return Find.WorldObjects.Settlements.Find(s => s.Faction == Faction.OfPlayer);
        }
    }
}
