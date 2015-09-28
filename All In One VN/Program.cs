using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Events;
using EloBuddy.SDK.Menu;
using EloBuddy.SDK.Menu.Values;
using System;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Reflection;
using VnHarry_AIO.Internal;
using VnHarry_AIO.Marksman;
using VnHarry_AIO.Utilities;
using Champion = VnHarry_AIO.Internal.Champion;
using Core = VnHarry_AIO.Activator.Core;

namespace VnHarry_AIO
{
    internal class Program
    {
        public static PluginBase ChampionPlugin;
        private static Core _instance;

        public static AIHeroClient _Player
        {
            get { return ObjectManager.Player; }
        }

        private static void Main(string[] args)
        {
            try
            {
                Loading.OnLoadingComplete += Loading_OnLoadingComplete;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
      
        private static void Loading_OnLoadingComplete(EventArgs args)
        {
            TargetSelector2.init();
            Bootstrap.Init(null);
            VnHarryFarmLogic.init();
            VnHarryDrawLogic.init();

            Variables.InfoMenu = MainMenu.AddMenu("VnHarry AIO", "MarksmanBuddy");
            Variables.InfoMenu.AddGroupLabel("VnHarry Tất cả trong một");
            Variables.InfoMenu.AddLabel("Version: " + "1.0.0.3");
            Variables.InfoMenu.AddSeparator();
            Variables.InfoMenu.AddLabel("Danh sách tướng hỗ trợ: ");
            Variables.InfoMenu.AddLabel("Corki ");
            Variables.InfoMenu.AddLabel("Draven ");
            Variables.InfoMenu.AddLabel("Graves ");
            Variables.InfoMenu.AddLabel("Sivir ");
            Variables.InfoMenu.AddLabel("Varus in develop");
            Variables.InfoMenu.AddLabel("Vayne ");
            Variables.InfoMenu.AddSeparator();
            Variables.InfoMenu.AddLabel("Có lỗi hay muốn phát triển liên hệ trực tiếp trên diễn đàn Harry!");
            Variables.InfoMenu.AddLabel("Trân trọng");
            Variables.InfoMenu.AddLabel("Được tạo bởi: " + "VnHarry");

            Variables.Activator = Variables.InfoMenu.AddSubMenu("MB Activator", "MBActivator");
            Variables.Activator.AddGroupLabel("Phép bổ trợ");
            Variables.Activator.Add("Activator.UseHeal", new CheckBox("Sử dụng bom máu (Heal)"));
            Variables.Activator.Add("Activator.UseHealPercent", new Slider("Phần trăm máu sử dụng", 35));
            Variables.Activator.AddGroupLabel("Máu và Mana");
            Variables.Activator.Add("Activator.UseHPPot", new CheckBox("Sử dụng bình Máu"));
            Variables.Activator.Add("Activator.UseHPPotPercent",
                new Slider("Sủ dung bình Máu khi máu còn %", 60));
            Variables.Activator.Add("Activator.UseMPPot", new CheckBox("Sử dụng bình Năng Lượng"));
            Variables.Activator.Add("Activator.UseMPPotPercent",
                new Slider("Sủ dung bình Năng Lượng khi máu còn %", 40));
            Variables.Activator.AddGroupLabel("Trang bị");
            Variables.Activator.Add("Activator.UseCutlass", new CheckBox("Sử dụng kiếm hải tặc trong Combo"));
            Variables.Activator.Add("Activator.UseYoumuus", new CheckBox("Sử dụng kiếm ma Youmuu trong Combo"));
            Variables.Activator.Add("Activator.UseBotrK", new CheckBox("Sử dụng gươm vô danh trong Combo"));

            var _Activator = new Core();
            Chat.Print("VnHarry AIO - <font color=\"#FFFFFF\">Loaded</font>", Color.FromArgb(255, 210, 68, 74));
            var championName = ObjectManager.Player.ChampionName.ToLower(CultureInfo.InvariantCulture);
            Variables.Config = Variables.InfoMenu.AddSubMenu(Player.Instance.ChampionName, Player.Instance.ChampionName);
            Chat.Print("VnHarry AIO - <font color=\"#FFFFFF\">{0} Loaded</font>",Color.FromArgb(255, 210, 68, 74), ObjectManager.Player.ChampionName);
            switch (championName)
            {
                case "ashe":
                    ChampionPlugin = new Champion();
                    break;

                case "caitlyn":
                    ChampionPlugin = new Champion();
                    break;

                case "corki":
                    ChampionPlugin = new Corki();
                    break;

                case "draven":
                    ChampionPlugin = new Draven();
                    break;

                case "ezreal":
                    ChampionPlugin = new Champion();
                    break;

                case "graves":
                    ChampionPlugin = new Graves();
                    break;

                case "gnar":
                    ChampionPlugin = new Champion();
                    break;

                case "jinx":
                    ChampionPlugin = new Champion();
                    break;

                case "kalista":
                    ChampionPlugin = new Champion();
                    break;

                case "kindred":
                    ChampionPlugin = new Champion();
                    break;

                case "kogmaw":
                    ChampionPlugin = new Champion();
                    break;

                case "lucian":
                    ChampionPlugin = new Champion();
                    break;

                case "missfortune":
                    ChampionPlugin = new Champion();
                    break;

                case "quinn":
                    ChampionPlugin = new Champion();
                    break;

                case "sivir":
                    ChampionPlugin = new Sivir();
                    break;

                case "teemo":
                    ChampionPlugin = new Champion();
                    break;

                case "tristana":
                    ChampionPlugin = new Champion();
                    break;

                case "twitch":
                    ChampionPlugin = new Champion();
                    break;

                case "urgot":
                    ChampionPlugin = new Champion();
                    break;

                case "vayne":
                    ChampionPlugin = new Vayne();
                    break;

                case "varus":
                    ChampionPlugin = new Champion();
                    break;
            }

            Game.OnTick += Game_OnTick;
        }

        private static void Game_OnTick(EventArgs args)
        {
            Variables.ComboMode = Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo);
            Variables.HarassMode = Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Harass);
            Variables.LaneClearMode = Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LaneClear);
            Variables.LastHitMode = Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LastHit);
            Variables.NoneMode = Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.None);
        }
    }
}