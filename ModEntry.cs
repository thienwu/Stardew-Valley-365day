using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.GameData;
using StardewValley.Menus;
using StardewValley.TerrainFeatures;
using System;
using System.Collections.Generic;
using System.Xml.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Linq;

namespace LongerSeasons
{
    /// <summary>The mod entry point.</summary>
    public partial class ModEntry : Mod
    {

        public static IMonitor SMonitor;
        public static IModHelper SHelper;
        public static ModConfig Config;

        public static ModEntry context;

        public static bool IsLeapYear(int year)
        {
            return year % 2 != 0;
        }

        public static int GetDaysInPastYears(int currentYear)
        {
            int days = 0;
            for (int y = 1; y < currentYear; y++) {
                days += IsLeapYear(y) ? 366 : 365;
            }
            return days;
        }

        public static int GetDaysInSeason(int seasonIndex, int year)
        {
            switch (seasonIndex)
            {
                case 0: return IsLeapYear(year) ? 91 : 90; // Spring
                case 1: return 91; // Summer
                case 2: return 92; // Fall
                case 3: return 92; // Winter
                default: return 90;
            }
        }
        public static int GetDaysInSeason(string season)
        {
            switch (season?.ToLower())
            {
                case "spring": return IsLeapYear(Game1.year) ? 91 : 90;
                case "summer": return 91;
                case "fall": return 92;
                case "winter": return 92;
                default: return 90;
            }
        }
        
        public static int GetDaysInSeason(Season season)
        {
            switch (season)
            {
                case Season.Spring: return IsLeapYear(Game1.year) ? 91 : 90;
                case Season.Summer: return 91;
                case Season.Fall: return 92;
                case Season.Winter: return 92;
                default: return 90;
            }
        }

        public static void GetMonthAndDay(int seasonIndex, int dayOfMonth, int year, out int month, out int day)
        {
            int baseMonth = seasonIndex * 3 + 1;
            if (seasonIndex == 0) // Spring: 31, 28/29, 31
            {
                int month2Days = IsLeapYear(year) ? 29 : 28;
                if (dayOfMonth <= 31) { month = baseMonth; day = dayOfMonth; }
                else if (dayOfMonth <= 31 + month2Days) { month = baseMonth + 1; day = Math.Min(month2Days, dayOfMonth - 31); }
                else { month = baseMonth + 2; day = Math.Min(31, dayOfMonth - (31 + month2Days)); }
            }
            else if (seasonIndex == 1) // Summer: 30, 31, 31
            {
                if (dayOfMonth <= 30) { month = baseMonth; day = dayOfMonth; }
                else if (dayOfMonth <= 61) { month = baseMonth + 1; day = Math.Min(31, dayOfMonth - 30); }
                else { month = baseMonth + 2; day = Math.Min(31, dayOfMonth - 61); }
            }
            else if (seasonIndex == 2) // Fall: 31, 30, 31
            {
                if (dayOfMonth <= 31) { month = baseMonth; day = dayOfMonth; }
                else if (dayOfMonth <= 61) { month = baseMonth + 1; day = Math.Min(30, dayOfMonth - 31); }
                else { month = baseMonth + 2; day = Math.Min(31, dayOfMonth - 61); }
            }
            else // Winter: 30, 31, 31
            {
                if (dayOfMonth <= 30) { month = baseMonth; day = dayOfMonth; }
                else if (dayOfMonth <= 61) { month = baseMonth + 1; day = Math.Min(31, dayOfMonth - 30); }
                else { month = baseMonth + 2; day = Math.Min(31, dayOfMonth - 61); }
            }
        }

        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            Config = Helper.ReadConfig<ModConfig>();

            if (!Config.EnableMod)
                return;

            context = this;

            SMonitor = Monitor;
            SHelper = helper;

            Helper.Events.GameLoop.DayStarted += GameLoop_DayStarted;
            Helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
            Helper.Events.GameLoop.SaveLoaded += GameLoop_SaveLoaded;
            Helper.Events.Content.AssetRequested += Content_AssetRequested;

            var harmony = new Harmony(ModManifest.UniqueID);

            // Game1 Patches

            harmony.Patch(
               original: AccessTools.Method(typeof(Game1), "_newDayAfterFade"),
               prefix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.Game1__newDayAfterFade_Prefix))
            );

            foreach(var type in typeof(Game1).Assembly.GetTypes())
            {
                if (type.FullName.StartsWith("StardewValley.Game1+<_newDayAfterFade>"))
                {
                    Monitor.Log($"Found {type}");
                    harmony.Patch(
                       original: AccessTools.Method(type, "MoveNext"),
                       transpiler: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.Game1__newDayAfterFade_Transpiler))
                    );
                    break;
                }
            }
            

            // SDate Patches

            harmony.Patch(
               original: AccessTools.Constructor(typeof(SDate), new Type[] { typeof(int), typeof(string), typeof(int), typeof(bool) }),
               transpiler: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.SDate_Transpiler)),
               postfix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.SDate_Postfix))
            );
            harmony.Patch(
               original: AccessTools.Constructor(typeof(SDate), new Type[] { typeof(int), typeof(string)}),
               postfix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.SDate_Postfix))
            );
            harmony.Patch(
               original: AccessTools.Constructor(typeof(SDate), new Type[] { typeof(int), typeof(string), typeof(int)}),
               postfix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.SDate_Postfix))
            );

            harmony.Patch(
               original: AccessTools.Constructor(typeof(SDate), new Type[] { typeof(int), typeof(Season), typeof(int), typeof(bool) }),
               transpiler: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.SDate_Transpiler)),
               postfix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.SDate_Postfix))
            );
            harmony.Patch(
               original: AccessTools.Constructor(typeof(SDate), new Type[] { typeof(int), typeof(Season)}),
               postfix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.SDate_Postfix))
            );
            harmony.Patch(
               original: AccessTools.Constructor(typeof(SDate), new Type[] { typeof(int), typeof(Season), typeof(int)}),
               postfix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.SDate_Postfix))
            );

            // Utility Patches


            harmony.Patch(
               original: AccessTools.Method(typeof(Utility), nameof(Utility.getDateStringFor)),
               prefix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.Utility_getDateStringFor_Prefix))
            );

            harmony.Patch(
               original: AccessTools.Method(typeof(Utility), nameof(Utility.getSeasonNameFromNumber)),
               postfix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.Utility_getSeasonNameFromNumber_Postfix))
            );

            harmony.Patch(
               original: AccessTools.Method(typeof(Utility), "getDaysOfBooksellerThisSeason"),
               postfix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.Utility_getDaysOfBooksellerThisSeason_Postfix))
            );

            // Billboard Patches

            harmony.Patch(
               original: AccessTools.Constructor(typeof(Billboard), new Type[]{ typeof(bool) }),
               postfix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.Billboard_Postfix))
            );
            harmony.Patch(
               original: AccessTools.Method(typeof(Billboard), nameof(Billboard.draw), new Type[] { typeof(SpriteBatch) }),
               transpiler: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.Billboard_draw_Transpiler)),
               postfix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.Billboard_draw_Postfix))
            );

            harmony.Patch(
               original: AccessTools.Method(typeof(DayTimeMoneyBox), nameof(DayTimeMoneyBox.draw), new Type[] { typeof(SpriteBatch) }),
               transpiler: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.DayTimeMoneyBox_draw_Transpiler))
            );

            // WorldDate Patches
            harmony.Patch(
               original: AccessTools.PropertyGetter(typeof(WorldDate), nameof(WorldDate.TotalDays)),
               prefix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.WorldDate_TotalDays_Getter_Prefix))
            );
            harmony.Patch(
               original: AccessTools.PropertySetter(typeof(WorldDate), nameof(WorldDate.TotalDays)),
               prefix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.WorldDate_TotalDays_Setter_Prefix))
            );

            // Bush Patches
            harmony.Patch(
               original: AccessTools.Method(typeof(Bush), nameof(Bush.inBloom)),
               prefix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.Bush_inBloom_Prefix))
            );
        }

        public static int GetMonthDayForHUD()
        {
            GetMonthAndDay(Game1.seasonIndex, Game1.dayOfMonth, Game1.year, out int m, out int d);
            return d;
        }

        public static string GetMonthDayForHUDString()
        {
            return GetMonthDayForHUD().ToString();
        }

        public static int GetMinusOne() => -1;

        public static IEnumerable<CodeInstruction> Billboard_draw_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            SMonitor.Log($"Transpiling Billboard.draw");
            var codes = new List<CodeInstruction>(instructions);
            for (int i = 0; i < codes.Count; i++)
            {
                if (codes[i].opcode == OpCodes.Ldsfld && codes[i].operand is FieldInfo f && f.Name == "dayOfMonth")
                {
                    codes[i].opcode = OpCodes.Call;
                    codes[i].operand = AccessTools.Method(typeof(ModEntry), nameof(GetMinusOne));
                }
            }
            return codes.AsEnumerable();
        }

        public static IEnumerable<CodeInstruction> DayTimeMoneyBox_draw_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            SMonitor.Log($"Transpiling DayTimeMoneyBox.draw");
            var codes = new List<CodeInstruction>(instructions);
            int dayOfMonthCount = 0;
            for (int i = 0; i < codes.Count; i++)
            {
                bool isDayOfMonth = false;
                if (codes[i].opcode == OpCodes.Ldsfld && codes[i].operand is FieldInfo f && f.Name == "dayOfMonth")
                    isDayOfMonth = true;
                else if (codes[i].opcode == OpCodes.Call && codes[i].operand is MethodInfo m && m.Name == "get_dayOfMonth")
                    isDayOfMonth = true;

                if (isDayOfMonth)
                {
                    dayOfMonthCount++;
                    if (dayOfMonthCount > 2)
                    {
                        codes[i].opcode = OpCodes.Call;
                        codes[i].operand = AccessTools.Method(typeof(ModEntry), nameof(GetMonthDayForHUD));
                    }
                }
                else if (codes[i].opcode == OpCodes.Ldsflda && codes[i].operand is FieldInfo f2 && f2.Name == "dayOfMonth")
                {
                    // For Vietnamese / custom languages that use ToString()
                    codes[i].opcode = OpCodes.Call;
                    codes[i].operand = AccessTools.Method(typeof(ModEntry), nameof(GetMonthDayForHUDString));
                    
                    if (i + 1 < codes.Count && codes[i + 1].opcode == OpCodes.Call && codes[i + 1].operand?.ToString().Contains("ToString") == true)
                    {
                        codes[i + 1].opcode = OpCodes.Nop;
                        codes[i + 1].operand = null;
                    }
                }
            }
            return codes.AsEnumerable();
        }

        public static void Utility_getDaysOfBooksellerThisSeason_Postfix(ref List<int> __result)
        {
            if (!Config.EnableMod) return;

            int springMonth3Bookseller = IsLeapYear(Game1.year) ? 76 : 75;

            var secondaryDays = new Dictionary<int, int[]> {
                { 0, new int[] { 47, springMonth3Bookseller } },
                { 1, new int[] { 45, 76 } },
                { 2, new int[] { 42, 69 } },
                { 3, new int[] { 42, 70 } }
            };

            if (secondaryDays.TryGetValue(Game1.seasonIndex, out int[] extraDays))
            {
                foreach (int day in extraDays)
                {
                    if (!__result.Contains(day))
                        __result.Add(day);
                }
            }
        }

        private void Content_AssetRequested(object sender, StardewModdingAPI.Events.AssetRequestedEventArgs e)
        {
            if (!Config.EnableMod)
                return;
            if (Config.SpreadFestivals && e.NameWithoutLocale.IsEquivalentTo("Data/PassiveFestivals"))
            {
                e.Edit(delegate (IAssetData data)
                {
                    var editor = data.AsDictionary<string, PassiveFestivalData>();
                    var newFestivals = new Dictionary<string, PassiveFestivalData>();
                    foreach(var k in editor.Data.Keys)
                    {
                        var ed = editor.Data[k];
                        if (ed.Season == Season.Spring) ed.StartDay += 0;
                        else if (ed.Season == Season.Summer) ed.StartDay += 0;
                        else if (ed.Season == Season.Fall) ed.StartDay += 0;
                        else if (ed.Season == Season.Winter) ed.StartDay += 0;
                    }
                });

            }
            else if (e.NameWithoutLocale.IsEquivalentTo("LooseSprites/Billboard"))
            {
                if (Game1.dayOfMonth > 28)
                {
                    e.Edit(delegate (IAssetData data)
                    {
                        var editor = data.AsImage();

                        Texture2D sourceImage = Helper.ModContent.Load<Texture2D>("assets/numbers.png");

                        int pageStartDay = ((Game1.dayOfMonth - 1) / 28) * 28 + 1;

                        int endDayToDraw = pageStartDay + 27;

                        for (int i = pageStartDay; i <= endDayToDraw; i++)
                        {
                            GetMonthAndDay(Game1.seasonIndex, i, Game1.year, out int tempM, out int tempD);
                            int displayNum = tempD; // 29, 30, 31, 1, 2, 3...
                            
                            // Erase old number by copying a blank wooden patch from the gap between columns
                            Rectangle blankWood = new Rectangle(54 + (i - pageStartDay) % 7 * 32, 248 + (i - pageStartDay) / 7 * 32, 14, 11);
                            editor.PatchImage(editor.Data, blankWood, new Rectangle(39 + (i - pageStartDay) % 7 * 32, 248 + (i - pageStartDay) / 7 * 32, 14, 11), PatchMode.Replace);
                            int cents = displayNum / 100;
                            int tens = (displayNum - cents * 100) / 10;
                            int ones = displayNum - cents * 100 - tens * 10;
                            int xOff = 7;
                            if (cents > 0)
                            {
                                xOff = 14;
                                editor.PatchImage(sourceImage, new Rectangle(6 * cents, 0, 7, 11), new Rectangle(39 + (i - pageStartDay) % 7 * 32, 248 + (i - pageStartDay) / 7 * 32, 7, 11), PatchMode.Overlay);
                            }
                            if (tens > 0 || cents > 0)
                            {
                                editor.PatchImage(sourceImage, new Rectangle(6 * tens, 0, 7, 11), new Rectangle(32 + xOff + (i - pageStartDay) % 7 * 32, 248 + (i - pageStartDay) / 7 * 32, 7, 11), PatchMode.Overlay);
                            }
                            editor.PatchImage(sourceImage, new Rectangle(6 * ones, 0, 7, 11), new Rectangle(39 + xOff + (i - pageStartDay) % 7 * 32, 248 + (i - pageStartDay) / 7 * 32, 7, 11), PatchMode.Overlay);
                        }
                    });
                }
            }
        }

        private void GameLoop_GameLaunched(object sender, StardewModdingAPI.Events.GameLaunchedEventArgs e)
        {
            // get Generic Mod Config Menu's API (if it's installed)
            var configMenu = Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
            if (configMenu is null)
                return;

            // register mod
            configMenu.Register(
                mod: ModManifest,
                reset: () => Config = new ModConfig(),
                save: () => Helper.WriteConfig(Config)
            );

            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Enable Mod",
                getValue: () => Config.EnableMod,
                setValue: value => Config.EnableMod = value
            );
            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Extend Berry Seasons",
                getValue: () => Config.ExtendBerry,
                setValue: value => Config.ExtendBerry = value
            );
        }

        private void GameLoop_DayStarted(object sender, StardewModdingAPI.Events.DayStartedEventArgs e)
        {
            Helper.GameContent.InvalidateCache("LooseSprites/Billboard");
        }

        private void GameLoop_SaveLoaded(object sender, StardewModdingAPI.Events.SaveLoadedEventArgs e)
        {
            if (!Config.EnableMod) return;

            // Calculate the correct total days based on the current Year, Season, and DayOfMonth 
            // under the new leap year calendar system.
            uint correctTotalDays = (uint)GetDaysInPastYears(Game1.year);
            for (int i = 0; i < Game1.seasonIndex; i++) {
                correctTotalDays += (uint)GetDaysInSeason(i, Game1.year);
            }
            correctTotalDays += (uint)(Game1.dayOfMonth - 1);

            // If the save file was vanilla, Game1.stats.DaysPlayed will be based on 28-day seasons.
            // We upgrade it to match the new scale, ensuring logic relying on DaysPlayed (like Grandpa's evaluation) works properly.
            if (Game1.stats.DaysPlayed < correctTotalDays)
            {
                Monitor.Log($"[LongerSeasons] Upgrading DaysPlayed from {Game1.stats.DaysPlayed} to {correctTotalDays} for 365-day compatibility.", StardewModdingAPI.LogLevel.Info);
                Game1.stats.DaysPlayed = correctTotalDays;
            }
        }
    }

    public class SeasonMonth
    {
        public int month = 1;
    }
}