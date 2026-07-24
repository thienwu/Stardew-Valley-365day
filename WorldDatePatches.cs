using HarmonyLib;
using Netcode;
using StardewModdingAPI.Utilities;
using StardewValley;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace LongerSeasons
{
    /// <summary>The mod entry point.</summary>
    public partial class ModEntry
    {
        private static bool WorldDate_TotalDays_Getter_Prefix(WorldDate __instance, ref int __result)
        {
            int days = GetDaysInPastYears(__instance.Year);
            for (int i = 0; i < __instance.SeasonIndex; i++) {
                days += GetDaysInSeason(i, __instance.Year);
            }
            days += (__instance.DayOfMonth - 1);
            __result = days;
            return false;
        }
        private static bool WorldDate_TotalDays_Setter_Prefix(WorldDate __instance, ref int value)
        {
            if (value < 0) value = 0;
            int remainingDays = value;
            int year = 1;
            while (remainingDays >= (IsLeapYear(year) ? 366 : 365)) {
                remainingDays -= (IsLeapYear(year) ? 366 : 365);
                year++;
            }
            __instance.Year = year;
            int season = 0;
            while (season < 4) {
                int daysInThisSeason = GetDaysInSeason(season, year);
                if (remainingDays < daysInThisSeason) {
                    break;
                }
                remainingDays -= daysInThisSeason;
                season++;
            }
            __instance.Season = (StardewValley.Season)season;
            __instance.DayOfMonth = remainingDays + 1;
            return false;
        }
    }
}