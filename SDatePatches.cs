using HarmonyLib;
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


        public static IEnumerable<CodeInstruction> SDate_Transpiler(IEnumerable<CodeInstruction> instructions, MethodBase original)
        {
            SMonitor.Log($"Transpiling SDate()");

            bool isSeasonOverload = original.GetParameters().Length >= 2 && original.GetParameters()[1].ParameterType == typeof(Season);

            var codes = new List<CodeInstruction>(instructions);

            for (int i = 0; i < codes.Count; i++)
            {
                if (codes[i].opcode == OpCodes.Ldarg_1 && codes[i+1].opcode == OpCodes.Ldarg_0 && codes[i+2].opcode == OpCodes.Ldfld && ((FieldInfo)codes[i + 2].operand).Name == "DaysInSeason")
                {
                    SMonitor.Log($"Avoiding SMAPI {((FieldInfo)codes[i + 2].operand).Name}");
                    codes[i + 1].opcode = OpCodes.Ldarg_2;
                    codes[i + 1].operand = null;
                    
                    codes[i + 2].opcode = OpCodes.Call;
                    if (isSeasonOverload)
                    {
                        codes[i + 2].operand = AccessTools.Method(typeof(ModEntry), nameof(ModEntry.GetDaysInSeason), new[] { typeof(Season) });
                    }
                    else
                    {
                        codes[i + 2].operand = AccessTools.Method(typeof(ModEntry), nameof(ModEntry.GetDaysInSeason), new[] { typeof(string) });
                    }
                }
            }

            return codes.AsEnumerable();
        }
        private static void SDate_Postfix(SDate __instance)
        {
            typeof(SDate).GetField("DaysInSeason", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(__instance, GetDaysInSeason((int)__instance.Season, __instance.Year));
        }

    }
}