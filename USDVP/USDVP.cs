using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using Harmony;
using Microsoft.Xna.Framework;
using StardewValley;

namespace USDVP
{
    public class USDVP : Mod
    {
        internal static IMonitor Logger;

        internal static Type GetSDVType(string type)
        {
            const string prefix = "StardewValley.";
            return Type.GetType(prefix + type + ", Stardew Valley") ?? Type.GetType(prefix + type + ", StardewValley");
        }

        public override void Entry(IModHelper helper)
        {
            Logger = this.Monitor;

            //load up the harmony patches
            var harmony = HarmonyInstance.Create("koihimenakamura.unofficialpatch");
            
            //resolve issue #7
            Monitor.Log("Patching Farmer::updateFriendshipGifts", LogLevel.Trace);
            var updateFriendshpGifts = AccessTools.Method(GetSDVType("Farmer"), "updateFriendshipGifts");
            var prefixGifts = helper.Reflection.GetMethod(typeof(Patches.UpdateFriendshipGifts), "Prefix").MethodInfo;
            harmony.Patch(updateFriendshpGifts, new HarmonyMethod(prefixGifts), null);

            SaveEvents.BeforeSave += HandleEndOfDay;
            SaveEvents.AfterLoad += SaveEvents_AfterLoad;

            helper.ConsoleCommands.Add("usdvp_giftreset", "This command force resets the gifts to be 0.", GiftReset);
        }

        private void GiftReset(string arg1, string[] arg2)
        {
            //manual command
            foreach (var f in Game1.player.friendshipData.Pairs)
            {
                f.Value.GiftsThisWeek = 0;
            }
        }

        private void SaveEvents_AfterLoad(object sender, EventArgs e)
        {
            //retro fix for 1.2 saves
            WorldDate cDate = new WorldDate(Game1.year, Game1.currentSeason, Game1.dayOfMonth -1);
            WorldDate today = new WorldDate(Game1.year, Game1.currentSeason, Game1.dayOfMonth);
            bool checkForManualReset = false;

            foreach (var f in Game1.player.friendshipData.Pairs){
                if (f.Value.GiftsThisWeek == 2 && f.Value.LastGiftDate == null)
                {
                    f.Value.LastGiftDate = cDate;
                    checkForManualReset = true;
                }

                f.Value.GiftsToday = 0;
            }

            if (today.DayOfWeek == DayOfWeek.Monday && checkForManualReset)
            {
                Game1.player.updateFriendshipGifts(today);
            }


        }

        private void HandleEndOfDay(object sender, EventArgs e)
        {
            Monitor.Log("Running evening fixes", LogLevel.Trace);
            //resolve issue #10.
            foreach (var v in Game1.player.friendshipData.Pairs)
            {
                if (v.Value.LastGiftDate?.Equals(new WorldDate(1, "spring", 0)) == true)
                {
                    v.Value.LastGiftDate = new WorldDate(Game1.year, Game1.currentSeason, (Game1.dayOfMonth -1));
                }
            }
        }
    }
}
