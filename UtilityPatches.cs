using HarmonyLib;
using StardewModdingAPI;
using StardewValley;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;

namespace LongerSeasons
{
    /// <summary>The mod entry point.</summary>
    public partial class ModEntry
    {
        public static void Utility_getSeasonNameFromNumber_Postfix(int number, ref string __result)
        {
            if (Context.IsWorldReady)
            {
                GetMonthAndDay(number, Game1.dayOfMonth, Game1.year, out int m, out int d);
                __result = $"Tháng {m} ({__result})";
            }
        }

        private static bool Utility_getDateStringFor_Prefix(int day, int season, int year, ref string __result)
        {
            GetMonthAndDay(season, day, year, out int m, out int d);
            __result = $"Ngày {d} Tháng {m} Năm {year}";
            return false;
        }
    }
}