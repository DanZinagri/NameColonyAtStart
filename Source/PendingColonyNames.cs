using System.Runtime.CompilerServices;
using Verse;

namespace NameColonyAtStart
{
    // Names chosen on the setup screens, waiting to be applied when the game starts.
    //
    // Keyed weakly to the GameInitData of the game being set up, so names never leak
    // into a different new game (back out to the main menu and start again and the
    // GameInitData is a fresh object). Nothing here is ever saved.
    public class PendingColonyNames
    {
        public string factionName;
        public string settlementName;

        public bool Any => !factionName.NullOrEmpty() || !settlementName.NullOrEmpty();

        private static readonly ConditionalWeakTable<GameInitData, PendingColonyNames> table =
            new ConditionalWeakTable<GameInitData, PendingColonyNames>();

        // Null when no game is being set up.
        public static PendingColonyNames Current
        {
            get
            {
                GameInitData initData = Find.GameInitData;
                return initData == null ? null : table.GetOrCreateValue(initData);
            }
        }

        // Read without creating an entry.
        public static PendingColonyNames Peek()
        {
            GameInitData initData = Find.GameInitData;
            if (initData != null && table.TryGetValue(initData, out PendingColonyNames names))
            {
                return names;
            }
            return null;
        }
    }
}
