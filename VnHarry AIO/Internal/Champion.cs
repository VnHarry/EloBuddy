using EloBuddy;
using System.Drawing;

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
            Variables.Config.AddGroupLabel("Tướng không hỗ trợ");
            Variables.Config.AddLabel("Tướng này chưa được phát triển");
        }
    }
}