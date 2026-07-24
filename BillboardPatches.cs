using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Objects;
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

        private static void Billboard_Postfix(Billboard __instance, bool dailyQuest)
        {
            if (dailyQuest)
                return;
            
            ModEntry.GetMonthAndDay(Game1.seasonIndex, Game1.dayOfMonth, Game1.year, out int month, out int monthDay);
            
            int pageStartDay = ((Game1.dayOfMonth - 1) / 28) * 28 + 1;
            int startDate = pageStartDay;

            __instance.calendarDays = new List<ClickableTextureComponent>();
            Dictionary<int, List<NPC>> birthdays = __instance.GetBirthdays();

            for (int day = startDate; day <= startDate + 27; day++)
            {
                List<Billboard.BillboardEvent> curEvents = __instance.GetEventsForDay(day, birthdays);
                if (curEvents.Count > 0)
                {
                    __instance.calendarDayData[day] = new Billboard.BillboardDay(curEvents.ToArray());
                }
                int index = (day - 1) % 28;
                ModEntry.GetMonthAndDay(Game1.seasonIndex, day, Game1.year, out int tempM, out int tempD);
                string dayStr = tempD.ToString();
                __instance.calendarDays.Add(new ClickableTextureComponent(dayStr, new Rectangle(__instance.xPositionOnScreen + 152 + index % 7 * 32 * 4, __instance.yPositionOnScreen + 200 + index / 7 * 32 * 4, 124, 124), string.Empty, string.Empty, null, Rectangle.Empty, 1f, false)
                {
                    myID = day,
                    rightNeighborID = ((index % 7 != 6) ? (day + 1) : (-1)),
                    leftNeighborID = ((index % 7 != 0) ? (day - 1) : (-1)),
                    downNeighborID = day + 7,
                    upNeighborID = ((index >= 7) ? (day - 7) : (-1))
                });
            }
        }

        private static void Billboard_draw_Postfix(Billboard __instance, Texture2D ___billboardTexture, bool ___dailyQuestBoard, SpriteBatch b)
        {
            if (___dailyQuestBoard)
                return;

            ModEntry.GetMonthAndDay(Game1.seasonIndex, Game1.dayOfMonth, Game1.year, out int month, out int monthDay);
            int pageStartDay = ((Game1.dayOfMonth - 1) / 28) * 28 + 1;
            int add = pageStartDay - 1;

            for (int i = 0; i < __instance.calendarDays.Count; i++)
            {
                int currentBoxDay = add + i + 1;
                if (Game1.dayOfMonth > currentBoxDay)
                {
                    b.Draw(Game1.staminaRect, __instance.calendarDays[i].bounds, Color.Gray * 0.25f);
                }
                else if (Game1.dayOfMonth == currentBoxDay)
                {
                    int offset = (int)(4f * Game1.dialogueButtonScale / 8f);
                    IClickableMenu.drawTextureBox(b, Game1.mouseCursors, new Rectangle(379, 357, 3, 3), __instance.calendarDays[i].bounds.X - offset, __instance.calendarDays[i].bounds.Y - offset, __instance.calendarDays[i].bounds.Width + offset * 2, __instance.calendarDays[i].bounds.Height + offset * 2, Color.Blue, 4f, false, -1f);
                }
            }
        }


    }
}