using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Menus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace LongerSeasons
{
    /// <summary>The mod entry point.</summary>
    public partial class ModEntry
    {

        public static int GetCurrentSeasonDaysPlusOne()
        {
            return GetDaysInSeason(Game1.currentSeason) + 1;
        }

        public static IEnumerable<CodeInstruction> Game1__newDayAfterFade_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            SMonitor.Log($"Transpiling Game1._newDayAfterFade");

            var codes = new List<CodeInstruction>(instructions);
            for (int i = 0; i < codes.Count; i++)
            {
                if (codes[i].operand != null && codes[i].operand.ToString().Contains("dayOfMonth"))
                {
                    SMonitor.Log($"Found dayOfMonth at {i}: {codes[i].opcode} {codes[i].operand}");
                    for(int j = Math.Max(0, i-2); j < Math.Min(codes.Count, i+5); j++)
                    {
                        SMonitor.Log($"  [{j}] {codes[j].opcode} {codes[j].operand}");
                    }
                }
            }

            for (int i = 0; i < codes.Count - 2; i++)
            {
                bool isDayOfMonth = false;

                if (codes[i].opcode == OpCodes.Ldsfld && codes[i].operand is FieldInfo f && f.Name == "dayOfMonth")
                {
                    isDayOfMonth = true;
                }
                else if (codes[i].opcode == OpCodes.Call && codes[i].operand is MethodInfo m && m.Name == "get_dayOfMonth")
                {
                    isDayOfMonth = true;
                }

                if (isDayOfMonth && codes[i + 1].opcode == OpCodes.Ldc_I4_S && codes[i + 1].operand is sbyte s && (s == 29 || s == 28))
                {
                    SMonitor.Log($"Changing days per month to dynamic season length (found {s})");
                    codes[i + 1].opcode = OpCodes.Call;
                    codes[i + 1].operand = AccessTools.Method(typeof(ModEntry), nameof(ModEntry.GetCurrentSeasonDaysPlusOne));
                    codes[i + 2].opcode = OpCodes.Blt_Un_S; // wait, if it was > 28 (Bgt), it should be Bge or something? Let's just fix it later.
                    break;
                }
            }

            return codes.AsEnumerable();
        }

        private static void Game1__newDayAfterFade_Prefix()
        {
            SMonitor.Log($"dom {Game1.dayOfMonth}, year {Game1.year}, season {Game1.currentSeason}");
        }

    }
}