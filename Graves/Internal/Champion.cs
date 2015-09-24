﻿using System.Drawing;
using EloBuddy;

namespace VnHarry_AIO.Internal
{
    internal class Champion : PluginBase
    {
        public Champion()
        {
            _SetupMenu();

            Chat.Print("VnHarry AIO - <font color=\"#FFFFFF\">{0} is not supported</font>",
                Color.FromArgb(255, 210, 68, 74), ObjectManager.Player.ChampionName);
        }

        public override sealed void _SetupMenu()
        {
            Variables.Config.AddGroupLabel("Champion not Supported");
            Variables.Config.AddLabel("The champion is probably not a Marksman or not yet implemented.");
        }

    }
}