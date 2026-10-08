using HarmonyLib;
using Verse;

namespace NameColonyAtStart
{
    [StaticConstructorOnStartup]
    public static class NameColonyAtStartMod
    {
        static NameColonyAtStartMod()
        {
            new Harmony("DanZinagri.NameColonyAtStart").PatchAll();
        }
    }
}
