using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StardewValley;

namespace USDVP.Patches
{
    class UpdateFriendshipGifts
    {
        [SuppressMessage("ReSharper", "InconsistentNaming")]
        public static bool Prefix(Farmer __instance)
        {
            WorldDate currDate = new WorldDate(Game1.year, Game1.currentSeason, Game1.dayOfMonth); //prop the date

            foreach (var key in __instance.friendshipData.Keys)
            {
                if (__instance.friendshipData[key].LastGiftDate != null && currDate.TotalDays != __instance.friendshipData[key].LastGiftDate.TotalDays)
                    __instance.friendshipData[key].GiftsToday = 0;
                
                // ReSharper disable once InvertIf
                if (__instance.friendshipData[key].LastGiftDate != null && currDate.TotalWeeks != __instance.friendshipData[key].LastGiftDate.TotalWeeks)
                {
                    if (__instance.friendshipData[key].GiftsThisWeek == 2)
                        __instance.friendshipData[key].Points = Math.Min(__instance.friendshipData[key].Points + 10, 2749);
                    __instance.friendshipData[key].GiftsThisWeek = 0;
                }
            }

            return false;
        }
    }
}
